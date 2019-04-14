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
  var divDrawTable = $("#tableDivLeague");
  divDrawTable.html('');

  $.ajax({
    url: '/Member/setLeague',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iLand: iLand, iDivision: iDivision, iMatchday: iMd },
    success: function (sBox) {
      divDrawTable.html(sBox).show();
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
