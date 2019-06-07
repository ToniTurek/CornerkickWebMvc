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
    public CornerkickManager.Main.Personal personal { get; set; }

    public List<SelectListItem> ltDdlPersonal { get; set; }
    public string sPersonal { get; set; }

    public List<SelectListItem> ltDdlPersonalKibitzer { get; set; }
    public string sPersonalKibitzer { get; set; }

    public PersonalModel()
    {
      ltDdlPersonal = new List<SelectListItem>();
      ltDdlPersonalKibitzer = new List<SelectListItem>();

      ltDdlPersonal.Add(new SelectListItem { Text = "-", Value = "0" });
      for (byte i = 1; i < 8; i++) {
        ltDdlPersonal.Add(
          new SelectListItem {
            Text = "Level: " + i.ToString(),
            Value = i.ToString()
          }
        );
      }

      ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "-",        Value = "0" });
      ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 1", Value = "1" });
      ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 2", Value = "2" });
      ltDdlPersonalKibitzer.Add(new SelectListItem { Text = "Level: 3", Value = "3" });
    }
  }
}