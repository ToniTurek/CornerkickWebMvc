function setMatchday(iMd, iGroup) {
  $.ajax({
    url: '/Member/setCupWc',
    type: "GET",
    dataType: "JSON",
    data: { iMatchday: iMd, iGroup: iGroup },
    success: function (sTable) {
      actionDrawTeams(sTable);
    }
  });
}

function actionDrawTeams(sTable) {
  var divDrawCupTeams = $("#tableDivCupWcTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTable);
  divDrawCupTeams.html(result).show();
}
