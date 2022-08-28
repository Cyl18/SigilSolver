






using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security;
using ImageMagick;
using Microsoft.VisualBasic;
using SigilSolver;

//var imageProcessor = new ImageProcessor();
//imageProcessor.CutBoard(new MagickImage("a.png")).Write("x1.png");
//new SolutionProducer().Run(new MagickImage("a.png"));

//  var grid = new Grid(10,10);
//  grid.TrySetBlock(new Point(5, 5), new Blocks().ZBlock1);
// grid.Print();



WinApi.TimeBeginPeriod(1);
Console.WriteLine("3");
var cr = Console.GetCursorPosition();
Console.SetCursorPosition(cr.Left, cr.Top-1);
Console.WriteLine("2");
Console.ReadLine();
 while (true)
 {
     start:
     try
    {
            Console.Clear();
         new SolutionProducer().Screenshot();
         using var screenshot = new MagickImage("screenshot.bmp");
         var (point, width, height) = ImageProcessor.GetBoardInfo(screenshot);
         if (width <= 0 || height <= 0 || width > 10 || height > 10) throw new Exception();
     }
     catch (Exception e)
     {
         Console.WriteLine(e);
         goto start;
     }

     try
     {
        
         new SolutionProducer().Run(new MagickImage("screenshot.bmp"));
    }
     catch (Exception e)
     {
         Console.WriteLine(e);
         goto start;
     }
     Thread.Sleep(1500);
}
// var stopwatch = Stopwatch.StartNew();
// var solverCore = new SolverCore(8, 6, new[]
//     {
//         BlockTypes.I, BlockTypes.O,BlockTypes.O,BlockTypes.O,BlockTypes.O,BlockTypes.O,
//         BlockTypes.T,
//         BlockTypes.T,
//         BlockTypes.Z,
//         BlockTypes.S,
//         BlockTypes.S,
//         BlockTypes.L,
//     });
//     solverCore.Solve();
// Console.WriteLine($"{stopwatch.ElapsedMilliseconds}ms");

// foreach (var (block, point) in solve)
// {
//     Console.WriteLine($"{block.BlockType}, {point}");
// }

