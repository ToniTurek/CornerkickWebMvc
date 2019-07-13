function setMatchday(iMd, iGroup) {
  $.ajax({
    url: '/Member/setCupWc',
    type: "GET",
    dataType: "JSON",
    data: { iMatchday: iMd, iGroup: iGroup },
    success: function (sTeams) {
      actionDrawTeams(sTeams);
      drawTableWc();
    }
  });
}

function actionDrawTeams(sTeams) {
  var divDrawCupTeams = $("#divCupWcTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTeams);
  divDrawCupTeams.html(result).show();
}

function drawTableWc() {
  var iMd = parseInt($('#ddlMatchdayCupWc').val());
  var iGp = $('#iGroup')     .val();

  var divTableWc = $("#divCupWcTable");
  var divScorer = $("#divLeagueScorer");

  divTableWc.html('');
  divScorer.html('');

  $.ajax({
    url: '/Member/CupWcGetLeague',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: 1, iMatchday: iMd, iGroup: iGp },
    success: function (sTable) {
      var sBox = drawTable(sTable);
      divTableWc.html(sBox).show();
    }
  });
}