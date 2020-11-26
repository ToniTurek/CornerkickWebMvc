function getContract(iPlayerId, iYears, iSalaryOffer, iBonusPlayOffer, iBonusPointOffer, iBonusGoalOffer, iFixedFee, tbodyCupBonus, bNego) {
  //alert(iPlayerId + ", " + iYears + ", " + iSalaryOffer);

  // Cast to int
  iYears = parseInt(iYears, 10);

  // Check if NaN
  if (Number.isNaN(iYears)) {
    return;
  }

  if (iYears < 0) {
    return;
  }

  if (iYears > 5) {
    alert("Error: Maximale Vertragslänge = 5 Jahre");

    iYears = 5;
    document.getElementById("tbContractYears").value = iYears.toString();
  }

  // Get cup bonus
  var iaCupBonus = getCupBonus(tbodyCupBonus);

  return $.ajax({
    url: '/Member/GetPlayerSalary',
    type: 'POST',
    dataType: "JSON",
    contentType: 'application/json; charset=utf-8',
    data: JSON.stringify({
      iPlayerId: iPlayerId,
      iYears: iYears,
      iSalary: iSalaryOffer,
      iBonusPlay: iBonusPlayOffer,
      iBonusPoint: iBonusPointOffer,
      iBonusGoal: iBonusGoalOffer,
      iaCupBonus: iaCupBonus,
      iFixedFee: iFixedFee,
      bNegotiateNextSeason: bNego,
      sPlayerMood: ""
    }),
    success: function (ltContract) {
    },
    error: function (jqXHR, msg, obj) {
      alert(msg);
      alert(obj);
    }
  });
}

function getTableTransferDetailsAjax(iPlayerId) {
  return $.ajax({
    url: "/Member/getTableTransferDetails2",
    dataType: "JSON",
    data: { iPlayerId: iPlayerId },
    success: function (sTable) {
    }
  });
}

function createTableTransferDetails() {
  var sReturn = '';

  sReturn += '<table id="tableTransferDetails" cellspacing="0" style="width: auto" class="display responsive nowrap">';
  /*
  sReturn += '<thead>';
  sReturn += '<tr>';
  sReturn += '<th>#</th>';
  sReturn += '<th>Verein</th>';
  sReturn += '<th>Ablöse</th>';
  sReturn += '</tr>';
  sReturn += '</thead>';
  */
  sReturn += '<tbody>';
  sReturn += '</tbody>';
  sReturn += '</table>';

  return sReturn;
}

