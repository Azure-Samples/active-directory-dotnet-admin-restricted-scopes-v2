using System.Web.Mvc;

namespace GroupManager.Controllers
{
	public class ErrorController : Controller
	{
		// GET: Error
		public ActionResult Index(string message)
		{
			ViewBag.Message = message;
			return View("Error");
		}
	}
}