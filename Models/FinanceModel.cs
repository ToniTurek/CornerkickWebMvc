using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CornerkickWebMvc.Models
{
  public class FinanceModel
  {
    public string sKonto { get; set; }
    public List<CornerkickCore.Finance.Account[]> ltKonto { get; set; }

    [DataType(DataType.Currency)]
    [Display(Name = "Stehplätze:")]
    public int iEintritt1 { get; set; }
    public int iPriceSeason1 { get; set; }

    [DataType(DataType.Currency)]
    [Display(Name = "Sitzplätze:")]
    public int iEintritt2 { get; set; }
    public int iPriceSeason2 { get; set; }

    [DataType(DataType.Currency)]
    [Display(Name = "V.I.P.:")]
    public int iEintritt3 { get; set; }
    public int iPriceSeason3 { get; set; }

    public int[] iSeasonalTickets { get; set; }

    public CornerkickCore.Finance.Budget budgetPlan { get; set; }
    public CornerkickCore.Finance.Budget budgetReal { get; set; }
    public bool bEditable { get; set; }
  }

  public class DiaryFinanceEvent
  {
    public int iID;
    public string sTitle;
    public string sDescription;
    public int SomeImportantKeyID;
    public string sStartDate;
    public string sEndDate;
    public string StatusString;
    public string sColor;
    public string sBackgroundColor;
    public string sBorderColor;
    public string sTextColor;
    public bool bEditable;
    public string ClassName;
  }

  //DataContract for Serializing Data - required to serve in JSON format
  [DataContract]
  public class DataPointKonto
  {
    public DataPointKonto(long x, long y)
    {
      this.X = x;
      this.Y = y;
    }

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "x")]
    public Nullable<long> X = null;
    //[DataMember(Name = "x")]
    //public Nullable<DateTime> X = null;

    //Explicitly setting the name to be used while serializing to JSON.
    [DataMember(Name = "y")]
    public Nullable<double> Y = null;
  }
}