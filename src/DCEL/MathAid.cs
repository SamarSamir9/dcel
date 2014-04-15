using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public enum RoundingMode
    {
        NearestNeighbor,
        Floor,
        Ceiling
    }

    public static class MathAid
    {
        public const float PI = (float)Math.PI;

        #region Math helper functions
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
        public static int IntComparison(int a, int b)
        {
            return a.CompareTo(b);
        }
        public static int VecInt2Comparison(VecInt2 a, VecInt2 b)
        {
            if (a.Y != b.Y)
                return IntComparison(a.Y, b.Y);
            else
                return IntComparison(a.X, b.X);
        }
        public static int FloatComparison(float a, float b)
        {
            return a.CompareTo(b);
        }
        public static Func<T, T, int> DictionaryComparison<T>(Func<T, T, int> first, Func<T, T, int> second)
        {
            return (s, t) =>
            {
                int c1 = first(s, t);
                if (c1 < 0)
                    return -1;
                else if (c1 > 0)
                    return 1;
                else
                    return second(s, t);
            };
        }
        public static Func<T, T, int> ReverseComparison<T>(Func<T, T, int> comparison)
        {
            return (t1, t2) => comparison(t2, t1);
        }

        public static float Pow(float a, float b)
        {
            return (float)Math.Pow(a, b);
        }
        public static int Pow2(int exp)
        {
            if (exp == 0)
                return 1;
            int x = Pow2(exp / 2);
            if (exp % 2 == 0)
                return x * x;
            else
                return 2 * x * x;
        }
        public static float Exp(float d)
        {
            return (float)Math.Exp(d);
        }
        public static float Log(float a, float newBase)
        {
            return (float)Math.Log(a, newBase);
        }
        public static float Log(float a)
        {
            return (float)Math.Log(a);
        }
        public static float Sqrt(float a)
        {
            return (float)Math.Sqrt(a);
        }
        public static long GCD(long x, long y)
        {
            while (y != 0)
            {
                long z = y;
                y = x % y;
                x = z;
            }
            return x;
        }

        public static int Round(float x, RoundingMode mode)
        {
            switch (mode)
            {
                case RoundingMode.NearestNeighbor:
                    return (int)Math.Round(x);
                case RoundingMode.Floor:
                    return (int)Math.Floor(x);
                case RoundingMode.Ceiling:
                    return (int)Math.Ceiling(x);
                default:
                    return Round(x);
            }
        }
        public static int Round(float x)
        {
            return Round(x, RoundingMode.NearestNeighbor);
        }

        public static int Clamp(int value, int lo, int hi)
        {
            if (value < lo)
                return lo;
            else if (value >= hi)
                return hi - 1;
            else
                return value;
        }

        public static int Wrap(int value, int lo, int hi)
        {
            int d = hi - lo;
            value = ((value - lo) % d) + lo;
            if (value < lo)
                value += d;
            return value;
        }
        public static float Wrap(float a, float min, float max)
        {
            float d = max - min;
            a = ((a - min) % d) + min;
            if (a < min)
                a += d;
            return a;
        }

        public static T[] Shuffle<T>(this IEnumerable<T> enumerable)
        {
            T[] arr = enumerable.ToArray();
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                int j = RandomAid.NextInt(i + 1);
                T temp = arr[j];
                arr[j] = arr[i];
                arr[i] = temp;
            }
            return arr;
        }

        /// <summary>
        /// Solve ax^2 + bx + c = 0. Returns number of real roots.  If 1, x1 is set.  If 2, x1 and x2 are set.
        /// </summary>
        public static int SolveQuadratic(float a, float b, float c, out float x1, out float x2)
        {
            float d = b * b - 4 * a * c;
            if (d < 0)
            {
                x1 = float.NaN;
                x2 = float.NaN;
                return 0;
            }
            else if (d == 0)
            {
                x1 = -0.5f * b / a;
                x2 = float.NaN;
                return 1;
            }
            else
            {
                float q = -0.5f * (b + Math.Sign(b) * MathAid.Sqrt(d));
                x1 = q / a;
                x2 = c / q;
                return 2;
            }
        }

        public static VecRat2 Solve2x2System(Rational a11, Rational a12, Rational a21, Rational a22, Rational b1, Rational b2)
        {
            //Swap rows if neccesary
            if (a11 == 0)
            {
                return Solve2x2System(a21, a22, a11, a12, b2, b1);
            }

            //Make a11 = 1
            a12 /= a11;
            b1 /= a11;
            
            //Make a21 = 0
            a22 -= a21 * a12;
            b2 -= a21 * b1;

            //Make a22 = 1
            b2 /= a22;

            //Make a12 = 0
            b1 -= a12 * b2;

            return new VecRat2(b1, b2);
        }

        public static Rational Det2(
            Rational a, Rational b,
            Rational c, Rational d)
        {
            return a * d - b * c;
        }

        public static Rational Det2(
            VecRat2 row1,
            VecRat2 row2)
        {
            return Det2(
                row1.X, row1.Y,
                row2.X, row2.Y);
        }

        public static Rational Det3(
            Rational a, Rational b, Rational c,
            Rational d, Rational e, Rational f,
            Rational g, Rational h, Rational i)
        {
            return a * e * i + b * f * g + c * d * h - a * f * h - b * d * i - c * e * g;
        }

        /// <summary>
        /// -1 => right turn
        /// 0 => no turn
        /// 1 => left turn
        /// </summary>
        public static int TurnTest(VecRat2 a, VecRat2 b, VecRat2 c)
        {
            //return Rational.Sign(a.X * b.Y + a.Y * c.X + b.X * c.Y - a.X * c.Y - a.Y * b.X - b.Y * c.X);
            VecRat2 u = b - a;
            u = new VecRat2(u.Y, -u.X); //turn to right 90 degrees
            Rational udota = u.X * a.X + u.Y * a.Y;
            Rational udotc = u.X * c.X + u.Y * c.Y;
            return udota.CompareTo(udotc);
        }

        public static Turn TurnTestDiscrete(VecRat2 a, VecRat2 b, VecRat2 c)
        {
            int sign = TurnTest(a, b, c);
            if (sign < 0)
                return Turn.Right;
            else if (sign > 0)
                return Turn.Left;
            else
                return Turn.Linear;
        }

        /// <summary>
        /// Determines whether u is in the circumcircle of a, b, and c.
        /// </summary>
        public static Containment CircumcircleTest(VecRat2 a, VecRat2 b, VecRat2 c, VecRat2 d)
        {
            Turn t = TurnTestDiscrete(a, b, c);
            if (t == Turn.Right)
            {
                VecRat2 temp = b;
                b = c;
                c = temp;
            }
            else if (t == Turn.Linear)
            {
                throw new Exception("Can't do circumcircle test with a,b,c colinear.");
            }

            Rational ld = d.LengthSquared();

            Rational r = Det3(
                a.X - d.X, a.Y - d.Y, a.LengthSquared() - ld,
                b.X - d.X, b.Y - d.Y, b.LengthSquared() - ld,
                c.X - d.X, c.Y - d.Y, c.LengthSquared() - ld);

            if (r > 0)
                return Containment.Inside;
            else if (r == 0)
                return Containment.Boundary;
            else
                return Containment.Outside;
        }

        public static VecRat2 LineIntersection(VecRat2 u1, VecRat2 v1, VecRat2 u2, VecRat2 v2)
        {
            Rational u1v1det = u1.X * v1.Y - u1.Y * v1.X;
            Rational u2v2det = u2.X * v2.Y - u2.Y * v2.X;
            VecRat2 diff1 = u1 - v1;
            VecRat2 diff2 = u2 - v2;

            Rational denom = diff1.X * diff2.Y - diff1.Y * diff2.X;

            Rational numx = u1v1det * diff2.X - u2v2det * diff1.X;
            Rational numy = u1v1det * diff2.Y - u2v2det * diff1.Y;
            return new VecRat2(numx / denom, numy / denom);
        }

        /// <summary>
        /// Returns whether x is in triangle(abc)
        /// </summary>
        public static bool TriangleTest(VecRat2 a, VecRat2 b, VecRat2 c, VecRat2 x)
        {
            int s1 = Rational.Sign(TurnTest(a, b, c));
            int s2 = Rational.Sign(TurnTest(a, b, x));
            if (s1 != s2)
            {
                return false;
            }
            int s3 = Rational.Sign(TurnTest(b, c, x));
            if (s2 != s3)
            {
                return false;
            }
            int s4 = Rational.Sign(TurnTest(c, a, x));
            return s3 == s4;
        }

        /// <summary>
        /// Decay from 1 to 0.
        /// </summary>
        public static float Decay(float halfLife, float elapsedTime)
        {
            if (halfLife <= 0)
                return 0;
            else
                return MathAid.Pow(2, -elapsedTime / halfLife);
        }

        public static float Lerp(float a, float b, float amount)
        {
            return a + (b - a) * amount;
        }

        /// <summary>
        /// Lerps between the two angles via the shorter side.
        /// </summary>
        public static float AngleLerp(float a, float b, float amount)
        {
            a = Wrap(a, 0, 2 * PI);
            b = Wrap(b, 0, 2 * PI);
            if (a < b - 2 * PI)
                return Lerp(a, b - 2 * PI, amount);
            else if (a > b + PI)
                return Lerp(a, b + 2 * PI, amount);
            else
                return Lerp(a, b, amount);
        }
        public static float AngleDistance(float a, float b)
        {
            return Math.Min(Wrap(b - a, 0, 2 * PI), Wrap(a - b, 0, 2 * PI));
        }
        
        private static readonly Rotation[] rotations = (Rotation[])Enum.GetValues(typeof(Rotation));
        public static Rotation OppositeRotation(Rotation r)
        {
            if (r == Rotation.None)
                return Rotation.CCW180;
            else if (r == Rotation.CCW90)
                return Rotation.CCW270;
            else if (r == Rotation.CCW180)
                return Rotation.None;
            else
                return Rotation.CCW90;
        }

        /// <summary>
        /// Converts an (unnormalized) pdf to a normalized pareto.
        /// </summary>
        public static IList<float> Pareto(IList<float> frequencies)
        {
            float[] pareto = new float[frequencies.Count];

            if (frequencies.Count == 0)
                return pareto;

            pareto[0] = frequencies[0];
            for (int i = 1; i < frequencies.Count; i++)
            {
                pareto[i] = pareto[i - 1] + frequencies[i];
            }

            for (int i = 0; i < frequencies.Count - 1; i++)
            {
                pareto[i] /= pareto[frequencies.Count - 1];
            }
            pareto[frequencies.Count - 1] = 1;
            return pareto;
        }

        public static IEnumerable<float> LinSpace(float min, float max, int segments)
        {
            //Assuming segments >= 1
            yield return min;
            for (int i = 1; i < segments; i++)
            {
                float t = (float)i / (float)segments;
                yield return Lerp(min, max, t);
            }
            yield return max;
        }
        public static IEnumerable<float> LinSpace(float min, float max, float approximateSegmentSize, RoundingMode mode)
        {
            return MathAid.LinSpace(min, max, CountSegments(min, max, approximateSegmentSize, mode));
        }
        public static int CountSegments(float min, float max, float approximateSegmentSize, RoundingMode mode)
        {
            return Math.Max(1, Round((max - min) / approximateSegmentSize, mode));
        }

        public static void Partition<T>(this IEnumerable<T> list, Predicate<T> test, out IEnumerable<T> passed, out IEnumerable<T> failed)
        {
            LinkedList<T> pass = new LinkedList<T>();
            LinkedList<T> fail = new LinkedList<T>();
            foreach (T t in list)
                (test(t) ? pass : fail).AddLast(t);
            passed = pass;
            failed = fail;
        }

        public static void Partition<T>(this IEnumerable<T> list, Predicate<T> test, out List<T> passed, out List<T> failed)
        {
            passed = new List<T>();
            failed = new List<T>();
            foreach (T t in list)
                (test(t) ? passed : failed).Add(t);
        }

        public static void RemoveDuplicates<T>(this List<T> list, Comparison<T> comparison)
        {
            list.RemoveDuplicates(comparison, (a, b) => a);
        }

        public static void RemoveDuplicates<T>(this List<T> list, Comparison<T> comparison, Func<T, T, T> join)
        {
            list.Sort(comparison);
            int next = 0;
            int start = 0;
            int end = 1;
            while (start < list.Count)
            {
                while (end < list.Count && comparison(list[start], list[end]) == 0)
                    end++;

                list[next] = list[start];
                for (int i = start + 1; i < end; i++)
                    list[next] = join(list[next], list[i]);
                next++;

                start = end;
            }

            //The new count is curr.
            list.RemoveRange(next, list.Count - next);
            list.TrimExcess();
        }

        public static IEnumerable<List<T>> SetPower<T>(this IEnumerable<T> set, int power)
        {
            if (power == 1)
            {
                foreach (T t in set)
                {
                    List<T> L = new List<T>(1);
                    L.Add(t);
                    yield return L;
                }
            }
            else
            {
                foreach (var X in set.SetPower(power-1))
                {
                    foreach (T t in set)
                    {
                        List<T> L = new List<T>(X.Count + 1);
                        L.AddRange(X);
                        L.Add(t);
                        yield return L;
                    }
                }
            }
        }

        //public static IEnumerable<List<bool>> AllBitVectors(int dimensions)
        //{
        //    if (dimensions == 1)
        //    {
        //        List<bool> u = new List<bool>(1);
        //        u.Add(false);
        //        List<bool> v = new List<bool>(1);
        //        v.Add(true);
        //        yield return u;
        //        yield return v;
        //    }
        //    else
        //    {
        //        foreach (var u in AllBitVectors(dimensions - 1))
        //        {
        //            List<bool> v = new List<bool>(u);
        //            u.Add(false);
        //            v.Add(true);
        //            yield return u;
        //            yield return v;
        //        }
        //    }
        //}

        #endregion

        #region Collection extension methods

        public static IntRange Indices<T>(this IList<T> list)
        {
            return new IntRange(0, list.Count);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> list)
        {
            return !list.GetEnumerator().MoveNext();
        }

        public static bool CountGreaterThan<T>(this IEnumerable<T> list, int n)
        {
            int i = 0;
            using (var enumerator = list.GetEnumerator())
            {
                while (enumerator.MoveNext() && i <= n)
                    i++;
            }
            return i > n;
        }

        public static T Min<T>(this IEnumerable<T> list, Func<T, T, int> comparison)
        {
            T min = list.First();
            foreach (T t in list)
            {
                int c = comparison(min, t);
                if (c > 0)
                {
                    min = t;
                }
            }
            return min;
        }

        public static T Max<T>(this IEnumerable<T> list, Func<T, T, int> comparison)
        {
            T max = list.First();
            foreach (T t in list)
            {
                int c = comparison(max, t);
                if (c < 0)
                {
                    max = t;
                }
            }
            return max;
        }
 
        /// <summary>
        /// Returns element t such that f(t) is maximum.
        /// </summary>
        public static T Argmax<T>(this IEnumerable<T> list, Func<T, float> f)
        {
            T best = list.First();
            float bestF = f(best);
            foreach (T t in list)
            {
                float tF = f(t);
                if (tF > bestF)
                {
                    best = t;
                    bestF = tF;
                }
            }
            return best;
        }

        /// <summary>
        /// Returns element t such that f(t) is minimum.
        /// </summary>
        public static T Argmin<T>(this IEnumerable<T> list, Func<T, float> f)
        {
            T best = list.First();
            float bestF = f(best);
            foreach (T t in list)
            {
                float tF = f(t);
                if (tF < bestF)
                {
                    best = t;
                    bestF = tF;
                }
            }
            return best;
        }

        /// <summary>
        /// Returns index of element t such that f(t) is maximum.
        /// </summary>
        public static int Idxmax<T>(this IList<T> list, Func<T, float> f)
        {
            int best = 0;
            float bestF = f(list[best]);
            for (int i = 0; i < list.Count; i++)
            {
                T t = list[i];
                float tF = f(t);
                if (tF > bestF)
                {
                    best = i;
                    bestF = tF;
                }
            }
            return best;
        }
        /// <summary>
        /// Returns index of element t such that f(t) is minimum.
        /// </summary>
        public static int Idxmin<T>(this IList<T> list, Func<T, float> f)
        {
            int best = 0;
            float bestF = f(list[best]);
            for (int i = 0; i < list.Count; i++)
            {
                T t = list[i];
                float tF = f(t);
                if (tF < bestF)
                {
                    best = i;
                    bestF = tF;
                }
            }
            return best;
        }

        public static float Median(this IEnumerable<float> list)
        {
            return new List<float>(list).Median();
        }
        public static float Median(this IList<float> list)
        {
            if (list.Count % 2 == 1)
            {
                return list.SelectKth(list.Count / 2);
            }
            else
            {
                float b = list.SelectKth(list.Count / 2);
                float a = list.Take(list.Count / 2).Max();
                return a + (b - a) / 2;
            }
        }
        //Enforces the median to be distinct from all the data points in the list
        public static float MedianDistinct(this IEnumerable<float> list)
        {
            return new List<float>(list).MedianDistinct();
        }
        public static float MedianDistinct(this IList<float> list)
        {
            int k = (list.Count - 1) / 2;
            float a = list.SelectKth(k);
            while (k < list.Count - 1)
            {
                float b = list.SelectKth(k + 1);
                if (a == b)
                {
                    a = b;
                    k++;
                }
                else
                {
                    return a + (b - a) / 2;
                }
            }
            return list.Max() + 1;
        }

        public static int SelectKth(this IList<int> list, int k)
        {
            return list.SelectKth(k, IntComparison);
        }
        public static float SelectKth(this IList<float> list, int k)
        {
            return list.SelectKth(k, FloatComparison);
        }

        /// <summary>
        /// Randomized Quickselect algorithm
        /// </summary>
        /// <remarks>
        /// O(n) expected
        /// O(n^2) worst-case
        /// </remarks>
        public static T SelectKth<T>(this IList<T> list, int k, Func<T, T, int> comparison)
        {
            if (list.Count == 0)
                throw new InvalidOperationException();

            IntRange indices = list.Indices();

            if (!indices.Contains(k))
                throw new IndexOutOfRangeException();

            int pivot;
            while (true)
            {
                pivot = list.Partition(comparison, indices);
                if (k == pivot)
                    return list[k];
                else if (k < pivot)
                    indices.Max = pivot;
                else
                    indices.Min = pivot + 1;
            }
        }
        private static int Partition<T>(this IList<T> list, Func<T, T, int> comparison, IntRange indices)
        {
            return list.Partition(RandomAid.NextInt(indices), comparison, indices);
        }
        private static int Partition<T>(this IList<T> list, int pivotIndex, Func<T, T, int> comparison, IntRange indices)
        {
            T pivot = list[pivotIndex];
            list.Swap(pivotIndex, indices.Max - 1);

            int L = indices.Min;
            int R = indices.Max - 2;
            while (true)
            {
                while (L <= R && comparison(list[L], pivot) <= 0)
                {
                    L++;
                }
                while (L <= R && comparison(pivot, list[R]) <= 0)
                {
                    R--;
                }
                if (L < R)
                {
                    list.Swap(L, R);
                }
                else
                {
                    list.Swap(L, indices.Max - 1);
                    return L;
                }
            }
        }
        private static void Swap<T>(this IList<T> list, int i, int j)
        {
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static IEnumerable<T[]> ConsecutivePairs<T>(this IEnumerable<T> list)
        {
            bool first = true;
            T prev = default(T);
            foreach (T t in list)
            {
                if (!first)
                {
                    yield return new T[] { prev, t };
                }
                prev = t;
                first = false;
            }
        }
        public static void ConsecutivePairs<T>(this IEnumerable<T> list, Action<T, T> action)
        {
            bool first = true;
            T prev = default(T);
            foreach (T t in list)
            {
                if (!first)
                {
                    action(prev, t);
                }
                prev = t;
                first = false;
            }
        }

        public static void ConsecutivePairs_Cycle<T>(this IList<T> list, Action<T, T> action)
        {
            for (int i = 1; i < list.Count; i++)
                action(list[i - 1], list[i]);
            action(list[list.Count - 1], list[0]);
        }

        public static void AllPairs<T>(this IList<T> list, Action<T, T> action)
        {
            for (int i = 0; i < list.Count; i++)
                for (int j = i + 1; j < list.Count; j++)
                    action(list[i], list[j]);
        }
        public static void AllPairs<T>(this IEnumerable<T> list, Action<T, T> action)
        {
            int i = 0;
            foreach (T ti in list)
            {
                int j = 0;
                foreach (T tj in list)
                {
                    if (i < j)
                    {
                        action(ti, tj);
                    }
                    j++;
                }
                i++;
            }
        }
        public static void AllPairs<T>(this LinkedList<T> list, Action<T, T> action)
        {
            for (var i = list.First; i != null; i = i.Next)
                for (var j = i.Next; j != null; j = j.Next)
                    action(i.Value, j.Value);
        }

        public static IEnumerable<T> CollapsePairs<T>(this IEnumerable<T> list, Func<T, T, T> collapse)
        {
            T even = default(T);
            T odd = default(T);
            int i = 0;
            foreach (T t in list)
            {
                if (i % 2 == 0)
                    even = t;
                else
                    odd = t;
                i++;

                if (i % 2 == 0)
                    yield return collapse(even, odd);
            }

            if (i % 2 == 1)
                yield return even;
        }
        
        public static int FirstIndex<T>(this IList<T> list, Predicate<T> test)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (test(list[i]))
                    return i;
            }
            return -1;
        }

        public static void Set<T>(this IList<T> list, Func<int, T> f)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = f(i);
            }
        }
        
        //public static bool Exists<T>(this IEnumerable<T> list, Predicate<T> predicate)
        //{
        //    foreach (T t in list)
        //        if (predicate(t))
        //            return true;
        //    return false;
        //}
        //public static bool All<T>(this IEnumerable<T> list, Predicate<T> predicate)
        //{
        //    foreach (T t in list)
        //        if (!predicate(t))
        //            return false;
        //    return true;
        //}

        public static void Remove<T>(this IList<T> list, Predicate<T> predicate)
        {
            Stack<int> indices = new Stack<int>();
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i]))
                {
                    indices.Push(i);
                }
            }
            while (indices.Count > 0)
            {
                list.RemoveAt(indices.Pop());
            }
        }

        #endregion
    }
}
