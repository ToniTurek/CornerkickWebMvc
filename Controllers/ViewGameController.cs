#define _AI2

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
    private static Models.ViewGameModel.gameData gD;

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
    [Authorize]
    public ActionResult ViewGame(Models.ViewGameModel view)
    {
      view.iStateGlobal = 0;

      // Get user
      CornerkickManager.User user = ckUser();
      if (user == null) return View(view);

      // Get user club
      CornerkickManager.Club clubUser = ckClub();

      view.bAdmin = AccountController.checkUserIsAdmin(User.Identity.GetUserName());

      if (view.bAdmin && user.game == null) {
        user.game = MvcApplication.ckcore.game.tl.getDefaultGame();
        user.game.data.iGameSpeed = 300;
      }

      view.ddlGames = new List<SelectListItem>();

      List<FileInfo> fiGames = getFileInfoGames(clubUser);

      foreach (FileInfo ckg in fiGames) {
        DateTime dtGame;
        int iTeamIdH;
        int iTeamIdA;
        string sFilenameInfo = getFilenameInfo(ckg, out dtGame, out iTeamIdH, out iTeamIdA);
        if (string.IsNullOrEmpty(sFilenameInfo)) continue;

        view.ddlGames.Insert(0,
          new SelectListItem {
            Text  = sFilenameInfo,
            Value = ckg.Name
          }
        );
      }

      if (view.ddlGames.Count > 0) view.ddlGames[0].Selected = true;

      if (user.game == null && fiGames.Count > 0) {
        string sFilenameGame = Path.Combine(MvcApplication.getHomeDir(), "save", "games", fiGames[fiGames.Count - 1].Name);
        try {
          user.game = MvcApplication.ckcore.io.loadGame(sFilenameGame);
        } catch {
          MvcApplication.ckcore.tl.writeLog("Unable to load game: '" + sFilenameGame + "'", MvcApplication.ckcore.sErrorFile);
        }
      }

      // Insert next games
      List<CornerkickGame.Game.Data> ltGdNextGames = MvcApplication.ckcore.tl.getNextGames(clubUser, MvcApplication.ckcore.dtDatum);
      for (byte j = 0; j < ltGdNextGames.Count; j++) {
        CornerkickGame.Game.Data gd = ltGdNextGames[j];
        view.ddlGames.Insert(0, new SelectListItem {
          Text = gd.dt.ToString("d", Controllers.MemberController.getCiStatic(User)) + " " + gd.dt.ToString("t", Controllers.MemberController.getCiStatic(User)) + " *: " + gd.team[0].sTeam + " - " + gd.team[1].sTeam,
          Value = (-j - 1).ToString()
        });
      }

      view.ddlShoots = new List<SelectListItem>(view.ddlHeatmap);
      view.ddlDuels  = new List<SelectListItem>(view.ddlHeatmap);
      view.ddlPasses = new List<SelectListItem>(view.ddlHeatmap);

      fHeatmapMax = 0f;

      setGame(view, user.game);

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
          string[] sFilenameData = Path.GetFileNameWithoutExtension(ckg.Name).Split('_');
          if (sFilenameData.Length < 3) continue;

          DateTime dtGame = new DateTime();
          int iTeamIdH = -1;
          int iTeamIdA = -1;
          if (string.IsNullOrEmpty(getFilenameInfo(ckg, out dtGame, out iTeamIdH, out iTeamIdA))) continue;

          if (iTeamIdH == clubUser.iId || iTeamIdA == clubUser.iId || AccountController.checkUserIsAdmin(User)) fiGames.Add(ckg);
        }
      }

      return fiGames;
    }

    private string getFilenameInfo(FileInfo fiGame, out DateTime dtGame, out int iTeamIdH, out int iTeamIdA)
    {
      string sFilenameInfo = "";

      dtGame = new DateTime();
      iTeamIdH = -1;
      iTeamIdA = -1;

      string[] sFilenameData = Path.GetFileNameWithoutExtension(fiGame.Name).Split('_');
      if (sFilenameData.Length < 3) return null;

      // Date/Time
      if (!DateTime.TryParseExact(sFilenameData[0], "yyyyMMdd-HHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dtGame)) return null;

      // Team names
      string[] sFilenameDataTeamIds = sFilenameData[2].Split('-');
      if (!int.TryParse(sFilenameDataTeamIds[0], out iTeamIdH)) return null;
      if (!int.TryParse(sFilenameDataTeamIds[1], out iTeamIdA)) return null;

      return dtGame.ToString("d", Controllers.MemberController.getCiStatic(User)) + " " + dtGame.ToString("t", Controllers.MemberController.getCiStatic(User)) + ": " + MvcApplication.ckcore.ltClubs[iTeamIdH].sName + " - " + MvcApplication.ckcore.ltClubs[iTeamIdA].sName;
    }

    public JsonResult loadGame(Models.ViewGameModel view, string sFilename)
    {
      // Get user
      CornerkickManager.User user = ckUser();
      if (user == null) return Json(false, JsonRequestBehavior.AllowGet);

      string sFilenameGame = Path.Combine(MvcApplication.getHomeDir(), "save", "games", sFilename);

      try {
        user.game = MvcApplication.ckcore.io.loadGame(sFilenameGame);
        setGame(view, user.game);
      } catch {
        MvcApplication.ckcore.tl.writeLog("Unable to load game: '" + sFilenameGame + "'", MvcApplication.ckcore.sErrorFile);
      }

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public void setGame(Models.ViewGameModel view, CornerkickGame.Game game)
    {
      view.sColorJerseyH = new string[4] { "white", "blue", "blue", "white" };
      view.sColorJerseyA = new string[4] { "white", "red",  "red",  "white" };

      if (game == null) {
        view.gD.iGoalsH = -1;
        view.gD.iGoalsA = -1;

        return;
      }

      gD = new Models.ViewGameModel.gameData();

      string sEmblemDir = Path.Combine(MvcApplication.getHomeDir(), "Content", "Uploads", "emblems");
      string sEmblemDirHtml = "/Content/Uploads/emblems/";
      string sEmblemH = game.data.team[0].iTeamId.ToString() + ".png";
      string sEmblemA = game.data.team[1].iTeamId.ToString() + ".png";

      if (game.data.team[0].iTeamId >= 0 && game.data.team[0].iTeamId < MvcApplication.ckcore.ltClubs.Count) {
        CornerkickManager.Club clubH = MvcApplication.ckcore.ltClubs[game.data.team[0].iTeamId];

        if (clubH.bNation) {
          sEmblemDir = Path.Combine(MvcApplication.getHomeDir(), "Content", "Icons", "flags");
          sEmblemDirHtml = "/Content/Icons/flags/";
          int iNationH = clubH.iLand;
          if (iNationH >= 0 && iNationH < MvcApplication.ckcore.sLandShort.Length) sEmblemH = MvcApplication.ckcore.sLandShort[iNationH] + ".png";
        }

        for (byte iC = 0; iC < clubH.cl.Length; iC++) view.sColorJerseyH[iC] = "rgb(" + clubH.cl[iC].R.ToString() + "," + clubH.cl[iC].G.ToString() + "," + clubH.cl[iC].B.ToString() + ")";
      }

      if (game.data.team[1].iTeamId >= 0 && game.data.team[1].iTeamId < MvcApplication.ckcore.ltClubs.Count) {
        CornerkickManager.Club clubA = MvcApplication.ckcore.ltClubs[game.data.team[1].iTeamId];

        if (clubA.bNation) {
          sEmblemDir = Path.Combine(MvcApplication.getHomeDir(), "Content", "Icons", "flags");
          sEmblemDirHtml = "/Content/Icons/flags/";
          int iNationA = clubA.iLand;
          if (iNationA >= 0 && iNationA < MvcApplication.ckcore.sLandShort.Length) sEmblemA = MvcApplication.ckcore.sLandShort[iNationA] + ".png";
        }

        for (byte iC = 0; iC < clubA.cl.Length; iC++) view.sColorJerseyA[iC] = "rgb(" + clubA.cl[iC].R.ToString() + "," + clubA.cl[iC].G.ToString() + "," + clubA.cl[iC].B.ToString() + ")";
      }

      /*
      if (!System.IO.File.Exists(Path.Combine(sEmblemDir, sEmblemH))) sEmblemH = "0.png";
      if (!System.IO.File.Exists(Path.Combine(sEmblemDir, sEmblemA))) sEmblemA = "0.png";
      */

      view.sEmblemH = sEmblemDirHtml + sEmblemH;
      view.sEmblemA = sEmblemDirHtml + sEmblemA;

      string[] sHA = new string[2] { "H", "A" };
      // Add player to heatmap
      for (byte iHA = 0; iHA < 2; iHA++) {
        for (byte iPl = 0; iPl < game.data.nPlStart; iPl++) {
          view.ddlHeatmap.Add(new SelectListItem { Text = "(" + sHA[iHA] + ") " + game.player[iHA][iPl].sName + " - " + game.player[iHA][iPl].iNr, Value = (2 + (iHA * game.data.nPlStart) + iPl).ToString() });
        }
      }

      view.iGameSpeed = game.data.iGameSpeed;

      view.game = game;

      gD = getAllGameData(view);

      view.gD = gD;
    }

    public JsonResult ViewGameLocations(int iState = -1/*, int iSleep = 0*/, bool bAverage = false)
    {
      Models.ViewGameModel.ltLoc = new List<float[]>();
      Models.ViewGameModel.gameLoc gLoc = new Models.ViewGameModel.gameLoc();

      CornerkickManager.User user = ckUser();

      if (user.game == null) {
        Response.StatusCode = 1;
        return Json(Models.ViewGameModel.ltLoc, JsonRequestBehavior.AllowGet);
      }

      bool bAdmin = AccountController.checkUserIsAdmin(User);

      // Colors home
      gLoc.sColorJerseyH = new string[2] { "white", "blue" };
      if (user.game.data.team[0].iTeamId >= 0 && user.game.data.team[0].iTeamId < MvcApplication.ckcore.ltClubs.Count) {
        CornerkickManager.Club clubH = MvcApplication.ckcore.ltClubs[user.game.data.team[0].iTeamId];
        for (byte iC = 0; iC < gLoc.sColorJerseyH.Length; iC++) gLoc.sColorJerseyH[iC] = "rgb(" + clubH.cl[iC].R.ToString() + "," + clubH.cl[iC].G.ToString() + "," + clubH.cl[iC].B.ToString() + ")";
        gLoc.bJerseyTextColorWhiteH = Controllers.MemberController.checkColorBW(clubH.cl[0]);
      }

      // Colors away
      gLoc.sColorJerseyA = new string[2] { "white", "red" };
      if (user.game.data.team[1].iTeamId >= 0 && user.game.data.team[1].iTeamId < MvcApplication.ckcore.ltClubs.Count) {
        CornerkickManager.Club clubA = MvcApplication.ckcore.ltClubs[user.game.data.team[1].iTeamId];
        for (byte iC = 0; iC < gLoc.sColorJerseyA.Length; iC++) gLoc.sColorJerseyA[iC] = "rgb(" + clubA.cl[iC + 2].R.ToString() + "," + clubA.cl[iC + 2].G.ToString() + "," + clubA.cl[iC + 2].B.ToString() + ")";
        gLoc.bJerseyTextColorWhiteA = Controllers.MemberController.checkColorBW(clubA.cl[2]);
      }

      // Set interval
      if (MvcApplication.ckcore.ltUser.Count > 0) {
        if (MvcApplication.ckcore.ltUser[0].nextGame != null) gLoc.iInterval = MvcApplication.ckcore.ltUser[0].nextGame.iGameSpeed;
      }

      CornerkickGame.Game.State state = user.game.newState();
      if (user.game.data.ltState.Count > 0) state = user.game.data.ltState[user.game.data.ltState.Count - 1];

      if (iState > 0 && iState < user.game.data.ltState.Count) state = user.game.data.ltState[iState];
      //if (fTime >= 0f) state = MvcApplication.ckcore.game.tl.getState(user.game.data, fTime);
      //CornerkickGame.Game.State state = MvcApplication.ckcore.game.tl.getState(user.game.data, fTime);

      float fFinished = 0f;
      /*if (fTime >= 0f) fFinished = state.i;
      else */if (user.game.data.bFinished) fFinished = 1f;
      gLoc.bFinished = user.game.data.bFinished;

      // Ball
      Models.ViewGameModel.gameBall gBall = new Models.ViewGameModel.gameBall();
      CornerkickGame.Game.Ball ball;
      if (iState < 0) ball = user.game.ball;
      else            ball = state.ball;

      CornerkickGame.Game.PointBall pbBall = new CornerkickGame.Game.PointBall();
      pbBall = ball.Pos;

      if (bAverage) {
        System.Drawing.Point ptAve = MvcApplication.ckcore.ui.getAveragePos(user.game, -1, -1, iState);
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
          if (iState < 0) pl = user.game.player[iHA][iP];
          else            pl = state.player[iHA][iP];

          if (string.IsNullOrEmpty(pl.sName)) continue;

          if      (pl.bYellowCard)                              gPlayer.iCard = 1;
          else if (pl.iSuspension[user.game.iSuspensionIx] > 0) gPlayer.iCard = 2;

          gPlayer.ptPos = pl.ptPos;
          if (bAverage) {
            System.Drawing.Point ptAve = MvcApplication.ckcore.ui.getAveragePos(user.game, iHA, iP, iState);
            gPlayer.ptPos = ptAve;
          }

          gPlayer.ptPosTarget = pl.ptPosTarget;
          gPlayer.iLookAt = pl.iLookAt;
          gPlayer.iNo = pl.iNr;

          gLoc.ltPlayer.Add(gPlayer);
          //Models.ViewGameModel.ltLoc.Add(new float[5] { iPosX, iPosY, pl.iLookAt, pl.iNr, fCard });

          if (bAdmin && iState >= 0) {
            user.game.player[iHA][iP].iHA     = pl.iHA;
            user.game.player[iHA][iP].iIndex  = pl.iIndex;
            user.game.player[iHA][iP].ptPos.X = pl.ptPos.X;
            user.game.player[iHA][iP].ptPos.Y = pl.ptPos.Y;
            user.game.player[iHA][iP].iLookAt = pl.iLookAt;
            user.game.player[iHA][iP].fSteps = 6f;
          }
        }
      }

      // If admin: set finished to true to avoid recall of drawGame()
      if (bAdmin) gLoc.bFinished = true;

      if (bAdmin && iState >= 0) {
        user.game.ball = ball;
        user.game.tsMinute = state.tsMinute;

        if (user.game.ball.iPassStep == 0) {
          for (byte iHA = 0; iHA < 2; iHA++) {
            for (int iP = 0; iP < MvcApplication.ckcore.game.data.nPlStart; iP++) {
              if (ball.ptPos == user.game.player[iHA][iP].ptPos) {
                user.game.ball.plAtBall = user.game.player[iHA][iP];
                break;
              }
            }
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

      return Json(gLoc, JsonRequestBehavior.AllowGet);
    }

    public JsonResult ViewGameGetDataStatisticObject(Models.ViewGameModel view, int iState = -1, int iStateLast = 0, int iHeatmap = -1, int iAllShoots = -1, int iAllDuels = -1, int iAllPasses = -1)
    {
      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();

      if (user.game == null) {
        setGameData(ref gD, null, user, club, iState, iHeatmap, iAllShoots, iAllDuels, iAllPasses);
        return Json(gD, JsonRequestBehavior.AllowGet);
      }

      CornerkickGame.Game.Data gameData = user.game.data;

      // Clear chart arrays
      gD.ltF = new List<Models.DataPointGeneral>[user.game.data.nPlStart];
      for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<Models.DataPointGeneral>();

      gD.ltM = new List<Models.DataPointGeneral>[user.game.data.nPlStart];
      for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();

      if (gD.nStates == 0) {
        gD = getAllGameData(view);
      } else if (iState >= 0) {
        gD = getAllGameData(view, iState);
      } else if (iState <  0) {
        // Return null if not new round
        if (gameData.ltState.Count > 0) {
          if (!gameData.ltState[gameData.ltState.Count - 1].bNewRound) return Json(null, JsonRequestBehavior.AllowGet);
        }

        // Add game data to struct
        for (int i = iStateLast; i < gameData.ltState.Count; i++) addGameData(ref gD, gameData, user, club, i);
      }

      setGameData(ref gD, gameData, user, club, iState, iHeatmap, iAllShoots, iAllDuels, iAllPasses);

      return Json(gD, JsonRequestBehavior.AllowGet);
    }

    // Adds comment, shoots/cards and chart data of current state to gD
    private void addGameData(ref Models.ViewGameModel.gameData gD, CornerkickGame.Game.Data gameData, CornerkickManager.User user, CornerkickManager.Club club, int iState = -1, bool bAddFMList = true)
    {
      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (user.game == null) return;

      if (gameData.ltState.Count == 0) return;

      CornerkickGame.Game.State state = gameData.ltState[gameData.ltState.Count - 1];
      if (iState >= 0 && iState < gameData.ltState.Count) state = gameData.ltState[iState];

      float fLeft = ((state.tsMinute.Hours * 60f) + state.tsMinute.Minutes + (state.tsMinute.Seconds / 60f)) / 0.9f;
      if (gameData.bFinished) fLeft = (100.0f * state.i) / gameData.ltState.Count;

      for (byte iHA = 0; iHA < 2; iHA++) {
        int iIconTop = 2;
        if (iHA == 1) iIconTop = 44;

        string sTeam = gameData.team[0].sTeam;
        if (iHA > 0) sTeam = gameData.team[1].sTeam;

        string sOnClick = "";
        string sCursor = "";
        if (gameData.bFinished) {
          sOnClick = "onclick =\"javascript: drawGame(" + state.i.ToString() + ")\" ";
          sCursor = "; cursor: pointer";
        }

        // Shoots
        CornerkickGame.Game.Shoot shoot = state.shoot;
        if (shoot.plShoot != null && shoot.iHA == iHA) {
          float fDist = CornerkickGame.Tool.getDistanceToGoal(shoot.plShoot, MvcApplication.ckcore.game.ptPitch.X, MvcApplication.ckcore.game.fConvertDist2Meter)[0];

          string sShootDesc = MvcApplication.ckcore.ui.getMinuteString(shoot.tsMinute, false) + " Min.: " +
                              shoot.iGoalsH.ToString() + ":" + shoot.iGoalsA.ToString();
          sShootDesc += " - " + shoot.plShoot.sName;
          if (shoot.iType == 5) sShootDesc += ", FE";
          else                  sShootDesc += ", Entf.:" + fDist.ToString("0.0").PadLeft(5) + "m";
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
          gD.iShoots[iHA]++;
          if (shoot.iResult > 0 && shoot.iResult < 7) gD.iShootsOnGoal[iHA]++;
        } // shoot

        // Passes
        CornerkickGame.Game.Pass pass = state.pass;
        if (pass.plPasser != null && pass.plPasser.iHA == iHA) {
          if      (pass.plReceiver     == null)              gD.iPassesBad [iHA]++;
          else if (pass.plReceiver.iHA != pass.plPasser.iHA) gD.iPassesBad [iHA]++;
          else if (pass.plReceiver     != pass.plPasser)     gD.iPassesGood[iHA]++;
        }

        // Duels
        CornerkickGame.Game.Duel duel = state.duel;
        if (duel.plDef != null && duel.plDef.iHA == iHA) {
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
          gD.iDuels[iHA]++;
          if (duel.iResult > 1) gD.iFouls[iHA]++;
        }
      } // iHA

      byte jHA = 0;
      if (user.game.data.team[1].iTeamId == club.iId) jHA = 1;

      // Chart
      if (bAddFMList) {
        if (gD.ltF == null || iState == -1) {
          gD.ltF = new List<Models.DataPointGeneral>[user.game.data.nPlStart];
          for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<Models.DataPointGeneral>();
        }

        if (gD.ltM == null || iState == -1) {
          gD.ltM = new List<Models.DataPointGeneral>[user.game.data.nPlStart];
          for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();
        }

        for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) {
          gD.ltF[iPl].Add(new Models.DataPointGeneral(state.i, state.player[jHA][iPl].fFresh));
        }
        for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) {
          gD.ltM[iPl].Add(new Models.DataPointGeneral(state.i, state.player[jHA][iPl].fMoral));
        }
      }
    }

    private void setGameData(ref Models.ViewGameModel.gameData gD, CornerkickGame.Game.Data gameData, CornerkickManager.User user, CornerkickManager.Club club, int iState = -1, int iHeatmap = -1, int iAllShoots = -1, int iAllDuels = -1, int iAllPasses = -1)
    {
      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (user.game == null) {
        gD.iGoalsH = -1;
        gD.iGoalsA = -1;
        return;
      }

      if (gameData.ltState.Count == 0) return;

      CornerkickGame.Game.State state = gameData.ltState[gameData.ltState.Count - 1];
      if (iState >= 0 && iState < gameData.ltState.Count) state = gameData.ltState[iState];

      gD.nStates = gameData.ltState.Count;

      gD.tsMinute = state.tsMinute;

      gD.ltDrawLineShoot = new List<Models.ViewGameModel.drawLine>();
      gD.ltDrawLinePass  = new List<Models.ViewGameModel.drawLine>();
      gD.sCard = "";
      gD.sStatSubs = "";

      // Draw shoot on pitch
      if ((iState >= 0 && iState < gameData.ltState.Count) || iAllShoots >= 0) {
        if (iAllShoots >= 0) {
          byte iHA   =  0;
          int  iPlIx = -1;
          getStatisticHAPlayerIx(iAllShoots, user.game.data.nPlStart, out iHA, out iPlIx);

          for (int iSt = 0; iSt < gameData.ltState.Count; iSt++) {
            CornerkickGame.Game.State stateTmp = gameData.ltState[iSt];
            gD.ltDrawLineShoot.AddRange(getShootLine(stateTmp, user.game, iHA, iPlIx));

            if (iState >= 0 && iSt >= iState) break; // If review --> stop at selected state
          }
        } else {
          gD.ltDrawLineShoot = getShootLine(state, user.game);
        }
      }

      // Draw pass on pitch
      if ((iState >= 0 && iState < gameData.ltState.Count) || iAllPasses >= 0) {
        if (iAllPasses >= 0) {
          byte iHA = 0;
          int iPlIx = -1;
          getStatisticHAPlayerIx(iAllPasses, user.game.data.nPlStart, out iHA, out iPlIx);

          for (int iSt = 0; iSt < gameData.ltState.Count; iSt++) {
            CornerkickGame.Game.State stateTmp = gameData.ltState[iSt];
            gD.ltDrawLinePass.Add(getPassLine(stateTmp, user.game, iHA, iPlIx));

            if (iState >= 0 && iSt >= iState) break; // If review --> stop at selected state
          }
        } else {
          gD.ltDrawLinePass.Clear();
          gD.ltDrawLinePass.Add(getPassLine(state, user.game));
        }
      }

      // Draw duel on pitch
      if ((iState >= 0 && iState < gameData.ltState.Count) || iAllDuels >= 0) {
        // Show duel on pitch
        if (iAllDuels >= 0) {
          byte iHA   =  0;
          int  iPlIx = -1;
          getStatisticHAPlayerIx(iAllDuels, user.game.data.nPlStart, out iHA, out iPlIx);

          for (int iSt = 0; iSt < gameData.ltState.Count; iSt++) {
            CornerkickGame.Game.State stateTmp = gameData.ltState[iSt];
            gD.sCard += getDuelIcon(stateTmp, user.game, iHA, iPlIx);

            if (iState >= 0 && iSt >= iState) break; // If review --> stop at selected state
          }
        } else {
          gD.sCard = getDuelIcon(state, user.game);
        }
      }

      int  [] nPossession = new int  [2];
      int  [] nCornerkick = new int  [2];
      int  [] nOffsite    = new int  [2];
      float[] fPasses     = new float[2];

      for (byte iHA = 0; iHA < 2; iHA++) {
        int iIconTop = 2;
        if (iHA == 1) iIconTop = 44;

        string sTeam = gameData.team[0].sTeam;
        if (iHA > 0) sTeam = gameData.team[1].sTeam;

        //int iStTmp = 0;
        // loop over states
        //foreach (CornerkickGame.Game.State state in gameData.ltState) {

        if (iHA == 0) {
          gD.iGoalsH = state.iGoalsH;
          nPossession[0] = state.iPossessionH;
          nCornerkick[0] = state.iCornerkickH;
          nOffsite   [0] = state.iOffsiteH;
        } else {
          gD.iGoalsA = state.iGoalsA;
          nPossession[1] = state.iPossessionA;
          nCornerkick[1] = state.iCornerkickA;
          nOffsite   [1] = state.iOffsiteA;
        }
        if (gD.iPassesGood[iHA] + gD.iPassesBad[iHA] > 0) fPasses[iHA] = (100 * gD.iPassesGood[iHA]) / (float)(gD.iPassesGood[iHA] + gD.iPassesBad[iHA]);

        //iStTmp++;
        //} // foreach state

        // Substitutions
        if (gameData.team[iHA].ltSubstitutions != null) {
          for (int iS = 0; iS < gameData.team[iHA].ltSubstitutions.Count; iS++) {
            int[] iSub = gameData.team[iHA].ltSubstitutions[iS];
            float fMin = iSub[2];

            string sSubDesc = iSub[2].ToString() + ". Min.: " + sTeam + " - " + MvcApplication.ckcore.ltPlayer[iSub[1]].sName + " für " + MvcApplication.ckcore.ltPlayer[iSub[0]].sName;

            if (string.IsNullOrEmpty(gD.sStatSubs)) gD.sStatSubs = "<br/><u>Spielerwechsel:</u>";
            gD.sStatSubs += "<br/>" + sSubDesc;

            gD.sTimelineIcons += "<img src=\"/Content/Icons/sub.png\" alt=\"Spielerwechsel\" style=\"position: absolute; top: " + iIconTop.ToString(nfi) + "px; width: 12px; left: " + ((fMin - 0.5) / 0.9).ToString(nfi) + "%\" title=\"" + sSubDesc + "\"/>";
          }
        }
      } // iHA

      // Referee quality
      gD.sRefereeQuality   = gameData.referee.fQuality.ToString("0.0%");
      gD.sRefereeDecisions = "-";
      if (gameData.referee.iDecisions[0] > 0) gD.sRefereeDecisions = (gameData.referee.iDecisions[1] / (float)gameData.referee.iDecisions[0]).ToString("0.0%");

      // Bar statistics
      float fPossessionH = 50f;
      if (nPossession[0] + nPossession[1] > 0) fPossessionH = 100f * nPossession[0] / (float)(nPossession[0] + nPossession[1]);

      gD.fDataH = new float[][] { new float[2] { (float)gD.iGoalsH, 0f }, new float[2] { (float)gD.iShoots[0], -1f }, new float[2] { (float)gD.iShootsOnGoal[0], -2f }, new float[2] {        fPossessionH, -3f }, new float[2] { (float)gD.iDuels[0], -4f }, new float[2] { (float)gD.iFouls[0], -5f }, new float[2] { (float)nCornerkick[0], -6f }, new float[2] { (float)nOffsite[0], -7f }, new float[2] { fPasses[0], -8f } };
      gD.fDataA = new float[][] { new float[2] { (float)gD.iGoalsA, 0f }, new float[2] { (float)gD.iShoots[1], -1f }, new float[2] { (float)gD.iShootsOnGoal[1], -2f }, new float[2] { 100f - fPossessionH, -3f }, new float[2] { (float)gD.iDuels[1], -4f }, new float[2] { (float)gD.iFouls[1], -5f }, new float[2] { (float)nCornerkick[1], -6f }, new float[2] { (float)nOffsite[1], -7f }, new float[2] { fPasses[1], -8f } };

      if (AccountController.checkUserIsAdmin(User.Identity.GetUserName())) {
        if (user.game.ball.plAtBall != null) {
#if _AI2
          float fShootOnGoal = user.game.ai.getChanceShootOnGoal(user.game.ball.plAtBall, 0);
#else
          float fShootOnGoal = user.game.ai.getChanceShootOnGoal    (user.game.ball.plAtBall);
#endif
          float fKeeperSave  = user.game.ai.getChanceShootKeeperSave(user.game.ball.plAtBall);

          gD.sAdminChanceShootOnGoal = "<br/><u>Change Schuss aufs Tor:</u> " + fShootOnGoal.ToString("0.0%");
          gD.sAdminChanceGoal        = "<br/><u>Change Tor:</u> " + (fShootOnGoal * (1f - fKeeperSave)).ToString("0.0%");
        }
      }

      // Heatmap
      gD.sDivHeatmap = "";
      if (iHeatmap >= 0) {
        byte iHA   =  0;
        int  iPlIx = -1;
        getStatisticHAPlayerIx(iHeatmap, user.game.data.nPlStart, out iHA, out iPlIx);
        gD.sDivHeatmap = getDivHeatmap(iHA, iState, iPlIx);
      }

      // Player action chances
      gD.fPlAction    = state.fPlAction;
      gD.fPlActionRnd = state.fPlActionRandom;
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

      Models.ViewGameModel.drawLine drawLine = new Models.ViewGameModel.drawLine();

      drawLine.X0 = plShoot.ptPos.X;
      drawLine.Y0 = plShoot.ptPos.Y;
      if (state.shoot.iResult == 3) { // keeper
        CornerkickGame.Player plKeeper = CornerkickGame.Tool.getKeeper(game.player[plShoot.iHA], game.iSuspensionIx, game.tc[plShoot.iHA].formation, game.ptPitch, game.data.nPlStart);

        drawLine.X1 = plKeeper.ptPos.X;
        drawLine.Y1 = plKeeper.ptPos.Y;
        drawLine.sColor = "yellow";

        ltDrawLine.Add(drawLine);

        drawLine = new Models.ViewGameModel.drawLine();
        drawLine.X0 = ltDrawLine[0].X1;
        drawLine.Y0 = ltDrawLine[0].Y1;
      } else if (state.shoot.iResult == 5) { // post
        drawLine.X1 = (1 - state.shoot.iHA) * game.ptPitch.X;
        drawLine.Y1 = -2;
        drawLine.sColor = "yellow";

        ltDrawLine.Add(drawLine);

        drawLine = new Models.ViewGameModel.drawLine();
        drawLine.X0 = ltDrawLine[0].X1;
        drawLine.Y0 = ltDrawLine[0].Y1;
      } else if (state.shoot.iResult == 6) { // bar
        drawLine.X1 = (1 - state.shoot.iHA) * game.ptPitch.X;
        drawLine.Y1 = 0;
        drawLine.sColor = "yellow";

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

      string sDuelDesc = MvcApplication.ckcore.ui.getMinuteString(state.duel.tsMinute, false) +
                         " - " + state.duel.plDef.sName + " vs. " + state.duel.plOff.sName;

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

      return "<img src=\"/Content/Icons/" + sImg + ".png\" alt=\"Karte\" style=\"position: absolute; top: " + ((plDef.ptPos.Y + game.ptPitch.Y) / (float)(2 * game.ptPitch.Y)).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; width: 12px; left: " + (plDef.ptPos.X / (float)game.ptPitch.X).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; z-index: 99\" title=\"" + sDuelDesc + "\" />";
    }

    public Models.ViewGameModel.gameData getAllGameData(Models.ViewGameModel view, int iState = -1)
    {
      gD = new Models.ViewGameModel.gameData();

      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      CornerkickManager.User user = ckUser();
      CornerkickManager.Club club = ckClub();

      if (user.game == null) return gD;

      if (view.game == null) view.game = user.game;

      // initialize chart values
      gD.ltF = new List<Models.DataPointGeneral>[user.game.data.nPlStart];
      for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<Models.DataPointGeneral>();
      gD.ltM = new List<Models.DataPointGeneral>[user.game.data.nPlStart];
      for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();

      gD.iShoots       = new int[2];
      gD.iShootsOnGoal = new int[2];
      gD.iPassesGood   = new int[2];
      gD.iPassesBad    = new int[2];
      gD.iDuels        = new int[2];
      gD.iFouls        = new int[2];

      CornerkickGame.Game.Data gameData = user.game.data;

      for (int iSt = 0; iSt < gameData.ltState.Count; iSt++) {
        CornerkickGame.Game.State state = gameData.ltState[iSt];
        addGameData(ref gD, gameData, user, club, iSt, state.tsMinute.Seconds == 0 && state.bNewRound);

        if (iState == iSt) break;
      }

      return gD;
    }

    float fHeatmapMax = 0f;
    private string getDivHeatmap(byte iHA = 0, int iStateMax = 0, int iPlayer = -1)
    {
      CornerkickManager.User user = ckUser();

      if (user.game == null) return "";

      float[][] fHeatmap = MvcApplication.ckcore.ui.getHeatmap(user.game, iHA, ref fHeatmapMax, iStateMax, iPlayer);

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

      userAdmin.game.player[iHA][iPl].ptPos = new System.Drawing.Point(iX, iY);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

  }
}