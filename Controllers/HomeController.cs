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
      if (league != null) mdUm.sAfLeague = league.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(2, MvcApplication.iNations[0]);
      if (cup != null) mdUm.sAfCup = cup.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cupGold = MvcApplication.ckcore.tl.getCup(3);
      if (cupGold != null) mdUm.sAfCupGold = cupGold.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cupSilver = MvcApplication.ckcore.tl.getCup(4);
      if (cupSilver != null) mdUm.sAfCupSilver = cupSilver.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup cupWc = MvcApplication.ckcore.tl.getCup(7);
      if (cupWc != null) mdUm.sAfWc = cupWc.settings.fAttraction.ToString("0.00");

      CornerkickManager.Cup tg = MvcApplication.ckcore.tl.getCup(5);
      if (tg != null) mdUm.sAfTg = tg.settings.fAttraction.ToString("0.00");

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
        iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new Models.DataPointGeneral( 500, iCostDays[0]));

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats + 1000;
        iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stNew, stDatum, user);
        dataPoints[iDp].Add(new Models.DataPointGeneral(1000, iCostDays[0]));

        stNew.blocks[0].iSeats = stDatum.blocks[0].iSeats + 2000;
        iCostDays = CornerkickManager.Stadium.getCostDaysContructStadium(stNew, stDatum, user);
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

      // Create club
      CornerkickManager.Club clb = new CornerkickManager.Club();
      mnUm.ltClubs.Add(clb);

      // Create player
      CornerkickManager.Player pl = new CornerkickManager.Player();
      pl.plGame.fCondition = 0.6f;
      pl.plGame.fFresh = 0.8f;
      pl.plGame.fMoral = 1.0f;
      pl.contract = CornerkickManager.PlayerTool.getContract(pl, 1, clb, mnUm.dtDatum, mnUm.dtSeasonEnd);
      pl.plGame.dtBirthday = mnUm.dtDatum.AddYears(-iAge);

      // Trainings camp
      CornerkickManager.TrainingCamp.Booking camp = new CornerkickManager.TrainingCamp.Booking();
      if (iCamp >= 0 && iCamp < MvcApplication.ckcore.tcp.ltCamps.Count) {
        camp.camp = MvcApplication.ckcore.tcp.ltCamps[iCamp];
        camp.dtDeparture = mnUm.dtDatum.AddDays(-1);
        camp.dtReturn    = mnUm.dtDatum.AddDays(+8);
      }

      // Doping
      if (iDoping >= 0 && iDoping < MvcApplication.ckcore.ltDoping.Count) pl.plGame.doDoping(MvcApplication.ckcore.ltDoping[iDoping]);

      // For the next 7 days ...
      for (byte iD = 0; iD < 7; iD++) {
        DateTime dtTmp = mnUm.dtDatum.AddDays(iD);

        if (iD > 0) {
          //if ((int)dtTmp.DayOfWeek == 0) break;

          // ... do training
          CornerkickManager.PlayerTool.Training training = CornerkickManager.PlayerTool.getTraining(iType, MvcApplication.ckcore.plt.ltTraining);
          CornerkickManager.PlayerTool.doTraining(ref pl, training, MvcApplication.ckcore.plt.ltTraining, iTrainerCondi, iTrainerPhysio, 2, 2, dtTmp, usr, iTrainingPerDay: 1, ltPlayerTeam: null, campBooking: camp, bJouth: false, bNoInjuries: true);
        }

        // ... add training data to dataPoints
        dataPoints[0].Add(new Models.DataPointGeneral(iD + 1, pl.plGame.fCondition));
        dataPoints[1].Add(new Models.DataPointGeneral(iD + 1, pl.plGame.fFresh));
        dataPoints[2].Add(new Models.DataPointGeneral(iD + 1, pl.plGame.fMoral));
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public ContentResult UmGetPlayerStepsFreshLoss(int iSpeed, int iAcceleration, int iStepsLast, string sFreshStart, string sCondition, string sForm, string sTcPower)
    {
      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[3];

      float fTcPower = 0.0f;
      if (!float.TryParse(sTcPower, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out fTcPower)) return null;

      // Create player
      CornerkickGame.Player pl = new CornerkickGame.Player();
      if (!float.TryParse(sCondition,  System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pl.fCondition)) return null;
      if (!float.TryParse(sForm,       System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pl.fForm))      return null;
      if (!float.TryParse(sFreshStart, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pl.fFresh))     return null;
      pl.iSkill[ 0] = (byte)iSpeed;
      pl.iSkill[16] = (byte)iAcceleration;
      pl.fMoral = 1.0f;
      pl.fExperiencePos[10] = 1f;

      CornerkickGame.Tool.setPlayerSteps(pl, 11, 0);
      pl.iStepsCurr = 0;
      pl.iStepsLast = (byte)iStepsLast;

      // Create player with no acceleration effect
      CornerkickGame.Player plNoAcc = pl.Clone();
      plNoAcc.iStepsLast = (byte)iSpeed;

      // Initialize dataPoints list
      for (byte j = 0; j < dataPoints.Length; j++) dataPoints[j] = new List<Models.DataPointGeneral>();

      float[] fFreshLoss = new float[2];
      // For all steps ...
      byte iS = 0;
      while (pl.fSteps > 0) {
        // ... add fresh
        dataPoints[0].Add(new Models.DataPointGeneral(iS, pl     .fFresh));
        dataPoints[1].Add(new Models.DataPointGeneral(iS, plNoAcc.fFresh));
        if (CornerkickGame.AI.getFreshPlayerMoveLimit(pl.fSteps, pl.fFresh, fTcPower) < 1f) dataPoints[2].Add(new Models.DataPointGeneral(iS, 1f));
        else                                                                                dataPoints[2].Add(new Models.DataPointGeneral(iS, 0f));

        pl.fSteps -= 1f;
        plNoAcc.fSteps -= 1f;

        pl     .iStepsCurr++;
        plNoAcc.iStepsCurr++;

        // Reduce fresh
        float fAcceleration = CornerkickGame.Tool.getSkillEff(pl, 16, 11);
        pl     .fFresh -= CornerkickGame.Tool.getFreshLoss(pl,      15, fAcceleration);
        plNoAcc.fFresh -= CornerkickGame.Tool.getFreshLoss(plNoAcc, 15, fAcceleration);

        iS++;
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

  }
}