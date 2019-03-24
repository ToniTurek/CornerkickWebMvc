﻿async function drawAufstellung(iFormation, iSelectedPlayer, onLoad) {
  var drawFormation = $("#drawFormation");
  var textteamaverage = $("#average");
  var divTacticOrientation    = document.getElementById("divTacticOrientation");
  var divTacticIndOrientation = document.getElementById("divTacticIndOrientation");

  var fWindowWidth = $(window).width();
  var bMobile = fWindowWidth < 960;

  // respond to the change event in here
  $.ajax({
    cache: false,
    url: "/Member/CkAufstellungFormation",
    type: "GET",
    data: { iF: iFormation - 1, iSP: iSelectedPlayer - 1, bMobile: bMobile },
    success: function (teamData) {
      if (teamData) {  // check if data is defined
        var result = "";
        var i = 0;

        drawFormation.html('');

        if (document.getElementById("rbDefence").checked) {
          $.each(teamData.ltPlayer, function (iFormation, player) {
            if (player.iIxManMarking >= 0) {
              plOpp = teamData.ltPlayerOpp[player.iIxManMarking];

              var iPos = convertPosToPix(player.ptPosDefault.Y, 122 - player.ptPosDefault.X, -plOpp.ptPosDefault.Y, plOpp.ptPosDefault.X, document.getElementById("drawFormation"), false);
              result += drawLine(iPos[0], iPos[1], iPos[2], iPos[3], "orange", 2);
            }

            result += getBoxFormation(player, i, teamData.ltPlayerAveSkill[i], false, iSelectedPlayer - 1, teamData.ltPlayerPos[i], bMobile);

            i = i + 1;
            return i !== 11;
          });

          // opponent player
          $.each(teamData.ltPlayerOpp, function (iFormation, player) {
            result += getBoxFormation(player, i, "", true, iSelectedPlayer - 1, teamData.ltPlayerOppPos[i], bMobile);

            i = i + 1;
            return i !== 11;
          });

          // hide orientation slider on start
          divTacticOrientation   .style.display = 'none';
          divTacticIndOrientation.style.display = 'none';
        } else {
          // print radius of action
          if (iSelectedPlayer > 0 && iSelectedPlayer < 12) {
            result += teamData.sDivRoa;

            if (onLoad && teamData.fIndOrientationMinMax[1] > teamData.fIndOrientationMinMax[0]) {
              $("#slider-IndOrientation").slider("value", parseInt(teamData.fIndOrientation * 100));

              divTacticIndOrientation.style.top = ((1.0 - teamData.fIndOrientationMinMax[1]) * 100).toString() + '%';
              divTacticIndOrientation.style.height = ((teamData.fIndOrientationMinMax[1] - teamData.fIndOrientationMinMax[0]) * 100).toString() + '%';
              divTacticIndOrientation.style.display = 'block';
            }
          } else {
            divTacticIndOrientation.style.display = 'none';
          }

          result += teamData.sDivPlayer;

          // show orientation slider on start
          divTacticOrientation.style.display = 'block';
        }

        if (iSelectedPlayer > 0 && iSelectedPlayer < 12) {
          if (document.getElementById("rbDefence").checked) {
            result += getBoxMovePlayer(teamData.ltPlayer[iSelectedPlayer - 1]);
          }
        }

        drawFormation/*.hide()*/.html(result).fadeIn('slow')/*.show()*/;
      } else {
        alert("data hasn't worked!");
      }
    }
  });

  $.ajax({
    cache: false,
    url: "/Member/GetPlayerStrengthAgeAve",
    type: "GET",
    data: { },
    success: function (fStrengthAgeAve) {
      textteamaverage.html('');
      textteamaverage.html("Durchschnittsstärke (-alter): " + fStrengthAgeAve[0].toFixed(2) + " (" + fStrengthAgeAve[1].toFixed(1) + ")");
    }
  });
}

