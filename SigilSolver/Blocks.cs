using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigilSolver
{
    public class Block
    {
        public Point[] Coordinates { get; }
        public BlockTypes BlockType { get; }
        public int Variant { get; }

        public Block(Point[] coordinates, BlockTypes type, int variant)
        {
            Coordinates = coordinates;
            BlockType = type;
            Variant = variant;
        }
    }

    public enum BlockTypes
    {
        NULL, I, O, T, L, J, S, Z
    }

    public class Blocks
    {
        public Block IBlock2 = new Block(new Point[] { new (0, 0), new (0, 1), new(0, 2), new(0, 3) }, BlockTypes.I, 2);
        public Block IBlock1 = new Block(new Point[] { new (0, 0), new (1, 0), new(2, 0), new(3, 0) }, BlockTypes.I, 1);

        public Block OBlock1 = new Block(new Point[] { new(0, 0), new(1, 0), new(0, 1), new(1, 1), }, BlockTypes.O, 1);

        public Block TBlock3 = new Block(new Point[] { new(0, 0), new(1, 0), new(2, 0), new(1, 1), }, BlockTypes.T, 3);
        public Block TBlock4 = new Block(new Point[] { new(0, 0), new(0, 1), new(-1, 1), new(0, 2), }, BlockTypes.T, 4);
        public Block TBlock2 = new Block(new Point[] { new(0, 0), new(0, 1), new(1, 1), new(0, 2), }, BlockTypes.T, 2);
        public Block TBlock1 = new Block(new Point[] { new(0, 0), new(0, 1), new(-1, 1), new(1, 1), }, BlockTypes.T, 1);

        public Block JBlock2 = new Block(new Point[] { new(0, 0), new(0, 1), new(0, 2), new(-1, 2), }, BlockTypes.J, 2);
        public Block JBlock3 = new Block(new Point[] { new(0, 0), new(0, 1), new(1, 1), new(2, 1), }, BlockTypes.J, 3);
        public Block JBlock4 = new Block(new Point[] { new(0, 0), new(1, 0), new(0, 1), new(0, 2), }, BlockTypes.J, 4);
        public Block JBlock1 = new Block(new Point[] { new(0, 0), new(1, 0), new(2, 0), new(2, 1), }, BlockTypes.J, 1);

        public Block LBlock4 = new Block(new Point[] { new(0, 0), new(0, 1), new(0, 2), new(1, 2), }, BlockTypes.L, 4);
        public Block LBlock3 = new Block(new Point[] { new(0, 0), new(0, 1), new(-1, 1), new(-2, 1), }, BlockTypes.L, 3);
        public Block LBlock2 = new Block(new Point[] { new(0, 0), new(1, 0), new(1, 1), new(1, 2), }, BlockTypes.L, 2);
        public Block LBlock1 = new Block(new Point[] { new(0, 0), new(1, 0), new(2, 0), new(0, 1), }, BlockTypes.L, 1);


        public Block SBlock1 = new Block(new Point[] { new(0, 0), new(1, 0), new(0, 1), new(-1, 1), }, BlockTypes.S, 1);
        public Block SBlock2 = new Block(new Point[] { new(0, 0), new(0, 1), new(1, 1), new(1, 2), }, BlockTypes.S, 2);


        public Block ZBlock1 = new Block(new Point[] { new(0, 0), new(1, 0), new(1, 1), new(2, 1), }, BlockTypes.Z, 1);
        public Block ZBlock2 = new Block(new Point[] { new(0, 0), new(0, 1), new(-1, 1), new(-1, 2), }, BlockTypes.Z, 2);
    }
}
