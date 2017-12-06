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

            public static string FetchWithOffsetBlock(int offset, int count) =>
$@" OFFSET     {offset}    ROWS 
    FETCH NEXT {count}     ROWS ONLY ";

            public static string FetchWithOffsetWithReserveBlock(int offset, int count) =>
$@" OFFSET     {offset}    ROWS 
    FETCH NEXT {count + 1} ROWS ONLY ";

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

            public static string OpenedUsersFilterWhereClause(string viewerId) =>
$@" WHERE {nameof(UserBasicInfo.FieldNames.PublicProfile)} = 1 OR 
          {nameof(UserBasicInfo.FieldNames.Id)} = '{viewerId}' OR 
          {nameof(UserBasicInfo.FieldNames.Id)} IN 
              ({Followers.UserFollowersIdsQuery(viewerId)}) ";

            public static string FollowedUsersFilterWhereClause(string viewerId) =>
$@" WHERE {nameof(UserBasicInfo.FieldNames.Id)} IN 
              (SELECT FollowedId FROM Followers WHERE FollowerId = '{viewerId}') AND 
          ({nameof(UserBasicInfo.FieldNames.PublicProfile)} = 1 OR 
              {nameof(UserBasicInfo.FieldNames.Id)} IN 
                  ({Followers.UserFollowersIdsQuery(viewerId)}) 
          ) ";

        }

        public static class UserBasicInfo
        {
            public enum FieldNames { Id, FullName, UserName, PublicProfile, }


            public static string UsersBasicInfoQuery(string userFilterwhereClause) =>
$@" SELECT Id            AS {nameof(FieldNames.Id)}, 
           FullName      AS {nameof(FieldNames.FullName)}, 
           UserName      AS {nameof(FieldNames.UserName)}, 
           PublicProfile AS {nameof(FieldNames.PublicProfile)} 

FROM AspNetUsers 
{userFilterwhereClause} ";

        }

        public static class PostView
        {
            public enum FieldNames
            {
                AuthorId, PostId, PostDateTime, PostMessage,
                AllLikesCount, MyLikesCount,
                AuthorFullName, AuthorUserName, AuthorPublicProfile
            }

            private static string AvailablePostsWithAuthorInfoQuery(string authorsBasicInfoQuery) =>
$@" SELECT P.AuthorId      AS {nameof(FieldNames.AuthorId)}, 
           P.Id            AS {nameof(FieldNames.PostId)}, 
           P.DateTime      AS {nameof(FieldNames.PostDateTime)}, 
           P.Message       AS {nameof(FieldNames.PostMessage)}, 
           A.FullName      AS {nameof(FieldNames.AuthorFullName)}, 
           A.UserName      AS {nameof(FieldNames.AuthorUserName)}, 
           A.PublicProfile AS {nameof(FieldNames.AuthorPublicProfile)}

FROM Posts P
JOIN ({authorsBasicInfoQuery}) A
ON P.AuthorId = A.Id ";

            public static string AvailablePostViewInfosQuery(string viewerId, string authorsBasicInfoQuery) =>
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

FROM ({AvailablePostsWithAuthorInfoQuery(authorsBasicInfoQuery)}) _PostsWithAuthorInfo";

        }

    }
}
