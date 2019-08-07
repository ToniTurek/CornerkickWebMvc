function addThousandSepToNumber(s) {
  return getIntFromString(s).toLocaleString('de-DE');
}

function getIntFromString(s) {
  s = s.toString();

  s = s.replace(/\./g, '');
  s = s.replace(/\,/g, '');

  return parseInt(s);
}

function addThousandSepToNumberInt(i) {
  return i.toLocaleString('de-DE');
}
