using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using PlatBlogs.Data;

namespace PlatBlogs.Views._Partials
{
    public class UserViewModel : IRenderable, IUserBasicInfo
    {
        public UserViewModel() { }

        public UserViewModel(ApplicationUser user)
        {
            Id = user.Id;
            FullName = user.FullName;
            UserName = user.UserName;
            AvatarPath = user.AvatarPath;
            PublicProfile = user.PublicProfile;
            ShortInfo = user.ShortInfo;
        }
        public string Id { get; set; }
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

        public static async Task<List<UserViewModel>> FromSqlReaderAsync(DbDataReader reader)
        {
            var result = new List<UserViewModel>();
            do
            {
                while (await reader.ReadAsync())
                {
                    var userView = new UserViewModel
                    {
                        Id = reader.GetString(0),
                        FullName = reader.GetString(1),
                        UserName = reader.GetString(2),
                        AvatarPath = reader.GetValue(3) as string,
                        PublicProfile = reader.GetBoolean(4),
                        ShortInfo = reader.GetValue(5) as string,
                    };
                    result.Add(userView);
                }
            }
            while (await reader.NextResultAsync());
            return result;
        }

        public async Task<object> RenderAsync(IHtmlHelper iHtmlHelper) 
            => await iHtmlHelper.PartialAsync("~/Views/_Partials/UserView.cshtml", this);
        
        public static implicit operator UserViewModel(ApplicationUser user) => new UserViewModel(user);
    }
}
