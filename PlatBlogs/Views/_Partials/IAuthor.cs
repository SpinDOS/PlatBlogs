using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Views._Partials
{
    public interface IAuthor
    {
        string FullName { get; }
        string UserName { get; }
        bool PublicProfile { get; }
    }

    public class SimpleAuthor: IAuthor {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public bool PublicProfile { get; set; }
    }
}
