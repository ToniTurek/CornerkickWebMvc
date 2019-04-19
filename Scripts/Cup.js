function changeLand(iSaison) {
  var iLand = $('#ddlLandCup').val();

  $.ajax({
    url: '/Member/CupGetDdlMatchdays',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iLand: iLand },
    success: function (ltMd) {
      $('#ddlMatchdayCup').empty();
      $.each(ltMd, function (i, p) {
        ltS = p.split(';');
        $('#ddlMatchdayCup').append($('<option></option>').val(ltS[0]).html(ltS[1]));
      });

      setCup2(iSaison);
    }
  });
}

function setCup2(iSaison) {
  var iLand = $('#ddlLandCup').val();
  var iMd = $('#ddlMatchdayCup').val();

  $.ajax({
    url: '/Member/setCup',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSaison, iLand: iLand, iMatchday: iMd },
    success: function (sTable) {
      actionDrawTeams(sTable);
    }
  });
}

function actionDrawTeams(sTable) {
  var divDrawCupTeams = $("#tableDivCupTeams");
  divDrawCupTeams.html('');
  result = drawCupTeams(sTable);
  divDrawCupTeams.html(result).show();
}

function drawCupTeams(sTable) {
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
  sBox += sTable;
  sBox += '</table>';

  return sBox;
}
