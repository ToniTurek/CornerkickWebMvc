async function drawAufstellung(iFormation, iSelectedPlayer, onLoad) {
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
          // Defence view
          $.each(teamData.ltPlayer, function (iPl, player) {
            if (teamData.ltPlayerOpp !== null) {
              if (player.iIxManMarking >= 0 && player.iIxManMarking < teamData.ltPlayerOpp.length) {
                plOpp = teamData.ltPlayerOpp[player.iIxManMarking];

                var iPos = convertPosToPix(teamData.formation.ptPos[iPl].Y, 122 - teamData.formation.ptPos[iPl].X, -teamData.formationOpp.ptPos[player.iIxManMarking].Y, teamData.formationOpp.ptPos[player.iIxManMarking].X, document.getElementById("drawFormation"), false);
                result += drawLine(iPos[0], iPos[1], iPos[2], iPos[3], "orange", 2);
              }
            }

            var sNo = player.iNr.toString();
            if (teamData.bNation) {
              sNo = (iPl + 1).toString();
            }

            result += getBoxFormation(iPl, teamData.formation.ptPos[iPl], player.sName, sNo, teamData.ltPlayerAveSkill[iPl], player.bYellowCard, false, iSelectedPlayer - 1, teamData.ltPlayerPos[iPl], bMobile, 1.0, null, null, teamData.ltPlayerNat[iPl], iPl === teamData.iCaptainIx, teamData.ltPlayerSusp[iPl]);

            i = i + 1;
            return i !== 11;
          });

          if (teamData.bOppTeam) {
            if (teamData.iKibitzer === 0) {
              result += '<div style="position: absolute; width: 90%; height: 50%; left: 5%; text-align: center"><p style="position: absolute; top: 40%; width: 100%; color: white; font-size: 150%">Stellen Sie einen <a style="color: white" href="/Member/Personal">Spielbeobachter</a> ein, wenn Sie mehr über Ihren nächsten Gegner erfahren wollen!</p></div>';
            } else if (teamData.ltPlayerOpp) {
              // opponent player
              var j = 0;
              $.each(teamData.ltPlayerOpp, function (iPl, playerOpp) {
                var sPlayerOppName = "";
                var sPlayerOppAveSkill = "";
                var sPlayerOppPos = "";
                if (teamData.iKibitzer > 1) {
                  sPlayerOppName = playerOpp.sName;

                  if (teamData.iKibitzer > 2) {
                    sPlayerOppAveSkill = teamData.ltPlayerOppAveSkill[iPl];
                    sPlayerOppPos = teamData.ltPlayerOppPos[iPl];
                  }
                }

                var sOppNo = playerOpp.iNr.toString();
                if (teamData.bNation) {
                  sOppNo = (iPl + 1).toString();
                }

                result += getBoxFormation(i, teamData.formationOpp.ptPos[iPl], sPlayerOppName, sOppNo, sPlayerOppAveSkill, playerOpp.bYellowCard, true, iSelectedPlayer - 1, sPlayerOppPos, bMobile);

                i = i + 1;
                j = j + 1;
                return j !== 11;
              });

              if (teamData.iKibitzer > 2) {
                var divTeamOppAve = $("#divTeamOppAve");
                sTeamOppAve = '<p style="position: absolute; margin: 4px; background-color: rgb(31, 158, 69); color: white; font-size: 100%">' + teamData.sTeamOppAveSkill + ' (' + teamData.sTeamOppAveAge + ')</p>';
                divTeamOppAve.html(sTeamOppAve).fadeIn('slow');
              }
            }
          } else {
            result += '<div style="position: absolute; width: 90%; height: 50%; left: 5%; text-align: center"><p style="position: absolute; top: 40%; width: 100%; color: white; font-size: 150%">Kein Gegner</p></div>';
          }

          // hide orientation slider on start
          divTacticOrientation   .style.display = 'none';
          divTacticIndOrientation.style.display = 'none';
        } else { // Offence view
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
            result += getBoxMovePlayer(teamData.formation.ptPos[iSelectedPlayer - 1]);
          }
        }

        // Team averages
        textteamaverage.html("Durchschnittsstärke (-alter): " + teamData.sTeamAveSkill + " (" + teamData.sTeamAveAge + ")");

        drawFormation/*.hide()*/.html(result).fadeIn('slow')/*.show()*/;

        for (var iDg = 0; iDg < 11; iDg++) {
          var divPlayerBox = document.getElementById("divPlayerBox_" + iDg.toString());
          dragElement(divPlayerBox);
        }
      } else {
        alert("data hasn't worked!");
      }
    }
  });
}