function getContractDialog(parent, iPlayerId, bFeeDialog) {
  // Remove existing contract dialogs
  var dgCt = document.getElementsByClassName("ui-dialog");
  for (var i = dgCt.length - 1; i >= 0; i--) {
    if (dgCt[i] && dgCt[i].parentElement && dgCt[i].contains(document.getElementById("dialogContract2"))) {
      dgCt[i].parentElement.removeChild(dgCt[i]);
    }
  }

  $.ajax({
    url: '/Member/PlayerCheckIfNewContract',
    type: "GET",
    dataType: "JSON",
    data: { iPlayerId: iPlayerId },
    success: function (iContractType) {
      var iLengthIni = 1;
      if (iContractType === 0) {
        iLengthIni = 0;
      }

      $.when(getContract(iPlayerId, iLengthIni, 0, 0, 0, 0, 0, null, false)).done(function (ltContract) {
        var contractReq = ltContract[0];
        var contractOff = ltContract[1];

        if (contractReq.fMood < 0) {
          alert("Der Spieler möchte nicht mehr mit Ihnen verhandeln.");
        } else {
          var div0 = document.createElement("div");
          div0.id = "dialogContract2";
          div0.title = "Vertragsverhandlung";
          if (iContractType > 0) {
            div0.title += " (neuer Vertrag)";
          } else {
            div0.title += " (Vertragsverlängerung)";
          }

          // Declare inputs
          var iptYears     = document.createElement("input");
          var iptSlry2     = document.createElement("input");
          var iptBnsPlay2  = document.createElement("input");
          var iptBnsPnt2   = document.createElement("input");
          var iptBnsGl2    = document.createElement("input");
          var iptFxdFee2   = document.createElement("input");
          var tBdyCupBonus = document.createElement("tbody");

          // Negotiation button
          var bnNegotiate = document.createElement("button");
          bnNegotiate.type = "submit";
          bnNegotiate.id = "bnNegotiate";
          bnNegotiate.className = "btn btn-default";
          bnNegotiate.style.width = "100%";
          bnNegotiate.innerText = "verhandeln";
          bnNegotiate.onclick = function () { updateContract(iPlayerId, iptYears, true, this, iptSlry2, iptBnsPlay2, iptBnsPnt2, iptBnsGl2, iptFxdFee2, tBdyCupBonus); };

          // Contract length
          var div1 = document.createElement("div");
          div1.style.position = "relative";
          div1.style.width = "100%";
          div1.style.height = "40px";
          div1.className = "form-group";
          var div11 = document.createElement("div");
          div11.style.position = "absolute";
          div11.style.width = "38%";
          div11.style.height = "auto";
          div11.style.top = "8px";
          div11.align = "right";
          div11.innerHTML = "<b class=\"left\">";
          if (iContractType === 0) {
            div11.innerHTML += "Zusätzl. ";
          }
          div11.innerHTML += "Laufzeit [a]: </b>";
          div1.appendChild(div11);
          var div12 = document.createElement("div");
          div12.style.position = "absolute";
          div12.style.width = "60%";
          div12.style.left = "40%";
          div12.align = "left";
          iptYears.className = "form-control tbContractYears text-box single-line";
          iptYears.id = "tbContractYears";
          iptYears.style.textAlign = "right";
          iptYears.style.width = "50px";
          iptYears.type = "tel";
          iptYears.min = iLengthIni.toString();
          iptYears.value = iLengthIni.toString();
          iptYears.max = "5";
          iptYears.step = "1";
          iptYears.autocomplete = "off";
          iptYears.onkeyup = function () { updateContract(iPlayerId, this, false, bnNegotiate, iptSlry2, iptBnsPlay2, iptBnsPnt2, iptBnsGl2, iptFxdFee2, tBdyCupBonus); };
          div12.appendChild(iptYears);
          if (iContractType > 1) {
            var lbNextSeason = document.createElement("label");
            lbNextSeason.style.position = "absolute";
            lbNextSeason.style.top = "6px";
            lbNextSeason.style.left = "60px";
            lbNextSeason.style.fontWeight = "normal";
            lbNextSeason.className = "noselect";
            var cbNextSeason = document.createElement("input");
            cbNextSeason.type = "checkbox";
            cbNextSeason.id = "cbNextSeason";
            lbNextSeason.appendChild(cbNextSeason);
            lbNextSeason.innerHTML += " Ab nächster Saison";
            div12.appendChild(lbNextSeason);

            if (iContractType === 3) {
              if (cbNextSeason) {
                cbNextSeason.checked = true;
                cbNextSeason.disabled = true;
              }

              if (lbNextSeason) {
                lbNextSeason.disabled = true;
              }
            }
          }
          div1.appendChild(div12);
          div0.appendChild(div1);

          /////////////////////////////////////////////////////////////////
          // Create table
          /////////////////////////////////////////////////////////////////
          //var tblNego = document.getElementById("tblTrainingPlan").getElementsByTagName('tbody')[0];
          var tblNego = document.createElement("table");
          tblNego.style.width = "100%";
          tblNego.style.tableLayout = "fixed";
          tblNego.border = "1";
          //tblNego.style.borderSpacing = "5px";
          //tblNego.cellPadding = "6px";
          tblNego.style.marginBottom = "10px";

          /////////////////////////////////////////////////////////////////
          // Table head
          /////////////////////////////////////////////////////////////////
          var tHead = tblNego.createTHead();
          var rowHead = tHead.insertRow();
          rowHead.style.backgroundColor = "gray";
          rowHead.style.color = "white";

          var cellHd0 = rowHead.insertCell();
          //cellHd0.innerHTML = "";
          cellHd0.style.textAlign = "center";
          cellHd0.style.fontWeight = 'bold';

          var cellHd1 = rowHead.insertCell();
          cellHd1.innerHTML = "Gefordert";
          cellHd1.style.textAlign = "center";
          cellHd1.style.fontWeight = 'bold';

          var cellHd2 = rowHead.insertCell();
          cellHd2.innerHTML = "Ihr Gebot";
          cellHd2.style.textAlign = "center";
          cellHd2.style.fontWeight = 'bold';

          /////////////////////////////////////////////////////////////////
          // Table body
          /////////////////////////////////////////////////////////////////
          var tBdyNego = document.createElement('tbody');

          // Salary row
          var rowSalary = tBdyNego.insertRow();

          var cellSlry0 = rowSalary.insertCell();
          cellSlry0.style.paddingRight = "4px";
          cellSlry0.style.fontWeight = "bold";
          cellSlry0.style.textAlign = "right";
          cellSlry0.innerText = "Gehalt:";

          var cellSlry1 = rowSalary.insertCell();
          cellSlry1.style.paddingRight = "6px";
          cellSlry1.style.textAlign = "right";
          var txtSlry1 = document.createElement("txt");
          txtSlry1.id = "txtContractMoney2";
          txtSlry1.innerText = contractReq.iSalary.toLocaleString() + " €";
          cellSlry1.appendChild(txtSlry1);

          var cellSlry2 = rowSalary.insertCell();
          cellSlry2.style.textAlign = "center";
          iptSlry2.className = "form-control tbContractSalaryOffer2";
          iptSlry2.id = "tbContractSalaryOffer2";
          iptSlry2.style.textAlign = "right";
          iptSlry2.style.width = "100%";
          iptSlry2.type = "tel";
          iptSlry2.min = "1";
          iptSlry2.step = "1000";
          iptSlry2.value = contractOff.iSalary.toLocaleString();
          iptSlry2.autocomplete = "off";
          $(iptSlry2).autoNumeric('init', {
            aSep: '.',
            aDec: ',',
            mDec: '0'
          });
          iptSlry2.onkeyup = function () { setContractQuotient(iPlayerId, iptYears, this, iptBnsPlay2, iptBnsPnt2, iptBnsGl2, iptFxdFee2, tBdyCupBonus); };
          cellSlry2.appendChild(iptSlry2);

          // Bonus play row
          var rowBonusPlay = tBdyNego.insertRow();

          var cellBnsPlay0 = rowBonusPlay.insertCell();
          cellBnsPlay0.style.paddingRight = "4px";
          cellBnsPlay0.style.textAlign = "right";
          cellBnsPlay0.style.fontWeight = "bold";
          cellBnsPlay0.innerText = "Auflaufprämie:";

          var cellBnsPlay1 = rowBonusPlay.insertCell();
          cellBnsPlay1.style.paddingRight = "6px";
          cellBnsPlay1.style.textAlign = "right";
          var txtBnsPlay1 = document.createElement("txt");
          txtBnsPlay1.id = "txtContractBonusPlay";
          txtBnsPlay1.innerText = contractReq.iPlay.toLocaleString() + " €";
          cellBnsPlay1.appendChild(txtBnsPlay1);

          var cellBnsPlay2 = rowBonusPlay.insertCell();
          cellBnsPlay2.style.textAlign = "center";
          iptBnsPlay2.className = "form-control tbContractBonusPlayOffer";
          iptBnsPlay2.id = "tbContractBonusPlayOffer";
          iptBnsPlay2.style.textAlign = "right";
          iptBnsPlay2.style.width = "100%";
          iptBnsPlay2.type = "tel";
          iptBnsPlay2.min = "1";
          iptBnsPlay2.step = "1000";
          iptBnsPlay2.value = contractOff.iPlay.toLocaleString();
          iptBnsPlay2.autocomplete = "off";
          $(iptBnsPlay2).autoNumeric('init', {
            aSep: '.',
            aDec: ',',
            mDec: '0'
          });
          iptBnsPlay2.onkeyup = function () { setContractQuotient(iPlayerId, iptYears, iptSlry2, this, iptBnsPnt2, iptBnsGl2, iptFxdFee2, tBdyCupBonus); };
          cellBnsPlay2.appendChild(iptBnsPlay2);

          // Bonus point row
          var rowBonusPnt = tBdyNego.insertRow();

          var cellBnsPnt0 = rowBonusPnt.insertCell();
          cellBnsPnt0.style.paddingRight = "4px";
          cellBnsPnt0.style.textAlign = "right";
          cellBnsPnt0.style.fontWeight = "bold";
          cellBnsPnt0.innerText = "Punktprämie:";

          var cellBnsPnt1 = rowBonusPnt.insertCell();
          cellBnsPnt1.style.paddingRight = "6px";
          cellBnsPnt1.style.textAlign = "right";
          var txtBnsPnt1 = document.createElement("txt");
          txtBnsPnt1.id = "txtContractBonusPnt";
          txtBnsPnt1.innerText = contractReq.iPoint.toLocaleString() + " €";
          cellBnsPnt1.appendChild(txtBnsPnt1);

          var cellBnsPnt2 = rowBonusPnt.insertCell();
          cellBnsPnt2.style.textAlign = "center";
          iptBnsPnt2.className = "form-control tbContractBonusPointOffer";
          iptBnsPnt2.id = "tbContractBonusPointOffer";
          iptBnsPnt2.style.textAlign = "right";
          iptBnsPnt2.style.width = "100%";
          iptBnsPnt2.type = "tel";
          iptBnsPnt2.min = "1";
          iptBnsPnt2.step = "1000";
          iptBnsPnt2.value = contractOff.iPoint.toLocaleString();
          iptBnsPnt2.autocomplete = "off";
          $(iptBnsPnt2).autoNumeric('init', {
            aSep: '.',
            aDec: ',',
            mDec: '0'
          });
          iptBnsPnt2.onkeyup = function () { setContractQuotient(iPlayerId, iptYears, iptSlry2, iptBnsPlay2, this, iptBnsGl2, iptFxdFee2, tBdyCupBonus); };
          cellBnsPnt2.appendChild(iptBnsPnt2);

          // Bonus goal row
          var rowBonusGl = tBdyNego.insertRow();

          var cellBnsGl0 = rowBonusGl.insertCell();
          cellBnsGl0.style.paddingRight = "4px";
          cellBnsGl0.style.textAlign = "right";
          cellBnsGl0.style.fontWeight = "bold";
          cellBnsGl0.innerText = "Torprämie:";

          var cellBnsGl1 = rowBonusGl.insertCell();
          cellBnsGl1.style.paddingRight = "6px";
          cellBnsGl1.style.textAlign = "right";
          var txtBnsGl1 = document.createElement("txt");
          txtBnsGl1.id = "txtContractBonusGoal";
          txtBnsGl1.innerText = contractReq.iGoal.toLocaleString() + " €";
          cellBnsGl1.appendChild(txtBnsGl1);

          var cellBnsGl2 = rowBonusGl.insertCell();
          cellBnsGl2.style.textAlign = "center";
          iptBnsGl2.className = "form-control tbContractBonusGoalOffer";
          iptBnsGl2.id = "tbContractBonusGoalOffer";
          iptBnsGl2.style.textAlign = "right";
          iptBnsGl2.style.width = "100%";
          iptBnsGl2.type = "tel";
          iptBnsGl2.min = "1";
          iptBnsGl2.step = "1000";
          iptBnsGl2.value = contractOff.iGoal.toLocaleString();
          iptBnsGl2.autocomplete = "off";
          $(iptBnsGl2).autoNumeric('init', {
            aSep: '.',
            aDec: ',',
            mDec: '0'
          });
          iptBnsGl2.onkeyup = function () { setContractQuotient(iPlayerId, iptYears, iptSlry2, iptBnsPlay2, iptBnsPnt2, this, iptFxdFee2, tBdyCupBonus); };
          cellBnsGl2.appendChild(iptBnsGl2);

          // Fixed fee row
          var rowFixedFee = tBdyNego.insertRow();

          var cellrowFxdFee0 = rowFixedFee.insertCell();
          cellrowFxdFee0.style.paddingRight = "4px";
          cellrowFxdFee0.style.textAlign = "right";
          cellrowFxdFee0.style.fontWeight = "bold";
          cellrowFxdFee0.innerText = "Fixe Ablöse:";

          var cellrowFxdFee1 = rowFixedFee.insertCell();
          cellrowFxdFee1.style.paddingRight = "6px";
          cellrowFxdFee1.style.textAlign = "right";
          var txtFxdFee1 = document.createElement("txt");
          txtFxdFee1.id = "txtContractFixedFeeReq";
          txtFxdFee1.innerText = contractReq.iFixTransferFee.toLocaleString() + " €";
          cellrowFxdFee1.appendChild(txtFxdFee1);

          var cellFxdFee2 = rowFixedFee.insertCell();
          cellFxdFee2.style.textAlign = "center";
          iptFxdFee2.className = "form-control tbContractFixTransferFeeOffer";
          iptFxdFee2.id = "tbContractFixTransferFeeOffer";
          iptFxdFee2.style.textAlign = "right";
          iptFxdFee2.style.width = "100%";
          iptFxdFee2.type = "tel";
          iptFxdFee2.min = "1";
          iptFxdFee2.step = "1000";
          iptFxdFee2.value = contractOff.iFixTransferFee.toLocaleString();
          iptFxdFee2.autocomplete = "off";
          $(iptFxdFee2).autoNumeric('init', {
            aSep: '.',
            aDec: ',',
            mDec: '0'
          });
          iptFxdFee2.onkeyup = function () { setContractQuotient(iPlayerId, iptYears, iptSlry2, iptBnsPlay2, iptBnsPnt2, iptBnsGl2, this, tBdyCupBonus); };
          cellFxdFee2.appendChild(iptFxdFee2);

          tblNego.appendChild(tBdyNego);
          div0.appendChild(tblNego);

          /////////////////////////////////////////////////////////////////
          // Cup bonus
          /////////////////////////////////////////////////////////////////
          //div0.appendChild(getCupBonusDiv());
          var txtCupBonusHeader = document.createElement("txt");
          //txtCupBonusHeader.style.marginTop = "10px";
          txtCupBonusHeader.style.fontWeight = "bold";
          txtCupBonusHeader.innerText = "Bonus Platzierung:";
          div0.appendChild(txtCupBonusHeader);

          var tblCupBonus = document.createElement("table");
          tblCupBonus.style.width = "100%";
          tblCupBonus.style.tableLayout = "fixed";
          tblCupBonus.border = "1";
          //tblCupBonus.style.disabled = true;

          // Table head
          var tHeadCupBonus = tblCupBonus.createTHead();
          var rowHeadCupBonus = tHeadCupBonus.insertRow();
          rowHeadCupBonus.style.backgroundColor = "gray";
          rowHeadCupBonus.style.color = "white";

          var cellHdCupBonus0 = rowHeadCupBonus.insertCell();
          //cellHdCupBonus0.style.width = "40%";
          cellHdCupBonus0.style.textAlign = "center";
          cellHdCupBonus0.style.fontWeight = "bold";
          cellHdCupBonus0.innerHTML = "Wettb.";

          var cellHdCupBonus1 = rowHeadCupBonus.insertCell();
          //cellHdCupBonus1.style.width = "20%";
          cellHdCupBonus1.style.textAlign = "center";
          cellHdCupBonus1.style.fontWeight = "bold";
          cellHdCupBonus1.innerHTML = "Platzierung";

          var cellHdCupBonus2 = rowHeadCupBonus.insertCell();
          //cellHdCupBonus2.style.width = "40%";
          cellHdCupBonus2.innerHTML = "Ihr Gebot";
          cellHdCupBonus2.style.textAlign = "center";
          cellHdCupBonus2.style.fontWeight = "bold";

          /////////////////////////////////////////////////////////////////
          // Table body
          if (contractOff.iaCupBonus != null) {
            for (var iCb = 0; iCb < contractOff.iaCupBonus.length; iCb++) {
              let iCupId = contractOff.iaCupBonus[iCb++];
              let iPlace = contractOff.iaCupBonus[iCb++];
              let iValue = contractOff.iaCupBonus[iCb];

              addCupBonusRow(tBdyCupBonus, iPlayerId, iptYears, iptSlry2, iptBnsPlay2, iptBnsPnt2, iptBnsGl2, iptFxdFee2, iCupId, iPlace, iValue);
            }
          }
          addCupBonusRow(tBdyCupBonus, iPlayerId, iptYears, iptSlry2, iptBnsPlay2, iptBnsPnt2, iptBnsGl2, iptFxdFee2, 0, 0, 0);

          tblCupBonus.appendChild(tBdyCupBonus);
          div0.appendChild(tblCupBonus);

          /////////////////////////////////////////////////////////////////
          // Mood / Negotiate button
          /////////////////////////////////////////////////////////////////
          var div6 = document.createElement("div");
          div6.style.position = "relative";
          div6.style.width = "96%";
          div6.style.left = "2%";
          div6.style.marginTop = "20px";
          //div6.style.bottom = "10px";
          div6.align = "center";

          // Quotient
          div6.innerHTML += "<b>Geboten/Gefordert: </b>";
          var txtContractQuot = document.createElement("text");
          txtContractQuot.id = "txtContractQuot";
          txtContractQuot.innerText = "100%\n";
          //div6.appendChild("<br>");
          div6.appendChild(txtContractQuot);

          // Mood
          div6.innerHTML += "<b>Stimmung: </b>";
          var txtContractMood = document.createElement("text");
          txtContractMood.id = "txtContractMood";
          txtContractMood.innerText = "100%";
          div6.appendChild(txtContractMood);

          bnNegotiate.style.marginTop = "4px";
          div6.appendChild(bnNegotiate);
          div0.appendChild(div6);

          parent.appendChild(div0);

          if (iContractType === 3) {
            var cbNextSeason2 = document.getElementById("cbNextSeason");
            if (cbNextSeason2) {
              cbNextSeason2.checked = true;
              cbNextSeason2.disabled = true;
            }

            var lbNextSeason2 = document.getElementById("lbNextSeason");
            if (lbNextSeason2) {
              lbNextSeason2.disabled = true;
            }
          }

          setContractQuotient(iPlayerId, iptYears, iptSlry2, iptBnsPlay2, iptBnsPnt2, iptBnsGl2, iptFxdFee2, tBdyCupBonus);
          setMoodText(contractReq.fMood);

          $(div0).dialog({
            autoOpen: true,
            autoResize: true,
            resizable: true,
            modal: true,
            width: 380,
            buttons: [
              {
                text: "Bestätigen",
                icon: "ui-icon-check",
                //class: "foo bar baz",
                id: "bnOk",
                click: function () {
                  var iYears           = getIntFromString(iptYears   .value);
                  var iSalaryOffer     = getIntFromString(txtSlry1   .innerText);
                  var iBonusPlayOffer  = getIntFromString(txtBnsPlay1.innerText);
                  var iBonusPointOffer = getIntFromString(txtBnsPnt1 .innerText);
                  var iBonusGoalOffer  = getIntFromString(txtBnsGl1  .innerText);
                  var iFixedFee        = getIntFromString(txtFxdFee1 .innerText);
                  var iaCupBonus = getCupBonus(tBdyCupBonus);
                  var sPlayerMood = $("#txtContractMood").text();

                  var bNextSeason = false;
                  var cbNextSeason = document.getElementById("cbNextSeason");
                  if (cbNextSeason !== null) {
                    bNextSeason = cbNextSeason.checked;
                  }

                  $.ajax({
                    url: "/Member/NegotiatePlayerContract",
                    type: 'POST',
                    dataType: "JSON",
                    contentType: 'application/json; charset=utf-8',
                    //data: { iId: iPlayerId, iYears: iYears, sSalary: sSalary, sBonusPlay: sBonusPlay, sBonusPoint: sBonusPoint, sBonusGoal: sBonusGoal, sFixTransferFee: sFixTransferFee, bNextSeason: bNextSeason, sPlayerMood: sPlayerMood },
                    data: JSON.stringify({
                      iPlayerId: iPlayerId,
                      iYears: iYears,
                      iSalary: iSalaryOffer,
                      iBonusPlay: iBonusPlayOffer,
                      iBonusPoint: iBonusPointOffer,
                      iBonusGoal: iBonusGoalOffer,
                      iaCupBonus: iaCupBonus,
                      iFixedFee: iFixedFee,
                      bNegotiateNextSeason: bNextSeason,
                      sPlayerMood: sPlayerMood
                    }),
                    success: function (response) {
                      alert(response);

                      if (bFeeDialog) {
                        $("#dialogTransferFee").dialog("open");
                      } else {
                        window.location.reload(true);
                      }
                    },
                    error: function (xhr) {
                      alert(xhr.responseText);
                    }
                  });

                  $(this).dialog('destroy').remove();
                }
              },
              {
                text: "Abbrechen",
                icon: "ui-icon-closethick",
                click: function () {
                  $(this).dialog('destroy').remove();
                }

                // Uncommenting the following line would hide the text,
                // resulting in the label being used as a tooltip
                //showText: false
              }
            ]
          });
        }
      });
    }
  });
}

