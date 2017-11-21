using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using PlatBlogs.Data;

namespace PlatBlogs.Extensions
{
    public static class ApplicationUserExtensions
    {
        public static string AvatarFilePath(this ApplicationUser user, IHostingEnvironment environment) =>
            environment.WebRootPath + user.AvatarPath;
    }
}
