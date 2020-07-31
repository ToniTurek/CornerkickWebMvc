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

        changePlayerJerseyColor(0, gLoc2.sColorJerseyH[0], gLoc2.sColorJerseyH[1]);
        changePlayerJerseyColor(1, gLoc2.sColorJerseyA[0], gLoc2.sColorJerseyA[1]);

        imgBall = drawBall();
        divBallTarget = drawBallTarget();
        //ptBallLast = gLoc2.gBall.ptPos;
        plotStatistics(-3);
      }

      // Set finished flag
      bFinished = gLoc2.bFinished;

      if (iState >= 0 || gLoc2.bFinished) { // If specific state or game is finished --> draw only once
        $("#tblComments tr").remove();

        gLocArray = [];
        drawGame2(gLoc2, iState, iGameSpeed);
        plotStatistics(iState);
      } else if (iState === -3) { // If initial call --> set global bFinished flag and recall function if game not finished
        if (!bFinished && iGameSpeed > 0) {
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
      drawGame2(gLoc, iState, iGameSpeed);

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
function drawGame2(gLoc, iState, iGameSpeed) {
  var iPositionsValue = parseInt(document.getElementById("ddlPositions").value);

  updatePlayer(playerGlobal, gLoc, iPositionsValue === 0);
  updateBallPos(imgBall, divBallTarget, gLoc.gBall, iGameSpeed);
  printComments(gLoc);

  if (iState < 0 && gLoc.bFinished) {
    iState = -2;
  }

  // Update statistic
  var bUpdate = gLoc.bUpdateStatistic;
  if (bUpdate === true && iState === -1) {
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
  divBallTargetTmp.style.top  = "49.1%";
  divBallTargetTmp.style.left = "49.4%";
  divBallTargetTmp.style.width = "1.2%";
  divBallTargetTmp.style.height = "1.8%";
  divBallTargetTmp.style.border = "2px solid rgb(0,230,230)";
  divBallTargetTmp.style.webkitBorderRadius = "50%";
  divBallTargetTmp.style.borderRadius = "50%";
  divBallTargetTmp.style.zIndex = "22";
  divBallTargetTmp.style.display = "none";

  divDrawGame.appendChild(divBallTargetTmp);

  return divBallTargetTmp;
}

//var ptBallLast;
var ptBallTargetGlobal;
function updateBallPos(imgBallTmp, divBallTargetTmp, gBall, iGameSpeed) {
  if (gBall.nPassSteps > 0) {
    //alert(gBall.nPassSteps + ", " + gBall.iPassStep);
    ptBallTargetGlobal = gBall.ptPosTarget;

    // Interpolate ball
    if (gBall.nPassSteps - gBall.iPassStep === 0) { // Start interpolation at first pass step only
      interpolateBall(imgBallTmp, gBall.ptPosLast, gBall.ptPosTarget, !gBall.bLowPass, 10 * gBall.nPassSteps, iGameSpeed * gBall.nPassSteps);
    }

    /*
    if (gBall.nPassSteps - gBall.iPassStep === 0) {
      ptBallLast = gBall.ptPosLast;
    }

    if (!ptBallLast) {
      ptBallLast = gBall.ptPosLast;
    }

    interpolateBall(imgBallTmp, ptBallLast, gBall.Pos, gBall, gBall.iPassType === 2, 10, iGameSpeed);

    ptBallLast.X = gBall.Pos.X;
    ptBallLast.Y = gBall.Pos.Y;
     */

    divBallTargetTmp.style.display = "block";
    var sXbt = ((100 * ( gBall.ptPosTarget.X       / 122.0)) - 0.60).toString();
    var sYbt = ((100 * ((gBall.ptPosTarget.Y + 25) /  50.0)) - 0.90).toString();
    divBallTargetTmp.style.left = sXbt + '%';
    divBallTargetTmp.style.top  = sYbt + '%';
  } else {
    var ptBall = getPosPix(gBall.Pos);
    var fSizeX = 1.25;
    var fSizeY = fSizeX * 1.5;

    imgBallTmp.style.left = ptBall[0].toString() + '%';
    imgBallTmp.style.top  = ptBall[1].toString() + '%';
    imgBallTmp.style.width  = fSizeX.toString() + '%';
    imgBallTmp.style.height = fSizeY.toString() + '%';

    divBallTargetTmp.style.display = "none";
  }
}

var timerInterpolateBall = null;
function interpolateBall(imgBallTmp, pt0Ck, pt1Ck, bHighPass, nInterpSteps, iGameSpeed) {
  var pt0 = getPosPix(pt0Ck);
  var pt1 = getPosPix(pt1Ck);

  if (timerInterpolateBall !== null) {
    clearTimeout(timerInterpolateBall);
    timerInterpolateBall = null;
  }

  interpolateBall2(imgBallTmp, 0, pt0, pt1, bHighPass, nInterpSteps, iGameSpeed);
}

function interpolateBall2(imgBallTmp, iB, pt0, pt1, bHighPass, nInterpSteps, iGameSpeed) {
  pt1 = getPosPix(ptBallTargetGlobal);

  // Position
  var fX = (pt0[0] * ((nInterpSteps - iB) / nInterpSteps)) + (pt1[0] * (iB / nInterpSteps));
  var fY = (pt0[1] * ((nInterpSteps - iB) / nInterpSteps)) + (pt1[1] * (iB / nInterpSteps));
  imgBallTmp.style.left = fX.toString() + '%';
  imgBallTmp.style.top  = fY.toString() + '%';

  // Size
  //var fSizeX = 1.25 + ((iB + (iPS * nInterpSteps)) / (nPS * nInterpSteps));
  var fSizeX = 1.25;
  if (bHighPass) {
    fSizeX = 1.25 + (1.0 - Math.pow((2.0 * (iB / nInterpSteps)) - 1.0, 2));
  }
  var fSizeY = fSizeX * 1.5;
  imgBallTmp.style.width  = fSizeX.toString() + '%';
  imgBallTmp.style.height = fSizeY.toString() + '%';

  if (iB < nInterpSteps && iGameSpeed > 0) {
    var fSlowDownFactor = 1.6;

    iB = iB + 1;
    timerInterpolateBall = setTimeout(function () { interpolateBall2(imgBallTmp, iB + 1, pt0, pt1, bHighPass, nInterpSteps, iGameSpeed); }, (iGameSpeed * fSlowDownFactor) / nInterpSteps);
  }
}

function getPosPix(ptPos) {
  var fX = ((100 * ( ptPos.X       / 122.0)) - 0.6250);
  var fY = ((100 * ((ptPos.Y + 25) /  50.0)) - 0.9375);

  return [fX, fY];
}

function drawPlayer(gLoc) {
  if (gLoc.ltPlayer.length < 1) return;

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
    if (!gLoc.ltPlayer[iP +  0]) {
      continue;
    }

    // Player Home
    var divPlH = document.createElement("div");
    divPlH.id = "divPlayerH_" + iP.toString();
    divPlH.style.position = "absolute";
    divPlH.style.width = "2%";
    divPlH.style.height = "3%";
    divPlH.style.top = (30 + (iP * 4)).toString() + "%";
    divPlH.style.left = "40%";
    divPlH.style.border = "2px solid";
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
    if (!gLoc.ltPlayer[iP + 11]) {
      continue;
    }

    // Player Away
    var divPlA = document.createElement("div");
    divPlA.id = "divPlayerA_" + iP.toString();
    divPlA.style.position = "absolute";
    divPlA.style.width = "2%";
    divPlA.style.height = "3%";
    divPlA.style.top = (30 + (iP * 4)).toString() + "%";
    divPlA.style.left = "60%";
    divPlA.style.border = "2px solid";
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

  var iPositionsValue = parseInt(document.getElementById("ddlPositions").value);

  var fLookAtSize = 0.3;

  var iP = 0;

  // For each player
  for (iP = 0; iP < 22; iP++) {
    var pl = gLoc.ltPlayer[iP];

    if (!pl) {
      continue;
    }

    if (pl.iCard > 1) { // if red card
      player[iP].style.display = "none";
      continue;
    }

    if (iPositionsValue < 0) {
      player[iP].style.display = "none";
      continue;
    }

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
    player[iP].style.top = sYh + '%';

    player[iP].style.display = "block";
  }
}

function changePlayerJerseyColor(iHA, cl1, cl2) {
  if (iHA === 0) {
    for (iP = 0; iP < 11; iP++) {
      if (!playerGlobal[iP]) {
        continue;
      }

      playerGlobal[iP].style.backgroundColor = cl1;
      if (iP === 0) {
        playerGlobal[iP].style.borderColor = "rgb(57,255,20)";
      } else {
        playerGlobal[iP].style.borderColor = cl2;
      }
    }
  } else if (iHA === 1) {
    for (iP = 11; iP < 22; iP++) {
      if (!playerGlobal[iP]) {
        continue;
      }

      playerGlobal[iP].style.backgroundColor = cl1;
      if (iP === 11) {
        playerGlobal[iP].style.borderColor = "rgb(243,243,21)";
      } else {
        playerGlobal[iP].style.borderColor = cl2;
      }
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
    data: { iState: iState, iHeatmap: iHeatmapValue, iAllShoots: iShootsValue, iAllDuels: iDuelsValue, iAllPasses: iPassesValue },
    cache: false,
    contentType: "application/json; charset=utf-8",
    error: function (xhr) {
      //alert(xhr.responseText);
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

          $(drawLine(iX0, iY0, iX1, iY1, drawLineShoot.sColor, drawLineShoot.sTitle, 1, 20)).appendTo('#drawHeatmap');
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

          $(drawLine(iX0, iY0, iX1, iY1, drawLinePass.sColor, "", 2, 20, "dashed")).appendTo('#drawHeatmap');
        });
      }

      if (gD.sCard) {
        $(gD.sCard).appendTo('#drawHeatmap');
      }

      $(".tooltipDuel").tooltip({
        content: function () {
          return "<div align=\"right\">" + this.getAttribute("title") + "</div>";
        }
      });

      $(".tooltipShoot").tooltip({
        content: function () {
          return this.getAttribute("title");
        }
      });

      // Bar statistics
      var dataH = gD.fDataH;
      var dataA = gD.fDataA;

      if (dataH && dataA) {
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
          { data: dataH, yaxis: 1, color: "#5482FF" },
          { data: dataA, yaxis: 2, color: "#F0000" }
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
          yaxes: [
            { show: false },
            { show: false }
          ],
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

            var s = "";
            var sH = dataH[ii][0].toFixed(1) + '%';
            var sA = dataA[ii][0].toFixed(1) + '%';

            if (ii === 0) {
              s = "Tore";
              sH = iGoalsH.toString();
              sA = iGoalsA.toString();
            } else if (ii === 1) {
              s = "Torschüsse";
              sH = iShootsH.toString();
              sA = iShootsA.toString();
            } else if (ii === 2) {
              s = "aufs Tor";
              sH = iShootsOnGoalH.toString();
              sA = iShootsOnGoalA.toString();
            } else if (ii === 3) {
              s = "Ballbesitz";
            } else if (ii === 4) {
              s = "Zweikämpfe";
            } else if (ii === 5) {
              s = "Fouls";
              sH = iFoulsH.toString();
              sA = iFoulsA.toString();
            } else if (ii === 6) {
              s = "Ecken";
              sH = iCornerkickH.toString();
              sA = iCornerkickA.toString();
            } else if (ii === 7) {
              s = "Abseits";
              sH = iOffsiteH.toString();
              sA = iOffsiteA.toString();
            } else if (ii === 8) {
              s = "Passquote";
              sH = fPassGoodH.toFixed(1) + '%';
              sA = fPassGoodA.toFixed(1) + '%';
            }

            $('<div class="data-point-label">' + s + '</div>').css({
              position: 'absolute',
              width: "100%",
              textAlign: "center",
              color: "white",
              left: 0,
              top: o.top - 10
            }).appendTo(p.getPlaceholder()).show();

            $('<div class="data-point-label">' + sH + '</div>').css({
              position: 'absolute',
              width: "50%",
              textAlign: "left",
              color: "white",
              left: 10,
              top: o.top - 10
            }).appendTo(p.getPlaceholder()).show();

            $('<div class="data-point-label">' + sA + '</div>').css({
              position: 'absolute',
              width: "50%",
              textAlign: "right",
              color: "white",
              right: 10,
              top: o.top - 10
            }).appendTo(p.getPlaceholder()).show();
          }

          ii = ii + 1;
        });
      }

      $("#txtReferee").html("Schiedsrichter: " + gD.sRefereeQuality + "<br/>Fehlentscheidungen: " + gD.sRefereeDecisions);

      $("#statistikGoals").html('');
      $("#statistikCards").html('');
      $("#statistikSubs") .html('');
      $("#statistikGoals").html(gD.sStatGoals);
      $("#statistikCards").html(gD.sStatCards);
      $("#statistikSubs") .html(gD.sStatSubs);

      // Heatmap
      $(gD.sDivHeatmap).appendTo('#drawHeatmap');

      $("#txtAdminChanceShootOnGoal").html(gD.sAdminChanceShootOnGoal);
      $("#txtAdminChanceGoal")       .html(gD.sAdminChanceGoal);

      // Update team table
      $('#tableTeamViewGame').DataTable().ajax.reload();

      /////////////////////////////////////////////////////////////////
      // Charts
      /////////////////////////////////////////////////////////////////
      // Plot fresh
      try {
        if (gD.ltF) {
          var iF = 0;

          for (iF = 0; iF < gD.ltF.length; ++iF) {
            if (!ltF[iF]) {
              ltF[iF] = [];
            }

            if (gD.ltF[iF]) {
              if (gD.ltF[iF].length > 0) {
                if (jState === -1) {
                  for (k = 0; k < gD.ltF[iF].length; ++k) {
                    ltF[iF].push(gD.ltF[iF][k]);
                  }
                } else {
                  ltF[iF].length = 0;

                  var jF = 0;
                  for (jF = 0; jF < iState; ++jF) {
                    if (jF >= gD.ltF[iF].length) {
                      break;
                    }

                    ltF[iF].push(gD.ltF[iF][jF]);
                  }
                }
              }
            }
          }

          chartF.render();
        }
      } catch (e) {
        gD.ltF = null;
      }

      // Plot moral
      /*
      try {
        if (gD.ltM) {
          var iM = 0;

          for (iM = 0; iM < gD.ltM.length; ++iM) {
            if (!ltM[iM]) {
              ltM[iM] = [];
            }

            if (gD.ltM[iM]) {
              if (gD.ltM[iM].length > 0) {
                if (jState === -1) {
                  for (k = 0; k < gD.ltM[iM].length; ++k) {
                    ltM[iM].push(gD.ltM[iM][k]);
                  }
                } else {
                  ltM[iM].length = 0;

                  var jM = 0;
                  for (jM = 0; jM < iState; ++jM) {
                    if (jM >= gD.ltM[iM].length) {
                      break;
                    }

                    ltM[iM].push(gD.ltM[iM][jM]);
                  }
                }
              }
            }
          }

          chartM.render();
        }
      } catch (e) {
        gD.ltM = null;
      }
      */

      if (document.getElementById("bShowChances").checked) {
        // Check if sum of chances > 0
        var fChanceTotal = 0;
        for (var iC = 0; iC < gD.fPlAction.length; iC++) {
          fChanceTotal += gD.fPlAction[iC];
        }

        // Only draw chances if sum > 0
        if (fChanceTotal > 0) {
          drawPlayerChances(gD.fPlAction, gD.fPlActionRnd);
        }
      }
    } // success function
  }); // ajax
}

