using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Interfaces
{
    public interface IUserView : IAuthor
    {
        string AvatarPath { get; }
        string ShortInfo { get; }
    }
}
