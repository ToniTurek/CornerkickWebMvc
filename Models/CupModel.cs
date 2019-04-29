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
    public int iLand { get; set; }

    [Display(Name = "Spieltag: ")]
    public int iSpTg { get; set; }

    public List<List<CornerkickGame.Game.Data>> ltErg { get; set; }
    public List<CornerkickManager.UI.Scorer> ltScorer { get; set; }

    public List<SelectListItem> ddlLand { get; set; }
    public List<SelectListItem> ltDdlSpTg { get; set; }

    public CupModel()
    {
      ddlLand = new List<SelectListItem>();
      ltDdlSpTg = new List<SelectListItem>();
    }
  }
}