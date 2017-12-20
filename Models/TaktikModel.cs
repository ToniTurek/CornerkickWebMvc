using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CornerkickWebMvc.Models
{
  public class TaktikModel
  {
    [Display(Name = "Ausrichtung")]
    public float fAusrichtung { get; set; }

    [Display(Name = "Schussentfernung")]
    public float fSchussentf { get; set; }

    [Display(Name = "Einsatz")]
    public float fEinsatz { get; set; }
  }
}