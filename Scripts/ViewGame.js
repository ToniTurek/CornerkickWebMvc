var gLocArray = []; // Array of gameLoc struct
var bFinished = true;
var playerGlobal = [];
var imgBall;
var divBallTarget;

// iState: -3: initial call, -2: game finished, -1: running game, >=0: specific state
function drawGame(iState, iGameSpeed) {
  var iPositionsValue = $('#ddlPositions').val();
  var bAverage = iPositionsValue > 0;
  var sAjaxTextStatus = "";

  if (!iGameSpeed) {
    iGameSpeed = 300;
  }

  $.ajax({
    cache: false,
    url: "/ViewGame/ViewGameLocations",
    type: "GET",
    dataType: "JSON",
    data: { iState: iState, bAverage: bAverage },
    success: function (gLoc2) {
      if (iState === -3) { // If initial call --> set global bFinished flag and recall function if game not finished
        playerGlobal = drawPlayer(gLoc2);
        imgBall = drawBall();
        divBallTarget = drawBallTarget();
      }

      if (iState >= 0 || gLoc2.bFinished) { // If specific state or game is finished --> draw only once
        $("#tblComments tr").remove();

        gLocArray = [];
        drawGame2(gLoc2, iState);
      } else if (iState === -3) { // If initial call --> set global bFinished flag and recall function if game not finished
        bFinished = gLoc2.bFinished;
        if (!bFinished) {
          setTimeout(function () { drawGame(-1, iGameSpeed); }, iGameSpeed);
        }
      } else { // If running game --> Add latest element of locations to the array
        gLocArray.push(gLoc2);
      }
    },
    error: function () {
      //alert("ERROR");
      plotStatistics(iState);
      return false;
    },
    complete: function (jqXHR, textStatus) {
      sAjaxTextStatus = textStatus;
    }
  });

  // If array big enough, show results
  if (gLocArray.length > 0) {
    var gLoc = gLocArray[0];

    bFinished = gLoc.bFinished;

    if (gLocArray.length > 2 && iState === -1) {
      drawGame2(gLoc, iState);

      // Remove first element from the array
      gLocArray.splice(0, 1);
    }
  }

  // If running game and not finished --> recall function (loop)
  if (iState === -1 && !bFinished && sAjaxTextStatus !== "error" && !bStopPlay && bAdminStop) {
    setTimeout(function () { drawGame(-1, iGameSpeed); }, iGameSpeed);
  }
}

var iB = 0;
function drawGame2(gLoc, iState) {
  //var drawGameDiv = $("#divDrawGame");
  var iPositionsValue = $('#ddlPositions').val();

  //drawGameDiv.html('');

  if (iPositionsValue >= 0) {
    updatePlayer(playerGlobal, gLoc, iPositionsValue == 0);
    updateBallPos(imgBall, divBallTarget, gLoc.gBall);
    printComments(gLoc);
  }

  if (iState < 0 && gLoc.bFinished) {
    iState = -2;
  }

  var bUpdate = true;
  if (iState === -1) {
    bUpdate = document.getElementById("cbUpdateStatistic").checked;
  }
  if (bUpdate === true) {
    plotStatistics(iState);
  }
}

function drawBall() {
  var divDrawGame = document.getElementById("divDrawGame");

  var divBallTmp = document.createElement("div");
  divBallTmp.id = "divBall";
  divBallTmp.style.position = "absolute";
  divBallTmp.style.top  = "49.0625%";
  divBallTmp.style.left = "49.35%";
  divBallTmp.style.width = "1.25%";
  divBallTmp.style.height = "1.875%";
  divBallTmp.style.zIndex = "23";
  var imgBallTmp = document.createElement("img");
  imgBallTmp.id = "imgBall";
  imgBallTmp.src = "/Content/Icons/ball_white.png";
  imgBallTmp.alt = "Ball";
  imgBallTmp.style.position = "absolute";
  imgBallTmp.style.top  = "0px";
  imgBallTmp.style.left = "0px";
  imgBallTmp.style.width  = "100%";
  imgBallTmp.style.height = "100%";
  divBallTmp.appendChild(imgBallTmp);

  divDrawGame.appendChild(divBallTmp);

  return divBallTmp;
}

