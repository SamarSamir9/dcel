using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public struct SegRat2
    {
        public VecRat2 A, B;
        public bool AClosed, BClosed;

        //The vector from a to b
        public VecRat2 AB() { return B - A; }

        //The vector from b to a
        public VecRat2 BA() { return A - B; }

        //Is this segment actually a point?
        public bool IsPoint() { return (AClosed && BClosed) && (A == B); }

        //Is this segment empty?
        public bool IsEmpty() { return !(AClosed && BClosed) && (A == B); }

        public SegRat2(VecRat2 a, VecRat2 b)
            : this(a, b, true)
        {
        }

        public SegRat2(VecRat2 a, VecRat2 b, bool closed)
            : this(a, b, closed, closed)
        {
        }

        public SegRat2(VecRat2 a, VecRat2 b, bool aClosed, bool bClosed)
        {
            this.A = a;
            this.B = b;
            this.AClosed = aClosed;
            this.BClosed = bClosed;
        }

        //Flip the orientation of this segment
        public void Flip()
        {
            MathAid.Swap(ref A, ref B);
            MathAid.Swap(ref AClosed, ref BClosed);
        }

        //Normalize the orientation of this segment (for making segment AB behave the same as segment BA)
        public void NormalizeOrientation()
        {
            if (A > B)
                Flip();
        }

        public SegRat2 Interior()
        {
            return new SegRat2(A, B, false);
        }

        public SegRat2 Closure()
        {
            return new SegRat2(A, B, true);
        }
    }
}
