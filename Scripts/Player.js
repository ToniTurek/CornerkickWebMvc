function openDialogContract(iPlId) {
  alert("openDialogContract");
  var divDialog = $("#dialogContract");
  alert(divDialog);
  divDialog.dialog("open");
  alert("C");

  $(function () {
    divDialog.dialog({
      autoOpen: false,
      buttons: {
        "Bestätigen": function () {
          var iYears  = $("#iContractYears").val();
          var sSalary = $("#txtContractMoney").text();

          $.ajax({
            url: "/Member/ExtentPlayerContract",
            type: 'POST',
            traditional: true,
            data: {
              iPlayer: iPlId, iYears: iYears, sSalary: sSalary
            },
            dataType: "json",
            success: function (response) {
              window.location.reload(true);
              alert(response);
            },
            error: function (xhr) {
              alert(xhr);
            }
          });

          $(this).dialog("close");
        },
        Cancel: function () {
          $(this).dialog("close");
        }
      }
    });
  });
}

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

function getNatIcon(sNat, sStyle) {
  var sIcon = '<img src="/Content/Icons/flags/';

  if (sNat) {
    sIcon += sNat + '.png" title="' + sNat;
  } else {
    sIcon += '0.png" title="unknown"';
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
