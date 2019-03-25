function setCup2(iMd) {
  $.ajax({
    url: '/Member/setCup',
    type: "GET",
    dataType: "JSON",
    data: { iGameday: iMd },
    success: function (ltGameData) {
      actionDrawTeams(ltGameData, iMd - 1);
    }
  });
}

function actionDrawTeams(ltGameData, iSpieltag) {
  var divDrawCupTeams = $("#tableDivCupTeams");
  divDrawCupTeams.html('');
  result = drawCupTeams(ltGameData, iSpieltag);
  divDrawCupTeams.html(result).show();
}

function drawCupTeams(ltGameData, iSpieltag) {
  var sBox = '';

  sBox += '<table id="tableCupTeams" cellpadding="2" style="width: 100%">';
  sBox += '<tr>';
  sBox += '<th colspan="1">Anstoß</th>';
  sBox += '<th style="text-align:right">Heim</th>';
  sBox += '<th style="text-align:center">&nbsp;</th>';
  sBox += '<th style="text-align:left">Auswärts</th>';
  sBox += '<th style="text-align:center">Erg.</th>';
  sBox += '<th>&nbsp;</th>';
  sBox += '</tr>';

  var sDateDay = $('#idDtDay' + iSpieltag).data('name');
  var sDateHour = $('#idDtHour' + iSpieltag).data('name');
  var iClubUser = $('#idClubUser').data('name');
  for (var i = 0; i < ltGameData.length; i++) {
    var gd = ltGameData[i];
    var sClubNameH = "freilos";
    var sClubNameA = "freilos";
    if (gd.team[0].iTeamId >= 0) {
      sClubNameH = $('#idClubNames' + gd.team[0].iTeamId).data('name');
    }
    if (gd.team[1].iTeamId >= 0) {
      sClubNameA = $('#idClubNames' + gd.team[1].iTeamId).data('name');
    }
    
    var sStyle = "";
    if (gd.team[0].iTeamId === iClubUser || gd.team[1].iTeamId === iClubUser) {
      sStyle = "font-weight:bold";
    }

    sBox += '<tr style=' + sStyle + '>';
    sBox += '<td>' + sDateDay + '&nbsp;' + sDateHour + '</td>';

    sBox += '<td align="right">' + sClubNameH + '</td>';
    sBox += '<td align="center">&nbsp;-&nbsp;</td>';
    sBox += '<td align="left">' + sClubNameA + '</td>';

    if (gd.team[0].iGoals >= 0 && gd.team[1].iGoals >= 0) {
      sBox += '<td align="center">' + gd.team[0].iGoals + ':' + gd.team[1].iGoals + '&nbsp;(' + gd.team[0].iGoalsHt + ':' + gd.team[1].iGoalsHt + ')</td>';
    } else {
      sBox += '<td align="center">-</td>';
    }
    sBox += '</tr>';
  }
  sBox += '</table>';

  return sBox;
}
