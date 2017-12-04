using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Exceptions
{
    public class OffsetException : OverflowException
    {
        public OffsetException() { }
        public OffsetException(int offset) { Offset = offset; }
        public int? Offset { get; }
    }
}