function updateContract(iPlayerId, iptYears, bNego, bnNegotiate, iptSlry, iptBnsPlay, iptBnsPnt, iptBnsGl, iptFxdFee, tBdyCupBonus) {
  bnNegotiate.disabled = true;

  if (iptYears.value > 5) {
    iptYears.style.backgroundColor = "red";
    return;
  } else {
    iptYears.style.backgroundColor = "";
  }

  var iYears           = getIntFromString(iptYears  .value);
  var iSalaryOffer     = getIntFromString(iptSlry   .value);
  var iBonusPlayOffer  = getIntFromString(iptBnsPlay.value);
  var iBonusPointOffer = getIntFromString(iptBnsPnt .value);
  var iBonusGoalOffer  = getIntFromString(iptBnsGl  .value);
  var iFixedFee        = getIntFromString(iptFxdFee .value);

  $.when(getContract(iPlayerId, iYears, iSalaryOffer, iBonusPlayOffer, iBonusPointOffer, iBonusGoalOffer, iFixedFee, tBdyCupBonus, bNego)).done(function (ltContract) {
    var contractReq = ltContract[0];
    var contractOff = ltContract[1];

    if (contractReq.fMood < 0) {
      alert("Der Spieler hat die Vertragsverhandlungen abgebrochen!");
      $("#dialogContract2").dialog("close");
    } else {
      if (bNego) {
        setMoodText(contractReq.fMood);
      } else {
        iptSlry   .value = contractOff.iSalary.toLocaleString();
        iptBnsPlay.value = contractOff.iPlay  .toLocaleString();
        iptBnsPnt .value = contractOff.iPoint .toLocaleString();
        iptBnsGl  .value = contractOff.iGoal  .toLocaleString();

        setContractQuotient(iPlayerId, iptYears, iptSlry, iptBnsPlay, iptBnsPnt, iptBnsGl, iptFxdFee, tBdyCupBonus);
      }

      $("#txtContractMoney2"   ).html(contractReq.iSalary.toLocaleString() + " €");
      $("#txtContractBonusPlay").html(contractReq.iPlay  .toLocaleString() + " €");
      $("#txtContractBonusPnt" ).html(contractReq.iPoint .toLocaleString() + " €");
      $("#txtContractBonusGoal").html(contractReq.iGoal  .toLocaleString() + " €");
      $("#txtContractFixedFeeReq").html(contractReq.iFixTransferFee.toLocaleString() + " €");

      bnNegotiate.disabled = false;
    }
  });
}

