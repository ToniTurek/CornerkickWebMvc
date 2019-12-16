function setMatchdayCupGold(iMd, iGroup) {
  $.ajax({
    url: '/Member/setCupGold',
    type: "GET",
    dataType: "JSON",
    data: { iMatchday: iMd, iGroup: iGroup },
    success: function (sTeams) {
      actionDrawTeams(sTeams);
      drawTableCupGold();
    }
  });
}

function actionDrawTeams(sTeams) {
  var divDrawCupTeams = $("#tableDivCupGoldTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTeams);
  divDrawCupTeams.html(result).show();
}

function drawTableWc() {
  var iMd = parseInt($('#ddlMatchdayCupWc').val());
  var iGp = parseInt($('#ddlGroupsCupGold').val());

  var divTableWc = $("#divCupGoldTable");
  var divScorer = $("#divCupGoldScorer");

  divTableWc.html('');
  divScorer.html('');

  $.ajax({
    url: '/Member/CupGetLeague',
    type: "GET",
    dataType: "JSON",
    data: { iCupId: 3, iSaison: 1, iMatchday: iMd, iGroup: iGp },
    success: function (sTable) {
      var sBox = drawTable(sTable);
      divTableWc.html(sBox).show();
    }
  });
}