using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class TrainingModel
  {
    public string sTraining { get; set; }
    public List<SelectListItem> ltDdlTraining { get; set; }

    public TrainingModel()
    {
      ltDdlTraining = new List<SelectListItem>();

      for (byte i = 0; i < MvcApplication.ckcore.sTraining.Length; i++) {
        ltDdlTraining.Add(
          new SelectListItem {
            Text = MvcApplication.ckcore.sTraining[i],
            Value = i.ToString()
          }
        );
      }
    }
  }
}