function drawBallTarget() {
  var divDrawGame = document.getElementById("divDrawGame");

  // Ball target
  var divBallTargetTmp = document.createElement("div");
  divBallTargetTmp.id = "divBallTarget";
  divBallTargetTmp.style.position = "absolute";
  divBallTargetTmp.style.top  = "49.0%";
  divBallTargetTmp.style.left = "49.25%";
  divBallTargetTmp.style.width = "1.50%";
  divBallTargetTmp.style.height = "2.25%";
  divBallTargetTmp.style.border = "2px solid rgb(0,230,230)";
  divBallTargetTmp.style.webkitBorderRadius = "50%";
  divBallTargetTmp.style.borderRadius = "50%";
  divBallTargetTmp.style.zIndex = "22";
  divBallTargetTmp.style.display = "none";

  divDrawGame.appendChild(divBallTargetTmp);

  return divBallTargetTmp;
}

function updateBallPos(imgBallTmp, divBallTargetTmp, gBall) {
  var fX = ((100 * ( gBall.Pos.X       / 122.0)) - 0.6250);
  var fY = ((100 * ((gBall.Pos.Y + 25) /  50.0)) - 0.9375);
  var fSizeX = 1.25 + gBall.Pos.Z;
  var fSizeY = fSizeX * 1.5;

  imgBallTmp.style.left = fX.toString() + '%';
  imgBallTmp.style.top  = fY.toString() + '%';
  imgBallTmp.style.width  = fSizeX.toString() + '%';
  imgBallTmp.style.height = fSizeY.toString() + '%';

  iB = 0;
  if (gBall.iPassType > 0) {
    //interpolateBall(gLoc.gBall, 10);

    divBallTargetTmp.style.display = "block";
    var sXbt = ((100 * ( gBall.ptPosTarget.X       / 122.0)) - 0.750).toString();
    var sYbt = ((100 * ((gBall.ptPosTarget.Y + 25) /  50.0)) - 1.125).toString();
    divBallTargetTmp.style.left = sXbt + '%';
    divBallTargetTmp.style.top  = sYbt + '%';
  } else {
    divBallTargetTmp.style.display = "none";
  }
}

function interpolateBall(gBall, nInterpSteps) {
  var imgBall = document.getElementById('imgBall');

  var fX0 = ((100 * ( gBall.ptPosLast  .X       / 122.0)) - 0.6250); // Start X
  var fY0 = ((100 * ((gBall.ptPosLast  .Y + 25) /  50.0)) - 0.9375); // Start Y
  var fX1 = ((100 * ( gBall.ptPosTarget.X       / 122.0)) - 0.6250); // Target X
  var fY1 = ((100 * ((gBall.ptPosTarget.Y + 25) /  50.0)) - 0.9375); // Target Y

  var iPS = gBall.nPassSteps - gBall.iPassStep;
  var fX00 = (fX0 * ((gBall.nPassSteps - iPS) / gBall.nPassSteps)) + (fX1 * (iPS / gBall.nPassSteps));
  var fY00 = (fY0 * ((gBall.nPassSteps - iPS) / gBall.nPassSteps)) + (fY1 * (iPS / gBall.nPassSteps));

  var fSizeX = 1.25 + ((iB + (iPS * nInterpSteps)) / (gBall.nPassSteps * nInterpSteps));
  var fSizeY = fSizeX * 1.5;

  iPS += 1;
  var fX01 = (fX0 * ((gBall.nPassSteps - iPS) / gBall.nPassSteps)) + (fX1 * (iPS / gBall.nPassSteps));
  var fY01 = (fY0 * ((gBall.nPassSteps - iPS) / gBall.nPassSteps)) + (fY1 * (iPS / gBall.nPassSteps));

  var fX = (fX00 * ((nInterpSteps - iB) / nInterpSteps)) + (fX01 * (iB / nInterpSteps));
  var fY = (fY00 * ((nInterpSteps - iB) / nInterpSteps)) + (fY01 * (iB / nInterpSteps));

  imgBall.style.left = fX.toString() + '%';
  imgBall.style.top  = fY.toString() + '%';
  imgBall.style.width  = fSizeX.toString() + '%';
  imgBall.style.height = fSizeY.toString() + '%';

  if (iB < nInterpSteps) {
    iB = iB + 1;
    setTimeout(function () { drawBallPos(gBall, nInterpSteps); }, 30);
  }
}

