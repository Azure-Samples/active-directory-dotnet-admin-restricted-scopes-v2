using GroupManager.Models;
using GroupManager.Utils;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

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

			try
			{
				// Get a token for our admin-restricted set of scopes Microsoft Graph
				string token = await GetGraphAccessToken(new string[] { "group.read.all" });

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
			catch (MsalUiRequiredException ex)
			{
				if (ex.ErrorCode == "user_null")
				{
					/*
					  If the tokens have expired or become invalid for any reason, ask the user to sign in again.
					  Another cause of this exception is when you restart the app using InMemory cache.
					  It will get wiped out while the user will be authenticated still because of their cookies, requiring the TokenCache to be initialized again
					  through the sign in flow.
					*/
					return new RedirectResult("/Account/SignIn/?redirectUrl=/Groups");
				}
				else if (ex.ErrorCode == "invalid_grant")
				{
					// If we got a token for the basic scopes, but not the admin-restricted scopes,
					// then we need to ask the admin to grant permissions by by connecting their tenant.
					return new RedirectResult("/Account/PermissionsRequired");
				}
				else
					return new RedirectResult("/Error?message=" + ex.Message);
			}
			// Handle unexpected errors.
			catch (Exception ex)
			{
				return new RedirectResult("/Error?message=" + ex.Message);
			}

			ViewBag.TenantId = tenantId;
			return View(groupList[tenantId]);
		}

		/// <summary>
		/// We obtain access token for Microsoft Graph with the scope "group.read.all". Since this access token was not obtained during the initial sign in process 
		/// (OnAuthorizationCodeReceived), the user will be prompted to consent again.
		/// </summary>
		/// <returns></returns>
		private async Task<string> GetGraphAccessToken(string[] scopes)
		{
			IConfidentialClientApplication cc = MsalAppBuilder.BuildConfidentialClientApplication();
			IAccount userAccount = await cc.GetAccountAsync(ClaimsPrincipal.Current.GetMsalAccountId());

			AuthenticationResult result = await cc.AcquireTokenSilent(scopes, userAccount).ExecuteAsync();
			return result.AccessToken;
		}
	}
}