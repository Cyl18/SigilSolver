using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;

namespace SigilSolver
{
    // 400 400
    // 2000 2000

    // 2400 200
    // 3500 2000
    internal class ImageProcessor
    {
        public static BoardInfo GetBoardInfo(MagickImage image)
        {
            using var magickImage = image.Clone();
            magickImage.Crop(new MagickGeometry(276, 120, 2172 - 276, 2000 - 120));
            magickImage.RePage();
            magickImage.Threshold(new Percentage(6.4));
            magickImage.Depth = 1;
            magickImage.Resize(new Percentage(20), new Percentage(20));
            using var pixels = magickImage.GetPixelsUnsafe();

            var minX = magickImage.Width;
            var minY = magickImage.Height;
            var maxX = 0;
            var maxY = 0;
            for (int x = 0; x < magickImage.Width; x++)
            {
                for (int y = 0; y < magickImage.Height; y++)
                {
                    var b = pixels[x, y][0] == 0;
                    if (!b) continue;

                    if (minX > x)
                    {
                        minX = x;
                    }

                    if (minY > y)
                    {
                        minY = y;
                    }

                    if (maxX < x)
                    {
                        maxX = x;
                    }

                    if (maxY < y)
                    {
                        maxY = y;
                    }
                }
            }
            // magickImage.Crop(new MagickGeometry(minX, minY, maxX - minX, maxY - minY));
            // magickImage.RePage();
            // magickImage.Write("ccc.png");
            // 40 by 40?
            return new BoardInfo(new Point(minX*5+276, minY*5+120), (int)Math.Round((maxX - (double)minX)/38), (int)Math.Round((maxY - (double)minY) / 38));
        }

        public static IMagickImage<byte> CutPieces(MagickImage image)
        {
            var magickImage = image.Clone();
            magickImage.Crop(new MagickGeometry(2400, 150, 3500 - 2400, 2100 - 150));
            magickImage.Resize(new Percentage(20), new Percentage(20));

            foreach (var pixel in magickImage.GetPixelsUnsafe())
            {
                var r = pixel.GetChannel(0);
                var g = pixel.GetChannel(1);
                var b = pixel.GetChannel(2);
                
                if (Math.Abs(r - b) < 15 && Math.Abs(r - g) < 15 && Math.Abs(g - b) < 15)
                {
                    pixel[0] = 0;
                }
                else
                {
                    pixel[0] = 255;
                }
            }
            return magickImage;
        }

        public static PieceInfo[] FindBlocks(IMagickImage<byte> image)
        {
            using var pixels = image.GetPixelsUnsafe();

            var blocks = new List<PieceInfo>();
            var i = 0;
            start:
            foreach (var pixel in pixels)
            {
                if (pixel[0] == 255)
                {
                    var startPoint = new Point(pixel.X, pixel.Y);
                    var blockStartFinder = new BlockStartFinder(image, startPoint);
                    blockStartFinder.Search(0, 0);
                    using var simage = blockStartFinder.CropToSizeAndGetResult();
                    if (simage.Width > 15 && simage.Height > 15)
                    {
                        //simage.Write($"{i++}.png");
                        var type = BlockTypeDetector.Get(simage);
                        Console.Write($"{type} ");
                        blocks.Add(new PieceInfo(type, new Point(pixel.X * 5+2400, pixel.Y * 5+150), simage.Width * 5, simage.Height * 5));
                    }
                    goto start;
                }
            }

            Console.WriteLine();

            return blocks.ToArray();
        }
    }

    // row 行
    record BoardInfo(Point Point, int Width, int Height);
    record PieceInfo(BlockTypes Type, Point Point, int Width, int Height);
    class BlockTypeDetector
    {
        public static BlockTypes Get(IMagickImage<byte> image)
        {
            var ratio = image.Width / (double)image.Height;
            if (ratio > 2.5)
            {
                return BlockTypes.I;
            }
            else if (Math.Abs(ratio - 1) < 0.25)
            {
                return BlockTypes.O;
            }
            else
            {
                bool a1, a2, a3;
                bool b1, b2, b3;
                var detSize = image.Width / 3;
                // 25% - 75%
                using var pixels = image.GetPixelsUnsafe();
                var start = detSize / 6;

                a1 = Run(0, 0);
                a2 = Run(detSize, 0);
                a3 = Run(detSize + detSize, 0);
                b1 = Run(0, detSize);
                b2 = Run(detSize, detSize);
                b3 = Run(detSize + detSize, detSize);

                if (a1 && a2 && b2 && b3) return BlockTypes.Z;
                if (a1 && a2 && a3 && b3) return BlockTypes.J;
                if (a1 && a2 && a3 && b1) return BlockTypes.L;
                if (b1 && a2 && a3 && b2) return BlockTypes.S;
                if (a2 && b3 && b2 && b1) return BlockTypes.T;

                bool Run(int startX, int startY)
                {
                    int all = 0, white = 0;
                    for (int x = startX + start*2; x < startX + start * 4; x++)
                    {
                        for (int y = startY + start*2; y < startY + start * 4; y++)
                        {
                            all++;
                            if (pixels[x, y][0] == 255)
                            {
                                white++;
                            }
                        }
                    }

                    return white / (double)all > 0.4;
                }

            }

            throw new InvalidOperationException();
        }
    }

    class BlockStartFinder
    {
        MagickImage buffer;
        IUnsafePixelCollection<byte> sourcePixels;
        IUnsafePixelCollection<byte> resultPixels;
        Point _startPoint;

        public BlockStartFinder(IMagickImage<byte> image, Point startPoint)
        {
            buffer = new MagickImage(MagickColors.Black, 400, 400);
            sourcePixels = image.GetPixelsUnsafe();
            resultPixels = buffer.GetPixelsUnsafe();
            _startPoint = startPoint;

        }

        // Search 0,0
        public void Search(int x, int y)
        {
            SetPixel(x, y);
            if (GetPixel(x + 1, y)) Search(x + 1, y);
            if (GetPixel(x, y + 1)) Search(x, y + 1);
            if (GetPixel(x - 1, y)) Search(x - 1, y);
            if (GetPixel(x, y - 1)) Search(x, y - 1);
        }

        public bool GetPixel(int x, int y)
        {
            return sourcePixels[x + _startPoint.X, y + _startPoint.Y][0] != 0;
        }

        public void SetPixel(int x, int y)
        {
            sourcePixels[x + _startPoint.X, y + _startPoint.Y][0] = 0;
            resultPixels[x + 200, y + 200][0] = 255;
        }

        public MagickImage CropToSizeAndGetResult()
        {
            var minX = 400;
            var minY = 400;
            var maxX = 0;
            var maxY = 0;


            for (int x = 0; x < 400; x++)
            {
                for (int y = 0; y < 400; y++)
                {
                    var b = resultPixels[x, y][0] == 255;
                    if (!b) continue;

                    if (minX > x)
                    {
                        minX = x;
                    }

                    if (minY > y)
                    {
                        minY = y;
                    }

                    if (maxX < x)
                    {
                        maxX = x;
                    }

                    if (maxY < y)
                    {
                        maxY = y;
                    }
                }
            }
            
            buffer.Crop(new MagickGeometry(minX, minY, maxX - minX+1, maxY - minY+1));
            buffer.RePage();
            return buffer;
        }
    }
}
