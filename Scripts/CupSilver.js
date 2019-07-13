function setMatchday(iMd) {
  $.ajax({
    url: '/Member/setCupSilver',
    type: "GET",
    dataType: "JSON",
    data: { iMatchday: iMd },
    success: function (sTable) {
      actionDrawTeams(sTable);
    }
  });
}

function actionDrawTeams(sTable) {
  var divDrawCupTeams = $("#tableDivCupSilverTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTable);
  divDrawCupTeams.html(result).show();
}
