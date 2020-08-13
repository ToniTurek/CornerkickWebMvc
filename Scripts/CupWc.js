function setMatchdayCupWc() {
  var ddlSeason = document.getElementById("ddlSeasonCupWc");
  var ddlMatchday = document.getElementById("ddlMatchdayCupWc");
  var ddlGroup = document.getElementById("ddlGroupsCupWc");

  var iMd = ddlMatchday.value;

  // Show/hide groups
  var divCupWcTable = document.getElementById("divCupWcTable");
  var lbGroupsCupWc = document.getElementById("lbGroupsCupWc");
  if (iMd > 0 && iMd < 4) {
    lbGroupsCupWc.style.display = "inline";

    // Table
    if (dtCupWcTable) {
      dtCupWcTable.ajax.reload();
    } else {
      dtCupWcTable = getTableDatatable(divCupWcTable, 7, ddlSeason, null, null, ddlMatchday, ddlGroup, null, null, 2, 0, 0, 0);
    }
  } else {
    lbGroupsCupWc.style.display = "none";
  }
  divCupWcTable.style.display = lbGroupsCupWc.style.display;

  $.ajax({
    url: '/Member/setCupWc',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: ddlSeason.value, iMatchday: iMd - 1, iGroup: ddlGroup.value },
    success: function (sTeams) {
      actionDrawTeams(sTeams);
    }
  });

  // Scorer table
  var dtCupWcScorer = setTableScorer(document.getElementById("divCupWcScorer"), 7, null, null);
}

function actionDrawTeams(sTeams) {
  var divDrawCupTeams = $("#divCupWcTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTeams);
  divDrawCupTeams.html(result).show();
}
