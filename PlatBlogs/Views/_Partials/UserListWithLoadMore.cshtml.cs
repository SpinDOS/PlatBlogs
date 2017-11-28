using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PlatBlogs.Data;

namespace PlatBlogs.Views._Partials
{
    public class UserListWithLoadMoreModel
    {
        public IList<ApplicationUser> Users { get; set; }
        public LoadMoreModel LoadMoreModel { get; set; }
        public string DefaultText { get; set; }
    }
}