using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Asp.Mvc.Attributes;

namespace Asp.Mvc.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        [ETagCache("Get", "id")]
        public  ActionResult Get(int id)
        {
            var dir = Server.MapPath("/Images");
            var path = Path.Combine(dir, id + ".jpg");
            return File(path, "image/jpeg");
        }

        [OutputCache(Duration = 3600, VaryByParam = "id")]
        public ActionResult GetOutputCache(int id)
        {
            var dir = Server.MapPath("/Images");
            var path = Path.Combine(dir, id + ".jpg");
            return File(path, "image/jpeg");
        }

        public ActionResult RemoveCache(int id)
        {
            var url = Url.Action(
                "GetOutputCache",
                "Home",
                new {id = id}
                );
            HttpResponse.RemoveOutputCacheItem(url);
            return RedirectToAction("GetOutputCache", new { id = id });
        }
    }
}