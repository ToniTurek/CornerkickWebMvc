﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class StadiumSurroundingsModel
  {
    public List<SelectListItem> ddlTrainingsgel { get; set; }
    public int iTrainingsgel { get; set; }

    public List<SelectListItem> ddlJouthInternat { get; set; }
    public int iJouthInternat { get; set; }

    public int iVereinsheim { get; set; }
    public int iVereinsmuseum { get; set; }
    public int iParkp { get; set; }
    public int iParkpNeu { get; set; }
    public int iTicketschalter { get; set; }

    public StadiumSurroundingsModel()
    {
      CornerkickCore.Core.Club clb = Controllers.AccountController.ckClub();

      ddlTrainingsgel = new List<SelectListItem>();
      for (int i = clb.iTrainingsgel  [1] + 1; i < MvcApplication.ckcore.sTrainingsgel.Length; i++) ddlTrainingsgel .Add(new SelectListItem { Text = MvcApplication.ckcore.sTrainingsgel[i], Value = i.ToString() });

      ddlJouthInternat = new List<SelectListItem>();
      for (int i = clb.iJugendinternat[1] + 1; i < MvcApplication.ckcore.sTrainingsgel.Length; i++) ddlJouthInternat.Add(new SelectListItem { Text = MvcApplication.ckcore.sJouthInternat[i], Value = i.ToString() });
    }
  }
}