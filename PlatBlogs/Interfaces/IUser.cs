using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Interfaces
{
    public interface IUser : IUserView
    {
        DateTime? DateOfBirth { get; }
        string City { get; }
    }
}
