using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PlatBlogs.Views
{
    public interface IRenderable
    {
        Task RenderAsync(IHtmlHelper iHtmlHelper);
    }
}
