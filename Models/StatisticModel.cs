﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class StatisticModel
  {
    public int iNation { get; set; }
    public List<SelectListItem> ddlNations { get; set; }

    public string sFormation { get; set; }
    public List<SelectListItem> ltsFormations { get; set; }

    public string sCupId { get; set; }
    public List<SelectListItem> ddlLeagues { get; set; }

    public string[][] sPlayerSkillBest { get; set; }
  }

  public class DatatableEntryTeams
  {
    public int iIx { get; set; }
    public int iTeamId { get; set; }
    public string sTeamName { get; set; }
    public string sTeamAveSkill { get; set; }
    public string sTeamAveAge { get; set; }
    public int iTeamValueTotal { get; set; }
    public int nPlayer { get; set; }
    public string sTeamAveSkill11 { get; set; }
    public string sTeamAveAge11 { get; set; }
    public int iTeamValueTotal11 { get; set; }
    public string sLeague { get; set; }
  }
}