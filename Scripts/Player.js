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
              debugger;
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

function getNatIcon(sNat) {
  if (sNat) {
    return '<img src="/Content/Icons/flags/' + sNat + '.png" title="' + sNat + '" style="width: 16px"/>';
  } else {
    return '<img src="/Content/Icons/flags/0.png" title="unknown" style="width: 16px"/>';
  }
}
