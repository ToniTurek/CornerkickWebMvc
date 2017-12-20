using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Controllers
{
  public class AdminController : Controller
  {
    // GET: Admin
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult Settings()
    {
      return View();
    }

    public ActionResult StartCalender(Models.AdminModel modelAdmin)
    {
      MvcApplication.timerCkCalender.Interval = modelAdmin.fCalenderInterval;
      MvcApplication.timerCkCalender.Enabled = !MvcApplication.timerCkCalender.Enabled;
      return View("Settings", "");
    }
  }
}