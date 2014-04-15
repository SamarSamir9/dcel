using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    public class OA_Source<DCEL_T>
        where DCEL_T : DCEL_Element
    {
        public DCEL_Subdivision Subdivision { get; set; }
        public DCEL_T Element { get; set; }

        public OA_Source(DCEL_Subdivision subdivision, DCEL_T element)
        {
            Subdivision = subdivision;
            Element = element;
        }

        public static Func<OA_Source<DCEL_T>, bool> IsFrom(DCEL_Subdivision subdivision)
        {
            return source => (source.Subdivision == subdivision);
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", Subdivision, Element);
        }
    }
}
