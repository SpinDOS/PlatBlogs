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
$@" OFFSET {offset} ROWS 
FETCH NEXT {count} ROWS ONLY ";

            public static string FetchWithOffsetWithReserveBlock(int offset, int count) =>
$@" OFFSET {offset} ROWS 
FETCH NEXT {count + 1} ROWS ONLY ";

        }
        public static class Followers
        {

            public static string UserFollowersIdsQuery(string userId) =>
                $" SELECT FollowerId FROM Followers WHERE FollowedId = '{userId}' ";

        }

        public static class WhereClause
        {

            public static string OpenedUsersFilterWhereClause(string viewerId) =>
                $" PublicProfile = 1 OR Id = '{viewerId}' OR Id IN ({Followers.UserFollowersIdsQuery(viewerId)}) ";

            public static string FollowedUsersFilterWhereClause(string viewerId) =>
$@" Id IN (SELECT FollowedId FROM Followers WHERE FollowerId = '{viewerId}') AND 
    (PublicProfile = 1 OR Id IN ({Followers.UserFollowersIdsQuery(viewerId)})) ";

        }

        public static class UserBasicInfo
        {

            public static string UsersBasicInfoQuery(string userFilterwhereClause) =>
$@" SELECT Id, FullName, UserName, PublicProfile 
FROM AspNetUsers 
WHERE ({userFilterwhereClause}) ";

        }

        public static class PostView
        {

            private static string AvailablePostsWithAuthorInfoQuery(string authorsBasicInfoQuery) =>
$@" SELECT P.AuthorId, P.Id AS PostId, 
    P.DateTime As PostDateTime, P.Message AS PostMessage, 
    A.FullName AS AuthorFullName, A.UserName AS AuthorUserName, 
    A.PublicProfile AS AuthorPublicProfile
FROM Posts P
JOIN ({authorsBasicInfoQuery}) A
ON P.AuthorId = A.Id ";

            public static string AvailablePostViewInfosQuery(string viewerId, string authorsBasicInfoQuery) =>
$@" SELECT AuthorId, PostId, PostDateTime, PostMessage, 
    (SELECT COUNT(*) FROM Likes 
        WHERE LikedUserId = AuthorId AND LikedPostId = PostId) AS AllLikesCount, 
    (SELECT COUNT(*) FROM Likes 
        WHERE LikedUserId = AuthorId AND LikedPostId = PostId AND LikerId = '{viewerId}') AS MyLikesCount, 
    AuthorFullName, AuthorUserName, AuthorPublicProfile
FROM ({AvailablePostsWithAuthorInfoQuery(authorsBasicInfoQuery)}) PostsWithAuthorInfo";

        }

    }
}
