using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class PersonalModel
  {
    public string sPersonal { get; set; }
    public List<SelectListItem> ltDdlPersonal { get; set; }

    public PersonalModel()
    {
      ltDdlPersonal = new List<SelectListItem>();

      for (byte i = 0; i < 8; i++) {
        ltDdlPersonal.Add(
          new SelectListItem {
            Text = "Level: " + (i + 1).ToString(),
            Value = i.ToString()
          }
        );
      }
    }
  }
}