using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace CornerkickWebMvc.Controllers
{
  //[Authorize]
  public class MemberController : Controller
  {
    public MemberController()
    {
#if _CONSOLE
      ConsoleNews();
#endif
    }

    // 
    // GET: /HelloWorld/ 
    [Authorize]
    public string Index()
    {
      return "This is my <b>default</b> action...";
    }

    // 
    // GET: /HelloWorld/Welcome/ 
    [Authorize]
    public string Welcome(string name, int numTimes = 1)
    {
      return HttpUtility.HtmlEncode("Hello " + name + ", NumTimes is: " + numTimes);
    }

#if _CONSOLE
    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Console
    /// </summary>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult ConsoleNews()
    {
      if (AccountController.ckconsole == null) return View("Console", "");

      string s = MvcApplication.ckcore.sWochentag[(int)MvcApplication.ckcore.dtDatum.DayOfWeek] + ", " + MvcApplication.ckcore.dtDatum.ToShortDateString() + ", " + MvcApplication.ckcore.dtDatum.ToShortTimeString() + " Uhr\n\n";

      if (ModelState.IsValid) {
        ModelState.Clear();
      }

      for (int iN = 0; iN < AccountController.ckUser.ltNews.Count; iN++) {
        CornerkickCore.Core.News news = AccountController.ckUser.ltNews[iN];
        if (news.bUnread) {
          s += news.sNews + '\n';
          //news.bUnread = false;
          //AccountController.ckUser.ltNews[iN] = news;
        }
      }

      ViewData["sNews"] = s;

      //return RedirectToAction("Console", "Member");
      return View("Console", "");
    }

    [Authorize]
    public ActionResult Console()
    {
      if (AccountController.ckconsole == null) return View("Console", "");

      string s = "";

      if (ModelState.IsValid) {
        ModelState.Clear();
      }
      //foreach (string s1 in AccountController.ckconsole.ltPrint) s += s1 + '\n';

      ViewData["s"] = s;

      //return RedirectToAction("Console", "Member");
      return View("Console", "");
    }

    public ActionResult ConsoleInput(Models.ConsoleModels input)
    {
      ViewData["s"] = "";

      if (ModelState.IsValid) {
        //TODO: SubscribeUser(model.Email);
      }

      if (string.IsNullOrEmpty(input.sIn)) AccountController.ckconsole.resetMenu();

      if (passInputToCk(input.sIn)) AccountController.ckconsole.game(AccountController.ckUser);
      Console();

      return View("Console", "");
    }

    private bool passInputToCk(string sIn)
    {
      //AccountController.ckconsole.sInput = sIn;
      return true;
    }
#endif

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Desk
    /// </summary>
    /// <param name="desk"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult Desk(Models.DeskModel desk)
    {
      desk.sNews = "";

      if (ModelState.IsValid) {
        ModelState.Clear();
      }

      for (int iN = 0; iN < AccountController.ckUser.ltNews.Count; iN++) {
        CornerkickCore.Core.News news = AccountController.ckUser.ltNews[iN];
        if (news.bUnread) {
          desk.sNews += news.sNews + '\n';
          //news.bUnread = false;
          //AccountController.ckUser.ltNews[iN] = news;
        }
      }

      return View(desk);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Team
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    //[Authorize]
    public ActionResult Team(Models.TeamModels team)
    {
      setModelLtPlayer();

      //List<System.Web.Mvc.SelectListItem> ltsFormations = new List<System.Web.Mvc.SelectListItem>();
      if (team.ltsFormations == null) team.ltsFormations = new List<SelectListItem>();
      team.ltsFormations.Clear();

      for (int i = 0; i < MvcApplication.ckcore.ltFormationen.Count; i++) {
        CornerkickCore.Core.Formation frm = MvcApplication.ckcore.ltFormationen[i];
        bool bSelected = false;
        if (MvcApplication.ckcore.ltClubs.Count > AccountController.ckUser.iTeam) {
          if (i == MvcApplication.ckcore.ltClubs[AccountController.ckUser.iTeam].formation.iID) {
            bSelected = true;
          }
        }
        team.ltsFormations.Add(new SelectListItem { Text = (i + 1).ToString() + " - " + frm.sName, Value = i.ToString(), Selected = bSelected });
      }

      return View(team);
    }

    private void setModelLtPlayer()
    {
      int iC = AccountController.ckUser.iTeam;

      if (Models.TeamModels.ltPlayer == null) Models.TeamModels.ltPlayer = new List<CornerkickGame.Game.Spieler>();
      else Models.TeamModels.ltPlayer.Clear();
      if (MvcApplication.ckcore.ltClubs.Count > iC) {
        foreach (int iSp in MvcApplication.ckcore.ltClubs[iC].ltSpielerID) {
          Models.TeamModels.ltPlayer.Add(MvcApplication.ckcore.ltSpieler[iSp]);
        }
      }
    }

    public void UpdateRow(int fromPosition, int toPosition)
    {
      if (fromPosition < 1 || toPosition < 1) return;

      int iC = AccountController.ckUser.iTeam;

      int iPlayerID = MvcApplication.ckcore.ltClubs[iC].ltSpielerID[fromPosition - 1];
      MvcApplication.ckcore.ltClubs[iC].ltSpielerID.RemoveAt(fromPosition - 1);
      MvcApplication.ckcore.ltClubs[iC].ltSpielerID.Insert(toPosition - 1, iPlayerID);

      setModelLtPlayer();

      CkAufstellungFormation(MvcApplication.ckcore.ltClubs[iC].formation.iID);
    }

    public JsonResult SwitchPlayerByIndex(int iIndex1, int iIndex2)
    {
      int iC = AccountController.ckUser.iTeam;

      int jPosMin = Math.Min(iIndex1, iIndex2);
      int jPosMax = Math.Max(iIndex1, iIndex2);

      int iPlayerID1 = MvcApplication.ckcore.ltClubs[iC].ltSpielerID[jPosMin];
      int iPlayerID2 = MvcApplication.ckcore.ltClubs[iC].ltSpielerID[jPosMax];

      MvcApplication.ckcore.ltClubs[iC].ltSpielerID.Remove(iPlayerID1);
      MvcApplication.ckcore.ltClubs[iC].ltSpielerID.Remove(iPlayerID2);

      MvcApplication.ckcore.ltClubs[iC].ltSpielerID.Insert(jPosMin, iPlayerID2);
      MvcApplication.ckcore.ltClubs[iC].ltSpielerID.Insert(jPosMax, iPlayerID1);

      if (jPosMin < 11) {
        CornerkickGame.Game.Spieler sp1 = MvcApplication.ckcore.ltSpieler[iPlayerID1];
        CornerkickGame.Game.Spieler sp2 = MvcApplication.ckcore.ltSpieler[iPlayerID2];

        byte iPos2Tmp = sp2.iPos;
        sp2.iPos = sp1.iPos;

        if (jPosMax < 11) {
          sp1.iPos = iPos2Tmp;
        }

        MvcApplication.ckcore.ltSpieler[iPlayerID1] = sp1;
        MvcApplication.ckcore.ltSpieler[iPlayerID2] = sp2;
      }

      setModelLtPlayer();

      return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet);
    }

    public JsonResult SwitchPlayerByID(int iID1, int iID2)
    {
      int iC = AccountController.ckUser.iTeam;

      int iIndex1 = MvcApplication.ckcore.ltClubs[iC].ltSpielerID.IndexOf(iID1);
      int iIndex2 = MvcApplication.ckcore.ltClubs[iC].ltSpielerID.IndexOf(iID2);

      return Json(SwitchPlayerByIndex(iIndex1, iIndex2), JsonRequestBehavior.AllowGet);
    }

    public class DatatableEntryTeam
    {
      public string sI { get; set; }
      public string sID { get; set; }
      public string sNr { get; set; }
      public string sNull { get; set; }
      public string sName { get; set; }
      public string sPosition { get; set; }
      public string sStaerke { get; set; }
      public string sKondi { get; set; }
      public string sFrische { get; set; }
      public string sMoral { get; set; }
      public string sErf { get; set; }
      public string sMarktwert { get; set; }
      public string sGehalt { get; set; }
      public string sLz { get; set; }
      public string sNat { get; set; }
      public string sForm { get; set; }
      public string sAlter { get; set; }
    }

    public ActionResult getJSONQuery()
    {
      /*
      List<CornerkickGame.Game.Spieler> ltSpieler = new List<CornerkickGame.Game.Spieler>();
      foreach (int iSp in AccountController.ckClub.ltSpielerID) {
        ltSpieler.Add(MvcApplication.ckcore.ltSpieler[iSp]);
      }
      */
      List<string[]> ltLV = MvcApplication.ckcore.sLVMannschaft(Models.TeamModels.ltPlayer, false, false);

      //The table or entity I'm querying
      DatatableEntryTeam[] query = new DatatableEntryTeam[Models.TeamModels.ltPlayer.Count];

      for (int i = 0; i < query.Length; i++) {
        //Hard coded data here that I want to replace with query results
        query[i] = new DatatableEntryTeam { sI = (i + 1).ToString(), sID = ltLV[i][0], sNr = ltLV[i][1], sNull = "", sName = ltLV[i][2], sPosition = ltLV[i][3], sStaerke = ltLV[i][4], sKondi = ltLV[i][5], sFrische = ltLV[i][6], sMoral = ltLV[i][7], sErf = ltLV[i][8], sMarktwert = ltLV[i][9], sGehalt = ltLV[i][10], sLz = ltLV[i][11], sNat = ltLV[i][12], sForm = ltLV[i][13], sAlter = ltLV[i][14] };
      }

      /*
      var query = new[]
                {
                    //Hard coded data here that I want to replace with query results
                    new DatatableEntryTeam { sI = "0", sID = "1", sNull = "", sName = "1" },
                    new DatatableEntryTeam { sI = "1", sID = "3", sNull = "", sName = "2" },
                    new DatatableEntryTeam { sI = "2", sID = "5", sNull = "", sName = "3" },
                    new DatatableEntryTeam { sI = "3", sID = "2", sNull = "", sName = "4" },
                    new DatatableEntryTeam { sI = "4", sID = "4", sNull = "", sName = "5" },
                    new DatatableEntryTeam { sI = "5", sID = "6", sNull = "", sName = "6" }
                };
      */

      return Json(new { aaData = query }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult CkAufstellungKI()
    {
      MvcApplication.ckcore.doAufstellungKI(AccountController.ckUser.iTeam, true, true);
      return RedirectToAction("Team");
    }

    public JsonResult CkAufstellungFormation(int iF)
    {
      if (iF >= 0 && iF < MvcApplication.ckcore.ltFormationen.Count && MvcApplication.ckcore.ltClubs.Count > AccountController.ckUser.iTeam) {
        CornerkickCore.Core.Club club = MvcApplication.ckcore.ltClubs[AccountController.ckUser.iTeam];
        club.formation = MvcApplication.ckcore.ltFormationen[iF];
        MvcApplication.ckcore.tl.setFormationToClub(ref club);
        MvcApplication.ckcore.ltClubs[AccountController.ckUser.iTeam] = club;

        setModelLtPlayer();
      }

      return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetPlayerStrength(int iPlayer)
    {
      CornerkickGame.Game.Spieler player = MvcApplication.ckcore.ltSpieler[iPlayer];
      return Json(MvcApplication.ckcore.spiel.tl.getStaerkeSpieler(player, player.iPos, true).ToString("0.0"), JsonRequestBehavior.AllowGet);
    }

    public ActionResult PlayerDetails(int i)
    {
      Models.TeamModels team = new Models.TeamModels();
      team.iPlayerDetails = i;
      team.iPlayerIndTr = MvcApplication.ckcore.ltSpieler[i].iIndTraining;
      return View(team);
    }

    public ActionResult setPlayerIndTraining(int iPlayer, int iIndTr)
    {
      CornerkickGame.Game.Spieler player = MvcApplication.ckcore.ltSpieler[iPlayer];
      player.iIndTraining = (byte)iIndTr;
      MvcApplication.ckcore.ltSpieler[iPlayer] = player;

      //return View(new Models.TeamModels { iPlayerIndTr = iIndTr });
      return Json(iIndTr, JsonRequestBehavior.AllowGet);
    }

    //[Authorize]
    public ActionResult Taktik(Models.TaktikModel taktik)
    {
      return View(taktik);
    }

    public ActionResult setTaktik(int iTaktik, float fTaktik)
    {
      if      (iTaktik == 0) AccountController.ckClub.taktik.fAusrichtung       = fTaktik;
      else if (iTaktik == 1) AccountController.ckClub.taktik.fEinsatz           = fTaktik;
      else if (iTaktik == 2) AccountController.ckClub.taktik.fSchusshaeufigkeit = fTaktik;

      return Json(fTaktik, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Training
    /// </summary>
    /// <param name="Training"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult Training(Models.TrainingModel training)
    {
      training = new Models.TrainingModel();

      return View(training);
    }

    public ActionResult setTraining(int iTraining, int iTag)
    {
      AccountController.ckClub.training.iTraining[iTag] = (byte)iTraining;
      return Json(iTraining, JsonRequestBehavior.AllowGet);
    }

    /// <summary>
    /// League
    /// </summary>
    /// <param name="league"></param>
    /// <returns></returns>
    [Authorize]
    public ActionResult League(Models.LeagueModels league)
    {
      league.ltErg = MvcApplication.ckcore.tl.getLtErgLiga(league.iSaison, league.iLand, league.iSpKl, false);
      league.ltTbl = MvcApplication.ckcore.getTabelleLiga(league.iSaison, league.iLand, league.iSpKl, league.iSpTg, 0);
      //ViewData["League"] = league.ltTbl;

      //return View("League", "");
      return View(league);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Calendar
    /// </summary>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Calendar(Models.CalendarModels cal)
    {
      /*
      if (cal.sCal == null) cal.sCal = new List<string>();
      cal.sCal.Add("Kalender 1");
      cal.sCal.Add("Kalender 2");
      cal.sCal.Add("Kalender 3");
      ViewData["sCal"] = cal.sCal;
      */
      return View("Calendar", "");
    }

    public ActionResult PostCalendarData()
    {
      //var result = getCalendarEvents();

      var ApptListForDate = Models.DiaryEvent.getCalendarEvents();
      var eventList = from e in ApptListForDate
                      select new
                      {
                        id = e.iID,
                        title = e.sTitle,
                        start = e.sStartDate,
                        end = e.sEndDate,
                        color = e.sColor,
                        editable = e.bEditable,
                        allDay = false
                      };

      var rows = eventList.ToArray();

      return Json(rows, JsonRequestBehavior.AllowGet);
    }

    private static DateTime ConvertFromUnixTimestamp(double timestamp)
    {
      var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      return origin.AddSeconds(timestamp);
    }

    // 
    // GET: /HelloWorld/Welcome/ 
    [Authorize]
    public string Print(string s)
    {
      return s;
    }

    // GET: Member
    [Authorize]
    public ActionResult Saison()
    {
      return View();
    }

    [HttpGet]
    public ActionResult FirstAjax()
    {
      return View();
    }

  }
}