using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SigilSolver
{
    internal class SolverCore
    {
        static Block[][] Blocks;

        static SolverCore()
        {
            Blocks = new Block[8][];
            var b = new Blocks();
            var blocks = typeof(Blocks).GetFields().ToDictionary(f => f.Name, f => (Block)f.GetValue(b)!);

            foreach (var name in Enum.GetNames<BlockTypes>())
            {
                var list = new List<Block>();
                for (int i = 1; i <= 4; i++)
                {
                    var key = $"{name}Block{i}";
                    if (blocks.ContainsKey(key))
                    {
                        list.Add(blocks[key]);
                    }
                }

                Blocks[(int)Enum.Parse<BlockTypes>(name)] = list.ToArray();
            }
        }
        public SolverCore(int height, int width, BlockTypes[] blockTypes)
        {
            grid = new Grid(height, width);
            bs = blockTypes;
        }


        readonly object drawLock = new ();
        public Solution Solve()
        {
            var cts = new CancellationTokenSource();
            bool fin = false;
            Task.Run(() =>
            {
                var drawn = false;
                while (true)
                {
                    lock (drawLock)
                    {
                        if (cts.IsCancellationRequested)
                        {
                            Draw(drawn, true);
                            Volatile.Write(ref fin, true);
                            break;
                        }
                        Draw(drawn);
                    }

                    Thread.SpinWait(100000);
                }

                void Draw(bool needClear, bool drawLast = false)
                {
                    var blocks = drawLast ? solutions.Min.Sequence : solutionStack.ToArray();
                    var drawGrid = new GridWithBlockType(grid.Height, grid.Width);
                    var count = 0;
                    foreach (var (block, point) in blocks)
                    {
                        count++;
                        drawGrid.TrySetBlock(point, block);
                    }

                    if (count==0) return;
                    drawn = true;
                    if (needClear)
                    {
                        var pos = Console.GetCursorPosition();
                        Console.SetCursorPosition(pos.Left, pos.Top-grid.Height);
                    }
                    drawGrid.Print();
                }

            });
            try
            {
                RunPermutations(0, bs.Length);
            }
            catch (TimeoutException)
            {
            }


            cts.Cancel();
            SpinWait.SpinUntil(() => Volatile.Read(ref fin));
            lock (drawLock)
            {
                Console.WriteLine($"> 共找到 {solutions.Count} 种解法, 最大解法变体数: {solutions.Max?.VariantsCount} 最小解法变体数: {solutions.Min?.VariantsCount}");
                if (solutions.Count == 0)
                {
                    return null;
                }
                else
                {
                    return solutions.Min;
                }
            }
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool ShouldSwap(BlockTypes[] str, int start, int curr)
        {
            for (int i = start; i < curr; i++)
                if (str[i] == str[curr])
                    return false;
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void Swap(int x, int y)
        {
            (bs[x], bs[y]) = (bs[y], bs[x]);
        }

        BlockTypes[] bs;
        Grid grid;

        SortedSet<Solution> solutions = new();
        ConcurrentStack<(Block, Point)> solutionStack = new ();
        Stopwatch runningStopwatch = Stopwatch.StartNew();
        internal void RunPermutations(int index, int n)
        {
            if (index >= n)
            {
                solutions.Add(new Solution(solutionStack.ToArray()));
                if (runningStopwatch.ElapsedMilliseconds > 1500)
                {
                    throw new TimeoutException();
                }
            }

            for (int i = index; i < n; i++)
            {
                bool check = ShouldSwap(bs, index, i);

                if (check)
                {
                    Swap(index, i);

                    var zp = grid.FindFirstZero();
                    foreach (var r in Blocks[(int)bs[index]])
                    {
                        if (grid.TrySetBlock(zp, r))
                        {
                            solutionStack.Push((r, zp));
                            RunPermutations(index + 1, n);
                            //Thread.SpinWait(500);
                            Thread.SpinWait(1000);
                            solutionStack.TryPop(out _);
                            grid.ClearBlock(zp, r);
                        }
                    }

                    Swap(index, i);
                }
            }
        }

    }

    class Solution : IComparable<Solution>, IComparable, IEnumerable<(Block, Point)>
    {
        public Solution((Block, Point)[] sequence)
        {
            Sequence = sequence;
            VariantsCount = sequence.Sum(b => b.Item1.Variant - 1);
        }

        public (Block, Point)[] Sequence { get; }

        public int VariantsCount { get; }

        public int CompareTo(Solution? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return VariantsCount.CompareTo(other.VariantsCount);
        }

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            return obj is Solution other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Solution)}");
        }

        public static bool operator <(Solution? left, Solution? right)
        {
            return Comparer<Solution>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(Solution? left, Solution? right)
        {
            return Comparer<Solution>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(Solution? left, Solution? right)
        {
            return Comparer<Solution>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(Solution? left, Solution? right)
        {
            return Comparer<Solution>.Default.Compare(left, right) >= 0;
        }

        public IEnumerator<(Block, Point)> GetEnumerator()
        {
            return Sequence.OfType<(Block, Point)>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Sequence.GetEnumerator();
        }
    }
    public static class WinApi
    {
        /// <summary>TimeBeginPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod", SetLastError = true)]

        public static extern uint TimeBeginPeriod(uint uMilliseconds);

        /// <summary>TimeEndPeriod(). See the Windows API documentation for details.</summary>

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"), SuppressUnmanagedCodeSecurity]
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod", SetLastError = true)]

        public static extern uint TimeEndPeriod(uint uMilliseconds);
    }

}
