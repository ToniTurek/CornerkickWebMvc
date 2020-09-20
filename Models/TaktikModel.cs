using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class TaktikModel
  {
    public CornerkickGame.Tactic tactic { get; set; }

    [Display(Name = "Ausrichtung")]
    public float fAusrichtung { get; set; }

    [Display(Name = "Schussentfernung")]
    public float fSchussentf { get; set; }

    [Display(Name = "Einsatz")]
    public float fEinsatz { get; set; }

    [Display(Name = "Aggressivität")]
    public float fZkAggressiv { get; set; }

    [Display(Name = "Passlänge")]
    public float fPassLength { get; set; }

    [Display(Name = "Passrisiko")]
    public float fPassRisk { get; set; }

    [Display(Name = "Passhäufigkeit")]
    public float fPassFreq { get; set; }

    [Display(Name = "Links")]
    public float fLeft { get; set; }

    [Display(Name = "Rechts")]
    public float fRight { get; set; }

    public List<SelectListItem>[] ltDdlStandards { get; set; }
    public string[] sStandards { get; set; }

    public int[] iStandards { get; set; }

    public List<SelectListItem>[] ddlAutoSubsOut { get; set; }
    public string[] sAutoSubsOut { get; set; }

    public List<SelectListItem>[] ddlAutoSubsIn { get; set; }
    public string[] sAutoSubsIn { get; set; }

    public int[] iAutoSubsMin { get; set; }

    public Controllers.MemberController.Tutorial tutorial { get; set; }
  }
}