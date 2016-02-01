using MP4BoxWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subtitles {
    class Program {
        static void Main(string[] args) {

            string UnoptimizedVideoPath = @"D:\CntComp_LCU_OnlyVideo\Unoptimize\shared\Test300.mp4";
            string OptimizedVideoPath = @"D:\CntComp_LCU_OnlyVideo\Optimize\shared\Test300.mp4";

            BoxWrapper mp4Box = new BoxWrapper();
            mp4Box.RestoreSubtitles(UnoptimizedVideoPath, OptimizedVideoPath);

            Console.WriteLine("Done! Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
