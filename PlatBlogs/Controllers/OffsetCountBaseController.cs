using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatBlogs.Attributes;
using PlatBlogs.Extensions;
using PlatBlogs.Helpers;
using PlatBlogs.Interfaces;
using PlatBlogs.Views._Partials;

namespace PlatBlogs.Controllers
{
    [Authorize]
    [OffsetExceptionFilter]
    public abstract class OffsetCountBaseController : Controller
    {
        public delegate Task<ListWithLoadMoreModel> ItemsLoaderDelegate(string userId, 
            int offset, int count, IAuthor fullUserInfo = null);

        protected OffsetCountBaseController(DbConnection dbConnection) { DbConnection = dbConnection; }
        public DbConnection DbConnection { get; set; }


        protected async Task<IActionResult> Get(string userName, ItemsLoaderDelegate itemsLoader, int offset, int count, 
            Func<IUser, string> itemsDefaultText, Func<IUser, string> title, Func<IUser, string> main)
        {
            int sum = OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);

            var userLeftMenuModel = await UserLeftMenuModel.FromDatabase(DbConnection, userName, User);
            if (userLeftMenuModel == null)
                return NotFound();

            var items = await itemsLoader(userLeftMenuModel.Id, 0, sum, userLeftMenuModel);
            if (items.DefaultText == null)
                items.DefaultText = itemsDefaultText(userLeftMenuModel);

            ViewData["User"] = userLeftMenuModel;
            ViewData["Title"] = title(userLeftMenuModel);
            ViewData["Main"] = main(userLeftMenuModel);

            return View("~/Views/SimpleListWithLoadMoreView.cshtml", items);
        }

        protected async Task<IActionResult> Post(string userName, ItemsLoaderDelegate itemsLoader, int offset, int count)
        {
            OffsetCountResolver.ResolveOffsetCountWithReserve(offset, ref count);
            var userId = await DbConnection.GetUserIdByNameAsync(userName);

            var items = await itemsLoader(userId, offset, count);
            if (items == null)
                return NotFound();
            return PartialView("~/Views/_Partials/ListWithLoadMore.cshtml", items);
        }
        
    }
}