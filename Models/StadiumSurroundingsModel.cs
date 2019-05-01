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
    public int iCarpark { get; set; }
    public int iCarparkNew { get; set; }
    public int iCounter { get; set; }
    public int iCounterNew { get; set; }
  }
}