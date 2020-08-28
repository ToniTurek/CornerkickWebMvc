using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class UserManualModel
  {
    // Cup attraction factors
    public string sAfLeague { get; set; }
    public string sAfCup { get; set; }
    public string sAfCupGold { get; set; }
    public string sAfCupSilver { get; set; }
    public string sAfWc { get; set; }
    public string sAfTg { get; set; }

    // Chart training CFM
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

    // Chart player steps fresh loss
    public string sStepsSpeed { get; set; }
    public List<SelectListItem> ddlStepsSpeed { get; set; }

    public string sStepsAcceleration { get; set; }
    public List<SelectListItem> ddlStepsAcceleration { get; set; }

    public string sStepsLastSteps { get; set; }
    public List<SelectListItem> ddlStepsLastSteps { get; set; }

    public UserManualModel()
    {
      // Chart training CFM
      ddlTraining = new List<SelectListItem>();
      foreach (CornerkickManager.PlayerTool.Training tr in MvcApplication.ckcore.plt.ltTraining) {
        if (tr.iId < 0) continue;

        ddlTraining.Add(
          new SelectListItem {
            Text = tr.sName,
            Value = tr.iId.ToString()
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

      // Chart player steps fresh loss
      ddlStepsSpeed = new List<SelectListItem>();
      for (byte i = 4; i < 11; i++) ddlStepsSpeed.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

      ddlStepsAcceleration = new List<SelectListItem>();
      for (byte i = 4; i < 11; i++) ddlStepsAcceleration.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });

      ddlStepsLastSteps = new List<SelectListItem>();
      for (byte i = 0; i < 9; i++) ddlStepsLastSteps.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
    }

  }
}