function drawPlayer(gLoc) {
  if (gLoc.ltPlayer.length < 1) return;

  var sColorH0 = gLoc.sColorJerseyH[0];
  var sColorH1 = gLoc.sColorJerseyH[1];
  var sColorA0 = gLoc.sColorJerseyA[0];
  var sColorA1 = gLoc.sColorJerseyA[1];

  var sColorTextH = "black";
  if (gLoc.bJerseyTextColorWhiteH) {
    sColorTextH = "white";
  }
  var sColorTextA = "black";
  if (gLoc.bJerseyTextColorWhiteA) {
    sColorTextA = "white";
  }

  var divDrawGame = document.getElementById("divDrawGame");
  var fLookAtSize = 0.3;

  var player = [];
  for (iP = 0; iP < 11; iP++) {
    // Player Home
    var divPlH = document.createElement("div");
    divPlH.id = "divPlayerH_" + iP.toString();
    divPlH.style.position = "absolute";
    divPlH.style.width = "2%";
    divPlH.style.height = "3%";
    divPlH.style.top = (30 + (iP * 4)).toString() + "%";
    divPlH.style.left = "40%";
    divPlH.style.backgroundColor = sColorH0;
    divPlH.style.border = "2px solid " + sColorH1;
    divPlH.style.webkitBorderRadius = "50%";
    divPlH.style.borderRadius = "50%";
    divPlH.style.zIndex = "21";

    var divPlNoH = document.createElement("div");
    divPlNoH.style.position = "absolute";
    divPlNoH.style.width = "100%";
    divPlNoH.style.height = "100%";
    divPlNoH.style.top = "0px";
    divPlNoH.style.left = "0px";
    divPlNoH.innerHTML = '<text style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 100%; color: ' + sColorTextH + '; z-index:22">' + gLoc.ltPlayer[iP + 0].iNo.toString() + '</text>';
    divPlH.appendChild(divPlNoH);

    // Draw look-at circle
    var divPlLookAtH = document.createElement("div");
    divPlLookAtH.style.position = "absolute";
    divPlLookAtH.style.width  = (fLookAtSize * 100).toString() + '%';
    divPlLookAtH.style.height = (fLookAtSize * 100).toString() + '%';
    divPlLookAtH.style.top = ((0.5 - (fLookAtSize / 2)) * 100).toString() + '%';
    divPlLookAtH.style.right = (-(fLookAtSize / 2) * 100).toString() + '%';
    divPlLookAtH.style.backgroundColor = 'black';
    divPlLookAtH.style.webkitBorderRadius = "50%";
    divPlLookAtH.style.borderRadius = "50%";
    divPlLookAtH.style.zIndex = "24";
    divPlH.appendChild(divPlLookAtH);
    divDrawGame.appendChild(divPlH);

    player.push(divPlH);
  }

  for (iP = 0; iP < 11; iP++) {
    // Player Away
    var divPlA = document.createElement("div");
    divPlA.id = "divPlayerA_" + iP.toString();
    divPlA.style.position = "absolute";
    divPlA.style.width = "2%";
    divPlA.style.height = "3%";
    divPlA.style.top = (30 + (iP * 4)).toString() + "%";
    divPlA.style.left = "60%";
    divPlA.style.backgroundColor = sColorA0;
    divPlA.style.border = "2px solid " + sColorA1;
    divPlA.style.webkitBorderRadius = "50%";
    divPlA.style.borderRadius = "50%";
    divPlA.style.zIndex = "21";

    var divPlNoA = document.createElement("div");
    divPlNoA.style.position = "absolute";
    divPlNoA.style.width = "100%";
    divPlNoA.style.height = "100%";
    divPlNoA.style.top = "0px";
    divPlNoA.style.left = "0px";
    divPlNoA.innerHTML = '<text style="position: absolute; text-align: center; vertical-align: middle; width: 100%; margin: 0; font-size: 100%; color: ' + sColorTextA + '; z-index:22">' + gLoc.ltPlayer[iP + 11].iNo.toString() + '</text>';
    divPlA.appendChild(divPlNoA);

    // Draw look-at circle
    var divPlLookAtA = document.createElement("div");
    divPlLookAtA.style.position = "absolute";
    divPlLookAtA.style.width = (fLookAtSize * 100).toString() + '%';
    divPlLookAtA.style.height = (fLookAtSize * 100).toString() + '%';
    divPlLookAtA.style.top = ((0.5 - (fLookAtSize / 2)) * 100).toString() + '%';
    divPlLookAtA.style.left = (-(fLookAtSize / 2) * 100).toString() + '%';
    divPlLookAtA.style.backgroundColor = 'black';
    divPlLookAtA.style.webkitBorderRadius = "50%";
    divPlLookAtA.style.borderRadius = "50%";
    divPlLookAtA.style.zIndex = "24";
    divPlA.appendChild(divPlLookAtA);
    divDrawGame.appendChild(divPlA);

    player.push(divPlA);
  }

  return player;
}

