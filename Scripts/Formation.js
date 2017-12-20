function drawAufstellung(iFormation) {
  var drawFormation = $("#drawFormation");
  //alert("Formation: " + iFormation);
  // respond to the change event in here
  $.ajax({
    cache: false,
    url: "/Member/CkAufstellungFormation",
    type: "GET",
    data: { iF: iFormation },
    success: function (dataFormation) {
      if (dataFormation) {  // check if data is defined
        var result = "";
        var i = 0;
        var sSt = '';

        drawFormation.html('');
        $.each(dataFormation, function (iFormation, player) {
          /*
          $.ajax({
            cache: false,
            url: "/Member/GetPlayerStrength",
            type: "GET",
            data: { iPlayer: player.iPlID },
            success: function (fPlSt) {
              alert("Test B");
              sSt = fPlSt;
            },
            error: function () {
              alert("Test C");
            }
          });
          */

          result += getBoxFormation(player, sSt);
          i = i + 1;
          return (i !== 11);
        });

        //alert("drawFormation A" + iFormation);
        drawFormation/*.hide()*/.html(result).fadeIn('slow')/*.show()*/;
        //alert("drawFormation B" + iFormation);
      } else {
        alert("data hasn't worked!");
      }
    }
  });
}

function getBoxFormation(player, sStrength) {
  var iTop = 100 - (((100 * player.ptPosDefault.X) / (122 / 2)) - 0);
  var iLeft = (((100 * player.ptPosDefault.Y) / 50) - 10);
  var iLeft2 = iLeft + 5;
  var sName = player.sName;
  var sNameSplit = sName.split(' ');
  if (sNameSplit.length > 1) sName = sNameSplit[0][0] + ". " + sNameSplit[sNameSplit.length - 1];
  var sPos = ["", "TW", "IV", "LV", "RV", "DM", "LM", "RM", "OM", "LA", "RA", "ST", "LIB", "OLV", "ORV", "ZM", "", "", "", "", "", "HS"];

  var sBox = "";
  sBox += '<div style="position: absolute; width: 5%; height: 6%; top: ' + iTop + '%; left: ' + iLeft + '%; background-color: black">' +
      '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 300%; color: white">' + player.iNr + '</h2>' +
    '</div>' +
    '<div style="position: absolute; width: 16%; height: 6%; top: ' + iTop + '%; left: ' + iLeft2 + '%; background-color: white; border: 2px solid black">' +
      '<div style="position: absolute; width: 100%; height: 65%; top: 0%; left: 0%; background-color: white; border: 1px solid black; word-break: break-word">' +
        '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 150%; color: black">' + sName + '</h2>' +
      '</div>' +
      '<div style="position: absolute; width: 25%; height: 35%; top: 65%; left: 0%; background-color: white; border: 1px solid black">' +
        '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 80%; color: black">' + sPos[player.iPos] + '</h2>' +
      '</div>' +
      '<div style="position: absolute; width: 25%; height: 35%; top: 65%; left: 25%; background-color: white; border: 1px solid black">' +
        '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 80%; color: black">' + sStrength + '</h2>' +
      '</div>' +
      '<div style="position: absolute; width: 25%; height: 35%; top: 65%; left: 50%; background-color: white; border: 1px solid black">' +
        '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 80%; color: black">X: ' + player.ptPosDefault.Y + '</h2>' +
      '</div>' +
      '<div style="position: absolute; width: 25%; height: 35%; top: 65%; left: 75%; background-color: white; border: 1px solid black">' +
        '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 80%; color: black">Y: ' + player.ptPosDefault.X + '</h2>' +
      '</div>' +
    '</div>';

  return sBox;
}

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
