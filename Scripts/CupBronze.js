function setMatchdayCupBronze(bTeamsOnly = false) {
  var ddlSeason = document.getElementById("ddlSeasonCupBronze");
  var ddlMatchday = document.getElementById("ddlMatchdayCupBronze");
  var ddlGroup = document.getElementById("ddlGroupsCupBronze");

  var iMd = ddlMatchday.value;

  $.ajax({
    url: '/Member/setCupBronze',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: ddlSeason.value, iMatchday: iMd - 1, iGroup: ddlGroup.value },
    success: function (sTeams) {
      actionDrawTeamsCupBronze(sTeams);
    }
  });

  if (!bTeamsOnly) {
    // Show/hide group ddl
    var divCupBronzeTable = document.getElementById("divCupBronzeTable");
    var lbGroupsCupBronze = document.getElementById("lbGroupsCupBronze");
    if (iMd > 0 && iMd < 7) {
      lbGroupsCupBronze.style.display = "inline";

      // Table
      if (dtCupBronzeTable) {
        dtCupBronzeTable.ajax.reload();
      } else {
        dtCupBronzeTable = getTableDatatable(divCupBronzeTable, 15, ddlSeason, null, null, ddlMatchday, ddlGroup, null, null, 2, 0, 0, 0);
      }
    } else {
      lbGroupsCupBronze.style.display = "none";
    }
    divCupBronzeTable.style.display = lbGroupsCupBronze.style.display;

    // Scorer table
    var dtCupBronzeScorer = setTableScorer(document.getElementById("divCupBronzeScorer"), 15, null, null);
  }
}

function actionDrawTeamsCupBronze(sTeams) {
  var divDrawCupTeams = $("#divCupBronzeTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTeams);
  divDrawCupTeams.html(result).show();
}
