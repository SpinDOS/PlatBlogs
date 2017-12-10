namespace PlatBlogs.Interfaces
{
    public interface IAuthor
    {
        string Id { get; }
        string FullName { get; }
        string UserName { get; }
        bool PublicProfile { get; }
    }
}
