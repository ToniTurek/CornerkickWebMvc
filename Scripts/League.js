function setLeague2() {
  var iSpTg = $('#ddlSpieltag').val();

  $.ajax({
    url: '/Member/setLeague',
    type: "GET",
    dataType: "JSON",
    data: { iGameday: iSpTg },
    success: function (ltTbl) {
      actionDrawTable(ltTbl);
    }
  });

  $.ajax({
    url: '/Member/setLeagueTeams',
    type: "GET",
    dataType: "JSON",
    data: { iGameday: iSpTg },
    success: function (ltErg) {
      actionDrawTeams(ltErg, iSpTg - 1);
    }
  });
}

function actionDrawTable(ltTbl) {
  var divDrawTable = $("#tableDivLeague");
  divDrawTable.html('');
  result = drawTable(ltTbl);
  divDrawTable.html(result).show();
}

function drawTable(ltTbl) {
  var sBox = '';
  var iClubUser = $('#idClubUser').data('name');

  sBox += '<table id="tableLeague" style="width: 100%">';
  sBox += '<tr>';
  sBox += '<th colspan="2">Pl.</th>';
  sBox += '<th>Verein</th>';
  sBox += '<th style="text-align:right; width: 3%">&nbsp;</th>';
  sBox += '<th style="text-align:right">Sp.</th>';
  sBox += '<th style="text-align:center; width: 3%">&nbsp;</th>';
  sBox += '<th style="text-align:right">g.</th>';
  sBox += '<th style="text-align:right">u.</th>';
  sBox += '<th style="text-align:right">v.</th>';
  sBox += '<th style="text-align:center; width: 3%">&nbsp;</th>';
  sBox += '<th style="text-align:center">Tore</th>';
  sBox += '<th style="text-align:right">Diff.</th>';
  sBox += '<th style="text-align:center; width: 3%">&nbsp;</th>';
  sBox += '<th style="text-align:right">Pkte.</th>';
  sBox += '</tr>';

  for (var i = 0; i < ltTbl.length; i++) {
    var tbpl = ltTbl[i];
    var iGames = tbpl.iGUV[0] + tbpl.iGUV[1] + tbpl.iGUV[2];
    var iDiff = tbpl.iGoals - tbpl.iGoalsOpp;

    var sStyle = "";
    if (tbpl.iId === iClubUser) {
      sStyle = "font-weight:bold";
    }

    var k = i + 1;
    sBox += '<tr style=' + sStyle + '>';
    sBox += '<td class="first"><b>' + k + '</b></td>';
    sBox += '<td><img src="http://mediadb.kicker.de/library/images/tendenz-hold.png" alt="" /></td>';
    sBox += '<td><a href="/Member/ClubDetails?iClub=__id" target="_blank">__name</a></td>)'.replace("__name", tbpl.sName).replace("__id", tbpl.iId);
    //sBox += '<td><a>' + tbpl.sName + '</a></td>';
    sBox += '<td>&nbsp;</td>';
    sBox += '<td align="right">' + iGames + '</td>';
    sBox += '<td>&nbsp;</td>';
    sBox += '<td align="right">' + tbpl.iGUV[0] + '</td>';
    sBox += '<td align="right">' + tbpl.iGUV[1] + '</td>';
    sBox += '<td align="right">' + tbpl.iGUV[2] + '</td>';
    sBox += '<td>&nbsp;</td>';
    sBox += '<td align="center">' + tbpl.iGoals + ':' + tbpl.iGoalsOpp + '</td>';
    sBox += '<td align="right">' + iDiff + '</td>';
    sBox += '<td>&nbsp;</td>';
    sBox += '<td align="right">' + tbpl.iPoints + '</td>';
    sBox += '</tr>';
  }
  sBox += '</table>';

  return sBox;
}

function actionDrawTeams(ltErg, iSpieltag) {
  var divDrawTeams = $("#tableDivTeams");
  divDrawTeams.html('');
  result = drawTeams(ltErg, iSpieltag);
  divDrawTeams.html(result).show();
}

function drawTeams(ltErg, iSpieltag) {
  var sBox = '';

  sBox += '<table id="tableLeagueTeams" border="0" cellpadding="2" style="width: 100%">';
  sBox += '<tr>';
  sBox += '  <th colspan="1">Anstoß</th>';
  sBox += '  <th style="text-align:right">Heim</th>';
  sBox += '  <th style="text-align:center">&nbsp;</th>';
  sBox += '  <th style="text-align:left">Auswärts</th>';
  sBox += '  <th style="text-align:center">Erg.</th>';
  sBox += '</tr>';

  var sDateDay  = $('#idDtDay' + iSpieltag).data('name');
  var sDateHour = $('#idDtHour' + iSpieltag).data('name');
  var iClubUser = $('#idClubUser').data('name');
  for (var i = 0; i < ltErg.length; i++) {
    iBeg = ltErg[i];
    var sClubNameH = $('#idClubNames' + iBeg[0]).data('name');
    var sClubNameA = $('#idClubNames' + iBeg[1]).data('name');

    var sStyle = "";
    if (iBeg[0] === iClubUser || iBeg[1] === iClubUser) {
      sStyle = "font-weight:bold";
    }

    sBox += '<tr style=' + sStyle + '>';
    sBox += '  <td>' + sDateDay + '&nbsp;' + sDateHour + '</td>';

    sBox += '<td align="right"><a href="/Member/ClubDetails?iClub=__id" target="_blank">__name</a></td>)'.replace("__name", sClubNameH).replace("__id", iBeg[0]) + '</td>';
    sBox += '<td align="center">&nbsp;-&nbsp;</td>';
    sBox += '<td align="left"><a href="/Member/ClubDetails?iClub=__id" target="_blank">__name</a></td>)'.replace("__name", sClubNameA).replace("__id", iBeg[1]) + '</td>';

    if (iBeg[2] >= 0 && iBeg[3] >= 0) {
      sBox += '<td align="center">' + iBeg[2] + ':' + iBeg[3] + '&nbsp;(' + iBeg[4] + ':' + iBeg[5] + ')</td>';
    } else {
      sBox += '<td align="center">-</td>';
    }
    sBox += '</tr>';
  }
  sBox += '</table>';

  return sBox;
}
