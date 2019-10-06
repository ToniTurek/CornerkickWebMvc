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
    public int iPlayer { get; set; }
    public int iPlayerIndTr { get; set; }

    public int iPlIdPrev { get; set; }
    public int iPlIdNext { get; set; }

    public bool bOwnPlayer { get; set; }
    public bool bJouth { get; set; }
    public bool bJouthBelow16 { get; set; }
    public bool bNation { get; set; }

    // Emblem
    public string sEmblem { get; set; }

    public string sColorJersey { get; set; }
    public string sColorJerseyNo { get; set; }

    // Contract
    [Display(Name = "zusätzl. Laufzeit [a]:")]
    public int iContractYears { get; set; }
    [Display(Name = "Gebotenes Gehalt:")]
    public int iContractSalaryOffer { get; set; }

    public string sName { get; set; }

    [Display(Name = "Neue Rückennr.:")]
    public int iNo { get; set; }
    public List<SelectListItem> ltDdlNo { get; set; }

    public bool bCaptain  { get; set; }
    public bool bCaptain2 { get; set; }

    public int iDp { get; set; }
    public List<SelectListItem> ddlDoping { get; set; }
  }
}