function getBoxFormation(player, i, sStrength, bOpponentTeam, iSelectedPlayer, iPos, bMobile) {
  if (!iPos) {
    iPos = 0;
  }

  var iTop  = 100 - ((100 * player.ptPosDefault.X) / 122);
  var iLeft = (100 * (player.ptPosDefault.Y + 25)) / 50;
  if (bOpponentTeam) {
    iTop  = 100 - iTop;
    iLeft = 100 - iLeft;
  }

  iTop  = iTop  -  1.75;
  iLeft = iLeft - 13.00;

  var sName = player.sName;
  var sNameSplit = sName.split(' ');
  var sSurname = sName;
  if (sNameSplit.length > 1) {
    sSurname = sNameSplit[sNameSplit.length - 1];
    sName = sNameSplit[0][0] + ". " + sSurname;
  }
  if (sName.length > 11) sName = sName.substring(0, 11);
  var sPos = ["", "TW", "IV", "LV", "RV", "DM", "LM", "RM", "OM", "LA", "RA", "ST", "LIB", "OLV", "ORV", "ZM", "", "", "", "", "", "HS"];

  var color  = "white";
  var color2 = "black";
  if (bOpponentTeam) {
    color = "lightgray";
  } else if (player.bYellowCard) {
    color = "yellow";
  } else if (iSelectedPlayer >= 0 && i !== iSelectedPlayer) {
    color  = "rgba(255, 255, 255, .5)";
    color2 = "rgba(0, 0, 0, .5)";
  }

  var sSelectPlayer = "";
  var sZIndex = "";
  if (!bOpponentTeam) {
    sSelectPlayer = " onclick=\"javascript: selectPlayer(" + i.toString() + ")\"; ontouchstart=\"selectPlayer(" + i.toString() + ")\"";
    if (i === iSelectedPlayer) {
      sZIndex = ";z-index:99";
    }
  } else if (iSelectedPlayer >= 0) {
    sSelectPlayer = " onclick=\"javascript: selectPlayerOpp(" + i.toString() + ")\"; ontouchstart=\"selectPlayerOpp(" + i.toString() + ")\"";
  }

  var sBox = "";

  //sBox += '<a href="">';

  var fWidth = 26;
  var iTextSize = 150;
  if (bMobile) {
    iTextSize = 100;
    sName = sSurname;
  }

  sBox +=
    '<div id="divPlayerBox_' + i.toString() + '" class="divPlayerBox"' + sSelectPlayer + ' style="position: absolute; width: ' + fWidth.toString() + '%; min-width: 100px; height: 3.5%; min-height: 26px; top: ' + iTop + '%; left: ' + iLeft + '%; cursor: pointer; -webkit-box-shadow: 0px 0px 4px 4px rgba(0, 0, 0, .3); box-shadow: 0px 0px 4px 4px rgba(0, 0, 0, .3)' + sZIndex + '">' +
      '<div style="position: absolute; width: 25%; height: 100%; background-color: ' + color2 + '">' +
        '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + (iTextSize * 1.6).toString() + '%; color: white">' + player.iNr + '</h2>' +
      '</div>' +
      '<div style="position: absolute; width: 75%; height: 100%; left: 25%; border: 2px solid black">' +
        '<div style="position: absolute; width: 100%; height: 65%; top: 0%; left: 0%; background-color: ' + color + '; word-break: break-word">' +
          '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + iTextSize.toString() + '%; color: black">' + sName + '</h2>' +
        '</div>' +
        '<div style="position: absolute; width: 25%; height: 35%; min-height: 8px; bottom: 0px; left: 0%; background-color: ' + color + '">' +
          '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + (iTextSize * 0.6).toString() + '%; color: black">' + sPos[iPos] + '</h2>' +
        '</div>' +
        '<div style="position: absolute; width: 25%; height: 35%; min-height: 8px; bottom: 0px; left: 25%; background-color: ' + color + '">' +
          '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + (iTextSize * 0.6).toString() + '%; color: black">' + sStrength + '</h2>' +
        '</div>' +
        '<div style="position: absolute; width: 50%; height: 35%; min-height: 8px; bottom: 0px; left: 50%; background-color: ' + color + '">' +
          '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + (iTextSize * 0.6).toString() + '%; color: black">' + player.ptPosDefault.Y.toString() + '/' + player.ptPosDefault.X.toString() + '</h2>' +
        '</div>' +
      '</div>' +
    '</div>';

    //sBox += '</a>';

  return sBox;
}

function getBoxMovePlayer(player) {
  // Total box width: 18%
  var iTop = (100 - ((100 * player.ptPosDefault.X) / 122)) - 1.75;
  var iLeft  = (100 *       (player.ptPosDefault.Y + 25))  / 50;
  var iRight = (100 * (50 - (player.ptPosDefault.Y + 25))) / 50;

  var sBox = "";

  sBox +=
    '<div style="position: absolute; width: 10%; min-width: 60px; top: ' + (iTop - 3) + '%; left: ' + (iLeft + 0 - 5) + '%">' +
      '<img id="img_arrow_1" onclick="javascript:movePlayer(1)" style="position: relative; width: 100%; cursor: pointer" src="/Content/Images/arrow_up.png"/>' +
    '</div>';

  sBox +=
    '<div style="position: absolute; width:  4%; min-width: 24px; top: ' + (iTop - 1.5) + '%; left: ' + (iLeft + 13 + 1) + '%">' +
      '<img id="img_arrow_2" onclick="javascript:movePlayer(2)" style="position: relative; width: 100%; cursor: pointer" src="/Content/Images/arrow_right.png"/>' +
    '</div>';

  sBox +=
    '<div style="position: absolute; width: 10%; min-width: 60px; top: ' + (iTop + 4) + '%; left: ' + (iLeft + 0 - 5) + '%">' +
      '<img id="img_arrow_3" onclick="javascript:movePlayer(3)" style="position: relative; width: 100%; cursor: pointer" src="/Content/Images/arrow_down.png"/>' +
    '</div>';

  sBox +=
    '<div style="position: absolute; width:  4%; min-width: 24px; top: ' + (iTop - 1.5) + '%; right: ' + (iRight + 13 + 1) + '%">' +
      '<img id="img_arrow_4" onclick="javascript:movePlayer(4)" style="position: relative; width: 100%; cursor: pointer" src="/Content/Images/arrow_left.png"/>' +
    '</div>';

  return sBox;
}

