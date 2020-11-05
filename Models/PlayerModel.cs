using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class PlayerModel
  {
    public Controllers.MemberController.Tutorial tutorial { get; set; }

    public int iPlayer { get; set; }
    public int iPlayerIndTr { get; set; }

    public int iPlIdPrev { get; set; }
    public int iPlIdNext { get; set; }

    public bool bOwnPlayer { get; set; }
    public bool bJouth { get; set; }
    public bool bJouthBelow16 { get; set; }
    public bool bJouthWithContract { get; set; }
    public bool bNation { get; set; }
    public bool bCpuPlayerNotOnTransferlist { get; set; }

    public byte iPos { get; set; }
    public string[][] sSkillTable { get; set; }

    // Emblem
    public string sPortrait { get; set; }
    public string sEmblem { get; set; }

    public string sColorJersey1 { get; set; }
    public string sColorJersey2 { get; set; }
    public string sColorJerseyNb { get; set; }

    // Contract
    [Display(Name = "zusätzl. Laufzeit [a]:")]
    public int iContractYears { get; set; }
    [Display(Name = "Gebotenes Gehalt:")]
    public int iContractSalaryOffer { get; set; }

    public string sName { get; set; }
    public float fTalentAve { get; set; }

    [Display(Name = "Neue Rückennr.:")]
    public int iNo { get; set; }
    public List<SelectListItem> ltDdlNo { get; set; }

    public bool bCaptain  { get; set; }
    public bool bCaptain2 { get; set; }

    public int iDp { get; set; }
    public List<SelectListItem> ddlDoping { get; set; }

    public bool bEditable { get; set; }
    public bool bSeasonStart { get; set; }

    public class DatatableEntryClubHistory
    {
      public int    iIx { get; set; }
      public string sPlayerName { get; set; }
      public string sClubTakeName { get; set; }
      public string sClubGiveName { get; set; }
      public string sDt { get; set; }
      public int iValue { get; set; }
      public int iTransferFee { get; set; }
    }

    public class DatatableEntryInjuryHistory
    {
      public int iIx { get; set; }
      public string sDt { get; set; }
      public string sInjuryName { get; set; }
      public int iInjuryLength { get; set; }
    }
  }
}