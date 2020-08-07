using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class ContractsModel
  {
    public class DatatableEntry
    {
      public string sID { get; set; }
      public string sNb { get; set; }
      public string sName { get; set; }
      public string sSkill { get; set; }
      public string sSkillIdeal { get; set; }
      public string sPosition { get; set; }
      public string sAge { get; set; }
      public string sTalent { get; set; }
      public int iValue { get; set; }
      public int iSalary { get; set; }
      public int iBonusPlay { get; set; }
      public int iBonusGoal { get; set; }
      public string sFixTransferFee { get; set; }
      public int iLength { get; set; }
      public string sNat { get; set; }
      public bool bJouth { get; set; }
    }

    // Filter
    public List<SelectListItem> ltDdlFilterPos { get; set; }
    [Display(Name = "Pos.: ")]
    public string sFilterPos { get; set; }

    public ContractsModel()
    {
      ltDdlFilterPos = new List<SelectListItem>();
      ltDdlFilterPos.Add(new SelectListItem { Text = "alle", Value = "0" });

      // Positionen zu Dropdown Menü hinzufügen
      for (int iPos = 1; iPos < 12; iPos++) {
        ltDdlFilterPos.Add(new SelectListItem { Text = CornerkickManager.Main.sPosition[iPos], Value = iPos.ToString() });
      }
    }
  }
}