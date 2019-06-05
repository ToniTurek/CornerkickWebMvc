function getScorerTable(sText) {
  var sBox = '';

  sBox += '<h4>Torschützen</h4>';
  sBox += '<table class="tStat" style="width: auto" border="0" cellpadding="2" summary="Scorer">';
  sBox += '<tr>';
  sBox += '<th>#</th>';
  sBox += '<th>Spieler</th>';
  sBox += '<th>Verein</th>';
  sBox += '<th style="text-align:right">Tore</th>';
  sBox += '<th style="text-align:right">Vorl.</th>';
  sBox += '<th style="text-align:right">Scor.</th>';
  sBox += '</tr>';

  sBox += sText;

  sBox += '</table>';

  return sBox;
}
