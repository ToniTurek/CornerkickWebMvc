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
    public List<CornerkickCore.UI.Scorer> ltScorer { get; set; }

    public List<SelectListItem> ltDdlSpTg { get; set; }

    public CupModel()
    {
      ltDdlSpTg = new List<SelectListItem>();

      CornerkickCore.Core.Cup cup = MvcApplication.ckcore.tl.getCup(iLand, 2);

      // Spieltage zu Dropdown Menü hinzufügen
      if (cup != null) {
        if (cup.ltMatchdays[0].ltGameData != null) {
          int nRound = MvcApplication.ckcore.tl.getPokalRundeVonNTeiln(cup.ltMatchdays[0].ltGameData.Count * 2);
          while (ltDdlSpTg.Count < nRound) {
            int iSpTg = ltDdlSpTg.Count + 1;
            ltDdlSpTg.Add(new SelectListItem { Text = MvcApplication.ckcore.sPokalrunde[nRound - iSpTg + 1], Value = iSpTg.ToString() });
          }
        }
      }

    }
  }
}