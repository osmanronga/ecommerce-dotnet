using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BestStoreApi.Services
{
    public class JwtReader
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            // User its type claims principal so we can get it from the class

            // var identity = User.Identity as ClaimsIdentity;
            var identity = user.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return 0;
            }

            var claim = identity.Claims.FirstOrDefault(c => c.Type.ToLower() == "id");
            if (claim == null)
            {
                return 0;
            }
            int userId;
            try
            {
                userId = int.Parse(claim.Value);
            }
            catch (System.Exception)
            {
                return 0;
            }

            return userId;
        }

        public static string GetUserRole(ClaimsPrincipal user)
        {
            // User its type claims principal so we can get it from the class

            // var identity = User.Identity as ClaimsIdentity;
            var identity = user.Identity as ClaimsIdentity;

            if (identity == null)
            {
                return "";
            }

            var claim = identity.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("role"));
            if (claim == null)
            {
                return "";
            }

            return claim.Value;
        }

        public static Dictionary<string, string> GetUserClaims(ClaimsPrincipal user)
        {
            Dictionary<string, string> claims = new Dictionary<string, string>();

            var identity = user.Identity as ClaimsIdentity;

            if (identity != null)
            {
                foreach (var claim in identity.Claims)
                {
                    claims.Add(claim.Type, claim.Value);
                }
            }

            return claims;
        }
    }
}