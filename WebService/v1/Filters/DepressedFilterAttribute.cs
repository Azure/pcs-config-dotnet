// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters
{
    public class DepressedFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.Result = new JsonResult(new { msg = "Not used for now , maybe used some time later." }) { StatusCode = 403 };
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.Result = await Task.FromResult(new JsonResult(new { msg = "Not used for now,maybe used some time later." }) { StatusCode = 403 });
        }
    }
}
