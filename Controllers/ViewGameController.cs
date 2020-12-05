#define _AI2
//#define _PLOT_MORAL

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.Web.Script.Serialization;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;

namespace CornerkickWebMvc.Controllers
{
  public class ViewGameController : Controller
  {
    private static List<Models.ViewGameModel.gameData2> ltGd;

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

      // National team
      if (usr.nation != null) {
        for (byte iN = 0; iN < CornerkickWebMvc.Controllers.MemberController.bShowClub.Length; iN++) {
          if (usr.nation.iLand == MvcApplication.iNations[iN]) {
            if (!CornerkickWebMvc.Controllers.MemberController.bShowClub[iN]) return usr.nation;
            break;
          }
        }
      }

      return usr.club;
    }

    //////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// ViewGame
    /// </summary>
    /// <param name="ViewGame"></param>
    /// <returns></returns>
    //////////////////////////////////////////////////////////////////////////
    private static Models.ViewGameModel.gameData2 getViewGameData(string sUserId)
    {
      for (int iGd = 0; iGd < ltGd.Count; iGd++) {
        if (ltGd[iGd].sUserId.Equals(sUserId)) return ltGd[iGd];
      }

      return null;
    }
    private static void setViewGameDataList(Models.ViewGameModel.gameData2 gd)
    {
      for (int iGd = 0; iGd < ltGd.Count; iGd++) {
        if (ltGd[iGd].sUserId == null) continue;

        if (ltGd[iGd].sUserId.Equals(gd.sUserId)) {
          ltGd[iGd] = gd;
          return;
        };
      }

      ltGd.Add(gd);
    }

    [Authorize]
    public ActionResult ViewGame(Models.ViewGameModel view, string sGameId = null)
    {
      view.iStateGlobal = 0;

      // Get user
      CornerkickManager.User user = ckUser();
      if (user == null) return View(view);

      // Get user club
      CornerkickManager.Club clubUser = ckClub();

      view.bAdmin = AccountController.checkUserIsAdmin(User.Identity.GetUserName());

      CornerkickGame.Game game = user.game;

      // Initialize own game flag
      view.bOwnLiveGame = game != null && !game.data.bFinished;

      if (view.bAdmin && game == null) {
        game = MvcApplication.ckcore.game.tl.getDefaultGame();
        game.data.iGameSpeed = 300;
      }

      view.bSound = true;
      if (user.lti.Count > 1) view.bSound = user.lti[1] > 0;

      // Create game selector menu
      view.ddlGames = new List<SelectListItem>();

      if (!string.IsNullOrEmpty(sGameId)) {
        // First, look for live games in user list
        bool bUserGame = false;
        foreach (CornerkickManager.User usrGame in MvcApplication.ckcore.ltUser) {
          if (usrGame.id.Equals(sGameId)) {
            game = usrGame.game;
            bUserGame = true;
            view.bOwnLiveGame = usrGame.id.Equals(user.id); // Set own live-game flag
            break;
          }
        }

        // If not live game, check for stored games to load
        if (!bUserGame) game = MvcApplication.ckcore.io.loadGame(Path.Combine(MvcApplication.getHomeDir(), "save", "games", sGameId + ".ckgx"));
      } else if (game == null || game.data.bFinished) {
        view.bOwnLiveGame = false; // Set own live-game flag

        List<FileInfo> fiGames = getFileInfoGames(clubUser);

        // Insert past games into dropdownmenu
        foreach (FileInfo ckg in fiGames) {
          DateTime dtGame;
          int iTeamIdH;
          int iTeamIdA;
          int iCupId;
          string sFilenameInfo = getFilenameInfo(ckg, out dtGame, out iTeamIdH, out iTeamIdA, out iCupId);
          if (string.IsNullOrEmpty(sFilenameInfo)) continue;

          view.ddlGames.Insert(0,
            new SelectListItem {
              Text = sFilenameInfo,
              Value = ckg.Name
            }
          );
        }

        // Admin livegame
        if (AccountController.checkUserIsAdmin(User)) {
          view.ddlGames.Insert(0, new SelectListItem { Text = "Livespiel", Value = "" });
        }

        if (view.ddlGames.Count > 0) view.ddlGames[0].Selected = true;

        if (game == null && fiGames.Count > 0) {
          string sFilenameGame = Path.Combine(MvcApplication.getHomeDir(), "save", "games", fiGames[fiGames.Count - 1].Name);
          try {
            game = MvcApplication.ckcore.io.loadGame(sFilenameGame);
          } catch (Exception e) {
            MvcApplication.ckcore.tl.writeLog("Unable to load game: '" + sFilenameGame + "'" + Environment.NewLine + e.Message + e.StackTrace, CornerkickManager.Main.sErrorFile);
          }
        }

        // Insert next games
        /*
        List<CornerkickGame.Game.Data> ltGdNextGames = MvcApplication.ckcore.tl.getNextGames(clubUser, MvcApplication.ckcore.dtDatum);
        for (byte j = 0; j < ltGdNextGames.Count; j++) {
          CornerkickGame.Game.Data gd = ltGdNextGames[j];
          view.ddlGames.Insert(0, new SelectListItem {
            Text = gd.dt.ToString("d", Controllers.MemberController.getCiStatic(User)) + " " + gd.dt.ToString("t", Controllers.MemberController.getCiStatic(User)) + " *: " + gd.team[0].sTeam + " - " + gd.team[1].sTeam,
            Value = (-j - 1).ToString()
          });
        }
        */
      }

      view.ddlShoots = new List<SelectListItem>(view.ddlHeatmap);
      view.ddlDuels  = new List<SelectListItem>(view.ddlHeatmap);
      view.ddlPasses = new List<SelectListItem>(view.ddlHeatmap);

      fHeatmapMax = 0f;

      iniGameData(view, game);

      return View(view);
    }

    private List<FileInfo> getFileInfoGames(CornerkickManager.Club clubUser)
    {
      List<FileInfo> fiGames = new List<FileInfo>();

      DirectoryInfo d = new DirectoryInfo(Path.Combine(MvcApplication.getHomeDir(), "save", "games"));

      if (d.Exists) {
        FileInfo[] ltCkgFiles = d.GetFiles("*.ckgx");

        int iG = 0;
        foreach (FileInfo ckg in ltCkgFiles) {
          string[] sFilenameData = Path.GetFileNameWithoutExtension(ckg.Name).Split('x');
          if (sFilenameData.Length < 3) continue;

          DateTime dtGame = new DateTime();
          int iTeamIdH = -1;
          int iTeamIdA = -1;
          int iCupId = -1;
          if (string.IsNullOrEmpty(getFilenameInfo(ckg, out dtGame, out iTeamIdH, out iTeamIdA, out iCupId))) continue;

          if (iTeamIdH == clubUser.iId || iTeamIdA == clubUser.iId || AccountController.checkUserIsAdmin(User)) fiGames.Add(ckg);
        }
      }

      return fiGames;
    }

