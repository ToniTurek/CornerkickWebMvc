using System;
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
    public readonly static string[] sCultureInfo = new string[82] {
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "en-GB",
      "",
      "",
      "",
      "fr-FR",
      "",
      "",
      "de-DE",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      "",
      ""
    };

    public MemberController()
    {
      #if _CONSOLE
      ConsoleNews();
#endif
    }

    public static CornerkickManager.User ckUserStatic(System.Security.Principal.IPrincipal User)
    {
      if (User == null) return null;

      string sUserId = User.Identity.GetUserId();
      foreach (CornerkickManager.User usr in MvcApplication.ckcore.ltUser) {
        if (usr.id.Equals(sUserId)) return usr;
      }

      return null;
    }

    public static CornerkickManager.Club ckClubStatic(System.Security.Principal.IPrincipal User)
    {
      CornerkickManager.User usr = ckUserStatic(User);
      if (usr == null) return null;

      if (usr.iTeam >= 0 && usr.iTeam < MvcApplication.ckcore.ltClubs.Count) {
        return MvcApplication.ckcore.ltClubs[usr.iTeam];
      }

      return null;
    }

    public CornerkickManager.User ckUser()
    {
      if (User == null) return null;

      string sUserId = User.Identity.GetUserId();
      foreach (CornerkickManager.User usr in MvcApplication.ckcore.ltUser) {
        if (usr.id.Equals(sUserId)) return usr;
      }

      return null;
    }

    public CornerkickManager.Club ckClub()
    {
      if (AccountController.checkUserIsAdmin(User.Identity.Name)) {
        if (MvcApplication.clubAdmin != null) return MvcApplication.clubAdmin;
      }

      CornerkickManager.User usr = ckUser();
      if (usr == null) return null;

      if (usr.iTeam >= 0 && usr.iTeam < MvcApplication.ckcore.ltClubs.Count) {
        return MvcApplication.ckcore.ltClubs[usr.iTeam];
      }

      return null;
    }

    public static CultureInfo getCiStatic(System.Security.Principal.IPrincipal User)
    {
      CornerkickManager.Club clb = ckClubStatic(User);
      if (clb != null) {
        int iLandUser = clb.iLand;
        if (iLandUser >= 0 && iLandUser < sCultureInfo.Length) return new CultureInfo(sCultureInfo[iLandUser]);
      }

      return CultureInfo.CurrentCulture;
    }

    public CultureInfo getCi()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb != null) {
        int iLandUser = clb.iLand;
        if (iLandUser >= 0 && iLandUser < sCultureInfo.Length) return new CultureInfo(sCultureInfo[iLandUser]);
      }

      return CultureInfo.CurrentCulture;
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
        CornerkickManager.Main.News news = AccountController.ckUser.ltNews[iN];
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
    public ActionResult Desk(Models.DeskModel desk, Models.LeagueModels mlLeague)
    {
      desk.sNews = "";

      if (ModelState.IsValid) {
        ModelState.Clear();
      }

      CornerkickManager.User usr = ckUser();
      if (usr        == null) return View(desk);
      if (usr.ltNews == null) return View(desk);
      desk.user = usr;

      if (usr.lti != null) {
        if (usr.lti.Count > 0) desk.iDeleteLog = usr.lti[0];
      }

      for (int iN = usr.ltNews.Count - 1; iN >= 0; iN--) {
        CornerkickManager.Main.News news = usr.ltNews[iN];
        if (news.iType < 99/* && news.bUnread*/) {
          if (news.bRead && news.bRead2) {
            if        (desk.iDeleteLog == 1 && (MvcApplication.ckcore.dtDatum - news.dt).TotalDays >  7) {
              usr.ltNews.Remove(news);
              continue;
            } else if (desk.iDeleteLog == 2 && (MvcApplication.ckcore.dtDatum - news.dt).TotalDays > 14) {
              usr.ltNews.Remove(news);
              continue;
            } else if (desk.iDeleteLog == 3 && (MvcApplication.ckcore.dtDatum - news.dt).TotalDays > 30) {
              usr.ltNews.Remove(news);
              continue;
            }
          }

          string sNews = news.dt.ToString("d", getCi()) + " " + news.dt.ToString("t", getCi()) + " - " + news.sText + '\n';
          if (news.bRead) desk.sNewsOld += sNews;
          else            desk.sNews    += sNews;

          news.bRead  = true;
          news.bRead2 = true;
          usr.ltNews[iN] = news;
        }
      }

      CornerkickManager.Club club = ckClub();
      if (club == null) return View(desk);
      desk.club = club;

      // Date next game
      desk.sDtNextGame = "";
      if (club.nextGame != null) {
        if (club.nextGame.dt.Date.Equals(MvcApplication.ckcore.dtDatum.Date)) {
          desk.sDtNextGame = "Heute, " + club.nextGame.dt.ToString("t", getCi()) + " Uhr";
        } else {
          desk.sDtNextGame = club.nextGame.dt.ToString("d", getCi()) + " (" + (club.nextGame.dt.Date - MvcApplication.ckcore.dtDatum.Date).TotalDays.ToString("0") + "d)";
        }

        // Weather
        desk.iWeather = club.nextGame.iWeather;
      }

      // Get Table
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, club.iLand, club.iDivision);

      desk.sTabellenplatz = "-";
      List<CornerkickManager.Tool.TableItem> ltTbl = MvcApplication.ckcore.tl.getLeagueTable(league);
      int iPlatz = 1;
      int iSpl = 0;
      foreach (CornerkickManager.Tool.TableItem tbl in ltTbl) {
        if (tbl.iId == club.iId) {
          iSpl = tbl.iGUV[0] + tbl.iGUV[1] + tbl.iGUV[2];
          break;
        }
        iPlatz++;
      }
      if (iSpl > 0) desk.sTabellenplatz = iPlatz.ToString() + ". Platz nach " + iSpl.ToString() + " von " + ((ltTbl.Count - 1) * 2).ToString() + " Spielen";

      // Get Cup Round
      desk.sPokalrunde = "-";
      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(2, club.iLand);
      if (cup != null) {
        if (cup.ltMatchdays != null) {
          if (cup.ltMatchdays.Count > 0) {
            if (cup.ltMatchdays[0].ltGameData != null) {
              int nPartFirstRound = cup.ltClubs[0].Count;

              if (nPartFirstRound > 0) {
                int nRound = cup.getKoRound(nPartFirstRound);
                //int iMdCup = MvcApplication.ckcore.tl.getMatchday(cup, MvcApplication.ckcore.dtDatum);
                int iMdClub = MvcApplication.ckcore.tl.getMatchday(cup, club);
                if (iMdClub >= 0) desk.sPokalrunde = MvcApplication.ckcore.sCupRound[nRound - iMdClub - 1];
                //if (nRound - club.iPokalrunde > 0 && nRound - club.iPokalrunde < MvcApplication.ckcore.sPokalrunde.Length) desk.sPokalrunde = MvcApplication.ckcore.sPokalrunde[nRound - club.iPokalrunde];
              }
            }
          }
        }
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

    public void SetDeleteLog(int iDeleteAfter)
    {
      CornerkickManager.User usr = ckUser();
      if (usr == null) return;

      if (usr.lti == null) usr.lti = new List<int>();
      if (usr.lti.Count < 1) usr.lti.Add(0);

      usr.lti[0] = iDeleteAfter;
    }

    public ContentResult GetLastGames()
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Content("", "application/json");

      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[3];
      dataPoints[0] = new List<Models.DataPointGeneral>();
      dataPoints[1] = new List<Models.DataPointGeneral>();
      dataPoints[2] = new List<Models.DataPointGeneral>();

      List<CornerkickGame.Game.Data> ltGameData = MvcApplication.ckcore.tl.getNextGames(club, MvcApplication.ckcore.dtDatum, false, true);
      int iLg = 0;
      foreach (CornerkickGame.Game.Data gs in ltGameData) {
        if (gs.iGameType < 1 || gs.iGameType == 5) continue; // Testgame
        if (gs.team[0].iTeamId == 0 && gs.team[1].iTeamId == 0) continue;

        CornerkickManager.Club clubH = null;
        if (gs.team[0].iTeamId >= 0 && gs.team[0].iTeamId < MvcApplication.ckcore.ltClubs.Count) clubH = MvcApplication.ckcore.ltClubs[gs.team[0].iTeamId];
        CornerkickManager.Club clubA = null;
        if (gs.team[1].iTeamId >= 0 && gs.team[1].iTeamId < MvcApplication.ckcore.ltClubs.Count) clubA = MvcApplication.ckcore.ltClubs[gs.team[1].iTeamId];

        string sDesc = gs.dt.ToString("d", getCi()) + "</br>";
        if (clubH != null && clubA != null) sDesc += clubH.sName + " - " + clubA.sName + "</br>";
        sDesc += MvcApplication.ckcore.ui.getResultString(gs);

        int iGameType = 0;
        if      (gs.iGameType == 2) iGameType = 1;
        else if (gs.iGameType == 5) iGameType = 2;

        if (gs.team[0].iGoals == gs.team[1].iGoals) {
          dataPoints[iGameType].Add(new Models.DataPointGeneral(--iLg,  0, sDesc));
        } else if ((gs.team[0].iGoals > gs.team[1].iGoals && gs.team[0].iTeamId == club.iId) || 
                   (gs.team[0].iGoals < gs.team[1].iGoals && gs.team[1].iTeamId == club.iId)) {
          dataPoints[iGameType].Add(new Models.DataPointGeneral(--iLg, +1, sDesc));
        } else if ((gs.team[0].iGoals < gs.team[1].iGoals && gs.team[0].iTeamId == club.iId) ||
                   (gs.team[0].iGoals > gs.team[1].iGoals && gs.team[1].iTeamId == club.iId)) {
          dataPoints[iGameType].Add(new Models.DataPointGeneral(--iLg, -1, sDesc));
        }
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public ActionResult PreviewGame(int i)
    {
      Models.PreviewGameModel mdPreview = new Models.PreviewGameModel();
      if (i < 0) return View(mdPreview);

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdPreview);

      List<CornerkickGame.Game.Data> ltGdNextGames = MvcApplication.ckcore.tl.getNextGames(clb, MvcApplication.ckcore.dtDatum);
      for (byte j = 0; j < ltGdNextGames.Count; j++) {
        CornerkickGame.Game.Data gd = ltGdNextGames[j];
        mdPreview.ddlGames.Add(new SelectListItem { Text = gd.dt.ToString("d", getCi()), Value = j.ToString() });
      }
      if (i >= ltGdNextGames.Count) return View(mdPreview);

      CornerkickGame.Game.Data gdNext = ltGdNextGames[i];
      mdPreview.sTeamH = gdNext.team[0].sTeam;
      mdPreview.sTeamA = gdNext.team[1].sTeam;

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(gdNext);
      if (cup != null) {
        mdPreview.sCupName = cup.sName;
        mdPreview.sMd = (MvcApplication.ckcore.tl.getMatchday(cup, MvcApplication.ckcore.dtDatum) + 1).ToString() + ". Spieltag";
      }

      mdPreview.sStadium = gdNext.stadium.sName;

      mdPreview.sReferee = "Qualität: " + gdNext.referee.fQuality.ToString("0.0%") + ", Härte: " + gdNext.referee.fStrict.ToString("0.0%");

      return View(mdPreview);
    }

    public ContentResult GetTeamDevelopmentData(bool bExpected = false)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Content("", "application/json");

      CornerkickManager.Main.TrainingHistory trHistCurrent = new CornerkickManager.Main.TrainingHistory();
      trHistCurrent.dt = MvcApplication.ckcore.dtDatum;
      trHistCurrent.fKFM = MvcApplication.ckcore.tl.getTeamAve(clb);

      List<Models.DataPointGeneral>[][] dataPoints = new List<Models.DataPointGeneral>[2][];
      dataPoints[0] = new List<Models.DataPointGeneral>[3];
      dataPoints[1] = new List<Models.DataPointGeneral>[3];

      for (byte j = 0; j < dataPoints[0].Length; j++) {
        dataPoints[0][j] = new List<Models.DataPointGeneral>();

        for (int i = 0; i < clb.ltTrainingHist.Count; i++) {
          CornerkickManager.Main.TrainingHistory trHist = clb.ltTrainingHist[i];
          if (trHist.dt.CompareTo(MvcApplication.ckcore.dtDatum.AddDays(-7)) > 0) {
            long iDate = convertDateTimeToTimestamp(trHist.dt);
            dataPoints[0][j].Add(new Models.DataPointGeneral(iDate, trHist.fKFM[j]));
          }
        }

        long iDateCurrent = convertDateTimeToTimestamp(trHistCurrent.dt);
        dataPoints[0][j].Add(new Models.DataPointGeneral(iDateCurrent, trHistCurrent.fKFM[j]));
      }

      if (bExpected) {
        // Initialize dataPoints list
        for (byte j = 0; j < dataPoints[1].Length; j++) dataPoints[1][j] = new List<Models.DataPointGeneral>();

        // Create temp. player
        List<CornerkickGame.Player> ltPlayerTmp = new List<CornerkickGame.Player>();
        foreach (int iPlId in clb.ltPlayerId) ltPlayerTmp.Add(MvcApplication.ckcore.ltPlayer[iPlId].Clone());

        // For the next 7 days ...
        for (byte iD = 0; iD < 7; iD++) {
          DateTime dtTmp = MvcApplication.ckcore.dtDatum.AddDays(iD);

          if (iD > 0) {
            if ((int)dtTmp.DayOfWeek == 0) break;

            // ... do training for each player
            for (int iP = 0; iP < ltPlayerTmp.Count; iP++) {
              CornerkickGame.Player plTmp = ltPlayerTmp[iP];
              MvcApplication.ckcore.plr.doTraining(ref plTmp, dtTmp, false, true);
            }
          }

          // ... get training history data
          CornerkickManager.Main.TrainingHistory trHistExp = new CornerkickManager.Main.TrainingHistory();
          trHistExp.dt   = dtTmp;
          trHistExp.fKFM = MvcApplication.ckcore.tl.getTeamAve(ltPlayerTmp);

          // ... add training history data to dataPoints
          for (byte j = 0; j < dataPoints[1].Length; j++) {
            long iDate = convertDateTimeToTimestamp(trHistExp.dt);
            dataPoints[1][j].Add(new Models.DataPointGeneral(iDate, trHistExp.fKFM[j]));
          }
        }
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public ContentResult GetTeamFAve()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Content("", "application/json");

      Models.DataPointGeneral[] dataPoints = new Models.DataPointGeneral[7];

      double[] fFAve = new double[7];
      int nPlKeeper = 0;
      foreach (int iPlId in clb.ltPlayerId) {
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

        fFAve[0] +=  pl.fSkillTraining[ 0];
        fFAve[1] += (pl.fSkillTraining[ 1] + pl.fSkillTraining[ 2])                                                / 2f; // Technik + Dribbling
        fFAve[2] +=  pl.fSkillTraining[ 3];                                                                              // Zweikampf
        fFAve[3] += (pl.fSkillTraining[ 4] + pl.fSkillTraining[ 5] + pl.fSkillTraining[ 6] + pl.fSkillTraining[7]) / 4f; // Abspiel
        fFAve[4] += (pl.fSkillTraining[ 8] + pl.fSkillTraining[ 9] + pl.fSkillTraining[10])                        / 3f; // Abschluss
        fFAve[5] += (pl.fSkillTraining[11] + pl.fSkillTraining[12])                                                / 2f; // Standards
        fFAve[6] += (pl.fSkillTraining[13] + pl.fSkillTraining[14] + pl.fSkillTraining[15])                        / 3f; // TW

        if (pl.fExperiencePos[0] > 0.999) nPlKeeper++;
      }

      for (int iF = 0; iF < fFAve.Length; iF++) {
        if (iF < fFAve.Length - 1) dataPoints[iF] = new Models.DataPointGeneral(iF, fFAve[iF] / clb.ltPlayerId.Count);
        else if (nPlKeeper > 0)    dataPoints[iF] = new Models.DataPointGeneral(iF, fFAve[iF] / nPlKeeper);
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
      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();
      team.club = club;
      if (club == null) return View(team);

      team.bAdmin = AccountController.checkUserIsAdmin(User.Identity.GetUserName());

      team.bGame = false;
      if (user != null && user.game != null) team.bGame = !user.game.data.bFinished;

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

      if (user != null && user.ltFormations != null) {
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

    private void setModelLtPlayer(CornerkickManager.User user)
    {
      Models.TeamModels.ltPlayer = new List<CornerkickGame.Player>();

      CornerkickManager.Club club = ckClub();
      if (club == null) return;

      foreach (int iPl in club.ltPlayerId) {
        Models.TeamModels.ltPlayer.Add(MvcApplication.ckcore.ltPlayer[iPl]);
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

      CornerkickManager.User user = ckUser();
      int iC = user.iTeam;

      int iPlayerID = MvcApplication.ckcore.ltClubs[iC].ltPlayerId[fromPosition - 1];
      if (!MvcApplication.ckcore.ltPlayer[iPlayerID].bPlayed) {
        MvcApplication.ckcore.ltClubs[iC].ltPlayerId.RemoveAt(fromPosition - 1);
        MvcApplication.ckcore.ltClubs[iC].ltPlayerId.Insert(toPosition - 1, iPlayerID);

        setModelLtPlayer(user);

        CkAufstellungFormation(MvcApplication.ckcore.ltClubs[iC].formation.iId);
      }
    }

    public JsonResult SwitchPlayerByIndex(int iIndex1, int iIndex2)
    {
      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      int jPosMin = Math.Min(iIndex1, iIndex2);
      int jPosMax = Math.Max(iIndex1, iIndex2);

      int iPlayerID1 = club.ltPlayerId[jPosMin];
      int iPlayerID2 = club.ltPlayerId[jPosMax];

      if (user.game != null) {
        if (!user.game.data.bFinished) {
          byte iHA = 0;
          if (club.iId == user.game.data.team[1].iTeamId) iHA = 1;

          // If switch of player in starting 11 --> do it directly
          if (jPosMin < user.game.data.nPlStart && jPosMax >= user.game.data.nPlStart) {
            // Return if ...
            if (user.game.player[iHA][jPosMax].bPlayed) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet); // ... player in has already played
            if (user.game.iSubstitutionsLeft[iHA] == 0) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet); // ... no subs left
            if (user.game.data.iGameType >                                              0 &&
                user.game.data.iGameType <= user.game.player[iHA][jPosMin].iSuspension.Length &&
                user.game.player[iHA][jPosMin].iSuspension[user.game.data.iGameType - 1] > 0) return Json(Models.TeamModels.ltPlayer, JsonRequestBehavior.AllowGet); // ... player out is suspended

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
      CornerkickManager.Club clb = ckClub();

      int iIndex1 = clb.ltPlayerId.IndexOf(iID1);
      int iIndex2 = clb.ltPlayerId.IndexOf(iID2);

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
      public string sLeader { get; set; }
      public string sMarktwert { get; set; }
      public string sGehalt { get; set; }
      public string sLz { get; set; }
      public string sNat { get; set; }
      public int iSuspended { get; set; }
      public string sCaptain { get; set; }
    }

    public ActionResult getTableTeam()
    {
      /*
      List<CornerkickGame.Player> ltSpieler = new List<CornerkickGame.Player>();
      foreach (int iSp in AccountController.ckClub.ltPlayerId) {
        ltSpieler.Add(MvcApplication.ckcore.ltPlayer[iSp]);
      }
      */
      var club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      var user = ckUser();

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

        int iNat = int.Parse(ltLV[i][12]);
        string sNat = MvcApplication.ckcore.sLandShort[iNat];

        //Hard coded data here that I want to replace with query results
        query[i] = new DatatableEntryTeam { iIndex = i + 1, sID = ltLV[i][0], sNr = ltLV[i][1], sNull = "", sName = sName, sPosition = ltLV[i][3], sStaerke = ltLV[i][4], sKondi = ltLV[i][5], sFrische = ltLV[i][6], sMoral = ltLV[i][7], sErf = ltLV[i][8], sMarktwert = ltLV[i][9], sGehalt = ltLV[i][10], sLz = ltLV[i][11], sNat = sNat, sForm = ltLV[i][13], sAlter = ltLV[i][14], sTalent = ltLV[i][15], bSubstituted = ltLV[i][16] == "ausg", sLeader = ltLV[i][19], sStaerkeIdeal = ltLV[i][17], iSuspended = iSusp, sCaptain = ltLV[i][20] };
      }

      return Json(new { aaData = query }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult CkAufstellungKI(int iType = 0)
    {
      // Check if game running
      CornerkickManager.User user = ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) return RedirectToAction("Team");
      }

      MvcApplication.ckcore.doFormation(user.iTeam);

      return RedirectToAction("Team");
    }

    public JsonResult doFormationKI(int iType = 0)
    {
      // Check if game running
      CornerkickManager.User user = ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) return Json("error", JsonRequestBehavior.AllowGet);
      }

      CornerkickManager.Club club = ckClub();

      MvcApplication.ckcore.doFormation(club.iId, iType);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult movePlayer(int iIndexPlayer, int iDirection)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

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

      updatePlayerOfGame(ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult movePlayerRoa(int iIndexPlayer, int iDirection)
    {
      if (iIndexPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]];

      if      (iDirection == 1) pl.fRadOfAction[0] += 0.2f;
      else if (iDirection == 2) pl.fRadOfAction[0] -= 0.2f;
      else if (iDirection == 3) pl.fRadOfAction[1] += 0.2f;
      else if (iDirection == 4) pl.fRadOfAction[1] -= 0.2f;

      MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]] = pl;

      //if (iIndex >= Models.TeamModels.ltPlayer.Count) setModelLtPlayer(usr);
      Models.TeamModels.ltPlayer[iIndexPlayer] = pl;

      updatePlayerOfGame(ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    public JsonResult TeamSetIndOrientation(int iIndexPlayer, float fIndOrientation)
    {
      if (iIndexPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]];

      pl.fIndOrientation = fIndOrientation;

      //MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIndexPlayer]] = pl;
      Models.TeamModels.ltPlayer[iIndexPlayer] = pl;

      updatePlayerOfGame(ckUser().game, club);

      return Json(fIndOrientation.ToString(), JsonRequestBehavior.AllowGet);
    }

    public JsonResult TeamSetManMarking(int iIxPlayer, sbyte iIxPlayerOpp)
    {
      if (iIxPlayer < 0) return Json("error", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iIxPlayer]];

      if (pl.iIxManMarking == iIxPlayerOpp) pl.iIxManMarking = -1;
      else                                  pl.iIxManMarking = iIxPlayerOpp;

      Models.TeamModels.ltPlayer[iIxPlayer] = pl;

      updatePlayerOfGame(ckUser().game, club);

      return Json("success", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult TeamSetSubstitutions()
    {
      // Check if game running
      CornerkickManager.User user = ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) {
          if (Models.TeamModels.ltiSubstitution != null) {
            CornerkickManager.Club club = ckClub();
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
      CornerkickManager.User user = ckUser();
      if (user.game != null) {
        if (!user.game.data.bFinished) {
          CornerkickManager.Club club = ckClub();

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

      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

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

    public JsonResult CkAufstellungFormation(int iF, int iSP = -1, bool bMobile = false)
    {
      CornerkickManager.User usr  = ckUser();
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

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
      tD.ltPlayerNat      = new List<string>();
      tD.ltPlayerSusp     = new List<bool>  ();
      foreach (CornerkickGame.Player pl in tD.ltPlayer) {
        tD.ltPlayerPos     .Add(MvcApplication.ckcore.game.tl.getBasisPos(MvcApplication.ckcore.game.tl.getPosRole(pl)));
        tD.ltPlayerAveSkill.Add(MvcApplication.ckcore.game.tl.getAveSkill(pl, 99).ToString("0.0"));
        tD.ltPlayerNat     .Add(MvcApplication.ckcore.sLandShort[pl.iNat1]);

        // Check if player is suspended
        bool bSusp = false;
        int iSuspIx = 0;
        if      (usr.game      != null && !usr.game.data.bFinished) iSuspIx = usr.game.data.iGameType - 1;
        else if (club.nextGame != null)                             iSuspIx = club.nextGame.iGameType - 1;

        if (iSuspIx >= 0 && iSuspIx < pl.iSuspension.Length) bSusp = pl.iSuspension[iSuspIx] > 0;

        tD.ltPlayerSusp.Add(bSusp);
      }

      tD.sDivPlayer = TeamGetPlayerOffence(tD.ltPlayer, club, bMobile);

      if (iSP >= 0) {
        tD.plSelected = tD.ltPlayer[iSP];
        tD.fIndOrientation = tD.plSelected.fIndOrientation;
        if (tD.fIndOrientation < -1f) tD.fIndOrientation = MvcApplication.ckcore.game.tl.getPlayerIndividualOrientationDefault(tD.plSelected);

        tD.sDivRoa               = TeamGetPlayerRadiusOfAction(iSP, club);
        tD.fIndOrientationMinMax = TeamGetIndOrientationMinMax(iSP, club);
      }

      tD.iCaptainIx = MvcApplication.ckcore.plr.getCaptainIx(club);

      // Team averages
      float[] fTeamAve11 = MvcApplication.ckcore.tl.getTeamAve(club, 11);
      tD.sTeamAveSkill = fTeamAve11[3].ToString("0.00");
      tD.sTeamAveAge   = fTeamAve11[4].ToString("0.0");

      if (club.nextGame != null) {
        tD.iKibitzer = club.personal.iKibitzer;

        int iClubOpp = club.nextGame.team[1].iTeamId;
        if (club.nextGame.team[1].iTeamId == club.iId) iClubOpp = club.nextGame.team[0].iTeamId;

        tD.ltPlayerOpp = new List<CornerkickGame.Player>();
        tD.ltPlayerOppPos = new List<byte>();
        tD.ltPlayerOppAveSkill = new List<string>();

        if (iClubOpp >= 0) {
          CornerkickManager.Club clubOpp = MvcApplication.ckcore.ltClubs[iClubOpp];

          for (byte iPl = 0; iPl < club.nextGame.nPlStart; iPl++) {
            int iPlId = clubOpp.ltPlayerId[iPl];
            CornerkickGame.Player plOpp = MvcApplication.ckcore.ltPlayer[iPlId];

            tD.ltPlayerOpp.Add(plOpp);
            tD.ltPlayerOppPos.Add(MvcApplication.ckcore.game.tl.getBasisPos(MvcApplication.ckcore.game.tl.getPosRole(plOpp)));

            float fPlOppAveSkill = MvcApplication.ckcore.game.tl.getAveSkill(plOpp, 99);
            if (tD.iKibitzer == 3) fPlOppAveSkill = (float)Math.Round(fPlOppAveSkill * 2f) / 2f;
            tD.ltPlayerOppAveSkill.Add(fPlOppAveSkill.ToString("0.0"));
          }

          // Opp. team averages
          float[] fTeamOppAve11 = MvcApplication.ckcore.tl.getTeamAve(clubOpp, club.nextGame.nPlStart);
          tD.sTeamOppAveSkill = fTeamOppAve11[3].ToString("0.00");
          tD.sTeamOppAveAge   = fTeamOppAve11[4].ToString("0.0");
        }
      }

      return Json(tD, JsonRequestBehavior.AllowGet);
    }

    // Set positions to player of current game
    private void updatePlayerOfGame(CornerkickGame.Game game, CornerkickManager.Club club)
    {
      if (game == null) return;
      if (game.data.bFinished) return;

      byte iHA = 0;
      if (game.data.team[1].iTeamId == club.iId) iHA = 1;

      for (byte iPl = 0; iPl < game.data.nPlStart; iPl++) {
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

      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      club.formation.iId = MvcApplication.ckcore.ltFormationen.Count + user.ltFormations.Count + 1;

      CornerkickManager.Main.Formation frmUser = MvcApplication.ckcore.ini.newFormation();
      frmUser.iId = club.formation.iId;
      frmUser.sName = sName;
      for (int iPt = 0; iPt < club.formation.ptPos.Length; iPt++) frmUser.ptPos[iPt] = club.formation.ptPos[iPt];

      user.ltFormations.Add(frmUser);

      return Json(MvcApplication.ckcore.ltFormationen.Count + user.ltFormations.Count, JsonRequestBehavior.AllowGet);
    }

    public JsonResult deleteFormation(int iFormation)
    {
      CornerkickManager.User user = ckUser();

      if (iFormation >= MvcApplication.ckcore.ltFormationen.Count + 1 && iFormation < MvcApplication.ckcore.ltFormationen.Count + user.ltFormations.Count + 1) {
        CornerkickManager.Club club = ckClub();

        user.ltFormations.RemoveAt(iFormation - MvcApplication.ckcore.ltFormationen.Count - 1);

        CkAufstellungFormation(0);
      }

      return Json(1, JsonRequestBehavior.AllowGet);
    }

    public JsonResult GetPlayerStrength(int iPlayer)
    {
      CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iPlayer];
      double fStrength = MvcApplication.ckcore.game.tl.getAveSkill(player, 99);
      return Json(fStrength, JsonRequestBehavior.AllowGet);
    }

    private string TeamGetPlayerOffence(List<CornerkickGame.Player> ltPlayer, CornerkickManager.Club club, bool bMobile = false)
    {
      string sDiv = "";

      if (ltPlayer       == null) return "";
      if (ltPlayer.Count ==    0) return "";

      byte iPl = 0;
      foreach (CornerkickGame.Player pl in ltPlayer) {
        if (iPl >= MvcApplication.ckcore.game.data.nPlStart) break;

        if (!MvcApplication.ckcore.game.tl.checkPlayerIsKeeper(pl)) {
          float fHeight = 0.05f;
          if (bMobile) fHeight = 0.07f;

          float fWidth  = fHeight * (3f / 2f);

          int iXOffence = MvcApplication.ckcore.game.tl.getXPosOffence(pl, club.tactic.fOrientation);
          float fTop  = 1f - (iXOffence / (float)MvcApplication.ckcore.game.ptPitch.X);
          float fLeft = (pl.ptPosDefault.Y + MvcApplication.ckcore.game.ptPitch.Y) / (float)(2 * MvcApplication.ckcore.game.ptPitch.Y);

          fTop  -= fHeight / 2f;
          fLeft -= fWidth  / 2f;

          sDiv += "<div onclick=\"javascript: selectPlayer(" + iPl.ToString() + ")\" style=\"position: absolute; ";
          sDiv += "top: "    + fTop   .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
          sDiv += "left: "   + fLeft  .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
          sDiv += "height: " + fHeight.ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
          sDiv += "width: "  + fWidth .ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; ";
          sDiv += "border: 4px solid blue; ";
          sDiv += "background-color: white; ";
          sDiv += "-webkit-border-radius: 50%; ";
          sDiv += "-moz-border-radius: 50%; ";
          sDiv += "cursor: pointer; ";
          sDiv += "-webkit-box-shadow: 0px 0px 8px 8px rgba(0, 0, 0, .3); box-shadow: 0px 0px 8px 8px rgba(0, 0, 0, .3)";
          sDiv += "\">";

          int iFontSize = 230;
          if (bMobile) iFontSize = 125;
          sDiv += "<b style=\"position: absolute; width: 100%; text-align:center; font-size:" + iFontSize.ToString() + "%; color: black\">";
          sDiv += pl.iNr.ToString();
          sDiv += "</b>";
          sDiv += "</div>";
        }

        iPl++;
      }

      return sDiv;
    }

    private string TeamGetPlayerRadiusOfAction(int iPlayerIndex, CornerkickManager.Club club)
    {
      string sDiv = "";

      if (iPlayerIndex < 0 || iPlayerIndex >= MvcApplication.ckcore.game.data.nPlStart || iPlayerIndex >= club.ltPlayerId.Count) return "";

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPlayerIndex]];

      int iXOffence = MvcApplication.ckcore.game.tl.getXPosOffence(pl, club.tactic.fOrientation);

      for (double fRoa = 0.5; fRoa < 1.01; fRoa += 0.1) {
        System.Drawing.Point ptRoaTL = MvcApplication.ckcore.game.tl.getRndPos(pl, club.tactic.fOrientation, +1, -1, fRoa, fRoa);
        System.Drawing.Point ptRoaBR = MvcApplication.ckcore.game.tl.getRndPos(pl, club.tactic.fOrientation, -1, +1, fRoa, fRoa);

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
      sDiv += "<div style=\"position: absolute; width: 8%; min-width: 40px; top: "
                + (fTopButton  - 0.100f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: " 
                + (fLeftButton - 0.030f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) 
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(1)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_up.png\"/>" +
              "</div>";
      sDiv += "<div style=\"position: absolute; width: 8%; min-width: 40px; top: "
                + (fTopButton  - 0.045f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: "
                + (fLeftButton - 0.030f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(2)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_down.png\"/>" +
              "</div>";
      sDiv += "<div style=\"position: absolute; width: 3%; min-width: 15px; top: "
                + (fTopButton  - 0.015f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: "
                + (fLeftButton - 0.120f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(3)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_left.png\"/>" +
              "</div>";
      sDiv += "<div style=\"position: absolute; width: 3%; min-width: 15px; top: "
                + (fTopButton  - 0.015f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; left: "
                + (fLeftButton - 0.070f).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture)
                + "; z-index: 10\"> " +
                "<img id=\"img_arrow_1\" onclick =\"javascript:moveRoa(4)\" style =\"position: relative; width: 100%; cursor: pointer\" src=\"/Content/Images/arrow_right.png\"/>" +
              "</div>";

      return sDiv;
    }

    private float[] TeamGetIndOrientationMinMax(int iPlayerIndex, CornerkickManager.Club club)
    {
      float[] fIndOrientationMinMax = new float[2];

      if (iPlayerIndex < 0 || iPlayerIndex >= MvcApplication.ckcore.game.data.nPlStart || iPlayerIndex >= club.ltPlayerId.Count) return fIndOrientationMinMax;

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[club.ltPlayerId[iPlayerIndex]];

      fIndOrientationMinMax[0] = MvcApplication.ckcore.game.tl.getXPosOffence(pl.ptPosDefault.X, club.tactic.fOrientation, -1f) / (float)MvcApplication.ckcore.game.ptPitch.X;
      fIndOrientationMinMax[1] = MvcApplication.ckcore.game.tl.getXPosOffence(pl.ptPosDefault.X, club.tactic.fOrientation, +1f) / (float)MvcApplication.ckcore.game.ptPitch.X;

      return fIndOrientationMinMax;
    }

    public ActionResult PlayerDetails(int i)
    {
      Models.PlayerModel plModel = new Models.PlayerModel();

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      plModel.iPlayer = i;
      CornerkickGame.Player plDetails = MvcApplication.ckcore.ltPlayer[i];
      plModel.iPlayerIndTr = plDetails.iIndTraining;

      plModel.bOwnPlayer = MvcApplication.ckcore.plr.ownPlayer(club, plDetails);
      plModel.bJouth = club.ltJugendspielerID.IndexOf(i) >= 0;

      plModel.iContractYears = 1;

      plModel.sName = plDetails.sName;

      plModel.ltDdlNo = new List<SelectListItem>();
      plModel.iNo = plDetails.iNr;

      plModel.sEmblem = getClubEmblem(plDetails.iClubId, "height: 100%; width: 100%; object-fit: contain");

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

      // Captain
      plModel.bCaptain  = i == club.iCaptainId[0];
      plModel.bCaptain2 = i == club.iCaptainId[1];

      // Doping
      plModel.ddlDoping = new List<SelectListItem>();
      byte iDp = 0;
      foreach (CornerkickGame.Player.Doping dp in MvcApplication.ckcore.ltDoping) {
        plModel.ddlDoping.Add(
          new SelectListItem {
            Text  = dp.sName,
            Value = iDp++.ToString()
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

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (club.iCaptainId == null) club.iCaptainId = new int[3];

      if (iC >= club.iCaptainId.Length) return Json("Error", JsonRequestBehavior.AllowGet);

      string sCaptain = "Kapitän";
      if (iC == 1) sCaptain = "Vize-Kapitän";

      MvcApplication.ckcore.plr.makePlayerCaptain(iPlayerId, iC, club);

      return Json("Sie haben " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " zum " + sCaptain + " ernannt.", JsonRequestBehavior.AllowGet);
    }

    public ActionResult getClubCaptain(int iC)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (club.iCaptainId[iC] >= 0) return Json(MvcApplication.ckcore.ltPlayer[club.iCaptainId[iC]].sName, JsonRequestBehavior.AllowGet);

      return Json("", JsonRequestBehavior.AllowGet);
    }

    public ActionResult PlayerDetailsDoDoping(int iPlayerId, byte iDp)
    {
      if (iPlayerId < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iDp >= MvcApplication.ckcore.ltDoping.Count) return Json("Error", JsonRequestBehavior.AllowGet);

      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlayerId];

      return Json(MvcApplication.ckcore.plr.doDoping(pl, MvcApplication.ckcore.ltDoping[iDp]), JsonRequestBehavior.AllowGet);
    }

    public ActionResult PlayerDetailsGetDopingDesc(byte iDp)
    {
      if (iDp >= MvcApplication.ckcore.ltDoping.Count) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickGame.Player.Doping dp = MvcApplication.ckcore.ltDoping[iDp];

      string sDesc = "Steigerung max. Kondition: " + dp.fEffectMax    .ToString("0.0%") + "<br>" +
                     "       Reduktionsrate / d: " + dp.fReductionRate.ToString("0.0%") + "<br>" +
                     "    Max. Detektionsrisiko: " + dp.fDetectable   .ToString("0.0%") + "<br>" +
                     "                   Kosten: " + dp.iCost.ToString("N0", getCi()) + " €";

      return Json(sDesc, JsonRequestBehavior.AllowGet);
    }

    public ContentResult PlayerDetailsGetCFM(int iPlId)
    {
      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

      Models.DataPointGeneral[] dataPoints = new Models.DataPointGeneral[4];

      float fCondiMax = 1f;
      if (pl.doping != null) fCondiMax += pl.doping.fEffect;
      dataPoints[0] = new Models.DataPointGeneral(0, fCondiMax,     "");
      dataPoints[1] = new Models.DataPointGeneral(0, pl.fCondition, pl.fCondition.ToString("0.00%"));
      dataPoints[2] = new Models.DataPointGeneral(1, pl.fFresh,     pl.fFresh.ToString("0.00%"));
      dataPoints[3] = new Models.DataPointGeneral(2, pl.fMoral,     pl.fMoral.ToString("0.00%"));

      //JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints), "application/json");
    }

    public ContentResult PlayerDetailsGetDevelopmentData(int iPlId)
    {
      CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[iPlId];

      List<Models.DataPointGeneral> dataPoints = new List<Models.DataPointGeneral>();

      foreach (CornerkickGame.Player.History hty in pl.ltHistory) {
        long iDate = convertDateTimeToTimestamp(hty.dt);
        dataPoints.Add(new Models.DataPointGeneral(iDate, hty.fStrength));
      }

      long iDateCurrent = convertDateTimeToTimestamp(MvcApplication.ckcore.dtDatum);
      dataPoints.Add(new Models.DataPointGeneral(iDateCurrent, MvcApplication.ckcore.game.tl.getAveSkill(pl, 0, false)));

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    [HttpPost]
    public JsonResult GetPlayerSalary(int iPlayerId, byte iYears, int iSalaryOffer = 0)
    {
      if (iPlayerId < 0) return Json("Invalid player",                    JsonRequestBehavior.AllowGet);
      if (iYears    < 1) return Json("Invalid number of contract length", JsonRequestBehavior.AllowGet);

      int iSalary = MvcApplication.ckcore.plr.getSalary(MvcApplication.ckcore.ltPlayer[iPlayerId], iYears);
      double fMood = 1.0;

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.csTransfer.TransferOffer offer = MvcApplication.ckcore.tr.getOffer(iPlayerId, club.iId);

      // get already negotiated contract (salary)
      if (offer.contract.iLength > 0) {
        if (offer.contract.iLength == iYears) {
          iSalary = offer.contract.iSalary;
        } else {
          double fFactorNego = offer.contract.iSalary / (double)MvcApplication.ckcore.plr.getSalary(MvcApplication.ckcore.ltPlayer[iPlayerId], offer.contract.iLength);
          iSalary = (int)(iSalary * fFactorNego);
        }

        fMood = offer.contract.fMood;
      } else { // new offer
        offer.iClubId = club.iId;
        offer.dt = MvcApplication.ckcore.dtDatum;
        offer.contract.iLength = iYears;
      }
      /*
      foreach (CornerkickManager.csTransfer.Transfer transfer in MvcApplication.ckcore.ltTransfer) {
        if (transfer.iPlayerId == iPlayerId) {
          if (transfer.ltOffers == null) break;

          foreach (CornerkickManager.csTransfer.TransferOffer offer in transfer.ltOffers) {
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

      offer.contract.fMood   = (float)fMood;
      offer.contract.iSalary = iSalary;

      if (iSalaryOffer > 0) MvcApplication.ckcore.tr.addChangeOffer(iPlayerId, offer);

      return Json(offer.contract, JsonRequestBehavior.AllowGet);
    }

    // iMode: 0 - Extention, 1 - new contract
    [HttpPost]
    public JsonResult NegotiatePlayerContract(int iId, int iYears, string sSalary, string sPlayerMood, int iMode)
    {
      // Initialize status code with ERROR
      Response.StatusCode = 1;

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
      float fPlayerMood = 1f;
      if (!float.TryParse(sPlayerMood, out fPlayerMood)) return Json("Error", JsonRequestBehavior.AllowGet);
      fPlayerMood /= 100f;

      string sReturn = "";
      if (iMode == 0) { // contract extention
        CornerkickGame.Player player = MvcApplication.ckcore.ltPlayer[iId];

        if (player.contract.iLength + iYears > 10) return Json("Error: Maximale Vertragslänge = 10 Jahre", JsonRequestBehavior.AllowGet);

        player.contract.iLength += (byte)iYears;
        player.contract.iSalary = iSalary;
        player.contract.fMood = fPlayerMood;
        MvcApplication.ckcore.ltPlayer[iId] = player;

        sReturn = "Der Vertrag mit " + player.sName + " wurde um " + iYears.ToString() + " Jahre verlängert.";
      } else { // new contract
        if (iYears > 10) return Json("Error: Maximale Vertragslänge = 10 Jahre", JsonRequestBehavior.AllowGet);

        // create new offer
        CornerkickManager.csTransfer.TransferOffer offer = new CornerkickManager.csTransfer.TransferOffer();
        CornerkickGame.Player.Contract contract = new CornerkickGame.Player.Contract();
        contract.iLength = (byte)iYears;
        contract.iSalary = iSalary;
        contract.fMood = fPlayerMood;
        offer.contract = contract;
        offer.iClubId = ckClub().iId;

        MvcApplication.ckcore.tr.addChangeOffer(iId, offer);
        sReturn = "Sie haben sich mit dem Spieler " + MvcApplication.ckcore.ltPlayer[iId].sName + " auf eine Zusammenarbeit über " + iYears.ToString() + " Jahre geeinigt.";
      }

      // Set status code to OK
      Response.StatusCode = 200;

      return Json(sReturn, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult ExtentPlayerContract(int iPlayer, int iYears, string sSalary)
    {
      if (iPlayer < 0) return Json("Error", JsonRequestBehavior.AllowGet);
      if (iYears  < 1) return Json("0",     JsonRequestBehavior.AllowGet);

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
      int iC = ckUser().iTeam;

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

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

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
        CornerkickManager.csTransfer.Transfer transfer = MvcApplication.ckcore.ltTransfer[iT];

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

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      int iClub = club.iId;

      for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
        CornerkickManager.csTransfer.Transfer transfer = MvcApplication.ckcore.ltTransfer[iT];

        if (transfer.iPlayerId == iPlayerId) {
          if (transfer.ltOffers != null) {
            for (int iO = 0; iO < transfer.ltOffers.Count; iO++) {
              CornerkickManager.csTransfer.TransferOffer offer = transfer.ltOffers[iO];
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

      CornerkickManager.Club clubTake = MvcApplication.ckcore.ltClubs[iClubId];
      if (MvcApplication.ckcore.ui.acceptTransferOffer(ckClub(), iPlayerId, clubTake)) {
        sReturn = "Sie haben das Transferangebot für dem Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " angenommen. Er wechselt mit sofortiger Wirkung zu " + clubTake.sName;
      }
      /*
      for (int iT = 0; iT < MvcApplication.ckcore.ltTransfer.Count; iT++) {
        CornerkickManager.csTransfer.Transfer transfer = MvcApplication.ckcore.ltTransfer[iT];

        if (transfer.iPlayerId == iPlayerId) {
          if (transfer.ltOffers != null) {
            for (int iO = 0; iO < transfer.ltOffers.Count; iO++) {
              CornerkickManager.csTransfer.TransferOffer offer = transfer.ltOffers[iO];
              if (offer.iClubId == iClubId) {
                CornerkickManager.Club clubUser = ckClub();
                clubUser.iKontostand += offer.iMoney;
                clubUser.ltPlayerId.Remove(iPlayerId);
                MvcApplication.ckcore.fz.setKonto(ref clubUser, MvcApplication.ckcore.dtDatum, +offer.iMoney, "Spielertransfer");
                AccountController.setCkClub(clubUser);

                CornerkickManager.Club clubTake = MvcApplication.ckcore.ltClubs[iClubId];
                clubTake.iKontostand -= offer.iMoney;
                clubTake.ltPlayerId.Add(iPlayerId);
                MvcApplication.ckcore.fz.setKonto(ref clubTake, MvcApplication.ckcore.dtDatum, -offer.iMoney, "Spielertransfer");
                MvcApplication.ckcore.Info("Ihr Transferangebot für den Spieler " + MvcApplication.ckcore.ltPlayer[iPlayerId].sName + " von " + offer.iMoney.ToString("N0", getCi()) + " wurde angenommen!", clubTake.iUser, 3, 0, clubTake.iUser);
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

      if (MvcApplication.ckcore.ui.cancelTransferOffer(iPlayerId, ckClub())) {
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
      foreach (CornerkickManager.csTransfer.Transfer transfer in MvcApplication.ckcore.ui.filterTransferlist("", -1, iPos, -1f, 0, iFType, iFValue, bJouth)) {
        CornerkickGame.Player plTr = MvcApplication.ckcore.ltPlayer[transfer.iPlayerId];

        CornerkickManager.Club club = MvcApplication.ckcore.ltClubs[plTr.iClubId];

        string sClub = "vereinslos";
        if (plTr.iClubId >= 0) {
          sClub = club.sName;
        }

        int iOffer = 0;
        CornerkickManager.Club clubUser = ckClub();
        if (clubUser.iId > 0) {
          if      (MvcApplication.ckcore.tr .negotiationCancelled(clubUser, plTr)) iOffer = -1;
          else if (MvcApplication.ckcore.tr .alreadyOffered      (clubUser, plTr)) iOffer = +1;
          else if (MvcApplication.ckcore.plr.ownPlayer           (clubUser, plTr)) iOffer = +2;
        }

        ltDeTransfer.Add(new Models.DatatableEntryTransfer {
          playerId = transfer.iPlayerId,
          empty = "",
          iOffer = iOffer,
          index = (iTr + 1).ToString(),
          datum = transfer.dt.ToString("d", getCi()),
          name = plTr.sName,
          position = MvcApplication.ckcore.plr.getStrPos(plTr),
          strength = MvcApplication.ckcore.game.tl.getAveSkill(plTr, 0, true).ToString("0.0"),
          strengthIdeal = MvcApplication.ckcore.game.tl.getAveSkill(plTr, 0, false).ToString("0.0"),
          age = plTr.getAge(MvcApplication.ckcore.dtDatum).ToString("0"),
          talent = (plTr.iTalent + 1).ToString(),
          mw = (MvcApplication.ckcore.plr.getValue(plTr) * 1000).ToString("N0", getCi()),
          club = sClub,
          nat = MvcApplication.ckcore.sLandShort[plTr.iNat1]
        });

        iTr++;
      }

      return Json(new { aaData = ltDeTransfer.ToArray() }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public ActionResult getTableTransferDetails2(int iPlayerId)
    {
      string sTable = "";

      CornerkickManager.Club clubUser = ckClub();

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
      foreach (CornerkickManager.csTransfer.Transfer transfer in MvcApplication.ckcore.ltTransfer) {
        if (transfer.iPlayerId == iPlayerId) {
          if (transfer.ltOffers == null) break;

          bool bOwnPlayer = MvcApplication.ckcore.plr.ownPlayer(clubUser, MvcApplication.ckcore.ltPlayer[iPlayerId]);

          foreach (CornerkickManager.csTransfer.TransferOffer offer in transfer.ltOffers) {
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
              sTable += "<td align=\"center\">" + offer.dt.ToString("d", getCi()) + "</td>";
              sTable += "<td align=\"center\">" + sClub + "</td>";
              sTable += "<td align=\"right\">" + offer.iFee.ToString("N0", getCi()) + " €" + "</td>";

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
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(tactic);
      tactic.tactic = clb.tactic;

      string[] sStandards = new string[4] { "11m", "Freistoß", "Ecke R.", "Ecke L." };

      tactic.ltDdlStandards = new List<SelectListItem>[4];
      for (byte iS = 0; iS < 4; iS++) {
        tactic.ltDdlStandards[iS] = new List<SelectListItem>();
        tactic.ltDdlStandards[iS].Add(new SelectListItem { Text = "auto (" + sStandards[iS] + ")", Value = "-1" });
        for (byte iPl = 0; iPl < MvcApplication.ckcore.game.data.nPlStart; iPl++) {
          if (clb.ltPlayerId.Count <= iPl) break;

          CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[clb.ltPlayerId[iPl]];
          tactic.ltDdlStandards[iS].Add(new SelectListItem { Text = pl.sName, Value = iPl.ToString(), Selected = iPl == clb.tactic.iStandards[iS] });
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
          if (iAS < clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned.Count) tactic.iAutoSubsMin[iAS] = clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][2];
        }
      }

      return View(tactic);
    }

    public ActionResult setTaktik(int iTaktik, float fTaktik)
    {
      float fRet = 0f;

      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if      (iTaktik == 0) clb.tactic.fOrientation = fTaktik;
      else if (iTaktik == 1) clb.tactic.fPower       = fTaktik;
      else if (iTaktik == 2) clb.tactic.fShootFreq   = fTaktik;
      else if (iTaktik == 3) clb.tactic.fAggressive  = fTaktik;
      else if (iTaktik == 4) clb.tactic.fPassRisk    = fTaktik;
      else if (iTaktik == 5) clb.tactic.fPassLength  = fTaktik;
      else if (iTaktik == 6) clb.tactic.fPassFreq    = fTaktik;
      else if (iTaktik == 7) {
        clb.tactic.fPassLeft  = fTaktik;
        if (clb.tactic.fPassLeft + clb.tactic.fPassRight > 1f) clb.tactic.fPassRight = (float)Math.Round(1f - clb.tactic.fPassLeft,  2);
        fRet = clb.tactic.fPassRight;
      }
      else if (iTaktik == 8) {
        clb.tactic.fPassRight = fTaktik;
        if (clb.tactic.fPassLeft + clb.tactic.fPassRight > 1f) clb.tactic.fPassLeft  = (float)Math.Round(1f - clb.tactic.fPassRight, 2);
        fRet = clb.tactic.fPassLeft;
      } else if (iTaktik == 9) clb.tactic.iAngriffAbseits = (int)Math.Round(fTaktik);

      // Set tactic of current game
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].tc = clb.tactic;
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].tc = clb.tactic;
        }
      }

      return Json(fRet, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setStandards(int iStandard, int iIndexPlayer)
    {
      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.tactic.iStandards[iStandard] = iIndexPlayer;

      // Set tactic of current game
      if (usr.game != null) {
        if (!usr.game.data.bFinished) {
          if      (usr.game.data.team[0].iTeamId == clb.iId) usr.game.data.team[0].tc = clb.tactic;
          else if (usr.game.data.team[1].iTeamId == clb.iId) usr.game.data.team[1].tc = clb.tactic;
        }
      }

      return Json("", JsonRequestBehavior.AllowGet);
    }

    public void fillDdlAutoSubs(Models.TaktikModel tactic)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return;

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
          if (iAS < clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned.Count) {
            if (iPl == clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][0] || iPl == clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][1]) bSelected = true;
          }

          string sPos = "";
          if (iPl < MvcApplication.ckcore.game.data.nPlStart) sPos = MvcApplication.ckcore.sPosition[MvcApplication.ckcore.game.tl.getBasisPos(MvcApplication.ckcore.game.tl.getPosRole(pl))];
          else                                           sPos = MvcApplication.ckcore.plr.getStrPos(pl);
          string sStrength = MvcApplication.ckcore.game.tl.getAveSkill(pl, 0, true).ToString(" (0.0)");
          SelectListItem sliAutoSub = new SelectListItem { Text = pl.sName + " - " + sPos + sStrength,
                                                           Value = iPl.ToString(),
                                                           Selected = bSelected
                                                         };

          if (iPl < MvcApplication.ckcore.game.data.nPlStart) {
            if (!checkIfAlreadyInDdl(iPl, tactic.ddlAutoSubsOut, iAS)) tactic.ddlAutoSubsOut[iAS].Add(sliAutoSub);
          } else {
            if (!checkIfAlreadyInDdl(iPl, tactic.ddlAutoSubsIn,  iAS)) tactic.ddlAutoSubsIn[iAS].Add(sliAutoSub);
          }
        }

        tactic.iAutoSubsMin[iAS] = 60;
        if (iAS < clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned.Count) tactic.iAutoSubsMin[iAS] = clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][2];
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

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.nextGame == null) Json(sBox, JsonRequestBehavior.AllowGet);

      byte iHA = 0;
      if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;

      sBox[0] += "<select name=\"sAutoSubsOut" + iAS.ToString() + "\" class=\"form-control\" id =\"ddlAutoSubOut" + iAS.ToString() + "\" onchange =\"setAutoSubs(" + iAS.ToString() + ")\"><option value=\"-1\">aus</option>";
      sBox[1] += "<select name=\"sAutoSubsIn" + iAS.ToString() + "\" class=\"form-control\" id =\"ddlAutoSubIn" + iAS.ToString() + "\" onchange =\"setAutoSubs(" + iAS.ToString() + ")\"><option value=\"-1\">aus</option>";

      // foreach player
      for (int iPl = 0; iPl < MvcApplication.ckcore.game.data.nPlStart + MvcApplication.ckcore.game.data.nPlRes; iPl++) {
        CornerkickGame.Player pl = MvcApplication.ckcore.ltPlayer[clb.ltPlayerId[iPl]];

        bool bOut = iPl < MvcApplication.ckcore.game.data.nPlStart;

        bool bContinue = false;
        byte jAS = 0;
        while (jAS < iAS) {
          if ( bOut && iPl == clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[jAS][0]) bContinue = true;
          if (!bOut && iPl == clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[jAS][1]) bContinue = true;
          jAS++;
        }
        if (bContinue) continue;

        string sSelectedO = "";
        string sSelectedI = "";
        if (iAS < clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned.Count) {
          if (clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][1] > clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][0]) {
            if (iPl == clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][0]) sSelectedO = " selected=\"selected\"";
            if (iPl == clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][1]) sSelectedI = " selected=\"selected\"";
          }
        }

        string sPos = "";
        if (iPl < MvcApplication.ckcore.game.data.nPlStart) sPos = MvcApplication.ckcore.sPosition[MvcApplication.ckcore.game.tl.getBasisPos(MvcApplication.ckcore.game.tl.getPosRole(pl))];
        else                                                sPos = MvcApplication.ckcore.plr.getStrPos(pl);
        string sStrength = MvcApplication.ckcore.game.tl.getAveSkill(pl).ToString(" (0.0)");

        if (bOut) sBox[0] += "<option" + sSelectedO + " value=\"" + iPl.ToString() + "\">" + pl.sName + " - " + sPos + sStrength + "</option>";
        else      sBox[1] += "<option" + sSelectedI + " value=\"" + iPl.ToString() + "\">" + pl.sName + " - " + sPos + sStrength + "</option>";
      }

      sBox[0] += "</select>";
      sBox[1] += "</select>";

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public ActionResult setAutoSubs(int iAS, int iIndexPlayerOut, int iIndexPlayerIn, byte iMin)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.nextGame == null) Json(false, JsonRequestBehavior.AllowGet);

      byte iHA = 0;
      if (clb.nextGame.team[1].iTeamId == clb.iId) iHA = 1;

      while (clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned.Count <= iAS) clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned.Add(new byte[3] { 0, 0, 0 });

      if (iIndexPlayerOut < 0 || iIndexPlayerIn < 0) {
        iIndexPlayerOut = 0;
        iIndexPlayerIn  = 0;
      }
      clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][0] = (byte)iIndexPlayerOut;
      clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][1] = (byte)iIndexPlayerIn;
      clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[iAS][2] = iMin;

      bool bValid = iIndexPlayerOut >= 0 && iIndexPlayerIn >= 0 && iIndexPlayerOut != iIndexPlayerIn && iMin >= 0;

      if (!bValid) {
        int jAS = iAS + 1;
        while (jAS < 3) {
          clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[jAS][0] = 0;
          clb.nextGame.team[iHA].tc.ltSubstitutionsPlanned[jAS][1] = 0;
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
    public ActionResult Training(Models.TrainingModel mdTraining)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdTraining);

      mdTraining.training = clb.training;

      return View(mdTraining);
    }

    public ActionResult setTraining(int iTraining, int iTag)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

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
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(stadionModel);

      if (clb.stadium.blocks == null) return View(stadionModel);

      stadionModel.stadion = clb.stadium;

      stadionModel.sName = clb.stadium.sName;

      stadionModel.iSeats = new int[clb.stadium.blocks.Length];
      for (int i = 0; i < clb.stadium.blocks.Length; i++) {
        stadionModel.iSeats[i] = clb.stadium.blocks[i].iSeats;
      }

      stadionModel.iSeatType = new int[clb.stadium.blocks.Length];
      for (int i = 0; i < clb.stadium.blocks.Length; i++) {
        stadionModel.iSeatType[i] = clb.stadium.blocks[i].iType;
      }

      stadionModel.iSeatsBuild = new int[clb.stadium.blocks.Length];
      for (int i = 0; i < clb.stadium.blocks.Length; i++) {
        stadionModel.iSeatsBuild[i] = clb.stadium.blocks[i].iSeatsDaysConstruct;
      }

      stadionModel.bOberring = bStadiumGetTopring(clb);

      stadionModel.iVideo = clb.stadium.iVideoNew;

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
      CornerkickManager.Club clb = ckClub();

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
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadium = convertToStadion(iSeats, iArt, null);
      stadium.bTopring = clb.stadium.bTopring;
      stadium.iVideoNew    = clb.stadium.iVideoNew;
      stadium.iSnackbarNew = clb.stadium.iSnackbarNew;
      stadium.iToiletsNew  = clb.stadium.iToiletsNew;
      stadium.iCarparkNew  = clb.stadium.iCarparkNew;
      stadium.iTicketcounterNew = clb.stadium.iTicketcounterNew;

      int[] iKostenDauer = MvcApplication.ckcore.st.getCostDaysContructStadium(stadium, clb.stadium, ckUser());
      int iDispoOk = 0;
      if (MvcApplication.ckcore.fz.checkDispoLimit(iKostenDauer[0], clb)) iDispoOk = 1;

      string[] sKostenDauer = new string[] { iKostenDauer[0].ToString("N0", getCi()), iKostenDauer[1].ToString(), iDispoOk.ToString() };

      return Json(sKostenDauer, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumChangeSet(int[] iSeats, int[] iArt)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadium = convertToStadion(iSeats, iArt, null);

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadium);

      return Json("Der Ausbau des Stadions wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetCostTopring()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.bTopring = !clb.stadium.bTopring;

      int[] iKostenDauer = MvcApplication.ckcore.st.getCostDaysContructStadium(stadion, clb.stadium, ckUser());
      int iDispoOk = 0;
      if (MvcApplication.ckcore.fz.checkDispoLimit(iKostenDauer[0], clb)) iDispoOk = 1;

      string[] sKostenDauer = new string[] { (iKostenDauer[0] / 1000000).ToString("N0", getCi()) + " mio. €", iKostenDauer[1].ToString(), iDispoOk.ToString() };

      return Json(sKostenDauer, JsonRequestBehavior.AllowGet);
    }

    private bool bStadiumGetTopring(CornerkickManager.Club clb)
    {
      return clb.stadium.bTopring && clb.stadium.iTopringDaysConstruct == 0;
    }

    [HttpPost]
    public JsonResult StadiumGetTopring()
    {
      CornerkickManager.Club clb = ckClub();
      return Json(bStadiumGetTopring(clb), JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetBlockProgress()
    {
      CornerkickManager.Club clb = ckClub();

      int nBlocksMax = clb.stadium.blocks.Length;
      if (!clb.stadium.bTopring) nBlocksMax = 10;
      float[] fPg = new float[nBlocksMax];

      for (byte iB = 0; iB < nBlocksMax; iB++) {
        int iDaysMax = MvcApplication.ckcore.st.getCostDaysContructStadiumBlock(clb.stadium.blocks[iB], clb.stadium.blocks[iB], 0, ckUser())[0];
        if (clb.stadium.blocks[iB].iSeatsDaysConstruct > 0) fPg[iB] = (clb.stadium.blocks[iB].iSeatsDaysConstructIni - clb.stadium.blocks[iB].iSeatsDaysConstruct) / (float)clb.stadium.blocks[iB].iSeatsDaysConstructIni;
        else                                                fPg[iB] = -1f;
      }

      return Json(fPg, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildTopring()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.bTopring = !clb.stadium.bTopring;

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadion);

      return Json("Der Bau des Oberrings wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    // Video-wall
    [HttpPost]
    public JsonResult StadiumGetCostVideo(byte iLevel)
    {
      string[] sCostDaysDispo = new string[3] { "0", "0", "0" };
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.stadium.iVideoNew != iLevel) {
        int iDispoOk = 0;
        if (MvcApplication.ckcore.fz.checkDispoLimit(CornerkickManager.csStadion.iVideoCost[iLevel], clb)) iDispoOk = 1;

        sCostDaysDispo[0] = CornerkickManager.csStadion.iVideoCost[iLevel].ToString("N0", getCi());
        sCostDaysDispo[1] = CornerkickManager.csStadion.iVideoDaysConstruct[iLevel].ToString();
        sCostDaysDispo[2] = iDispoOk.ToString();
      }

      return Json(sCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildVideo(byte iLevel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.iVideoNew = iLevel;

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadion);

      return Json("Der Bau der Anzeigentafel wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    // Snackbars
    [HttpPost]
    public JsonResult StadiumGetCostSnackbar(byte iCount)
    {
      string[] sCostDaysDispo = new string[3] { "0", "0", "0" };

      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.stadium.iSnackbarNew != iCount) {
        int iDispoOk = 0;
        int[] iCostDays = MvcApplication.ckcore.st.getCostDaysContructSnackbar(iCount, clb.stadium.iSnackbarNew, usr);
        if (MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

        sCostDaysDispo[0] = iCostDays[0].ToString("N0", getCi());
        sCostDaysDispo[1] = iCostDays[1].ToString();
        sCostDaysDispo[2] = iDispoOk.ToString();
      }

      return Json(sCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildSnackbar(byte iCount)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.iSnackbarNew = iCount;

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadion);

      return Json("Der Ausbau der Imbissbuden wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    // Toilets
    [HttpPost]
    public JsonResult StadiumGetCostToilets(byte iCount)
    {
      string[] sCostDaysDispo = new string[3] { "0", "0", "0" };

      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (clb.stadium.iToiletsNew != iCount) {
        int iDispoOk = 0;
        int[] iCostDays = MvcApplication.ckcore.st.getCostDaysContructToilets(iCount, clb.stadium.iToiletsNew, usr);
        if (MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

        sCostDaysDispo[0] = iCostDays[0].ToString("N0", getCi());
        sCostDaysDispo[1] = iCostDays[1].ToString();
        sCostDaysDispo[2] = iDispoOk.ToString();
      }

      return Json(sCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildToilets(byte iCount)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadion = clb.stadium.Clone();
      stadion.iToiletsNew = iCount;

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadion);

      return Json("Der Ausbau der Toiletten wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    internal CornerkickGame.Stadium convertToStadion(int[] iSeats, int[] iSeatType, int[] iSeatsBuild)
    {
      CornerkickGame.Stadium stadium = new CornerkickGame.Stadium();
      if (iSeats != null) {
        for (int i = 0; i < iSeats.Length; i++) stadium.blocks[i].iSeats = iSeats[i];
      }
      if (iSeatType != null) {
        for (int i = 0; i < iSeatType.Length; i++) stadium.blocks[i].iType = (byte)iSeatType[i];
      }
      if (iSeatsBuild != null) {
        for (int i = 0; i < iSeatsBuild.Length; i++) stadium.blocks[i].iSeatsDaysConstruct = iSeatsBuild[i];
      }

      return stadium;
    }

    [HttpPost]
    public JsonResult StadiumRenewPitchCost()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      return Json(MvcApplication.ckcore.st.getCostStadiumRenewPitch(clb.stadium, 0.1f, ckUser()).ToString("N0", getCi()), JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumRenewPitch()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      MvcApplication.ckcore.ui.renewStadiumPitch(ref clb, 0.1f);

      if (clb.iUser >= 0) MvcApplication.ckcore.ltClubs[MvcApplication.ckcore.ltUser[clb.iUser].iTeam] = clb;

      return Json("Der Stadionrasen wurde erneuert", JsonRequestBehavior.AllowGet);
    }

    public void StadiumSetName(string sName)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return;

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
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(mdStadionSurr);

      mdStadionSurr.ddlTrainingsgel = new List<SelectListItem>();
      mdStadionSurr.ddlJouthInternat = new List<SelectListItem>();

      for (int i = clb.iTrainingsgel  [1] + 1; i < MvcApplication.ckcore.st.sTrainingsgel.Length; i++) mdStadionSurr.ddlTrainingsgel .Add(new SelectListItem { Text = MvcApplication.ckcore.st.sTrainingsgel [i], Value = i.ToString() });
      for (int i = clb.iJugendinternat[1] + 1; i < MvcApplication.ckcore.st.sTrainingsgel.Length; i++) mdStadionSurr.ddlJouthInternat.Add(new SelectListItem { Text = MvcApplication.ckcore.st.sJouthInternat[i], Value = i.ToString() });

      mdStadionSurr.iTrainingsgel     = clb.iTrainingsgel  [0];
      mdStadionSurr.iTrainingNew      = clb.iTrainingsgel  [2];
      mdStadionSurr.iJouthInternat    = clb.iJugendinternat[0];
      mdStadionSurr.iJouthInternatNew = clb.iJugendinternat[2];
      mdStadionSurr.iCarpark       = Math.Max(clb.stadium.iCarpark, clb.stadium.iCarparkNew);
      mdStadionSurr.iCarparkNew    = clb.stadium.iCarparkNew;
      mdStadionSurr.iCounter       = Math.Max(clb.stadium.iTicketcounter, clb.stadium.iTicketcounterNew);
      mdStadionSurr.iCounterNew    = clb.stadium.iTicketcounterNew;

      return View(mdStadionSurr);
    }

    [HttpPost]
    public JsonResult StadiumGetSurrGetCurrent()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickManager.User usr = ckUser();

      int nCarparkDaysConstract       = MvcApplication.ckcore.st.getCostDaysContructCarpark      (clb.stadium.iCarparkNew,       clb.stadium.iCarpark,       usr)[1];
      int nTicketcounterDaysConstract = MvcApplication.ckcore.st.getCostDaysContructTicketcounter(clb.stadium.iTicketcounterNew, clb.stadium.iTicketcounter, usr)[1];
      return Json(new string[4][] {
        new string [4] { MvcApplication.ckcore.st.sTrainingsgel [clb.iTrainingsgel  [0]], MvcApplication.ckcore.st.sTrainingsgel [clb.iTrainingsgel  [1]], clb.iTrainingsgel  [2].ToString(), ((MvcApplication.ckcore.st.iTrainingsgelDaysConstruct [clb.iTrainingsgel  [1]] - clb.iTrainingsgel  [2]) / (float)MvcApplication.ckcore.st.iTrainingsgelDaysConstruct [clb.iTrainingsgel  [1]]).ToString("0.0%") },
        new string [4] { MvcApplication.ckcore.st.sJouthInternat[clb.iJugendinternat[0]], MvcApplication.ckcore.st.sJouthInternat[clb.iJugendinternat[1]], clb.iJugendinternat[2].ToString(), ((MvcApplication.ckcore.st.iJouthInternatDaysConstruct[clb.iJugendinternat[1]] - clb.iJugendinternat[2]) / (float)MvcApplication.ckcore.st.iJouthInternatDaysConstruct[clb.iJugendinternat[1]]).ToString("0.0%") },
        new string [4] { clb.stadium.iCarpark      .ToString(), clb.stadium.iCarparkNew      .ToString(), clb.stadium.iCarparkDaysConstruct      .ToString(), ((nCarparkDaysConstract       - clb.stadium.iCarparkDaysConstruct      ) / (float)nCarparkDaysConstract      ).ToString("0.0%") },
        new string [4] { clb.stadium.iTicketcounter.ToString(), clb.stadium.iTicketcounterNew.ToString(), clb.stadium.iTicketcounterDaysConstruct.ToString(), ((nTicketcounterDaysConstract - clb.stadium.iTicketcounterDaysConstruct) / (float)nTicketcounterDaysConstract).ToString("0.0%") }
      }, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumGetCostSurr(int i, byte iType)
    {
      string[] sCostDaysDispo = new string[4] { "0", "0", "0", "0" };

      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      if (iType == 0) {
        if (clb.iTrainingsgel[0] != i) {
          int iDispoOk = 0;
          if (MvcApplication.ckcore.fz.checkDispoLimit(MvcApplication.ckcore.st.iTrainingsgelCost[i], clb)) iDispoOk = 1;

          sCostDaysDispo[0] = MvcApplication.ckcore.st.iTrainingsgelCost[i].ToString("N0", getCi());
          sCostDaysDispo[1] = MvcApplication.ckcore.st.iTrainingsgelDaysConstruct[i].ToString();
          sCostDaysDispo[2] = iDispoOk.ToString();
        }
      } else if (iType == 1) {
        if (clb.iJugendinternat[0] != i) {
          int iDispoOk = 0;
          if (MvcApplication.ckcore.fz.checkDispoLimit(MvcApplication.ckcore.st.iJouthInternatCost[i], clb)) iDispoOk = 1;

          sCostDaysDispo[0] = MvcApplication.ckcore.st.iJouthInternatCost[i].ToString("N0", getCi());
          sCostDaysDispo[1] = MvcApplication.ckcore.st.iJouthInternatDaysConstruct[i].ToString();
          sCostDaysDispo[2] = iDispoOk.ToString();
        }
      } else if (iType == 2) {
        if (clb.stadium.iCarparkNew != i) {
          int iDispoOk = 0;

          int[] iCostDays = MvcApplication.ckcore.st.getCostDaysContructCarpark(i, clb.stadium.iCarpark, usr);
          if (MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

          sCostDaysDispo[0] = iCostDays[0].ToString("N0", getCi());
          sCostDaysDispo[1] = iCostDays[1].ToString();
          sCostDaysDispo[2] = iDispoOk.ToString();
        }
      } else if (iType == 3) {
        if (clb.stadium.iTicketcounterNew != i) {
          int iDispoOk = 0;

          int[] iCostDays = MvcApplication.ckcore.st.getCostDaysContructTicketcounter(i, clb.stadium.iTicketcounter, usr);
          if (MvcApplication.ckcore.fz.checkDispoLimit(iCostDays[0], clb)) iDispoOk = 1;

          sCostDaysDispo[0] = iCostDays[0].ToString("N0", getCi());
          sCostDaysDispo[1] = iCostDays[1].ToString();
          sCostDaysDispo[2] = iDispoOk.ToString();
        }
      }

      return Json(sCostDaysDispo, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildTrGel(int iTrGel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.iTrainingsgel[1] = iTrGel;
      clb.iTrainingsgel[2] = MvcApplication.ckcore.st.iTrainingsgelDaysConstruct[iTrGel];

      MvcApplication.ckcore.fz.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -MvcApplication.ckcore.st.iTrainingsgelCost[iTrGel], "Bau Trainingsgelände", CornerkickManager.Finance.iTransferralTypePayStadiumSurr);

      return Json("Der Bau des Trainingsgeländes wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildJouthInt(byte iInt)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.iJugendinternat[1] = iInt;
      clb.iJugendinternat[2] = MvcApplication.ckcore.st.iJouthInternatDaysConstruct[iInt];

      MvcApplication.ckcore.fz.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -MvcApplication.ckcore.st.iJouthInternatCost[iInt], "Bau Jugendinternat", CornerkickManager.Finance.iTransferralTypePayStadiumSurr);

      return Json("Der Bau des Jugendinternats wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildPark(int iCount)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.iCarparkNew = iCount;

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadiumNew);

      return Json("Der Bau der Parkplätze wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult StadiumBuildCounter(byte iCount)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      CornerkickGame.Stadium stadiumNew = clb.stadium.Clone();
      stadiumNew.iTicketcounterNew = iCount;

      MvcApplication.ckcore.ui.buildStadion(ref clb, stadiumNew);

      return Json("Der Bau der Ticketschalter wurde in Auftrag gegeben", JsonRequestBehavior.AllowGet);
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
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(personal);

      personal.personal = clb.personal;

      personal.ltDdlPersonalCoachCo      = new List<SelectListItem>();
      personal.ltDdlPersonalCoachCondi   = new List<SelectListItem>();
      personal.ltDdlPersonalMasseur       = new List<SelectListItem>();
      personal.ltDdlPersonalMental        = new List<SelectListItem>();
      personal.ltDdlPersonalMed           = new List<SelectListItem>();
      personal.ltDdlPersonalJouthCoach    = new List<SelectListItem>();
      personal.ltDdlPersonalJouthScouting = new List<SelectListItem>();
      personal.ltDdlPersonalKibitzer      = new List<SelectListItem>();

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalCoachCo.Add(new SelectListItem{ Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostCoachCo[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalCoachCo.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalCoachCondi.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostCoachCondi[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalCoachCondi.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalMasseur.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostMasseur[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalMasseur.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalMental.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostMental[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalMental.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalMed.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostMed[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalMed.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalJouthCoach.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostJouthCoach[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalJouthCoach.Add(new SelectListItem { Text = "-", Value = "0" });

      for (byte i = 7; i > 0; i--) personal.ltDdlPersonalJouthScouting.Add(new SelectListItem { Text = "Level: " + i.ToString() + " - " + CornerkickManager.Finance.iCostJouthScouting[i].ToString("N0", getCi()) + " €/Monat", Value = i.ToString() });
      personal.ltDdlPersonalJouthScouting.Add(new SelectListItem { Text = "-", Value = "0" });

      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 4 - " + CornerkickManager.Finance.iCostKibitzer[4].ToString("N0", getCi()) + " €/Monat", Value = "4" });
      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 3 - " + CornerkickManager.Finance.iCostKibitzer[3].ToString("N0", getCi()) + " €/Monat", Value = "3" });
      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 2 - " + CornerkickManager.Finance.iCostKibitzer[2].ToString("N0", getCi()) + " €/Monat", Value = "2" });
      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 1 - " + CornerkickManager.Finance.iCostKibitzer[1].ToString("N0", getCi()) + " €/Monat", Value = "1" });
      personal.ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "-", Value = "0" });

      return View(personal);
    }

    [HttpPost]
    public JsonResult PersonalCheckCost(int[] iLevel)
    {
      CornerkickManager.Club clb = new CornerkickManager.Club();

      clb.personal.iTrainerCo      = (byte)iLevel[0];
      clb.personal.iTrainerKondi   = (byte)iLevel[1];
      clb.personal.iMasseur        = (byte)iLevel[2];
      clb.personal.iTrainerMental  = (byte)iLevel[3];
      clb.personal.iArzt           = (byte)iLevel[4];
      clb.personal.iJugendTrainer  = (byte)iLevel[5];
      clb.personal.iJugendScouting = (byte)iLevel[6];
      clb.personal.iKibitzer       = (byte)iLevel[7];

      int iKosten = (int)(MvcApplication.ckcore.tl.getStuffSalary(clb) / 12f);
      string sKosten = iKosten.ToString("N0", getCi());

      sKosten = "Kosten: " + sKosten + " €/Monat";

      // Add personal pay-off costs
      int iPayOff = getPersonalPayOff(iLevel);
      if (iPayOff > 0) sKosten += ", Abfindungen: " + iPayOff.ToString("N0", getCi()) + " €";

      return Json(sKosten, JsonRequestBehavior.AllowGet);
    }

    private int getPersonalPayOff(int[] iLevelNew)
    {
      int iPayOff = 0;
      for (byte iP = 0; iP < 8; iP++) iPayOff += getPersonalPayOff(iP, iLevelNew[iP]);

      return iPayOff;
    }

    private int getPersonalPayOff(int iPersonal, int iLevelNew)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return 0;

      int iMoney = 0;
      int iMonthDiff = (MvcApplication.ckcore.dtSeasonEnd.Month - MvcApplication.ckcore.dtDatum.Month) + (12 * (MvcApplication.ckcore.dtSeasonEnd.Year - MvcApplication.ckcore.dtDatum.Year));
      if      (iPersonal == 0 && iLevelNew != clb.personal.iTrainerCo)      iMoney = iMonthDiff * (CornerkickManager.Finance.iCostCoachCo      [clb.personal.iTrainerCo]      / 2);
      else if (iPersonal == 1 && iLevelNew != clb.personal.iTrainerKondi)   iMoney = iMonthDiff * (CornerkickManager.Finance.iCostCoachCondi   [clb.personal.iTrainerKondi]   / 2);
      else if (iPersonal == 2 && iLevelNew != clb.personal.iMasseur)        iMoney = iMonthDiff * (CornerkickManager.Finance.iCostMasseur      [clb.personal.iMasseur]        / 2);
      else if (iPersonal == 3 && iLevelNew != clb.personal.iTrainerMental)  iMoney = iMonthDiff * (CornerkickManager.Finance.iCostMental       [clb.personal.iTrainerMental]  / 2);
      else if (iPersonal == 4 && iLevelNew != clb.personal.iArzt)           iMoney = iMonthDiff * (CornerkickManager.Finance.iCostMed          [clb.personal.iArzt]           / 2);
      else if (iPersonal == 5 && iLevelNew != clb.personal.iJugendTrainer)  iMoney = iMonthDiff * (CornerkickManager.Finance.iCostJouthCoach   [clb.personal.iJugendTrainer]  / 2);
      else if (iPersonal == 6 && iLevelNew != clb.personal.iJugendScouting) iMoney = iMonthDiff * (CornerkickManager.Finance.iCostJouthScouting[clb.personal.iJugendScouting] / 2);
      else if (iPersonal == 7 && iLevelNew != clb.personal.iKibitzer)       iMoney = iMonthDiff * (CornerkickManager.Finance.iCostKibitzer     [clb.personal.iKibitzer]       / 2);

      return iMoney;
    }

    [HttpPost]
    public JsonResult PersonalHire(int[] iLevel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      // First: Pay personal pay-off costs
      int iPayOff = getPersonalPayOff(iLevel);
      MvcApplication.ckcore.fz.doTransaction(ref clb, MvcApplication.ckcore.dtDatum, -iPayOff, "Abfindungen", CornerkickManager.Finance.iTransferralTypePaySalaryStaff);

      // Then, hire new personal
      clb.personal.iTrainerCo      = (byte)iLevel[0];
      clb.personal.iTrainerKondi   = (byte)iLevel[1];
      clb.personal.iMasseur        = (byte)iLevel[2];
      clb.personal.iTrainerMental  = (byte)iLevel[3];
      clb.personal.iArzt           = (byte)iLevel[4];
      clb.personal.iJugendTrainer  = (byte)iLevel[5];
      clb.personal.iJugendScouting = (byte)iLevel[6];
      clb.personal.iKibitzer       = (byte)iLevel[7];

      return Json("Neues Personal eingestellt!", JsonRequestBehavior.AllowGet);
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

        mdClub.sEmblem = getClubEmblem(mdClub.club, "height: 100%; width: 100%; object-fit: contain");

        mdClub.sRecordLWinH = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 1, +1, 0));
        mdClub.sRecordLWinA = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 1, +1, 1));
        mdClub.sRecordLDefH = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 1, -1, 0));
        mdClub.sRecordLDefA = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 1, -1, 1));

        mdClub.sRecordCWinH = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 2, +1, 0));
        mdClub.sRecordCWinA = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 2, +1, 1));
        mdClub.sRecordCDefH = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 2, -1, 0));
        mdClub.sRecordCDefA = getStringRecordGame(MvcApplication.ckcore.ui.getRecordGame(mdClub.club, 2, -1, 1));
      }

      return View(mdClub);
    }

    private string getClubEmblem(int iClubId, string sStyle = "")
    {
      CornerkickManager.Club clb = null;
      if (iClubId >= 0 && iClubId < MvcApplication.ckcore.ltClubs.Count) clb = MvcApplication.ckcore.ltClubs[iClubId];

      return getClubEmblem(clb, sStyle);
    }
    private string getClubEmblem(CornerkickManager.Club clb, string sStyle = "")
    {
      string sEmblem = "<img src=\"/Content/Uploads/emblems/";

      if (clb == null) return sEmblem + "0.png\" alt=\"Wappen\" " + sStyle + " title=\"" + clb.sName + "\"/>";

#if DEBUG
      string sEmblemFile = MvcApplication.getHomeDir() + "/Content/Uploads/" + clb.iId.ToString() + ".png";
#else
      string sEmblemFile = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~"), "Content", "Uploads", "emblems", clb.iId.ToString() + ".png");
#endif
      if (System.IO.File.Exists(sEmblemFile)) sEmblem += clb.iId.ToString();
      else                                    sEmblem += "0";

      if (!string.IsNullOrEmpty(sStyle)) sStyle = " style=\"" + sStyle + "\"";
      sEmblem += ".png\" alt=\"Wappen\"" + sStyle + " title=\"" + clb.sName + "\"/>";

      return sEmblem;
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
    public ActionResult League(Models.LeagueModels mlLeague)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) View(mlLeague);

      mlLeague.iClubId = clb.iId;

      mlLeague.iLand = clb.iLand;
      mlLeague.iSpKl = clb.iDivision;

      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, clb.iLand, clb.iDivision);
      if (league == null) return View(mlLeague);

      int iMd = MvcApplication.ckcore.tl.getMatchday(clb, MvcApplication.ckcore.dtDatum, 1);
      mlLeague.league = league;
      mlLeague.ltTbl  = MvcApplication.ckcore.tl.getLeagueTable(league, iMd - 1, 0);
      mlLeague.iLeagueSize = MvcApplication.ckcore.tl.getCupParticipants(league, iMd).Count;

      // Add lands to dropdown list
      foreach (int iLand in MvcApplication.iNations) {
        mlLeague.ddlLand.Add(new SelectListItem { Text = MvcApplication.ckcore.sLand[iLand], Value = iLand.ToString() });
      }

      return View(mlLeague);
    }

    public JsonResult getDdlMatchdays(ushort iSaison, int iLand, byte iDivision)
    {
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, iLand, iDivision);

      string[] ltMd = new string[league.getMatchdaysTotal()];
      // Spieltage zu Dropdown Menü hinzufügen
      for (int iMd = 0; iMd < ltMd.Length; iMd++) {
        ltMd[iMd] = (iMd + 1).ToString();
      }

      return Json(ltMd, JsonRequestBehavior.AllowGet);
    }

    public JsonResult LeagueGetMatchday()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(1, JsonRequestBehavior.AllowGet);

      // Get current matchday
      int iMd = MvcApplication.ckcore.tl.getMatchday(clb, MvcApplication.ckcore.dtDatum, 1);

      // Increment matchday match is if today or tomorrow
      CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clb, MvcApplication.ckcore.dtDatum, iGameType: 1);
      if (gdNext != null && (gdNext.dt.Date - MvcApplication.ckcore.dtDatum.Date).Days < 2) iMd++;

      // Limit to 1
      iMd = Math.Max(iMd, 1);

      return Json(iMd, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setLeague(Models.LeagueModels mlLeague, ushort iSaison, int iLand, byte iDivision, int iMatchday, byte iHA)
    {
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, iLand, iDivision);
      CornerkickManager.Club clb = ckClub();

      List<CornerkickManager.Tool.TableItem> ltTblLast = MvcApplication.ckcore.tl.getLeagueTable(league, iMatchday - 1, iHA);
      List<CornerkickManager.Tool.TableItem> ltTbl     = MvcApplication.ckcore.tl.getLeagueTable(league, iMatchday,     iHA);

      string sBox = "";
      for (var i = 0; i < ltTbl.Count; i++) {
        CornerkickManager.Tool.TableItem tbpl = ltTbl[i];
        int iGames = tbpl.iGUV[0] + tbpl.iGUV[1] + tbpl.iGUV[2];
        int iDiff = tbpl.iGoals - tbpl.iGoalsOpp;

        string sStyle = "";
        if (clb != null && tbpl.iId == clb.iId) sStyle = "style=\"font-weight:bold\" ";

        string sBgColor = "white";
        if      (i == 0) sBgColor = "#ffffcc";
        else if (i <  4) sBgColor = "#ccffcc";
        else if (i <  8) sBgColor = "#cce5ff";
        else if (i == ltTbl.Count - 1) sBgColor = "#ffcccc";

        var k = i + 1;

        string sPlaceLast = "-";
        string sColor = "black";
        for (var iLast = 0; iLast < ltTblLast.Count; iLast++) {
          if (ltTblLast[iLast].iId == tbpl.iId) {
            if (i != iLast) {
              sPlaceLast = (iLast + 1).ToString();
              if (i > iLast) sColor = "red";
              else           sColor = "green";
            }
            break;
          }
        }

        string sEmblem = getClubEmblem(tbpl.iId, "width: 12px");

        sBox += "<tr " + sStyle + "\">";
        sBox += "<td class=\"first\" bgcolor=\"" + sBgColor + "\" align=\"center\"><b>" + k + "</b></td>";
        sBox += "<td style=\"color: " + sColor + "\" align=\"center\"> " + sPlaceLast + "</td>";
        sBox += "<td>" + sEmblem + "</td>";
        sBox += "<td><a href=\"/Member/ClubDetails?iClub=" + tbpl.iId.ToString() + "\" target=\"\">" + tbpl.sName + "</a></td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"right\">" + iGames.ToString() + "</td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"right\">" + tbpl.iGUV[0].ToString() + "</td>";
        sBox += "<td align=\"right\">" + tbpl.iGUV[1].ToString() + "</td>";
        sBox += "<td align=\"right\">" + tbpl.iGUV[2].ToString() + "</td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"center\">" + tbpl.iGoals.ToString() + ":" + tbpl.iGoalsOpp.ToString() + "</td>";
        sBox += "<td align=\"right\">" + iDiff.ToString() + "</td>";
        sBox += "<td>&nbsp;</td>";
        sBox += "<td align=\"right\">" + tbpl.iPoints.ToString() + "</td>";
        sBox += "</tr>";
      }

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setLeagueTeams(ushort iSaison, int iLand, byte iDivision, int iMatchday)
    {
      if (iMatchday < 1) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clbUser = ckClub();

      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, iLand, iDivision);

      CornerkickManager.Cup.Matchday md = new CornerkickManager.Cup.Matchday();
      md.ltGameData = new List<CornerkickGame.Game.Data>();

      string sBox = "";
      foreach (CornerkickGame.Game.Data gd in league.ltMatchdays[iMatchday - 1].ltGameData) {
        string sClubNameH = "-";
        string sClubNameA = "-";
        int iIdH = gd.team[0].iTeamId;
        int iIdA = gd.team[1].iTeamId;
        if (gd.team[0].iTeamId >= 0) sClubNameH = MvcApplication.ckcore.ltClubs[iIdH].sName;
        if (gd.team[1].iTeamId >= 0) sClubNameA = MvcApplication.ckcore.ltClubs[iIdA].sName;

        string sStyle = "";
        if (clbUser != null && (iIdH == clbUser.iId || iIdA == clbUser.iId)) sStyle = " style=\"font-weight:bold\"";

        sBox += "<tr" + sStyle + ">";
        sBox += "<td>" + gd.dt.ToString("d", getCi()) + "&nbsp;" + gd.dt.ToString("t", getCi()) + "</td>";

        sBox += "<td style=\"white-space: nowrap\" align=\"right\"><a href=\"/Member/ClubDetails?iClub=" + iIdH.ToString() + "\" target=\"\">" + sClubNameH + "</a></td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"center\">&nbsp;-&nbsp;</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"left\"><a href=\"/Member/ClubDetails?iClub=" + iIdA.ToString() + "\" target=\"\">" + sClubNameA + "</a></td>";

        if (gd.team[0].iGoals + gd.team[1].iGoals >= 0) {
          sBox += "<td style=\"white-space: nowrap\" align=\"center\">" + MvcApplication.ckcore.ui.getResultString(gd) + "</td>";
        } else {
          sBox += "<td style=\"white-space: nowrap\" align=\"center\">-</td>";
        }
        sBox += "</tr>";
      }

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public JsonResult LeagueCupGetScorer(byte iGameType, int iLand, int iDivision)
    {
      CornerkickManager.Club clbUser = ckClub();

      string sBox = "";

      var ltScorer = MvcApplication.ckcore.ui.getScorer(iGameType, iLand, iDivision);

      int iNo = 0;
      foreach (CornerkickManager.UI.Scorer sc in ltScorer) {
        iNo++;
        string sStyle2 = "";
        if (clbUser != null && MvcApplication.ckcore.ltPlayer[sc.iId].iClubId == clbUser.iId) sStyle2 = "font-weight:bold";

        sBox += "<tr style=" + sStyle2 + " class=\"\">";
        sBox += "<td class=\"first\">" + iNo.ToString() + "</td>";
        sBox += "<td style=\"white-space: nowrap\">" + sc.sName + "</td>";
        sBox += "<td style=\"white-space: nowrap\">" + sc.sTeam + "</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"right\">" + sc.iGoals  .ToString() + "</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"right\">" + sc.iAssists.ToString() + "</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"right\">" + (sc.iGoals + sc.iAssists).ToString() + "</td>";
        sBox += "</tr>";
      }

      return Json(sBox, JsonRequestBehavior.AllowGet);
    }

    public ContentResult GetLeaguePlaceHistory()
    {
      CornerkickManager.Club club = ckClub();

      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, club.iLand, club.iDivision);

      List<Models.DataPointGeneral> dataPoints = new List<Models.DataPointGeneral>();

      for (int iSpT = 1; iSpT < league.getMatchdaysTotal(); iSpT++) {
        int iPlace = MvcApplication.ckcore.tl.getLeaguePlace(league, club, iSpT);
        if (iPlace > 0) dataPoints.Add(new Models.DataPointGeneral(iSpT, iPlace));
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    [Authorize]
    public ActionResult Cup(Models.CupModel cupModel)
    {
      cupModel.ltErg = new List<List<CornerkickGame.Game.Data>>();

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(cupModel);

      cupModel.iClubId = clb.iId;

      cupModel.iLand = clb.iLand;

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(2, cupModel.iLand);

      if (cup == null) return View(cupModel);
      if (cup.ltMatchdays == null) return View(cupModel);
      if (cup.ltMatchdays.Count < 1) return View(cupModel);

      // Add lands to dropdown list
      foreach (int iLand in MvcApplication.iNations) {
        cupModel.ddlLand.Add(new SelectListItem { Text = MvcApplication.ckcore.sLand[iLand], Value = iLand.ToString() });
      }

      return View(cupModel);
    }

    public JsonResult CupGetDdlMatchdays(ushort iSaison, int iLand)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(null, JsonRequestBehavior.AllowGet);

      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(2, iLand);
      if (cup == null) return Json(null, JsonRequestBehavior.AllowGet);

      string[] ltMd = new string[cup.getMatchdaysTotal()];
      // Spieltage zu Dropdown Menü hinzufügen
      for (int iMd = 0; iMd < ltMd.Length; iMd++) {
        ltMd[iMd] = (iMd + 1).ToString();

        int nRound = cup.getKoRound(cup.ltClubs[0].Count);
        ltMd[iMd] += ";" + MvcApplication.ckcore.sCupRound[nRound - iMd - 1];
      }

      /*
      // Spieltage zu Dropdown Menü hinzufügen
      if (cup.ltMatchdays[0].ltGameData != null) {
        int nRound = cup.getKoRound(cup.ltClubs[0].Count);
        while (cupModel.ltDdlSpTg.Count < nRound) {
          int iMd = cupModel.ltDdlSpTg.Count + 1;
          cupModel.ltDdlSpTg.Add(new SelectListItem { Text = MvcApplication.ckcore.sCupRound[nRound - iMd], Value = iMd.ToString() });
        }
      }
      */

      return Json(ltMd, JsonRequestBehavior.AllowGet);
    }

    public JsonResult CupGetMatchday()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(1, JsonRequestBehavior.AllowGet);

      // Get current matchday
      int iMd = MvcApplication.ckcore.tl.getMatchday(clb, MvcApplication.ckcore.dtDatum, 2);

      // Increment matchday match is if today or tomorrow
      CornerkickGame.Game.Data gdNext = MvcApplication.ckcore.tl.getNextGame(clb, MvcApplication.ckcore.dtDatum, iGameType: 2);
      if (gdNext != null && (gdNext.dt.Date - MvcApplication.ckcore.dtDatum.Date).Days < 2) iMd++;

      // Limit to 1
      iMd = Math.Max(iMd, 1);

      return Json(iMd, JsonRequestBehavior.AllowGet);
    }

    public JsonResult setCup(Models.CupModel cupModel, ushort iSaison, int iLand, int iMatchday)
    {
      CornerkickManager.Cup cup = MvcApplication.ckcore.tl.getCup(2, iLand);
      if (cup == null)             return Json("", JsonRequestBehavior.AllowGet);
      if (cup.ltMatchdays == null) return Json("", JsonRequestBehavior.AllowGet);

      CornerkickManager.Club clbUser = ckClub();

      string sBox = "";
      CornerkickManager.Cup.Matchday md = cup.ltMatchdays[iMatchday - 1];
      if (md.ltGameData == null || md.ltGameData.Count == 0) {
        List<CornerkickManager.Club> ltClubs = MvcApplication.ckcore.tl.getCupParticipants(cup, iMatchday - 1);

        foreach (CornerkickManager.Club clb in ltClubs) {
          var sStyle = "";
          if (clb == clbUser) sStyle = "font-weight:bold";

          sBox += "<tr style=" + sStyle + ">";
          sBox += "<td></td>";

          sBox += "<td align=\"right\"><a href=\"/Member/ClubDetails?iClub=" + clb.iId.ToString() + "\" target=\"_blank\">" + clb.sName + "</a></td></td>";
          sBox += "<td align=\"center\"></td>";
          sBox += "<td align=\"left\"></td>)</td>";
          sBox += "<td align=\"center\"></td>";

          sBox += "</tr>";
        }

        return Json(sBox, JsonRequestBehavior.AllowGet);
      }

      foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
        string sClubNameH = "freilos";
        string sClubNameA = "freilos";
        int iIdH = gd.team[0].iTeamId;
        int iIdA = gd.team[1].iTeamId;
        if (gd.team[0].iTeamId >= 0) sClubNameH = MvcApplication.ckcore.ltClubs[iIdH].sName;
        if (gd.team[1].iTeamId >= 0) sClubNameA = MvcApplication.ckcore.ltClubs[iIdA].sName;

        var sStyle = "";
        if (clbUser != null && (iIdH == clbUser.iId || iIdA == clbUser.iId)) sStyle = "font-weight:bold";

        sBox += "<tr style=" + sStyle + ">";
        sBox += "<td>" + gd.dt.ToString("d", getCi()) + "&nbsp;" + gd.dt.ToString("t", getCi()) + "</td>";

        sBox += "<td style=\"white-space: nowrap\" align=\"right\"><a href=\"/Member/ClubDetails?iClub=" + iIdH.ToString() + "\" target=\"\">" + sClubNameH + "</a></td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"center\">&nbsp;-&nbsp;</td>";
        sBox += "<td style=\"white-space: nowrap\" align=\"left\"><a href=\"/Member/ClubDetails?iClub=" + iIdA.ToString() + "\" target=\"\">" + sClubNameA + "</a></td>";

        if (gd.team[0].iGoals >= 0 && gd.team[1].iGoals >= 0) {
          sBox += "<td style=\"white-space: nowrap\" align=\"center\">" + MvcApplication.ckcore.ui.getResultString(gd) + "</td>";
        } else {
          sBox += "<td style=\"white-space: nowrap\" align=\"center\">-</td>";
        }
        sBox += "</tr>";
      }

      return Json(sBox, JsonRequestBehavior.AllowGet);
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

      cal.iClubId = ckClub().iId;

      cal.ltTestgames = new List<Models.Testgame>();
      foreach (CornerkickManager.Cup cup in MvcApplication.ckcore.ltCups) {
        if (cup.iId == -5) {
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
              if (gd.team[1].iTeamId == ckClub().iId) {
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

    public List<Models.DiaryEvent> getCalendarEvents()
    {
      List<Models.DiaryEvent> ltEvents = new List<Models.DiaryEvent>();

      CornerkickManager.Club club = ckClub();

      //DateTime dt = new DateTime(MvcApplication.ckcore.dtDatum.Year, MvcApplication.ckcore.dtDatum.Month, MvcApplication.ckcore.dtDatum.Day);
      DateTime dt = MvcApplication.ckcore.dtSeasonStart.Date;
      while (dt.CompareTo(MvcApplication.ckcore.dtSeasonEnd) < 0) {
        // New Year
        if (dt.Day == 1 && dt.Month == 1) {
          ltEvents.Add(new Models.DiaryEvent {
            iID = ltEvents.Count,
            sTitle = "Neujahr",
            sDescription = "Neujahr",
            sStartDate = dt.ToString("yyyy-MM-ddTHH:mm:ss"),
            sColor = "rgb(200, 200, 200)",
            bEditable = false,
            bAllDay = true
          });
        }

        // Trainingscamp
        bool bCampTravelDay = false;
        CornerkickManager.csTrainingCamp.Booking booking = MvcApplication.ckcore.tcp.getCurrentCamp(club, dt, true);
        if (booking.camp.iId > 0) {
          if (dt.Date.Equals(booking.dtArrival.Date)) {
            ltEvents.Add(new Models.DiaryEvent {
              iID = ltEvents.Count,
              sTitle = "Abreise Trainingslager",
              sDescription = booking.camp.sName,
              sStartDate = booking.dtArrival.ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = booking.dtArrival.AddHours(8).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 100, 0)",
              bEditable = false,
              bAllDay = false
            });

            bCampTravelDay = true;
          } else if (dt.Date.Equals(booking.dtDeparture.Date)) {
            ltEvents.Add(new Models.DiaryEvent {
              iID = ltEvents.Count,
              sTitle = "Rückreise Trainingslager",
              sDescription = booking.camp.sName,
              sStartDate = booking.dtDeparture.AddHours(-8).ToString("yyyy-MM-ddTHH:mm:ss"),
              sEndDate = booking.dtDeparture.ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 100, 0)",
              bEditable = false,
              bAllDay = false
            });

            bCampTravelDay = true;
          } else {
            ltEvents.Add(new Models.DiaryEvent {
              iID = ltEvents.Count,
              sTitle = "Trainingslager",
              sDescription = booking.camp.sName,
              sStartDate = dt.ToString("yyyy-MM-ddTHH:mm:ss"),
              //sEndDate = dt.AddHours(23).ToString("yyyy-MM-ddTHH:mm:ss"),
              sColor = "rgb(255, 150, 0)",
              bEditable = false,
              bAllDay = true
            });
          }
        }

        // Training
        if (club.training.iTraining[(int)dt.DayOfWeek] > 0 && !bCampTravelDay) {
          DateTime dtTmp = new DateTime(dt.Year, dt.Month, dt.Day, 10, 00, 00);

          ltEvents.Add(new Models.DiaryEvent {
            iID = ltEvents.Count,
            sTitle = " Training (" + MvcApplication.ckcore.sTraining[club.training.iTraining[(int)dt.DayOfWeek]] + ")",
            sDescription = MvcApplication.ckcore.sTraining[club.training.iTraining[(int)dt.DayOfWeek]],
            sStartDate = dtTmp.ToString("yyyy-MM-ddTHH:mm:ss"),
            sEndDate = dtTmp.AddMinutes(120).ToString("yyyy-MM-ddTHH:mm:ss"),
            sColor = "rgb(255, 200, 0)",
            bEditable = false,
            bAllDay = false
          });
        }

        // Cup
        foreach (CornerkickManager.Cup cup in MvcApplication.ckcore.ltCups) {
          int iSpTg = 0;
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            if (dt.Date.Equals(md.dt.Date)) {
              if (md.ltGameData == null) continue;

              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                int iIdH = gd.team[0].iTeamId;
                int iIdA = gd.team[1].iTeamId;
                if (iIdH == club.iId ||
                    iIdA == club.iId) {
                  string sH = "";
                  string sA = "";
                  if (iIdH >= 0 && iIdH < MvcApplication.ckcore.ltClubs.Count) sH = MvcApplication.ckcore.ltClubs[iIdH].sName;
                  if (iIdA >= 0 && iIdA < MvcApplication.ckcore.ltClubs.Count) sA = MvcApplication.ckcore.ltClubs[iIdA].sName;

                  string sRes = MvcApplication.ckcore.ui.getResultString(gd);

                  string sTitle = " " + cup.sName;
                  string sColor = "rgb(200, 0, 200)";
                  if (cup.iId == 1) {
                    sTitle = " Liga, " + (iSpTg + 1).ToString().PadLeft(2) + ". Spieltag";
                    sColor = "rgb(0, 175, 100)";
                  } else if (cup.iId == 2) {
                    string sPokalrunde = "";
                    sPokalrunde = MvcApplication.ckcore.sCupRound[cup.getKoRound(md.ltGameData.Count)];
                    sTitle += ", " + sPokalrunde;

                    sColor = "rgb(100, 100, 255)";
                  } else if (cup.iId == -5) {
                    sColor = "rgb(255, 200, 255)";
                  }
                  sTitle += ": " + sH + " vs. " + sA;
                  if (!string.IsNullOrEmpty(sRes)) sTitle += ", " + sRes;

                  ltEvents.Add(new Models.DiaryEvent {
                    iID = ltEvents.Count,
                    sTitle = sTitle,
                    sStartDate = md.dt.ToString("yyyy-MM-ddTHH:mm:ss"),
                    sEndDate = md.dt.AddMinutes(105).ToString("yyyy-MM-ddTHH:mm:ss"),
                    sColor = sColor,
                    bEditable = false,
                    bAllDay = false
                  });

                  if (cup.iId == 2 && cup.iId2 == club.iLand && md.ltGameData.Count > 1) {
                    ltEvents.Add(new Models.DiaryEvent {
                      iID = ltEvents.Count,
                      sTitle = " " + cup.sName + ", Auslosung " + MvcApplication.ckcore.sCupRound[cup.getKoRound(md.ltGameData.Count) - 1],
                      sStartDate = new DateTime(md.dt.Year, md.dt.Month, md.dt.AddDays(1).Day, 12,  0, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                      sEndDate   = new DateTime(md.dt.Year, md.dt.Month, md.dt.AddDays(1).Day, 12, 30, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                      sColor     = "rgb(100, 200, 255)",
                      bEditable = false,
                      bAllDay = false
                    });
                  }

                  break;
                }
              }
            } else if (cup.iId == 2 && cup.iId2 == club.iLand && iSpTg == 0 && dt.Date.Equals(MvcApplication.ckcore.dtSeasonStart.AddDays(6).Date)) {
              ltEvents.Add(new Models.DiaryEvent {
                iID = ltEvents.Count,
                sTitle = " " + cup.sName + ", Auslosung 1. Runde",
                sStartDate = new DateTime(dt.Year, dt.Month, dt.Day, 12,  0, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                sEndDate   = new DateTime(dt.Year, dt.Month, dt.Day, 12, 30, 0).ToString("yyyy-MM-ddTHH:mm:ss"),
                sColor = "rgb(100, 200, 255)",
                bEditable = false,
                bAllDay = false
              });
            }
            iSpTg++;
          }
        }

        dt = dt.AddDays(1);
      }

      return ltEvents;
    }

    public ActionResult PostCalendarData()
    {
      //var result = getCalendarEvents();

      var ApptListForDate = getCalendarEvents();
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

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(sReturn, JsonRequestBehavior.AllowGet);

      int iTeamIdUser = club.iId;
      //int iTeamId = -1;
      //int.TryParse(sTeamId, out iTeamId);
      if (iTeamId >= 0 && iTeamId < MvcApplication.ckcore.ltClubs.Count && iTeamId != iTeamIdUser) {
        CornerkickManager.Cup.Matchday md = new CornerkickManager.Cup.Matchday();
        DateTime.TryParse(start, out md.dt);

        md.ltGameData = new List<CornerkickGame.Game.Data>();
        CornerkickGame.Game.Data gd = new CornerkickGame.Game.Data();
        gd.team[0].iTeamId = iTeamIdUser;
        gd.team[1].iTeamId = iTeamId;

        md.ltGameData.Add(gd);

        // Inform user
        CornerkickManager.Club clubRequest = MvcApplication.ckcore.ltClubs[iTeamId];
        if (clubRequest.iUser < 0) {
          createTestgame(md);

          sReturn = "Testspiel am " + md.dt.ToString("d", getCi()) + " " + md.dt.ToString("t", getCi()) + " gegen " + clubRequest.sName + " vereinbart";

          club.nextGame = MvcApplication.ckcore.tl.getNextGame(club, MvcApplication.ckcore.dtDatum);
        } else {
          CornerkickManager.Cup cup = new CornerkickManager.Cup();
          cup.iId = -5;
          cup.sName = "Anfrage Testspiel";
          cup.ltMatchdays.Add(md);
          MvcApplication.ckcore.ltCups.Add(cup);

          sReturn = "Anfrage für Testspiel am " + md.dt.ToString("d", getCi()) + " " + md.dt.ToString("t", getCi()) + " gegen " + MvcApplication.ckcore.ltClubs[iTeamId].sName + " gesendet";

          MvcApplication.ckcore.Info(clubRequest.iUser, "Sie haben eine neue Anfrage für ein Testspiel erhalten.", 2, iTeamId);
        }
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

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      for (int iC = 0; iC < MvcApplication.ckcore.ltCups.Count; iC++) {
        CornerkickManager.Cup cup = MvcApplication.ckcore.ltCups[iC];

        if (cup.iId == -5) {
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            if (md.dt.Equals(dt)) {
              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                if (gd.team[1].iTeamId == club.iId) {
                  createTestgame(md);

                  MvcApplication.ckcore.ltCups.RemoveAt(iC);

                  club.nextGame = MvcApplication.ckcore.tl.getNextGame(club, MvcApplication.ckcore.dtDatum);

                  CornerkickManager.Club clubH = MvcApplication.ckcore.ltClubs[gd.team[0].iTeamId];
                  clubH.nextGame = MvcApplication.ckcore.tl.getNextGame(clubH, MvcApplication.ckcore.dtDatum);

                  if (clubH.iUser >= 0) {
                    MvcApplication.ckcore.Info(clubH.iUser, "Ihre Anfrage an " + club.sName + " für ein Testspiel am " + dt.ToString("d", getCi()) + " um " + dt.ToString("t", getCi()) + " wurde akzeptiert!", 2, gd.team[0].iTeamId);
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

    private void createTestgame(CornerkickManager.Cup.Matchday md)
    {
      CornerkickManager.Cup cupTestGames = MvcApplication.ckcore.tl.getCup(5);

      if (cupTestGames == null) {
        cupTestGames = new CornerkickManager.Cup();
        cupTestGames.iId = 5;
        cupTestGames.settings.fAttraction = 0.5f;
        cupTestGames.settings.iNeutral = 1;
        cupTestGames.sName = "Testspiel";
        MvcApplication.ckcore.ltCups.Add(cupTestGames);
      }

      cupTestGames.ltMatchdays.Add(md);
    }

    [HttpPost]
    public JsonResult DeclineTestgame(string sDateTestgame)
    {
      DateTime dt = new DateTime();
      if (!DateTime.TryParse(sDateTestgame, out dt)) {
        return Json("", JsonRequestBehavior.AllowGet);
      }

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      for (int iC = 0; iC < MvcApplication.ckcore.ltCups.Count; iC++) {
        CornerkickManager.Cup cup = MvcApplication.ckcore.ltCups[iC];

        if (cup.iId == -5) {
          foreach (CornerkickManager.Cup.Matchday md in cup.ltMatchdays) {
            if (md.dt.Equals(dt)) {
              foreach (CornerkickGame.Game.Data gd in md.ltGameData) {
                if (gd.team[1].iTeamId == clb.iId) {
                  cup.iId = 5;
                  MvcApplication.ckcore.ltCups.RemoveAt(iC);

                  CornerkickManager.Club clubH = MvcApplication.ckcore.ltClubs[gd.team[0].iTeamId];
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

      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      foreach (CornerkickGame.Game.Data data in MvcApplication.ckcore.tl.getNextGames(club, MvcApplication.ckcore.dtDatum, false)) {
        if (data.iGameType == 5) continue;

        if (dtStart.Date.Date.CompareTo(data.dt) == 0) return Json(  "Abreise am Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);
        if (dtEnd  .Date.Date.CompareTo(data.dt) == 0) return Json("Rückreise am Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);

        if (dtStart.Date.Date.CompareTo(data.dt) < 0 /* Start date before game date */ &&
            dtEnd  .Date.Date.CompareTo(data.dt) > 0 /* End date after game date */) {
          return Json("Trainingslager über Spieltag nicht erlaubt!", JsonRequestBehavior.AllowGet);
        }
      }

      CornerkickManager.csTrainingCamp.Camp camp = MvcApplication.ckcore.tcp.ltCamps[iIx];

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
        //ltDays.Add((iDay + 1).ToString().PadLeft(2) + " (Rückreise: " + dtStart.Date.AddDays(iDay).ToString("d", getCi()) + ")");
      }

      return Json(ltDays.ToArray(), JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public int getDaysUntilNextGame(DateTime dtStart, int iIgnoreGameType = 0)
    {
      int nDays = 999;

      CornerkickManager.Club club = ckClub();
      if (club == null) return nDays;

      foreach (CornerkickGame.Game.Data data in MvcApplication.ckcore.tl.getNextGames(club, MvcApplication.ckcore.dtDatum, false)) {
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
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(financeModel);

      financeModel.ltAccount = clb.ltAccount;

      financeModel.iEintritt1 = clb.iAdmissionPrice[0];
      financeModel.iEintritt2 = clb.iAdmissionPrice[1];
      financeModel.iEintritt3 = clb.iAdmissionPrice[2];

      financeModel.iPriceSeason1 = clb.iAdmissionPriceSeasonal[0];
      financeModel.iPriceSeason2 = clb.iAdmissionPriceSeasonal[1];
      financeModel.iPriceSeason3 = clb.iAdmissionPriceSeasonal[2];

      financeModel.iSeasonalTickets = new int[clb.iSpectatorsSeasonal.Length];
      financeModel.iSeasonalTickets = clb.iSpectatorsSeasonal;

      financeModel.bEditable = MvcApplication.ckcore.dtDatum.Date.Equals(MvcApplication.ckcore.dtSeasonStart.Date);
      financeModel.budgetPlan = ckUser().budget;
      financeModel.budgetReal = MvcApplication.ckcore.ui.getActualBudget(clb);

      if (financeModel.budgetPlan.iPaySalary == 0) {
        CornerkickManager.Finance.Budget bg = financeModel.budgetPlan;
        bg.iPaySalary = MvcApplication.ckcore.tl.getPlayerSalary(clb);
        financeModel.budgetPlan = bg;
      }

      if (financeModel.budgetPlan.iPayStaff == 0) {
        CornerkickManager.Finance.Budget bg = financeModel.budgetPlan;
        bg.iPayStaff = MvcApplication.ckcore.tl.getStuffSalary(clb);
        financeModel.budgetPlan = bg;
      }

      // Secret Balance
      financeModel.fBalanceSecretFracAdmissionPrice = clb.fBalanceSecretFracAdmissionPrice * 100f;
      financeModel.sBalanceSecret                   = clb.iBalanceSecret.ToString("N0", getCi()) + " €";

      return View(financeModel);
    }

    public ContentResult FinanceGetDevelopmentData(Models.FinanceModel financeModel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Content("", "application/json");

      List<Models.DataPointGeneral> dataPoints = new List<Models.DataPointGeneral>();

      foreach (CornerkickManager.Finance.Account kto in clb.ltAccount) {
        if (kto.dt.CompareTo(MvcApplication.ckcore.dtDatum.AddDays(-30)) > 0) {
          long iDate = convertDateTimeToTimestamp(kto.dt);
          dataPoints.Add(new Models.DataPointGeneral(iDate, kto.iBalance));
        }
      }

      long iDateCurrent = convertDateTimeToTimestamp(MvcApplication.ckcore.dtDatum);
      dataPoints.Add(new Models.DataPointGeneral(iDateCurrent, clb.iBalance));

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    public ContentResult FinanceGetSpec(Models.FinanceModel financeModel)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Content("", "application/json");

      List<Models.DataPointGeneral>[] dataPoints = new List<Models.DataPointGeneral>[2];
      dataPoints[0] = new List<Models.DataPointGeneral>(); // Actual spec.
      dataPoints[1] = new List<Models.DataPointGeneral>(); // Stadium size

      List<CornerkickGame.Game.Data> ltGdPast = MvcApplication.ckcore.tl.getNextGames(clb, MvcApplication.ckcore.dtDatum, false, true);
      int i = 0;
      foreach (CornerkickGame.Game.Data gd in ltGdPast) {
        if (gd.team[0].iTeamId == clb.iId) {
          i--;

          string sCupName = "";
          if      (gd.iGameType == 1) sCupName = " - Liga";
          else if (gd.iGameType == 2) sCupName = " - Pokal";
          else if (gd.iGameType == 5) sCupName = " - Testspiel";

          string sInfo0 = gd.dt.ToString("d", getCi()) + sCupName + "</br>" +
                          (gd.iSpectators[0] + gd.iSpectators[1] + gd.iSpectators[2]).ToString("N0", getCi()) + " (" + gd.iSpectators[0].ToString("N0", getCi()) + "/" + gd.iSpectators[1].ToString("N0", getCi()) + "/" + gd.iSpectators[2].ToString("N0", getCi()) + ")" + "</br>" +
                          gd.team[1].sTeam;
          dataPoints[0].Add(new Models.DataPointGeneral(i, gd.iSpectators[0] + gd.iSpectators[1] + gd.iSpectators[2], sInfo0));

          string sInfo1 = gd.dt.ToString("d", getCi()) + "</br>" +
                          gd.stadium.getSeats().ToString("N0", getCi()) + " (" + gd.stadium.getSeats(0).ToString("N0", getCi()) + "/" + gd.stadium.getSeats(1).ToString("N0", getCi()) + "/" + gd.stadium.getSeats(2).ToString("N0", getCi()) + ")" + "</br>" +
                          gd.team[1].sTeam;
          dataPoints[1].Add(new Models.DataPointGeneral(i, gd.stadium.getSeats(), sInfo1));
        }
      }

      JsonSerializerSettings _jsonSetting = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };

      return Content(JsonConvert.SerializeObject(dataPoints, _jsonSetting), "application/json");
    }

    [HttpPost]
    public JsonResult FinanceSetAdmissionPrice(int[] iEintritt)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.iAdmissionPrice[0] = iEintritt[0];
      clb.iAdmissionPrice[1] = iEintritt[1];
      clb.iAdmissionPrice[2] = iEintritt[2];

      int iInSpec = 0;
      CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, clb.iLand, clb.iDivision);
      if (league != null) {
        iInSpec = (clb.stadium.getSeats(0) * clb.iAdmissionPrice[0]) +
                  (clb.stadium.getSeats(1) * clb.iAdmissionPrice[1]) +
                  (clb.stadium.getSeats(2) * clb.iAdmissionPrice[2]);
        int nGamesHome = league.getMatchdaysTotal();
        iInSpec *= nGamesHome;
      }

      return Json(iInSpec.ToString("N0", getCi()) + "€", JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult FinanceSetAdmissionPriceSeasonal(int[] iPrice)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.iAdmissionPriceSeasonal[0] = iPrice[0];
      clb.iAdmissionPriceSeasonal[1] = iPrice[1];
      clb.iAdmissionPriceSeasonal[2] = iPrice[2];

      return Json(null, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult SetBudget(int[] iBudgetIn, int[] iBudgetPay)
    {
      CornerkickManager.User user = ckUser();

      if (iBudgetIn  != null) {
        user.budget.iInSpec         = iBudgetIn[0];
        user.budget.iInBonusCup     = iBudgetIn[1];
        user.budget.iInBonusSponsor = iBudgetIn[2];
        user.budget.iInTransfer     = iBudgetIn[3];
      }

      if (iBudgetPay != null) {
        user.budget.iPaySalary   = iBudgetPay[0];
        user.budget.iPayStaff    = iBudgetPay[1];
        user.budget.iPayTransfer = iBudgetPay[2];
        user.budget.iPayStadium  = iBudgetPay[3];
        user.budget.iPayInterest = iBudgetPay[4];
      }

      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      long iInPlanTotal  = MvcApplication.ckcore.fz.getBudgetInTotal (user.budget);
      long iPayPlanTotal = MvcApplication.ckcore.fz.getBudgetPayTotal(user.budget);

      long iPlannedResult = iInPlanTotal - iPayPlanTotal;
      string sPlannedResult = "0";
      if (iPlannedResult != 0) {
        sPlannedResult = iPlannedResult.ToString("N0", getCi());
      }

      CornerkickManager.Finance.Budget bgReal = MvcApplication.ckcore.ui.getActualBudget(clb);
      long iInCurrTotal  = MvcApplication.ckcore.fz.getBudgetInTotal (bgReal);
      long iPayCurrTotal = MvcApplication.ckcore.fz.getBudgetPayTotal(bgReal);
      string sCurrentResult = "0";
      if (iInCurrTotal - iPayCurrTotal != 0) {
        sCurrentResult = (iInCurrTotal - iPayCurrTotal).ToString("N0", getCi());
      }

      string[] sTotal = new string[4] {
        iInPlanTotal .ToString("N0", getCi()),
        iPayPlanTotal.ToString("N0", getCi()),
        sPlannedResult,
        sCurrentResult
      };

      return Json(sTotal, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult FinanceSetBalanceSecret(float fBalanceSecretFracAdmissionPrice)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      clb.fBalanceSecretFracAdmissionPrice = fBalanceSecretFracAdmissionPrice / 100f;

      return Json(true, JsonRequestBehavior.AllowGet);
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
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return View(sponsorModel);

      sponsorModel.sponsorMain     = clb.sponsorMain;
      sponsorModel.ltSponsorBoards = clb.ltSponsorBoards;
      sponsorModel.ltSponsorOffers = clb.ltSponsorOffers;

      sponsorModel.ltSponsorBoardIds = new List<int>();
      foreach (CornerkickManager.Finance.Sponsor spon in sponsorModel.ltSponsorBoards) {
        for (int iB = 0; iB < spon.nBoards; iB++) sponsorModel.ltSponsorBoardIds.Add(spon.iId);
      }

      return View(sponsorModel);
    }

    [HttpPost]
    public JsonResult SponsorSet(int iSponsorIndex)
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      string sCash = MvcApplication.ckcore.ui.setSponsor(ref clb, clb.ltSponsorOffers[iSponsorIndex]).ToString("N0", getCi());

      return Json(sCash, JsonRequestBehavior.AllowGet);
    }

    [HttpPost]
    public JsonResult getLtSponsorBoardIds()
    {
      CornerkickManager.Club clb = ckClub();
      if (clb == null) return Json(false, JsonRequestBehavior.AllowGet);

      List<int> ltSponsorBoardIds = new List<int>();
      foreach (CornerkickManager.Finance.Sponsor spon in clb.ltSponsorBoards) {
        for (int iB = 0; iB < spon.nBoards; iB++) ltSponsorBoardIds.Add(spon.iId);
      }

      return Json(ltSponsorBoardIds, JsonRequestBehavior.AllowGet);
    }

    public ActionResult getTableSponsorBoard()
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return Json(false, JsonRequestBehavior.AllowGet);

      //The table or entity I'm querying
      List<Models.DatatableEntrySponsorBoard> query = new List<Models.DatatableEntrySponsorBoard>();

      int iSpOffer = 0;
      bool bOffer = false;
      foreach (List<CornerkickManager.Finance.Sponsor> ltSponsor in new List<CornerkickManager.Finance.Sponsor>[] { club.ltSponsorBoards, club.ltSponsorOffers }) {
        foreach (CornerkickManager.Finance.Sponsor spon in ltSponsor) {
          if (bOffer) iSpOffer++;

          if (spon.bMain) continue;

          Models.DatatableEntrySponsorBoard deSponsorBoard = new Models.DatatableEntrySponsorBoard();
          deSponsorBoard.bOffer = bOffer;
          deSponsorBoard.iId = spon.iId;
          if (bOffer) deSponsorBoard.iIndex = iSpOffer - 1;
          deSponsorBoard.sName = MvcApplication.ckcore.fz.ltSponsoren[spon.iId].sName;
          deSponsorBoard.sMoneyVicHome = spon.iMoneyVicHome.ToString("N0", getCi());
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
    /// Transfer
    /// </summary>
    /// <param name="Transfer"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    public ActionResult Statistic(Models.StatisticModel statisticModel)
    {
      statisticModel.ddlNations = new List<SelectListItem>();
      statisticModel.ddlNations.Add(new SelectListItem {
                                      Text = "Weltauswahl",
                                      Value = "-1",
                                      Selected = true
                                    }
      );
      for (int iN = 0; iN < MvcApplication.iNations.Length; iN++) {
        statisticModel.ddlNations.Add(new SelectListItem {
                                        Text  = MvcApplication.ckcore.sLand[MvcApplication.iNations[iN]],
                                        Value = MvcApplication.iNations[iN].ToString()
                                      }
        );
      }

      statisticModel.ltsFormations = new List<SelectListItem>();
      for (int i = 0; i < MvcApplication.ckcore.ltFormationen.Count; i++) {
        statisticModel.ltsFormations.Add(new SelectListItem {
                                           Text = (i + 1).ToString() + " - " + MvcApplication.ckcore.ltFormationen[i].sName,
                                           Value = i.ToString(),
                                           Selected = i == 9
                                         }
        );
      }

      return View(statisticModel);
    }

    public JsonResult StatisticGetBest11(int iNat, int iF, bool bJouth = false)
    {
      Models.TeamModels.TeamData tD = new Models.TeamModels.TeamData();
      tD.ltPlayer         = new List<CornerkickGame.Player>();
      tD.ltPlayerAveSkill = new List<string>();
      tD.ltPlayerPos      = new List<byte>  ();
      tD.ltPlayerTeamname = new List<string>();
      tD.ltPlayerAge      = new List<string>();
      tD.ltPlayerNat      = new List<string>();

      for (byte iP = 0; iP < 11; iP++) {
        float fStrength = 0f;
        tD.ltPlayer        .Add(null);
        tD.ltPlayerPos     .Add(0);
        tD.ltPlayerAveSkill.Add(null);
        tD.ltPlayerTeamname.Add("vereinslos");
        tD.ltPlayerAge     .Add(null);
        tD.ltPlayerNat     .Add(null);

        byte iPosExact = MvcApplication.ckcore.game.tl.getPosRole(MvcApplication.ckcore.ltFormationen[iF].ptPos[iP]);
        byte iPos = MvcApplication.ckcore.game.tl.getBasisPos(iPosExact);

        foreach (CornerkickGame.Player pl in MvcApplication.ckcore.ltPlayer) {
          if (bJouth && pl.getAge(MvcApplication.ckcore.dtDatum) > 18f) continue;
          if (iNat >= 0 && pl.iNat1 != iNat) continue;

          // Check if same player already in same role
          bool bSame = false;
          foreach (CornerkickGame.Player plSame in tD.ltPlayer) {
            if (plSame != null && plSame.iId == pl.iId && MvcApplication.ckcore.game.tl.getPosRole(plSame.ptPosDefault) == iPosExact) {
              bSame = true;
              break;
            }
          }
          if (bSame) continue;

          float fStrengthTmp = MvcApplication.ckcore.game.tl.getAveSkill(pl, iPos);
          if (fStrengthTmp > fStrength) {
            tD.ltPlayer[iP] = pl.Clone(true);

            tD.ltPlayer        [iP].ptPosDefault = MvcApplication.ckcore.ltFormationen[iF].ptPos[iP];
            tD.ltPlayer        [iP].iNr = (byte)(iP + 1);
            tD.ltPlayerAveSkill[iP] = fStrengthTmp.ToString("0.0");

            fStrength = fStrengthTmp;
          }
        }

        tD.ltPlayerPos[iP] = iPos;
        if (tD.ltPlayer[iP] != null) {
          if (tD.ltPlayer[iP].iClubId >= 0) tD.ltPlayerTeamname[iP] = MvcApplication.ckcore.ltClubs[tD.ltPlayer[iP].iClubId].sName;
          tD.ltPlayerAge[iP] = tD.ltPlayer[iP].getAge(MvcApplication.ckcore.dtDatum).ToString("0.0");
          tD.ltPlayerNat[iP] = MvcApplication.ckcore.sLandShort[tD.ltPlayer[iP].iNat1];
        }
      }

      float[] fTeamAve11 = MvcApplication.ckcore.tl.getTeamAve(tD.ltPlayer, 11);
      tD.fTeamAveStrength = fTeamAve11[3];
      tD.fTeamAveAge      = fTeamAve11[4];

      return Json(tD, JsonRequestBehavior.AllowGet);
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
      mdUser.ltDdlUser = new List<SelectListItem>();

      mdUser.ltDdlUser.Add(new SelectListItem { Text = "-", Value = "-1" });

      // Positionen zu Dropdown Menü hinzufügen
      for (int iU = 0; iU < MvcApplication.ckcore.ltUser.Count; iU++) {
        CornerkickManager.User userTmp = MvcApplication.ckcore.ltUser[iU];
        if (userTmp.id == User.Identity.GetUserId()) continue;

        string sName = userTmp.sFirstname + " " + userTmp.sSurname;

        sName += " (" + MvcApplication.ckcore.ltClubs[userTmp.iTeam].sName + ")";

        mdUser.ltDdlUser.Add(new SelectListItem { Text = sName, Value = iU.ToString() });
      }

      mdUser.ltUserMail = new List<CornerkickManager.Main.News>();

      CornerkickManager.User user = ckUser();

      mdUser.ltUserMail = new List<CornerkickManager.Main.News>();

      if (user.ltNews != null) {
        foreach (CornerkickManager.Main.News news in user.ltNews) {
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

      MvcApplication.ckcore.Info(iTo, sText, 99, 0, 0, System.DateTime.Now, AccountController.getiUser(ckUser()));

      return Json("Nachricht an " + MvcApplication.ckcore.ltUser[iTo].sFirstname + " " + MvcApplication.ckcore.ltUser[iTo].sSurname + " gesendet!", JsonRequestBehavior.AllowGet);
    }

    public void MailMarkRead(int iIndexMail)
    {
      if (iIndexMail < 0) return;

      CornerkickManager.User user = ckUser();

      if (user.ltNews != null) {
        int iMail = 0;
        for (int iN = 0; iN < user.ltNews.Count; iN++) {
          CornerkickManager.Main.News news = user.ltNews[iN];
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

    public int countNewMails()
    {
      CornerkickManager.User user = ckUser();
      if (user == null) return 0;

      int iMails = 0;
      if (user.ltNews != null) {
        foreach (CornerkickManager.Main.News news in user.ltNews) {
          if (news.iType == 99 && !news.bRead) iMails++;
        }
      }

      return iMails;
    }

    public int compareDates(DateTime dt)
    {
      CornerkickManager.Club club = ckClub();
      if (club == null) return 0;

      List<CornerkickGame.Game.Data> ltGd = MvcApplication.ckcore.tl.getNextGames(club, MvcApplication.ckcore.dtDatum, false);

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