//function getBoxFormation(player, i, sName, sNo, sStrength, bOpponentTeam, iSelectedPlayer, iPos, bMobile, fScale, sTeamname, sAge, sNat, bCaptain, bSuspended) {
function getBoxFormation(i, ptPos, sName, sNo, sStrength, bYellowCard, bOpponentTeam, iSelectedPlayer, iPos, bMobile, fScale, sTeamname, sAge, sNat, bCaptain, bSuspended)
{
  if (!iPos) {
    iPos = 0;
  }

  if (!fScale) {
    fScale = 1.0;
  }

  var fHeightTot = 122 * fScale;
  var fHeightBox = 3.5 / fScale;

  var iTop  = 100 - ((100 * ptPos.X) / fHeightTot);
  var iLeft = (100 * (ptPos.Y + 25)) / 50;
  if (bOpponentTeam) {
    iTop  = 100 - iTop;
    iLeft = 100 - iLeft;
  }

  iTop  = iTop  - (fHeightBox / 2.0);
  iLeft = iLeft - 13.00;

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
  } else if (bSuspended) {
    color  = "rgba(255, 30, 0, .3)";
    color2 = "rgba(0, 0, 0, .5)";
  } else if (bYellowCard) {
    color = "yellow";
  } else if (iSelectedPlayer >= 0 && i !== iSelectedPlayer) {
    color  = "rgba(255, 255, 255, .5)";
    color2 = "rgba(0, 0, 0, .5)";
  }

  var sSelectPlayer = "";
  var sZIndex = "";
  if (!bOpponentTeam) {
    sSelectPlayer = " onclick=\"javascript: selectPlayer(" + i.toString() + ")\" ontouchstart=\"selectPlayer(" + i.toString() + ")\" data-ix=\"" + i.toString() + "\"";
    if (i === iSelectedPlayer) {
      sZIndex = "; z-index: 98";
    }
  } else if (iSelectedPlayer >= 0) {
    sSelectPlayer = " onclick=\"javascript: selectPlayerOpp(" + i.toString() + ")\" ontouchstart=\"selectPlayerOpp(" + i.toString() + ")\"";
  }

  var fWidth = 26;
  var iTextSize = 150;
  if (bMobile) {
    iTextSize = 100;
    sName = sSurname;
  }

  var sBox = "";

  sBox +=
    '<div class="divPlayerBox" id="divPlayerBox_' + i.toString() + '"' + sSelectPlayer + ' dragMe="true" style="position: absolute; width: ' + fWidth.toString() + '%; min-width: 100px; height: ' + fHeightBox.toString() + '%; min-height: 26px; top: ' + iTop.toString() + '%; left: ' + iLeft.toString() + '%; cursor: pointer; -webkit-box-shadow: 0px 0px 4px 4px rgba(0, 0, 0, .3); box-shadow: 0px 0px 4px 4px rgba(0, 0, 0, .3)' + sZIndex + '">' +
      '<div style="position: absolute; width: 25%; height: 100%; background-color: ' + color2 + '">' +
        '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + (iTextSize * 1.6).toString() + '%; color: white">' + sNo + '</h2>' +
      '</div>' +
      '<div style="position: absolute; width: 75%; height: 100%; left: 25%; border: 2px solid black; background-color: ' + color + '">' +
        '<div style="position: absolute; width: 100%; height: 65%; top: 0px; left: 0px; background-color: ' + color + '; word-break: break-word; vertical-align: middle">';
  if (sNat) {
    sBox +=
      getNatIcon(sNat, "position: absolute; width: 16px; top: 0px; right: 1px");
  }
  if (bCaptain) {
    sBox +=
      '<img src="/Content/Icons/captain.png" title="Kapitän" style="position: absolute; width: 16px; top: 2px; left: 2px"/>';
  }
  sBox +=
          '<text style="position: absolute; top: -4px; text-align: center; width: 100%; margin: 0; padding: 0px; font-size: ' + iTextSize.toString() + '%; color: black">' + sName + '</text>' +
        '</div>' +
        '<div style="position: absolute; width: 25%; height: 35%; min-height: 8px; bottom: 0px; left: 0%; background-color: ' + color + '">' +
          '<text style="position: absolute; top: -2px; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + (iTextSize * 0.6).toString() + '%; color: black">' + sPos[iPos] + '</text>' +
        '</div>' +
        '<div style="position: absolute; width: 25%; height: 35%; min-height: 8px; bottom: 0px; left: 25%; background-color: ' + color + '">' +
          '<text style="position: absolute; top: -2px; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + (iTextSize * 0.6).toString() + '%; color: black">' + sStrength + '</text>' +
        '</div>' +
        '<div style="position: absolute; width: 50%; height: 35%; min-height: 8px; bottom: 0px; left: 50%; background-color: ' + color + '">';
  if (sAge) {
    sBox += '<text style="position: absolute; top: -2px; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + (iTextSize * 0.6).toString() + '%; color: black">' + sAge + '</text>';
  } else {
    sBox += '<text class="txtPosition" style="position: absolute; top: -2px; text-align: center; vertical-align: middle; width: 100%; margin: 0px; padding: 0px; font-size: ' + (iTextSize * 0.6).toString() + '%; color: black">' + ptPos.Y.toString() + '/' + ptPos.X.toString() + '</text>';
  }
  sBox +=
        '</div>';
  if (sTeamname) {
    sBox +=
        '<div style="position: absolute; width: 100%; height: 35%; min-height: 8px; bottom: -35%; left: 0px">' +
          '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: ' + (iTextSize * 0.6).toString() + '%; color: white">' + sTeamname + '</h2>' +
        '</div>';
  }

  sBox +=
      '</div>' +
    '</div>';

    //sBox += '</a>';

  return sBox;
}

