using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlatBlogs.Pages._Partials;

namespace PlatBlogs.Views._Partials
{
    public class PostCollectionWithLoadMoreModel
    {
        public IList<PostView> Posts { get; set; }
        public LoadMoreModel LoadMoreModel { get; set; }
    }
}