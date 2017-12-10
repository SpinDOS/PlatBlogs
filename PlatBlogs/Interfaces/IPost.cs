using System;

namespace PlatBlogs.Interfaces
{
    public interface IPost
    {
        IAuthor Author { get; }
        int Id { get; }
        string Message { get; }
        DateTime DateTime { get; }
    }
}
