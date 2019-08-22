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

    public string sPlayerTrainingCoachCondi { get; set; }
    public List<SelectListItem> ddlPlayerTrainingCoachCondi { get; set; }

    public string sPlayerTrainingCoachPhysio { get; set; }
    public List<SelectListItem> ddlPlayerTrainingCoachPhysio { get; set; }

    public string sPlayerTrainingCamp { get; set; }
    public List<SelectListItem> ddlPlayerTrainingCamp { get; set; }

    public string sPlayerTrainingDoping { get; set; }
    public List<SelectListItem> ddlPlayerTrainingDoping { get; set; }

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

      ddlPlayerTrainingCoachCondi = new List<SelectListItem>();
      for (byte i = 7; i > 0; i--) ddlPlayerTrainingCoachCondi.Add(new SelectListItem { Text = "Level: " + i.ToString(), Value = i.ToString() });
      ddlPlayerTrainingCoachCondi.Add(new SelectListItem { Text = "-", Value = "0" });

      ddlPlayerTrainingCoachPhysio = new List<SelectListItem>();
      for (byte i = 7; i > 0; i--) ddlPlayerTrainingCoachPhysio.Add(new SelectListItem { Text = "Level: " + i.ToString(), Value = i.ToString() });
      ddlPlayerTrainingCoachPhysio.Add(new SelectListItem { Text = "-", Value = "0" });

      // Trainings camp
      ddlPlayerTrainingCamp = new List<SelectListItem>();
      ddlPlayerTrainingCamp.Add(new SelectListItem { Text = "-", Value = "-1" });
      for (byte i = 0; i < MvcApplication.ckcore.tcp.ltCamps.Count; i++) ddlPlayerTrainingCamp.Add(new SelectListItem { Text = MvcApplication.ckcore.tcp.ltCamps[i].sName, Value = i.ToString() });

      // Doping
      ddlPlayerTrainingDoping = new List<SelectListItem>();
      ddlPlayerTrainingDoping.Add(new SelectListItem { Text = "-", Value = "-1" });
      for (byte i = 0; i < MvcApplication.ckcore.ltDoping.Count; i++) ddlPlayerTrainingDoping.Add(new SelectListItem { Text = MvcApplication.ckcore.ltDoping[i].sName, Value = i.ToString() });
    }

  }
}