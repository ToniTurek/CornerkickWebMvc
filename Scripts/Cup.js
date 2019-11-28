function changeLandCup() {
  var iLand = $('#ddlLandCup').val();
  var iSeason = $('#ddlSeasonCup').val();

  $.ajax({
    url: '/Member/CupGetDdlMatchdays',
    type: "GET",
    dataType: "JSON",
    data: { iSaison: iSeason, iLand: iLand },
    success: function (ltMd) {
      $('#ddlMatchdayCup').empty();
      $.each(ltMd, function (i, p) {
        ltS = p.split(';');
        $('#ddlMatchdayCup').append($('<option></option>').val(ltS[0]).html(ltS[1]));
      });

      $.ajax({
        url: '/Member/CupGetMatchday',
        type: "GET",
        dataType: "JSON",
        data: { iSaison: iSeason, iLand: iLand },
        success: function (iMd) {
          document.getElementById("ddlMatchdayCup").value = iMd;
          setCup2(iSeason);
        }
      });
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

  $.ajax({
    url: '/Member/LeagueCupGetScorer',
    type: "GET",
    dataType: "JSON",
    data: { iGameType: 2, iLand: iLand, iDivision: -1 },
    success: function (sText) {
      if (sText) {
        $("#divCupScorer").html(getScorerTable(sText)).show();
      }
    }
  });
}

function actionDrawTeams(sTable) {
  var divDrawCupTeams = $("#tableDivCupTeams");
  divDrawCupTeams.html('');
  result = drawTeams(sTable);
  divDrawCupTeams.html(result).show();
}
