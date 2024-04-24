using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace BestStoreApi.Middelwares
{
    public class StatsMiddelware
    {
        private readonly RequestDelegate _next;
        public StatsMiddelware(RequestDelegate next)
        {
            this._next = next;

        }

        public Task Invoke(HttpContext httpContext)
        {
            // handel the request (befor executing the controller)
            DateTime requestTime = DateTime.Now;

            var result = _next(httpContext);

            // handel the request (befor executing the controller)
            DateTime responseTime = DateTime.Now;
            TimeSpan timeSpanDuration = responseTime - requestTime;

            Console.WriteLine("[class middleware] process Duration = " + timeSpanDuration.TotalMilliseconds + " ms");


            return result;
        }
    }
}