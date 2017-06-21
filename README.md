---
services: active-directory
platforms: dotnet
author: jmprieur
---

# Build an app with admin restricted scopes using the v2.0 endpoint
Certain actions in the Microsoft ecosystem are considered highly sensitive, such as deleting a user from a company's tenant, changing a user's password, or reading a list of groups in a company.  Yet there are many valid reasons why applications need to perform these actions for their customers.  For this reason, some permissions are considered **admin restricted**, and require a tenant administrator to approve their use in applications.  This sample application shows how to use the [Azure AD v2.0 endpoint](http://aka.ms/aadv2) to access data in the [Microsoft Graph](https://graph.microsoft.io) that requires administrative consent.

The app is built as an ASP.NET 4.5 MVC application, using the OWIN OpenID Connect middleware to sign-in users and the preview Microsoft Authentication Library (MSAL) to perform token acquisition.  It uses an incremental consent pattern, in which it first requests a basic permission that an ordinary user can consent to; the ability to read a list of users in the user's organization.  Then, when the user tries to read a list of groups in the user's organization, it asks the administrator for the necessary admin restricted permission.  In this way, any Microsoft business user can sign up for the application without contacting their tenant administrator, and the tenant administrator is only involved when absolutely necessary.

For more information on the concepts used in this sample, be sure to read the [v2.0 scope and permission reference](https://azure.microsoft.com/documentation/articles/active-directory-v2-scopes).

> Looking for previous versions of this code sample? Check out the tags on the [releases](../../releases) GitHub page.

## Running the sample app
Follow the steps below to run the application and create your own multi-tenant web app.  We recommend using Visual Studio 2015 to do so.

### Register an app
Create a new app at [apps.dev.microsoft.com](https://apps.dev.microsoft.com), or follow these [detailed steps](active-directory-v2-app-registration.md).  Make sure to:

- Copy down the **Application Id** assigned to your app, you'll need it soon.
- Add the **Web** platform for your app.
- Enter two **Redirect URI**s. The base URL for this sample, `https://localhost:44321/`, as well as `https://localhost:44321/Account/AADTenantConnected`.  These are the locations which the v2.0 endpoint will be allowed to return to after authentication.
- Generate an **Application Secret** of the type **password**, and copy it for later.  Note that in production apps you should always use certificates as your application secrets, but for this sample we will use a simple shared secret password.

If you have an existing application that you have registered in the past, feel free to use that instead of creating a new registration.

### Configure your app for admin consent
In order to request admin restricted permissions, you'll need to declare the permissions your app will use ahead of time.  While still in the registration portal,

- Locate the **Microsoft Graph Permissions** section on your app registration.
- Under **Delegated Permissions**, add the following permissions: `Group.Read.All`, `User.ReadBasic.All`, `openid`, `email`, `profile`, and `offline_access`.
- Be sure to **Save** your app registration.

> If you have issues saving the `openid`, `email`, `profile`, or `offline_access` scopes, you can continue with the rest of the sample.  This is a known issue that will be fixed shortly.

### Download & configure the sample code
You can download this repo as a .zip file using the button above, or run the following command:

`git clone https://github.com/Azure-Samples/active-directory-dotnet-admin-restricted-scopes-v2.git`

Once you've downloaded the sample, open it using Visual Studio.  Open the `Utils\Globals.cs` file, and replace the following values:

- Replace the `clientId` value with the application ID you copied above.
- Replace the `clientSecret` value with the application secret you copied above.

### Run the sample
Start the GroupManager application, and begin by signing in as an administrator in your Azure AD tenant.  If you don't have an Azure AD tenant for testing, you can [follow these instructions](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/) to get one.

When you sign in, the app will first ask you for permission to sign you in, read your user profile, and read a list of users in your tenant.  Any user in your tenant will be able to consent to these permissions.  The application will then show a list of users from your Azure AD tenant via the Microsoft Graph, on the **Users** page.

Then, navigate to the **Groups** page.  The app will try to query the Microsoft Graph for a list of groups in your tenant. If it is unable to do so, it will ask you (the tenant administrator) to connect your tenant to the application, providing permission to read groups in your tenant.  Only administrators in your tenant will be able to consent to this permission.  Once administrative consent is acquired, no other users in the tenant will be asked to consent to the app going forward.

The relevant code for this sample is in the following files:

- Initial sign-in & basic permissions: `App_Start\Startup.Auth.cs`, `Controllers\AccountController.cs`
- Getting the list of users: `Controllers\UsersController.cs`
- Getting the list of groups: `Controllers\GroupsController.cs`
- Acquiring permissions from the tenant admin using the admin consent endpoint: `Controllers\AccountController.cs`
