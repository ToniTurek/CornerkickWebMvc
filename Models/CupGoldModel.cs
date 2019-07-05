using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class CupGoldModel
  {
    public List<SelectListItem> ddlMatchday { get; set; }
    public int iMatchday { get; set; }

    public CupGoldModel()
    {
      ddlMatchday = new List<SelectListItem>();
      for (int iMd = 0; iMd < 6; iMd++) {
        ddlMatchday.Add(new SelectListItem { Text = (iMd + 1).ToString(), Value = iMd.ToString() });
      }
    }
  }
}