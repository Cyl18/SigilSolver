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
        public int Height { get; }
        public int Width { get; }
        static char[] symbols = new[] {' ', '@', '#', '$', '%', '&', '*', '+'};

        public GridWithBlockType(int height, int width)
        {
            this.grid = new BlockTypes[height * width];
            Height = height;
            Width = width;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int x, int y, BlockTypes v)
        {
            grid[y * Width + x] = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockTypes Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return 0;

            return grid[y * Width + x];
            //return ((grid[y] >> x) & 1) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point FindFirstZero()
        {
            for (var i = 0; i < grid.Length; i++)
            {
                if (grid[i] != 0)
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

        public string Print()
        {
            var sb = new StringBuilder();
            lock (grid)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        sb.Append(symbols[(int)Get(x, y)]);
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString();
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
                foreach (var point in block.Coordinates)
                {
                    Set(point.X + x, point.Y + y, block.BlockType);
                }

                return true;
            }
        }
        
    }
}
