using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GroupManager.Utils
{
    public static class Globals
    {
        public const string ConsumerTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";
        public const string IssuerClaim = "iss";
        public const string Authority = "https://login.microsoftonline.com/common/v2.0/";
        public const string RedirectUri = "https://localhost:44321/";
        public const string TenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string MicrosoftGraphGroupsApi = "https://graph.microsoft.com/v1.0/groups";
        public const string MicrosoftGraphUsersApi = "https://graph.microsoft.com/v1.0/users";
        public const string AdminConsentFormat = "https://login.microsoftonline.com/{0}/adminconsent?client_id={1}&state={2}&redirect_uri={3}";
        public const string BasicSignInScopes = "openid profile email offline_access user.readbasic.all";
        public const string NameClaimType = "name";

        // WARNING! You really shouldn't store important security artifacts in code like this.
        public const string ClientSecret = "asdsadssa";
        public const string ClientId = "sdsadsadsa";
    }
}