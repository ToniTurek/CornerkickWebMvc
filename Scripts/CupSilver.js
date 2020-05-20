function setMatchdayCupSilver(iSaison, iMd, iGroup) {
  // Show/hide group ddl
  var lbGroupsCupSilver = document.getElementById("lbGroupsCupSilver");
  if (iMd > 5) {
    lbGroupsCupSilver.style.display = "none";
  } else {
    lbGroupsCupSilver.style.display = "inline";
  }

  $.ajax({
    url: '/Member/setCupSilver',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iMatchday: iMd, iGroup: iGroup },
    success: function (sTeams) {
      actionDrawTeamsCupSilver(sTeams);
      drawTableCupSilver();
    }
  });
}

function actionDrawTeamsCupSilver(sTeams) {
  var divDrawCupSilverTeams = $("#tableDivCupSilverTeams");
  divDrawCupSilverTeams.html('');
  result = drawTeams(sTeams);
  divDrawCupSilverTeams.html(result).show();
}

function drawTableCupSilver() {
  var iSn = parseInt($('#ddlSeasonCupSilver').val());
  var iMd = parseInt($('#ddlMatchdayCupSilver').val());
  var iGp = parseInt($('#ddlGroupsCupSilver').val());

  var divTableCupSilver = $("#divCupSilverTable");
  var divScorerCupSilver = $("#divCupSilverScorer");

  divTableCupSilver.html('');
  divScorerCupSilver.html('');

  $.ajax({
    url: '/Member/CupGetLeague',
    type: "GET",
    dataType: "JSON",
    data: { iCupId: 4, iSaison: iSn, iMatchday: iMd, iGroup: iGp },
    success: function (sTable) {
      var sBox = drawTable(sTable);
      divTableCupSilver.html(sBox).show();
    }
  });
}