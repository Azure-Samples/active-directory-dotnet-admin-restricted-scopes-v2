using GroupManager.Utils;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace GroupManager.Controllers
{
	public class AccountController : Controller
	{
		public void SignIn(string redirectUrl)
		{
			redirectUrl = redirectUrl ?? "/Users";
			// Trigger a sign in using a basic set of scopes
			HttpContext.GetOwinContext().Authentication.Challenge(new AuthenticationProperties { RedirectUri = redirectUrl }, OpenIdConnectAuthenticationDefaults.AuthenticationType);
		}

		public async Task SignOut()
		{
			await MsalAppBuilder.ClearUserTokenCache();
			HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
			Response.Redirect("/");
		}

		[Authorize]
		public ActionResult ConnectAADTenant()
		{
			// Redirect the admin to grant your app permissions
			string tenantId = ClaimsPrincipal.Current.FindFirst(Globals.TenantIdClaimType).Value;
			string url = String.Format(Globals.AdminConsentFormat, tenantId, Globals.ClientId, "state_code", Globals.RedirectUri + "Account/AADTenantConnected");
			return new RedirectResult(url);
		}

		// When the admin completes granting the permissions, they will be redirected here.
		[Authorize]
		public void AADTenantConnected(string state, string tenant, string admin_consent, string error, string error_description)
		{
			if (error != null)
			{
				// If the admin did not grant permissions, ask them to do so again
				TempData["Error"] = error_description;
				Response.Redirect("/Account/PermissionsRequired");
				return;
			}

			// Note: Here the state parameter will contain whatever you passed in the outgoing request. You can
			// use this state to encode any information that you wish to track during execution of this request.

			Response.Redirect("/Account/SignOut");
		}

		public ActionResult PermissionsRequired()
		{
			ViewBag.Error = TempData["Error"];
			return View();
		}

		/// <summary>
		/// Called by Azure AD. Here we end the user's session, but don't redirect to AAD for sign out.
		/// </summary>
		public async Task EndSession()
		{
			await MsalAppBuilder.ClearUserTokenCache();
			HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
		}
	}
}