function addThousandSepToNumber(s) {
  return getIntFromString(s).toLocaleString('de-DE');
}

function getIntFromString(s) {
  if (!s) return 0;

  s = s.toString();

  s = s.replace(/\€/g, ''); // Remove currency symbol
  s = s.replace(/\./g, ''); // Remove dot
  s = s.replace(/\,/g, ''); // Remove comma

  return parseInt(s);
}

function addThousandSepToNumberInt(i) {
  return i.toLocaleString('de-DE');
}
