using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SigilSolver
{
    internal class Grid
    {
        internal BitArray grid;
        public int Height { get; }
        public int Width { get; }

        public Grid(int height, int width)
        {
            this.grid = new BitArray(height * width);
            Height = height;
            Width = width;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int x, int y, bool v)
        {
            grid.Set(y * Width + x, v);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return true;
            
            return grid.Get(y * Width + x);
            //return ((grid[y] >> x) & 1) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point FindFirstZero()
        {
            for (var i = 0; i < grid.Count; i++)
            {
                if (!grid.Get(i))
                {
                    var y = i / Width;
                    var x = i % Width;
                    return new Point(x, y);
                }
            }
            return Throw();
        }

        static Point Throw()
        {
            throw new InvalidOperationException();
        }

        public void Print()
        {
            lock (grid)
            {
                Console.Clear();
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (Get(x,y))
                        {
                            Console.Write('*');
                        }
                        else
                        {
                            Console.Write(' ');

                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        public bool TrySetBlock(Point p, Block block)
        {
            lock (grid)
            {
                var x = p.X;
                var y = p.Y;
                foreach (var point in block.Coordinates)
                {
                    if (Get(point.X+x, point.Y+y))
                    {
                        return false;
                    }
                }
                foreach (var point in block.Coordinates)
                {
                    Set(point.X + x, point.Y + y, true);
                }
            
                return true;
            }
        }

        public void ClearBlock(Point p, Block block)
        {
            lock (grid)
            {
                var x = p.X;
                var y = p.Y;
                foreach (var point in block.Coordinates)
                {
                    Set(point.X + x, point.Y + y, false);
                }
            }
        }
    }

    internal class GridWithBlockType
    {
        internal BlockTypes[] grid;
        internal ConsoleColor?[] pieceGrid;
        public int Height { get; }
        public int Width { get; }
        //static char[] symbols = new[] {' ', '@', '#', '$', '%', '&', '*', '+'};
        static char[] symbols = new[] {'\u25A0', '\u25A0', '\u25A0', '\u25A0', '\u25A0', '\u25A0', '\u25A0', '\u25A0'};
        static ConsoleColor[] colors = new[]
        {
            ConsoleColor.Cyan,
            ConsoleColor.Blue,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkYellow,
            ConsoleColor.Green,
            ConsoleColor.DarkBlue,
            ConsoleColor.Red,
            ConsoleColor.DarkGray,
            ConsoleColor.Yellow,
            ConsoleColor.Magenta,
            ConsoleColor.White
        };
        int currentPiece = 1;

        public GridWithBlockType(int height, int width)
        {
            this.grid = new BlockTypes[height * width];
            this.pieceGrid = new ConsoleColor?[height * width];
            Height = height;
            Width = width;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int x, int y, BlockTypes v, ConsoleColor piece)
        {
            grid[y * Width + x] = v;
            pieceGrid[y * Width + x] = piece;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockTypes Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return 0;

            return grid[y * Width + x];
            //return ((grid[y] >> x) & 1) == 1;
        }
        
        static Point Throw()
        {
            throw new InvalidOperationException();
        }

        public void Print()
        {
            ConsoleColor lastColor = ConsoleColor.White;
            lock (grid)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var i = (int) Get(x, y);
                        var symbol = symbols[i];
                        var color = pieceGrid[y * Width + x] ?? ConsoleColor.Black;
                        if (lastColor != color)
                        {
                            Console.ForegroundColor = color;
                            lastColor = color;
                        }
                        Console.Write(symbol);
                    }


                    Console.WriteLine();
                }
            }
            Console.ForegroundColor = ConsoleColor.White;
            
        }

        public bool TrySetBlock(Point p, Block block)
        {
            lock (grid)
            {
                var x = p.X;
                var y = p.Y;
                foreach (var point in block.Coordinates)
                {
                    if (Get(point.X + x, point.Y + y) != 0)
                    {
                        return false;
                    }
                }

                var piece = currentPiece++;
                foreach (var point in block.Coordinates)
                {
                    Set(point.X + x, point.Y + y, block.BlockType, colors[piece % colors.Length]);
                }

                return true;
            }
        }
        
    }
}
