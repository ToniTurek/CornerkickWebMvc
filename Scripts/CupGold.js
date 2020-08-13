function setMatchdayCupGold() {
  var ddlSeason = document.getElementById("ddlSeasonCupGold");
  var ddlMatchday = document.getElementById("ddlMatchdayCupGold");
  var ddlGroup = document.getElementById("ddlGroupsCupGold");

  var iMd = ddlMatchday.value;

  // Show/hide group ddl
  var divCupGoldTable = document.getElementById("divCupGoldTable");
  var lbGroupsCupGold = document.getElementById("lbGroupsCupGold");
  if (iMd > 0 && iMd < 7) {
    lbGroupsCupGold.style.display = "inline";

    // Table
    if (dtCupGoldTable) {
      dtCupGoldTable.ajax.reload();
    } else {
      dtCupGoldTable = getTableDatatable(divCupGoldTable, 3, ddlSeason, null, null, ddlMatchday, ddlGroup, null, null, 2, 0, 0, 0);
    }
  } else {
    lbGroupsCupGold.style.display = "none";
  }
  divCupGoldTable.style.display = lbGroupsCupGold.style.display;

  $.ajax({
    url: '/Member/setCupGold',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: ddlSeason.value, iMatchday: iMd - 1, iGroup: ddlGroup.value },
    success: function (sTeams) {
      actionDrawTeamsCupGold(sTeams);
    }
  });

  // Scorer table
  var dtCupGoldScorer = setTableScorer(document.getElementById("divCupGoldScorer"), 3, null, null);
}

function actionDrawTeamsCupGold(sTeams) {
  var divDrawCupTeams = $("#divCupGoldTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTeams);
  divDrawCupTeams.html(result).show();
}
