using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BestStoreApi.Services
{
    public class TimeService
    {
        public string GetDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        public string GetTime()
        {
            return DateTime.Now.ToString("hh:mm:ss tt");
        }
    }
}