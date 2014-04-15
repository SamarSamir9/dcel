using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public interface IRegion<T>
    {
        bool Contains(T t);
    }

    public interface IRegion<T, TOther> : IRegion<T>
    {
        bool Intersects(TOther other);
    }

    public interface IRegion<T, TOther, TIntersection> : IRegion<T, TOther>
    {
        bool Intersects(TOther other, out TIntersection pointOnThis, out TIntersection pointOnOther);
    }
}