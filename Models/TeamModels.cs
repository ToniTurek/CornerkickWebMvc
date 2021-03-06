﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class TeamModels
  {
    public CornerkickManager.Club club { get; set; }

    public bool bAdmin { get; set; }
    public Controllers.MemberController.Tutorial tutorial { get; set; }

    public static List<CalendarModels> ltCalendar = new List<CalendarModels>();

    public static List<CornerkickGame.Player> ltPlayer { get; set; }
    public int iPlayerDetails { get; set; }
    public int iPlayerIndTr { get; set; }
    public string sPlayerIndTr { get; set; }
    public bool bPlayerIndTr { get; set; }
    public static List<string[]> ltsSubstitution { get; set; }
    public static List<int   []> ltiSubstitution { get; set; }
    public static byte iSubRest { get; set; }
    public bool bGame { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Formation")]
    public string sFormation { get; set; }
    public List<SelectListItem> ltsFormations { get; set; }
    public List<SelectListItem> ltsFormationsOwn { get; set; }

    [DataType(DataType.Text)]
    [Display(Name = "Typ")]
    public string sAutoFormationType { get; set; }
    public List<SelectListItem> ltDdlAutoFormationType { get; set; }

    public TeamData tD;

    public class Player
    {
      public int iId { get; set; }
      public byte iPos { get; set; }
      public string sName { get; set; }
      public string sSkillAve { get; set; }
      public string sTeamname { get; set; }
      public string sAge { get; set; }
      public string sNat { get; set; }
      public string sPortrait { get; set; }
      public bool bSusp { get; set; }
      public bool bYellowCard { get; set; }
      public sbyte iIxManMarking { get; set; }
      public bool bOffStandards { get; set; }
      public byte iNb { get; set; }
    }

    public struct TeamData
    {
      // Player details
      public List<Player> ltPlayer2 { get; set; }
      public CornerkickGame.Tactic.Formation formation { get; set; }
      public string sTeamAveSkill { get; set; }
      public string sTeamAveAge { get; set; }
      public string sEmblem { get; set; }

      // Opponent player details
      public byte iKibitzer { get; set; }
      public bool bOppTeam { get; set; } // Opponent team exist
      public List<Player> ltPlayerOpp2 { get; set; }
      public CornerkickGame.Tactic.Formation formationOpp { get; set; }
      public string sTeamOppAveSkill { get; set; }
      public string sTeamOppAveAge { get; set; }
      public string sEmblemOpp { get; set; }

      public CornerkickGame.Player plSelected { get; set; }
      public byte iCaptainIx { get; set; }
      public string sDivPlayer { get; set; }
      public string sDivRoa { get; set; }
      public float   fIndOrientation       { get; set; }
      public float[] fIndOrientationMinMax { get; set; }

      public float fTeamAveStrength { get; set; }
      public float fTeamAveAge { get; set; }

      public bool bNation { get; set; }
    }

    public TeamModels()
    {
      string sAutoFormType = "0";
      if (!string.IsNullOrEmpty(sAutoFormationType)) sAutoFormType = sAutoFormationType;

      ltDdlAutoFormationType = new List<SelectListItem>();
      ltDdlAutoFormationType.Add(new SelectListItem { Text = "max. Stärke",    Value =  "0", Selected = sAutoFormType.Equals( "0") });
      ltDdlAutoFormationType.Add(new SelectListItem { Text = "max. Kondition", Value = "+1", Selected = sAutoFormType.Equals("+1") });
      ltDdlAutoFormationType.Add(new SelectListItem { Text = "min. Kondition", Value = "-1", Selected = sAutoFormType.Equals("-1") });
      ltDdlAutoFormationType.Add(new SelectListItem { Text = "max. Frische",   Value = "+2", Selected = sAutoFormType.Equals("+2") });
      ltDdlAutoFormationType.Add(new SelectListItem { Text = "min. Frische",   Value = "-2", Selected = sAutoFormType.Equals("-2") });
      ltDdlAutoFormationType.Add(new SelectListItem { Text = "max. Erfahrung", Value = "+3", Selected = sAutoFormType.Equals("+3") });
      ltDdlAutoFormationType.Add(new SelectListItem { Text = "min. Erfahrung", Value = "-3", Selected = sAutoFormType.Equals("-3") });
      ltDdlAutoFormationType.Add(new SelectListItem { Text = "max. Alter",     Value = "+4", Selected = sAutoFormType.Equals("+4") });
      ltDdlAutoFormationType.Add(new SelectListItem { Text = "min. Alter",     Value = "-4", Selected = sAutoFormType.Equals("-4") });
    }
  }
}