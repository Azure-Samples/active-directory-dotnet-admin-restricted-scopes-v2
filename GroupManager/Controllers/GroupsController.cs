using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GroupManager.Models;
using GroupManager.Utils;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GroupManager.Controllers
{
    public class GroupsController : Controller
    {
        // For simplicity, this sample uses an in-memory data store instead of a db.
        private ConcurrentDictionary<string, List<Group>> groupList = new ConcurrentDictionary<string, List<Group>>();

        [Authorize]
        // GET: Group
        public async Task<ActionResult> Index()
        {
            string tenantId = ClaimsPrincipal.Current.FindFirst(Globals.TenantIdClaimType).Value;
            string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

            try
            {
                // Try to get a token for our basic set of scopes
                string token = await GetGraphAccessToken(userId, new string[] { "user.readbasic.all" });
            }
            catch (MsalException ex)
            {
                if (ex.ErrorCode == "failed_to_acquire_token_silently")
                {
                    // If basic token acquisition failed, the user has either revoked our basic permissions
                    // or their tokens have expired.  We need to ask them to sign-in again.
                    return new RedirectResult("/Account/AADTenantConnected");
                }

                return new RedirectResult("/Error?message=" + ex.Message);
            }

            try
            {
                // Get a token for our admin-restricted set of scopes Microsoft Graph
                string token = await GetGraphAccessToken(userId, new string[] { "group.read.all" });

                // Construct the groups query
                HttpClient client = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Globals.MicrosoftGraphGroupsApi);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                // Ensure a successful response
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Populate the data store with the first page of groups
                string json = await response.Content.ReadAsStringAsync();
                GroupResponse result = JsonConvert.DeserializeObject<GroupResponse>(json);
                groupList[tenantId] = result.value;
            }
            catch (MsalUiRequiredException)
            {
                // If we got a token for the basic scopes, but not the admin-restricted scopes, 
                // then we need to ask the admin to grant permissions by by connecting their tenant.
                return new RedirectResult("/Account/PermissionsRequired");
            }
            // Handle unexpected errors.
            catch (Exception ex)
            {
                return new RedirectResult("/Error?message=" + ex.Message);
            }

            ViewBag.TenantId = tenantId;
            return View(groupList[tenantId]);
        }
        
        // Use MSAL to get a the token we need for the Microsoft Graph
        private async Task<string> GetGraphAccessToken (string userId, string[] scopes)
        {
            TokenCache userTokenCache = new MsalSessionTokenCache(userId, HttpContext).GetMsalCacheInstance();
            ConfidentialClientApplication cc = new ConfidentialClientApplication(Globals.ClientId, Globals.RedirectUri, new ClientCredential(Globals.ClientSecret), userTokenCache, null);
            AuthenticationResult result = await cc.AcquireTokenSilentAsync(scopes, cc.Users.First());
            return result.AccessToken;
        }
    }
}