function drawGame(iState, iGameSpeed) {
  //alert("drawGame: " + iState);

  var drawGameDiv = $("#divDrawGame");
  var bFinished = false;
  var iPositionsValue = $('#ddlPositions').val();
  var bAverage = iPositionsValue > 0;

  $.ajax({
    cache: false,
    url: "/ViewGame/ViewGameLocations",
    type: "GET",
    dataType: "JSON",
    data: { iState: iState, bAverage: bAverage },
    success: function (gLoc) {
      drawGameDiv.html('');

      if (iPositionsValue >= 0) {
        data = getPlayer(gLoc, iPositionsValue == 0);
        drawGameDiv.html(data);
      }

      bFinished = gLoc.bFinished;

      if (iState < 0 && bFinished) {
        iState = -2;
      }

      var bUpdate = true;
      if (iState === -1) {
        bUpdate = document.getElementById("cbUpdateStatistic").checked;
      }
      if (bUpdate === true) {
        plotStatistics(iState);
      }
    },
    error: function () {
      //alert("ERROR");
      return false;
    },
    complete: function (jqXHR, textStatus) {
      if (!iGameSpeed) {
        iGameSpeed = 300;
      }

      // Schedule the next request when the current one's complete
      if ((iState === -1 || iState === -3) && !bFinished && textStatus !== "error" && !bStopPlay && !bAdminStopPlay) {
        setTimeout(function () { drawGame(-1, iGameSpeed); }, iGameSpeed);
      }
    }
  });
}

