using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public class IntRange : IEnumerable<int>, IRange<int>
    {
        //Inclusive
        public int Min { get; set; }
        //Exclusive
        public int Max { get; set; }

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public int IndexOf(int x)
        {
            return x - Min;
        }

        #region IEnumerable<int> Members

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = Min; i < Max; i++)
                yield return i;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IRegion<int> Members

        public bool Contains(int t)
        {
            return Min <= t && t < Max;
        }

        #endregion

        #region IRange<int> Members

        public int Size
        {
            get { return Max - Min; }
        }

        public int Clamp(int t)
        {
            return MathAid.Clamp(t, Min, Max);
        }

        #endregion
    }
}
