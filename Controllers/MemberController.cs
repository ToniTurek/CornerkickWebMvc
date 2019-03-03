﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services;
using System.Web.Services.Protocols;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace CornerkickWebMvc.Controllers
{
#if !DEBUG
  [Authorize]
#endif
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
    public ActionResult Desk(Models.DeskModel desk, Models.LeagueModels league)
    {
      desk.sNews = "";

      if (ModelState.IsValid) {
        ModelState.Clear();
      }

      CornerkickCore.Core.User usr = AccountController.ckUser();
      if (usr.ltNews == null) return View(desk);

      //desk.sNews += "Html.ActionLink(test, \"PlayerDetails\", \"Member\", new { i = 1 }, new { target = \"_blank\" })" + '\n';
      for (int iN = usr.ltNews.Count - 1; iN >= 0; iN--) {
        CornerkickCore.Core.News news = usr.ltNews[iN];
        if (news.iType < 99/* && news.bUnread*/) {
          string sNews = news.dt.ToString("d", AccountController.ciUser) + " " + news.dt.ToString("t", AccountController.ciUser) + " - " + news.sText + '\n';
          if (news.bRead) desk.sNewsOld += sNews;
          else            desk.sNews    += sNews;
          //news.bUnread = false;
          //AccountController.ckUser.ltNews[iN] = news;

          news.bRead  = true;
          news.bRead2 = true;
          usr.ltNews[iN] = news;
        }
      }

      CornerkickCore.Core.Club club = AccountController.ckClub();

      // Weather
      if (club.nextGame != null) desk.iWeather = club.nextGame.iWeather;

      // Get Table
      desk.sTabellenplatz = "-";
      List<CornerkickCore.Tool.TableItem> ltTbl = MvcApplication.ckcore.tl.getLeagueTable(league.iSaison, league.iLand, league.iSpKl, league.iSpTg - 1, 0);
      int iPlatz = 1;
      int iSpl = 0;
      foreach (CornerkickCore.Tool.TableItem tbl in ltTbl) {
        if (tbl.iId == club.iId) {
          iSpl = tbl.iGUV[0] + tbl.iGUV[1] + tbl.iGUV[2];
          break;
        }
        iPlatz++;
      }
      if (iSpl > 0) desk.sTabellenplatz = iPlatz.ToString() + ". Platz nach " + iSpl.ToString() + " von " + ((ltTbl.Count - 1) * 2).ToString() + " Spielen";

      // Get Cup Round
      desk.sPokalrunde = "-";
      CornerkickCore.Core.Cup cup = MvcApplication.ckcore.tl.getCup(club.iLand, 2);
      if (cup != null) {
        int nPartFirstRound = 0;
        if (cup.ltMatchdays != null) {
          if (cup.ltMatchdays.Count > 0) {
            if (cup.ltMatchdays[0].ltGameData != null) nPartFirstRound = cup.ltMatchdays[0].ltGameData.Count * 2;
          }
        }

        int nRound = MvcApplication.ckcore.tl.getPokalRundeVonNTeiln(nPartFirstRound);
        if (nRound - club.iPokalrunde > 0 && nRound - club.iPokalrunde < MvcApplication.ckcore.sPokalrunde.Length) desk.sPokalrunde = MvcApplication.ckcore.sPokalrunde[nRound - club.iPokalrunde];
      }

      /*
      desk.sVDL = "";
      List<CornerkickGame.Game.GameSummary> ltGameSummary = MvcApplication.ckcore.tl.getNextGames(club, false, true);
      foreach (CornerkickGame.Game.GameSummary gs in ltGameSummary) {
        if        (gs.iGoalsH == gs.team[1].iGoals) {
          if      (gs.team[0].iTeamId == club.iId) desk.sVDL += "U, ";
          else if (gs.team[1].iTeamId == club.iId) desk.sVDL += "u, ";
        } else if (gs.iGoalsH >  gs.team[1].iGoals) {
          if      (gs.team[0].iTeamId == club.iId) desk.sVDL += "S, ";
          else if (gs.team[1].iTeamId == club.iId) desk.sVDL += "n, ";
        } else if (gs.iGoalsH <  gs.team[1].iGoals) {
          if      (gs.team[0].iTeamId == club.iId) desk.sVDL += "N, ";
          else if (gs.team[1].iTeamId == club.iId) desk.sVDL += "s, ";
        }
      }
      desk.sVDL = desk.sVDL.Trim();
      if (string.IsNullOrEmpty(desk.sVDL)) desk.sVDL = "-";
      */

      // Kondi/Frische/Moral
      float[] fTeamAve   = MvcApplication.ckcore.tl.getTeamAve(club);
      float[] fTeamAve11 = MvcApplication.ckcore.tl.getTeamAve(club, 11);
      desk.sKFM = "K: " + fTeamAve[0].ToString("0.0%") + ", F: " + fTeamAve[1].ToString("0.0%") + ", M: " + fTeamAve[2].ToString("0.0%");
      desk.sStrength = "Durchschnittsstärke (Startelf): " + fTeamAve[3].ToString("0.00") + fTeamAve11[3].ToString(" (0.00)");

      return View(desk);
    }
    
    public ContentResult GetLastGames()
    {
      CornerkickCore.Core.Club club = AccountController.ckClub();

      List<Models.DataPointLastGames2>[] dataPoints = new List<Models.DataPointLastGames2>[3];
      dataPoints[0] = new List<Models.DataPointLastGames2>();
      dataPoints[1] = new List<Models.DataPointLastGames2>();
      dataPoints[2] = new List<Models.DataPointLastGames2>();

      List<CornerkickGame.Game.Data> ltGameData = MvcApplication.ckcore.tl.getNextGames(club, false, true);
      int iLg = 0;
      foreach (CornerkickGame.Game.Data gs in ltGameData) {
        if (gs.iGameType < 1 || gs.iGameType == 5) continue; // Testgame
        if (gs.team[0].iTeamId == 0 && gs.team[1].iTeamId == 0) continue;

        int iGameType = 0;
        if      (gs.iGameType == 2) iGameType = 1;
        else if (gs.iGameType == 5) iGameType = 2;

        if (gs.team[0].iGoals == gs.team[1].iGoals) {
          dataPoints[iGameType].Add(new Models.DataPointLastGames2(--iLg, 0));
        } else if ((gs.team[0].iGoals > gs.team[1].iGoals && gs.team[0].iTeamId == club.iId) || 
                   (gs.team[0].iGoals < gs.team[1].iGoals && gs.team[1].iTeamId == club.iId)) {
          dataPoints[iGameType].Add(new Models.DataPointLastGames2(--iLg, +1));
        } else if ((gs.team[0].iGoals < gs.team[1].iGoals && gs.team[0].iTeamId == club.iId) ||
                   (gs.team[0].iGoals > gs.team[1].iGoals && gs.team[1].iTeamId == club.iId)) {
          dataPoints[iGameType].Add(new Models.DataPointLastGames2(--iLg, -1));
        }
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public ContentResult GetTeamDevelopmentData()
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      CornerkickCore.Core.TrainingHistory trHistCurrent = new CornerkickCore.Core.TrainingHistory();
      trHistCurrent.dt = MvcApplication.ckcore.dtDatum;
      trHistCurrent.fKFM = MvcApplication.ckcore.tl.getTeamAve(clb);

      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[3];
      for (byte j = 0; j < dataPoints.Length; j++) {
        dataPoints[j] = new List<Models.DataPointGeneral>();

        for (int i = 0; i < clb.ltTrainingHist.Count; i++) {
          CornerkickCore.Core.TrainingHistory trHist = clb.ltTrainingHist[i];
          if (trHist.dt.CompareTo(MvcApplication.ckcore.dtDatum.AddDays(-7)) > 0) {
            long iDate = convertDateTimeToTimestamp(trHist.dt);
            dataPoints[j].Add(new Models.DataPointGeneral(iDate, trHist.fKFM[j]));
          }
        }

        long iDateCurrent = convertDateTimeToTimestamp(trHistCurrent.dt);
        dataPoints[j].Add(new Models.DataPointGeneral(iDateCurrent, trHistCurrent.fKFM[j]));
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public ContentResult GetTeamFAve()
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      Models.DataPointGeneral[] dataPoints = new Models.DataPointGeneral[7];

      double[] fFAve = new double[7];
      foreach (int iPlId in clb.ltPlayerId) {
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

        fFAve[0] +=  pl.fFTraining[ 0];
        fFAve[1] += (pl.fFTraining[ 1] + pl.fFTraining[ 2])                                        / 2f; // Technik + Dribbling
        fFAve[2] +=  pl.fFTraining[ 3];                                                                  // Zweikampf
        fFAve[3] += (pl.fFTraining[ 4] + pl.fFTraining[ 5] + pl.fFTraining[ 6] + pl.fFTraining[7]) / 4f; // Abspiel
        fFAve[4] += (pl.fFTraining[ 8] + pl.fFTraining[ 9] + pl.fFTraining[10])                    / 3f; // Abschluss
        fFAve[5] += (pl.fFTraining[11] + pl.fFTraining[12])                                        / 2f; // Standards
        fFAve[6] += (pl.fFTraining[13] + pl.fFTraining[14] + pl.fFTraining[15])                    / 3f; // TW
      }

      for (int iF = 0; iF < fFAve.Length; iF++) {
        dataPoints[iF] = new Models.DataPointGeneral(iF, fFAve[iF] / clb.ltPlayerId.Count);
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");

      /*
      return Json(JsonConvert.SerializeObject(dataPoints), JsonRequestBehavior.AllowGet);
      */
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
      CornerkickCore.Core.User user = AccountController.ckUser();
      CornerkickCore.Core.Club club = AccountController.ckClub();

      team.bAdmin = AccountController.checkUserIsAdmin(AccountController.appUser);

      team.bGame = false;
      if (user.game != null) team.bGame = !user.game.data.bFinished;

      setModelLtPlayer(user);

      // Formationen
      team.ltsFormations    = new List<SelectListItem>();
      team.ltsFormationsOwn = new List<SelectListItem>();

      team.ltsFormations.Add(new SelectListItem { Text = "0 - Eigene", Value = "0" });
      for (int i = 0; i < MvcApplication.ckcore.ltFormationen.Count; i++) {
        team.ltsFormations.Add(new SelectListItem { Text     = (i + 1).ToString() + " - " + MvcApplication.ckcore.ltFormationen[i].sName,
                                                    Value    = (i + 1).ToString(),
                                                    Selected = (i + 1) == club.formation.iId
                                                  }
        );
      }

      if (user.ltFormations != null) {
        for (int i = 0; i < user.ltFormations.Count; i++) {
          team.ltsFormations.Add(new SelectListItem { Text     = (MvcApplication.ckcore.ltFormationen.Count + i + 1).ToString() + " - " + user.ltFormations[i].sName,
                                                      Value    = (MvcApplication.ckcore.ltFormationen.Count + i + 1).ToString(),
                                                      Selected = (MvcApplication.ckcore.ltFormationen.Count + i + 1) == club.formation.iId
                                                    }
          );
        }
      }

      return View(team);
    }

    private void setModelLtPlayer(CornerkickCore.Core.User user)
    {
      Models.TeamModels.ltPlayer = new List<CornerkickGame.Player>();

      CornerkickCore.Core.Club club = AccountController.ckClub();
      foreach (int iSp in club.ltPlayerId) {
        Models.TeamModels.ltPlayer.Add(MvcApplication.ckcore.ltPlayer[iSp]);
      }

      if (user.game != null) {
        if (!user.game.data.bFinished) {
          Models.TeamModels.ltsSubstitution = new List<string[]>();

          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          Models.TeamModels.iSubRest = user.game.iSubstitutionsLeft[iHA];

          if (Models.TeamModels.ltiSubstitution != null) {
            foreach (int[] iSub in Models.TeamModels.ltiSubstitution) {
              Models.TeamModels.ltsSubstitution.Add(new string[] { user.game.player[iHA][iSub[0]].sName, user.game.player[iHA][iSub[1]].sName, iSub[2].ToString() });
            }
          }
        }
      }
    }

    public void UpdateRow(int fromPosition, int toPosition)
    {
      if (fromPosition < 1 || toPosition < 1) return;

      CornerkickCore.Core.User user = AccountController.ckUser();
      int iC = user.iTeam;

      int iPlayerID = MvcApplication.ckcore.ltClubs[iC].ltPlayerId[fromPosition - 1];
      if (!MvcApplication.ckcore.ltPlayer[iPlayerID].bGespielt) {
        MvcApplication.ckcore.ltClubs[iC].ltPlayerId.RemoveAt(fromPosition - 1);
        MvcApplication.ckcore.ltClubs[iC].ltPlayerId.Insert(toPosition - 1, iPlayerID);

        setModelLtPlayer(user);

        CkAufstellungFormation(MvcApplication.ckcore.ltClubs[iC].formation.iId);
      }
    }

    public JsonResult SwitchPlayerByIndex(int iIndex1, int iIndex2)
    {
      CornerkickCore.Core.User user = AccountController.ckUser();
      int iC = user.iTeam;
      CornerkickCore.Core.Club club = MvcApplication.ckcore.ltClubs[iC];

      int jPosMin = Math.Min(iIndex1, iIndex2);
      int jPosMax = Math.Max(iIndex1, iIndex2);

      int iPlayerID1 = club.ltPlayerId[jPosMin];
      int iPlayerID2 = club.ltPlayerId[jPosMax];

      if (user.game != null) {
        if (!user.game.data.bFinished) {
          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          // If switch of player in starting 11 --> do it directly
          if (jPosMin < 11 && jPosMax >= 11) {
            // Return if player in already played or if no subs left
            if (user.game.player[iHA][jPosMax].bGespielt || user.game.iSubstitutionsLeft[iHA] == 0) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet);

            if (Models.TeamModels.ltiSubstitution == null) {
              Models.TeamModels.ltiSubstitution = new List<int[]>();
            }
            Models.TeamModels.ltiSubstitution.Add(new int[] { jPosMin, jPosMax, 0 });
          } else {
            user.game.substitute(iHA == 0, (byte)jPosMin, (byte)jPosMax, 0);
          }
        }
      }

      if (jPosMin < 11) {
        CornerkickGame.Player sp1 = MvcApplication.ckcore.ltPlayer[iPlayerID1];
        CornerkickGame.Player sp2 = MvcApplication.ckcore.ltPlayer[iPlayerID2];

        System.Drawing.Point pt2Tmp = sp2.ptPosDefault;
        sp2.ptPosDefault = sp1.ptPosDefault;

        if (jPosMax < 11) {
          sp1.ptPosDefault = pt2Tmp;
        }

        //MvcApplication.ckcore.ltPlayer[iPlayerID1] = sp1;
        //MvcApplication.ckcore.ltPlayer[iPlayerID2] = sp2;
      }

      // Switch player in club list
      club.ltPlayerId.Remove(iPlayerID1);
      club.ltPlayerId.Remove(iPlayerID2);

      club.ltPlayerId.Insert(jPosMin, iPlayerID2);
      club.ltPlayerId.Insert(jPosMax, iPlayerID1);

      //MvcApplication.ckcore.ltClubs[iC] = club;
      setModelLtPlayer(user);

      return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet);
    }

    public JsonResult SwitchPlayerByID(int iID1, int iID2)
    {
      int iC = AccountController.ckUser().iTeam;

      int iIndex1 = MvcApplication.ckcore.ltClubs[iC].ltPlayerId.IndexOf(iID1);
      int iIndex2 = MvcApplication.ckcore.ltClubs[iC].ltPlayerId.IndexOf(iID2);

      return Json(SwitchPlayerByIndex(iIndex1, iIndex2), JsonRequestBehavior.AllowGet);
    }

    public class DatatableEntryTeam
    {
      public int    iIndex { get; set; }
      public string sID { get; set; }
      public string sNr { get; set; }
      public string sNull { get; set; }
      public string sName { get; set; }
      public string sPosition { get; set; }
      public string sStaerke { get; set; }
      public string sKondi { get; set; }
      public string sFrische { get; set; }
      public string sMoral { get; set; }
      public string sStaerkeIdeal { get; set; }
      public string sErf { get; set; }
      public string sForm { get; set; }
      public string sAlter { get; set; }
      public string sTalent { get; set; }
      public bool   bSubstituted { get; set; }
      public float  fLeader { get; set; }
      public string sMarktwert { get; set; }
      public string sGehalt { get; set; }
      public string sLz { get; set; }
      public string sNat { get; set; }
      public int iSuspended { get; set; }
    }

    public ActionResult getTableTeam()
    {
      /*
      List<CornerkickGame.Player> ltSpieler = new List<CornerkickGame.Player>();
      foreach (int iSp in AccountController.ckClub.ltPlayerId) {
        ltSpieler.Add(MvcApplication.ckcore.ltPlayer[iSp]);
      }
      */
      var club = CornerkickWebMvc.Controllers.AccountController.ckClub();
      var user = AccountController.ckUser();

      int iGameType = 0;
      if (club.nextGame != null) iGameType = club.nextGame.iGameType;

      bool bGame = false;
      if (user.game != null) {
        bGame = !user.game.data.bFinished;
        if (bGame) iGameType = user.game.data.iGameType;
      }

      List<string[]> ltLV = MvcApplication.ckcore.ui.listTeam(Models.TeamModels.ltPlayer, bGame, iGameType);

      //The table or entity I'm querying
      DatatableEntryTeam[] query = new DatatableEntryTeam[ltLV.Count];

      for (int i = 0; i < query.Length; i++) {
        int iSusp = 0;
        int.TryParse(ltLV[i][18], out iSusp);

        string sName = ltLV[i][2];
        int iId = -1;
        int.TryParse(ltLV[i][0], out iId);

        if (i == MvcApplication.ckcore.plr.getCaptainIx(club)) sName += " (C)";

        //Hard coded data here that I want to replace with query results
        query[i] = new DatatableEntryTeam { iIndex = i + 1, sID = ltLV[i][0], sNr = ltLV[i][1], sNull = "", sName = sName, sPosition = ltLV[i][3], sStaerke = ltLV[i][4], sKondi = ltLV[i][5], sFrische = ltLV[i][6], sMoral = ltLV[i][7], sErf = ltLV[i][8], sMarktwert = ltLV[i][9], sGehalt = ltLV[i][10], sLz = ltLV[i][11], sNat = ltLV[i][12], sForm = ltLV[i][13], sAlter = ltLV[i][14], sTalent = ltLV[i][15], bSubstituted = ltLV[i][16] == "ausg", fLeader = Convert.ToSingle(ltLV[i][19]), sStaerkeIdeal = ltLV[i][17], iSuspended = iSusp };
      }

      return Json(new { aaData = query }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult CkAufstellungKI(int iType = 0)
    {
      // Check if game running
      CornerkickCore.Core.User user = AccountController.ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) return RedirectToAction("Team");
      }

      MvcApplication.ckcore.doFormationKI(user.iTeam, true);

      return RedirectToAction("Team");
    }

    public JsonResult doFormationKI(int iType = 0)
    {
      // Check if game running
      CornerkickCore.Core.User user = AccountController.ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) return Json("error", JsonRequestBehavior.AllowGet);
      }

      MvcApplication.ckcore.doFormationKI(user.iTeam, true, iType);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult movePlayer(int iIndexPlayer, int iDirection)
    {
      CornerkickCore.Core.Club club = AccountController.ckClub();

      if (iIndexPlayer < 0 || iIndexPlayer >= club.formation.ptPos.Length) return Json("error", JsonRequestBehavior.AllowGet);

      if        (iDirection == 1) {
        if (club.formation.ptPos[iIndexPlayer].X < (MvcApplication.ckcore.game.ptPitch.X / 2) - 1) {
          club.formation.ptPos[iIndexPlayer].X += 2;
        }
      } else if (iDirection == 2) {
        if (club.formation.ptPos[iIndexPlayer].Y < +MvcApplication.ckcore.game.ptPitch.Y) {
          club.formation.ptPos[iIndexPlayer].Y += 1;
          if (club.formation.ptPos[iIndexPlayer].Y % 2 == 0) club.formation.ptPos[iIndexPlayer].X += 1;
          else                                         club.formation.ptPos[iIndexPlayer].X -= 1;
        }
      } else if (iDirection == 3) {
        if (club.formation.ptPos[iIndexPlayer].X > 1) {
          club.formation.ptPos[iIndexPlayer].X -= 2;
        }
      } else if (iDirection == 4) {
        if (club.formation.ptPos[iIndexPlayer].Y > -MvcApplication.ckcore.game.ptPitch.Y) {
          club.formation.ptPos[iIndexPlayer].Y -= 1;
          if (club.formation.ptPos[iIndexPlayer].Y % 2 == 0) club.formation.ptPos[iIndexPlayer].X += 1;
          else                                         club.formation.ptPos[iIndexPlayer].X -= 1;
        }
      }

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]];
      pl.ptPosDefault = club.formation.ptPos[iIndexPlayer];
      MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]] = pl;
      club.formation.iId = 0;

      //if (iIndexPlayer >= Models.TeamModels.ltPlayer.Count) setModelLtPlayer(usr);
      Models.TeamModels.ltPlayer[iIndexPlayer] = pl;

      updatePlayerOfGame(AccountController.ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult movePlayerRoa(int iIndexPlayer, int iDirection)
    {
      if (iIndexPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickCore.Core.Club club = AccountController.ckClub();

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]];

      if      (iDirection == 1) pl.fRadOfAction[0] += 0.2f;
      else if (iDirection == 2) pl.fRadOfAction[0] -= 0.2f;
      else if (iDirection == 3) pl.fRadOfAction[1] += 0.2f;
      else if (iDirection == 4) pl.fRadOfAction[1] -= 0.2f;

      MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]] = pl;

      //if (iIndex >= Models.TeamModels.ltPlayer.Count) setModelLtPlayer(usr);
      Models.TeamModels.ltPlayer[iIndexPlayer] = pl;

      updatePlayerOfGame(AccountController.ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult TeamSetIndOrientation(int iIndexPlayer, float fIndOrientation)
    {
      if (iIndexPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickCore.Core.Club club = AccountController.ckClub();
      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]];

      pl.fIndOrientation = fIndOrientation;

      //MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]] = pl;
      Models.TeamModels.ltPlayer[iIndexPlayer] = pl;

      updatePlayerOfGame(AccountController.ckUser().game, club);

      return Json(fIndOrientation.ToString(), JsonRequestBehavior.AllowGet);
    }

    public JsonResult TeamSetManMarking(int iIxPlayer, sbyte iIxPlayerOpp)
    {
      if (iIxPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickCore.Core.Club club = AccountController.ckClub();

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIxPlayer]];

      if (pl.iIxManMarking == iIxPlayerOpp) pl.iIxManMarking = -1;
      else                                  pl.iIxManMarking = iIxPlayerOpp;

      Models.TeamModels.ltPlayer[iIxPlayer] = pl;

      updatePlayerOfGame(AccountController.ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult TeamSetSubstitutions()
    {
      // Check if game running
      CornerkickCore.Core.User user = AccountController.ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) {
          if (Models.TeamModels.ltiSubstitution != null) {
            CornerkickCore.Core.Club club = AccountController.ckClub();
            byte iHA = 0;
            if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

            foreach (int[] iSub in Models.TeamModels.ltiSubstitution) {
              user.game.substitute(iHA == 0, (byte)iSub[0], (byte)iSub[1], 0);
            }
            Models.TeamModels.ltiSubstitution.Clear();
          }
        }
      }

      return RedirectToAction("Team");
    }

    [HttpPost]
    public ActionResult TeamUnsetSubstitutions()
    {
      // Check if game running
      CornerkickCore.Core.User user = AccountController.ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) {
          CornerkickCore.Core.Club club = AccountController.ckClub();

          Models.TeamModels.ltiSubstitution = new List<int[]>();

          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          // Create list of temp player
          List<int> ltiSpErsatz = new List<int>();
          for (int i = 18; i < club.ltPlayerId.Count; i++) ltiSpErsatz.Add(club.ltPlayerId[i]);

          // Clear current list of players
          club.ltPlayerId.Clear();

          // Add player from current game
          foreach (CornerkickGame.Player sp in user.game.player[iHA]) club.ltPlayerId.Add(sp.iId);

          // Add temp player
          foreach (int iSp in ltiSpErsatz) club.ltPlayerId.Add(iSp);

          return RedirectToAction("Team");
        }
      }

      return RedirectToAction("Team");
    }

    public JsonResult GetSubstitutionList()
    {
      List<string[]> ltsSubstitution = new List<string[]>();

      CornerkickCore.Core.User user = AccountController.ckUser();
      CornerkickCore.Core.Club club = AccountController.ckClub();

      if (user.game != null) {
        if (!user.game.data.bFinished) {
          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          Models.TeamModels.iSubRest = user.game.iSubstitutionsLeft[iHA];

          if (Models.TeamModels.ltiSubstitution != null) {
            foreach (int[] iSub in Models.TeamModels.ltiSubstitution) {
              ltsSubstitution.Add(new string[] { user.game.player[iHA][iSub[0]].sName, user.game.player[iHA][iSub[1]].sName, iSub[2].ToString() });
            }
          }
        }
      }

      return Json(ltsSubstitution, JsonRequestBehavior.AllowGet);
    }

    public JsonResult CkAufstellungFormation(int iF, int iSP = -1)
    {
      CornerkickCore.Core.User usr  = AccountController.ckUser();
      CornerkickCore.Core.Club club = AccountController.ckClub();

      if (iF >= 0) {
        if        (iF < MvcApplication.ckcore.ltFormationen.Count) {
          MvcApplication.ckcore.tl.setFormationToClub(ref club, MvcApplication.ckcore.ltFormationen[iF]);
        } else if (iF < MvcApplication.ckcore.ltFormationen.Count + usr.ltFormations.Count) {
          MvcApplication.ckcore.tl.setFormationToClub(ref club, usr.ltFormations[iF - MvcApplication.ckcore.ltFormationen.Count]);
        }

        updatePlayerOfGame(usr.game, club);
      }

      setModelLtPlayer(usr);

      Models.TeamModels.TeamData tD = new Models.TeamModels.TeamData();
      //return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet);

      tD.ltPlayer = Models.TeamModels.ltPlayer;
      tD.ltPlayerPos      = new List<byte>  ();
      tD.ltPlayerAveSkill = new List<string>();
      foreach (CornerkickGame.Player pl in tD.ltPlayer) {
        tD.ltPlayerPos     .Add(MvcApplication.ckcore.game.tl.getBasisPos(MvcApplication.ckcore.game.tl.getPosRole(pl)));
        tD.ltPlayerAveSkill.Add(MvcApplication.ckcore.game.tl.getAveSkill(pl, 99).ToString("0.0"));
      }
      tD.sDivPlayer = TeamGetPlayerOffence(tD.ltPlayer, club);
      if (iSP >= 0) {
        tD.plSelected = tD.ltPlayer[iSP];
        tD.fIndOrientation = tD.plSelected.fIndOrientation;
        if (tD.fIndOrientation < -1f) tD.fIndOrientation = MvcApplication.ckcore.game.tl.getPlayerIndividualOrientationDefault(tD.plSelected);

        tD.sDivRoa               = TeamGetPlayerRadiusOfAction(iSP, club);
        tD.fIndOrientationMinMax = TeamGetIndOrientationMinMax(iSP, club);
      }

      if (club.nextGame != null) {
        int iClubOpp = club.nextGame.team[1].iTeamId;
        if (club.nextGame.team[1].iTeamId == club.iId) iClubOpp = club.nextGame.team[0].iTeamId;
        tD.ltPlayerOpp = new List<CornerkickGame.Player>();
        if (iClubOpp > 0) {
          for (byte iPl = 0; iPl < 11; iPl++) {
            int iPlId = MvcApplication.ckcore.ltClubs[iClubOpp].ltPlayerId[iPl];
            tD.ltPlayerOpp.Add(MvcApplication.ckcore.ltPlayer[iPlId]);
          }
        }
      }

      return Json(tD, JsonRequestBehavior.AllowGet);
    }

    // Set positions to player of current game
    private void updatePlayerOfGame(CornerkickGame.Game game, CornerkickCore.Core.Club club)
    {
      if (game == null) return;
      if (game.data.bFinished) return;

      byte iHA = 0;
      if (game.data.team[1].iTeamId == club.iId) iHA = 1;

      for (byte iPl = 0; iPl < game.nPlStart; iPl++) {
        foreach (int iPlId in club.ltPlayerId) {
          if (iPlId == game.player[iHA][iPl].iId) {
            game.player[iHA][iPl] = MvcApplication.ckcore.ltPlayer[iPlId];
            break;
          }
        }

        game.player[iHA][iPl].ptPosDefault = club.formation.ptPos[iPl];
      }
    }

    public JsonResult saveFormation(string sName)
    {
      if (string.IsNullOrEmpty(sName)) Json(-1, JsonRequestBehavior.AllowGet);

      CornerkickCore.Core.User user = AccountController.ckUser();
      CornerkickCore.Core.Club club = AccountController.ckClub();

      club.formation.iId = MvcApplication.ckcore.ltFormationen.Count + user.ltFormations.Count + 1;

      CornerkickCore.Core.Formation frmUser = MvcApplication.ckcore.ini.newFormation();
      frmUser.iId = club.formation.iId;
      frmUser.sName = sName;
      for (int iPt = 0; iPt < club.formation.ptPos.Length; iPt++) frmUser.ptPos[iPt] = club.formation.ptPos[iPt];

      user.ltFormations.Add(frmUser);

      AccountController.setCkUser(user);

      return Json(MvcApplication.ckcore.ltFormationen.Count + user.ltFormations.Count, JsonRequestBehavior.AllowGet);
    }

    public JsonResult deleteFormation(int iFormation)
    {
      CornerkickCore.Core.User user = AccountController.ckUser();

      if (iFormation >= MvcApplication.ckcore.ltFormationen.Count + 1 && iFormation < MvcApplication.ckcore.ltFormationen.Count + user.ltFormations.Count + 1) {
        CornerkickCore.Core.Club club = AccountController.ckClub();

        user.ltFormations.RemoveAt(iFormation - MvcApplication.ckcore.ltFormationen.Count - 1);

        CkAufstellungFormation(0);
      }

      AccountController.setCkUser(user);

      return Json(1, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetPlayerStrength(int iPlayer)
    {
      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      double fStrength = MvcApplication.ckcore.game.tl.getAveSkill(player, 99);
      return Json(fStrength, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetPlayerStrengthAgeAve()
    {
      CornerkickCore.Core.Club club = AccountController.ckClub();
      float[] f = new float[2];
      for (byte i = 0; i < MvcApplication.ckcore.game.nPlStart; i++) {
        CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[i]];
        f[0] += MvcApplication.ckcore.game.tl.getAveSkill(player, 99);
        f[1] += MvcApplication.ckcore.game.tl.getPlayerAgeFloat(player, MvcApplication.ckcore.dtDatum);
      }

      f[0] /= MvcApplication.ckcore.game.nPlStart;
      f[1] /= MvcApplication.ckcore.game.nPlStart;

      return Json(f, JsonRequestBehavior.AllowGet);
    }

    private string TeamGetPlayerOffence(List<CornerkickGame.Player> ltPlayer, CornerkickCore.Core.Club club)
    {
      string sDiv = "";

      if (ltPlayer       == null) return "";
      if (ltPlayer.Count ==    0) return "";

      byte iPl = 0;
      foreach (CornerkickGame.Player pl in ltPlayer) {
        if (iPl >= MvcApplication.ckcore.game.nPlStart) break;

        if (MvcApplication.ckcore.game.tl.getPosRole(pl.ptPosDefault) != 1) {
          float fHeight = 0.04f;
          float fWidth  = fHeight * (3f / 2f);

          int iXOffence = MvcApplication.ckcore.game.tl.getXPosOffence(pl, club.taktik.fOrientation);
          float fTop = 1f - (iXOffence / (float)MvcApplication.ckcore.game.ptPitch.X);
          float fLeft = (pl.ptPosDefault.Y + MvcApplication.ckcore.game.ptPitch.Y) / (float)(2 * MvcApplication.ckcore.game.ptPitch.Y);

          fTop  -= fHeight / 2f;
          fLeft -= fWidth  / 2f;

          sDiv += "<div onclick=\"javascript: selectPlayer(" + iPl.ToString() + ")\" style=\"position: absolute; ";
          sDiv += "top: "  + fTop .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
          sDiv += "left: " + fLeft.ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
          sDiv += "height: 5%; ";
          sDiv += "width: 7.5%; ";
          sDiv += "border: 4px solid blue; ";
          sDiv += "background-color: white; ";
          sDiv += "-webkit-border-radius: 50%; ";
          sDiv += "-moz-border-radius: 50%; ";
          sDiv += "cursor: pointer; ";
          sDiv += "\">";
          sDiv += "<b style=\"position: absolute; width: 100%; text-align:center; font-size:200%; color: black\">";
          sDiv += pl.iNr.ToString();
          sDiv += "</b>";
          sDiv += "</div>";
        }

        iPl++;
      }

      return sDiv;
    }

    private string TeamGetPlayerRadiusOfAction(int iPlayerIndex, CornerkickCore.Core.Club club)
    {
      string sDiv = "";

      if (iPlayerIndex < 0 || iPlayerIndex >= MvcApplication.ckcore.game.nPlStart || iPlayerIndex >= club.ltPlayerId.Count) return "";

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPlayerIndex]];

      int iXOffence = MvcApplication.ckcore.game.tl.getXPosOffence(pl, club.taktik.fOrientation);

      for (double fRoa = 0.5; fRoa < 1.01; fRoa += 0.1) {
        System.Drawing.Point ptRoaTL = MvcApplication.ckcore.game.tl.getRndPos(pl, club.taktik.fOrientation, +1, -1, fRoa, fRoa);
        System.Drawing.Point ptRoaBR = MvcApplication.ckcore.game.tl.getRndPos(pl, club.taktik.fOrientation, -1, +1, fRoa, fRoa);

        float fTopRoa  = 1f - ((iXOffence + ptRoaTL.X) / (float)MvcApplication.ckcore.game.ptPitch.X);
        float fLeftRoa = (pl.ptPosDefault.Y + ptRoaTL.Y + MvcApplication.ckcore.game.ptPitch.Y) / (float)(2 * MvcApplication.ckcore.game.ptPitch.Y);
        float fHeightRoa = Math.Abs(ptRoaBR.X - ptRoaTL.X) / (float)     MvcApplication.ckcore.game.ptPitch.X;
        float fWidthRoa  = Math.Abs(ptRoaBR.Y - ptRoaTL.Y) / (float)(2 * MvcApplication.ckcore.game.ptPitch.Y);

        sDiv += "<div ";
        sDiv += "style=\"position: absolute; ";
        sDiv += "top: "    + fTopRoa   .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "left: "   + fLeftRoa  .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "height: " + fHeightRoa.ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "width: "  + fWidthRoa .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
        sDiv += "border: 1px solid yellow; ";
        sDiv += "\">"; // end of style section

        sDiv += "<text style=\"color: yellow\">";
        sDiv += fRoa.ToString(" 0%");
        sDiv += "</text>";

        /*
        if (fRoa > 0.89 && fRoa < 0.91) {
          sDiv += "<div style=\"position: absolute; width: 40px; top: -22px; left: 50%; z-index: 10\"> " +
                    "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(1)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_up.png\"/>" +
                  "</div>";
          sDiv += "<div style=\"position: absolute; width: 40px; top: 0px; left: 50%; z-index: 10\"> " +
                    "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(2)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_down.png\"/>" +
                  "</div>";
          sDiv += "<div style=\"position: absolute; width: 16px; top: 50%; left: -20px; z-index: 10\"> " +
                    "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(3)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_left.png\"/>" +
                  "</div>";
          sDiv += "<div style=\"position: absolute; width: 16px; top: 50%; left: 2px; z-index: 10\"> " +
                    "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(4)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_right.png\"/>" +
                  "</div>";
        }
        */

        sDiv += "</div>";
      }

      float fTopButton  = 1f - (iXOffence / (float)MvcApplication.ckcore.game.ptPitch.X);
      float fLeftButton = (pl.ptPosDefault.Y + MvcApplication.ckcore.game.ptPitch.Y) / (float)(2 * MvcApplication.ckcore.game.ptPitch.Y);
      sDiv += "<div style=\"position: absolute; width: 8%; top: "
                + (fTopButton  - 0.100f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: " 
                + (fLeftButton - 0.030f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) 
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(1)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_up.png\"/>" +
              "</div>";
      sDiv += "<div style=\"position: absolute; width: 8%; top: "
                + (fTopButton  - 0.045f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: "
                + (fLeftButton - 0.030f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(2)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_down.png\"/>" +
              "</div>";
      sDiv += "<div style=\"position: absolute; width: 3%; top: "
                + (fTopButton  - 0.015f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: "
                + (fLeftButton - 0.120f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(3)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_left.png\"/>" +
              "</div>";
      sDiv += "<div style=\"position: absolute; width: 3%; top: "
                + (fTopButton  - 0.015f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: "
                + (fLeftButton - 0.070f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(4)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_right.png\"/>" +
              "</div>";

      return sDiv;
    }

    private float[] TeamGetIndOrientationMinMax(int iPlayerIndex, CornerkickCore.Core.Club club)
    {
      float[] fIndOrientationMinMax = new float[2];

      if (iPlayerIndex < 0 || iPlayerIndex >= MvcApplication.ckcore.game.nPlStart || iPlayerIndex >= club.ltPlayerId.Count) return fIndOrientationMinMax;

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPlayerIndex]];

      fIndOrientationMinMax[0] = MvcApplication.ckcore.game.tl.getXPosOffence(pl.ptPosDefault.X, club.taktik.fOrientation, -1f) / (float)MvcApplication.ckcore.game.ptPitch.X;
      fIndOrientationMinMax[1] = MvcApplication.ckcore.game.tl.getXPosOffence(pl.ptPosDefault.X, club.taktik.fOrientation, +1f) / (float)MvcApplication.ckcore.game.ptPitch.X;

      return fIndOrientationMinMax;
    }

    public ActionResult PlayerDetails(int i)
    {
      Models.PlayerModel plModel = new Models.PlayerModel();

      CornerkickCore.Core.Club club = AccountController.ckClub();

      plModel.iPlayer = i;
      CornerkickGame.Player plDetails = MvcApplication.ckcore.ltPlayer[i];
      plModel.iPlayerIndTr = plDetails.iIndTraining;

      plModel.iContractYears = 1;

      plModel.sName = plDetails.sName;

      plModel.ltDdlNo = new List<SelectListItem>();
      plModel.iNo = plDetails.iNr;

      List<int> ltNoExist = new List<int>();
      foreach (int iPlId in club.ltPlayerId) {
        ltNoExist.Add(MvcApplication.ckcore.ltPlayer[iPlId].iNr);
      }

      if (plDetails.iNr == 0) {
        plModel.ltDdlNo.Add(
          new SelectListItem {
            Text  = "0",
            Value = "0"
          }
        );
      }

      for (int j = 1; j < 41; j++) {
        if (ltNoExist.IndexOf(j) >= 0 && j != plDetails.iNr) continue;

        plModel.ltDdlNo.Add(
          new SelectListItem {
            Text  = j.ToString(),
            Value = j.ToString()
          }
        );
      }

      // Next / Prev. Player
      plModel.iPlIdPrev = -1;
      plModel.iPlIdNext = -1;

      int iIndex = club.ltPlayerId.IndexOf(plDetails.iId);

      if (iIndex >= 0) {
        if (iIndex >                         0) plModel.iPlIdPrev = club.ltPlayerId[iIndex - 1];
        if (iIndex < club.ltPlayerId.Count - 1) plModel.iPlIdNext = club.ltPlayerId[iIndex + 1];
      }

      return View(plModel);
    }

    public ActionResult setPlayerIndTraining(int iPlayer, int iIndTr)
    {
      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      player.iIndTraining = (byte)iIndTr;
      //MvcApplication.ckcore.ltPlayer[iPlayer] = player;

      return Json(iIndTr, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setPlayerName(int iPlayer, string sName)
    {
      if (string.IsNullOrEmpty(sName)) {
        Response.StatusCode = 1;
        return Json("Name darf nicht leer sein!", JsonRequestBehavior.AllowGet);
        //return Json(new { message = "Name darf nicht leer sein!" }, JsonRequestBehavior.AllowGet);
      }

      foreach (CornerkickGame.Player pl in MvcApplication.ckcore.ltPlayer) {
        if (string.IsNullOrEmpty(pl.sName)) continue;

        if (pl.sName.Equals(sName)) {
          Response.StatusCode = 1;
          return Json("Name bereits vorhanden!", JsonRequestBehavior.AllowGet);
          //return Json(new { message = "Name bereits vorhanden!" }, JsonRequestBehavior.AllowGet);
        }
      }

      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      player.sName = sName;
      //MvcApplication.ckcore.ltPlayer[iPlayer] = player;

      return Json(player.sName, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setPlayerNo(int iPlayer, byte iNo)
    {
      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      player.iNr = iNo;
      //MvcApplication.ckcore.ltPlayer[iPlayer] = player;

      return Json(player.iNr, JsonRequestBehavior.AllowGet);
    }

    public ActionResult PlayerMakeCaptain(int iPlayerId, byte iC)
    {
      if (iPlayerId < 0) return Json("Error", JsonRequestBehavior.AllowGet);

      CornerkickCore.Core.Club club = AccountController.ckClub();

      if (club.iCaptainId == null) club.iCaptainId = new int[3];

      if (iC >= club.iCaptainId.Length) return Json("Error", JsonRequestBehavior.AllowGet);

      string sCaptain = "Kapitän";
      if (iC == 1) sCaptain = "Vize-Kapitän";

      MvcApplication.ckcore.plr.makePlayerCaptain(iPlayerId, iC, club);

      //MvcApplication.ckcore.ltPlayer[iPlayerId] = player;

      return Json("Sie haben " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " zum " + sCaptain + " ernannt.", JsonRequestBehavior.AllowGet);
    }

    public ActionResult getClubCaptain(int iC)
    {
      CornerkickCore.Core.Club club = AccountController.ckClub();

      if (club.iCaptainId[iC] >= 0) return Json(MvcApplication.ckcore.ltPlayer[club.iCaptainId[iC]].sName, JsonRequestBehavior.AllowGet);

      return Json("", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult GetPlayerSalary(int iPlayerId, byte iYears, int iSalaryOffer = 0)
    {
      if (iPlayerId < 1) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iYears    < 1) return Json("0",     JsonRequestBehavior.AllowGet);

      int iSalary = MvcApplication.ckcore.plr.getSalary(MvcApplication.ckcore.ltPlayer[iPlayerId], iYears);
      double fMood = 1.0;

      CornerkickCore.Core.Club club = AccountController.ckClub();

      CornerkickCore.csTransfer.TransferOffer offer = MvcApplication.ckcore.tr.getOffer(iPlayerId, club.iId);

      // get already negotiated contract (salary)
      if (offer.contract.iLength > 0/* && offer.contract.iLength == iYears*/) {
        if (offer.contract.iLength == iYears) {
          iSalary = offer.contract.iSalary;
        } else {
          double fFactorNego = offer.contract.iSalary / (double)MvcApplication.ckcore.plr.getSalary(MvcApplication.ckcore.ltPlayer[iPlayerId], offer.contract.iLength);
          iSalary = (int)(iSalary * fFactorNego);
        }
        fMood   = offer.contract.fPlayerMood;
      } else { // new offer
        offer.iClubId = club.iId;
        offer.dt = MvcApplication.ckcore.dtDatum;
        offer.contract.iLength = iYears;
      }
      /*
      foreach (CornerkickCore.csTransfer.Transfer transfer in MvcApplication.ckcore.ltTransfer) {
        if (transfer.iPlayerId == iPlayerId) {
          if (transfer.ltOffers == null) break;

          foreach (CornerkickCore.csTransfer.TransferOffer offer in transfer.ltOffers) {
            if (offer.iClubId == club.iId) {
              iSalary = offer.contract.iSalary;
              fMood   = offer.contract.fPlayerMood;

              break;
            }
          }

          break;
        }
      }
      */

      // negotiate
      if (iSalaryOffer > 0) {
        if (offer.contractOffered.iSalary > 0 && iSalaryOffer < offer.contractOffered.iSalary) {
          fMood = -1f;
        } else {
          offer.contractOffered.iLength = iYears;
          offer.contractOffered.iSalary = iSalaryOffer;
          MvcApplication.ckcore.tl.negotiate(ref fMood, ref iSalary, iSalaryOffer);
        }
      }

      if (fMood < 0.1) fMood = -1f; // negotiation cancelled

      offer.contract.fPlayerMood = fMood;
      offer.contract.iSalary     = iSalary;

      if (iSalaryOffer > 0) MvcApplication.ckcore.tr.addChangeOffer(iPlayerId, offer);

      return Json(offer.contract, JsonRequestBehavior.AllowGet);
      //return Json(iSalary.ToString("#,#", AccountController.ciUser), JsonRequestBehavior.AllowGet);
    }

    // iMode: 0 - Extention, 1 - new contract
    [HttpPost]
    public JsonResult NegotiatePlayerContract(int iId, int iYears, string sSalary, string sPlayerMood, int iMode)
    {
      if (iId    < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iYears < 1) return Json("0",     JsonRequestBehavior.AllowGet);

      // convert salary to int
      sSalary = sSalary.Replace("€", string.Empty);
      sSalary = sSalary.Replace(".", string.Empty);
      sSalary = sSalary.Trim();

      int iSalary = 0;
      if (!int.TryParse(sSalary, out iSalary)) return Json("Error", JsonRequestBehavior.AllowGet);

      // convert player mood to double
      sPlayerMood = sPlayerMood.Replace("%", string.Empty);
      sPlayerMood = sPlayerMood.Replace(".", string.Empty);
      sPlayerMood = sPlayerMood.Trim();
      double fPlayerMood = 1.0;
      if (!double.TryParse(sPlayerMood, out fPlayerMood)) return Json("Error", JsonRequestBehavior.AllowGet);
      fPlayerMood /= 100.0;

      string sReturn = "";
      if (iMode == 0) { // contract extention
        CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iId];

        if (player.contract.iLength + iYears > 9) return Json("Error: Maximale Vertragslänge = 10 Jahre", JsonRequestBehavior.AllowGet);

        player.contract.iLength += (byte)iYears;
        player.contract.iSalary = iSalary;
        player.contract.fPlayerMood = fPlayerMood;
        MvcApplication.ckcore.ltPlayer[iId] = player;

        sReturn = "Der Vertrag mit " + player.sName + " wurde um " + iYears.ToString() + " Jahre verlängert.";
      } else { // new contract
        if (iYears > 9) return Json("Error: Maximale Vertragslänge = 10 Jahre", JsonRequestBehavior.AllowGet);

        // create new offer
        CornerkickCore.csTransfer.TransferOffer offer = new CornerkickCore.csTransfer.TransferOffer();
        CornerkickGame.Player.Contract contract = new CornerkickGame.Player.Contract();
        contract.iLength = (byte)iYears;
        contract.iSalary = iSalary;
        contract.fPlayerMood = fPlayerMood;
        offer.contract = contract;
        offer.iClubId = AccountController.ckClub().iId;

        MvcApplication.ckcore.tr.addChangeOffer(iId, offer);
        sReturn = "Sie haben sich mit dem Spieler " + MvcApplication.ckcore.ltPlayer[iId].sName + " auf eine Zusammenarbeit über " + iYears.ToString() + " Jahre geeinigt.";
        /*
        for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
          CornerkickCore.csTransfer.Transfer transfer = MvcApplication.ckcore.ltTransfer[iT];

          if (transfer.iPlayerId == iId) {
            if (transfer.ltOffers == null) transfer.ltOffers = new List<CornerkickCore.csTransfer.TransferOffer>();

            // Remove offer if already exist
            for (int iO = 0; iO < transfer.ltOffers.Count; iO++) {
              if (transfer.ltOffers[iO].iClubId == offer.iClubId) {
                transfer.ltOffers.RemoveAt(iO);
                break;
              }
            }

            transfer.ltOffers.Add(offer);
            MvcApplication.ckcore.ltTransfer[iT] = transfer;

            sReturn = "Sie haben sich mit dem Spieler " + MvcApplication.ckcore.ltPlayer[transfer.iPlayerId].sName + " auf eine Zusammenarbeit über " + iYears.ToString() + " Jahre geeinigt.";
          }
        }
        */
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult ExtentPlayerContract(int iPlayer, int iYears, string sSalary)
    {
      if (iPlayer < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iYears  < 1) return Json("0", JsonRequestBehavior.AllowGet);

      sSalary = sSalary.Replace("€", string.Empty);
      sSalary = sSalary.Replace(".", string.Empty);
      sSalary = sSalary.Trim();

      int iSalary = 0;
      if (!int.TryParse(sSalary, out iSalary)) return Json("Error", JsonRequestBehavior.AllowGet);

      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      //uint iSallery = MvcApplication.ckcore.plr.getSalary(player, (byte)iYears, 0);
      player.contract.iLength += (byte)iYears;
      player.contract.iSalary = iSalary;
      MvcApplication.ckcore.ltPlayer[iPlayer] = player;

      return Json("Der Vertrag mit " + player.sName + " wurde um " + iYears.ToString() + " Jahre verlängert.", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult GetPlayerName(int iPlayer)
    {
      if (iPlayer < 0) return Json("", JsonRequestBehavior.AllowGet);

      return Json(MvcApplication.ckcore.ltPlayer[iPlayer].sName, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Jouth
    /// </summary>
    /// <param name="Jouth"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    //[Authorize]
    public ActionResult Jouth(Models.JouthModel jouth)
    {
      int iC = AccountController.ckUser().iTeam;

      Models.JouthModel.ltPlayerJouth = new List<CornerkickGame.Player>();

      if (MvcApplication.ckcore.ltClubs.Count > iC) {
        foreach (int iSp in MvcApplication.ckcore.ltClubs[iC].ltJugendspielerID) {
          CornerkickGame.Player sp = MvcApplication.ckcore.ltPlayer[iSp];

          /*
          // Change Birthday if too young
          if (MvcApplication.ckcore.game.tl.getPlayerAgeFloat(sp, MvcApplication.ckcore.dtDatum) < 15) {
            sp.dtGeburt = new DateTime(sp.dtGeburt.Year - 5, sp.dtGeburt.Month, sp.dtGeburt.Day);
            MvcApplication.ckcore.ltPlayer[iSp] = sp;
          }
          */

          Models.JouthModel.ltPlayerJouth.Add(sp);
        }
      }

      return View(jouth);
    }

    [HttpPost]
    public JsonResult TakeJouth(int iJouthID)
    {
      if (iJouthID < 0) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickCore.Core.Club clb = AccountController.ckClub();

      clb.ltJugendspielerID.Remove(iJouthID);
      clb.ltPlayerId.Add(iJouthID);

      CornerkickGame.Player sp = MvcApplication.ckcore.ltPlayer[iJouthID];

      sp.contract.iLength = 2;
      sp.contract.iSalary = MvcApplication.ckcore.plr.getSalary(sp, sp.contract.iLength, MvcApplication.ckcore.fz.iMoneyTotal());

      MvcApplication.ckcore.ltPlayer[iJouthID] = sp;

      return Json("", JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Transfer
    /// </summary>
    /// <param name="Transfer"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult Transfer(Models.TransferModel transfer)
    {
      transfer.iContractYears = 1;
      return View(transfer);
    }

    [HttpPost]
    public JsonResult PutOnTransferList(int iPlayerId)
    {
      MvcApplication.ckcore.ui.putPlayerOnTransferlist(iPlayerId, 0);

      return Json("Der Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " wurde auf die Transferliste gesetzt", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult TakeFromTransferList(int iPlayerId)
    {
      for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
        CornerkickCore.csTransfer.Transfer transfer = MvcApplication.ckcore.ltTransfer[iT];

        if (transfer.iPlayerId == iPlayerId) {
          MvcApplication.ckcore.ltTransfer.RemoveAt(iT);
          break;
        }
      }

      return Json("Der Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " wurde von der Transferliste genommen", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult MakeTransferOffer(int iPlayerId, int iTransferFee)
    {
      string sReturn = "Error";

      CornerkickCore.Core.Club club = AccountController.ckClub();

      int iClub = club.iId;

      for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
        CornerkickCore.csTransfer.Transfer transfer = MvcApplication.ckcore.ltTransfer[iT];

        if (transfer.iPlayerId == iPlayerId) {
          if (transfer.ltOffers != null) {
            for (int iO = 0; iO < transfer.ltOffers.Count; iO++) {
              CornerkickCore.csTransfer.TransferOffer offer = transfer.ltOffers[iO];
              if (offer.iClubId == iClub) {
                if (!MvcApplication.ckcore.fz.checkDispoLimit(iTransferFee, club)) {
                  transfer.ltOffers.Remove(offer);
                  return Json("Ihr Kreditrahmen ist leider nicht hoch genug", JsonRequestBehavior.AllowGet);
                }

                offer.dt = MvcApplication.ckcore.dtDatum;
                offer.iFee = iTransferFee;

                transfer.ltOffers[iO] = offer;

                MvcApplication.ckcore.ltTransfer[iT] = transfer;

                int iClubPlayer = MvcApplication.ckcore.ltPlayer[iPlayerId].iClubId;
                if (iClubPlayer >= 0) {
                  int iUserPlayer = MvcApplication.ckcore.ltClubs[iClubPlayer].iUser;
                  if (iUserPlayer >= 0) {
                    MvcApplication.ckcore.Info(iUserPlayer, "Sie haben ein neues Transferangebot für den Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " erhalten!", 1, iPlayerId, 0);
                  }
                }

                CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayerId];
                player.character.fMoney += 0.05f;
                MvcApplication.ckcore.ltPlayer[iPlayerId] = player;

                sReturn = "Sie haben das Transferangebot für dem Spieler " + MvcApplication.ckcore.ltPlayer[transfer.iPlayerId].sName + " erfolgreich abgegeben.";
                break;
              }
            }
          }
        }
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult AcceptTransferOffer(int iPlayerId, int iClubId)
    {
      string sReturn = "Error";

      CornerkickCore.Core.Club clubTake = MvcApplication.ckcore.ltClubs[iClubId];
      if (MvcApplication.ckcore.ui.acceptTransferOffer(AccountController.ckClub(), iPlayerId, clubTake)) {
        sReturn = "Sie haben das Transferangebot für dem Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " angenommen. Er wechselt mit sofortiger Wirkung zu " + clubTake.sName;
      }
      /*
      for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
        CornerkickCore.csTransfer.Transfer transfer = MvcApplication.ckcore.ltTransfer[iT];

        if (transfer.iPlayerId == iPlayerId) {
          if (transfer.ltOffers != null) {
            for (int iO = 0; iO < transfer.ltOffers.Count; iO++) {
              CornerkickCore.csTransfer.TransferOffer offer = transfer.ltOffers[iO];
              if (offer.iClubId == iClubId) {
                CornerkickCore.Core.Club clubUser = AccountController.ckClub();
                clubUser.iKontostand += offer.iMoney;
                clubUser.ltPlayerId.Remove(iPlayerId);
                MvcApplication.ckcore.fz.setKonto(ref clubUser, MvcApplication.ckcore.dtDatum, +offer.iMoney, "Spielertransfer");
                AccountController.setCkClub(clubUser);

                CornerkickCore.Core.Club clubTake = MvcApplication.ckcore.ltClubs[iClubId];
                clubTake.iKontostand -= offer.iMoney;
                clubTake.ltPlayerId.Add(iPlayerId);
                MvcApplication.ckcore.fz.setKonto(ref clubTake, MvcApplication.ckcore.dtDatum, -offer.iMoney, "Spielertransfer");
                MvcApplication.ckcore.Info("Ihr Transferangebot für den Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " von " + offer.iMoney.ToString("#,#", AccountController.ciUser) + " wurde angenommen!", clubTake.iUser, 3, 0, clubTake.iUser);
                MvcApplication.ckcore.ltClubs[iClubId] = clubTake;

                MvcApplication.ckcore.ltTransfer.Remove(transfer);

                sReturn = "Sie haben das Transferangebot für dem Spieler " + MvcApplication.ckcore.ltPlayer[transfer.iPlayerId].sName + " angenommen. Er wechselt mit sofortiger Wirkung zu " + clubTake.sName;

                return Json(sReturn, JsonRequestBehavior.AllowGet);
              }
            }
          }
        }
      }
      */

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult CancelTransferOffer(int iPlayerId)
    {
      string sReturn = "Error";

      if (MvcApplication.ckcore.ui.cancelTransferOffer(iPlayerId, AccountController.ckClub())) {
        CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayerId];
        player.character.fMoney -= 0.05f;
        sReturn = "Sie haben Ihr Transferangebot für dem Spieler " + player.sName + " zurückgezogen.";
        MvcApplication.ckcore.ltPlayer[iPlayerId] = player;
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    public ActionResult getTableTransfer(int iPos, int iFType, int iFValue, bool bJouth = false)
    {
      //The table or entity I'm querying
      List<Models.DatatableEntryTransfer> ltDeTransfer = new List<Models.DatatableEntryTransfer>();

      int iTr = 0;
      foreach (CornerkickCore.csTransfer.Transfer transfer in MvcApplication.ckcore.ui.filterTransferlist("", -1, iPos, -1f, 0, iFType, iFValue, bJouth)) {
        CornerkickGame.Player spTr = MvcApplication.ckcore.ltPlayer[transfer.iPlayerId];

        CornerkickCore.Core.Club club = MvcApplication.ckcore.ltClubs[spTr.iClubId];

        string sClub = "vereinslos";
        if (spTr.iClubId >= 0) {
          sClub = club.sName;
        }

        int iOffer = 0;
        CornerkickCore.Core.Club clubUser = AccountController.ckClub();
        if (clubUser.iId > 0) {
          if      (MvcApplication.ckcore.tr .negotiationCancelled(clubUser, spTr)) iOffer = -1;
          else if (MvcApplication.ckcore.tr .alreadyOffered      (clubUser, spTr)) iOffer = +1;
          else if (MvcApplication.ckcore.plr.ownPlayer           (clubUser, spTr)) iOffer = +2;
        }

        ltDeTransfer.Add(new Models.DatatableEntryTransfer {
          playerId = transfer.iPlayerId,
          empty = "",
          iOffer = iOffer,
          index = (iTr + 1).ToString(),
          datum = transfer.dt.ToString("d", AccountController.ciUser),
          name = spTr.sName,
          position = MvcApplication.ckcore.plr.getStrPos(spTr),
          strength = MvcApplication.ckcore.game.tl.getAveSkill(spTr, 0, true).ToString("0.0"),
          strengthIdeal = MvcApplication.ckcore.game.tl.getAveSkill(spTr, 0, false).ToString("0.0"),
          age = MvcApplication.ckcore.game.tl.getPlayerAge(spTr, MvcApplication.ckcore.dtDatum).ToString("0"),
          talent = (spTr.iTalent + 1).ToString(),
          mw = (MvcApplication.ckcore.plr.getValue(spTr) * 1000).ToString("#,#", AccountController.ciUser),
          club = sClub,
          nat = MvcApplication.ckcore.sLandShort[MvcApplication.ckcore.iNatUmrechnung[spTr.iNat1 + 1]]
        });

        iTr++;
      }

      return Json(new { aaData = ltDeTransfer.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult getTableTransferDetails2(int iPlayerId)
    {
      string sTable = "";

      CornerkickCore.Core.Club clubUser = AccountController.ckClub();

      sTable += "<table id=\"tableTransferDetails\" cellspacing=\"0\" style =\"width: auto\" class=\"display responsive nowrap\" > ";
      sTable += "<thead>";
      sTable += "<tr>";
      sTable += "<th>#</th>";
      sTable += "<th>Datum</th>";
      sTable += "<th>Verein</th>";
      sTable += "<th>Ablöse</th>";
      sTable += "<th></th>";
      sTable += "</tr>";
      sTable += "</thead>";
      sTable += "<tbody>";

      int iTr = 0;
      foreach (CornerkickCore.csTransfer.Transfer transfer in MvcApplication.ckcore.ltTransfer) {
        if (transfer.iPlayerId == iPlayerId) {
          if (transfer.ltOffers == null) break;

          bool bOwnPlayer = MvcApplication.ckcore.plr.ownPlayer(clubUser, MvcApplication.ckcore.ltPlayer[iPlayerId]);

          foreach (CornerkickCore.csTransfer.TransferOffer offer in transfer.ltOffers) {
            if (bOwnPlayer || // If own player
                clubUser.iId == offer.iClubId) {
              string sClub = "vereinslos";
              if (offer.iClubId >= 0) {
                sClub = MvcApplication.ckcore.ltClubs[offer.iClubId].sName;
              }

              string sStyle = "";
              if (offer.iClubId == clubUser.iId) sStyle = "font-weight:bold";

              sTable += "<tr id=\"rowTransferDetail_" + offer.iClubId.ToString() + "\" style=" + sStyle + ">";
              sTable += "<td>" + (iTr + 1).ToString() + "</td>";
              sTable += "<td align=\"center\">" + offer.dt.ToString("d", AccountController.ciUser) + "</td>";
              sTable += "<td align=\"center\">" + sClub + "</td>";
              sTable += "<td align=\"right\">" + offer.iFee.ToString("#,#", AccountController.ciUser) + " €" + "</td>";

              if (bOwnPlayer) {
                string sChecked = "";
                //if (iTr == 0) sChecked = " checked";
                sTable += "<td><input type=\"radio\" id=\"rB_OfferClubId_" + iTr.ToString() + "\" name=\"OfferClubId\" onclick=\"handleClick(this);\" value =\"" + offer.iClubId.ToString() + "\"" + sChecked + "></td>";
              }

              sTable += "</tr>";

              iTr++;
            }
          }

          break;
        }
      }

      sTable += "</tbody>";
      sTable += "</table>";

      // Reset sTable if no offers
      if (iTr == 0) sTable = "";

      return Json(sTable, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Taktik
    /// </summary>
    /// <param name="Taktik"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////

    //[Authorize]
    public ActionResult Taktik(Models.TaktikModel tactic)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      string[] sStandards = new string[4] { "11m", "Freistoß", "Ecke R.", "Ecke L." };

      tactic.ltDdlStandards = new List<SelectListItem>[4];
      for (byte iS = 0; iS < 4; iS++) {
        tactic.ltDdlStandards[iS] = new List<SelectListItem>();
        tactic.ltDdlStandards[iS].Add(new SelectListItem { Text = "auto (" + sStandards[iS] + ")", Value = "-1" });
        for (byte iPl = 0; iPl < MvcApplication.ckcore.game.nPlStart; iPl++) {
          if (clb.ltPlayerId.Count <= iPl) break;

          CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[clb.ltPlayerId[iPl]];
          tactic.ltDdlStandards[iS].Add(new SelectListItem { Text = pl.sName, Value = iPl.ToString(), Selected = iPl == clb.taktik.iStandards[iS] });
        }
      }

      //fillDdlAutoSubs(taktik);
      const byte nSubs = 3;
      tactic.ddlAutoSubsOut = new List<SelectListItem>[nSubs];
      tactic.ddlAutoSubsIn  = new List<SelectListItem>[nSubs];
      tactic.iAutoSubsMin   = new int                 [nSubs];

      if (clb.nextGame != null) {
        byte iHA = 0;
        if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;
        for (byte iAS = 0; iAS < nSubs; iAS++) {
          tactic.iAutoSubsMin[iAS] = 60;
          if (iAS < clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) tactic.iAutoSubsMin[iAS] = clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][2];
        }
      }

      return View(tactic);
    }

    public ActionResult setTaktik(int iTaktik, float fTaktik)
    {
      float fRet = 0f;

      CornerkickCore.Core.User usr = AccountController.ckUser();
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      if      (iTaktik == 0) clb.taktik.fOrientation = fTaktik;
      else if (iTaktik == 1) clb.taktik.fPower       = fTaktik;
      else if (iTaktik == 2) clb.taktik.fShootFreq   = fTaktik;
      else if (iTaktik == 3) clb.taktik.fAggressive  = fTaktik;
      else if (iTaktik == 4) clb.taktik.fPassRisk    = fTaktik;
      else if (iTaktik == 5) clb.taktik.fPassLength  = fTaktik;
      else if (iTaktik == 6) clb.taktik.fPassFreq    = fTaktik;
      else if (iTaktik == 7) {
        clb.taktik.fPassLeft  = fTaktik;
        if (clb.taktik.fPassLeft + clb.taktik.fPassRight > 1f) clb.taktik.fPassRight = (float)Math.Round(1f - clb.taktik.fPassLeft,  2);
        fRet = clb.taktik.fPassRight;
      }
      else if (iTaktik == 8) {
        clb.taktik.fPassRight = fTaktik;
        if (clb.taktik.fPassLeft + clb.taktik.fPassRight > 1f) clb.taktik.fPassLeft  = (float)Math.Round(1f - clb.taktik.fPassRight, 2);
        fRet = clb.taktik.fPassLeft;
      }

      // Set tactic of current game
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].tc = clb.taktik;
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].tc = clb.taktik;
        }
      }

      return Json(fRet, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setStandards(int iStandard, int iIndexPlayer)
    {
      CornerkickCore.Core.User usr = AccountController.ckUser();
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      clb.taktik.iStandards[iStandard] = iIndexPlayer;

      // Set tactic of current game
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].tc = clb.taktik;
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].tc = clb.taktik;
        }
      }

      return Json("", JsonRequestBehavior.AllowGet);
    }

    public void fillDdlAutoSubs(Models.TaktikModel tactic)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      if (clb.nextGame == null) return;

      const byte nSubs = 3;
      tactic.ddlAutoSubsOut = new List<SelectListItem>[nSubs];
      tactic.ddlAutoSubsIn  = new List<SelectListItem>[nSubs];
      tactic.iAutoSubsMin   = new int                 [nSubs];
      byte iHA = 0;
      if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;
      for (byte iAS = 0; iAS < nSubs; iAS++) {
        tactic.ddlAutoSubsOut[iAS] = new List<SelectListItem>();
        tactic.ddlAutoSubsIn [iAS] = new List<SelectListItem>();

        tactic.ddlAutoSubsOut[iAS].Add(new SelectListItem { Text = "aus", Value = "-1" });
        tactic.ddlAutoSubsIn [iAS].Add(new SelectListItem { Text = "aus", Value = "-1" });

        for (byte iPl = 0; iPl < clb.ltPlayerId.Count; iPl++) {
          CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[clb.ltPlayerId[iPl]];

          bool bSelected = false;
          if (iAS < clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) {
            if (iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][0] || iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][1]) bSelected = true;
          }

          string sPos = "";
          if (iPl < MvcApplication.ckcore.game.nPlStart) sPos = MvcApplication.ckcore.sPosition[MvcApplication.ckcore.game.tl.getBasisPos(MvcApplication.ckcore.game.tl.getPosRole(pl))];
          else                                           sPos = MvcApplication.ckcore.plr.getStrPos(pl);
          string sStrength = MvcApplication.ckcore.game.tl.getAveSkill(pl, 0, true).ToString(" (0.0)");
          SelectListItem sliAutoSub = new SelectListItem { Text = pl.sName + " - " + sPos + sStrength,
                                                           Value = iPl.ToString(),
                                                           Selected = bSelected
                                                         };

          if (iPl < MvcApplication.ckcore.game.nPlStart) {
            if (!checkIfAlreadyInDdl(iPl, tactic.ddlAutoSubsOut, iAS)) tactic.ddlAutoSubsOut[iAS].Add(sliAutoSub);
          } else {
            if (!checkIfAlreadyInDdl(iPl, tactic.ddlAutoSubsIn,  iAS)) tactic.ddlAutoSubsIn[iAS].Add(sliAutoSub);
          }
        }

        tactic.iAutoSubsMin[iAS] = 60;
        if (iAS < clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) tactic.iAutoSubsMin[iAS] = clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][2];
      }
    }

    private bool checkIfAlreadyInDdl(int iPl, List<SelectListItem>[] ddlAutoSubs, byte iAsCurrent)
    {
      for (int jAS = iAsCurrent - 1; jAS >= 0; jAS--) {
        foreach (SelectListItem sliAutoSubTmp in ddlAutoSubs[jAS]) {
          if (sliAutoSubTmp.Value.Equals(iPl.ToString())) {
            if (sliAutoSubTmp.Selected) return true;
            break;
          }
        }
      }

      return false;
    }

    public JsonResult getHtmlAutoSubOut(Models.TaktikModel tactic, byte iAS)
    {
      string[] sBox = new string[2]; // [Out, In]

      CornerkickCore.Core.Club clb = AccountController.ckClub();

      if (clb.nextGame == null) Json(sBox, JsonRequestBehavior.AllowGet);

      byte iHA = 0;
      if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;

      sBox[0] += "<select name=\"sAutoSubsOut" + iAS.ToString() + "\" class=\"form-control\" id =\"ddlAutoSubOut" + iAS.ToString() + "\" onchange =\"setAutoSubs(" + iAS.ToString() + ")\"><option value=\"-1\">aus</option>";
      sBox[1] += "<select name=\"sAutoSubsIn" + iAS.ToString() + "\" class=\"form-control\" id =\"ddlAutoSubIn" + iAS.ToString() + "\" onchange =\"setAutoSubs(" + iAS.ToString() + ")\"><option value=\"-1\">aus</option>";

      // foreach player
      for (int iPl = 0; iPl < MvcApplication.ckcore.game.nPlStart + MvcApplication.ckcore.game.nPlRes; iPl++) {
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[clb.ltPlayerId[iPl]];

        bool bOut = iPl < MvcApplication.ckcore.game.nPlStart;

        bool bContinue = false;
        byte jAS = 0;
        while (jAS < iAS) {
          if ( bOut && iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][0]) bContinue = true;
          if (!bOut && iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][1]) bContinue = true;
          jAS++;
        }
        if (bContinue) continue;

        string sSelectedO = "";
        string sSelectedI = "";
        if (iAS < clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count) {
          if (iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][0]) sSelectedO = " selected=\"selected\"";
          if (iPl == clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][1]) sSelectedI = " selected=\"selected\"";
        }

        string sPos = "";
        if (iPl < MvcApplication.ckcore.game.nPlStart) sPos = MvcApplication.ckcore.sPosition[MvcApplication.ckcore.game.tl.getBasisPos(MvcApplication.ckcore.game.tl.getPosRole(pl))];
        else                                           sPos = MvcApplication.ckcore.plr.getStrPos(pl);
        string sStrength = MvcApplication.ckcore.game.tl.getAveSkill(pl).ToString(" (0.0)");

        if (bOut) sBox[0] += "<option" + sSelectedO + " value=\"" + iPl.ToString() + "\">" + pl.sName + " - " + sPos + sStrength + "</option>";
        else      sBox[1] += "<option" + sSelectedI + " value=\"" + iPl.ToString() + "\">" + pl.sName + " - " + sPos + sStrength + "</option>";
      }

      sBox[0] += "</select>";
      sBox[1] += "</select>";

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setAutoSubs(int iAS, int iIndexPlayerOut, int iIndexPlayerIn, int iMin)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      if (clb.nextGame == null) Json(false, JsonRequestBehavior.AllowGet);

      byte iHA = 0;
      if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;

      while (clb.nextGame.team[iHA].ltSubstitutionsPlanned.Count <= iAS) clb.nextGame.team[iHA].ltSubstitutionsPlanned.Add(new int[3] { -1, -1, -1 });

      clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][0] = iIndexPlayerOut;
      clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][1] = iIndexPlayerIn;
      clb.nextGame.team[iHA].ltSubstitutionsPlanned[iAS][2] = iMin;

      bool bValid = iIndexPlayerOut >= 0 && iIndexPlayerIn >= 0 && iMin >= 0;

      if (!bValid) {
        int jAS = iAS + 1;
        while (jAS < 3) {
          clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][0] = -1;
          clb.nextGame.team[iHA].ltSubstitutionsPlanned[jAS][1] = -1;
          jAS++;
        }
      }

      return Json(bValid, JsonRequestBehavior.AllowGet);
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
      CornerkickCore.Core.Club clb = AccountController.ckClub();
      clb.training.iTraining[iTag] = (byte)iTraining;
      return Json(iTraining, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Stadion
    /// </summary>
    /// <param name="Stadion"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult Stadion(Models.StadionModel stadionModel)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      if (clb.stadium.blocks == null) return View(stadionModel);

      stadionModel.stadion = clb.stadium;

      stadionModel.sName = clb.stadium.sName;

      stadionModel.iSeats = new int[clb.stadium.iSeats.Length];
      for (int i = 0; i < clb.stadium.blocks.Length; i++) {
        stadionModel.iSeats[i] = clb.stadium.blocks[i].iSeats;
      }

      stadionModel.iSeatType = new int[clb.stadium.iType.Length];
      for (int i = 0; i < clb.stadium.iType.Length; i++) {
        stadionModel.iSeatType[i] = clb.stadium.blocks[i].iType;
      }

      stadionModel.iSeatsBuild = new int[clb.stadium.iAusbauPl.Length];
      for (int i = 0; i < clb.stadium.iAusbauPl.Length; i++) {
        stadionModel.iSeatsBuild[i] = clb.stadium.blocks[i].iDaysConstructSeats;
      }

      stadionModel.bOberring = bStadiumGetTopring(clb);

      stadionModel.iVideo = clb.stadium.iVideo;

      stadionModel.stadionNew = convertToStadion(stadionModel.iSeats, stadionModel.iSeatType, stadionModel.iSeatsBuild);

      /*
      stModel.stadionNew = MvcApplication.ckcore.ini.newStadion();
      for (int i = 0; i < stModel.stadionNew.iPlaetze.Length; i++) {
        stModel.stadionNew.iPlaetze[i] = clb.stadium.iSeats[i];
      }

      if (stadionModel.iSeats == null) {
        stadionModel.iSeats = new int[clb.stadium.iSeats.Length];
        for (int i = 0; i < clb.stadium.iSeats.Length; i++) {
          stadionModel.iSeats[i] = clb.stadium.iSeats[i];
        }
      }
      */

      return View(stadionModel);
    }

    /*
    public JsonResult StadionChange(Models.StadionModel stadionModel, string sId, int iSeats)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      byte iB = 255;
      string[] sB = sId.Split('_');
      if (sB.Length < 1) return Json(0, JsonRequestBehavior.AllowGet);

      byte.TryParse(sB[sB.Length - 1], out iB);
      if (iB > stadionModel.stadion.iPlaetze.Length) return Json(0, JsonRequestBehavior.AllowGet);
      stadionModel.stadion.iPlaetze[iB] = iSeats;

      UpdateModel(stadionModel);

      int iKosten = MvcApplication.ckcore.tl.iKostenStadionAusbau(stadionModel.stadion, clb.stadium);

      return Json(iKosten, JsonRequestBehavior.AllowGet);
      //return View(stadionModel);
    }
    */

    [HttpPost]
    public JsonResult StadiumChange(int[] iSeats, int[] iArt)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      CornerkickGame.Stadium stadium = convertToStadion(iSeats, iArt, null);
      stadium.bTopring = clb.stadium.bTopring;

      int[] iKostenDauer = MvcApplication.ckcore.st.iCostDaysContructStadium(stadium, clb.stadium, AccountController.ckUser());
      int iDispoOk = 0;
      if (MvcApplication.ckcore.fz.checkDispoLimit(iKostenDauer[0], clb)) iDispoOk = 1;

      string[] sKostenDauer = new string[] { iKostenDauer[0].ToString("#,#", AccountController.ciUser), iKostenDauer[1].ToString(), iDispoOk.ToString() };

      return Json(sKostenDauer, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumChangeSet(int[] iSeats, int[] iArt)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      CornerkickGame.Stadium stadium = convertToStadion(iSeats, iArt, null);

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadium);

      return Json("Der Ausbau des Stadions wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetCostTopring()
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      CornerkickGame.Stadium stadion = clb.stadium;
      stadion.bTopring = !clb.stadium.bTopring;

      int[] iKostenDauer = MvcApplication.ckcore.st.iCostDaysContructStadium(stadion, clb.stadium, AccountController.ckUser());
      int iDispoOk = 0;
      if (MvcApplication.ckcore.fz.checkDispoLimit(iKostenDauer[0], clb)) iDispoOk = 1;

      string[] sKostenDauer = new string[] { iKostenDauer[0].ToString("#,#", AccountController.ciUser), iKostenDauer[1].ToString(), iDispoOk.ToString() };

      return Json(sKostenDauer, JsonRequestBehavior.AllowGet);
    }

    private bool bStadiumGetTopring(CornerkickCore.Core.Club clb)
    {
      return clb.stadium.bTopring && clb.stadium.iDaysConstructTopring == 0;
    }

    [HttpPost]
    public JsonResult StadiumGetTopring()
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();
      return Json(bStadiumGetTopring(clb), JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildTopring()
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      CornerkickGame.Stadium stadion = clb.stadium;
      stadion.bTopring = !clb.stadium.bTopring;

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadion);

      return Json("Der Bau des Oberrings wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetCostVideo(byte iVideo)
    {
      string[] sCostDaysDispo = new string[3] { "0", "0", "0" };
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      if (clb.stadium.iVideo != iVideo) {
        int iDispoOk = 0;
        if (MvcApplication.ckcore.fz.checkDispoLimit(MvcApplication.ckcore.st.iVideoCost[iVideo], clb)) iDispoOk = 1;

        sCostDaysDispo[0] = MvcApplication.ckcore.st.iVideoCost[iVideo].ToString("#,#", AccountController.ciUser);
        sCostDaysDispo[1] = MvcApplication.ckcore.st.iVideoDaysConstruct[iVideo].ToString();
        sCostDaysDispo[2] = iDispoOk.ToString();
      }

      return Json(sCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildVideo(byte iVideo)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      CornerkickGame.Stadium stadion = clb.stadium;
      stadion.iVideo = iVideo;

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadion);

      return Json("Der Bau der Anzeigentafel wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    internal CornerkickGame.Stadium convertToStadion(int[] iSeats, int[] iSeatType, int[] iSeatsBuild)
    {
      CornerkickGame.Stadium stadium = MvcApplication.ckcore.ini.newStadium();
      if (iSeats != null) {
        for (int i = 0; i < iSeats.Length; i++) stadium.blocks[i].iSeats = iSeats[i];
      }
      if (iSeatType != null) {
        for (int i = 0; i < iSeatType.Length; i++) stadium.blocks[i].iType = (byte)iSeatType[i];
      }
      if (iSeatsBuild != null) {
        for (int i = 0; i < iSeatsBuild.Length; i++) stadium.blocks[i].iDaysConstructSeats = iSeatsBuild[i];
      }

      return stadium;
    }

    [HttpPost]
    public JsonResult StadiumRenewPitchCost()
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      return Json(MvcApplication.ckcore.st.iCostStadiumRenewPitch(clb.stadium, 0.1f, AccountController.ckUser()).ToString("#,#", AccountController.ciUser), JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumRenewPitch()
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      MvcApplication.ckcore.ui.renewStadiumPitch(ref clb, 0.1f);

      if (clb.iUser >= 0) MvcApplication.ckcore.ltClubs[MvcApplication.ckcore.ltUser[clb.iUser].iTeam] = clb;

      return Json("Der Stadionrasen wurde erneuert", JsonRequestBehavior.AllowGet);
    }

    public void StadiumSetName(string sName)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();
      clb.stadium.sName = sName;
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Stadion
    /// </summary>
    /// <param name="Stadion"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult StadiumSurroundings(Models.StadiumSurroundingsModel mdStadionSurr)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      mdStadionSurr.iTrainingsgel  = clb.iTrainingsgel  [0];
      mdStadionSurr.iJouthInternat = clb.iJugendinternat[0];
      mdStadionSurr.iParkp         = Math.Max(clb.stadium.iParkp, clb.stadium.iParkpNeu);
      mdStadionSurr.iParkpNeu      = clb.stadium.iParkpNeu;

      return View(mdStadionSurr);
    }

    [HttpPost]
    public JsonResult StadiumGetSurrGetCurrent()
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      return Json(new string[3][] {
        new string [3] { MvcApplication.ckcore.sTrainingsgel [clb.iTrainingsgel  [0]], MvcApplication.ckcore.sTrainingsgel [clb.iTrainingsgel  [1]], clb.iTrainingsgel  [2].ToString() },
        new string [3] { MvcApplication.ckcore.sJouthInternat[clb.iJugendinternat[0]], MvcApplication.ckcore.sJouthInternat[clb.iJugendinternat[1]], clb.iJugendinternat[2].ToString() },
        new string [3] { clb.stadium.iParkp.ToString(), clb.stadium.iParkpNeu.ToString(), clb.stadium.iAusbauParkp.ToString() }
      }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetCostSurr(int i, byte iType)
    {
      string[] sCostDaysDispo = new string[3] { "0", "0", "0" };
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      if (iType == 0) {
        if (clb.iTrainingsgel[0] != i) {
          int iDispoOk = 0;
          if (MvcApplication.ckcore.fz.checkDispoLimit(MvcApplication.ckcore.iTrainingsgelCost[i], clb)) iDispoOk = 1;

          sCostDaysDispo[0] = MvcApplication.ckcore.iTrainingsgelCost[i].ToString("#,#", AccountController.ciUser);
          sCostDaysDispo[1] = MvcApplication.ckcore.iTrainingsgelDaysConstruct[i].ToString();
          sCostDaysDispo[2] = iDispoOk.ToString();
        }
      } else if (iType == 1) {
        if (clb.iJugendinternat[0] != i) {
          int iDispoOk = 0;
          if (MvcApplication.ckcore.fz.checkDispoLimit(MvcApplication.ckcore.iJouthInternatCost[i], clb)) iDispoOk = 1;

          sCostDaysDispo[0] = MvcApplication.ckcore.iJouthInternatCost[i].ToString("#,#", AccountController.ciUser);
          sCostDaysDispo[1] = MvcApplication.ckcore.iJouthInternatDaysConstruct[i].ToString();
          sCostDaysDispo[2] = iDispoOk.ToString();
        }
      } else if (iType == 2) {
        if (clb.stadium.iParkpNeu != i) {
          int iDispoOk = 0;
          int iParkDiff = Math.Abs(clb.stadium.iParkp - i);
          int iCost = iParkDiff * 2500;
          int iDays = (int)(iParkDiff * 0.25);
          if (MvcApplication.ckcore.fz.checkDispoLimit(iCost, clb)) iDispoOk = 1;

          sCostDaysDispo[0] = iCost.ToString("#,#", AccountController.ciUser);
          sCostDaysDispo[1] = iDays.ToString();
          sCostDaysDispo[2] = iDispoOk.ToString();
        }
      }

      return Json(sCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildTrGel(int iTrGel)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      clb.iTrainingsgel[1] = iTrGel;
      clb.iTrainingsgel[2] = MvcApplication.ckcore.iTrainingsgelDaysConstruct[iTrGel];

      MvcApplication.ckcore.fz.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -MvcApplication.ckcore.iTrainingsgelCost[iTrGel], "Bau Trainingsgelände", 110);

      return Json("Der Bau des Trainingsgeländes wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildJouthInt(byte iInt)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      clb.iJugendinternat[1] = iInt;
      clb.iJugendinternat[2] = MvcApplication.ckcore.iJouthInternatDaysConstruct[iInt];

      MvcApplication.ckcore.fz.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -MvcApplication.ckcore.iJouthInternatCost[iInt], "Bau Jugendinternat", 110);

      return Json("Der Bau des Jugendinternats wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildPark(int iPark)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      clb.stadium.iParkpNeu = iPark;

      int iParkDiff = Math.Abs(clb.stadium.iParkp - iPark);
      int iCost = iParkDiff * 2500;
      int iDays = (int)(iParkDiff * 0.25);

      clb.stadium.iAusbauParkp = iDays;

      MvcApplication.ckcore.fz.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -iCost, "Bau Parkplätze", 110);

      return Json("Der Bau der Parkplätze wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Personal
    /// </summary>
    /// <param name="Personal"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult Personal(Models.PersonalModel personal)
    {
      personal = new Models.PersonalModel();

      return View(personal);
    }

    public ActionResult setPersonal(int iPersonal, int iLevel)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();
      if      (iPersonal == 0) clb.personal.iTrainerCo =      (byte)iLevel;
      else if (iPersonal == 1) clb.personal.iTrainerKondi =   (byte)iLevel;
      else if (iPersonal == 2) clb.personal.iMasseur =        (byte)iLevel;
      else if (iPersonal == 3) clb.personal.iTrainerMental =  (byte)iLevel;
      else if (iPersonal == 4) clb.personal.iArzt =           (byte)iLevel;
      else if (iPersonal == 5) clb.personal.iJugendTrainer =  (byte)iLevel;
      else if (iPersonal == 6) clb.personal.iJugendScouting = (byte)iLevel;

      int iKosten = (int)(MvcApplication.ckcore.tl.getStuffSalary(clb) / 12f);
      string sKosten = "0";
      if (iKosten != 0) sKosten = iKosten.ToString("#,#", AccountController.ciUser);

      return Json(sKosten, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// ClubDetails
    /// </summary>
    /// <param name="Club Details"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult ClubDetails(Models.ClubModel mdClub, int iClub)
    {
      if (iClub >= 0 && iClub < MvcApplication.ckcore.ltClubs.Count) {
        mdClub.club = MvcApplication.ckcore.ltClubs[iClub];
      }
    
      mdClub.sRecordLWinH = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 1, +1, 0));
      mdClub.sRecordLWinA = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 1, +1, 1));
      mdClub.sRecordLDefH = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 1, -1, 0));
      mdClub.sRecordLDefA = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 1, -1, 1));

      mdClub.sRecordCWinH = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 2, +1, 0));
      mdClub.sRecordCWinA = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 2, +1, 1));
      mdClub.sRecordCDefH = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 2, -1, 0));
      mdClub.sRecordCDefA = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 2, -1, 1));

      return View(mdClub);
    }

    private string getStringRecordGame(CornerkickGame.Game.Data gdRecord)
    {
      if (gdRecord != null) {
        return gdRecord.team[0].iGoals.ToString() + ":" + gdRecord.team[1].iGoals.ToString() + " - " + gdRecord.team[0].sTeam + " vs. " + gdRecord.team[1].sTeam + ", " + gdRecord.dt.ToLongDateString();
      }

      return "-";
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// League
    /// </summary>
    /// <param name="league"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult League(Models.LeagueModels league)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();
      league.iLand = clb.iLand;
      league.iSpKl = clb.iDivision;

      league.ltScorer = MvcApplication.ckcore.ui.getScorer(1, league.iLand, league.iSpKl);

      league.iSpTg = MvcApplication.ckcore.tl.getMatchday(clb, MvcApplication.ckcore.dtDatum, 1);
      league.ltErg = MvcApplication.ckcore.tl.getLtErgLiga(league.iSaison, league.iLand, league.iSpKl, false);
      league.ltTbl = MvcApplication.ckcore.tl.getLeagueTable(league.iSaison, league.iLand, league.iSpKl, league.iSpTg - 1, 0);

      return View(league);
    }

    public JsonResult setLeague(Models.LeagueModels league, int iGameday)
    {
      league.ltTbl = MvcApplication.ckcore.tl.getLeagueTable(league.iSaison, league.iLand, league.iSpKl, iGameday, 0);

      return Json(league.ltTbl, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setLeagueTeams(Models.LeagueModels league, int iGameday)
    {
      league.ltErg = MvcApplication.ckcore.tl.getLtErgLiga(league.iSaison, league.iLand, league.iSpKl, false);

      return Json(league.ltErg[iGameday - 1], JsonRequestBehavior.AllowGet);
    }

    public ContentResult GetLeaguePlaceHistory()
    {
      CornerkickCore.Core.Club club = AccountController.ckClub();

      List<Models.DataPointGeneral> dataPoints = new List<Models.DataPointGeneral>();

      for (int iSpT = 1; iSpT < MvcApplication.ckcore.tl.iSpieltageGesFromClub(club); iSpT++) {
        int iPlace = MvcApplication.ckcore.tl.getLeaguePlace(club, iSpT);
        if (iPlace > 0) dataPoints.Add(new Models.DataPointGeneral(iSpT, iPlace));
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    [Authorize]
    public ActionResult Cup(Models.CupModel cupModel)
    {
      cupModel.ltErg = new List<List<CornerkickGame.Game.Data>>();

      CornerkickCore.Core.Club clb = AccountController.ckClub();
      cupModel.iLand = clb.iLand;

      cupModel.ltScorer = MvcApplication.ckcore.ui.getScorer(2, cupModel.iLand, 0);

      CornerkickCore.Core.Cup cup = MvcApplication.ckcore.tl.getCup(cupModel.iLand, 2);
      if (cup.ltMatchdays == null) return View(cupModel);

      foreach (CornerkickCore.Core.Matchday gd in cup.ltMatchdays) {
        cupModel.ltErg.Add(gd.ltGameData);
      }

      return View(cupModel);
    }

    public JsonResult setCup(Models.CupModel cupModel, int iGameday)
    {
      cupModel.ltErg = new List<List<CornerkickGame.Game.Data>>();

      CornerkickCore.Core.Cup cup = MvcApplication.ckcore.tl.getCup(cupModel.iLand, 2);
      if (cup.ltMatchdays == null) return Json("", JsonRequestBehavior.AllowGet);

      foreach (CornerkickCore.Core.Matchday gd in cup.ltMatchdays) {
        cupModel.ltErg.Add(gd.ltGameData);
      }

      return Json(cupModel.ltErg[iGameday - 1], JsonRequestBehavior.AllowGet);
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

      cal.ltTestgames = new List<Models.Testgame>();
      foreach (CornerkickCore.Core.Cup cup in MvcApplication.ckcore.ltCups) {
        if (cup.iId == -5) {
          foreach (CornerkickCore.Core.Matchday md in cup.ltMatchdays) {
            foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
              if (gd.team[1].iTeamId == AccountController.ckClub().iId) {
                Models.Testgame tg = new Models.Testgame();
                tg.dt = md.dt;
                tg.iTeamHome = gd.team[0].iTeamId;
                tg.iTeamAway = gd.team[1].iTeamId;
                cal.ltTestgames.Add(tg);
              }
            }
          }
        }
      }

      return View(cal);
    }

    public ActionResult PostCalendarData()
    {
      //var result = getCalendarEvents();

      var ApptListForDate = Models.DiaryEvent.getCalendarEvents();
      var eventList = from e in ApptListForDate
                      select new
                      {
                        id          = e.iID,
                        title       = e.sTitle,
                        description = e.sDescription,
                        start       = e.sStartDate,
                        end         = e.sEndDate,
                        color       = e.sColor,
                        editable    = e.bEditable,
                        allDay      = e.bAllDay
                      };

      var rows = eventList.ToArray();

      return Json(rows, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AddTestGameToCalendar(string title, string start, int iTeamId)
    {
      string sReturn = "Error";

      CornerkickCore.Core.Club club = AccountController.ckClub();
      int iTeamIdUser = club.iId;
      //int iTeamId = -1;
      //int.TryParse(sTeamId, out iTeamId);
      if (iTeamId >= 0 && iTeamId < MvcApplication.ckcore.ltClubs.Count && iTeamId != iTeamIdUser) {
        CornerkickCore.Core.Cup cup = MvcApplication.ckcore.ini.newCup();
        cup.iId = -5;
        cup.fAttraction = 0.5f;
        cup.iNeutral = 1;
        cup.sName = "Testspiel";

        CornerkickCore.Core.Matchday md = new CornerkickCore.Core.Matchday();
        DateTime.TryParse(start, out md.dt);

        md.ltGameData = new List<CornerkickGame.Game.Data>();
        CornerkickGame.Game.Data gd = MvcApplication.ckcore.game.newData();
        gd.team[0].iTeamId = iTeamIdUser;
        gd.team[1].iTeamId = iTeamId;

        md.ltGameData.Add(gd);

        cup.ltMatchdays.Add(md);

        sReturn = "Anfrage für Testspiel am " + md.dt.ToString("d", AccountController.ciUser) + " " + md.dt.ToString("t", AccountController.ciUser) + " gegen " + MvcApplication.ckcore.ltClubs[iTeamId].sName + " gesendet";

        // Inform user
        CornerkickCore.Core.Club clubRequest = MvcApplication.ckcore.ltClubs[iTeamId];
        if (clubRequest.iUser < 0) {
          cup.iId = 5;
        } else {
          MvcApplication.ckcore.Info(clubRequest.iUser, "Sie haben eine neue Anfrage für ein Testspiel erhalten.", 2, iTeamId);
        }

        MvcApplication.ckcore.ltCups.Add(cup);
      }

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult AcceptTestgame(string sDateTestgame)
    {
      DateTime dt = new DateTime();
      if (!DateTime.TryParse(sDateTestgame, out dt)) {
        Response.StatusCode = 1;
        return Json(new { message = "Error parsing '" + sDateTestgame + "' to Date" }, JsonRequestBehavior.AllowGet);
      }

      CornerkickCore.Core.Club club = AccountController.ckClub();

      for (int iC = 0; iC < MvcApplication.ckcore.ltCups.Count; iC++) {
        CornerkickCore.Core.Cup cup = MvcApplication.ckcore.ltCups[iC];

        if (cup.iId == -5) {
          foreach (CornerkickCore.Core.Matchday md in cup.ltMatchdays) {
            if (md.dt.Equals(dt)) {
              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                if (gd.team[1].iTeamId == club.iId) {
                  cup.iId = 5;
                  MvcApplication.ckcore.ltCups[iC] = cup;

                  club.nextGame = MvcApplication.ckcore.tl.getNextGames(club)[0];
                  MvcApplication.ckcore.ltClubs[gd.team[1].iTeamId] = club;

                  CornerkickCore.Core.Club clubH = MvcApplication.ckcore.ltClubs[gd.team[0].iTeamId];
                  clubH.nextGame = MvcApplication.ckcore.tl.getNextGames(clubH)[0];
                  MvcApplication.ckcore.ltClubs[gd.team[0].iTeamId] = clubH;

                  if (clubH.iUser >= 0) {
                    MvcApplication.ckcore.Info(clubH.iUser, "Ihre Anfrage an " + club.sName + " für ein Testspiel am " + dt.ToString("dd.MM.yyyy") + " um " + dt.ToString("hh:mm") + " wurde akzeptiert!", 2, gd.team[0].iTeamId);
                  }

                  return Json("", JsonRequestBehavior.AllowGet);
                }
              }
            }
          }
        }
      }

      return Json("", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult DeclineTestgame(string sDateTestgame)
    {
      DateTime dt = new DateTime();
      if (!DateTime.TryParse(sDateTestgame, out dt)) {
        return Json("", JsonRequestBehavior.AllowGet);
      }

      CornerkickCore.Core.Club clb = AccountController.ckClub();

      for (int iC = 0; iC < MvcApplication.ckcore.ltCups.Count; iC++) {
        CornerkickCore.Core.Cup cup = MvcApplication.ckcore.ltCups[iC];

        if (cup.iId == -5) {
          foreach (CornerkickCore.Core.Matchday md in cup.ltMatchdays) {
            if (md.dt.Equals(dt)) {
              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                if (gd.team[1].iTeamId == clb.iId) {
                  cup.iId = 5;
                  MvcApplication.ckcore.ltCups.RemoveAt(iC);

                  CornerkickCore.Core.Club clubH = MvcApplication.ckcore.ltClubs[gd.team[0].iTeamId];
                  if (clubH.iUser >= 0) {
                    MvcApplication.ckcore.Info(clubH.iUser, "Ihre Anfrage an " + clb.sName + " für ein Testspiel am " + dt.ToString("dd.MM.yyyy") + " um " + dt.ToString("hh:mm") + " wurde abgelehnt!", 2, gd.team[0].iTeamId);
                  }

                  return Json("", JsonRequestBehavior.AllowGet);
                }
              }
            }
          }
        }
      }

      return Json("", JsonRequestBehavior.AllowGet);
    }

    public JsonResult bookTrainingscamp(int iIx, string sStart, int nDays)
    {
      DateTime dtStart = new DateTime();
      if (!DateTime.TryParse(sStart, out dtStart)) return Json("Not a valid start date format!", JsonRequestBehavior.AllowGet);
      dtStart = dtStart.Date.AddHours(9);

      if ((int)(dtStart.Date - MvcApplication.ckcore.dtDatum.Date).TotalDays < 2) {
        return Json("Trainingslager müssen min. 2 Tage im vorraus gebucht werden!", JsonRequestBehavior.AllowGet);
      }

      DateTime dtEnd = dtStart.Date.AddDays(nDays).AddHours(18);

      CornerkickCore.Core.Club club = AccountController.ckClub();

      foreach (CornerkickGame.Game.Data data in MvcApplication.ckcore.tl.getNextGames(club, false)) {
        if (data.iGameType == 5) continue;

        if (dtStart.Date.Date.CompareTo(data.dt) == 0) return Json(  "Abreise am Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);
        if (dtEnd  .Date.Date.CompareTo(data.dt) == 0) return Json("Rückreise am Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);

        if (dtStart.Date.Date.CompareTo(data.dt) < 0 /* Start date before game date */ &&
            dtEnd  .Date.Date.CompareTo(data.dt) > 0 /* End date after game date */) {
          return Json("Trainingslager über Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);
        }
      }

      CornerkickCore.csTrainingCamp.Camp camp = MvcApplication.ckcore.tcp.ltCamps[iIx];

      MvcApplication.ckcore.tcp.bookCamp(ref club, camp, dtStart, dtEnd);

      return Json("Trainingslager gebucht!", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult getStringDaysUntilNextGame(DateTime dtStart, int iIgnoreGameType = 0)
    {
      int nDays = getDaysUntilNextGame(dtStart, iIgnoreGameType);

      List<string> ltDays = new List<string>();
      for (int iDay = 1; iDay < nDays; iDay++) {
        ltDays.Add(iDay.ToString().PadLeft(2) + " (Abreise: " + dtStart.Date.AddDays(iDay).ToLongDateString() + ")");
        //ltDays.Add((iDay + 1).ToString().PadLeft(2) + " (Rückreise: " + dtStart.Date.AddDays(iDay).ToString("d", AccountController.ciUser) + ")");
      }

      return Json(ltDays.ToArray(), JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public int getDaysUntilNextGame(DateTime dtStart, int iIgnoreGameType = 0)
    {
      int nDays = 999;

      CornerkickCore.Core.Club club = AccountController.ckClub();

      foreach (CornerkickGame.Game.Data data in MvcApplication.ckcore.tl.getNextGames(club, false)) {
        if (data.iGameType == iIgnoreGameType) continue;

        nDays = Math.Min(nDays, (int)(data.dt - dtStart).TotalDays);
      }

      return nDays;
    }

    public int getDaysUntilNextGame(DateTime dtStart, List<int> ltIgnoreGameTypes)
    {
      int nDays = 999;

      foreach (int iIgnoreGameType in ltIgnoreGameTypes) {
        nDays = Math.Min(nDays, getDaysUntilNextGame(dtStart, iIgnoreGameType));
      }

      return nDays;
    }

    public JsonResult setSelectedCamp(Models.CalendarModels mdCalendar, int iIx)
    {
      mdCalendar.camp = MvcApplication.ckcore.tcp.ltCamps[iIx];

      return Json(mdCalendar.camp, JsonRequestBehavior.AllowGet);
    }

    private static DateTime convertTimestampToDateTime(double timestamp)
    {
      var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
      return origin.AddSeconds(timestamp);
    }

    private static long convertDateTimeToTimestamp(DateTime dt)
    {
      return (dt.Ticks - 621355968000000000) / 10000;
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Finanzen
    /// </summary>
    /// <param name="Finanzen"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Finance(Models.FinanceModel financeModel)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      financeModel.iEintritt1 = clb.iEintrittspreise[0];
      financeModel.iEintritt2 = clb.iEintrittspreise[1];
      financeModel.iEintritt3 = clb.iEintrittspreise[2];

      financeModel.bEditable = MvcApplication.ckcore.dtDatum.Date.Equals(MvcApplication.ckcore.dtSaisonstart.Date);
      financeModel.budgetPlan = AccountController.ckUser().budget;
      financeModel.budgetReal = MvcApplication.ckcore.ui.getActualBudget(clb);

      if (financeModel.budgetPlan.iPaySalary == 0) {
        CornerkickCore.Finance.Budget bg = financeModel.budgetPlan;
        bg.iPaySalary = MvcApplication.ckcore.tl.getPlayerSalary(clb);
        financeModel.budgetPlan = bg;
      }

      if (financeModel.budgetPlan.iPayStaff == 0) {
        CornerkickCore.Finance.Budget bg = financeModel.budgetPlan;
        bg.iPayStaff = MvcApplication.ckcore.tl.getStuffSalary(clb);
        financeModel.budgetPlan = bg;
      }

      return View(financeModel);
    }

    public ContentResult GetFinanceDevelopmentData(Models.FinanceModel financeModel)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      CornerkickCore.Core.TrainingHistory trHistCurrent = new CornerkickCore.Core.TrainingHistory();
      trHistCurrent.dt = MvcApplication.ckcore.dtDatum;
      trHistCurrent.fKFM = MvcApplication.ckcore.tl.getTeamAve(clb);

      List<Models.DataPointGeneral> dataPoints = new List<Models.DataPointGeneral>();

      foreach (CornerkickCore.Finance.Account kto in clb.ltAccount) {
        if (kto.dt.CompareTo(MvcApplication.ckcore.dtDatum.AddDays(-30)) > 0) {
          long iDate = convertDateTimeToTimestamp(kto.dt);
          dataPoints.Add(new Models.DataPointGeneral(iDate, kto.iBalance));
        }
      }

      long iDateCurrent = convertDateTimeToTimestamp(trHistCurrent.dt);
      dataPoints.Add(new Models.DataPointGeneral(iDateCurrent, clb.iBalance));

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    [HttpPost]
    public JsonResult EintrittChange(int[] iEintritt)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      clb.iEintrittspreise[0] = iEintritt[0];
      clb.iEintrittspreise[1] = iEintritt[1];
      clb.iEintrittspreise[2] = iEintritt[2];

      int iInSpec = 0;
      if (clb.iLand >= 0 && clb.iDivision >= 0) {
        iInSpec = (MvcApplication.ckcore.st.getStadiumSeats(clb.stadium, 0) * clb.iEintrittspreise[0]) +
                  (MvcApplication.ckcore.st.getStadiumSeats(clb.stadium, 1) * clb.iEintrittspreise[1]) +
                  (MvcApplication.ckcore.st.getStadiumSeats(clb.stadium, 2) * clb.iEintrittspreise[2]);
        int nGamesHome = (MvcApplication.ckcore.ltLiga[clb.iLand][clb.iDivision].Count - 1);
        iInSpec *= nGamesHome;
      }

      return Json(iInSpec.ToString("#,#", AccountController.ciUser) + "€", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult SetBudget(int[] iBudgetIn, int[] iBudgetPay)
    {
      CornerkickCore.Core.User user = AccountController.ckUser();

      if (iBudgetIn  != null) {
        user.budget.iInSpec         = iBudgetIn[0];
        user.budget.iInBonusSponsor = iBudgetIn[1];
        user.budget.iInTransfer     = iBudgetIn[2];
      }

      if (iBudgetPay != null) {
        user.budget.iPaySalary   = iBudgetPay[0];
        user.budget.iPayStaff    = iBudgetPay[1];
        user.budget.iPayTransfer = iBudgetPay[2];
        user.budget.iPayStadium  = iBudgetPay[3];
        user.budget.iPayInterest = iBudgetPay[4];
      }
      AccountController.setCkUser(user);

      CornerkickCore.Core.Club clb = AccountController.ckClub();

      long iInPlanTotal  = MvcApplication.ckcore.fz.getBudgetInTotal (user.budget);
      long iPayPlanTotal = MvcApplication.ckcore.fz.getBudgetPayTotal(user.budget);

      long iPlannedResult = iInPlanTotal - iPayPlanTotal;
      string sPlannedResult = "0";
      if (iPlannedResult != 0) {
        sPlannedResult = iPlannedResult.ToString("#,#", AccountController.ciUser);
      }

      CornerkickCore.Finance.Budget bgReal = MvcApplication.ckcore.ui.getActualBudget(clb);
      long iInCurrTotal  = MvcApplication.ckcore.fz.getBudgetInTotal (bgReal);
      long iPayCurrTotal = MvcApplication.ckcore.fz.getBudgetPayTotal(bgReal);
      string sCurrentResult = "0";
      if (iInCurrTotal - iPayCurrTotal != 0) {
        sCurrentResult = (iInCurrTotal - iPayCurrTotal).ToString("#,#", AccountController.ciUser);
      }

      string[] sTotal = new string[4] {
        iInPlanTotal .ToString("#,#", AccountController.ciUser),
        iPayPlanTotal.ToString("#,#", AccountController.ciUser),
        sPlannedResult,
        sCurrentResult
      };

      return Json(sTotal, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Sponsor
    /// </summary>
    /// <param name="Sponsor"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    [Authorize]
    public ActionResult Sponsor(Models.SponsorModel sponsorModel)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      sponsorModel.sponsorMain     = clb.sponsorMain;
      sponsorModel.ltSponsorBoards = clb.ltSponsorBoards;
      sponsorModel.ltSponsorOffers = clb.ltSponsorOffers;

      sponsorModel.ltSponsorBoardIds = new List<int>();
      foreach (CornerkickCore.Finance.Sponsor spon in sponsorModel.ltSponsorBoards) {
        for (int iB = 0; iB < spon.nBoards; iB++) sponsorModel.ltSponsorBoardIds.Add(spon.iId);
      }

      return View(sponsorModel);
    }

    [HttpPost]
    public JsonResult SponsorSet(int iSponsorIndex)
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      string sCash = MvcApplication.ckcore.ui.setSponsor(ref clb, clb.ltSponsorOffers[iSponsorIndex]).ToString("#,#", AccountController.ciUser);

      return Json(sCash, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult getLtSponsorBoardIds()
    {
      CornerkickCore.Core.Club clb = AccountController.ckClub();

      List<int> ltSponsorBoardIds = new List<int>();
      foreach (CornerkickCore.Finance.Sponsor spon in clb.ltSponsorBoards) {
        for (int iB = 0; iB < spon.nBoards; iB++) ltSponsorBoardIds.Add(spon.iId);
      }

      return Json(ltSponsorBoardIds, JsonRequestBehavior.AllowGet);
    }

    public ActionResult getTableSponsorBoard()
    {
      CornerkickCore.Core.Club club = AccountController.ckClub();

      //The table or entity I'm querying
      List<Models.DatatableEntrySponsorBoard> query = new List<Models.DatatableEntrySponsorBoard>();

      int iSpOffer = 0;
      bool bOffer = false;
      foreach (List<CornerkickCore.Finance.Sponsor> ltSponsor in new List<CornerkickCore.Finance.Sponsor>[] { club.ltSponsorBoards, club.ltSponsorOffers }) {
        foreach (CornerkickCore.Finance.Sponsor spon in ltSponsor) {
          if (bOffer) iSpOffer++;

          if (spon.bMain) continue;

          Models.DatatableEntrySponsorBoard deSponsorBoard = new Models.DatatableEntrySponsorBoard();
          deSponsorBoard.bOffer = bOffer;
          deSponsorBoard.iId = spon.iId;
          if (bOffer) deSponsorBoard.iIndex = iSpOffer - 1;
          deSponsorBoard.sName = MvcApplication.ckcore.fz.ltSponsoren[spon.iId].sName;
          deSponsorBoard.sMoneyVicHome = spon.iMoneyVicHome.ToString("#,#", AccountController.ciUser);
          deSponsorBoard.nBoards = spon.nBoards;
          deSponsorBoard.iYears = spon.iYears;

          query.Add(deSponsorBoard);
        }

        bOffer = true;
      }

      return Json(new { aaData = query.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Mail
    /// </summary>
    /// <param name="Mail"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult Mail(Models.UserModel mdUser)
    {
      mdUser.ltUserMail = new List<CornerkickCore.Core.News>();

      CornerkickCore.Core.User user = AccountController.ckUser();

      mdUser.ltUserMail = new List<CornerkickCore.Core.News>();

      if (user.ltNews != null) {
        foreach (CornerkickCore.Core.News news in user.ltNews) {
          if (news.iType == 99) mdUser.ltUserMail.Add(news);
        }
      }

      return View(mdUser);
    }

    [HttpPost]
    public JsonResult MailSend(int iTo, string sText)
    {
      if (iTo < 0) return Json("Error. Unknown user", JsonRequestBehavior.AllowGet);
      if (string.IsNullOrEmpty(sText)) return Json("Error. Nachricht leer", JsonRequestBehavior.AllowGet);

      MvcApplication.ckcore.Info(iTo, sText, 99, 0, 0, System.DateTime.Now, AccountController.getiUser(AccountController.ckUser()));

      return Json("Nachricht an " + MvcApplication.ckcore.ltUser[iTo].sVorname + " " + MvcApplication.ckcore.ltUser[iTo].sNachname + " gesendet!", JsonRequestBehavior.AllowGet);
    }

    public void MailMarkRead(int iIndexMail)
    {
      if (iIndexMail < 0) return;

      CornerkickCore.Core.User user = AccountController.ckUser();

      if (user.ltNews != null) {
        int iMail = 0;
        for (int iN = 0; iN < user.ltNews.Count; iN++) {
          CornerkickCore.Core.News news = user.ltNews[iN];
          if (news.iType == 99) {
            if (iMail == iIndexMail) {
              news.bRead = true;
              user.ltNews[iN] = news;
              return;
            }

            iMail++;
          }
        }
      }
    }

    public static int countNewMails()
    {
      CornerkickCore.Core.User user = AccountController.ckUser();

      int iMails = 0;
      if (user.ltNews != null) {
        foreach (CornerkickCore.Core.News news in user.ltNews) {
          if (news.iType == 99 && !news.bRead) iMails++;
        }
      }

      return iMails;
    }

    public int compareDates(DateTime dt)
    {
      CornerkickCore.Core.Club club = AccountController.ckClub();

      List<CornerkickGame.Game.Data> ltGd = MvcApplication.ckcore.tl.getNextGames(club, false);

      foreach (CornerkickGame.Game.Data gd in ltGd) {
        /*
        double fTotH = (dt - gd.dt).TotalHours;
        if (fTotH > 0.0 && fTotH < +4) return -2;
        if (fTotH < 0.0 && fTotH > -4) return -2;
        */
        if (gd.dt.Date.Equals(dt.Date)) return -2;
        if (Math.Abs((dt - gd.dt).TotalHours) < 8) return -2;
      }

      return dt.CompareTo(MvcApplication.ckcore.dtDatum);
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

  }
}