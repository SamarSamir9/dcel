using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCEL;

namespace DCEL.Scripts
{
    class Program
    {
        static void Main(string[] args)
        {
            var A = new DCEL_Subdivision(new VecRat2(0, 0), new VecRat2(1, 1));
            var B = new DCEL_Subdivision(new VecRat2(0, 1), new VecRat2(1, 0));

            var C = OA_Algorithm.Overlay(A, B);
            C = C;
        }
    }
}
