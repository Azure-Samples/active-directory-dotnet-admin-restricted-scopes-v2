using System.Web.Mvc;

namespace GroupManager.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}