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

    public List<SelectListItem> ltDdlPersonalCoachCo { get; set; }
    public string sPersonalCoachCo { get; set; }

    public List<SelectListItem> ltDdlPersonalCoachCondi { get; set; }
    public string sPersonalCoachCondi { get; set; }

    public List<SelectListItem> ltDdlPersonalMasseur { get; set; }
    public string sPersonalMasseur { get; set; }

    public List<SelectListItem> ltDdlPersonalMental { get; set; }
    public string sPersonalMental { get; set; }

    public List<SelectListItem> ltDdlPersonalMed { get; set; }
    public string sPersonalMed { get; set; }

    public List<SelectListItem> ltDdlPersonalJouthCoach { get; set; }
    public string sPersonalJouthCoach { get; set; }

    public List<SelectListItem> ltDdlPersonalJouthScouting { get; set; }
    public string sPersonalJouthScouting { get; set; }

    public List<SelectListItem> ltDdlPersonalKibitzer { get; set; }
    public string sPersonalKibitzer { get; set; }
  }
}