using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PlatBlogs.Exceptions;

namespace PlatBlogs.Attributes
{
    public class OffsetExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (!context.ExceptionHandled && context.Exception is OffsetException)
            {
                context.ExceptionHandled = true;
                context.Result = new BadRequestResult();
            }
            else
            {
                base.OnException(context);
            }
        }
    }
}
