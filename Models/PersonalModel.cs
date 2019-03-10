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

      ltDdlPersonal.Add(new SelectListItem { Text = "-", Value = "0" });
      for (byte i = 1; i < 8; i++) {
        ltDdlPersonal.Add(
          new SelectListItem {
            Text = "Level: " + i.ToString(),
            Value = i.ToString()
          }
        );
      }
    }
  }
}