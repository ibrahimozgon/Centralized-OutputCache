using System;
using System.Web.Mvc;
using DevTrends.MvcDonutCaching;

namespace OutputCacheExample.Web.Controllers
{
    public class AdvertDetailController : Controller
    {
        [DonutOutputCache(
            Duration = 3600,
            VaryByParam = "id",
            VaryByCustom = "IsMobile")]
        [HttpGet]
        public ActionResult Detail(int? id)
        {
            var obj = new
            {
                data = 123,
                rnd = new Random().Next(),
                id
            };
            return View(obj);
        }
    }
}