function changeLand(iDivision) {
  var iSeason = $('#ddlSeason').val();
  var iLand = $('#ddlLand').val();

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
          setLeague2(iDivision);
        }
      });
    }
  });

  plotLeaguePlaceGraph(iSeason);
}

function setLeague2(iDivision) {
  var iSeason = $('#ddlSeason').val();
  var iLand = $('#ddlLand').val();
  var iMd = $('#ddlMatchday').val();
  var divDrawTable = $("#tableDivLeague");

  var iHA = 0;
  if (document.getElementById("rbTableH").checked) {
    iHA = 1;
  } else if (document.getElementById("rbTableA").checked) {
    iHA = 2;
  }

  divDrawTable.html('');

  $.ajax({
    url: '/Member/setLeague',
    type: "GET",
    dataType: "JSON",
    data: { iSeason: iSeason, iLand: iLand, iDivision: iDivision, iMatchday: iMd, iHA: iHA },
    success: function (sTable) {
      var sBox = drawTable(sTable);
      divDrawTable.html(sBox).show();

      $.ajax({
        url: '/Member/setLeagueTeams',
        type: "GET",
        dataType: "JSON",
        data: { iSeason: iSeason, iLand: iLand, iDivision: iDivision, iMatchday: iMd },
        success: function (sTeams) {
          actionDrawTeams(sTeams);
        }
      });
    }
  });

  if (oTableScorer) {
    oTableScorer.ajax.reload();
  } else {
    oTableScorer = setTableScorer($('#tblLeagueScorer'), 1, document.getElementById('ddlLand'), 0);
  }
}

function setTableScorer(tbl, iGameType, ddlLand, iDivision) {
  return tbl.DataTable({
    "ajax": {
      "url": '/Member/GetScorerTable',
      "type": 'GET',
      "dataType": "JSON",
      "data": function (d) {
        d.iGameType = iGameType;
        d.iLand = ddlLand.value;
        d.iDivision = iDivision;
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

function drawTable(sTable) {
  var sBox = '';

  if (!sTable) {
    return sBox;
  }

  sBox += "<table id=\"tableLeague\" style=\"width: 100%\">";
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
