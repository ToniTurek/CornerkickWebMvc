using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
      public string sLength { get; set; }
      public string sNat { get; set; }
    }
  }
}