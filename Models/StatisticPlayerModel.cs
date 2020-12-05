using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CornerkickWebMvc.Models
{
  public class StatisticPlayerModel
  {
    public int iNation { get; set; }
    public List<SelectListItem> ddlNations { get; set; }

    public string sFormation { get; set; }
    public List<SelectListItem> ltsFormations { get; set; }

    public string[][] sPlayerSkillBest { get; set; }

    public List<SelectListItem> ddlFilterCup { get; set; }
    public string sFilterCup { get; set; }

    public StatisticPlayerModel()
    {
      ddlFilterCup = new List<SelectListItem>();

      /*
      foreach (int iN in MvcApplication.iNations) {
        for (byte iD = 0; iD < 2; iD++) {
          CornerkickManager.Cup league = MvcApplication.ckcore.tl.getCup(1, iN, iD);
          if (league != null) ddlFilterCup.Add(new SelectListItem { Text = league.sName, Value = "1," + iN.ToString() + "," + iD.ToString() });
        }

        CornerkickManager.Cup cupNat = MvcApplication.ckcore.tl.getCup(2, iN, -1);
        ddlFilterCup.Add(new SelectListItem { Text = cupNat.sName, Value = "2," + iN.ToString() + ",-1" });
      }
      */

      ddlFilterCup.Add(new SelectListItem { Text = "Liga", Value = "1,-1,-1" });
      ddlFilterCup.Add(new SelectListItem { Text = "Nat. Pokal", Value = "2,-1,-1" });

      CornerkickManager.Cup cupG = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdGold);
      ddlFilterCup.Add(new SelectListItem { Text = cupG.sName, Value = MvcApplication.iCupIdGold.ToString() + ",-1,-1" });
      CornerkickManager.Cup cupS = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdSilver);
      ddlFilterCup.Add(new SelectListItem { Text = cupS.sName, Value = MvcApplication.iCupIdSilver.ToString() + ",-1,-1" });
      CornerkickManager.Cup cupB = MvcApplication.ckcore.tl.getCup(MvcApplication.iCupIdBronze);
      ddlFilterCup.Add(new SelectListItem { Text = cupB.sName, Value = MvcApplication.iCupIdBronze.ToString() + ",-1,-1" });

      ddlFilterCup.Add(new SelectListItem { Text = "Nationalm.", Value = "7,-1,-1" });
    }
  }

  /*
   0 - Games
   1 - Goals right foot
   2 - Goals left foot
   3 - Goals header
   4 - Goals penalty +
   5 - Goals penalty -
   6 - Goals free-kick +
   7 - Goals free-kick -
   8 - Assists
   9 - Shoots
  10 - Shoots on goal
  11 - Shoots saved (KP)
  12 - Goals against (KP)
  13 - Penalties saved (KP)
  14 - Penalties passed (KP)
  15 - Pass +
  16 - Pass -
  17 - Duel def. +
  18 - Duel def. -
  19 - Duel off. +
  20 - Duel off. -
  21 - Fouls
  22 - Yellow Cards
  23 - Y-R Cards
  24 - Red Cards
  25 - Ball contacts
  26 - Pass stolen
  27 - Assists shoot (all assists for shoots incl. assists for goals)
  28 - Minutes played
  29 - Grade
  30 - Graded games
  31 - Steps
  */
  public class DatatableEntryPlayer
  {
    public int iPlayerIx { get; set; }
    public string sName { get; set; }
    public string sClubName { get; set; }
    public string sNat { get; set; }
    public float fAge { get; set; }
    public int iGames { get; set; }
    public int iMinutes { get; set; }
    public int iGoals { get; set; }
    public float fGoalsPerGame { get; set; }
    public int iGoalsLeft { get; set; }
    public int iGoalsRight { get; set; }
    public int iGoalsHeader { get; set; }
    public int iPenaltyP { get; set; }
    public int iPenaltyM { get; set; }
    public float fPenalty { get; set; }
    public int iFreekickP { get; set; }
    public int iFreekickM { get; set; }
    public float fFreekick { get; set; }
    public int iAssists { get; set; }
    public int iShoots { get; set; }
    public int iShootsOG { get; set; }
    public int iAssistShoots { get; set; }
    public int iPassP { get; set; }
    public int iPassM { get; set; }
    public float fPass { get; set; }
    public int iDuelDefP { get; set; }
    public int iDuelDefM { get; set; }
    public float fDuelDef { get; set; }
    public int iDuelOffP { get; set; }
    public int iDuelOffM { get; set; }
    public float fDuelOff { get; set; }
    public int iFouls { get; set; }
    public int iCardY { get; set; }
    public int iCardYR { get; set; }
    public int iCardR { get; set; }
    public int iBallContacts { get; set; }
    public int iPassStolen { get; set; }
    public float fGrade { get; set; }
  }
}