    private string getFilenameInfo(FileInfo fiGame, out DateTime dtGame, out int iTeamIdH, out int iTeamIdA, out int iCupId)
    {
      return getFilenameInfo(fiGame, out dtGame, out iTeamIdH, out iTeamIdA, out iCupId, Controllers.MemberController.getCiStatic(User));
    }
    public static string getFilenameInfo(FileInfo fiGame, out DateTime dtGame, out int iTeamIdH, out int iTeamIdA, out int iCupId, CultureInfo ciUser = null)
    {
      string sFilenameInfo = "";

      dtGame = new DateTime();
      iCupId = -1;
      iTeamIdH = -1;
      iTeamIdA = -1;

      string[] sFilenameData = Path.GetFileNameWithoutExtension(fiGame.Name).Split('x');
      if (sFilenameData.Length < 3) return null;

      // Date/Time
      if (!DateTime.TryParseExact(sFilenameData[0], "yyyyMMdd_HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtGame)) return null;

      // Cup ID
      string[] sFilenameDataCupId = sFilenameData[1].Split('_');
      if (!int.TryParse(sFilenameDataCupId[0], out iCupId)) return null;

      // Team names
      string[] sFilenameDataTeamIds = sFilenameData[2].Split('_');
      if (!int.TryParse(sFilenameDataTeamIds[0], out iTeamIdH)) return null;
      if (!int.TryParse(sFilenameDataTeamIds[1], out iTeamIdA)) return null;

      return dtGame.ToString("d", ciUser) + " " + dtGame.ToString("t", ciUser) + ": " + MvcApplication.ckcore.ltClubs[iTeamIdH].sName + " - " + MvcApplication.ckcore.ltClubs[iTeamIdA].sName;
    }

    public JsonResult loadGame(Models.ViewGameModel view, string sFilename)
    {
      // Get user
      CornerkickManager.User usr = ckUser();
      if (usr == null) return Json(false, JsonRequestBehavior.AllowGet);
      Models.ViewGameModel.gameData2 gd2 = getViewGameData(usr.id);

      string sFilenameGame = Path.Combine(MvcApplication.getHomeDir(), "save", "games", sFilename);

      try {
        gd2.game = MvcApplication.ckcore.io.loadGame(sFilenameGame);
        iniGameData(view, gd2.game);
      } catch {
        MvcApplication.ckcore.tl.writeLog("Unable to load game: '" + sFilenameGame + "'", CornerkickManager.Main.sErrorFile);
      }

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public void iniGameData(Models.ViewGameModel view, CornerkickGame.Game game)
    {
      CornerkickManager.User usr = ckUser();
      CornerkickManager.Club clbUser = ckClub();

      if (game == null) {
        view.gD = new Models.ViewGameModel.gameData();
        view.gD.iGoalsH = -1;
        view.gD.iGoalsA = -1;

        return;
      }

      if (ltGd == null) ltGd = new List<Models.ViewGameModel.gameData2>();
      Models.ViewGameModel.gameData2 gd2 = new Models.ViewGameModel.gameData2();
      Models.ViewGameModel.gameData gD = new Models.ViewGameModel.gameData();
      gd2.sUserId = usr.id;
      gd2.viewGd = gD;
      gd2.game = game;

      gD.iTeamId = clbUser.iId;
      gD.bOwnLiveGame = view.bOwnLiveGame;

      gD.sColorJerseyH = new string[] { "white", "blue", "blue", "blue", "white", "white" };
      gD.sColorJerseyA = new string[] { "white", "red",  "red",  "red",  "white", "white" };

      string sEmblemDir = Path.Combine(MvcApplication.getHomeDir(), "Content", "Uploads", "emblems");
      string sEmblemDirHtml = "/Content/Uploads/emblems/";
      string sEmblemH = game.data.team[0].iTeamId.ToString() + ".png";
      string sEmblemA = game.data.team[1].iTeamId.ToString() + ".png";

      gD.sStadium = game.data.stadium.sName;

      if (game.data.team[0].iTeamId >= 0 && game.data.team[0].iTeamId < MvcApplication.ckcore.ltClubs.Count) {
        CornerkickManager.Club clubH = MvcApplication.ckcore.ltClubs[game.data.team[0].iTeamId];

        // Set team name
        gD.sTeamH = clubH.sName;

        if (clubH.bNation) {
          sEmblemDir = Path.Combine(MvcApplication.getHomeDir(), "Content", "Icons", "flags");
          sEmblemDirHtml = "/Content/Icons/flags/";
          int iNationH = clubH.iLand;
          if (iNationH >= 0 && iNationH < CornerkickManager.Main.sLandShort.Length) sEmblemH = CornerkickManager.Main.sLandShort[iNationH] + ".png";
        }

        for (int iC = 0; iC < clubH.cl1.Length; iC++) gD.sColorJerseyH[iC                   ] = "rgb(" + clubH.cl1[iC].R.ToString() + "," + clubH.cl1[iC].G.ToString() + "," + clubH.cl1[iC].B.ToString() + ")";
        for (int iC = 0; iC < clubH.cl2.Length; iC++) gD.sColorJerseyH[iC + clubH.cl1.Length] = "rgb(" + clubH.cl2[iC].R.ToString() + "," + clubH.cl2[iC].G.ToString() + "," + clubH.cl2[iC].B.ToString() + ")";

        if (string.IsNullOrEmpty(gD.sStadium)) gD.sStadium = clubH.stadium.sName;
      }

      if (game.data.team[1].iTeamId >= 0 && game.data.team[1].iTeamId < MvcApplication.ckcore.ltClubs.Count) {
        CornerkickManager.Club clubA = MvcApplication.ckcore.ltClubs[game.data.team[1].iTeamId];

        // Set team name
        gD.sTeamA = clubA.sName;

        if (clubA.bNation) {
          sEmblemDir = Path.Combine(MvcApplication.getHomeDir(), "Content", "Icons", "flags");
          sEmblemDirHtml = "/Content/Icons/flags/";
          int iNationA = clubA.iLand;
          if (iNationA >= 0 && iNationA < CornerkickManager.Main.sLandShort.Length) sEmblemA = CornerkickManager.Main.sLandShort[iNationA] + ".png";
        }

        //for (byte iC = 0; iC < clubA.cl.Length; iC++) view.sColorJerseyA[iC] = "rgb(" + clubA.cl[iC].R.ToString() + "," + clubA.cl[iC].G.ToString() + "," + clubA.cl[iC].B.ToString() + ")";
        for (int iC = 0; iC < clubA.cl1.Length; iC++) gD.sColorJerseyA[iC                   ] = "rgb(" + clubA.cl1[iC].R.ToString() + "," + clubA.cl1[iC].G.ToString() + "," + clubA.cl1[iC].B.ToString() + ")";
        for (int iC = 0; iC < clubA.cl2.Length; iC++) gD.sColorJerseyA[iC + clubA.cl1.Length] = "rgb(" + clubA.cl2[iC].R.ToString() + "," + clubA.cl2[iC].G.ToString() + "," + clubA.cl2[iC].B.ToString() + ")";
      }

      /*
      if (!System.IO.File.Exists(Path.Combine(sEmblemDir, sEmblemH))) sEmblemH = "0.png";
      if (!System.IO.File.Exists(Path.Combine(sEmblemDir, sEmblemA))) sEmblemA = "0.png";
      */

      gD.sEmblemH = sEmblemDirHtml + sEmblemH;
      gD.sEmblemA = sEmblemDirHtml + sEmblemA;

      // Stadium seats
      gD.sStadium += " (" + (game.data.iSpectators[0] + game.data.iSpectators[1] + game.data.iSpectators[2]).ToString("N0", CornerkickWebMvc.Controllers.MemberController.getCiStatic(User)) + " / " + game.data.stadium.getSeats().ToString("N0", CornerkickWebMvc.Controllers.MemberController.getCiStatic(User)) + ")";

      // Heatmap
      string[] sHA = new string[2] { "H", "A" };
      // Add player to heatmap
      for (byte iHA = 0; iHA < 2; iHA++) {
        for (byte iPl = 0; iPl < game.data.nPlStart; iPl++) {
          if (game.player[iHA][iPl] == null) continue;
          view.ddlHeatmap.Add(new SelectListItem { Text = "(" + sHA[iHA] + ") " + game.player[iHA][iPl].sName + " - " + game.player[iHA][iPl].iNr, Value = (2 + (iHA * game.data.nPlStart) + iPl).ToString() });
        }
      }

      view.iGameSpeed = game.data.iGameSpeed;

      view.game = game;

      //gD = getAllGameData(view);

      setGameData(ref gD, gd2.game, gd2.sUserId);

      view.gD = gD;
    }

    public JsonResult ViewGameLocations(int iState = -1/*, int iSleep = 0*/, bool bAverage = false)
    {
      Models.ViewGameModel.ltLoc = new List<float[]>();
      Models.ViewGameModel.gameLoc gLoc = new Models.ViewGameModel.gameLoc();

      CornerkickManager.User usr = ckUser();
      Models.ViewGameModel.gameData2 gd2 = getViewGameData(usr.id);

      if (gd2.game == null) {
        Response.StatusCode = 1;
        return Json(Models.ViewGameModel.ltLoc, JsonRequestBehavior.AllowGet);
      }

      bool bAdmin = AccountController.checkUserIsAdmin(User);

      // Set interval
      if (MvcApplication.ckcore.ltUser.Count > 0) {
        if (MvcApplication.ckcore.ltUser[0].nextGame != null) gLoc.iInterval = MvcApplication.ckcore.ltUser[0].nextGame.iGameSpeed;
      }

      CornerkickGame.Game.State state = gd2.game.newState();
      if (gd2.game.data.ltState.Count > 0) state = gd2.game.data.ltState[gd2.game.data.ltState.Count - 1];

      if (iState > 0 && iState < gd2.game.data.ltState.Count) state = gd2.game.data.ltState[iState];
      //if (fTime >= 0f) state = MvcApplication.ckcore.game.tl.getState(user.game.data, fTime);
      //CornerkickGame.Game.State state = MvcApplication.ckcore.game.tl.getState(user.game.data, fTime);

      /*
      float fFinished = 0f;
      if (fTime >= 0f) fFinished = state.i;
      else if (user.game.data.bFinished) fFinished = 1f;
      */
      gLoc.bFinished = gd2.game.data.bFinished;
      gLoc.bHalftime = gd2.game.iStandard == 9;

      // Ball
      Models.ViewGameModel.gameBall gBall = new Models.ViewGameModel.gameBall();
      CornerkickGame.Game.Ball ball;
      if (iState < 0) ball = gd2.game.ball;
      else            ball = state.ball;

      CornerkickGame.Game.PointBall pbBall = new CornerkickGame.Game.PointBall();
      pbBall = ball.Pos;

      if (bAverage) {
        System.Drawing.Point ptAve = CornerkickManager.UI.getAveragePos(gd2.game, -1, -1, iState);
        pbBall.X = ptAve.X;
        pbBall.Y = ptAve.Y;
        pbBall.Z = 0f;
      }
      gBall.Pos = pbBall;
      //Models.ViewGameModel.ltLoc.Add(new float[5] { fBallPosX, fBallPosY, fBallPosZ, fFinished, 0f });

      // Player
      gLoc.ltPlayer = new List<Models.ViewGameModel.gamePlayer>();

      for (byte iHA = 0; iHA < 2; iHA++) {
        for (int iP = 0; iP < MvcApplication.ckcore.game.data.nPlStart; iP++) {
          Models.ViewGameModel.gamePlayer gPlayer = new Models.ViewGameModel.gamePlayer();

          CornerkickGame.Player pl;
          if (iState < 0) pl = gd2.game.player[iHA][iP];
          else            pl = state.player[iHA][iP];

          if (pl == null) continue;
          if (string.IsNullOrEmpty(pl.sName)) continue;

          if      (pl.bYellowCard)                              gPlayer.iCard = 1;
          else if (pl.iSuspension[gd2.game.iSuspensionIx] > 0) gPlayer.iCard = 2;

          gPlayer.ptPos = pl.ptPos;
          if (bAverage) {
            System.Drawing.Point ptAve = CornerkickManager.UI.getAveragePos(gd2.game, iHA, iP, iState);
            gPlayer.ptPos = ptAve;
          }

          gPlayer.ptPosTarget = pl.ptPosTarget;
          gPlayer.iLookAt = pl.iLookAt;
          gPlayer.iNo = pl.iNr;

          gLoc.ltPlayer.Add(gPlayer);
          //Models.ViewGameModel.ltLoc.Add(new float[5] { iPosX, iPosY, pl.iLookAt, pl.iNr, fCard });

          if (bAdmin && iState >= 0) {
            gd2.game.player[iHA][iP].iHA     = pl.iHA;
            gd2.game.player[iHA][iP].iIndex  = pl.iIndex;
            gd2.game.player[iHA][iP].ptPos.X = pl.ptPos.X;
            gd2.game.player[iHA][iP].ptPos.Y = pl.ptPos.Y;
            gd2.game.player[iHA][iP].iLookAt = pl.iLookAt;
            gd2.game.player[iHA][iP].fSteps = 6f;
          }
        }
      }

      // If admin: set finished to true to avoid recall of drawGame()
      if (bAdmin) gLoc.bFinished = true;

      if (bAdmin && iState >= 0) {
        gd2.game.ball = ball;
        gd2.game.tsMinute = state.tsMinute;

        // Find player at ball
        gd2.game.ball.plAtBall = null;
        if (gd2.game.ball.iPassStep == 0) {
          for (byte iHA = 0; iHA < 2; iHA++) {
            for (int iP = 0; iP < MvcApplication.ckcore.game.data.nPlStart; iP++) {
              if (iP < gd2.game.player[iHA].Length) {
                CornerkickGame.Player plAtBall = gd2.game.player[iHA][iP];
                if (plAtBall == null) continue;

                if (ball.ptPos == plAtBall.ptPos) {
                  gd2.game.ball.plAtBall = plAtBall;
                  break;
                }
              }
            }

            if (gd2.game.ball.plAtBall != null) break;
          }
        }
      }

      // Ball target
      if (ball.iPassStep > 0 && ball.nPassSteps > 0) {
        gBall.bLowPass = ball.bLowPass;
      }
      gBall.ptPosTarget = ball.ptPos;
      gBall.ptPosLast   = ball.ptPosLast;
      //gBall.fPassFraction = (ball.nPassSteps - ball.iPassStep) / (float)ball.nPassSteps;
      gBall.iPassStep  = (byte)ball.iPassStep;
      gBall.nPassSteps = (byte)ball.nPassSteps;
      //Models.ViewGameModel.ltLoc.Add(new float[5] { ball.ptPos.X, ball.ptPos.Y, fPass, fFinished, 0f });

      gLoc.gBall = gBall;

      // Comments
      gLoc.ltComments = new List<string[]>();
      for (int iC = 0; iC < state.ltComment.Count; iC++) {
        CornerkickGame.Game.Comment k = state.ltComment[iC];

        if (string.IsNullOrEmpty(k.sText)) continue;

        string[] sCommentarNew = new string[3];
        sCommentarNew[0] = MvcApplication.ckcore.ui.getMinuteString(k.tsMinute, true) + ": ";
        sCommentarNew[1] = k.sText;
        if (state.shoot.plShoot != null && state.shoot.iResult == 1) {
          sCommentarNew[2] = "bold";
        }
        gLoc.ltComments.Add(sCommentarNew);
      }

      // Set flag for updating statistic
      gLoc.bUpdateStatistic = state.bNewRound;

      ///////////////////////////////////////////////////////////////////
      // Events
      ///////////////////////////////////////////////////////////////////
      if (iState != -4) {
        // Shoots
        if (state.shoot.plShoot != null) {
          if (state.shoot.iResult == 1) gLoc.iEvent = (byte)(1 + state.shoot.plShoot.iHA);
          else if (state.shoot.iResult == 5 || state.shoot.iResult == 6) gLoc.iEvent = 3;
          else gLoc.iEvent = 4;
        }

        // Foul
        if (state.duel.plDef != null && state.duel.iResult > 1) {
          gLoc.iEvent = 5;
        }

        // Whistle
        if (state.iStandard == 9) {
          gLoc.iEvent = 6;
        }
      }

      return Json(gLoc, JsonRequestBehavior.AllowGet);
    }

    public JsonResult ViewGameGetDataStatisticObject(Models.ViewGameModel view, int iState = -1, int iHeatmap = -1, int iAllShoots = -1, int iAllDuels = -1, int iAllPasses = -1)
    {
      CornerkickManager.User usr = ckUser();
      //CornerkickManager.Club club = ckClub();

      Models.ViewGameModel.gameData2 gd2 = getViewGameData(usr.id); // view.gD;
      Models.ViewGameModel.gameData gD = gd2.viewGd; // view.gD;
      sbyte iStandard = 0;

      if (gd2.game == null) {
        setGameData(ref gD, gd2.game, gd2.sUserId, out iStandard, iState, iHeatmap, iAllShoots, iAllDuels, iAllPasses);
        return Json(gD, JsonRequestBehavior.AllowGet);
      }

      CornerkickGame.Game.Data gameData = gd2.game.data;

      // Clear chart arrays
      gD.ltF = new List<Models.DataPointGeneral>[gameData.nPlStart];
      for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<Models.DataPointGeneral>();

#if _PLOT_MORAL
      gD.ltM = new List<Models.DataPointGeneral>[user.game.data.nPlStart];
      for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();
#endif

      if (gD.nStates == 0) {
        gD = getAllGameData(gameData);
      } else if (iState >= 0) {
        gD = getAllGameData(gameData, iState);
      } else if (iState <  0) {
        if (iState == -3) {
          gD.iLastStatePerformed = 0;
          gD.sTimelineIcons = "";
        }

        /*
        // Return null if not new round
        if (gameData.ltState.Count > 0 && iState != -3) {
          if (!gameData.ltState[gameData.ltState.Count - 1].bNewRound) return Json(null, JsonRequestBehavior.AllowGet);
        }
        */

        // Add game data to struct
        for (int i = gD.iLastStatePerformed; i < gameData.ltState.Count; i++) addGameData(ref gD, gameData, i);
      }

      iStandard = gd2.game.iStandard;
      setGameData(ref gD, gd2.game, gd2.sUserId, out iStandard, iState, iHeatmap, iAllShoots, iAllDuels, iAllPasses);

      return Json(gD, JsonRequestBehavior.AllowGet);
    }

    // Adds comment, shoots/cards and chart data of current state to gD
    private void addGameData(ref Models.ViewGameModel.gameData gD, CornerkickGame.Game.Data gameData, int iState = -1, bool bAddFMList = true)
    {
      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (gameData?.ltState == null) return;

      if (gameData.ltState.Count == 0) return;

      CornerkickGame.Game.State state = gameData.ltState[gameData.ltState.Count - 1];
      if (iState >= 0 && iState < gameData.ltState.Count) state = gameData.ltState[iState];

      float fLeft = ((state.tsMinute.Hours * 60f) + state.tsMinute.Minutes + (state.tsMinute.Seconds / 60f)) / 0.9f;
      if (gameData.bFinished) fLeft = (100.0f * state.i) / gameData.ltState.Count;

      for (byte jHA = 0; jHA < 2; jHA++) {
        int iIconTop = 2;
        if (jHA == 1) iIconTop = 44;

        string sTeam = gameData.team[0].sTeam;
        if (jHA > 0) sTeam = gameData.team[1].sTeam;

        string sOnClick = "";
        string sCursor = "";
        if (gameData.bFinished) {
          sOnClick = "onclick =\"javascript: drawGame(" + state.i.ToString() + ")\" ";
          sCursor = "; cursor: pointer";
        }

        // Shoots
        CornerkickGame.Game.Shoot shoot = state.shoot;
        if (shoot.plShoot != null && shoot.iHA == jHA) {
          string sShootDesc = MvcApplication.ckcore.ui.getMinuteString(shoot.tsMinute, false) + " Min.: " +
                              shoot.iGoalsH.ToString() + ":" + shoot.iGoalsA.ToString();
          sShootDesc += " - " + shoot.plShoot.sName;
          if (shoot.iType == 5) {
            sShootDesc += ", FE";
          } else {
            float fDist = CornerkickGame.Tool.getDistanceToGoal(shoot.plShoot, MvcApplication.ckcore.game.ptPitch.X, MvcApplication.ckcore.game.fConvertDist2Meter)[0];

            sShootDesc += ", Entf.:" + fDist.ToString("0.0").PadLeft(5) + "m";
          }

          if (shoot.plAssist != null) sShootDesc += " (" + shoot.plAssist.sName + ")";

          string sImg = "yellow";
          if (shoot.iResult == 1) {
            sImg = "white";

            if (string.IsNullOrEmpty(gD.sStatGoals)) gD.sStatGoals = "<br/><u>Tore:</u>";
            gD.sStatGoals += "<br/>" + sShootDesc;
          } else if (shoot.iResult == 0 || shoot.iResult == 7) {
            sImg = "cyan";
          }

          if (shoot.iType == 5) sImg += "_penalty";

          gD.sTimelineIcons += "<img " + sOnClick + "src=\"/Content/Icons/ball_" + sImg + ".png\" alt=\"Torschuss\" style=\"position: absolute; top: " + iIconTop.ToString() + "px; width: 12px; left: " + (fLeft - 0.5).ToString(nfi) + "%" + sCursor + "\" title=\"" + sShootDesc + "\"/>";

          // Count shoots
          gD.iShoots[jHA]++;
          if (shoot.iResult > 0 && shoot.iResult < 7) gD.iShootsOnGoal[jHA]++;
        } // shoot

        // Passes
        CornerkickGame.Game.Pass pass = state.pass;
        if (pass.plPasser != null && pass.plPasser.iHA == jHA) {
          if      (pass.plReceiver     == null)              gD.iPassesBad [jHA]++;
          else if (pass.plReceiver.iHA != pass.plPasser.iHA) gD.iPassesBad [jHA]++;
          else if (pass.plReceiver     != pass.plPasser)     gD.iPassesGood[jHA]++;
        }

        // Duels
        CornerkickGame.Game.Duel duel = state.duel;
        if (duel.plDef != null && duel.plDef.iHA == jHA) {
          if (duel.iResult > 2) {
            string sCardDesc = MvcApplication.ckcore.ui.getMinuteString(duel.tsMinute, false) + " Min.: " +
                                sTeam +
                                " - " +
                                duel.plDef.sName;

            string sImg = "y";
            if      (duel.iResult == 4) sImg = "yr";
            else if (duel.iResult == 5) sImg = "r";

            gD.sTimelineIcons += "<img " + sOnClick + "src=\"/Content/Icons/" + sImg + "Card.png\" alt=\"Karte\" style=\"position: absolute; top: " + iIconTop.ToString() + "px; width: 12px; left: " + (fLeft - 0.5).ToString(nfi) + "%" + sCursor + "\" title=\"" + sCardDesc + "\"/>";

            if (string.IsNullOrEmpty(gD.sStatCards)) gD.sStatCards = "<br/><u>Karten:</u>";
            gD.sStatCards += "<br/><img " + sOnClick + "style=\"position: relative" + sCursor + "\" src ='/Content/Icons/" + sImg + "Card.png'/>" + sCardDesc;
          }

          // Count duels (fouls)
          gD.iDuels[jHA]++;
          if (duel.iResult > 1) gD.iFouls[jHA]++;
        }
      } // iHA

      // Chart
      if (gD.bOwnLiveGame) {
        byte iHA = 2;
        if      (gD.iTeamId == gameData.team[0].iTeamId) iHA = 0;
        else if (gD.iTeamId == gameData.team[1].iTeamId) iHA = 1;
        if (bAddFMList && iHA < 2) {
          if (gD.ltF == null || iState == -1) {
            gD.ltF = new List<Models.DataPointGeneral>[gameData.nPlStart];
            for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<Models.DataPointGeneral>();
          }

#if _PLOT_MORAL
          if (gD.ltM == null || iState == -1) {
            gD.ltM = new List<Models.DataPointGeneral>[user.game.data.nPlStart];
            for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();
          }
#endif

          for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) {
            gD.ltF[iPl].Add(new Models.DataPointGeneral(state.i, state.player[iHA][iPl].fFresh));
          }
#if _PLOT_MORAL
          for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) {
            gD.ltM[iPl].Add(new Models.DataPointGeneral(state.i, state.player[jHA][iPl].fMoral));
          }
#endif
        }
      }

      // Set counter of performed state
      gD.iLastStatePerformed = iState;
    }

    private void setGameData(ref Models.ViewGameModel.gameData gd, CornerkickGame.Game game, string sUserId, int iState = -1, int iHeatmap = -1, int iAllShoots = -1, int iAllDuels = -1, int iAllPasses = -1)
    {
      sbyte iStandard;
      setGameData(ref gd, game, sUserId, out iStandard, iState: iState, iHeatmap: iHeatmap, iAllShoots: iAllShoots, iAllDuels: iAllDuels, iAllPasses: iAllPasses);
    }
    private void setGameData(ref Models.ViewGameModel.gameData gd, CornerkickGame.Game game, string sUserId, out sbyte iStandard, int iState = -1, int iHeatmap = -1, int iAllShoots = -1, int iAllDuels = -1, int iAllPasses = -1)
    {
      iStandard = 0;

      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (game == null) {
        gd.iGoalsH = -1;
        gd.iGoalsA = -1;
        return;
      }

      if (game.data?.ltState == null) return;
      if (game.data.ltState.Count == 0) return;

      CornerkickGame.Game.State state = game.data.ltState[game.data.ltState.Count - 1];
      if (iState >= 0 && iState < game.data.ltState.Count) state = game.data.ltState[iState];

      gd.nStates = game.data.ltState.Count;

      gd.tsMinute = state.tsMinute;

      iStandard = state.iStandard;

      gd.ltDrawLineShoot = new List<Models.ViewGameModel.drawLine>();
      gd.ltDrawLinePass  = new List<Models.ViewGameModel.drawLine>();
      gd.sCard = "";
      gd.sStatSubs = "";

      // Draw shoot on pitch
      if ((iState >= 0 && iState < game.data.ltState.Count) || iAllShoots >= 0) {
        if (iAllShoots >= 0) {
          byte iHA   =  0;
          int  iPlIx = -1;
          getStatisticHAPlayerIx(iAllShoots, game.data.nPlStart, out iHA, out iPlIx);

          for (int iSt = 0; iSt < game.data.ltState.Count; iSt++) {
            CornerkickGame.Game.State stateTmp = game.data.ltState[iSt];
            gd.ltDrawLineShoot.AddRange(getShootLine(stateTmp, game, iHA, iPlIx));

            if (iState >= 0 && iSt >= iState) break; // If review --> stop at selected state
          }
        } else {
          gd.ltDrawLineShoot = getShootLine(state, game);
        }
      }

      // Draw pass on pitch
      if ((iState >= 0 && iState < game.data.ltState.Count) || iAllPasses >= 0) {
        if (iAllPasses >= 0) {
          byte iHA = 0;
          int iPlIx = -1;
          getStatisticHAPlayerIx(iAllPasses, game.data.nPlStart, out iHA, out iPlIx);

          for (int iSt = 0; iSt < game.data.ltState.Count; iSt++) {
            CornerkickGame.Game.State stateTmp = game.data.ltState[iSt];
            gd.ltDrawLinePass.Add(getPassLine(stateTmp, game, iHA, iPlIx));

            if (iState >= 0 && iSt >= iState) break; // If review --> stop at selected state
          }
        } else {
          gd.ltDrawLinePass.Clear();
          gd.ltDrawLinePass.Add(getPassLine(state, game));
        }
      }

      // Draw duel on pitch
      if ((iState >= 0 && iState < game.data.ltState.Count) || iAllDuels >= 0) {
        // Show duel on pitch
        if (iAllDuels >= 0) {
          byte iHA   =  0;
          int  iPlIx = -1;
          getStatisticHAPlayerIx(iAllDuels, game.data.nPlStart, out iHA, out iPlIx);

          for (int iSt = 0; iSt < game.data.ltState.Count; iSt++) {
            CornerkickGame.Game.State stateTmp = game.data.ltState[iSt];
            gd.sCard += getDuelIcon(stateTmp, game, iHA, iPlIx);

            if (iState >= 0 && iSt >= iState) break; // If review --> stop at selected state
          }
        } else {
          gd.sCard = getDuelIcon(state, game);
        }
      }

      int  [] nPossession = new int  [2];
      int  [] nCornerkick = new int  [2];
      int  [] nOffsite    = new int  [2];
      float[] fPasses     = new float[2];

      for (byte iHA = 0; iHA < 2; iHA++) {
        int iIconTop = 2;
        if (iHA == 1) iIconTop = 44;

        string sTeam = game.data.team[0].sTeam;
        if (iHA > 0) sTeam = game.data.team[1].sTeam;

        //int iStTmp = 0;
        // loop over states
        //foreach (CornerkickGame.Game.State state in gameData.ltState) {

        if (iHA == 0) {
          gd.iGoalsH = state.iGoalsH;
          nPossession[0] = state.iPossessionH;
          nCornerkick[0] = state.iCornerkickH;
          nOffsite   [0] = state.iOffsiteH;
        } else {
          gd.iGoalsA = state.iGoalsA;
          nPossession[1] = state.iPossessionA;
          nCornerkick[1] = state.iCornerkickA;
          nOffsite   [1] = state.iOffsiteA;
        }
        if (gd.iPassesGood[iHA] + gd.iPassesBad[iHA] > 0) fPasses[iHA] = (100 * gd.iPassesGood[iHA]) / (float)(gd.iPassesGood[iHA] + gd.iPassesBad[iHA]);

        //iStTmp++;
        //} // foreach state

        // Substitutions
        if (game.data.team[iHA].ltSubstitutions != null) {
          for (int iS = 0; iS < game.data.team[iHA].ltSubstitutions.Count; iS++) {
            int[] iSub = game.data.team[iHA].ltSubstitutions[iS];
            float fMin = iSub[2];

            string sSubDesc = (iSub[2] + 1).ToString() + ". Min.: " + sTeam + " - " + MvcApplication.ckcore.ltPlayer[iSub[1]].plGame.sName + " für " + MvcApplication.ckcore.ltPlayer[iSub[0]].plGame.sName;

            if (string.IsNullOrEmpty(gd.sStatSubs)) gd.sStatSubs = "<br/><u>Spielerwechsel:</u>";
            gd.sStatSubs += "<br/>" + sSubDesc;

            gd.sTimelineIcons += "<img src=\"/Content/Icons/sub.png\" alt=\"Spielerwechsel\" style=\"position: absolute; top: " + iIconTop.ToString(nfi) + "px; width: 12px; left: " + ((fMin - 0.5) / 0.9).ToString(nfi) + "%\" title=\"" + sSubDesc + "\"/>";
          }
        }
      } // iHA

      // Referee quality
      gd.sRefereeQuality   = game.data.referee.fQuality.ToString("0.0%");
      gd.sRefereeDecisions = "-";
      if (game.data.referee.iDecisions[0] > 0) gd.sRefereeDecisions = (game.data.referee.iDecisions[1] / (float)game.data.referee.iDecisions[0]).ToString("0.0%");

      // Bar statistics
      float fPossessionH = 50f;
      if (nPossession[0] + nPossession[1] > 0) fPossessionH = 100f * nPossession[0] / (float)(nPossession[0] + nPossession[1]);

      gd.fDataH = new float[][] { new float[2] { (float)gd.iGoalsH, 0f }, new float[2] { (float)gd.iShoots[0], -1f }, new float[2] { (float)gd.iShootsOnGoal[0], -2f }, new float[2] {        fPossessionH, -3f }, new float[2] { (float)gd.iDuels[0], -4f }, new float[2] { (float)gd.iFouls[0], -5f }, new float[2] { (float)nCornerkick[0], -6f }, new float[2] { (float)nOffsite[0], -7f }, new float[2] { fPasses[0], -8f } };
      gd.fDataA = new float[][] { new float[2] { (float)gd.iGoalsA, 0f }, new float[2] { (float)gd.iShoots[1], -1f }, new float[2] { (float)gd.iShootsOnGoal[1], -2f }, new float[2] { 100f - fPossessionH, -3f }, new float[2] { (float)gd.iDuels[1], -4f }, new float[2] { (float)gd.iFouls[1], -5f }, new float[2] { (float)nCornerkick[1], -6f }, new float[2] { (float)nOffsite[1], -7f }, new float[2] { fPasses[1], -8f } };

      if (AccountController.checkUserIsAdmin(User.Identity.GetUserName())) {
        if (game.ball.plAtBall != null) {
#if _AI2
          float fShootOnGoal = game.ai.getChanceShootOnGoal(game.ball.plAtBall, 0);
#else
          float fShootOnGoal = game.ai.getChanceShootOnGoal    (game.ball.plAtBall);
#endif
          float fKeeperSave  = game.ai.getChanceShootKeeperSave(game.ball.plAtBall);

          gd.sAdminChanceShootOnGoal = "<br/><u>Change Schuss aufs Tor:</u> " + fShootOnGoal.ToString("0.0%");
          gd.sAdminChanceGoal        = "<br/><u>Change Tor:</u> " + (fShootOnGoal * (1f - fKeeperSave)).ToString("0.0%");
        }
      }

      // Heatmap
      gd.sDivHeatmap = "";
      if (iHeatmap >= 0) {
        byte iHA   =  0;
        int  iPlIx = -1;
        getStatisticHAPlayerIx(iHeatmap, game.data.nPlStart, out iHA, out iPlIx);
        gd.sDivHeatmap = getDivHeatmap(iHA, iState, iPlIx);
      }

      // Player action chances
      gd.fPlAction    = state.fPlAction;
      gd.fPlActionRnd = state.fPlActionRandom;

      Models.ViewGameModel.gameData2 gd2 = new Models.ViewGameModel.gameData2() {
        sUserId = sUserId,
        viewGd = gd,
        game = game
      };
      setViewGameDataList(gd2);
    }

    private void getStatisticHAPlayerIx(int i, byte nPlStart, out byte iHA, out int iPl)
    {
      iHA =  0;
      iPl = -1;
      if (i < 2) {
        iHA = (byte)i;
      } else {
        iHA = (byte)((i - 2) / nPlStart);
        iPl = (i - 2) - (iHA * nPlStart);
      }
    }

    private List<Models.ViewGameModel.drawLine> getShootLine(CornerkickGame.Game.State state, CornerkickGame.Game game, int iHA = -1, int iPlIx = -1)
    {
      List<Models.ViewGameModel.drawLine> ltDrawLine = new List<Models.ViewGameModel.drawLine>();

      CornerkickGame.Player plShoot = state.shoot.plShoot;

      if (plShoot == null) return ltDrawLine;
      if (iHA   >= 0 && plShoot.iHA    != iHA)   return ltDrawLine;
      if (iPlIx >= 0 && plShoot.iIndex != iPlIx) return ltDrawLine;

      string sTitle = "";
      if (state.shoot.fChanceOnGoal > 0f) {
        sTitle += "<div align=\"right\">";

        if (state.shoot.iResult > 1 && state.shoot.iResult < 5) sTitle += "<strong>";
        sTitle += "Gehalten: " + 0.ToString("0.0%") + " .. " + state.shoot.fChanceKeeperSave.ToString("0.0%");
        if (state.shoot.iResult > 1 && state.shoot.iResult < 5) sTitle += "</strong>";
        sTitle += "<br/>";

        if (state.shoot.iResult == 1) sTitle += "<strong>";
        sTitle += "Tor: " + state.shoot.fChanceKeeperSave.ToString("0.0%") + " .. " + state.shoot.fChanceOnGoal.ToString("0.0%");
        if (state.shoot.iResult == 1) sTitle += "</strong>";
        sTitle += "<br/>";

        if (state.shoot.iResult == 5 || state.shoot.iResult == 6) sTitle += "<strong>";
        sTitle += "Alu: " + state.shoot.fChanceOnGoal.ToString("0.0%") + " .. " + (state.shoot.fChanceOnGoal + state.shoot.fChancePostBar).ToString("0.0%");
        if (state.shoot.iResult == 5 || state.shoot.iResult == 6) sTitle += "</strong>";
        sTitle += "<br/>";

        if (state.shoot.iResult == 0) sTitle += "<strong>";
        sTitle += "Daneben: " + (state.shoot.fChanceOnGoal + state.shoot.fChancePostBar).ToString("0.0%") + " .. " + 1.ToString("0.0%");
        if (state.shoot.iResult == 0) sTitle += "</strong>";
        sTitle += "<br/>";

        sTitle += "<strong>Ergebnis: " + state.shoot.fRnd.ToString("0.0%") + "</strong>";

        sTitle += "</div>";
      }

      Models.ViewGameModel.drawLine drawLine = new Models.ViewGameModel.drawLine();

      drawLine.X0 = plShoot.ptPos.X;
      drawLine.Y0 = plShoot.ptPos.Y;
      if (state.shoot.iResult == 3) { // keeper
        CornerkickGame.Player plKeeper = CornerkickGame.Tool.getKeeper(game.player[1 - plShoot.iHA], game.iSuspensionIx, game.tc[plShoot.iHA].formation, game.ptPitch, game.data.nPlStart);

        drawLine.X1 = plKeeper.ptPos.X;
        drawLine.Y1 = plKeeper.ptPos.Y;
        drawLine.sColor = "yellow";
        drawLine.sTitle = sTitle;

        ltDrawLine.Add(drawLine);

        drawLine = new Models.ViewGameModel.drawLine();
        drawLine.X0 = ltDrawLine[0].X1;
        drawLine.Y0 = ltDrawLine[0].Y1;
      } else if (state.shoot.iResult == 5) { // post
        drawLine.X1 = (1 - state.shoot.iHA) * game.ptPitch.X;
        drawLine.Y1 = -2;
        drawLine.sColor = "yellow";
        drawLine.sTitle = sTitle;

        ltDrawLine.Add(drawLine);

        drawLine = new Models.ViewGameModel.drawLine();
        drawLine.X0 = ltDrawLine[0].X1;
        drawLine.Y0 = ltDrawLine[0].Y1;
      } else if (state.shoot.iResult == 6) { // bar
        drawLine.X1 = (1 - state.shoot.iHA) * game.ptPitch.X;
        drawLine.Y1 = 0;
        drawLine.sColor = "yellow";
        drawLine.sTitle = sTitle;

        ltDrawLine.Add(drawLine);

        drawLine = new Models.ViewGameModel.drawLine();
        drawLine.X0 = ltDrawLine[0].X1;
        drawLine.Y0 = ltDrawLine[0].Y1;
      }
      drawLine.X1 = state.ball.ptPos.X;
      drawLine.Y1 = state.ball.ptPos.Y;
      if      (state.shoot.iResult == 1)                             drawLine.sColor = "red";
      else if (state.shoot.iResult == 0 || state.shoot.iResult == 7) drawLine.sColor = "cyan";
      else                                                           drawLine.sColor = "yellow";

      drawLine.sTitle = sTitle;

      ltDrawLine.Add(drawLine);

      return ltDrawLine;
    }

    private Models.ViewGameModel.drawLine getPassLine(CornerkickGame.Game.State state, CornerkickGame.Game game, int iHA = -1, int iPlIx = -1)
    {
      Models.ViewGameModel.drawLine drawLine = new Models.ViewGameModel.drawLine();

      CornerkickGame.Player plPass = state.pass.plPasser;

      if (plPass == null) return drawLine;
      if (iHA >= 0 && plPass.iHA != iHA) return drawLine;
      if (iPlIx >= 0 && plPass.iIndex != iPlIx) return drawLine;

      drawLine.X0 = plPass.ptPos.X;
      drawLine.Y0 = plPass.ptPos.Y;
      drawLine.X1 = state.ball.ptPos.X;
      drawLine.Y1 = state.ball.ptPos.Y;
      drawLine.sColor = "lime";
      if      (state.pass.plReceiver == null) drawLine.sColor = "red";
      else if (state.pass.plReceiver.iHA != plPass.iHA) drawLine.sColor = "red";

      return drawLine;
    }

    private string getDuelIcon(CornerkickGame.Game.State state, CornerkickGame.Game game, int iHA = -1, int iPlIx = -1)
    {
      CornerkickGame.Player plDef = state.duel.plDef;
      if (plDef == null) return "";

      CornerkickGame.Player plOff = state.duel.plOff;
      if (plOff == null) return "";

      if (iPlIx >= 0 && plDef.iIndex != iPlIx && plOff.iIndex != iPlIx) return "";

      string sDuelDesc = MvcApplication.ckcore.ui.getMinuteString(state.duel.tsMinute, false) + " Min.: " +
                         state.duel.plDef.sName + " vs. " + state.duel.plOff.sName;
      sDuelDesc += "<br/>";

      if (state.duel.iResult > 1) sDuelDesc += "<strong>";
      sDuelDesc += "Foul: " + 0.ToString("0.0%") + " .. " + state.duel.fChanceFoul.ToString("0.0%");
      if (state.duel.iResult > 1) sDuelDesc += "</strong>";
      sDuelDesc += "<br/>";

      if (state.duel.iResult == 1) sDuelDesc += "<strong>";
      sDuelDesc += "Def.: " + state.duel.fChanceFoul.ToString("0.0%") + " .. " + (state.duel.fChanceFoul + state.duel.fChanceWinDef).ToString("0.0%");
      if (state.duel.iResult == 1) sDuelDesc += "</strong>";
      sDuelDesc += "<br/>";

      if (state.duel.iResult == 0) sDuelDesc += "<strong>";
      sDuelDesc += "Off.: " + (state.duel.fChanceFoul + state.duel.fChanceWinDef).ToString("0.0%").PadLeft(5) + " .. 100.0%";
      if (state.duel.iResult == 0) sDuelDesc += "</strong>";
      sDuelDesc += "<br/>";

      sDuelDesc += "<strong>Ergebnis: " + state.duel.fRnd.ToString("0.0%") + "</strong>";

      string sDefOff = "off";
      if (iHA >= 0 && iHA != state.duel.plOff.iHA) sDefOff = "def";

      string sImg = "duel_" + sDefOff + "_1"; // win off. / loose def.
      if (state.duel.iResult == 0) sImg = "duel_" + sDefOff + "_0"; // win def. / loose off.

      // win off. / fould (and card) def.
      if (iHA < 0 || iHA == state.duel.plDef.iHA) {
        if      (state.duel.iResult == 2) sImg = "whistle";
        else if (state.duel.iResult == 3) sImg = "yCard";
        else if (state.duel.iResult == 4) sImg = "yrCard";
        else if (state.duel.iResult == 5) sImg = "rCard";
      }

      string sDuelIcon = "<div style=\"position: absolute; top: " + ((plDef.ptPos.Y + game.ptPitch.Y) / (float)(2 * game.ptPitch.Y)).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; left: " + (plDef.ptPos.X / (float)game.ptPitch.X).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; z-index: 99\">";
      sDuelIcon += "<img class=\"tooltipDuel\" src=\"/Content/Icons/" + sImg + ".png\" alt=\"Karte\" style=\"position: relative; width: 12px\" title=\"" + sDuelDesc + "\"/>";
      sDuelIcon += "</div>";

      return sDuelIcon;
    }

    public Models.ViewGameModel.gameData getAllGameData(CornerkickGame.Game.Data gd, int iState = -1)
    {
      CornerkickManager.Club clbUser = ckClub();

      Models.ViewGameModel.gameData gD = new Models.ViewGameModel.gameData();
      gD.iTeamId = clbUser.iId;

      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (gd == null) return gD;

      // Initialize chart values
      gD.ltF = new List<Models.DataPointGeneral>[gd.nPlStart];
      for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<Models.DataPointGeneral>();
#if _PLOT_MORAL
      gD.ltM = new List<Models.DataPointGeneral>[user.game.data.nPlStart];
      for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();
#endif

      for (int iSt = 0; iSt < gd.ltState.Count; iSt++) {
        CornerkickGame.Game.State state = gd.ltState[iSt];
        addGameData(ref gD, gd, iSt, state.tsMinute.Seconds == 0 && state.bNewRound);

        if (iState == iSt) break;
      }

      return gD;
    }

    float fHeatmapMax = 0f;
    private string getDivHeatmap(byte iHA = 0, int iStateMax = 0, int iPlayer = -1)
    {
      CornerkickManager.User usr = ckUser();
      Models.ViewGameModel.gameData2 gd2 = getViewGameData(usr.id);

      if (gd2.game == null) return "";

      float[][] fHeatmap = MvcApplication.ckcore.ui.getHeatmap(gd2.game, iHA, ref fHeatmapMax, iStateMax, iPlayer);

      string sDiv = "";

      for (int iX = 0; iX < fHeatmap.Length; iX++) {
        float fXper = iX / (float)(fHeatmap.Length - 1);
        fXper -= 0.01f; // - 1% (half width)

        for (int iY = 0; iY < fHeatmap[iX].Length; iY++) {
          string sColor = "white";
          float fYper = iY / (float)(fHeatmap[iX].Length - 1);
          fYper -= 0.015f; // - 1.5% (half height)

          float fHeat = fHeatmap[iX][iY];

          if (fHeat == 0f) continue;

          int iZindex = 8;

          if (fHeat < 0.01f) {
            sColor = "DarkBlue";
            iZindex = 1;
          } else if (fHeat < 0.05f) {
            sColor = "LightSkyBlue";
            iZindex = 2;
          //} else if (fHeat < 0.2f) { sColor = "DarkGreen;
          //} else if (fHeat < 0.3f) { sColor = "ForestGreen;
          } else if (fHeat < 0.10f) {
            sColor = "Lime";
            iZindex = 3;
          } else if (fHeat < 0.20f) {
            sColor = "Yellow";
            iZindex = 4;
          } else if (fHeat < 0.30f) {
            sColor = "Orange";
            iZindex = 5;
          } else if (fHeat < 0.40f) {
            sColor = "Red";
            iZindex = 6;
          } else if (fHeat < 0.50f) {
            sColor = "Magenta";
            iZindex = 7;
          }
          //iZindex = (int)(fHeat * 10) + 1;

          sDiv += "<div style=\"position: absolute; width: 2%; height: 3%; top: " + fYper.ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; left: " + fXper.ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; background-color: " + sColor + "; -webkit-border-radius: 50%; -moz-border-radius: 50%; z-index:" + iZindex.ToString() + "\">" +
                     //"<h2 style=\"position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 100%; color: black; z-index:2\">" + fHeat.ToString("0%") + "</h2>" +
                  "</div>";
        }
      }

      return sDiv;
    }

    public JsonResult AdminPlay(Models.ViewGameModel view)
    {
      CornerkickManager.User userAdmin = ckUser();

      if (userAdmin.game == null) {
        userAdmin.game = MvcApplication.ckcore.game.tl.getDefaultGame();
      }

      userAdmin.game.data.iGameSpeed = 300;

      userAdmin.game.start();

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AdminStop(Models.ViewGameModel view)
    {
      CornerkickManager.User userAdmin = ckUser();

      if (userAdmin.game == null) Json(false, JsonRequestBehavior.AllowGet);

      userAdmin.game.stop();

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AdminNext(Models.ViewGameModel view, sbyte iNextAction, int iX = -1, int iY = -1)
    {
      CornerkickManager.User userAdmin = ckUser();

      if (userAdmin.game == null) {
        userAdmin.game = MvcApplication.ckcore.game.tl.getDefaultGame();
      }

      userAdmin.game.data.iGameSpeed = 1;

      userAdmin.game.next(iPlayerNextAction: iNextAction, iPassNextActionX: iX, iPassNextActionY: iY);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AdminNew(Models.ViewGameModel view)
    {
      CornerkickManager.User userAdmin = ckUser();

      userAdmin.game = MvcApplication.ckcore.game.tl.getDefaultGame();

      userAdmin.game.data.iGameSpeed = 300;

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AdminSetPos(Models.ViewGameModel view, int iHA, int iPl, int iX, int iY)
    {
      CornerkickManager.User userAdmin = ckUser();

      if (userAdmin.game == null) {
        userAdmin.game = MvcApplication.ckcore.game.tl.getDefaultGame();
      }

      if (iHA < 0) {
        userAdmin.game.ball.ptPos = new System.Drawing.Point(iX, iY);
        userAdmin.game.ball.iPassStep  = 0;
        userAdmin.game.ball.nPassSteps = 0;
        userAdmin.game.ball.setPos();
        userAdmin.game.ball.plAtBall = null;
      } else {
        userAdmin.game.player[iHA][iPl].ptPos = new System.Drawing.Point(iX, iY);
      }

      for (int jHA = 0; jHA < 2; jHA++) {
        foreach (CornerkickGame.Player pl in userAdmin.game.player[jHA]) {
          if (pl.ptPos == userAdmin.game.ball.ptPos) {
            userAdmin.game.ball.plAtBall = pl;
            break;
          }
        }
      }

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AdminSetReferee(Models.ViewGameModel view, float fReferee)
    {
      CornerkickManager.User userAdmin = ckUser();

      if (userAdmin.game == null) {
        userAdmin.game = MvcApplication.ckcore.game.tl.getDefaultGame();
      }

      userAdmin.game.data.referee.fStrict = fReferee;

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public ActionResult AdminSetTaktik(int iTaktik, float fTaktik)
    {
      float fRet = 0f;
      byte iTactic = 0;

      CornerkickManager.User usrAdmin = ckUser();

      CornerkickGame.Tactic tc = usrAdmin.game.data.team[0].ltTactic[iTactic];
      if      (iTaktik == 0) tc.fOrientation = fTaktik;
      else if (iTaktik == 1) tc.fPower       = fTaktik;
      else if (iTaktik == 2) tc.fShootFreq   = fTaktik;
      else if (iTaktik == 3) tc.fAggressive  = fTaktik;
      else if (iTaktik == 4) tc.fPassRisk    = fTaktik;
      else if (iTaktik == 5) tc.fPassLength  = fTaktik;
      else if (iTaktik == 6) tc.fPassFreq    = fTaktik;
      else if (iTaktik == 7) {
        tc.fPassLeft = fTaktik;
        if (tc.fPassLeft + tc.fPassRight > 1f) tc.fPassRight = (float)Math.Round(1f - tc.fPassLeft,  2);
        fRet = tc.fPassRight;
      } else if (iTaktik == 8) {
        tc.fPassRight = fTaktik;
        if (tc.fPassLeft + tc.fPassRight > 1f) tc.fPassLeft  = (float)Math.Round(1f - tc.fPassRight, 2);
        fRet = tc.fPassLeft;
      } else if (iTaktik == 9) tc.iGapOffsite = (int)Math.Round(fTaktik);

      // Set tactic of current game
      if (usrAdmin.game != null) {
        usrAdmin.game.data.team[0].ltTactic[iTactic] = tc;
        usrAdmin.game.data.team[1].ltTactic[iTactic] = tc;
        usrAdmin.game.tc[0] = tc;
        usrAdmin.game.tc[1] = tc;
      }

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AdminGetPlayerChances()
    {
      CornerkickManager.User usrAdmin = ckUser();

      if (usrAdmin.game != null) {
        float[] fPlAction;
        sbyte iAction = usrAdmin.game.ai.getPlayerAction(usrAdmin.game.ball.plAtBall, out fPlAction);

        return Json(fPlAction, JsonRequestBehavior.AllowGet);
      }

      return Json(null, JsonRequestBehavior.AllowGet);
    }
  }
}