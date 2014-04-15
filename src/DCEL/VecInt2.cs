using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public struct VecInt2 : IEquatable<VecInt2>
    {
        public int X, Y;

        public VecInt2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static VecInt2 Zero
        {
            get { return new VecInt2(0, 0); }
        }

        public static VecInt2 UnitX
        {
            get { return new VecInt2(1, 0); }
        }

        public static VecInt2 UnitY
        {
            get { return new VecInt2(0, 1); }
        }

        public static VecInt2 One
        {
            get { return new VecInt2(1, 1); }
        }

        public static VecInt2 operator +(VecInt2 u, VecInt2 v)
        {
            return new VecInt2(u.X + v.X, u.Y + v.Y);
        }

        public static VecInt2 operator -(VecInt2 u, VecInt2 v)
        {
            return new VecInt2(u.X - v.X, u.Y - v.Y);
        }

        public static VecInt2 operator *(int c, VecInt2 u)
        {
            return new VecInt2(c * u.X, c * u.Y);
        }

        public static VecInt2 operator -(VecInt2 u)
        {
            return -1 * u;
        }

        public static bool operator ==(VecInt2 u, VecInt2 v)
        {
            return u.X == v.X && u.Y == v.Y;
        }
        
        public static bool operator >=(VecInt2 u, VecInt2 v)
        {
            return u.X >= v.X && u.Y >= v.Y;
        }

        public static bool operator >(VecInt2 u, VecInt2 v)
        {
            return u.X > v.X && u.Y > v.Y;
        }

        public static bool operator !=(VecInt2 u, VecInt2 v)
        {
            return !(u == v);
        }

        public static bool operator <(VecInt2 u, VecInt2 v)
        {
            return v > u;
        }

        public static bool operator <=(VecInt2 u, VecInt2 v)
        {
            return v >= u;
        }

        public static int ManhattanDist(VecInt2 u, VecInt2 v)
        {
            return Math.Abs(u.X - v.X) + Math.Abs(u.Y - v.Y);
        }

        public static VecInt2 AbsoluteDifference(VecInt2 u, VecInt2 v)
        {
            return new VecInt2(Math.Abs(u.X - v.X), Math.Abs(u.Y - v.Y));
        }

        public static VecInt2 Clamp(VecInt2 value, VecInt2 lo, VecInt2 hi)
        {
            return new VecInt2(MathAid.Clamp(value.X, lo.X, hi.X), MathAid.Clamp(value.Y, lo.Y, hi.Y));
        }

        public override string ToString()
        {
            return String.Format("({0},{1})", X, Y);
        }

        public static VecInt2 Rotate90CW(VecInt2 v)
        {
            return new VecInt2(v.Y, -v.X);
        }

        public static VecInt2 Rotate90CCW(VecInt2 v)
        {
            return new VecInt2(-v.Y, v.X);
        }

        public static VecInt2 Rotate(VecInt2 v, Rotation rotation)
        {
            if (rotation == Rotation.None)
                return v;
            else if (rotation == Rotation.CCW90)
                return Rotate90CCW(v);
            else if (rotation == Rotation.CCW180)
                return -v;
            else
                return Rotate90CW(v);
        }

        #region IEquatable<VecInt2> Members

        public bool Equals(VecInt2 other)
        {
            return other.X == this.X && other.Y == this.Y;
        }

        #endregion

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
