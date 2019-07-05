function setMatchday(iMd) {
  $.ajax({
    url: '/Member/setCupGold',
    type: "GET",
    dataType: "JSON",
    data: { iMatchday: iMd },
    success: function (sTable) {
      actionDrawTeams(sTable);
    }
  });
}

function actionDrawTeams(sTable) {
  var divDrawCupTeams = $("#tableDivCupGoldTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTable);
  divDrawCupTeams.html(result).show();
}
