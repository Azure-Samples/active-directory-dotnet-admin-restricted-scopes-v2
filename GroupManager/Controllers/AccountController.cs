using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using GroupManager.Utils;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

namespace GroupManager.Controllers
{
    public class AccountController : Controller
    {
        public void SignIn()
        {
            // Trigger a sign in using a basic set of scopes
            HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = "/Users" }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
        }

        public void SignOut()
        {
            HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            Response.Redirect("/");
        }

        [Authorize]
        public ActionResult ConnectAADTenant()
        {
            // Redirect the admin to grant your app permissions
            string tenantId = ClaimsPrincipal.Current.FindFirst(Globals.TenantIdClaimType).Value;
            string url = String.Format(Globals.AdminConsentFormat, tenantId, Globals.ClientId, "whatever_you_want", Globals.RedirectUri + "Account/AADTenantConnected");
            return new RedirectResult(url);
        }

        // When the admin completes granting the permissions, they will be redirected here.
        [Authorize]
        public void AADTenantConnected(string state, string tenant, string admin_consent, string error, string error_description)
        {
            if (error != null)
            {
                // If the admin did not grant permissions, ask them to do so again
                Response.Redirect("/Account/PermissionsRequired?error=" + error_description);
                return;
            }

            // Note: Here the state parameter will contain whatever you passed in the outgoing request. You can
            // use this state to encode any information that you wish to track during execution of this request.

            Response.Redirect("/Groups");
        }

        public ActionResult PermissionsRequired(string error)
        {
            ViewBag.Error = error;
            return View();
        }
    }
}