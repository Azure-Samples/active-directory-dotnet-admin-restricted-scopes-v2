---
services: active-directory
platforms: dotnet
author: jmprieur
level: 400 
client: ASP.NET Web App
service: Microsoft Graph 
endpoint: AAD V2 
---
![Build Badge](https://identitydivision.visualstudio.com/_apis/public/build/definitions/a7934fdd-dcde-4492-a406-7fad6ac00e17/51/badge)

# Build an app with admin restricted scopes using the v2.0 endpoint

## About this sample

### Overview

Certain actions in the Azure Active Directory tenant are considered highly sensitive, such as deleting a user from the tenant, creating and managing applications, listing and assigning users to security groups.  Yet there are many valid reasons why applications need to perform these actions for their customers.  For this reason, some permissions are considered [admin restricted](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent#permission-types), and require a tenant administrator to approve their use in applications.  
This sample application shows how to use the [Azure AD v2.0 endpoint](http://aka.ms/aadv2) to access data in the [Microsoft Graph](https://graph.microsoft.io) that requires [consent](https://docs.microsoft.com/en-us/azure/active-directory/develop/application-consent-experience) for permissions that have an administrative scope.

![](ReadmeFiles/Topology.png)

### Scenario

The app is built as an ASP.NET 4.5 MVC application, using the OWIN OpenID Connect middleware to sign-in users and uses the  [Microsoft Authentication Library (MSAL)](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet)] to perform token acquisition.  It uses an [incremental consent](https://docs.microsoft.com/en-us/azure/active-directory/develop/azure-ad-endpoint-comparison#incremental-and-dynamic-consent) pattern, in which it first requests consent for a basic set of permission that an ordinary user can consent to themselves; like the ability to read a list of users in the user's organization.
Then, when the user tries to read a list of groups in the user's organization, it will ask the administrator for the necessary admin restricted permission.  In this way, any Microsoft business user can sign up for the application without contacting their tenant administrator, and the tenant administrator is only involved when absolutely necessary.

![](ReadmeFiles/overview.png)

For more information on the concepts used in this sample, be sure to read the [v2.0 scope and permission reference](https://azure.microsoft.com/documentation/articles/active-directory-v2-scopes).

> Looking for previous versions of this code sample? Check out the tags on the [releases](../../releases) GitHub page.

## How to run this sample

To run this sample, you'll need:

- [Visual Studio 2017](https://aka.ms/vsdownload)
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant, or a Microsoft personal account. You need to have at least one account which is a directory administrator to test the features which require an administrator to consent.

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnet-admin-restricted-scopes-v2.git
```

or download and extract the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

### Step 2:  Register the sample with your Azure Active Directory tenant

#### Register the service app (restricted-scopes-v2)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `restricted-scopes-v2`.
   - Change **Supported account types** to **Accounts in any organizational directory and personal Microsoft accounts (e.g. Skype, Xbox, Outlook.com)**.
     > Note that there are more than one redirect URIs. You'll need to add them from the **Authentication** tab later after the app has been created successfully.
1. Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. In the list of pages for the app, select **Authentication**..
   - In the Redirect URIs section, select **Web** in the combo-box and enter the following redirect URIs.
       - `https://localhost:44321/`
       - `https://localhost:44321/Account/AADTenantConnected`
   - In the **Advanced settings** section set **Logout URL** to `https://localhost:44321/Account/EndSession`
   - In the **Advanced settings** | **Implicit grant** section, check **ID tokens** as this sample requires
     the [Implicit grant flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow) to be enabled to
     sign-in the user, and call an API.
1. Select **Save**.
1. From the **Certificates & secrets** page, in the **Client secrets** section, choose **New client secret**:

   - Type a key description (of instance `app secret`),
   - Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   - When you press the **Add** button, the key value will be displayed, copy, and save the value in a safe location.
   - You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means,
     so record it as soon as it is visible from the Azure portal.
1. In the list of pages for the app, select **API permissions**
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Delegated permissions** section, ensure that the right permissions are checked: **openid**, **email**, **profile**, **offline_access**, **User.Read**, **Group.Read.All**, **User.ReadBasic.All**. Use the search box if necessary.
   - Select the **Add permissions** button

1. Once you've run the script, be sure to follow the manual steps. Indeed Azure AD PowerShell does not create an app which audience is Work or School + personal accounts, even if this registration is already possible from the Azure portal:
     In the list of pages for the application registration of the application, select **Manifest**
     - search for **signInAudience** and make sure it's set to **AzureADandPersonalMicrosoftAccount**
     - Select **Save**

If you have an existing application that you have registered in the past, feel free to use that instead of creating a new registration.

### Step 3:  Configure the sample to use your Azure AD tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the  project

1. Open the `Utils\Globals.cs` file, and replace the following values:
1. Replace the `clientId` value with the application ID you copied above during App Registration.
1. Replace the `clientSecret` value with the application secret you copied above during App Registration.

### Step 4: Run the sample

Start the GroupManager application, and begin by signing in as an administrator in your Azure AD tenant.  If you don't have an Azure AD tenant for testing, you can [follow these instructions](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/) to get one.

When you sign in, the app will first ask you for permission to sign you in, read your user profile, and read a list of users in your tenant.  Any user in your tenant will be able to consent to these permissions.  The application will then show a list of users from your Azure AD tenant via the Microsoft Graph, on the **Users** page.

Then, navigate to the **Groups** page.  The app will try to query the Microsoft Graph for a list of groups in your tenant. If it is unable to do so, it will ask you (the tenant administrator) to connect your tenant to the application, providing permission to read groups in your tenant.  Only administrators in your tenant will be able to consent to this permission.  Once administrative consent is acquired, no other users in the tenant will be asked to consent to the app going forward.

![](ReadmeFiles/AdminConsentRequired.png)

## About the code

The relevant code for this sample is in the following files:

- Initial sign-in & basic permissions: `App_Start\Startup.Auth.cs` and `Controllers\AccountController.cs`. In particular, the actions on the controller have an Authorize attribute, which forces the user to sign-in. The application uses the [authorization code flow](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-with-authorization-codes-on-web-apps) to sign-in the user. When the token is received (See method `OnAuthorizationCodeReceived`) in [Startup.Auth.cs#L58-L65](https://github.com/Azure-Samples/active-directory-dotnet-admin-restricted-scopes-v2/blob/master/GroupManager/App_Start/Startup.Auth.cs#L58-L65),
the application gets the token, which MSAL.NET stores into the token cache (See the `Utils\MsalSessionTokenCache` class). Then, when the controllers need to access the graph, they get a token by calling their private method `GetGraphAccessToken`
[GetGraphAccessToken](https://github.com/Azure-Samples/active-directory-dotnet-admin-restricted-scopes-v2/blob/master/GroupManager/Controllers/UsersController.cs#L67-L73)

- Getting the list of users: `Controllers\UsersController.cs`

- Getting the list of groups: `Controllers\GroupsController.cs`

- Acquiring permissions from the tenant admin using the admin consent endpoint: `Controllers\AccountController.cs`

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`adal` `msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, see MSAL.NET's [conceptual documentation](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki):

- [Quickstart: Register an application with the Microsoft identity platform (Preview)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Consent framework](https://docs.microsoft.com/en-us/azure/active-directory/develop/consent-framework)
- [Permissions and consent in the Azure Active Directory v2.0 endpoint](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent)
- [Understanding Azure AD application consent experiences](https://docs.microsoft.com/en-us/azure/active-directory/develop/application-consent-experience)
- [Recommended pattern to acquire a token](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-Tokens)
- [Customizing Token cache serialization](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/token-cache-serialization)
- [How to get consent for several resources](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/Acquiring-tokens-interactively#how-to-get-consent-for-several-resources)
- [Comparing the Azure AD v2.0 endpoint with the v1.0 endpoint](https://docs.microsoft.com/en-us/azure/active-directory/develop/azure-ad-endpoint-comparison)
- [Problems with application consent](https://docs.microsoft.com/en-us/azure/active-directory/manage-apps/application-sign-in-problem-first-party-microsoft?%2F%3FWT.mc_id=AKA_MS_Apps_Troubleshooting_Link#problems-with-application-consent)

For more information about how OAuth 2.0 protocols work in this scenario and other scenarios, see [Authentication Scenarios for Azure AD](http://go.microsoft.com/fwlink/?LinkId=394414).