function getSubstitutionList() {
  $.ajax({
    url: '/Member/GetSubstitutionList',
    type: "GET",
    dataType: "JSON",
    success: function (ltsSubstitution) {
      if (ltsSubstitution) {
        if (ltsSubstitution.length > 0) {
          drawSubstitutionList(ltsSubstitution);
        }
      }
      return true;
    },
    error: function () {
      alert("ERROR in getSubstitutionList");
      return false;
    }
  });
}

function drawSubstitutionList(ltsSubstitution) {
  var divDrawSubstitutions = $("#drawSubstitutions");
  divDrawSubstitutions.html('');

  var sBox = "";
  sBox += '<table style="width: 50 %; min-width: 300px" border="1" cellpadding="3">';
  sBox += '<thead>';
  sBox += '<tr>';
  sBox += '<th style="text-align:right">#</th>';
  sBox += '<th style="text-align:center">aus</th>';
  sBox += '<th style="text-align:center">ein</th>';
  sBox += '</tr>';
  sBox += '</thead>';
  sBox += '<tbody>';
  for (var i = 0; i < ltsSubstitution.length; i++) {
    var sSub = ltsSubstitution[i]; 
    sBox += '<tr>';
    sBox += '<td align="right">' + (i + 1) + '</td>';
    sBox += '<td align="center">' + sSub[0] + '</td>';
    sBox += '<td align="center">' + sSub[1] + '</td>';
    sBox += '</tr>';
  }
  sBox += '</tbody>';
  sBox += '</table>';

  divDrawSubstitutions.html(sBox);
}

/*
function actionDrawDatatable(ltLV) {
  var drawDatatable = $("#tableTeam2");
  drawDatatable.html('');
  result = drawAufstellung2(ltLV);
  alert(result);
  drawDatatable.html(result).show();
}

function drawAufstellung2(ltLV) {
  var sBox = '';

  sBox += '<table id="tableTeam2" cellspacing="0" style="width: auto" class="display responsive nowrap">';
  sBox += ' <thead>';
  sBox += '     <tr>';
  sBox += '      <th>ID</th>';
  sBox += '      <th></th>';
  sBox += '      <th>Nr</th>';
  sBox += '      <th>Name</th>';
  sBox += '      <th>Position</th>';
  sBox += '      <th>Stärke</th>';
  sBox += '      <th>Kondi.</th>';
  sBox += '      <th>Frische</th>';
  sBox += '      <th>Moral</th>';
  sBox += '      <th>Erf.</th>';
  sBox += '      <th>Marktwert [€]</th>';
  sBox += '      <th>Gehalt [€]</th>';
  sBox += '      <th>Lz</th>';
  sBox += '      <th>Nat.</th>';
  sBox += '      <th>Form</th>';
  sBox += '      <th>Alter</th>';
  sBox += '    </tr>';
  sBox += '  </thead>';
  sBox += '<tbody>;';

  for (var i = 0; i < ltLV.length; i++) {
    var k = i + 1;
    var iPlID = 0;
    //int.TryParse(ltLV[i][0], out iPlID);
    sBox += '<tr>';
    sBox += '  <td align="center" id="@iPlID">' + k + '</td>';
    sBox += '  <td>';
    sBox += '    <img src="~/Content/Images/10.jpg" width="20" height="26" class="playerPhoto" />';
    sBox += '     <input type="hidden" class="playerId" value="' + iPlID + '" />';
    sBox += '   </td>';
    
    for (var j = 1; j < ltLV[i].length - 1; j++) {
      if (j === 2) {
        sBox += '<td align="center">@Html.ActionLink(' + ltLV[i][j] + ', "PlayerDetails", "Member", new {i = ' + iPlID + '}, new {target = "_blank"})</td>';
      }
      else {
        sBox += '<td align="center">' + ltLV[i][j] + '</td>';
      }
    }
    sBox += '</tr>';
  }
  sBox += '</tbody>';
  sBox += '</table>';

  return sBox;
}
*/
