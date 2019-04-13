function changeLand(iSaison, iDivision) {
  var iLand = $('#ddlLand').val();

  $.ajax({
    url: '/Member/getDdlMatchdays',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iLand: iLand, iDivision: iDivision },
    success: function (ltMd) {
      $('#ddlMatchday').empty();
      $.each(ltMd, function (i, p) {
        $('#ddlMatchday').append($('<option></option>').val(p).html(p));
      });

      setLeague2(iSaison, iDivision);
    }
  });
}

function setLeague2(iSaison, iDivision) {
  var iLand = $('#ddlLand').val();
  var iMd = $('#ddlMatchday').val();

  $.ajax({
    url: '/Member/setLeague',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iLand: iLand, iDivision: iDivision, iMatchday: iMd },
    success: function (ltTbl) {
      actionDrawTable(ltTbl);
    }
  });

  $.ajax({
    url: '/Member/setLeagueTeams',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iLand: iLand, iDivision: iDivision, iMatchday: iMd },
    success: function (ltGamedata) {
      actionDrawTeams(ltGamedata, iMd - 1);
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

  for (var i = 0; i < ltTbl[0].length; i++) {
    var tbpl = ltTbl[0][i];
    var iGames = tbpl.iGUV[0] + tbpl.iGUV[1] + tbpl.iGUV[2];
    var iDiff = tbpl.iGoals - tbpl.iGoalsOpp;

    var sStyle = "";
    if (tbpl.iId === iClubUser) {
      sStyle = "font-weight:bold";
    }

    var k = i + 1;

    var sPlaceLast = "-";
    var sColor = "black";
    for (var iLast = 0; iLast < ltTbl[1].length; iLast++) {
      if (ltTbl[1][iLast].iId === tbpl.iId) {
        if (i !== iLast) {
          sPlaceLast = (iLast + 1).toString();
          if (i > iLast) {
            sColor = "red";
          } else {
            sColor = "green";
          }
        }
        break;
      }
    }

    sBox += '<tr style=' + sStyle + '>';
    sBox += '<td class="first"><b>' + k + '</b></td>';
    //sBox += '<td><img src="http://mediadb.kicker.de/library/images/tendenz-hold.png" alt="" /></td>';
    sBox += '<td style="color: ' + sColor + '">' + sPlaceLast + '</td>';
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

function actionDrawTeams(ltGamedata, iMd) {
  var divDrawTeams = $("#tableDivTeams");
  divDrawTeams.html('');
  result = drawTeams(ltGamedata, iMd);
  divDrawTeams.html(result).show();
}

function drawTeams(ltGamedata, iMd) {
  var sBox = '';

  sBox += '<table id="tableLeagueTeams" border="0" cellpadding="2" style="width: 100%">';
  sBox += '<tr>';
  sBox += '  <th colspan="1">Anstoß</th>';
  sBox += '  <th style="text-align:right">Heim</th>';
  sBox += '  <th style="text-align:center">&nbsp;</th>';
  sBox += '  <th style="text-align:left">Auswärts</th>';
  sBox += '  <th style="text-align:center">Erg.</th>';
  sBox += '</tr>';

  var sDateDay  = $('#idDtDay' + iMd).data('name');
  var sDateHour = $('#idDtHour' + iMd).data('name');
  var iClubUser = $('#idClubUser').data('name');
  for (var i = 0; i < ltGamedata.length; i++) {
    gd = ltGamedata[i];
    var sClubNameH = $('#idClubNames' + gd.team[0].iTeamId).data('name');
    var sClubNameA = $('#idClubNames' + gd.team[1].iTeamId).data('name');

    var sStyle = "";
    if (gd.team[0].iTeamId === iClubUser || gd.team[1].iTeamId === iClubUser) {
      sStyle = "font-weight:bold";
    }

    sBox += '<tr style=' + sStyle + '>';
    sBox += '  <td>' + sDateDay + '&nbsp;' + sDateHour + '</td>';

    sBox += '<td align="right"><a href="/Member/ClubDetails?iClub=__id" target="_blank">__name</a></td>)'.replace("__name", sClubNameH).replace("__id", gd.team[0].iTeamId) + '</td>';
    sBox += '<td align="center">&nbsp;-&nbsp;</td>';
    sBox += '<td align="left"><a href="/Member/ClubDetails?iClub=__id" target="_blank">__name</a></td>)'.replace("__name", sClubNameA).replace("__id", gd.team[1].iTeamId) + '</td>';

    if (gd.team[0].iGoals + gd.team[1].iGoals >= 0) {
      sBox += '<td align="center">' + gd.team[0].iGoals + ':' + gd.team[1].iGoals + '&nbsp;(' + gd.team[0].iGoalsHt + ':' + gd.team[1].iGoalsHt + ')</td>';
    } else {
      sBox += '<td align="center">-</td>';
    }
    sBox += '</tr>';
  }
  sBox += '</table>';

  return sBox;
}