function addCupBonusRow(tbody, iPlayerId, iptYears, iptSlry, iptBnsPlay, iptBnsPnt, iptBnsGl, iptFixedFee, iCupId, iPlace, iValue) {
  var row = tbody.insertRow();

  var cell0 = row.insertCell();
  //cell0.style.paddingRight = "4px";

  // Cup select
  var sctCupBonusCup = document.createElement("select");
  //sctCupBonusCup.classList.add("form-control");
  sctCupBonusCup.className = "form-control sctCupBonus";
  sctCupBonusCup.style.width = "100%";
  //sctCupBonusCup.disabled = true;

  var optCupBonusCup0 = document.createElement('option');
  optCupBonusCup0.value = "0";
  optCupBonusCup0.innerText = "kein";
  optCupBonusCup0.selected = parseInt(optCupBonusCup0.value) === iCupId;
  sctCupBonusCup.appendChild(optCupBonusCup0);

  var optCupBonusCup1 = document.createElement('option');
  optCupBonusCup1.value = "1";
  optCupBonusCup1.innerText = "Liga";
  optCupBonusCup1.selected = parseInt(optCupBonusCup1.value) === iCupId;
  sctCupBonusCup.appendChild(optCupBonusCup1);

  var optCupBonusCup2 = document.createElement('option');
  optCupBonusCup2.value = "2";
  optCupBonusCup2.innerText = "Pokal";
  optCupBonusCup2.selected = parseInt(optCupBonusCup2.value) === iCupId;
  sctCupBonusCup.appendChild(optCupBonusCup2);

  var optCupBonusCup3 = document.createElement('option');
  optCupBonusCup3.value = "3";
  optCupBonusCup3.innerText = "Gold-Cup";
  optCupBonusCup3.selected = parseInt(optCupBonusCup3.value) === iCupId;
  sctCupBonusCup.appendChild(optCupBonusCup3);

  var optCupBonusCup4 = document.createElement('option');
  optCupBonusCup4.value = "4";
  optCupBonusCup4.innerText = "Silver-Cup";
  optCupBonusCup4.selected = parseInt(optCupBonusCup4.value) === iCupId;
  sctCupBonusCup.appendChild(optCupBonusCup4);

  var optCupBonusCup5 = document.createElement('option');
  optCupBonusCup5.value = "15";
  optCupBonusCup5.innerText = "Bronze-Cup";
  optCupBonusCup5.selected = parseInt(optCupBonusCup5.value) === iCupId;
  sctCupBonusCup.appendChild(optCupBonusCup5);

  cell0.appendChild(sctCupBonusCup);

  var cell1 = row.insertCell();
  //cell1.style.paddingRight = "6px";
  var iptCupBonusPlace = document.createElement("input");
  iptCupBonusPlace.className = "form-control iptCupBonusPlace";
  //iptCupBonusPlace.id = "iptCupBonusPlace";
  iptCupBonusPlace.style.textAlign = "right";
  iptCupBonusPlace.style.width = "100%";
  iptCupBonusPlace.type = "tel";
  iptCupBonusPlace.min = "1";
  iptCupBonusPlace.step = "1";
  if (iPlace > 0) { iptCupBonusPlace.value = iPlace.toString(); }
  else { iptCupBonusPlace.value = "1"; }
  iptCupBonusPlace.autocomplete = "off";
  iptCupBonusPlace.disabled = parseInt(sctCupBonusCup.value) === 0;
  iptCupBonusPlace.onkeyup = function () { setContractQuotient(iPlayerId, iptYears, iptSlry, iptBnsPlay, iptBnsPnt, iptBnsGl, iptFixedFee, tbody); };
  cell1.appendChild(iptCupBonusPlace);

  var cell2 = row.insertCell();
  cell2.style.textAlign = "center";
  var iptCupBonusValue = document.createElement("input");
  iptCupBonusValue.className = "form-control iptCupBonusValue";
  //iptCupBonusValue.id = "iptCupBonusValue";
  iptCupBonusValue.style.textAlign = "right";
  iptCupBonusValue.style.width = "100%";
  iptCupBonusValue.type = "tel";
  iptCupBonusValue.min = "1";
  iptCupBonusValue.step = "1000";
  iptCupBonusValue.value = iValue.toLocaleString();
  iptCupBonusValue.autocomplete = "off";
  $(iptCupBonusValue).autoNumeric('init', {
    aSep: '.',
    aDec: ',',
    mDec: '0'
  });
  iptCupBonusValue.disabled = parseInt(sctCupBonusCup.value) === 0;
  iptCupBonusValue.onkeyup = function () { setContractQuotient(iPlayerId, iptYears, iptSlry, iptBnsPlay, iptBnsPnt, iptBnsGl, iptFixedFee, tbody); };
  cell2.appendChild(iptCupBonusValue);

  sctCupBonusCup.onchange = function () {
    iptCupBonusPlace.disabled = parseInt(this.value) === 0;

    iptCupBonusValue.disabled = iptCupBonusPlace.disabled;

    for (var iChd = 0; iChd < tbody.children.length; iChd++) {
      var child = tbody.children[iChd];

      var ltSctToRm = child.getElementsByClassName("sctCupBonus");

      for (var i = 0; i < ltSctToRm.length; i++) {
        sctToRm = ltSctToRm[i];
        if (sctToRm != this && parseInt(sctToRm.value) === 0) {
          tbody.removeChild(child);
        }
      }
    }

    if (parseInt(this.value) > 0) {
      addCupBonusRow(tbody, iPlayerId, iptYears, iptSlry, iptBnsPlay, iptBnsPnt, iptBnsGl, iptFixedFee, 0, 0, 0);
    }

    setContractQuotient(iPlayerId, iptYears, iptSlry, iptBnsPlay, iptBnsPnt, iptBnsGl, iptFixedFee, tbody);
  };
}

