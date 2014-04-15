using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCEL;
using System.Drawing;
using System.Reflection;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace DCEL.Scripts
{
    class Program
    {
        static void Main(string[] args)
        {
            var A = DCEL_Subdivision.MakeClosedPolygon(new VecRat2(0, 0), new VecRat2(0, 1), new VecRat2(2, 1), new VecRat2(1, 0));
            var B = new DCEL_Subdivision(new VecRat2(0, 1), new VecRat2(2, 0));
            var C = DCEL_Subdivision.MakeClosedPolygon(new VecRat2(0, -1), new VecRat2(-1, 2), new VecRat2(2, 2), new VecRat2(4, -2));
            var D = DCEL_Subdivision.MakeClosedPolygon(new VecRat2(2, -1), new VecRat2(1, -1), new VecRat2(3, -2));

            var Z = OA_Algorithm.OverlayMany(new[] { A, B, C, D });
            SubdivisionRenderer.Render(Z, bitmap => bitmap.Save("render.png", ImageFormat.Png));
        }
    }
}
