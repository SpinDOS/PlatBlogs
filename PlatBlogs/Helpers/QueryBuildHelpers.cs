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
            public static string UserFollowersIdsQuery(string userId) =>
$@" 
SELECT FollowerId 
FROM Followers 
WHERE FollowedId = '{userId}' 
";

        }

        public static class WhereClause
        {

            public static string OpenedUsersWhereClause(string viewerId, string U = "U") =>
$@" 
{U}.PublicProfile = 1 OR {U}.Id = '{viewerId}' OR 
    {U}.Id IN ({Followers.UserFollowersIdsQuery(viewerId)}) 
";

            public static string FollowedUsersWhereClause(string viewerId, string U = "U") =>
$@" 
{U}.Id IN (SELECT FollowedId FROM Followers WHERE FollowerId = '{viewerId}') AND 
    ({U}.PublicProfile = 1 OR {U}.Id IN ({Followers.UserFollowersIdsQuery(viewerId)}) ) 
";

        }

        public static class SelectFields
        {
            public static string UserView(string U = "U") =>
                $" {U}.Id, {U}.FullName, {U}.UserName, {U}.AvatarPath, {U}.PublicProfile, {U}.ShortInfo ";

            public static string Author(string U = "U") =>
                $" {U}.FullName, {U}.UserName, {U}.PublicProfile ";
            public static string Post(string P = "P") => 
                $" {P}.AuthorId, {P}.Id, {P}.DateTime, {P}.Message ";
            public static string PostView(string U = "U", string P = "P") =>
                $" {Post(P)}, AllLikesCount, MyLikesCount, {Author(U)} ";

        }

        public static class CrossApply
        {
            public static string AllLikesCount(string U = "U", string P = "P") => 
$@" 
CROSS APPLY (SELECT AllLikesCount = COUNT(*) 
             FROM Likes 
             WHERE LikedUserId = {U}.Id AND 
                   LikedPostId = {P}.Id) _AllLikesQuery 
";

            public static string MyLikesCount(string viewerId, string U = "U", string P = "P") =>
$@" 
CROSS APPLY (SELECT MyLikesCount = COUNT(*) 
             FROM Likes 
             WHERE LikedUserId = {U}.Id AND 
                   LikedPostId = {P}.Id AND 
                   LikerId = '{viewerId}') _MyLikesQuery 
";

            public static string LikesCounts(string viewerId, string U = "U", string P = "P") =>
                $"{AllLikesCount(U, P)} {MyLikesCount(viewerId, U, P)}";
        }
        
    }
}
