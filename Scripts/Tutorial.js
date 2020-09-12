function drawTutorial(parent, iLevel, ttRef) {
  setTimeout(function () { drawTutorial2(parent, iLevel, ttRef); }, 500);
}

function drawTutorial2(parent, iLevel, ttRef) {
  var sText = "";
  var bNextButton = false;

  if (ttRef) {
    ttRef.iLevel = iLevel;
  }

  if (iLevel === 0) {
    sText = "Hallo!</br>Dies ist ein kleines Tutorial, welches dir die grundlegenden Kenntnisse von CORNERKICK vermitteln soll.</br>Für detailiertere Informationen besuche bitte die <a href=\"/Home/UserManual\">Anleitung</a>.";
    sText += "</br></br>Zum Fortfahren, klicke auf den button 'weiter'";
    bNextButton = true;
  } else if (iLevel === 1) {
    sText = "Diese Seite stellt das Hauptmenü von CORNERKICK dar. Über die Menüleiste oben kannst du auf die verschiedenen Seiten navigieren.</br>Mit einem Klick auf 'Cornerkick' oben links kommst du immer wieder hierhin zurück.";
    bNextButton = true;
  } else if (iLevel === 2) {
    sText = "Jetzt wollen wir uns mal deine Mannschaft ansehen. Klicke dazu im Menü oben auf 'Mannschaft->Aufstellung'.";
    sText += "</br></br>(oder auf den link 'Durchschnittsstärke (Startelf)' unter dem Abschnitt 'Aktuelle Lage' auf dieser Seite)";
    bNextButton = false;
  } else if (iLevel === 3) {
    sText = "Sehr gut! Auf der unteren Hälfe des Fussballfeldes befindet sich deine Mannschaft. Du kannst deine Spieler mit der Maus auf andere Positionen ziehen, oder eine Standardformation über das Dropdown-Menü links unter dem Spielfeld auswählen.";
    sText += "</br></br>Sobald der nächste Gegner feststeht (und du einen Spielbeobachter eingestellt hast), siehst du die Aufstellung deines Gegners in der oberen Hälfte des Spielfeldes.";
    sText += "</br></br>Klicke jetzt auf 'weiter'";
    bNextButton = true;
  } else if (iLevel === 4) {
    sText = "Unter dem Fussballfeld ist deine Mannschaft aufgelistet. Die ersten elf Spieler sind in der Startformation. Die nächsten sieben stehen als Einwechselspieler beim nächsten Spiel zur Verfügung. Alle weiteren Spieler gehören nicht dem Kader für das nächste Spiel an.";
    sText += "</br></br>Klicke auf 'weiter'";
    bNextButton = true;
  } else if (iLevel === 5) {
    sText = "Klicke auf einen Reservespieler in der Liste aber nicht auf den Namen. Dieser wird markiert. Wenn du nun einen anderen Spieler anklickst, werden sie getauscht.";
    sText += "</br></br>Probiere das mal aus!";
    bNextButton = false;
  } else if (iLevel === 6) {
    sText = "Ausgezeichnet!";
    sText += "</br></br>Wenn du auf den button 'Co-Trainer' unterhalb des Spielfeldes klickst, wird immer die aktuell beste Mannschaft aufgestellt.";
    bNextButton = true;
  } else if (iLevel === 7) {
    sText = "Als nächstes schauen wir uns mal einen deiner Spieler genauer an. Klicke hierzu auf den Namen eines Spielers in der Aufstellungsliste.";
    bNextButton = false;
  }

  drawTutorialDialog(parent, iLevel, sText, bNextButton, ttRef);
}

function drawTutorialDialog(parent, iLevel, sText, bNextButton, ttRef) {
  // Remove existing tutorial dialogs
  var dgTt = document.getElementsByClassName("ui-dialog");
  for (var i = dgTt.length - 1; i >= 0; i--) {
    if (dgTt[i] && dgTt[i].parentElement && dgTt[i].contains(document.getElementById("divTutorial"))) {
      dgTt[i].parentElement.removeChild(dgTt[i]);
    }
  }

  var div0 = document.createElement("div");
  div0.id = "divTutorial";
  div0.title = "Tutorial - Level " + iLevel.toString();

  // Text section
  var div1 = document.createElement("div");
  div1.style.position = "relative";
  div1.style.width = "90%";
  div1.style.height = "auto";
  div1.innerHTML = sText;
  div0.appendChild(div1);

  parent.appendChild(div0);

  var iWidth = Math.max(320, Math.trunc(parent.offsetWidth * 0.5));

  // Create buttons
  var buttons = [
    {
      text: "schließen",
      icon: "ui-icon-closethick",
      click: function () {
        $(this).dialog('destroy').remove();
      }
    }
  ];

  if (iLevel > 0) {
    buttons.push({
      text: "nochmal von Vorne",
      //icon: "ui-icon-closethick",
      click: function () {
        $.when(setLevel(true, 0)).done(function () {
          window.open('/Member/Desk', '_self', false);
          drawTutorial(parent, 0);
        });

        $(this).dialog('destroy').remove();
      }
    });
  }

  if (bNextButton) {
    buttons.push({
      text: "weiter",
      icon: "ui-icon-check",
      //class: "foo bar baz",
      id: "bnOk",
      click: function () {
        $.when(setLevel(true, iLevel + 1)).done(function () {
          if (iLevel === 3) {
            /*
            $('html, body').animate({
              scrollTop: $("#tablediv").offset().top
            }, 1000, function () {
              drawTutorial(parent, iLevel + 1);
            });
            */

            let elem = document.getElementById("tablediv");
            elem.scrollIntoView({ left: 0, block: 'start', behavior: 'smooth' });
            //e.preventDefault();

            setTimeout(function () { drawTutorial2(parent, iLevel + 1, ttRef); }, 1000);
          } else {
            drawTutorial2(parent, iLevel + 1, ttRef);
          }
        });

        $(this).dialog('destroy').remove();
      }
    });
  }

  $(div0).dialog({
    autoOpen: true,
    width: iWidth,
    buttons: buttons
  });
}

function setLevel(bShow, iLevel) {
  $.ajax({
    type: 'GET',
    url: '/Member/SetTutorialLevel',
    dataType: "json",
    data: { bShow: bShow, iLevel: iLevel },
    success: function (bOk) {
    }
  });
}
