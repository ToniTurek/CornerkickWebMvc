using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class StatisticModel
  {
    public string sFormation { get; set; }
    public List<SelectListItem> ltsFormations { get; set; }
  }
}