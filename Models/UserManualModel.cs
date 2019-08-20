using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class UserManualModel
  {
    public string sTraining { get; set; }
    public List<SelectListItem> ddlTraining { get; set; }

    public UserManualModel()
    {
      ddlTraining = new List<SelectListItem>();
      for (byte i = 0; i < MvcApplication.ckcore.sTraining.Length; i++) {
        ddlTraining.Add(
          new SelectListItem {
            Text = MvcApplication.ckcore.sTraining[i],
            Value = i.ToString()
          }
        );
      }
    }
  }

}