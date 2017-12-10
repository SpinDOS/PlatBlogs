using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlatBlogs.Interfaces;

namespace PlatBlogs.Views._Partials
{
    public class ListWithLoadMoreModel
    {
        public IReadOnlyCollection<IRenderable> Elements { get; set; }
        public string DefaultText { get; set; }
        public LoadMoreModel LoadMoreModel { get; set; }
    }
}
