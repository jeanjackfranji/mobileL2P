using System;
using Microsoft.AspNet.Http;
using System.Linq;
using L2PAPIClient;
using System.Threading.Tasks;

namespace Grp.L2PSite.MobileApp.Helpers
{
    public static class Tools
    {
        public static bool loggedIn = false;
        public static String cId = null;
        public static String pWd = "Integr$atorPa%ss123!@#$5";
        public static Boolean hasCookieToken = false;

        public static void checkIfTokenCookieExists(IReadableStringCollection cookies){
            var cookiesList = from cookieToken in cookies
                         where cookieToken.Key == "CRTID" || cookieToken.Key == "CRAID"
                         select cookieToken;
            if (cookiesList.Any())
            {
                foreach(var cookie in cookiesList)
                {
                    if(cookie.Key == "CRTID")
                        Config.setRefreshToken(cookie.Value.First());
                    else
                        Config.setAccessToken(cookie.Value.First());
                    
                }
                if(cookiesList.Count() == 2)
                    hasCookieToken = true;
            }
        }

        public static async void activateL2PClient()
        {
            if (!isL2PAPIClientActive() && Tools.hasCookieToken && !String.IsNullOrEmpty(Config.getRefreshToken()))
            {
                await AuthenticationManager.CheckStateAsync();
                await AuthenticationManager.CheckAccessTokenAsync();
            }
        }

        public static Boolean isL2PAPIClientActive()
        {
            return AuthenticationManager.getState() == AuthenticationManager.AuthenticationState.ACTIVE;
        }
    }
}
