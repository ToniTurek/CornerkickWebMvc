function changeLand() {
  var iSeason = $('#ddlSeason').val();
  var iLand = $('#ddlLand').val();
  var iDivision = $('#ddlDivision').val();

  $.ajax({
    url: '/Member/getDdlMatchdays',
    type: "GET",
    dataType: "JSON",
    data: { iSeason: iSeason, iLand: iLand, iDivision: iDivision },
    success: function (ltMd) {
      $('#ddlMatchday').empty();
      $.each(ltMd, function (i, p) {
        $('#ddlMatchday').append($('<option></option>').val(p).html(p));
      });

      $.ajax({
        url: '/Member/LeagueGetMatchday',
        type: "GET",
        dataType: "JSON",
        data: { iSeason: iSeason, iLand: iLand, iDivision: iDivision },
        success: function (iMd) {
          document.getElementById("ddlMatchday").value = iMd;
          setLeague2();
        }
      });
    }
  });

  plotLeaguePlaceGraph(iSeason);
}

function setLeague2() {
  var ddlSeason = document.getElementById("ddlSeason");
  var ddlLand = document.getElementById("ddlLand");
  var ddlDivision = document.getElementById("ddlDivision");
  var ddlMatchday = document.getElementById("ddlMatchday");
  var rbTableH = document.getElementById("rbTableH");
  var rbTableA = document.getElementById("rbTableA");

  // Teams
  $.ajax({
    url: '/Member/setLeagueTeams',
    type: "GET",
    dataType: "JSON",
    data: { iSeason: ddlSeason.value, iLand: ddlLand.value, iDivision: ddlDivision.value, iMatchday: ddlMatchday.value },
    success: function (sTeams) {
      actionDrawTeams(sTeams);
    }
  });

  // Table
  if (oTableLeague) {
    oTableLeague.ajax.reload();
  } else {
    oTableLeague = getTableDatatable($("#tblLeague"), ddlSeason, ddlLand, ddlDivision, ddlMatchday, rbTableH, rbTableA);
  }

  // Scorer table
  if (oTableScorer) {
    oTableScorer.ajax.reload();
  } else {
    oTableScorer = setTableScorer($('#tblLeagueScorer'), 1, ddlLand, ddlDivision);
  }
}

function getTableDatatable(tblLeague, ddlSeason, ddlLand, ddlDivision, ddlMatchday, rbTableH, rbTableA) {
  return tblLeague.DataTable({
    "ajax": {
      "url": '/Member/getTableDatatable',
      "type": 'GET',
      "dataType": "JSON",
      "data": function (d) {
        d.iSeason = ddlSeason.value;
        d.iType = 1;
        d.iLand = ddlLand.value;
        d.iDivision = ddlDivision.value;
        d.iMatchday = ddlMatchday.value;
        d.iGroup = -1;
        d.bH = rbTableH.checked;
        d.bA = rbTableA.checked;
        d.iColor1 = 1;
        d.iColor2 = 4;
        d.iColor3 = 8;
        d.iColor4 = -1;
      },
      "cache": false,
      "contentType": "application/json; charset=utf-8"
    },
    "columns": [
      { "data": "iId" },
      { "data": "iPl" },
      {
        "data": "iPlLast",
        "render": function (iPlLast, type, row) {
          if (iPlLast === 0) {
            return "-";
          }
          return iPlLast;
        }
      },
      { "data": "sEmblem" },
      { "data": "sClubName" },
      { "data": "iGames" },
      { "data": "iWin" },
      { "data": "iDraw" },
      { "data": "iDefeat" },
      { "data": "sGoals" },
      { "data": "iGoalsDiff" },
      { "data": "iPoints" },
      { "data": "sBgColor" },
      { "data": "bBold" }
    ],
    "paging": false,
    "info": false,
    "searching": false,
    "order": [[1, "asc"]],
    "language": {
      "emptyTable": "keine Vereine"
    },
    "columnDefs": [
      {
        "targets": [0, 12, 13],
        "visible": false,
        "orderable": false,
        "searchable": false
      },
      {
        "targets": [2, 9],
        "orderable": false,
        "className": "dt-center"
      },
      {
        "targets": [3, 11],
        "orderable": false,
        "className": "dt-right"
      },
      {
        "targets": [1, 5, 6, 7, 8, 10],
        "className": "dt-right"
      }
    ],
    "fnRowCallback": function (nRow, aData, iDisplayIndex) {
      $('td', nRow).eq(0).css("background-color", aData.sBgColor);

      if (aData.bBold) {
        $('td', nRow).css("font-weight", "bold");
      }

      if (aData.iPlLast > 0) {
        if (aData.iPlLast < aData.iPl) {
          $('td', nRow).eq(1).css("color", "red");
        } else if (aData.iPlLast > aData.iPl) {
          $('td', nRow).eq(1).css("color", "green");
        }
      }
    }
  });
}

