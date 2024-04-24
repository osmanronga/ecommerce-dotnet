using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BestStoreApi.Filters
{
    public class StatFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            DateTime refrenceDate = new DateTime(2022, 1, 1);
            TimeSpan timeSpan = DateTime.Now - refrenceDate;
            Console.WriteLine(" [StatFilter] OnActionExecuting time = " + timeSpan.TotalMilliseconds + "ms");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            DateTime refrenceDate = new DateTime(2022, 1, 1);
            TimeSpan timeSpan = DateTime.Now - refrenceDate;
            Console.WriteLine(" [StatFilter] OnActionExecuted time = " + timeSpan.TotalMilliseconds + "ms");
        }
    }
}