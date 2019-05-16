using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class ViewGameModel
  {
    public bool bAdmin { get; set; }

    public int iStateGlobal { get; set; }

    public CornerkickGame.Game game { get; set; }

    public List<SelectListItem> ddlGames { get; set; }
    public string sSelectedGame { get; set; }

    public static List<float[]> ltLoc { get; set; }
    //public string sKommentar { get; set; }

    public int iPositions { get; set; }
    public List<SelectListItem> ddlPositions { get; set; }
    public int iHeatmap   { get; set; }
    public List<SelectListItem> ddlHeatmap   { get; set; }
    public int iShoots    { get; set; }
    public List<SelectListItem> ddlShoots    { get; set; }
    public int iDuels     { get; set; }
    public List<SelectListItem> ddlDuels     { get; set; }
    public int iPasses    { get; set; }
    public List<SelectListItem> ddlPasses    { get; set; }

    //public List<CornerkickGame.Game.Shoot>[] ltShoots { get; set; }

    public string sEmblemH { get; set; }
    public string sEmblemA { get; set; }

    public int iGameSpeed { get; set; }

    public struct gameLoc
    {
      public bool bFinished { get; set; }
      public int iInterval { get; set; }
      public List<gamePlayer> ltPlayer { get; set; }
      public gameBall gBall { get; set; }
    }

    public struct gamePlayer
    {
      public System.Drawing.Point ptPos       { get; set; }
      public System.Drawing.Point ptPosTarget { get; set; }
      public byte iNo     { get; set; }
      public byte iLookAt { get; set; }
      public byte iCard   { get; set; }
    }

    public struct gameBall
    {
      public CornerkickGame.Game.PointBall Pos { get; set; }
      public System.Drawing.Point ptPosTarget { get; set; }
      public byte iPassType { get; set; } // 0 - no pass, 1 - low pass, 2 - high pass
    }

    public struct gameData
    {
      public TimeSpan tsMinute { get; set; }

      public int iGoalsH { get; set; }
      public int iGoalsA { get; set; }
      public int[] iShoots { get; set; }
      public int[] iShootsOnGoal { get; set; }
      public int[] iPassesGood { get; set; }
      public int[] iPassesBad  { get; set; }
      public int[] iDuels { get; set; }
      public int[] iFouls { get; set; }

      public int iState  { get; set; }
      public int nStates { get; set; }
      public CornerkickGame.Game.Shoot[][] ltShoots { get; set; }
      public CornerkickGame.Game.Duel [][] ltDuels  { get; set; }

      // Bar statistics
      public float[][] fDataH { get; set; }
      public float[][] fDataA { get; set; }

      public List<drawLine> ltDrawLineShoot { get; set; }
      public List<drawLine> ltDrawLinePass { get; set; }
      public string sCard { get; set; }

      public string sTimelineIcons { get; set; }
      public string sStatGoals { get; set; }
      public string sStatCards { get; set; }
      public string sStatSubs  { get; set; }

      public string sRefereeQuality { get; set; }
      public string sRefereeDecisions { get; set; }

      public string sDivHeatmap { get; set; }

      public string sComment { get; set; }

      public string sAdminChanceShootOnGoal { get; set; }
      public string sAdminChanceGoal { get; set; }

      public List<Models.DataPointGeneral>[] ltF { get; set; }
      public List<Models.DataPointGeneral>[] ltM { get; set; }
    }
    public gameData gD;

    public struct drawLine
    {
      public int X0 { get; set; }
      public int Y0 { get; set; }
      public int X1 { get; set; }
      public int Y1 { get; set; }
      public string sColor { get; set; }
    }

    public ViewGameModel()
    {
      ddlHeatmap = new List<SelectListItem>();
      ddlHeatmap.Add(new SelectListItem { Text = "aus",      Value = "-1" });
      ddlHeatmap.Add(new SelectListItem { Text = "Heim",     Value =  "0" });
      ddlHeatmap.Add(new SelectListItem { Text = "Auswärts", Value =  "1" });
      iHeatmap = -1;
      iShoots  = -1;
      iDuels   = -1;
      iPasses  = -1;

      ddlPositions = new List<SelectListItem>();
      ddlPositions.Add(new SelectListItem { Text = "aus",         Value = "-1" });
      ddlPositions.Add(new SelectListItem { Text = "tatsächlich", Value =  "0" });
      ddlPositions.Add(new SelectListItem { Text = "gemittelt",   Value =  "1" });
      iPositions = 0;
    }
  }
}