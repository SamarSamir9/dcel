using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public abstract class DCEL_Element : IComparable<DCEL_Element>
    {
        private static int NextID = 0;

        public int ID { get; private set; }

        public DCEL_Element()
        {
            ID = NextID++;
        }

        public int CompareTo(DCEL_Element other)
        {
            return Compare(this, other);
        }

        public static int Compare(DCEL_Element a, DCEL_Element b)
        {
            return a.ID.CompareTo(b.ID);
        }

        public abstract String TypeName { get; }
        public abstract IEnumerable<Tuple<DCEL_Element, String>> IncidentElements { get; }

        
    }
}
