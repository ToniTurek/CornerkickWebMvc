using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
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

    public ActionResult UserManual(Models.UserManualModel mdUm)
    {
      ViewBag.Message = "Cornerkick User Manual";

      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, MvcApplication.iNations[0], 0);
      mdUm.sAfLeague = league.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(2, MvcApplication.iNations[0]);
      mdUm.sAfCup = cup.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cupGold = MvcApplication.ckcore.tl.getCup(3);
      mdUm.sAfCupGold = cupGold.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cupSilver = MvcApplication.ckcore.tl.getCup(4);
      mdUm.sAfCupSilver = cupSilver.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cupWc = MvcApplication.ckcore.tl.getCup(7);
      mdUm.sAfWc = cupWc.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup tg = MvcApplication.ckcore.tl.getCup(5);
      mdUm.sAfTg = tg.settings.fAttraction.ToString("0.00");

      return View(mdUm);
    }

    private CornerkickManager.User ckUser()
    {
      string sUserId = User.Identity.GetUserId();
      foreach (CornerkickManager.User usr in MvcApplication.ckcore.ltUser) {
        if (usr.id.Equals(sUserId)) return usr;
      }

      return null;
    }

    private CornerkickManager.Club ckClub()
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return null;

      return usr.club;
    }

    public ContentResult UmGetStadiumCost()
    {
      CornerkickManager.Club club = ckClub();

      CornerkickManager.User user = new CornerkickManager.User();
      user.sSurname = "manual";
      user.iLevel = 1;

      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[3];
      dataPoints[0] = new List<Models.DataPointGeneral>();
      dataPoints[1] = new List<Models.DataPointGeneral>();
      dataPoints[2] = new List<Models.DataPointGeneral>();

      int[] iCostDays;

      for (int iDp = 0; iDp < 3; iDp++) {
        CornerkickGame.Stadium stDatum = new CornerkickGame.Stadium();
        stDatum.blocks[0].iSeats = iDp * 1000;

        CornerkickGame.Stadium stNew = new CornerkickGame.Stadium();

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats +  500;
        iCostDays = MvcApplication.ckcore.st.getCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new Models.DataPointGeneral( 500, iCostDays[0]));

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats + 1000;
        iCostDays = MvcApplication.ckcore.st.getCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new Models.DataPointGeneral(1000, iCostDays[0]));

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats + 2000;
        iCostDays = MvcApplication.ckcore.st.getCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new Models.DataPointGeneral(2000, iCostDays[0]));
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public ContentResult UmGetPlayerTraining(int iType, int iTrainerCondi, int iTrainerPhysio, int iCamp, int iDoping, int iAge)
    {
      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[3];

      // Initialize dataPoints list
      for (byte j = 0; j < dataPoints.Length; j++) dataPoints[j] = new List<Models.DataPointGeneral>();

      // Create temp. ck manager instance
      CornerkickManager.Main mnUm = new CornerkickManager.Main();
      mnUm.dtDatum = mnUm.dtDatum.AddDays(1);

      // Create user
      CornerkickManager.User usr = new CornerkickManager.User();
      mnUm.ltUser.Add(usr);

      CornerkickManager.Main.Training training = new CornerkickManager.Main.Training();
      for (byte iT = 0; iT < training.iType.Length; iT++) training.iType[iT] = (byte)iType; // Condition

      // Create player
      CornerkickGame.Player pl = new CornerkickGame.Player();
      pl.fCondition = 0.6f;
      pl.fFresh = 0.8f;
      pl.fMoral = 1.0f;
      pl.iClubId = 0;
      pl.dtBirthday = mnUm.dtDatum.AddYears(-iAge);

      // Trainings camp
      CornerkickManager.TrainingCamp.Booking camp = new CornerkickManager.TrainingCamp.Booking();
      if (iCamp >= 0 && iCamp < mnUm.tcp.ltCamps.Count) {
        camp.camp = mnUm.tcp.ltCamps[iCamp];
        camp.dtArrival   = mnUm.dtDatum.AddDays(-1);
        camp.dtDeparture = mnUm.dtDatum.AddDays(+8);
      }

      // Doping
      if (iDoping >= 0 && iDoping < mnUm.ltDoping.Count) pl.doDoping(mnUm.ltDoping[iDoping]);

      // For the next 7 days ...
      for (byte iD = 0; iD < 7; iD++) {
        DateTime dtTmp = mnUm.dtDatum.AddDays(iD);

        if (iD > 0) {
          //if ((int)dtTmp.DayOfWeek == 0) break;

          // ... do training
          CornerkickManager.Player.doTraining(ref pl, training, iTrainerCondi, iTrainerPhysio, 2, 2, dtTmp, usr, iTrainingPerDay: 1, ltPlayerTeam: null, campBooking: camp, bJouth: false, bNoInjuries: true);
        }

        // ... add training data to dataPoints
        dataPoints[0].Add(new Models.DataPointGeneral(iD + 1, pl.fCondition));
        dataPoints[1].Add(new Models.DataPointGeneral(iD + 1, pl.fFresh));
        dataPoints[2].Add(new Models.DataPointGeneral(iD + 1, pl.fMoral));
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

  }
}