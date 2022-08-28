using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using ImageMagick;
using WindowsInput;

namespace SigilSolver
{
    internal class SolutionProducer
    {
        [SupportedOSPlatform("windows")]
        public void Screenshot()
        {
            if (File.Exists("screenshot.png"))
            {
                File.Delete("screenshot.png");
            }

            var sw = Stopwatch.StartNew();
            using var bitmap = new Bitmap(3840, 2160);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0,
                    bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            bitmap.Save("screenshot.bmp", ImageFormat.Bmp);
            Console.WriteLine($"- 截图用时:     {sw.Elapsed.TotalSeconds:F3}s");
        }


        IMouseSimulator mouse = new InputSimulator().Mouse;
        public void Run(MagickImage image)
        {
            var sw = Stopwatch.StartNew();
            var boardInfo = ImageProcessor.GetBoardInfo(image);
            Console.WriteLine($"棋盘大小 {boardInfo.Width}*{boardInfo.Height} 左上角坐标 ({boardInfo.Point.X},{boardInfo.Point.Y})");
            Console.WriteLine($"- 棋盘获取用时: {sw.Elapsed.TotalSeconds:F3}s");
            sw = Stopwatch.StartNew();
            var cut = ImageProcessor.CutPieces(image);
            Console.WriteLine($"- 切分方块用时: {sw.Elapsed.TotalSeconds:F3}s");
            sw = Stopwatch.StartNew();
            var pieceInfo = ImageProcessor.FindBlocks(cut).ToList();
            Console.WriteLine($"- 寻找方块用时: {sw.Elapsed.TotalSeconds:F3}s");
            var boardGridSize = 187;
            // 200*200
            sw = Stopwatch.StartNew();
            var solverCore = new SolverCore(boardInfo.Height, boardInfo.Width, pieceInfo.Select(p => p.Type).ToArray());
            var solveResult = solverCore.Solve().ToArray();
            Console.WriteLine($"- 找可行解用时: {sw.Elapsed.TotalSeconds:F3}s");
            foreach (var (block, point) in solveResult)
            {
                var pieceIndex = pieceInfo.FindIndex(p => p.Type == block.BlockType);
                var piece = pieceInfo[pieceIndex];
                pieceInfo.RemoveAt(pieceIndex);

                var sourceX = (piece.Point.X +  piece.Width / 2);
                var sourceY = (piece.Point.Y +
                               (piece.Type is BlockTypes.J or BlockTypes.L ? piece.Height / 4 : piece.Height / 2));
                Console.WriteLine($"正在放置: {block.BlockType} 变体 {block.Variant} 位置: ({sourceX},{sourceY})");
                mouse.MoveMouseTo(sourceX * (65536 / 3840.0), sourceY * (65536 / 2160.0));
                Thread.Sleep(10);
                mouse.LeftButtonDown();
                Thread.Sleep(10);
                mouse.LeftButtonUp();
                Thread.Sleep(40);

                for (int i = 1; i < block.Variant; i++)
                {
                    mouse.RightButtonDown();
                    Thread.Sleep(20);
                    mouse.RightButtonUp();
                    Thread.Sleep(40);
                }

                if (block.Variant > 1)
                {
                    Thread.Sleep(10 * block.Variant);
                }
                var center = GetCenter(block);

                var targetX = (boardInfo.Point.X + (point.X + center.x) * boardGridSize);
                var targetY = (boardInfo.Point.Y + (point.Y + center.y) * boardGridSize);
                Console.WriteLine($"移动至: ({targetX},{targetY})");
                mouse.MoveMouseTo(
                    targetX * (65536 / 3840.0),
                    targetY * (65536 / 2160.0)
                    );
                
                Thread.Sleep(60);
                mouse.LeftButtonClick();
                Thread.Sleep(10);
            }

            Thread.Sleep(100);
            mouse.MoveMouseTo(3624 * (65536 / 3840.0), 1965 * (65536 / 2160.0));
            Thread.Sleep(50);
            mouse.LeftButtonDown();
            Thread.Sleep(50);
            mouse.LeftButtonUp();
            Console.WriteLine("当前求解完成.");
            Console.WriteLine();
        }



        private (double x, double y) GetCenter(Block block)
        {
            switch (block.BlockType)
            {
                case BlockTypes.I:
                    return block.Variant == 1 ? (2, 0.5) : (0.5, 2);
                   
                case BlockTypes.O:
                    return (1, 1);
                case BlockTypes.T:
                    return block.Variant switch
                    {
                        1 => (0.5, 1),
                        2 => (1, 1.5),
                        3 => (1.5, 1),
                        4 => (0, 1.5),
                    };
                case BlockTypes.L:
                    return block.Variant switch
                    {
                        1 => (1.5, 0.5),
                        2 => (1.5, 1.5),
                        3 => (-0.5, 1.5),
                        4 => (0.5, 1.5)
                    };
                case BlockTypes.J:
                    return block.Variant switch
                    {
                        1 => (1.5, 0.5),
                        2 => (0.5, 1.5),
                        3 => (1.5, 1.5),
                        4 => (0.5, 1.5),
                    };
                case BlockTypes.S:
                    return (block.Variant) == 2 ? (1, 1.5) : (0.5, 1);
                case BlockTypes.Z:
                    return (block.Variant ) == 2 ? (0, 1.5) :(1.5, 1) ;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
