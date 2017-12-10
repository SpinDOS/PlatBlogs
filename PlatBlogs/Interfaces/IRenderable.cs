using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PlatBlogs.Interfaces
{
    public interface IRenderable
    {
        Task<object> RenderAsync(IHtmlHelper iHtmlHelper);
    }
}
