function getScorerTable(sText) {
  var sBox = '';

  sBox += '<h4>Torschützen</h4>';
  sBox += '<table class="tStat" style="width: auto" border="0" cellpadding="2" summary="Scorer">';
  sBox += '<tr>';
  sBox += '<th>#</th>';
  sBox += '<th>Spieler</th>';
  sBox += '<th>Verein</th>';
  sBox += '<th style="text-align:right">Tore</th>';
  sBox += '<th style="text-align:right">Vorl.</th>';
  sBox += '<th style="text-align:right">Scor.</th>';
  sBox += '</tr>';

  sBox += sText;

  sBox += '</table>';

  return sBox;
}

function setTableScorer(parent, iGameType, ddlLand, ddlDivision) {
  parent.innerText = "";

  var header = document.createElement("h4");
  header.innerText = "Torschützen";
  parent.appendChild(header);

  var tbl = document.createElement("table");
  tbl.style.position = "relative";
  tbl.cellPadding = 0;
  tbl.border = 0;
  tbl.className = "display responsive nowrap compact";

  var thead = document.createElement("thead");
  var tr = document.createElement("tr");

  var th0 = document.createElement("th");
  th0.innerText = "#";
  tr.appendChild(th0);
  var th1 = document.createElement("th");
  th1.innerText = "id";
  tr.appendChild(th1);
  var th2 = document.createElement("th");
  th2.innerText = "Spieler";
  tr.appendChild(th2);
  var th3 = document.createElement("th");
  th3.innerText = "Verein";
  tr.appendChild(th3);
  var th4 = document.createElement("th");
  th4.innerText = "T.";
  tr.appendChild(th4);
  var th5 = document.createElement("th");
  th5.innerText = "Vl.";
  tr.appendChild(th5);
  var th6 = document.createElement("th");
  th6.innerText = "Sc.";
  tr.appendChild(th6);
  var th7 = document.createElement("th");
  tr.appendChild(th7);

  thead.appendChild(tr);
  tbl.appendChild(thead);
  tbl.appendChild(document.createElement("tbody"));
  parent.appendChild(tbl);

  return $(tbl).DataTable({
    "ajax": {
      "url": '/Member/GetScorerTable',
      "type": 'GET',
      "dataType": "JSON",
      "data": function (d) {
        var iLand = -1;
        if (ddlLand) {
          iLand = ddlLand.value;
        }

        var iDivision = -1;
        if (ddlDivision) {
          iDivision = ddlDivision.value;
        }

        d.iGameType = iGameType;
        d.iLand = iLand;
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
      "emptyTable": "Noch keine Torschützen"
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
