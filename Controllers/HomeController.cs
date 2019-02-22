﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace CornerkickWebMvc.Controllers
{
  public class HomeController : Controller
  {
    public ActionResult Index()
    {
      return View();
    }

    public ActionResult About()
    {
      ViewBag.Message = "Your application description page.";

      return View();
    }

    public ActionResult Contact()
    {
      ViewBag.Message = "Your contact page.";

      return View();
    }

    public ActionResult UserManual()
    {
      ViewBag.Message = "Cornerkick User Manual";

      return View();
    }

    public ContentResult UmGetStadiumCost()
    {
      CornerkickCore.Core.Club club = AccountController.ckClub();

      CornerkickCore.Core.User user = MvcApplication.ckcore.ini.newUser();
      user.sNachname = "manual";
      user.iLevel = 1;

      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[3];
      dataPoints[0] = new List<Models.DataPointGeneral>();
      dataPoints[1] = new List<Models.DataPointGeneral>();
      dataPoints[2] = new List<Models.DataPointGeneral>();

      int[] iCostDays;

      for (int iDp = 0; iDp < 3; iDp++) {
        CornerkickGame.Stadium stDatum = MvcApplication.ckcore.ini.newStadium();
        stDatum.blocks[0].iSeats = iDp * 1000;

        CornerkickGame.Stadium stNew = MvcApplication.ckcore.ini.newStadium();

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats +  500;
        iCostDays = MvcApplication.ckcore.st.iCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new Models.DataPointGeneral( 500, iCostDays[0]));

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats + 1000;
        iCostDays = MvcApplication.ckcore.st.iCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new Models.DataPointGeneral(1000, iCostDays[0]));

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats + 2000;
        iCostDays = MvcApplication.ckcore.st.iCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new Models.DataPointGeneral(2000, iCostDays[0]));
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }
  }
}