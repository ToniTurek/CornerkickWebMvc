function setMatchdayCupSilver(iSaison, iMd, iGroup) {
  $.ajax({
    url: '/Member/setCupSilver',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iMatchday: iMd, iGroup: iGroup },
    success: function (sTeams) {
      actionDrawTeams(sTeams);
      drawTableCupSilver();
    }
  });
}

function actionDrawTeams(sTeams) {
  var divDrawCupTeams = $("#tableDivCupSilverTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTeams);
  divDrawCupTeams.html(result).show();
}

function drawTableWc() {
  var iMd = parseInt($('#ddlMatchdayCupSilver').val());
  var iGp = parseInt($('#ddlGroupsCupSilver').val());

  var divTableWc = $("#divCupSilverTable");
  var divScorer = $("#divCupSilverScorer");

  divTableWc.html('');
  divScorer.html('');

  $.ajax({
    url: '/Member/CupGetLeague',
    type: "GET",
    dataType: "JSON",
    data: { iCupId: 4, iSaison: 1, iMatchday: iMd, iGroup: iGp },
    success: function (sTable) {
      var sBox = drawTable(sTable);
      divTableWc.html(sBox).show();
    }
  });
}