function drawPlayerChances(fPlAction, fPlActionRnd) {
  var chartPlAction = new CanvasJS.Chart("divPlActionChart", {
    backgroundColor: "transparent",
    animationEnabled: false,
    theme: "theme2",//theme1
    toolTip: {
      shared: true,
      borderColor: "black",
      contentFormatter: function (e) {
        var content = "<table>";

        // For each chance type
        for (var i = 0; i < e.entries.length; i++) {
          content += "<tr><td style=\"text-align:right\"><strong>" + e.entries[i].dataSeries.name + ":</strong></td><td style=\"text-align:right\">" + (e.entries[i].dataPoint.y * 100).toFixed(1) + "%</td>";
        }

        content += "<tr><td style=\"text-align:right\"><strong>Entscheidung:</strong></td><td style=\"text-align:right\">" + (fPlActionRnd * 100).toFixed(1) + "%</td>";
        content += "</table>";

        return content;
      }
    },
    axisX: {
      title: "",
      tickLength: 0,
      margin: 0,
      lineThickness: 0,
      valueFormatString: " " //comment this to show numeric values
    },
    axisY: {
      interval: 100,
      title: "",
      tickLength: 0,
      lineThickness: 0,
      margin: 0,
      valueFormatString: " ", //comment this to show numeric values
      stripLines: [{
        value: fPlActionRnd * 100.0,
        color: "black",
        thickness: 2
      }]
    },
    data: [
      {
        // Change type to "bar", "column", "splineArea", "area", "spline", "pie",etc.
        type: "stackedBar100",
        color: "red",
        name: "Schuss",
        dataPoints: [
          { y: fPlAction[0] }
        ]
      },
      {
        // Change type to "bar", "column", "splineArea", "area", "spline", "pie",etc.
        type: "stackedBar100",
        color: "blue",
        name: "Pass",
        dataPoints: [
          { y: fPlAction[1] }
        ]
      },
      {
        // Change type to "bar", "column", "splineArea", "area", "spline", "pie",etc.
        type: "stackedBar100",
        color: "yellow",
        name: "Dribbling",
        dataPoints: [
          { y: fPlAction[2] }
        ]
      },
      {
        // Change type to "bar", "column", "splineArea", "area", "spline", "pie",etc.
        type: "stackedBar100",
        color: "grey",
        name: "Warten",
        dataPoints: [
          { y: fPlAction[3] }
        ]
      }
    ]
  });

  chartPlAction.render();
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
