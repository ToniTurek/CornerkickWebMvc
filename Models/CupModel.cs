using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class CupModel
  {
    public int iClubId { get; set; }

    public int iLand { get; set; }

    public List<List<CornerkickGame.Game.Data>> ltErg { get; set; }

    public List<SelectListItem> ddlLand { get; set; }

    public CupModel()
    {
      ddlLand = new List<SelectListItem>();
      if (MvcApplication.iNations.Length > 0) iLand = MvcApplication.iNations[0];
    }
  }
}