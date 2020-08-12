function setMatchdayCupWc(iSaison, iMd, iGroup) {
  // Show/hide group ddl
  var lbGroupsCupWc = document.getElementById("lbGroupsCupWc");
  if (iMd >= 0 && iMd < 3) {
    lbGroupsCupWc.style.display = "inline";
  } else {
    lbGroupsCupWc.style.display = "none";
  }

  $.ajax({
    url: '/Member/setCupWc',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iMatchday: iMd, iGroup: iGroup },
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
  var iGp = parseInt($('#ddlGroupsCupWc').val());

  var divTableWc = $("#divCupWcTable");
  var divScorer = $("#divCupWcScorer");

  divTableWc.html('');
  divScorer.html('');

  $.ajax({
    url: '/Member/CupGetLeague',
    type: "GET",
    dataType: "JSON",
    data: { iCupId: 7, iSaison: 1, iMatchday: iMd, iGroup: iGp },
    success: function (sTable) {
      var sBox = drawTable(sTable);
      divTableWc.html(sBox).show();
    }
  });

  // Scorer table
  var dtCupWcScorer = setTableScorer(document.getElementById("divCupWcScorer"), 7, null, null);
}