using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public static class RandomAid
    {
        private static Random random = new Random();

        public static bool NextBool(float p)
        {
            return NextFloat() <= p;
        }

        public static int NextInt(int max)
        {
            return random.Next(max);
        }

        public static int NextInt(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int NextInt(IntRange range)
        {
            return random.Next(range.Min, range.Max);
        }

        public static float NextFloat()
        {
            return (float)random.NextDouble();
        }

        public static float NextGaussian(float mean, float standardDeviation)
        {
            return mean + standardDeviation * (float)(Math.Sqrt(-2 * Math.Log(random.NextDouble())) * Math.Cos(2 * Math.PI * random.NextDouble()));
        }

        public static int SamplePareto(IList<float> pareto)
        {
            //Could do binary search, but don't bother.
            float y = (float)random.NextDouble();
            return pareto.FirstIndex(x => y <= x);
        }

        public static T Random<T>(this IEnumerable<T> list)
        {
            return list.ElementAt(NextInt(list.Count()));
        }
    }
}