function getCupBonus(tbodyCupBonus) {
  if (tbodyCupBonus == null) return null;

  var iaCupBonus = [];
  for (var iChd = 0; iChd < tbodyCupBonus.children.length; iChd++) {
    var child = tbodyCupBonus.children[iChd];

    var sctCb = child.getElementsByClassName("sctCupBonus")[0];
    if (parseInt(sctCb.value) > 0) {
      var iptCbPlace = child.getElementsByClassName("iptCupBonusPlace")[0];
      var iptCbValue = child.getElementsByClassName("iptCupBonusValue")[0];
      //var cb = { iCupId: parseInt(sctCb.value), iPlace: parseInt(iptCbPlace.value), iValue: getIntFromString(iptCbValue.value) };

      iaCupBonus.push(parseInt(sctCb.value));
      iaCupBonus.push(parseInt(iptCbPlace.value));
      iaCupBonus.push(getIntFromString(iptCbValue.value));
    }
  }

  return iaCupBonus;
}

function setContractQuotient(iPlayerId, iptYears, iptSalaryOffer, iptBonusPlayOffer, iptBonusPointOffer, iptBonusGoalOffer, iptFixedFee, tbodyCupBonus) {
  var txtContractQuot = document.getElementById("txtContractQuot");

  // Convert to int
  var iYears           = parseInt        (iptYears          .value);
  var iSalaryOffer     = getIntFromString(iptSalaryOffer    .value);
  var iBonusPlayOffer  = getIntFromString(iptBonusPlayOffer .value);
  var iBonusPointOffer = getIntFromString(iptBonusPointOffer.value);
  var iBonusGoalOffer  = getIntFromString(iptBonusGoalOffer .value);
  var iFixedFee        = getIntFromString(iptFixedFee       .value);

  // Get cup bonus
  var iaCupBonus = getCupBonus(tbodyCupBonus);

  $.ajax({
    url: '/Member/getContractQuotientOfferedRequired',
    type: "POST",
    dataType: "JSON",
    contentType: 'application/json; charset=utf-8',
    data: JSON.stringify({
      iPlayerId: iPlayerId,
      iYears: iYears,
      iSalaryOff: iSalaryOffer,
      iBonusPlayOff: iBonusPlayOffer,
      iBonusPointOff: iBonusPointOffer,
      iBonusGoalOff: iBonusGoalOffer,
      iaCupBonus: iaCupBonus,
      iFixTransferFee: iFixedFee
    }),
    success: function (fQuot) {
      txtContractQuot.innerText = (fQuot * 100).toFixed(0) + "%\n";
    }
  });
}