function updatePlayer(player, gLoc, bShowLookAt) {
  if (gLoc.ltPlayer.length < 1) return;

  var fLookAtSize = 0.3;

  var iP = 0;

  // Player home
  for (iP = 0; iP < 22; iP++) {
    var pl = gLoc.ltPlayer[iP];
    if (pl.iCard < 2) { // if not red card
      var fXh = gLoc.ltPlayer[iP +  0].ptPos.X / 122.0;
      var fYh = gLoc.ltPlayer[iP +  0].ptPos.Y /  50.0;

      var sXh = ((100 *  fXh       ) - 1.0).toString();
      var sYh = ((100 * (fYh + 0.5)) - 1.5).toString();

      if (bShowLookAt) {
        divLookAt = player[iP].children[1];
        var fLftH = ((1.0 - Math.cos(pl.iLookAt * 60 * Math.PI / 180.0)) / 2);
        divLookAt.style.left = ((fLftH - (fLookAtSize / 2)) * 100).toString() + '%';
        var fTopH = ((1.0 - Math.sin(pl.iLookAt * 60 * Math.PI / 180.0)) / 2);
        divLookAt.style.top  = ((fTopH - (fLookAtSize / 2)) * 100).toString() + '%';
      } else {
        divLookAt.style.display = "none";
      }

      player[iP].style.left = sXh + '%';
      player[iP].style.top  = sYh + '%';
    }
  }
}

function printComments(gLoc) {
  // Comment box
  var tblComments = document.getElementById('tblComments');

  var sLastComment = [];
  var colLast = tblComments.getElementsByTagName("tbody")[0];
  if (colLast.getElementsByTagName('td').length > 1) {
    sLastComment[0] = colLast.getElementsByTagName('td')[0].innerHTML;
    sLastComment[1] = colLast.getElementsByTagName('td')[1].innerHTML;
  }

  var iC = 0;
  for (iC = 0; iC < gLoc.ltComments.length; ++iC) {
    if (sLastComment[0] === gLoc.ltComments[iC][0] && sLastComment[1] === gLoc.ltComments[iC][1]) {
      continue;
    }

    var rowComments = tblComments.insertRow(0);
    var cellComments0 = rowComments.insertCell(0);
    var cellComments1 = rowComments.insertCell(1);
    cellComments0.innerHTML = gLoc.ltComments[iC][0];
    cellComments1.innerHTML = gLoc.ltComments[iC][1];
    if (gLoc.ltComments[iC][2]) {
      cellComments0.style.fontWeight = gLoc.ltComments[iC][2];
      cellComments1.style.fontWeight = gLoc.ltComments[iC][2];
    }
  }
}

