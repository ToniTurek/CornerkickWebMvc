﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace CornerkickWebMvc.Controllers
{
  public class ViewGameController : Controller
  {
    private static Models.ViewGameModel.gameData gD;

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
      gD = new Models.ViewGameModel.gameData();

      CornerkickCore.Core.User user = AccountController.ckUser();

      view.bAdmin = AccountController.checkUserIsAdmin(AccountController.appUser);

      if (view.bAdmin && user.game == null) {
        user.game = MvcApplication.ckcore.game.tl.getDefaultGame();
        user.game.iGameSpeed = 300;
        AccountController.setCkUser(user);
      }

      view.game = user.game;

      if (user.game != null) {
        string sEmblemDir = MvcApplication.getHomeDir() + "Content/Uploads/";
        view.sEmblemH = user.game.data.team[0].iTeamId.ToString() + ".png";
        view.sEmblemA = user.game.data.team[1].iTeamId.ToString() + ".png";
        if (!System.IO.File.Exists(sEmblemDir + view.sEmblemH)) view.sEmblemH = "0.png";
        if (!System.IO.File.Exists(sEmblemDir + view.sEmblemA)) view.sEmblemA = "0.png";

        // Add player to heatmap
        for (byte iHA = 0; iHA < 2; iHA++) {
          for (byte iPl = 0; iPl < user.game.nPlStart; iPl++) {
            view.ddlHeatmap.Add(new SelectListItem { Text = user.game.player[iHA][iPl].sName, Value = (2 + (iHA * user.game.nPlStart) + iPl).ToString() });
          }
        }
      }

      fHeatmapMax = 0f;

      gD = getGameData(view);

      view.gD = gD;

      return View(view);
    }

    public JsonResult ViewGameLocations(int iState = -1/*, int iSleep = 0*/, bool bAverage = false)
    {
      Models.ViewGameModel.ltLoc = new List<float[]>();
      Models.ViewGameModel.gameLoc gLoc = new Models.ViewGameModel.gameLoc();

      CornerkickCore.Core.User user = AccountController.ckUser();

      if (user.game == null) {
        Response.StatusCode = 1;
        return Json(Models.ViewGameModel.ltLoc, JsonRequestBehavior.AllowGet);
      }

      /*
      if (iSleep > 0) {
        System.Threading.Thread.Sleep(iSleep);
      }
      */

      /*
      if (user.game.data.bFinished) {
        Response.StatusCode = 1;
      }
      */

      CornerkickGame.Game.State state = user.game.newState();
      if (user.game.data.ltState.Count > 0) state = user.game.data.ltState[0];

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
        for (int iP = 0; iP < MvcApplication.ckcore.game.nPlStart; iP++) {
          Models.ViewGameModel.gamePlayer gPlayer = new Models.ViewGameModel.gamePlayer();

          CornerkickGame.Player pl;
          if (iState < 0) pl = user.game.player[iHA][iP];
          else            pl = state.player[iHA][iP];

          if (string.IsNullOrEmpty(pl.sName)) continue;

          if      (pl.bYellowCard)                         gPlayer.iCard = 1;
          else if (pl.iSperre[user.game.iIndexSperre] > 0) gPlayer.iCard = 2;

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
        }
      }

      // Ball target
      if (ball.iPassStep > 0 && ball.nPassSteps > 0) {
        if (ball.Pos.Z > 0.0) gBall.iPassType = 2;
        else                  gBall.iPassType = 1;
      }
      gBall.ptPosTarget = ball.ptPos;
      //Models.ViewGameModel.ltLoc.Add(new float[5] { ball.ptPos.X, ball.ptPos.Y, fPass, fFinished, 0f });

      gLoc.gBall = gBall;

      return Json(gLoc, JsonRequestBehavior.AllowGet);
    }

    public JsonResult ViewGameGetDataStatisticObject(Models.ViewGameModel view, int iState = -1, int iHeatmap = -1)
    {
      CornerkickCore.Core.User user = AccountController.ckUser();
      CornerkickCore.Core.Club club = AccountController.ckClub();

      CornerkickGame.Game.Data gameData = user.game.data;

      gD.ltF = new List<Models.DataPointGeneral>[user.game.nPlStart];
      for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<Models.DataPointGeneral>();

      gD.ltM = new List<Models.DataPointGeneral>[user.game.nPlStart];
      for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();

      if (gD.nStates == 0) {
        gD = getGameData(view);
      } else if (iState >= 0) {
        gD = getGameData(view, iState);
      } else if (iState <  0) {
        addGameData(ref gD, gameData, user, club, iState, iHeatmap);
      }

      setGameData(ref gD, gameData, user, club, iState, iHeatmap);

      return Json(gD, JsonRequestBehavior.AllowGet);
    }

    private void addGameData(ref Models.ViewGameModel.gameData gD, CornerkickGame.Game.Data gameData, CornerkickCore.Core.User user, CornerkickCore.Core.Club club, int iState = -1, int iHeatmap = -1, bool bReduced = false)
    {
      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (user.game == null) return;

      if (gameData.ltState.Count == 0) return;

      CornerkickGame.Game.State state = gameData.ltState[gameData.ltState.Count - 1];
      if (iState >= 0 && iState < gameData.ltState.Count) state = gameData.ltState[iState];

      //int iStComm = 0;
      //foreach (CornerkickGame.Game.State stateTmp in gameData.ltState) {
      string sCommTmp = "";
      foreach (CornerkickGame.Game.Kommentar k in Enumerable.Reverse(state.ltCommend)) {
        if (!string.IsNullOrEmpty(k.sKommentar)) sCommTmp += MvcApplication.ckcore.game.tl.sSpielmin(k.tsSpielminute, true) + k.sKommentar + '\n';
      }

      gD.sComment = sCommTmp + gD.sComment;

      //if (iState == iStComm) break;

      //iStComm++;
      //}

      float fLeft = ((state.tsMinute.Hours * 60f) + state.tsMinute.Minutes + (state.tsMinute.Seconds / 60f)) / 0.9f;
      if (gameData.bFinished) fLeft = (100.0f * state.i) / gameData.ltState.Count;

      for (byte iHA = 0; iHA < 2; iHA++) {
        int iIconTop = 2;
        if (iHA == 1) iIconTop = 44;

        string sTeam = gameData.team[0].sTeam;
        if (iHA > 0) sTeam = gameData.team[1].sTeam;

        //int iStTmp = 0;
        // loop over states
        //foreach (CornerkickGame.Game.State state in gameData.ltState) {
        string sOnClick = "";
        string sCursor = "";
        if (gameData.bFinished) {
          sOnClick = "onclick =\"javascript: drawGame(" + state.i.ToString() + ")\" ";
          sCursor = "; cursor: pointer";
        }

        // Shoots
        CornerkickGame.Game.Shoot shoot = state.shoot;
        if (shoot.plShoot != null && shoot.iHA == iHA) {
          float fDist = MvcApplication.ckcore.game.tl.getDistanceToGoal(shoot.plShoot)[0];

          string sShootDesc = MvcApplication.ckcore.game.tl.sSpielmin(shoot.tsMinute, false) +
                              shoot.iGoalsH.ToString() + ":" + shoot.iGoalsA.ToString();
          sShootDesc += " - " + shoot.plShoot.sName;
          if (shoot.iType == 5) sShootDesc += ", FE";
          else sShootDesc += ", Entf.:" + fDist.ToString("0.0").PadLeft(5) + "m";
          if (shoot.plAssist != null) sShootDesc += " (" + shoot.plAssist.sName + ")";

          string sImg = "yellow";
          if (shoot.iResult == 1) {
            sImg = "white";

            if (string.IsNullOrEmpty(gD.sStatGoals)) gD.sStatGoals = "<br/><u>Tore:</u>";
            gD.sStatGoals += "<br/>" + sShootDesc;
          } else if (shoot.iResult == 0 || shoot.iResult == 7) {
            sImg = "cyan";
          }

          gD.sTimelineIcons += "<img " + sOnClick + "src=\"/Content/Icons/ball_" + sImg + ".png\" alt=\"Torschuss\" style=\"position: absolute; top: " + iIconTop.ToString() + "px; width: 12px; left: " + (fLeft - 0.5).ToString(nfi) + "%" + sCursor + "\" title=\"" + sShootDesc + "\"/>";

          // Count shoots
          gD.iShoots[iHA]++;
          if (shoot.iResult > 0 && shoot.iResult < 7) gD.iShootsOnGoal[iHA]++;
        } // shoot

        // Cards
        CornerkickGame.Game.Card card = state.card;
        if (card.plCard != null && card.iHA == iHA) {
          string sCardDesc = MvcApplication.ckcore.game.tl.sSpielmin(card.tsMinute, false) +
                              sTeam +
                              " - " +
                              card.plCard.sName;

          string sImg = "Y";
          if (card.iType == 1) sImg = "YR";
          else if (card.iType == 2) sImg = "R";

          gD.sTimelineIcons += "<img " + sOnClick + "src=\"/Content/Icons/" + sImg + "Card.bmp\" alt=\"Karte\" style=\"position: absolute; top: " + iIconTop.ToString() + "px; width: 12px; left: " + (fLeft - 0.5).ToString(nfi) + "%" + sCursor + "\" title=\"" + sCardDesc + "\"/>";

          if (string.IsNullOrEmpty(gD.sStatCards)) gD.sStatCards = "<br/><u>Karten:</u>";
          gD.sStatCards += "<br/><img " + sOnClick + "style=\"position: relative" + sCursor + "\" src ='/Content/Icons/" + sImg + "Card.bmp'/>" + sCardDesc;
        }

        //iStTmp++;
        //} // foreach state
      } // iHA

      byte jHA = 0;
      if (user.game.data.team[1].iTeamId == club.iId) jHA = 1;

      // Chart
      if (!bReduced) {
        if (gD.ltF[0].Count < state.i) {
          for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) {
            gD.ltF[iPl].Add(new Models.DataPointGeneral(state.i, state.player[jHA][iPl].fFrische));
          }
        }
        if (gD.ltM[0].Count < state.i) {
          for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) {
            gD.ltM[iPl].Add(new Models.DataPointGeneral(state.i, state.player[jHA][iPl].fMoral));
          }
        }
      }
    }

    private void setGameData(ref Models.ViewGameModel.gameData gD, CornerkickGame.Game.Data gameData, CornerkickCore.Core.User user, CornerkickCore.Core.Club club, int iState = -1, int iHeatmap = -1)
    {
      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      if (user.game == null) return;

      if (gameData.ltState.Count == 0) return;

      CornerkickGame.Game.State state = gameData.ltState[gameData.ltState.Count - 1];
      if (iState >= 0 && iState < gameData.ltState.Count) state = gameData.ltState[iState];

      gD.nStates = gameData.ltState.Count;

      gD.tsMinute = state.tsMinute;

      gD.ltDrawLineShoot = new List<Models.ViewGameModel.drawLine>();
      gD.sCard = "";

      // Draw shoot line / card on pitch
      if (iState >= 0 && iState < gameData.ltState.Count) {
        // set shoot line
        if (state.shoot.plShoot != null) {
          CornerkickGame.Player plShoot = state.shoot.plShoot;

          Models.ViewGameModel.drawLine drawLine = new Models.ViewGameModel.drawLine();
          drawLine.X0 = state.shoot.plShoot.ptPos.X;
          drawLine.Y0 = state.shoot.plShoot.ptPos.Y;
          if (state.shoot.iResult == 3) { // keeper
            CornerkickGame.Player plKeeper = user.game.tl.getKeeper(state.shoot.iHA == 1);

            drawLine.X1 = plKeeper.ptPos.X;
            drawLine.Y1 = plKeeper.ptPos.Y;
            drawLine.sColor = "yellow";

            gD.ltDrawLineShoot.Add(drawLine);

            drawLine = new Models.ViewGameModel.drawLine();
            drawLine.X0 = gD.ltDrawLineShoot[0].X1;
            drawLine.Y0 = gD.ltDrawLineShoot[0].Y1;
          } else if (state.shoot.iResult == 5) { // post
            drawLine.X1 = (1 - state.shoot.iHA) * user.game.ptPitch.X;
            drawLine.Y1 = -2;
            drawLine.sColor = "yellow";

            gD.ltDrawLineShoot.Add(drawLine);

            drawLine = new Models.ViewGameModel.drawLine();
            drawLine.X0 = gD.ltDrawLineShoot[0].X1;
            drawLine.Y0 = gD.ltDrawLineShoot[0].Y1;
          } else if (state.shoot.iResult == 6) { // bar
            drawLine.X1 = (1 - state.shoot.iHA) * user.game.ptPitch.X;
            drawLine.Y1 = 0;
            drawLine.sColor = "yellow";

            gD.ltDrawLineShoot.Add(drawLine);

            drawLine = new Models.ViewGameModel.drawLine();
            drawLine.X0 = gD.ltDrawLineShoot[0].X1;
            drawLine.Y0 = gD.ltDrawLineShoot[0].Y1;
          }
          drawLine.X1 = state.ball.ptPos.X;
          drawLine.Y1 = state.ball.ptPos.Y;
          if      (state.shoot.iResult == 1)                             drawLine.sColor = "red";
          else if (state.shoot.iResult == 0 || state.shoot.iResult == 7) drawLine.sColor = "cyan";
          else                                                           drawLine.sColor = "yellow";

          gD.ltDrawLineShoot.Add(drawLine);
        }

        if (state.card.plCard != null) {
          string sCardDesc = MvcApplication.ckcore.game.tl.sSpielmin(state.card.tsMinute, false) +
                             " - " +
                             state.card.plCard.sName;

          string sImg = "Y";
          if      (state.card.iType == 1) sImg = "YR";
          else if (state.card.iType == 2) sImg = "R";

          gD.sCard = "<img src=\"/Content/Icons/" + sImg + "Card.bmp\" alt=\"Karte\" style=\"position: absolute; top: " + ((state.card.plCard.ptPos.Y + user.game.ptPitch.Y) / (float)(2 * user.game.ptPitch.Y)).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "; width: 12px; left: " + (state.card.plCard.ptPos.X / (float)user.game.ptPitch.X).ToString("0.00%", System.Globalization.CultureInfo.InvariantCulture) + "\" title=\"" + sCardDesc + "\"/>";
        }
      }

      // Shoots
      int[] nPossession   = new int[2];
      int[] nDuel         = new int[2];
      int[] nCornerkick   = new int[2];
      int[] nOffsite      = new int[2];

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
          nDuel      [0] = state.iDuelH;
          nCornerkick[0] = state.iCornerkickH;
          nOffsite   [0] = state.iOffsiteH;
        } else {
          gD.iGoalsA = state.iGoalsA;
          nPossession[1] = state.iPossessionA;
          nDuel      [1] = state.iDuelA;
          nCornerkick[1] = state.iCornerkickA;
          nOffsite   [1] = state.iOffsiteA;
        }

        //iStTmp++;
        //} // foreach state

        // Substitutions
        gD.sStatSubs = "";
        if (gameData.team[iHA].ltSubstitutions != null) {
          foreach (int[] iSub in gameData.team[iHA].ltSubstitutions) {
            float fMin = iSub[2];

            string sSubDesc = iSub[2].ToString() + ". Min.: " + sTeam + " - " + MvcApplication.ckcore.ltPlayer[iSub[1]].sName + " für " + MvcApplication.ckcore.ltPlayer[iSub[0]].sName;

            if (string.IsNullOrEmpty(gD.sStatSubs)) gD.sStatSubs = "<br/><u>Spielerwechsel:</u>";
            gD.sStatSubs += "<br/>" + sSubDesc;

            gD.sTimelineIcons += "<img src=\"/Content/Icons/sub.png\" alt=\"Spielerwechsel\" style=\"position: absolute; top: " + iIconTop.ToString(nfi) + "px; width: 12px; left: " + ((fMin - 0.5) / 0.9).ToString(nfi) + "%\" title=\"" + sSubDesc + "\"/>";
          }
        }
      } // iHA

      //////////////////////////////////////////////////////////////////////////////////
      // Bar statistics
      //////////////////////////////////////////////////////////////////////////////////
      // Ballbesitz
      float fPossessionH = 50f;
      if (nPossession[0] + nPossession[1] > 0) fPossessionH = 100f * nPossession[0] / (float)(nPossession[0] + nPossession[1]);

      // Zweikämpfe
      float fDuelH = 50f;
      if (nDuel[0] + nDuel[1] > 0) fDuelH = 100 * nDuel[0] / (float)(nDuel[0] + nDuel[1]);

      gD.fDataH = new float[][] { new float[2] { (float)gD.iGoalsH, 0f }, new float[2] { (float)gD.iShoots[0], -1f }, new float[2] { (float)gD.iShootsOnGoal[0], -2f }, new float[2] {        fPossessionH, -3f }, new float[2] {        fDuelH, -4f }, new float[2] { (float)nCornerkick[0], -5f }, new float[2] { (float)nOffsite[0], -6f } };
      gD.fDataA = new float[][] { new float[2] { (float)gD.iGoalsA, 0f }, new float[2] { (float)gD.iShoots[1], -1f }, new float[2] { (float)gD.iShootsOnGoal[1], -2f }, new float[2] { 100f - fPossessionH, -3f }, new float[2] { 100f - fDuelH, -4f }, new float[2] { (float)nCornerkick[1], -5f }, new float[2] { (float)nOffsite[1], -6f } };

      if (AccountController.checkUserIsAdmin(AccountController.appUser)) {
        if (user.game.ball.plAtBall != null) {
          float fShootOnGoal = user.game.ai.getChanceShootOnGoal(user.game.ball.plAtBall);
          float fKeeperSave  = user.game.ai.getChanceKeeperSave (user.game.ball.plAtBall);

          gD.sAdminChanceShootOnGoal = "<br/><u>Change Schuss aufs Tor:</u> " + fShootOnGoal.ToString("0.0%");
          gD.sAdminChanceGoal = "<br/><u>Change Tor:</u> " + (fShootOnGoal * (1f - fKeeperSave)).ToString("0.0%");
        }
      }

      // Heatmap
      gD.sDivHeatmap = "";
      if (iHeatmap >= 0) {
        byte iHA =  0;
        int  iPl = -1;
        if (iHeatmap < 2) {
          iHA = (byte)iHeatmap;
        } else {
          iHA = (byte)((iHeatmap - 2) / user.game.nPlStart);
          iPl = (iHeatmap - 2) - (iHA * user.game.nPlStart);
        }
        gD.sDivHeatmap = getDivHeatmap(iHA, iState, iPl);
      }
    }

    public Models.ViewGameModel.gameData getGameData(Models.ViewGameModel view, int iState = -1, int iHeatmap = -1)
    {
      gD = new Models.ViewGameModel.gameData();

      NumberFormatInfo nfi = new NumberFormatInfo();
      nfi.NumberDecimalSeparator = ".";

      CornerkickCore.Core.User user = AccountController.ckUser();
      CornerkickCore.Core.Club club = AccountController.ckClub();

      if (user.game == null) return gD;

      if (view.game == null) view.game = user.game;

      gD.sComment = "";

      // initialize chart values
      gD.ltF = new List<Models.DataPointGeneral>[user.game.nPlStart];
      for (byte iPl = 0; iPl < gD.ltF.Length; iPl++) gD.ltF[iPl] = new List<Models.DataPointGeneral>();
      gD.ltM = new List<Models.DataPointGeneral>[user.game.nPlStart];
      for (byte iPl = 0; iPl < gD.ltM.Length; iPl++) gD.ltM[iPl] = new List<Models.DataPointGeneral>();

      gD.iShoots       = new int[2];
      gD.iShootsOnGoal = new int[2];

      CornerkickGame.Game.Data gameData = user.game.data;

      int iStTmp = 0;
      foreach (CornerkickGame.Game.State state in gameData.ltState) {
        addGameData(ref gD, gameData, user, club, iStTmp, iHeatmap, true);

        if (iState == iStTmp) break;

        iStTmp++;
      }

      return gD;
    }

    float fHeatmapMax = 0f;
    private string getDivHeatmap(byte iHA = 0, int iStateMax = 0, int iPlayer = -1)
    {
      CornerkickCore.Core.User user = AccountController.ckUser();

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
      CornerkickCore.Core.User userAdmin = AccountController.ckUser();

      if (userAdmin.game == null) {
        userAdmin.game = MvcApplication.ckcore.game.tl.getDefaultGame();
      }

      userAdmin.game.iGameSpeed = 300;

      userAdmin.game.start();

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AdminStop(Models.ViewGameModel view)
    {
      CornerkickCore.Core.User userAdmin = AccountController.ckUser();

      if (userAdmin.game == null) Json(false, JsonRequestBehavior.AllowGet);

      userAdmin.game.stop();

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AdminNext(Models.ViewGameModel view)
    {
      CornerkickCore.Core.User userAdmin = AccountController.ckUser();

      if (userAdmin.game == null) {
        userAdmin.game = MvcApplication.ckcore.game.tl.getDefaultGame();
      }

      userAdmin.game.iGameSpeed =  1;

      userAdmin.game.next();

      return Json(true, JsonRequestBehavior.AllowGet);
    }

    public JsonResult AdminNew(Models.ViewGameModel view)
    {
      CornerkickCore.Core.User userAdmin = AccountController.ckUser();

      userAdmin.game = MvcApplication.ckcore.game.tl.getDefaultGame();

      userAdmin.game.iGameSpeed = 300;

      AccountController.setCkUser(userAdmin);

      return Json(true, JsonRequestBehavior.AllowGet);
    }

  }
}