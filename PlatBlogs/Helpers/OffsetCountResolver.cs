using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatBlogs.Exceptions;

namespace PlatBlogs.Helpers
{
    public static class OffsetCountResolver
    {
        public static int ResolveOffsetCount(int offset, ref int count)
        {
            if (offset < 0 || offset >= int.MaxValue)
                throw new OffsetException(offset);
            count = Math.Min(count, int.MaxValue - offset);
            return offset + count;
        }
        public static int ResolveOffsetCountWithReserve(int offset, ref int count)
        {
            const int limit = int.MaxValue - 1;
            if (offset < 0 || offset >= limit)
                throw new OffsetException(offset);
            count = Math.Min(count, limit - offset);
            return offset + count;
        }
    }
}
