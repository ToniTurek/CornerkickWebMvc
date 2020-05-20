function setMatchdayCupGold(iSaison, iMd, iGroup) {
  // Show/hide group ddl
  var lbGroupsCupGold = document.getElementById("lbGroupsCupGold");
  if (iMd > 5) {
    lbGroupsCupGold.style.display = "none";
  } else {
    lbGroupsCupGold.style.display = "inline";
  }

  $.ajax({
    url: '/Member/setCupGold',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iMatchday: iMd, iGroup: iGroup },
    success: function (sTeams) {
      actionDrawTeamsCupGold(sTeams);
      drawTableCupGold();
    }
  });
}

function actionDrawTeamsCupGold(sTeams) {
  var divDrawCupTeams = $("#tableDivCupGoldTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTeams);
  divDrawCupTeams.html(result).show();
}

function drawTableCupGold() {
  var iSn = parseInt($('#ddlSeasonCupGold').val());
  var iMd = parseInt($('#ddlMatchdayCupGold').val());
  var iGp = parseInt($('#ddlGroupsCupGold').val());

  var divTableCupGold = $("#divCupGoldTable");
  var divScorerCupGold = $("#divCupGoldScorer");

  divTableCupGold.html('');
  divScorerCupGold.html('');

  $.ajax({
    url: '/Member/CupGetLeague',
    type: "GET",
    dataType: "JSON",
    data: { iCupId: 3, iSaison: iSn, iMatchday: iMd, iGroup: iGp },
    success: function (sTable) {
      var sBox = drawTable(sTable);
      divTableCupGold.html(sBox).show();
    }
  });
}