using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlatBlogs.Helpers
{
    public static class QueryBuildHelpers
    {
        
        public static class OffsetCount
        {

            public static string FetchWithOffsetBlock(int offset, int? count = null) =>
                $" OFFSET {offset} ROWS " +
                (count.HasValue? $"FETCH NEXT {count.Value} ROWS ONLY " : null);

            public static string FetchWithOffsetWithReserveBlock(int offset, int? count = null) =>
                $" OFFSET {offset} ROWS " + 
                (count.HasValue ? $"FETCH NEXT {count.Value + 1} ROWS ONLY " : null);


        }
        public static class Followers
        {
            public enum FieldNames { FollowerId, }

            public static string UserFollowersIdsQuery(string userId) =>
$@" SELECT FollowerId AS {nameof(FieldNames.FollowerId)} 
FROM Followers 
WHERE FollowedId = '{userId}' ";

        }

        public static class WhereClause
        {

            public static string OpenedUsersWhereClause(string viewerId) =>
$@" WHERE {nameof(Users.FieldNames.PublicProfile)} = 1 OR 
          {nameof(Users.FieldNames.Id)} = '{viewerId}' OR 
          {nameof(Users.FieldNames.Id)} IN 
              ({Followers.UserFollowersIdsQuery(viewerId)}) ";

            public static string FollowedUsersWhereClause(string viewerId) =>
$@" WHERE {nameof(Users.FieldNames.Id)} IN 
              (SELECT FollowedId FROM Followers WHERE FollowerId = '{viewerId}') AND 
          ({nameof(Users.FieldNames.PublicProfile)} = 1 OR 
              {nameof(Users.FieldNames.Id)} IN 
                  ({Followers.UserFollowersIdsQuery(viewerId)}) 
          ) ";

        }

        public static class Users
        {
            public enum FieldNames
            {
                Id, FullName, UserName,
                DateOfBirth, City, ShortInfo,
                AvatarPath, PublicProfile,
            }

            public static string UsersQuery(IEnumerable<FieldNames> fields, string usersWhereClause = null) =>
                $@" SELECT {string.Join(", ", fields)} FROM AspNetUsers {usersWhereClause} ";

            public static string AuthorsQuery(string usersWhereClause = null) =>
                UsersQuery(new[] 
                    {FieldNames.Id, FieldNames.FullName, FieldNames.UserName, FieldNames.PublicProfile},
                    usersWhereClause);

            public static string UserViewsQuery(string usersWhereClause = null) =>
                UsersQuery(new[]
                        {FieldNames.Id, FieldNames.FullName, FieldNames.UserName,
                         FieldNames.AvatarPath, FieldNames.PublicProfile, FieldNames.ShortInfo},
                    usersWhereClause);

        }

        public static class Posts
        {
            public enum FieldNames
            {
                AuthorId, PostId, PostDateTime, PostMessage,
                AllLikesCount, MyLikesCount,
                AuthorFullName, AuthorUserName, AuthorPublicProfile
            }

            public static string PostsWithAuthorsQuery(string authorsQuery, string postsWhereClause = null) =>
$@" SELECT P.AuthorId      AS {nameof(FieldNames.AuthorId)}, 
           P.Id            AS {nameof(FieldNames.PostId)}, 
           P.DateTime      AS {nameof(FieldNames.PostDateTime)}, 
           P.Message       AS {nameof(FieldNames.PostMessage)}, 
           A.FullName      AS {nameof(FieldNames.AuthorFullName)}, 
           A.UserName      AS {nameof(FieldNames.AuthorUserName)}, 
           A.PublicProfile AS {nameof(FieldNames.AuthorPublicProfile)}

FROM Posts P
JOIN ({authorsQuery}) A
ON P.AuthorId = A.Id 
{postsWhereClause} ";

            public static string PostViewsQuery(string viewerId, string postsWithAuthorsQuery) =>
$@" SELECT {nameof(FieldNames.AuthorId)}, 
           {nameof(FieldNames.PostId)}, 
           {nameof(FieldNames.PostDateTime)}, 
           {nameof(FieldNames.PostMessage)}, 

    (SELECT COUNT(*) FROM Likes 
        WHERE LikedUserId = {nameof(FieldNames.AuthorId)} AND 
              LikedPostId = {nameof(FieldNames.PostId)}) 
    AS     {nameof(FieldNames.AllLikesCount)}, 

    (SELECT COUNT(*) FROM Likes 
        WHERE LikedUserId = {nameof(FieldNames.AuthorId)} AND 
              LikedPostId = {nameof(FieldNames.PostId)} AND 
              LikerId = '{viewerId}') 
    AS     {nameof(FieldNames.MyLikesCount)}, 

           {nameof(FieldNames.AuthorFullName)}, 
           {nameof(FieldNames.AuthorUserName)}, 
           {nameof(FieldNames.AuthorPublicProfile)}

FROM ({postsWithAuthorsQuery}) _PostsWithAuthorInfo";

        }

    }
}
