using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Views
{
    public class SearchModel
    {
        public string Q { get; set; }
        public ListWithLoadMoreModel Users { get; set; }
        public ListWithLoadMoreModel Posts { get; set; }
    }
}
