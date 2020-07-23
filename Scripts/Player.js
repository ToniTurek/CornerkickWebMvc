function getContract(iPlayerId, iYears, iSalaryOffer, iBonusPlayOffer, iBonusGoalOffer, iFixedFee, bNego) {
  //alert(iPlayerId + ", " + iYears + ", " + iSalaryOffer);
  if (iYears) {
    return $.ajax({
      type: 'post',
      url: '/Member/GetPlayerSalary',
      dataType: "json",
      data: {
        iPlayerId: iPlayerId, iYears: iYears, iSalaryOffer: iSalaryOffer, iBonusPlayOffer: iBonusPlayOffer, iBonusGoalOffer: iBonusGoalOffer, iFixedFee: iFixedFee, bNegotiate: bNego
      },
      success: function (contract) {
      },
      error: function (sReturn) {
        alert(sReturn);
      }
    });
  }
}

function getTableTransferDetailsAjax(iPlayerId) {
  return $.ajax({
    type: 'POST',
    url: "/Member/getTableTransferDetails2",
    dataType: "json",
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

  $.when(getContract(iPlayerId, 1, 0, 0, 0, 0, false)).done(function (contract) {
    if (contract.fMood < 0) {
      alert("Der Spieler möchte nicht mehr mit Ihnen verhandeln.");
    } else {
      $.ajax({
        url: '/Member/PlayerCheckIfNewContract',
        type: "POST",
        dataType: "JSON",
        data: { iPlayerId: iPlayerId },
        success: function (iContractType) {
          var div0 = document.createElement("div");
          div0.id = "dialogContract2";
          div0.title = "Vertragsverhandlung";
          if (iContractType > 0) {
            div0.title += " (neuer Vertrag)";
          } else {
            div0.title += " (Vertragsverlängerung)";
          }

          // Negotiation button
          var bnNegotiate = document.createElement("button");
          bnNegotiate.type = "submit";
          bnNegotiate.id = "bnNegotiate";
          bnNegotiate.className = "btn btn-default";
          bnNegotiate.style.width = "100%";
          bnNegotiate.innerText = "verhandeln";
          bnNegotiate.onclick = function () { updateContract(iPlayerId, input12, true, this); };

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
          var input12 = document.createElement("input");
          input12.className = "form-control tbContractYears text-box single-line";
          input12.id = "tbContractYears";
          input12.style.textAlign = "right";
          input12.style.width = "50px";
          input12.type = "tel";
          input12.min = "1";
          input12.max = "5";
          input12.step = "1";
          input12.value = "1";
          input12.autocomplete = "off";
          input12.onkeyup = function () { updateContract(iPlayerId, input12, false, bnNegotiate); };
          div12.appendChild(input12);
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
          }
          div1.appendChild(div12);
          div0.appendChild(div1);

          // Salary textbox
          var div2 = document.createElement("div");
          div2.style.position = "relative";
          div2.style.width = "100%";
          div2.style.height = "60px";
          var div21 = document.createElement("div");
          div21.style.position = "absolute";
          div21.style.width = "45%";
          div21.align = "center";
          div21.innerHTML = "<b>Gefordertes Gehalt:</b>";
          var p21 = document.createElement("p");
          p21.id = "txtContractMoney2";
          p21.innerText = contract.iSalary.toLocaleString() + " €";
          div21.appendChild(p21);
          div2.appendChild(div21);
          var div22 = document.createElement("div");
          div22.style.position = "absolute";
          div22.style.width = "45%";
          div22.style.left = "55%";
          var div221 = document.createElement("div");
          div221.style.position = "absolute";
          div221.style.width = "100%";
          div221.align = "center";
          div221.innerHTML = "<b class=\"left\">Gebotenes Gehalt:</b>";
          div22.appendChild(div221);
          var div222 = document.createElement("div");
          div222.style.position = "absolute";
          div222.style.width = "100%";
          div222.style.top = "20px";
          div222.align = "right";
          var input222 = document.createElement("input");
          input222.className = "form-control tbContractSalaryOffer2";
          input222.id = "tbContractSalaryOffer2";
          input222.style.textAlign = "right";
          input222.style.width = "100%";
          input222.type = "tel";
          input222.min = "1";
          input222.step = "1000";
          input222.value = contract.iSalary.toLocaleString();
          input222.autocomplete = "off";
          $(input222).autoNumeric('init', {
            aSep: '.',
            aDec: ',',
            mDec: '0'
          });
          div222.appendChild(input222);
          div22.appendChild(div222);
          div2.appendChild(div22);
          div0.appendChild(div2);

          // Bonus play textbox
          var div3 = document.createElement("div");
          div3.style.position = "relative";
          div3.style.width = "100%";
          div3.style.height = "60px";
          var div31 = document.createElement("div");
          div31.style.position = "absolute";
          div31.style.width = "45%";
          div31.align = "center";
          div31.innerHTML = "<b>Gef. Auflaufprämie:</b>";
          var p31 = document.createElement("p");
          p31.id = "txtContractBonusPlay";
          p31.innerText = contract.iPlay.toLocaleString() + " €";
          div31.appendChild(p31);
          div3.appendChild(div31);
          var div32 = document.createElement("div");
          div32.style.position = "absolute";
          div32.style.width = "45%";
          div32.style.left = "55%";
          div32.style.top = "0px";
          var div321 = document.createElement("div");
          div321.style.position = "absolute";
          div321.style.width = "100%";
          div321.align = "center";
          div321.innerHTML = "<b class=\"left\">Geb. Auflaufprämie:</b>";
          div32.appendChild(div321);
          var div322 = document.createElement("div");
          div322.style.position = "absolute";
          div322.style.width = "100%";
          div322.style.top = "20px";
          div322.align = "right";
          var input322 = document.createElement("input");
          input322.className = "form-control tbContractBonusPlayOffer";
          input322.id = "tbContractBonusPlayOffer";
          input322.style.textAlign = "right";
          input322.style.width = "100%";
          input322.type = "tel";
          input322.min = "1";
          input322.step = "100";
          input322.value = contract.iPlay.toLocaleString();
          input322.autocomplete = "off";
          $(input322).autoNumeric('init', {
            aSep: '.',
            aDec: ',',
            mDec: '0'
          });
          div322.appendChild(input322);
          div32.appendChild(div322);
          div3.appendChild(div32);
          div0.appendChild(div3);

          // Bonus goal textbox
          var div4 = document.createElement("div");
          div4.style.position = "relative";
          div4.style.width = "100%";
          div4.style.height = "60px";
          var div41 = document.createElement("div");
          div41.style.position = "absolute";
          div41.style.width = "45%";
          div41.align = "center";
          div41.innerHTML = "<b>Gef. Torprämie:</b>";
          var p41 = document.createElement("p");
          p41.id = "txtContractBonusGoal";
          p41.innerText = contract.iGoal.toLocaleString() + " €";
          div41.appendChild(p41);
          div4.appendChild(div41);
          var div25 = document.createElement("div");
          div25.style.position = "absolute";
          div25.style.width = "45%";
          div25.style.left = "55%";
          var div251 = document.createElement("div");
          div251.style.position = "absolute";
          div251.style.width = "100%";
          div251.align = "center";
          div251.innerHTML = "<b class=\"left\">Geb. Torprämie:</b>";
          div25.appendChild(div251);
          var div252 = document.createElement("div");
          div252.style.position = "absolute";
          div252.style.width = "100%";
          div252.style.top = "20px";
          div252.align = "right";
          var input252 = document.createElement("input");
          input252.className = "form-control tbContractBonusGoalOffer";
          input252.id = "tbContractBonusGoalOffer";
          input252.style.textAlign = "right";
          input252.style.width = "100%";
          input252.type = "tel";
          input252.min = "1";
          input252.step = "100";
          input252.value = contract.iGoal.toLocaleString();
          input252.autocomplete = "off";
          $(input252).autoNumeric('init', {
            aSep: '.',
            aDec: ',',
            mDec: '0'
          });
          div252.appendChild(input252);
          div25.appendChild(div252);
          div4.appendChild(div25);
          div0.appendChild(div4);

          // Fix transfer fee textbox
          var divFixFee = document.createElement("div");
          divFixFee.style.position = "relative";
          divFixFee.style.width = "100%";
          divFixFee.style.height = "60px";
          var div26 = document.createElement("div");
          div26.style.position = "relative";
          div26.style.width = "45%";
          div26.style.left = "55%";
          div26.style.height = "60px";
          var div261 = document.createElement("div");
          div261.style.position = "absolute";
          div261.style.width = "100%";
          div261.align = "center";
          div261.innerHTML = "<b class=\"left\">Fixe Ablöse:</b>";
          div26.appendChild(div261);
          var div262 = document.createElement("div");
          div262.style.position = "absolute";
          div262.style.width = "100%";
          div262.style.top = "20px";
          div262.align = "right";
          var input262 = document.createElement("input");
          input262.className = "form-control tbContractFixTransferFeeOffer";
          input262.id = "tbContractFixTransferFeeOffer";
          input262.style.textAlign = "right";
          input262.style.width = "100%";
          input262.type = "tel";
          input262.min = "1";
          input262.step = "100";
          input262.value = contract.iFixTransferFee.toLocaleString();
          input262.autocomplete = "off";
          $(input262).autoNumeric('init', {
            aSep: '.',
            aDec: ',',
            mDec: '0'
          });
          input262.onkeyup = function () { updateContract(iPlayerId, input12, false, bnNegotiate); };
          div262.appendChild(input262);
          div26.appendChild(div262);
          divFixFee.appendChild(div26);
          div0.appendChild(divFixFee);

          // Mood
          var div6 = document.createElement("div");
          div6.style.position = "relative";
          div6.style.width = "100%";
          div6.style.top = "10px";
          div6.align = "center";
          div6.innerHTML = "<b>Stimmung: </b>";
          var txt6 = document.createElement("text");
          txt6.id = "txtContractMood2";
          txt6.innerText = "100%";
          div6.appendChild(txt6);
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

          setMoodText(contract.fMood);

          $(div0).dialog({
            autoOpen: true,
            width: 380,
            height: 480,
            buttons: [
              {
                text: "Bestätigen",
                icon: "ui-icon-check",
                //class: "foo bar baz",
                id: "bnOk",
                click: function () {
                  var iYears = $("#tbContractYears").val();
                  var sSalary = $("#txtContractMoney2").text();
                  var sBonusPlay = $("#txtContractBonusPlay").text();
                  var sBonusGoal = $("#txtContractBonusGoal").text();
                  var sFixTransferFee = $("#tbContractFixTransferFeeOffer").val();
                  var sPlayerMood = $("#txtContractMood2").text();

                  var bNextSeason = false;
                  var cbNextSeason = document.getElementById("cbNextSeason");
                  if (cbNextSeason !== null) {
                    bNextSeason = cbNextSeason.checked;
                  }

                  $.ajax({
                    url: "/Member/NegotiatePlayerContract",
                    type: 'POST',
                    traditional: true,
                    data: { iId: iPlayerId, iYears: iYears, sSalary: sSalary, sBonusPlay: sBonusPlay, sBonusGoal: sBonusGoal, sFixTransferFee: sFixTransferFee, bNextSeason: bNextSeason, sPlayerMood: sPlayerMood },
                    dataType: "json",
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

function input12Keyup(input12, iPlayerId) {
  setInitialContractReq(input12, iPlayerId);
}

function setInitialContractReq(input12, iPlayerId) {
  var bnNegotiate = document.getElementById("bnNegotiate");

  if (input12.value > 10) {
    input12.style.backgroundColor = "red";
    bnNegotiate.disabled = true;
  } else {
    input12.style.backgroundColor = "";
    bnNegotiate.disabled = false;
  }

  var iFixedFee = getIntFromString($("#tbContractFixTransferFeeOffer").val());
  $.when(getContract(iPlayerId, input12.value, 0, 0, 0, iFixedFee, false)).done(function (contract) {
    $("#txtContractMoney2").text(contract.iSalary.toLocaleString() + " €");
    setMoodText(contract.fMood);
    $("#tbContractSalaryOffer2").val(contract.iSalary.toLocaleString());
  });
}

function updateContract(iPlayerId, input12, bNego, bnNegotiate) {
  bnNegotiate.disabled = true;

  if (input12.value > 10) {
    input12.style.backgroundColor = "red";
    return;
  } else {
    input12.style.backgroundColor = "";
  }

  var iYears = $("#tbContractYears").val();
  var iSalaryOffer    = getIntFromString($("#tbContractSalaryOffer2").val());
  var iBonusPlayOffer = getIntFromString($("#tbContractBonusPlayOffer").val());
  var iBonusGoalOffer = getIntFromString($("#tbContractBonusGoalOffer").val());
  var iFixedFee = getIntFromString($("#tbContractFixTransferFeeOffer").val());

  $.when(getContract(iPlayerId, iYears, iSalaryOffer, iBonusPlayOffer, iBonusGoalOffer, iFixedFee, bNego)).done(function (contract) {
    if (contract.fMood < 0) {
      alert("Der Spieler hat die Vertragsverhandlungen abgebrochen!");
      $("#dialogContract2").dialog("close");
    } else {
      if (bNego) {
        setMoodText(contract.fMood);
      } else {
        $("#tbContractSalaryOffer2")  .val(contract.iSalary.toLocaleString());
        $("#tbContractBonusPlayOffer").val(contract.iPlay  .toLocaleString());
        $("#tbContractBonusGoalOffer").val(contract.iGoal  .toLocaleString());
      }

      $("#txtContractMoney2"   ).html(contract.iSalary.toLocaleString() + " €");
      $("#txtContractBonusPlay").html(contract.iPlay  .toLocaleString() + " €");
      $("#txtContractBonusGoal").html(contract.iGoal.toLocaleString() + " €");

      bnNegotiate.disabled = false;
    }
  });
}

function setMoodText(fMood) {
  var txtContractMood = document.getElementById("txtContractMood2");
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
