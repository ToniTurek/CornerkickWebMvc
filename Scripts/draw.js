function drawLine(ax, ay, bx, by, sColor, iWidth, izIndex) {
  //alert(ax + ", " + ay + ", " + bx + ", " + by + ", " + sColor);

  if (!sColor) {
    sColor = "black";
  }

  if (!iWidth) {
    iWidth = 1;
  }

  if (ax > bx) {
    bx = ax + bx;
    ax = bx - ax;
    bx = bx - ax;
    by = ay + by;
    ay = by - ay;
    by = by - ay;
  }

  var angle = Math.atan((ay - by) / (bx - ax));

  angle = (angle * 180 / Math.PI);
  angle = -angle;

  var length = Math.sqrt((ax - bx) * (ax - bx) + (ay - by) * (ay - by));

  var style = "";
  style += "left:" + ax.toString() + "px;";
  style += "top:" + ay.toString() + "px;";
  style += "width:" + length.toString() + "px;";
  style += "height:" + iWidth.toString() + "px;";
  style += "background-color:" + sColor + ";";
  style += "position:absolute;";
  style += "transform:rotate(" + angle.toString() + "deg);";
  style += "-ms-transform:rotate(" + angle.toString() + "deg);";
  style += "transform-origin:0% 0%;";
  style += "-moz-transform:rotate(" + angle.toString() + "deg);";
  style += "-moz-transform-origin:0% 0%;";
  style += "-webkit-transform:rotate(" + angle.toString() + "deg);";
  style += "-webkit-transform-origin:0% 0%;";
  style += "-o-transform:rotate(" + angle.toString() + "deg);";
  style += "-o-transform-origin:0% 0%;";
  style += "-webkit-box-shadow: 0px 0px 2px 2px rgba(0, 0, 0, .1);";
  style += "box-shadow: 0px 0px 2px 2px rgba(0, 0, 0, .1)";

  if (izIndex) {
    style += ";z-index:" + izIndex.toString();
  }

  //$("<div style='" + style + "'></div>").appendTo('#divDrawGame');
  return "<div style='" + style + "'></div>";
}

function convertPosToPix(iX0, iY0, iX1, iY1, div, bHorizontal) {
  var iPos = [];

  if (bHorizontal) {
    iDivPixX = div.offsetWidth .toString();
    iDivPixY = div.offsetHeight.toString();

    iPos.push(( iX0       * iDivPixX) / 122);
    iPos.push(((iY0 + 25) * iDivPixY) /  50);
    iPos.push(( iX1       * iDivPixX) / 122);
    iPos.push(((iY1 + 25) * iDivPixY) /  50);
  } else {
    iDivPixX = div.offsetHeight.toString();
    iDivPixY = div.offsetWidth .toString();

    iPos.push(((iX0 + 25) * iDivPixY) /  50);
    iPos.push(( iY0       * iDivPixX) / 122);
    iPos.push(((iX1 + 25) * iDivPixY) /  50);
    iPos.push(( iY1       * iDivPixX) / 122);
  }

  return iPos;
}

function drawnode(x, y) {
  var ele = ""
  var style = "";
  style += "position:absolute;";
  style += "z-index:100;"
  ele += "<div class='relNode' style=" + style + ">";
  ele += "<span> Test Node</span>"
  ele += "<div>"

  $('#divDrawGame').show();
  var node = $(ele).appendTo('#divDrawGame');
  var width = node.width();
  var height = node.height();

  var centerX = width / 2;
  var centerY = height / 2;

  var startX = x - centerX;
  var startY = y - centerY;

  node.css("left", startX).css("top", startY);
}