function getPlayer(gLoc, bShowLookAt = true) {
  var sBox = "";

  if (gLoc.ltPlayer.length < 1) return sBox;

  var divGame = document.getElementById("divDrawGame");
  iDivWidthPix  = divGame.offsetWidth .toString();
  iDivHeightPix = divGame.offsetHeight.toString();

  var cbTargetPos = document.getElementById("cbTargetPos");

  var fLookAtSize = 0.3;

  for (var iP = 0; iP < 11; iP++) {
    // Player Home
    if (gLoc.ltPlayer[iP +  0].iCard < 2) { // if not red card
      var fXh = gLoc.ltPlayer[iP +  0].ptPos.X / 122.0;
      var fYh = gLoc.ltPlayer[iP +  0].ptPos.Y /  50.0;

      var sCardH = "white";
      if (gLoc.ltPlayer[iP +  0].iCard > 0) {
        sCardH = "yellow";
      }

      var sXh = ((100 *  fXh       ) - 1.0).toString();
      var sYh = ((100 * (fYh + 0.5)) - 1.5).toString();

      var fLookAtAngleXh = -Math.cos(gLoc.ltPlayer[iP +  0].iLookAt * 60 * Math.PI / 180.0);
      var fLookAtAngleYh = -Math.sin(gLoc.ltPlayer[iP +  0].iLookAt * 60 * Math.PI / 180.0);

      var sLookAtXh = ((0.5 + (fLookAtAngleXh / 2) - (fLookAtSize / 2)) * 100).toString();
      var sLookAtYh = ((0.5 + (fLookAtAngleYh / 2) - (fLookAtSize / 2)) * 100).toString();
      sBox += '<div style="position: absolute; width: 2%; height: 3%; top: ' + sYh + '%; left: ' + sXh + '%; background-color: ' + sCardH + '; border: 2px solid blue; -webkit-border-radius: 50%; -moz-border-radius: 50%; z-index:21">';
      sBox   += '<div style="position: absolute; width: 100%; height: 100%; top: 0%; left: 0%">' +
                  '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 100%; color: black; z-index:22">' + gLoc.ltPlayer[iP +  0].iNo.toString() + '</h2>' + 
                '</div>';

      // draw look-at circle
      if (bShowLookAt) {
        sBox += '<div style="position: absolute; width: ' + (fLookAtSize * 100).toString() + '%; height: ' + (fLookAtSize * 100).toString() + '%; top: ' + sLookAtYh + '%; left: ' + sLookAtXh + '%; background-color: black; -webkit-border-radius: 50%; -moz-border-radius: 50%; z-index:23"></div>';
      }

      sBox += '</div>';
    }

    // Player Away
    if (gLoc.ltPlayer[iP + 11].iCard < 2) { // if not red card
      var fXa = gLoc.ltPlayer[iP + 11].ptPos.X / 122.0;
      var fYa = gLoc.ltPlayer[iP + 11].ptPos.Y /  50.0;

      var sCardA = "white";
      if (gLoc.ltPlayer[iP + 11].iCard > 0) {
        sCardA = "yellow";
      }

      var sXa = ((100 *  fXa       ) - 1.0).toString();
      var sYa = ((100 * (fYa + 0.5)) - 1.5).toString();

      var fLookAtAngleXa = -Math.cos(gLoc.ltPlayer[iP + 11].iLookAt * 60 * Math.PI / 180.0);
      var fLookAtAngleYa = -Math.sin(gLoc.ltPlayer[iP + 11].iLookAt * 60 * Math.PI / 180.0);

      var sLookAtXa = ((0.5 + (fLookAtAngleXa / 2) - (fLookAtSize / 2)) * 100).toString();
      var sLookAtYa = ((0.5 + (fLookAtAngleYa / 2) - (fLookAtSize / 2)) * 100).toString();
      sBox += '<div style="position: absolute; width: 2%; height: 3%; top: ' + sYa + '%; left: ' + sXa + '%; background-color: ' + sCardA + '; border: 2px solid red; -webkit-border-radius: 50%; -moz-border-radius: 50%; z-index:21">';
      sBox   += '<div style="position: absolute; width: 100%; height: 100%; top: 0%; left: 0%">' +
                  '<h2 style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 100%; color: black; z-index:22">' + gLoc.ltPlayer[iP + 11].iNo.toString() + '</h2>' + 
                '</div>';

      // draw look-at circle
      if (bShowLookAt) {
        sBox += '<div style="position: absolute; width: ' + (fLookAtSize * 100).toString() + '%; height: ' + (fLookAtSize * 100).toString() + '%; top: ' + sLookAtYa + '%; left: ' + sLookAtXa + '%; background-color: black; -webkit-border-radius: 50%; -moz-border-radius: 50%; z-index:23"></div>';
      }

      sBox += '</div>';

      if (cbTargetPos) {
        if (cbTargetPos.checked && gLoc.ltPlayer[iP + 11].ptPosTarget.X > 0) {
          iX0 = ( fXa        * iDivWidthPix);
          iY0 = ((fYa + 0.5) * iDivHeightPix);
          iX1 = ( gLoc.ltPlayer[iP + 11].ptPosTarget.X       * iDivWidthPix ) / 122;
          iY1 = ((gLoc.ltPlayer[iP + 11].ptPosTarget.Y + 25) * iDivHeightPix) /  50;

          sBox += drawline(iX0, iY0, iX1, iY1, "black");
        }
      }
    }
  }

  // Ball target
  if (gLoc.gBall.iPassType > 0) {
    var sXbt = ((100 * ( gLoc.gBall.ptPosTarget.X       / 122.0)) - 0.750).toString();
    var sYbt = ((100 * ((gLoc.gBall.ptPosTarget.Y + 25) /  50.0)) - 1.125).toString();
    var fSizeXt = 1.5;
    var fSizeYt = fSizeXt * 1.5;
    sBox += '<div style="position: absolute; width: ' + fSizeXt.toString() + '%; height: ' + fSizeYt.toString() + '%; top: ' + sYbt + '%; left: ' + sXbt + '%; border: 2px solid rgb(0,230,230); -webkit-border-radius: 50%; -moz-border-radius: 50%; z-index:22"></div>';
  }

  // Ball
  var sXb = ((100 * ( gLoc.gBall.Pos.X       / 122.0)) - 0.6250).toString();
  var sYb = ((100 * ((gLoc.gBall.Pos.Y + 25) /  50.0)) - 0.9375).toString();
  var fSizeX = 1.25 + gLoc.gBall.Pos.Z;
  var fSizeY = fSizeX * 1.5;
  //sBox += '<div style="position: absolute; width: ' + fSizeX.toString() + '%; height: ' + fSizeY.toString() + '%; top: ' + sYb + '%; left: ' + sXb + '%; background-color: white; border: 2px solid black; -webkit-border-radius: 50%; -moz-border-radius: 50%"></div>';
  sBox += '<img src="/Content/Icons/ball_white.png\" alt=\"Ball\" style=\"position: absolute; width: ' + fSizeX.toString() + '%; height: ' + fSizeY.toString() + '%; top: ' + sYb + '%; left: ' + sXb + '%; z-index:23"/>';

  return sBox;
}

