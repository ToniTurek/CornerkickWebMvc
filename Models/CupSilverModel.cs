using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class CupSilverModel
  {
    public List<SelectListItem> ddlSeason { get; set; }
    public int iSeason { get; set; }

    public List<SelectListItem> ddlGroup { get; set; }
    public int iGroup { get; set; }

    public List<SelectListItem> ddlMatchday { get; set; }
    public int iMatchday { get; set; }

    public CupSilverModel()
    {
      ddlGroup = new List<SelectListItem>();
      for (byte iG = 0; iG < 8; iG++) ddlGroup.Add(new SelectListItem { Text = ((char)(65 + iG)).ToString(), Value = iG.ToString() });

      ddlMatchday = new List<SelectListItem>();
      for (int iMd = 0; iMd < 6; iMd++) {
        ddlMatchday.Add(new SelectListItem { Text = (iMd + 1).ToString(), Value = iMd.ToString() });
      }
    }
  }
}