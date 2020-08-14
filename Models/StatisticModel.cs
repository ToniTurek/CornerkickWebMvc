using System;
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
    public float fAttrFactor { get; set; }
    public string sLeague { get; set; }
  }

  public class DatatableEntryStadiums
  {
    public int iIx { get; set; }
    public string sName { get; set; }
    public string sClubName { get; set; }
    public int iTotal { get; set; }
    public int iTotalCtn { get; set; }
    public int iType0 { get; set; }
    public int iType1 { get; set; }
    public int iType2 { get; set; }
    public int iType0Ctn { get; set; }
    public int iType1Ctn { get; set; }
    public int iType2Ctn { get; set; }
    public bool bTopring { get; set; }
  }
}