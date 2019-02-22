﻿using System;
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

    // Contract
    [Display(Name = "zusätzl. Laufzeit [a]:")]
    public int iContractYears { get; set; }

    public string sName { get; set; }

    [Display(Name = "Neue Rückennr.:")]
    public int iNo { get; set; }

    public List<SelectListItem> ltDdlNo { get; set; }
  }
}