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
            if (offset < 0 || offset == int.MaxValue)
                throw new OffsetException(offset);
            count = Math.Min(count, int.MaxValue - offset);
            return offset + count;
        }
    }
}