function setMoodText(fMood) {
  var txtContractMood = document.getElementById("txtContractMood");
  var fMoodAdj = (fMood - 0.1) / 0.9;

  txtContractMood.innerText = (fMoodAdj * 100).toFixed(0) + "%";
  if (fMoodAdj > 0.5) {
    txtContractMood.style.color = "green";
  } else if (fMoodAdj > 0.25) {
    txtContractMood.style.color = "orange";
  } else {
    txtContractMood.style.color = "red";
  }
}

function getPortrait(iPlayerId, sStyle) {
  var sImg = '';

  //sImg += ' onerror="if (this.src!=/Content/Images/Portraits/0.png) this.src=/Content/Images/Portraits/0.png;"';
  sImg += '<object data="/Content/Uploads/Portraits/' + iPlayerId.toString() + '.png"';
  sImg += ' style="';
  if (sStyle) {
    sImg += sStyle;
  } else {
    sImg += 'width: 64px';
  }
  sImg += '"/>';

  sImg += '<img src="/Content/Images/Portraits/0.png"';

  sImg += ' style="';
  if (sStyle) {
    sImg += sStyle;
  } else {
    sImg += 'width: 64px';
  }
  sImg += '"/>';

  sImg += '</object>';

  return sImg;
}

function getNatIcon(sNat, sStyle) {
  var sIcon = '<img src="/Content/Icons/flags/';

  if (sNat) {
    sIcon += sNat + '.png" title="' + sNat;
  } else {
    sIcon += '0.png" title="unknown';
  }

  sIcon += '" style="';

  if (sStyle) {
    sIcon += sStyle;
  } else {
    sIcon += 'width: 16px';
  }

  sIcon += '"/>';

  return sIcon;
}

function getFormIcon(sForm) {
  if (!sForm) return 'o';

  sForm = sForm.trim();

  var sIcon = '<img src="/Content/Icons/';
  if        (sForm === '---') {
    sIcon += 'form0';
  } else if (sForm ===  '-')  {
    sIcon += 'form1';
  } else if (sForm ===  'o')  {
    sIcon += 'form2';
  } else if (sForm ===  '+')  {
    sIcon += 'form3';
  } else if (sForm === '+++') {
    sIcon += 'form4';
  } else if (sForm === 'verl') {
    sIcon += 'ambulance';
  } else if (sForm === 'ang.') {
    sIcon += 'ambulance2';
  }

  sIcon += '.png" title="' + sForm + '" style="width: 16px"/>';

  return sIcon;
}
