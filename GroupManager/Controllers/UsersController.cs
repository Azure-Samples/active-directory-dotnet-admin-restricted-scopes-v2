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
	public class UsersController : Controller
	{
		// For simplicity, this sample uses an in-memory data store instead of a db.
		private ConcurrentDictionary<string, List<GroupManager.Models.User>> userList = new ConcurrentDictionary<string, List<GroupManager.Models.User>>();

		[Authorize]
		// GET: Users
		public async Task<ActionResult> Index()
		{
			string tenantId = ClaimsPrincipal.Current.FindFirst(Globals.TenantIdClaimType).Value;
			string userId = ClaimsPrincipal.Current.FindFirst(ClaimTypes.NameIdentifier).Value;

			try
			{
				// Get a token for the Microsoft Graph
				string token = await GetGraphAccessToken(userId);

				// Construct the query
				HttpClient client = new HttpClient();
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Globals.MicrosoftGraphUsersApi);
				request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

				// Ensure a successful response
				HttpResponseMessage response = await client.SendAsync(request);
				response.EnsureSuccessStatusCode();

				// Populate the data store with the first page of groups
				string json = await response.Content.ReadAsStringAsync();
				UserResponse result = JsonConvert.DeserializeObject<UserResponse>(json);
				userList[tenantId] = result.value;
			}
			catch (MsalUiRequiredException ex)
			{
				/*
                    If the tokens have expired or become invalid for any reason, ask the user to sign in again.
                    Another cause of this exception is when you restart the app using InMemory cache.
                    It will get wiped out while the user will be authenticated still because of their cookies, requiring the TokenCache to be initialized again
                    through the sign in flow.
                */
				return new RedirectResult("/Account/SignIn");
			}
			// Handle unexpected errors.
			catch (Exception ex)
			{
				return new RedirectResult("/Error?message=" + ex.Message);
			}

			ViewBag.TenantId = tenantId;
			return View(userList[tenantId]);
		}

		private async Task<string> GetGraphAccessToken(string userId)
		{
			IConfidentialClientApplication cc = MsalAppBuilder.BuildConfidentialClientApplication();
			var userAccount = await cc.GetAccountAsync(ClaimsPrincipal.Current.GetMsalAccountId());

			AuthenticationResult result = await cc.AcquireTokenSilent(new string[] { "user.readbasic.all" }, userAccount).ExecuteAsync();
			return result.AccessToken;
		}
	}
}