using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PlatBlogs.Views
{
    public interface IRenderable
    {
        Task<object> RenderAsync(IHtmlHelper iHtmlHelper);
    }
}
