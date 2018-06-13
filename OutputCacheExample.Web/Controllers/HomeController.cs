using System;
using System.Web.Mvc;
using System.Web.UI;
using DevTrends.MvcDonutCaching;

namespace OutputCacheExample.Web.Controllers
{
    public class HomeController : Controller
    {
        [DonutOutputCache(Duration = 3600,
            Location = OutputCacheLocation.Server,
            VaryByParam = "*")]
        public ActionResult Index()
        {
            var obj = new
            {
                data = 123,
                rnd = new Random().Next()
            };
            return View(obj);
        }

        [DonutOutputCache(Duration = 3600,
            Location = OutputCacheLocation.Server,
            Order = 1,
            VaryByParam = "id")]
        public ActionResult About()
        {
            var obj = new
            {
                data = 123,
                rnd = new Random().Next()
            };
            return View(obj);
        }

        public JsonResult RemoveAdvert(int id)
        {
            var cacheManager = new OutputCacheManager();
            cacheManager.RemoveItem("AdvertDetail", "Detail", new {id, isMobile = "mobile" });
            cacheManager.RemoveItem("AdvertDetail", "Detail", new {id, isMobile = "web" });
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
    }
}