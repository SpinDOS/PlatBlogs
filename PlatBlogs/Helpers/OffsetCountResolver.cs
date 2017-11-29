using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Helpers
{
    public static class OffsetCountResolver
    {
        public static bool ResolveOffsetCount(ref int offset, ref int count, bool ajax)
        {
            offset = Math.Max(offset, 0);

            var overflow = offset + count + 1 < 0;
            if (overflow)
                count = int.MaxValue - offset - 1;

            if (!ajax)
            {
                count += offset;
                offset = 0;
            }
            return overflow;
        }
    }
}
