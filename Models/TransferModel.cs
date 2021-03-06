﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class TransferModel
  {
    public bool bTransferlistOpen { get; set; }

    [Display(Name = "Laufzeit [a]:")]
    public int iContractYears { get; set; }
    [Display(Name = "Gebotenes Gehalt:")]
    public int iContractSalaryOffer { get; set; }

    [Display(Name = "Ablöse [mio. €]:")]
    [DisplayFormat(DataFormatString = "{0:0,0}")]
    public int iTransferFee { get; set; }
    public int iTransferFeeSecretBalance { get; set; }

    public int iOfferClubId { get; set; }

    // Filter
    public List<SelectListItem> ltDdlFilterPos { get; set; }
    [Display(Name = "Pos.: ")]
    public string sFilterPos { get; set; }

    public List<SelectListItem> ltDdlFilterFType { get; set; }
    public string sFilterFType { get; set; }
    public List<SelectListItem> ltDdlFilterF { get; set; }
    public string sFilterF { get; set; }

    public List<SelectListItem> ddlFilterLeague { get; set; }
    public string sFilterLeague { get; set; }

    public List<SelectListItem> ddlFilterNation { get; set; }
    public string sFilterNation { get; set; }

    public bool bNation { get; set; }

    public bool bSound { get; set; }

    public TransferModel()
    {
      ltDdlFilterPos = new List<SelectListItem>();
      ltDdlFilterPos.Add(new SelectListItem { Text = "alle", Value = "0" });

      // Positionen zu Dropdown Menü hinzufügen
      for (int iPos = 1; iPos < 12; iPos++) {
        ltDdlFilterPos.Add(new SelectListItem { Text = CornerkickManager.Main.sPosition[iPos], Value = iPos.ToString() });
      }

      ltDdlFilterFType = new List<SelectListItem>();
      ltDdlFilterFType.Add(new SelectListItem { Text = "-", Value = "-1" });
      // Positionen zu Dropdown Menü hinzufügen
      for (int iF = 0; iF < CornerkickManager.PlayerTool.sSkills.Length - 1; iF++) {
        ltDdlFilterFType.Add(new SelectListItem { Text = CornerkickManager.PlayerTool.sSkills[iF], Value = iF.ToString() });
      }

      ltDdlFilterF = new List<SelectListItem>();
      // Positionen zu Dropdown Menü hinzufügen
      for (int iF = 1; iF < 11; iF++) {
        ltDdlFilterF.Add(new SelectListItem { Text = iF.ToString(), Value = iF.ToString() });
      }

      ddlFilterLeague = new List<SelectListItem>();

      // Nations
      ddlFilterNation = new List<SelectListItem>();
    }
  }

  public class DatatableEntryTransfer
  {
    public int playerId { get; set; }
    public string empty { get; set; }
    public int    iOffer { get; set; }
    public string index { get; set; }
    public string datum { get; set; }
    public string name { get; set; }
    public string position { get; set; }
    public float strength { get; set; }
    public float strengthIdeal { get; set; }
    public float fTalentAve { get; set; }
    public string club { get; set; }
    public int mw { get; set; }
    public int fixtransferfee { get; set; }
    public string nat { get; set; }
    public string age { get; set; }
    public bool bEndingContract { get; set; }
  }

  public class DatatableEntryTransferDetails
  {
    public int i { get; set; }
    public int iPlayerId { get; set; }
    public int iClubId { get; set; }
    public string club { get; set; }
    public string fee { get; set; }
  }
}