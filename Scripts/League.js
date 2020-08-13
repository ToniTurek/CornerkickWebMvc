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
    oTableLeague = getTableDatatable(document.getElementById("divTableLeague"), 1, ddlSeason, ddlLand, ddlDivision, ddlMatchday, null, rbTableH, rbTableA, 1, 4, 8, -1);
  }

  // Scorer table
  if (oTableScorer) {
    oTableScorer.ajax.reload();
  } else {
    oTableScorer = setTableScorer(document.getElementById("divLeagueScorer"), 1, ddlLand, ddlDivision);
  }
}

function getTableDatatable(parent, iGameType, ddlSeason, ddlLand, ddlDivision, ddlMatchday, ddlGroup, rbTableH, rbTableA, iColor1, iColor2, iColor3, iColor4) {
  var tbl = document.createElement("table");
  tbl.style.position = "relative";
  tbl.cellPadding = 0;
  tbl.border = 0;
  tbl.className = "display responsive nowrap compact";

  var thead = document.createElement("thead");
  var tr = document.createElement("tr");

  var th0 = document.createElement("th");
  th0.innerText = "id";
  tr.appendChild(th0);
  var th1 = document.createElement("th");
  th1.innerText = "#";
  tr.appendChild(th1);
  var th2 = document.createElement("th");
  th2.innerText = "";
  tr.appendChild(th2);
  var th3 = document.createElement("th");
  th3.innerText = "";
  tr.appendChild(th3);
  var th4 = document.createElement("th");
  th4.innerText = "Verein";
  tr.appendChild(th4);
  var th5 = document.createElement("th");
  th5.innerText = "Sp";
  tr.appendChild(th5);
  var th6 = document.createElement("th");
  th6.innerText = "g";
  tr.appendChild(th6);
  var th7 = document.createElement("th");
  th7.innerText = "u";
  tr.appendChild(th7);
  var th8 = document.createElement("th");
  th8.innerText = "v";
  tr.appendChild(th8);
  var th9 = document.createElement("th");
  th9.innerText = "Tore";
  tr.appendChild(th9);
  var th10 = document.createElement("th");
  th10.innerText = "+/-";
  tr.appendChild(th10);
  var th11 = document.createElement("th");
  th11.innerText = "Pkte.";
  tr.appendChild(th11);
  var th12 = document.createElement("th");
  th12.innerText = "bgcl";
  tr.appendChild(th12);
  var th13 = document.createElement("th");
  th13.innerText = "bold";
  tr.appendChild(th13);

  thead.appendChild(tr);
  tbl.appendChild(thead);
  tbl.appendChild(document.createElement("tbody"));
  parent.appendChild(tbl);

  return $(tbl).DataTable({
    "ajax": {
      "url": '/Member/getTableDatatable',
      "type": 'GET',
      "dataType": "JSON",
      "data": function (d) {
        var iLand = -1;
        if (ddlLand) { iLand = ddlLand.value; }

        var iDivision = -1;
        if (ddlDivision) { iDivision = ddlDivision.value; }

        var iGroup = -1;
        if (ddlGroup) { iGroup = ddlGroup.value; }

        var bH = false;
        if (rbTableH) { bH = rbTableH.checked; }

        var bA = false;
        if (rbTableA) { bA = rbTableA.checked; }

        d.iSeason = ddlSeason.value;
        d.iType = iGameType;
        d.iLand = iLand;
        d.iDivision = iDivision;
        d.iMatchday = ddlMatchday.value;
        d.iGroup = iGroup;
        d.bH = bH;
        d.bA = bA;
        d.iColor1 = iColor1;
        d.iColor2 = iColor2;
        d.iColor3 = iColor3;
        d.iColor4 = iColor4;
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

function changeMatchday(iPrePost, ddlMatchday, exeFunction) {
  if (!ddlMatchday) {
    return;
  }

  var iMdNew = 0;
  if (iPrePost < 0 && parseInt(ddlMatchday.value) > 0) {
    iMdNew = parseInt(ddlMatchday.value) - 1;
  }

  if (iPrePost > 0) {
    iMdNew = parseInt(ddlMatchday.value) + 1;
  } else {
    iMdNew = parseInt(ddlMatchday.value) - 1;
  }

  if (ddlMatchday.innerHTML.indexOf('value="' + iMdNew.toString() + '"') < 0) {
    return;
  }

  ddlMatchday.display = "none";
  ddlMatchday.value = iMdNew.toString();
  ddlMatchday.display = "block";

  exeFunction();
}
