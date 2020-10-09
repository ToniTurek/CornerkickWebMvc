function setMatchdayCupSilver(bTeamsOnly = false) {
  var ddlSeason = document.getElementById("ddlSeasonCupSilver");
  var ddlMatchday = document.getElementById("ddlMatchdayCupSilver");
  var ddlGroup = document.getElementById("ddlGroupsCupSilver");

  var iMd = ddlMatchday.value;

  $.ajax({
    url: '/Member/setCupSilver',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: ddlSeason.value, iMatchday: iMd - 1, iGroup: ddlGroup.value },
    success: function (sTeams) {
      actionDrawTeamsCupSilver(sTeams);
    }
  });

  if (!bTeamsOnly) {
    // Show/hide group ddl
    var divCupSilverTable = document.getElementById("divCupSilverTable");
    var lbGroupsCupSilver = document.getElementById("lbGroupsCupSilver");
    if (iMd > 0 && iMd < 7) {
      lbGroupsCupSilver.style.display = "inline";

      // Table
      if (dtCupSilverTable) {
        dtCupSilverTable.ajax.reload();
      } else {
        dtCupSilverTable = getTableDatatable(divCupSilverTable, 4, ddlSeason, null, null, ddlMatchday, ddlGroup, null, null, 2, 0, 0, 0);
      }
    } else {
      lbGroupsCupSilver.style.display = "none";
    }
    divCupSilverTable.style.display = lbGroupsCupSilver.style.display;

    // Scorer table
    var dtCupSilverScorer = setTableScorer(document.getElementById("divCupSilverScorer"), 4, null, null);
  }
}

function actionDrawTeamsCupSilver(sTeams) {
  var divDrawCupSilverTeams = $("#divCupSilverTeams");
  divDrawCupSilverTeams.html('');
  result = drawTeams(sTeams);
  divDrawCupSilverTeams.html(result).show();
}
