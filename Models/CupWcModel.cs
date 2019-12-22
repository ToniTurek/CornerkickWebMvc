using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class CupWcModel
  {
    public List<SelectListItem> ddlSeason { get; set; }
    public int iSeason { get; set; }

    public List<SelectListItem> ddlGroup { get; set; }
    public int iGroup { get; set; }

    public List<SelectListItem> ddlMatchday { get; set; }
    public int iMatchday { get; set; }

    public CupWcModel()
    {
      ddlGroup = new List<SelectListItem>();
      ddlGroup.Add(new SelectListItem { Text = "A", Value = "0" });
      ddlGroup.Add(new SelectListItem { Text = "B", Value = "1" });

      ddlMatchday = new List<SelectListItem>();
      for (int iMd = 0; iMd < 5; iMd++) {
        ddlMatchday.Add(new SelectListItem { Text = (iMd + 1).ToString(), Value = iMd.ToString() });
      }
    }
  }
}