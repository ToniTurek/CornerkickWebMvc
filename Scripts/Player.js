function getSalary(iPlayerId, iYears, iSalaryOffer) {
  //alert(iPlayerId + ", " + iYears + ", " + iSalaryOffer);
  return $.ajax({
    type: 'post',
    url: '/Member/GetPlayerSalary',
    dataType: "json",
    data: {
      iPlayerId: iPlayerId, iYears: iYears, iSalaryOffer: iSalaryOffer
    },
    success: function (contract) {
    }
  });
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
  $.when(getSalary(iPlayerId, 1, 0)).done(function (contract) {
    if (contract.fMood < 0) {
      alert("Der Spieler möchte nicht mehr mit Ihnen verhandeln.");
    } else {
      var div0 = document.createElement("div");
      div0.id = "dialogContract2";
      div0.title = "Vertragsverhandlung";

      var div1 = document.createElement("div");
      div1.style.position = "relative";
      div1.style.width = "100%";
      div1.style.height = "40px";
      div1.className = "form-group";
      var div11 = document.createElement("div");
      div11.style.position = "absolute";
      div11.style.width = "48%";
      div11.style.height = "auto";
      div11.style.top = "10px";
      div11.align = "right";
      div11.innerHTML = "<b class=\"left\">Laufzeit [a]: </b>";
      div1.appendChild(div11);
      var div12 = document.createElement("div");
      div12.style.position = "absolute";
      div12.style.width = "48%";
      div12.style.left = "52%";
      div12.align = "left";
      var input12 = document.createElement("input");
      input12.className = "form-control tbContractYears text-box single-line";
      input12.id = "iContractYears2";
      input12.style.textAlign = "right";
      input12.style.width = "60px";
      input12.type = "tel";
      input12.min = "1";
      input12.max = "10";
      input12.step = "1";
      input12.value = contract.iLength.toString();
      input12.autocomplete = "off";
      input12.onkeyup = function () { input12Keyup(input12, iPlayerId); };
      div12.appendChild(input12);
      div1.appendChild(div12);
      div0.appendChild(div1);

      var div2 = document.createElement("div");
      div2.style.position = "relative";
      div2.style.width = "100%";
      div2.style.height = "90px";
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
      var div23 = document.createElement("div");
      div23.style.position = "absolute";
      div23.style.width = "45%";
      div23.style.top = "40px";
      div23.align = "center";
      div23.innerHTML = "<b>Stimmung:</b>";
      var p23 = document.createElement("p");
      p23.id = "txtContractMood2";
      p23.innerText = "100%";
      div23.appendChild(p23);
      div2.appendChild(div23);
      div0.appendChild(div2);
      var bnNegotiate = document.createElement("button");
      bnNegotiate.type = "submit";
      bnNegotiate.id = "bnNegotiate";
      bnNegotiate.className = "btn btn-default";
      bnNegotiate.style.width = "100%";
      bnNegotiate.innerText = "verhandeln";
      bnNegotiate.onclick = function () { bnNegotiateClick(iPlayerId); };
      div0.appendChild(bnNegotiate);
      parent.appendChild(div0);

      setMoodText(contract.fMood);

      $(div0).dialog({
        autoOpen: true,
        width: 400,
        height: 300,
        buttons: [
          {
            text: "Bestätigen",
            icon: "ui-icon-check",
            //class: "foo bar baz",
            id: "bnOk",
            click: function () {
              var iYears = $("#iContractYears2").val();
              var sSalary = $("#txtContractMoney2").text();
              var sPlayerMood = $("#txtContractMood2").text();

              var iMode = 0;
              if (bFeeDialog) {
                iMode = 1;
              }

              $.ajax({
                url: "/Member/NegotiatePlayerContract",
                type: 'POST',
                traditional: true,
                data: { iId: iPlayerId, iYears: iYears, sSalary: sSalary, sPlayerMood: sPlayerMood, iMode: iMode },
                dataType: "json",
                success: function (response) {
                  alert(response);

                  if (bFeeDialog) {
                    $("#dialogTransferFee").dialog("open");
                  }
                },
                error: function (xhr) {
                  alert(xhr.responseText);
                }
              });

              $(this).dialog("close");
            }
          },
          {
            text: "Abbrechen",
            icon: "ui-icon-closethick",
            click: function () {
              $(this).dialog("close");
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

function input12Keyup(input12, iPlayerId) {
  var bnNegotiate = document.getElementById("bnNegotiate");

  if (input12.value > 10) {
    input12.style.backgroundColor = "red";
    bnNegotiate.disabled = true;
  } else {
    input12.style.backgroundColor = "";
    bnNegotiate.disabled = false;
  }

  $.when(getSalary(iPlayerId, input12.value, 0)).done(function (contract) {
    $("#txtContractMoney2").text(contract.iSalary.toLocaleString() + " €");
    setMoodText(contract.fMood);
    $("#tbContractSalaryOffer2").val(contract.iSalary.toLocaleString());
  });
}

function bnNegotiateClick(iPlayerId) {
  var iYears = $("#iContractYears2").val();
  var iSalaryOffer = getIntFromString($("#tbContractSalaryOffer2").val());

  $.when(getSalary(iPlayerId, iYears, iSalaryOffer)).done(function (contract) {
    if (contract.fMood < 0) {
      alert("Der Spieler hat die Vertragsverhandlungen abgebrochen!");
      jTable.ajax.reload();

      $("#dialogContract").dialog("close");
    } else {
      $("#txtContractMoney2").html(contract.iSalary.toLocaleString() + " €");
      setMoodText(contract.fMood);
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
