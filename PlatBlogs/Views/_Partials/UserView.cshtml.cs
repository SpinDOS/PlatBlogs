using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlatBlogs.Data;

namespace PlatBlogs.Views._Partials
{
    public class UserViewModel : IRenderable
    {
        public UserViewModel() { }

        public UserViewModel(ApplicationUser user)
        {
            FullName = user.FullName;
            UserName = user.UserName;
            AvatarPath = user.AvatarPath;
            PublicProfile = user.PublicProfile;
            ShortInfo = user.ShortInfo;
        }
        public string FullName { get; set; }
        public string UserName { get; set; }
        private string _avatarPath;
        public string AvatarPath
        {
            get => _avatarPath ?? "/avatars/_no_image_.png";
            set => _avatarPath = value;
        }
        public bool PublicProfile { get; set; }
        public string ShortInfo { get; set; }

        public async Task RenderAsync(IHtmlHelper iHtmlHelper)
        {
            await iHtmlHelper.PartialAsync("~/Views/_Partials/UserView.cshtml", this);
        }

        public static implicit operator UserViewModel(ApplicationUser user) => new UserViewModel(user);
    }
}
