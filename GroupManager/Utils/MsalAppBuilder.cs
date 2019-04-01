using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Identity.Client.AppConfig;

namespace GroupManager.Utils
{
    public static class MsalAppBuilder
    {
        public static IConfidentialClientApplication BuildConfidentialClientApplication(HttpContextBase httpContext)
        {
            IConfidentialClientApplication clientapp = ConfidentialClientApplicationBuilder.Create(Globals.ClientId)
                  .WithClientSecret(Globals.ClientSecret)
                  .WithRedirectUri(Globals.RedirectUri)
                  .WithAuthority(new Uri(Globals.Authority))
                  .Build();

            MSALPerUserSessionTokenCache userTokenCache = new MSALPerUserSessionTokenCache(clientapp.UserTokenCache, httpContext);
            MSALAppSessionTokenCache appTokenCache = new MSALAppSessionTokenCache(clientapp.AppTokenCache, Globals.ClientId, httpContext);
            return clientapp;
        }

        public static void ClearTokenCaches(HttpContextBase httpContext)
        {
            IConfidentialClientApplication clientapp = ConfidentialClientApplicationBuilder.Create(Globals.ClientId)
                  .WithClientSecret(Globals.ClientSecret)
                  .WithRedirectUri(Globals.RedirectUri)
                  .WithAuthority(new Uri(Globals.Authority))
                  .Build();

            MSALPerUserSessionTokenCache userTokenCache = new MSALPerUserSessionTokenCache(clientapp.UserTokenCache, httpContext);
            userTokenCache.Clear();
            MSALAppSessionTokenCache appTokenCache = new MSALAppSessionTokenCache(clientapp.AppTokenCache, Globals.ClientId, httpContext);
            appTokenCache.Clear();
        }
    }
}