function getBoxMovePlayer(ptPos) {
  // Total box width: 18%
  var iTop = (100 - ((100 * ptPos.X) / 122)) - 1.75;
  var iLeft  = (100 *       (ptPos.Y + 25))  / 50;
  var iRight = (100 * (50 - (ptPos.Y + 25))) / 50;

  var sBox = "";

  sBox +=
    '<div style="position: absolute; width: 10%; min-width: 60px; top: ' + (iTop - 3) + '%; left: ' + (iLeft + 0 - 5) + '%; z-index: 99">' +
      '<img id="img_arrow_1" onclick="javascript:movePlayer(1)" style="position: relative; width: 100%; cursor: pointer" src="/Content/Images/arrow_up.png"/>' +
    '</div>';

  sBox +=
    '<div style="position: absolute; width:  4%; min-width: 24px; top: ' + (iTop - 1.5) + '%; left: ' + (iLeft + 13 + 1) + '%; z-index: 99">' +
      '<img id="img_arrow_2" onclick="javascript:movePlayer(2)" style="position: relative; width: 100%; cursor: pointer" src="/Content/Images/arrow_right.png"/>' +
    '</div>';

  sBox +=
    '<div style="position: absolute; width: 10%; min-width: 60px; top: ' + (iTop + 4) + '%; left: ' + (iLeft + 0 - 5) + '%; z-index: 99">' +
      '<img id="img_arrow_3" onclick="javascript:movePlayer(3)" style="position: relative; width: 100%; cursor: pointer" src="/Content/Images/arrow_down.png"/>' +
    '</div>';

  sBox +=
    '<div style="position: absolute; width:  4%; min-width: 24px; top: ' + (iTop - 1.5) + '%; right: ' + (iRight + 13 + 1) + '%; z-index: 99">' +
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