function plotStatistics(jState = -1) {
  //alert(jState);
  iState = jState;

  var iHeatmapValue = $('#ddlHeatmap').val();
  var iShootsValue  = $('#ddlShoots').val();
  var iDuelsValue   = $('#ddlDuels').val();
  var iPassesValue  = $('#ddlPasses').val();

  $.ajax({
    url: '/ViewGame/ViewGameGetDataStatisticObject',
    type: "GET",
    dataType: "JSON",
    data: { iState: iState, iHeatmap: iHeatmapValue, iAllShoots: iShootsValue, iAllDuels: iDuelsValue, iAllPasses: iPassesValue },
    cache: false,
    contentType: "application/json; charset=utf-8",
    error: function (xhr) {
      alert(xhr.responseText);
    },
    success: function (gD) {
      nStates = gD.nStates;

      if (jState === -2) {
        iState = nStates;
      }

      var fMinute = (gD.tsMinute.Hours * 60) + gD.tsMinute.Minutes + (gD.tsMinute.Seconds / 60);

      document.getElementById("lbGoalsH").innerText = gD.iGoalsH.toString();
      document.getElementById("lbGoalsA").innerText = gD.iGoalsA.toString();

      pMainSlider = $('#slider-Minute');
      if (iState < 0) {
        iState = gD.nStates;
        //iState = pMainSlider.slider("value");
        nStates = Math.max(nStates, Math.round(iState * (90.0 / fMinute)));
        //alert(gD.nStates + ", " + nStates);
        $("#slider-Minute").slider("option", "max", nStates);
        $('#slider-Minute').slider("value", iState);
      } else {
        $("#slider-Minute").slider("option", "max", nStates);
        $('#slider-Minute').slider("value", iState);
      }
      //alert(pMainSlider.slider("value") + ", " + pMainSlider.slider("option", "max"));

      // Get seconds from float number
      var iSeconds = (fMinute % 1) * 60;

      // Set Minute label
      var sZero = '';
      if (iSeconds === 0) {
        sZero = '0';
      }
      var sMinute = Math.floor(fMinute).toFixed(0) + ':' + iSeconds.toFixed(0) + sZero + ' Min.';
      $("#divMinute").html('<a id="txtMinute" style="position: absolute; width: 80px; left: ' + (((100.0 * iState) / nStates) + 0.5) + '%">' + sMinute + '</a>');

      $("#divTimelineIcons").html(gD.sTimelineIcons);

      $("#drawHeatmap").html('');

      // draw shoot lines
      if (gD.ltDrawLineShoot) {
        gD.ltDrawLineShoot.forEach(function (drawLineShoot) {
          var divGame = document.getElementById("divDrawGame");
          iDivWidthPix  = divGame.offsetWidth .toString();
          iDivHeightPix = divGame.offsetHeight.toString();

          iX0 = ( drawLineShoot.X0       * iDivWidthPix ) / 122;
          iY0 = ((drawLineShoot.Y0 + 25) * iDivHeightPix) /  50;
          iX1 = ( drawLineShoot.X1       * iDivWidthPix ) / 122;
          iY1 = ((drawLineShoot.Y1 + 25) * iDivHeightPix) /  50;

          $(drawLine(iX0, iY0, iX1, iY1, drawLineShoot.sColor, 1, 20)).appendTo('#drawHeatmap');
        });
      }

      if (gD.sCard) {
        $(gD.sCard).appendTo('#drawHeatmap');
      }

      // Bar statistics
      var dataH = gD.fDataH;
      var dataA = gD.fDataA;

      var iGoalsH        = dataH[0][0];
      var iGoalsA        = dataA[0][0];
      var iShootsH       = dataH[1][0];
      var iShootsA       = dataA[1][0];
      var iShootsOnGoalH = dataH[2][0];
      var iShootsOnGoalA = dataA[2][0];
      var iDuelsH        = dataH[4][0];
      var iDuelsA        = dataA[4][0];
      var iFoulsH        = dataH[5][0];
      var iFoulsA        = dataA[5][0];
      var iCornerkickH   = dataH[6][0];
      var iCornerkickA   = dataA[6][0];
      var iOffsiteH      = dataH[7][0];
      var iOffsiteA      = dataA[7][0];
      if (iGoalsH + iGoalsA > 0) {
        dataH[0][0] = 100 * iGoalsH / (iGoalsH + iGoalsA);
        dataA[0][0] = 100 - dataH[0][0];
      }
      if (iShootsH + iShootsA > 0) {
        dataH[1][0] = 100 * iShootsH / (iShootsH + iShootsA);
        dataA[1][0] = 100 - dataH[1][0];
      }
      if (iShootsOnGoalH + iShootsOnGoalA > 0) {
        dataH[2][0] = 100 * iShootsOnGoalH / (iShootsOnGoalH + iShootsOnGoalA);
        dataA[2][0] = 100 - dataH[2][0];
      }
      if (iDuelsH + iDuelsA > 0) {
        dataH[4][0] = 100 * iDuelsH / (iDuelsH + iDuelsA);
        dataA[4][0] = 100 - dataH[4][0];
      }
      if (iFoulsH + iFoulsA > 0) {
        dataH[5][0] = 100 * iFoulsH / (iFoulsH + iFoulsA);
        dataA[5][0] = 100 - dataH[5][0];
      }
      if (iCornerkickH + iCornerkickA > 0) {
        dataH[6][0] = 100 * iCornerkickH / (iCornerkickH + iCornerkickA);
        dataA[6][0] = 100 - dataH[6][0];
      }
      if (iOffsiteH + iOffsiteA > 0) {
        dataH[7][0] = 100 * iOffsiteH / (iOffsiteH + iOffsiteA);
        dataA[7][0] = 100 - dataH[7][0];
      }

      // Statistic bars
      var flotDataset = [
        { label: "Home", data: dataH, color: "#5482FF" },
        { label: "Away", data: dataA, color: "#F0000" }
      ];

      var ticks = [
        [0, "Tore"], [-1, "Torschüsse"], [-2, "aufs Tor"], [-3, "Ballbesitz"], [-4, "Zweikämpfe"], [-5, "Fouls"], [-6, "Ecken"], [-7, "Abseits"]
      ];

      var flotOptions = {
        series: {
          stack: true,
          bars: {
            show: true,
            fill: true
          }
        },
        bars: {
          align: "center",
          lineWidth: 1,
          barWidth: 0.72,
          horizontal: true,
          fillColor: { colors: [{ opacity: 0.5 }, { opacity: 1 }] }
        },
        xaxis: {
          show: false,
          position: "bottom",
          min: 0,
          max: 100,
          tickFormatter: "string"
        },
        yaxis: {
          show: true,
          min: -7.6,
          max: 0.6,
          ticks: ticks
        },
        grid: {
          hoverable: true,
          borderWidth: 2,
          backgroundColor: { colors: ["#EDF5FF", "#ffffff"] }
        }
      };

      //alert(flotDataset);
      p = $.plot($("#divPlotStatistics"), flotDataset, flotOptions);

      var ii = 0;
      $.each(p.getData()[0].data, function (i, el) {
        if (el[0] > 0) {
          var o = p.pointOffset({ x: el[0], y: el[1] });
          var s = el[0].toFixed(1) + '%';
          if (ii === 0) {
            s = iGoalsH.toString();
          } else if (ii === 1) {
            s = iShootsH.toString();
          } else if (ii === 2) {
            s = iShootsOnGoalH.toString();
          } else if (ii === 5) {
            s = iFoulsH.toString();
          } else if (ii === 6) {
            s = iCornerkickH.toString();
          } else if (ii === 7) {
            s = iOffsiteH.toString();
          }

          $('<div class="data-point-label">' + s + '</div>').css({
            position: 'absolute',
            left: (60 + o.left) / 2,
            top: o.top - 10,
            display: 'none'
          }).appendTo(p.getPlaceholder()).show();
        }

        ii = ii + 1;
      });

      ii = 0;
      $.each(p.getData()[1].data, function (i, el) {
        if (el[0] > 0) {
          var o = p.pointOffset({ x: el[0], y: el[1] });
          var s = el[0].toFixed(1) + '%';
          if (ii === 0) {
            s = iGoalsA.toString();
          } else if (ii === 1) {
            s = iShootsA.toString();
          } else if (ii === 2) {
            s = iShootsOnGoalA.toString();
          } else if (ii === 5) {
            s = iFoulsA.toString();
          } else if (ii === 6) {
            s = iCornerkickA.toString();
          } else if (ii === 7) {
            s = iOffsiteA.toString();
          }

          $('<div class="data-point-label">' + s + '</div>').css({
            position: 'absolute',
            right: (60 + o.left) / 2,
            top: o.top - 10,
            display: 'none'
          }).appendTo(p.getPlaceholder()).show();
          //}).appendTo(p.getPlaceholder()).fadeIn('slow');
        }

        ii = ii + 1;
      });

      // Comment box
      var tbKomm = document.getElementById("tbKommentar");
      tbKomm.value = gD.sComment;

      $("#statistikGoals").html(gD.sStatGoals);
      $("#statistikCards").html(gD.sStatCards);
      $("#statistikSubs") .html(gD.sStatSubs);

      // Heatmap
      $(gD.sDivHeatmap).appendTo('#drawHeatmap');

      $("#txtAdminChanceShootOnGoal").html(gD.sAdminChanceShootOnGoal);
      $("#txtAdminChanceGoal")       .html(gD.sAdminChanceGoal);

      //alert(jState);
      var i = 0;
      var j = 0;
      for (i = 0; i < gD.ltF.length; ++i) {
        if (!ltF[i]) {
          ltF[i] = [];
        }
        if (!ltM[i]) {
          ltM[i] = [];
        }

        if (gD.ltF[i]) {
          if (gD.ltF[i].length > 0) {
            if (jState === -1) {
              ltF[i].push(gD.ltF[i][0]);
            } else {
              ltF[i].length = 0;

              for (j = 0; j < iState; ++j) {
                if (j >= gD.ltF[i].length) {
                  break;
                }

                ltF[i].push(gD.ltF[i][j]);
              }
            }
          }
        }

        if (gD.ltM[i]) {
          if (gD.ltM[i].length > 0) {
            if (jState === -1) {
              ltM[i].push(gD.ltM[i][0]);
            } else {
              ltM[i].length = 0;

              for (j = 0; j < iState; ++j) {
                if (j >= gD.ltM[i].length) {
                  break;
                }

                ltM[i].push(gD.ltM[i][j]);
              }
            }
          }
        }
      }

      //alert(ltF[0][0].x + ", " + ltF[0][ltF[0].length - 1].x);
      /*
      var chartF = new CanvasJS.Chart("chartContainerF", {
        animationEnabled: false,
        theme: "theme2",//theme1
        title: {
          text: "Frische"
        },
        axisX: {
          gridThickness: 1
        },
        axisY: {
          valueFormatString: "0.0%",
          includeZero: false
        },
        legend: {
          horizontalAlign: "center", // left, center, right
          verticalAlign: "bottom",  // top, center, bottom
          dockInsidePlotArea: true
        },
        data: [
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "1",
            dataPoints: gD.ltF[0]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "2",
            dataPoints: gD.ltF[1]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "3",
            dataPoints: gD.ltF[2]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "4",
            dataPoints: gD.ltF[3]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "5",
            dataPoints: gD.ltF[4]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "6",
            dataPoints: gD.ltF[5]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "7",
            dataPoints: gD.ltF[6]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "8",
            dataPoints: gD.ltF[7]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "9",
            dataPoints: gD.ltF[8]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "10",
            dataPoints: gD.ltF[9]
          },
          {
            // Change type to "bar", "splineArea", "area", "spline", "pie",etc.
            showInLegend: true,
            type: "line",
            legendText: "11",
            dataPoints: gD.ltF[10]
          }
        ]
      });
      */

      chartF.render();
      chartM.render();
    } // success function
  }); // ajax
}

async function play(iState) {
  if (bStopPlay) {
    return;
  }

  iState = iState + 1;

  drawGame(iState);

  setTimeout(function () { play(iState) }, 250);
}

async function stop() {
  bStopPlay = true;
}
