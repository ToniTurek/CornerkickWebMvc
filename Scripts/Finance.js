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

function updateBalance(iBalancePre) {
  $.when(getBalanceAjax()).done(function (iBalance) {
    incrementBalance(iBalancePre, iBalance[0], iBalance[1], iBalancePre - iBalance[0]);
  });
}

function incrementBalance(iBalancePre, iBalancePost, iBalanceSec, iDiffIni) {
  if (iBalancePre !== iBalancePost) {
    var iDiff = 1;
    if (Math.abs(iBalancePre - iBalancePost) > 1111111) {
      iDiff = 111111;
    } else if (Math.abs(iBalancePre - iBalancePost) > 111111) {
      iDiff = 11111;
    } else if (Math.abs(iBalancePre - iBalancePost) > 11111) {
      iDiff = 1111;
    } else if (Math.abs(iBalancePre - iBalancePost) > 1111) {
      iDiff = 111;
    } else if (Math.abs(iBalancePre - iBalancePost) > 111) {
      iDiff = 11;
    }

    if (iDiffIni > 0) {
      iBalancePre = iBalancePre - iDiff;
    } else {
      iBalancePre = iBalancePre + iDiff;
    }

    setBalance(iBalancePre, iBalanceSec);

    setTimeout(function () { incrementBalance(iBalancePre, iBalancePost, iBalanceSec, iDiffIni); }, 200000 / iDiffIni);
  }
}

function getBalanceAjax() {
  return $.ajax({
    type: 'GET',
    url: '/Member/GetBalance',
    dataType: "json",
    success: function (iBalance) {
    }
  });
}

function setBalance(iBalance, iBalanceSecret) {
  var txtBalance = document.getElementById("txtBalance");

  var sBalance = iBalance.toLocaleString();
  if (iBalanceSecret !== 0) {
    sBalance += " (" + iBalanceSecret.toLocaleString() + ")";
  }
  sBalance += " €";

  txtBalance.innerText = sBalance;
}
