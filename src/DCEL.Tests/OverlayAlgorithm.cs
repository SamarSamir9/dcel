using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DCEL;

namespace DCEL.Tests
{
    [TestClass]
    public class OverlayAlgorithm
    {
        [TestMethod]
        public void TestMethod1()
        {
            var A = new DCEL_Subdivision(new VecRat2(0, 0), new VecRat2(1, 1));
            var B = new DCEL_Subdivision(new VecRat2(0, 1), new VecRat2(1, 0));

            var C = OA_Algorithm.Overlay(A, B);
        }
    }
}
