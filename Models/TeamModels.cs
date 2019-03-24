using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class TeamModels
  {
    public bool bAdmin { get; set; }

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

    public struct TeamData
    {
      // Player details
      public List<CornerkickGame.Player> ltPlayer { get; set; }
      public List<byte> ltPlayerPos { get; set; }
      public List<string> ltPlayerAveSkill { get; set; }

      // Opponent player details
      public List<CornerkickGame.Player> ltPlayerOpp { get; set; }
      public List<byte> ltPlayerOppPos { get; set; }
      public List<string> ltPlayerOppAveSkill { get; set; }

      public CornerkickGame.Player plSelected { get; set; }
      public string sDivPlayer { get; set; }
      public string sDivRoa { get; set; }
      public float   fIndOrientation       { get; set; }
      public float[] fIndOrientationMinMax { get; set; }
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