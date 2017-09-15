using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters
{
    public class DepressedFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            context.Result = new JsonResult(new {msg = "Not used for now , maybe used some time later." }) { StatusCode=403}; 
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            context.Result = await Task.FromResult(new JsonResult(new {msg = "Not used for now,maybe used some time later." }) { StatusCode = 403 }) ;
        }
    }
}
