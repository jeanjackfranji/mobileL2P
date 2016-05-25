using System;
using System.Collections.Generic;
using System.Text;

namespace L2PAPIClient
{
    class Config
    {
	internal const string OAuthEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/code";

        internal const string OAuthTokenEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/token";

        internal const string ClientID = "TAgzsUUaRiAqRQAR8BSSvxmfPCITYeiSfAVuX8GDQIQHbKiFFWYkLQhEEBTwbS8q.apps.rwth-aachen.de";

        internal const string L2PEndPoint = "https://www3.elearning.rwth-aachen.de/_vti_bin/l2pservices/api.svc/v1";

        internal const string OAuthTokenInfoEndPoint = "https://oauth.campus.rwth-aachen.de/oauth2waitress/oauth2.svc/tokeninfo";

        #region Token Management (Add Storage Option in here!)

        private static string accessToken = "36bZwO830oeZNFhswbxxCIQlrH6djhJWdJyCsSPt1vBzyI2jJU6LncTP466bjMfc";

        internal static string getAccessToken()
        {
            return accessToken;
        }

        internal static void setAccessToken(string token)
        {
            accessToken = token;
        }

        private static string refreshToken = "RhFQDmBse3wN6cfMPgLYOQZW9TczOdE0FBPFjC1XwQsSaTbRgJ75XzX7DYRpQ1Ve";

        internal static string getRefreshToken()
        {
            return refreshToken;
        }

        internal static void setRefreshToken(string token)
        {
            refreshToken = token;
        }


        private static string deviceToken = "";

        internal static string getDeviceToken()
        {
            return deviceToken;
        }

        internal static void setDeviceToken(string token)
        {
            deviceToken = token;
        }


        #endregion
    }
}