var iStateLast = 0;
function plotStatistics(jState = -1) {
  var iState = jState;

  var iHeatmapValue = $('#ddlHeatmap').val();
  var iShootsValue  = $('#ddlShoots').val();
  var iDuelsValue   = $('#ddlDuels').val();
  var iPassesValue  = $('#ddlPasses').val();

  $.ajax({
    url: '/ViewGame/ViewGameGetDataStatisticObject',
    type: "GET",
    dataType: "JSON",
    data: { iState: iState, iStateLast: iStateLast, iHeatmap: iHeatmapValue, iAllShoots: iShootsValue, iAllDuels: iDuelsValue, iAllPasses: iPassesValue },
    cache: false,
    contentType: "application/json; charset=utf-8",
    error: function (xhr) {
      //alert(xhr.responseText);
    },
    success: function (gD) {
      nStates = gD.nStates;
      iStateLast = nStates;

      if (jState === -2) {
        iState = nStates;
      }

      var fMinute = (gD.tsMinute.Hours * 60) + gD.tsMinute.Minutes + (gD.tsMinute.Seconds / 60);

      //document.getElementById("lbGoalsH").innerText = gD.iGoalsH.toString();
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

      // draw pass lines
      if (gD.ltDrawLinePass) {
        gD.ltDrawLinePass.forEach(function (drawLinePass) {
          var divGame = document.getElementById("divDrawGame");
          iDivWidthPix  = divGame.offsetWidth.toString();
          iDivHeightPix = divGame.offsetHeight.toString();

          iX0 = (drawLinePass.X0 * iDivWidthPix) / 122;
          iY0 = ((drawLinePass.Y0 + 25) * iDivHeightPix) / 50;
          iX1 = (drawLinePass.X1 * iDivWidthPix) / 122;
          iY1 = ((drawLinePass.Y1 + 25) * iDivHeightPix) / 50;

          $(drawLine(iX0, iY0, iX1, iY1, drawLinePass.sColor, 2, 20, "dashed")).appendTo('#drawHeatmap');
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
      var fPassGoodH     = dataH[8][0];
      var fPassGoodA     = dataA[8][0];
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
      if (fPassGoodH + fPassGoodA > 0) {
        dataH[8][0] = 100 * fPassGoodH / (fPassGoodH + fPassGoodA);
        dataA[8][0] = 100 - dataH[8][0];
      }

      // Statistic bars
      var flotDataset = [
        { label: "Home", data: dataH, color: "#5482FF" },
        { label: "Away", data: dataA, color: "#F0000" }
      ];

      var ticks = [
        [0, "Tore"], [-1, "Torschüsse"], [-2, "aufs Tor"], [-3, "Ballbesitz"], [-4, "Zweikämpfe"], [-5, "Fouls"], [-6, "Ecken"], [-7, "Abseits"], [-8, "Passquote"]
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
          min: -8.6,
          max: 0.6,
          ticks: ticks
        },
        grid: {
          hoverable: true,
          borderWidth: 2,
          backgroundColor: { colors: ["#EDF5FF", "#ffffff"] }
        }
      };

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
          } else if (ii === 8) {
            s = fPassGoodH.toFixed(1) + '%';
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
          } else if (ii === 8) {
            s = fPassGoodA.toFixed(1) + '%';
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

      $("#txtReferee").html("Schiedsrichter: " + gD.sRefereeQuality + "<br/>Fehlentscheidungen: " + gD.sRefereeDecisions);

      $("#statistikGoals").html(gD.sStatGoals);
      $("#statistikCards").html(gD.sStatCards);
      $("#statistikSubs") .html(gD.sStatSubs);

      // Heatmap
      $(gD.sDivHeatmap).appendTo('#drawHeatmap');

      $("#txtAdminChanceShootOnGoal").html(gD.sAdminChanceShootOnGoal);
      $("#txtAdminChanceGoal")       .html(gD.sAdminChanceGoal);

      // Update team table
      $('#tableTeamViewGame').DataTable().ajax.reload();

      // Charts
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
              for (k = 0; k < gD.ltF[i].length; ++k) {
                ltF[i].push(gD.ltF[i][k]);
              }
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
              for (k = 0; k < gD.ltM[i].length; ++k) {
                ltM[i].push(gD.ltM[i][k]);
              }
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

      chartF.render();
      chartM.render();

      //if (document.getElementById("myCheck").checked) {
      if ($('#dialogPlAction').dialog('isOpen')) {
        $("#txtPlActionShoot").html((gD.fPlAction[0] * 100).toFixed(1) + '%');
        $("#txtPlActionPass" ).html((gD.fPlAction[1] * 100).toFixed(1) + '%');
        $("#txtPlActionGo"   ).html((gD.fPlAction[2] * 100).toFixed(1) + '%');
        $("#txtPlActionWait" ).html((gD.fPlAction[3] * 100).toFixed(1) + '%');
        $("#txtPlActionRnd"  ).html((gD.fPlActionRnd * 100).toFixed(1) + '%');
      }
    } // success function
  }); // ajax
}

async function play(iState) {
  if (bStopPlay) {
    return;
  }

  iState = iState + 1;

  drawGame(iState);

  setTimeout(function () { play(iState); }, 250);
}

async function stop() {
  bStopPlay = true;
}
