using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(GroupManager.Startup))]

namespace GroupManager
{
	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			ConfigureAuth(app);
		}
	}
}