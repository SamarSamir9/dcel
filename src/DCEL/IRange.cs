using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public interface IRange<T> : IRegion<T>
    {
        T Size { get; }
        T Clamp(T t);
    }
}