function setTableScorer(tbl, iGameType, ddlLand, ddlDivision) {
  return tbl.DataTable({
    "ajax": {
      "url": '/Member/GetScorerTable',
      "type": 'GET',
      "dataType": "JSON",
      "data": function (d) {
        d.iGameType = iGameType;
        d.iLand = ddlLand.value;
        d.iDivision = ddlDivision.value;
      },
      "cache": false,
      "contentType": "application/json; charset=utf-8"
    },
    "columns": [
      { "data": "iIx" },
      { "data": "iId" },
      { "data": "sPlName" },
      { "data": "sClubName" },
      { "data": "iGoals" },
      { "data": "iAssists" },
      { "data": "iScorer" },
      { "data": "bBold" }
    ],
    "paging": false,
    "info": false,
    "searching": false,
    "order": [[4, "desc"]],
    "language": {
      "emptyTable": "keine Torschützen"
    },
    "columnDefs": [
      {
        "targets": [1, 7],
        "visible": false,
        "orderable": false,
        "searchable": false
      },
      {
        "targets": [0, 2],
        "orderable": false,
        "searchable": false
      },
      {
        "targets": [0, 4, 5, 6],
        "className": "dt-right"
      }
    ],
    "fnRowCallback": function (nRow, aData, iDisplayIndex) {
      if (aData.bBold) {
        $('td', nRow).css("font-weight", "bold");
      }
    }
  });
}

function actionDrawTeams(sTeams) {
  var divDrawTeams = $("#tableDivTeams");
  divDrawTeams.html('');
  result = drawTeams(sTeams);
  divDrawTeams.html(result).show();
}

function drawTeams(sTeams) {
  var sBox = '';

  if (!sTeams) {
    return sBox;
  }

  if (sTeams.includes('<td>')) {
    sBox += '<h4>Begegnungen</h4>';
    sBox += '<table id="tableTeams" border="0" cellpadding="2" style="width: 100%">';
    sBox += '<tr>';
    sBox += '  <th colspan="1">Anstoß</th>';
    sBox += '  <th style="text-align:right">Heim</th>';
    sBox += '  <th style="text-align:center">&nbsp;</th>';
    sBox += '  <th style="text-align:left">Auswärts</th>';
    sBox += '  <th style="text-align:center">Erg.</th>';
    sBox += sTeams;
    sBox += '</tr>';

    sBox += '</table>';
  } else {
    sBox += '<h4>Teilnehmer</h4>';
    sBox += sTeams;
  }

  return sBox;
}

// DEPRECATED
function drawTable(sTable) {
  var sBox = '';

  if (!sTable) {
    return sBox;
  }

  sBox += "<table id=\"tableLeague\" style=\"width: 100%\" class=\"display responsive nowrap compact\">";
  sBox += "<tr>";
  sBox += "<th colspan=\"2\">Pl.</th>";
  sBox += "<th style=\"text-align:center; width: 3%\">&nbsp;</th>";
  sBox += "<th>Verein</th>";
  sBox += "<th style=\"text-align:right; width: 3%\">&nbsp;</th>";
  sBox += "<th style=\"text-align:right\">Sp.</th>";
  sBox += "<th style=\"text-align:center; width: 3%\">&nbsp;</th>";
  sBox += "<th style=\"text-align:right\">g.</th>";
  sBox += "<th style=\"text-align:right\">u.</th>";
  sBox += "<th style=\"text-align:right\">v.</th>";
  sBox += "<th style=\"text-align:center; width: 3%\">&nbsp;</th>";
  sBox += "<th style=\"text-align:center\">Tore</th>";
  sBox += "<th style=\"text-align:right\">Diff.</th>";
  sBox += "<th style=\"text-align:center; width: 3%\">&nbsp;</th>";
  sBox += "<th style=\"text-align:right\"> Pkte.</th>";
  sBox += "</tr>";
  sBox += sTable;
  sBox += "</table>";

  return sBox;
}
