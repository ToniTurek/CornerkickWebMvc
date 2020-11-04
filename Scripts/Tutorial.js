function drawTutorial(parent, iLevel, ttRef) {
  setTimeout(function () { drawTutorial2(parent, iLevel, ttRef); }, 500);
}

function drawTutorial2(parent, iLevel, ttRef) {
  var sText = "";
  var sHeader = "";
  var iNextButton = 1; // Default: next

  if (ttRef) {
    ttRef.iLevel = iLevel;
  }

  // Header
  if (iLevel < 2) {
    sHeader += "Einleitung";
  } else if(iLevel < 7) {
    sHeader += "Aufstellung";
  } else if (iLevel < 15) {
    sHeader += "Spieler";
  } else if (iLevel < 19) {
    sHeader += "Training";
  } else if (iLevel < 21) {
    sHeader += "Personal";
  } else if (iLevel < 24) {
    sHeader += "Saisonvorbereitung";
  } else if (iLevel < 28) {
    sHeader += "Taktik";
  } else if (iLevel < 32) {
    sHeader += "Sponsoren";
  } else {
    sHeader += "Ende";
  }

  // Text
  if (iLevel === 0) {
    sText = "Hallo!</br>Dies ist ein kleines Tutorial, welches dir die grundlegenden Kenntnisse von CORNERKICK vermitteln soll.</br>Für detailiertere Informationen besuche bitte die <a href=\"/Home/UserManual\">Anleitung</a>.";
    sText += "</br></br>Zum Fortfahren, klicke auf den button \"weiter\"";
    iNextButton = 1;
  } else if (iLevel === 1) {
    sText = "Diese Seite stellt das Hauptmenü von CORNERKICK dar. Über die Menüleiste oben kannst du auf die verschiedenen Seiten navigieren.</br></br>Mit einem Klick auf \"Cornerkick\" oben links kommst du immer wieder hierhin zurück.";
    iNextButton = 1;
  } else if (iLevel === 2) {
    sText = "Jetzt wollen wir uns mal deine Mannschaft ansehen. Klicke dazu im Menü oben auf \"Mannschaft->Aufstellung\" (oder auf den link \"Durchschnittsstärke (Startelf)\" unter dem Abschnitt \"Aktuelle Lage\" auf dieser Seite).";
    iNextButton = 0;
  } else if (iLevel === 3) {
    sText = "Sehr gut! Auf der unteren Hälfe des Fussballfeldes befindet sich deine Mannschaft. Du kannst deine Spieler mit der Maus auf andere Positionen ziehen, oder eine Standardformation über das Dropdown-Menü links unter dem Spielfeld auswählen.";
    sText += "</br></br>Sobald der nächste Gegner feststeht (und du einen Spielbeobachter eingestellt hast), siehst du die Aufstellung deines Gegners in der oberen Hälfte des Spielfeldes.";
    sText += "</br></br>Klicke jetzt auf \"weiter\"";
    iNextButton = 1;
  } else if (iLevel === 4) {
    sText = "Unter dem Fussballfeld ist deine Mannschaft aufgelistet. Die ersten elf Spieler sind in der Startformation. Die nächsten sieben stehen als Einwechselspieler beim nächsten Spiel zur Verfügung. Alle weiteren Spieler gehören nicht dem Kader für das nächste Spiel an.";
    sText += "</br></br>Klicke auf \"weiter\"";
    iNextButton = 1;
  } else if (iLevel === 5) {
    sText = "Klicke auf einen Reservespieler in der Liste aber nicht auf den Namen. Dieser wird markiert. Wenn du nun einen anderen Spieler anklickst, werden sie getauscht.";
    sText += "</br></br>Probiere das mal aus!";
    iNextButton = 2;
  } else if (iLevel === 6) {
    sText = "Ausgezeichnet!";
    sText += "</br></br>Wenn du auf den button \"Co-Trainer\" unterhalb des Spielfeldes klickst, wird immer die aktuell beste Mannschaft aufgestellt.";
    iNextButton = 1;
  } else if (iLevel === 7) {
    sText = "Als nächstes schauen wir uns mal einen deiner Spieler genauer an. Klicke hierzu auf den Namen eines Spielers in der Aufstellungsliste.";
    iNextButton = 0;
  } else if (iLevel === 8) {
    sText = "Gut gemacht. Hier findest du alle Informationen über den Spieler in den drei Menüs \"Überblick\", \"Fähigkeiten\" und \"Statistik\".";
    iNextButton = 1;
  } else if (iLevel === 9) {
    sText = "Im Menü \"Überblick\" hast du unten links verschiedene Optionen. Zum Beispiel kannst du den Spieler auf die Transferliste setzen, oder zu deinem Kapitän oder Vize-Kapitän ernennen.";
    sText += "</br></br>Kapitäne sollten die Spieler mit der höchsten Führungspersönlichkeit (FP) sein.";
    iNextButton = 1;
  } else if (iLevel === 10) {
    sText = "Klicke jetzt auf den Reiter \"Fähigkeiten\".";
    iNextButton = 0;
  } else if (iLevel === 11) {
    sText = "Links unter \"Positionen\" siehst du, welche relative (\"Wert\") und absolute Stärke dein Spieler auf den jeweiligen Positionen hat. Setzt du einen Spieler auf einer Position ein, auf der er noch nicht einen Wert von 100% erreicht hat, erlernt er diese Position abhängig von seiner Characktereigenschaft \"Flexibilität\" (s. Menü \"Überblick\").";
    iNextButton = 1;
  } else if (iLevel === 12) {
    sText = "Unter der Überschrift \"Individuelle Fähigkeiten\" sind die einzelnen Fähigkeiten deines Spielers aufgelistet, welche jeweils einer Kategorie (\"Kat.\") zugeordnet sind. Aus ihnen sowie der Moral und der Frische deines Spielers setzt sich seine aktuelle Gesamtstärke (s. \"Stärke\" im Menü \"Überblick\") zusammen. Bei der bereinigten Stärke wird für die Frische und die Moral ein Wert von 100% angenommen.";
    iNextButton = 1;
  } else if (iLevel === 13) {
    sText = "Die individuellen Fähigkeiten kannst du als Trainer auf zwei Arten beeinflussen:</br>Zum Einen wird durch Mannschaftstraining wie \"Torschüsse\" der Trainingswert der Fähigkeit (in diesem Fall \"Schusskraft\" und \"Schussgenauigkeit\") verbessert.</br>Zum Anderen kann ein Spieler auch eine Fähigkeit individuell trainieren. Welche das ist, kannst du über die Knöpfe unter \"Ind.\" festlegen. Wie wahrscheinlich sich dein Spieler in dieser Fähigkeit verbessert, hängt u.A. von seinem Talent (\"Tal.\") in der jeweiligen Kategorie ab. Aber auch mit jedem Mannschaftstraining (sowie durch Aktionen im Spiel) steigt sein Bonus in dieser Fähigkeit an, wodurch seine Chancen ebenfalls erhöht werden, sich in dieser um ein Level zu verbessern.";
    iNextButton = 1;
  } else if (iLevel === 14) {
    sText = "Manche Fähigkeiten wie \"Schnelligkeit\" sind dabei schwerer zu erlernen als andere (wie z.B. \"Zweikampf\"). In der Graphik rechts unter \"Entwicklungschancen\" sind die einzelnen Faktoren sowie die resultierende Wahrscheinlichkeit einer Verbesserung pro Tag dargestellt.";
    iNextButton = 1;
  } else if (iLevel === 15) {
    sText = "Jetzt wollen wir mal das Training für die nächste Woche festlegen. Klicke dazu im Menü oben auf \"Mannschaft->Mannschaftstraining\".";
    iNextButton = 0;
  } else if (iLevel === 16) {
    sText = "Du kannst pro Tag maximal drei Trainingseinheiten festlegen, es sein denn, während einer Trainingszeit findet ein Spiel oder ein Ereignis statt.";
    sText += "</br></br>Oben unter \"Trainingsplan\" kannst du für jede Einheit individuell festlegen welche Trainingsart durchgeführt wird.";
    iNextButton = 1;
  } else if (iLevel === 17) {
    sText = "Unter \"Vorlagen Trainingswoche\" stehen auch vorgefertigte Trainingspläne für eine Woche zur Verfügung.";
    iNextButton = 1;
  } else if (iLevel === 18) {
    sText = "Zu Beginn der Saison solltest du erst einmal die Kondition deiner Spieler trainieren. Mit hoher Kondition verlieren deine Spieler weniger Frische im Spiel oder Training.";
    sText += "</br></br>An Spieltagen solltest du möglichst nur Regeneration trainieren, damit die Frische deiner Spieler beim Spiel möglichst hoch ist. Diese wirkt sich nämlich direkt auf die Fähigkeiten der Spieler aus.";
    iNextButton = 1;
  } else if (iLevel === 19) {
    sText = "Um effektives Training durchführen zu können, brauchst du Personal wie z.B. einen Co-Trainer, einen Konditionstrainer und einen Masseur.";
    sText += "</br></br>Um Personal einzustellen, gehe in das Menü \"Verein->Personal\".";
    iNextButton = 0;
  } else if (iLevel === 20) {
    sText = "Ein Co-Trainer hilft dir bei der Entwicklung der individuellen Fähigkeiten deiner Spieler. Konditionstrainer und Masseur erhöhen die Effektivität von Konditions- und Regenerationstraining.";
    sText += "</br></br>Wähle jetzt dein Personal aus und klicke auf den button \"Personal einstellen\".</br>ACHTUNG: Um dein Personal anschließend zu ändern, musst du dem bisherigen Trainer eine Abfindung in Höhe des halben Jahresgehalts zahlen.";
    iNextButton = 2;
  } else if (iLevel === 21) {
    sText = "Als nächstes wollen wir zur Vorbereitung auf die Saison ein Testspiel durchführen. Dies, sowie weitere Ereignisse wie Trainingslager, Weihnachtsfeiern usw. können über den Kalender geplant werden.";
    sText += "</br></br>Diesen kannst du über \"Saison->Kalender\" oder durch einen klick auf das aktuelle Datum oben links erreichen.";
    iNextButton = 0;
  } else if (iLevel === 22) {
    sText = "Um ein Testspiel zu planen, klicke einfach in ein freies Feld im Kalender. Anschließend wählst du die Option \"Testspiel vereinbaren\" und suchst dir einen gewünschten Testspielgegner aus. Ist es ein Computerverein, bekommst du sofort eine Antwort, andernfalls bekommt der Manager des Vereins eine Anfrage.";
    sText += "</br></br>Testspielanfragen an dich findest du unter dem Kalender. Dort kannst du sie entweder bestätigen oder ablehnen.";
    iNextButton = 1;
  } else if (iLevel === 23) {
    sText = "Trainingslager oder Ereignisse durchzuführen, kann sehr vorteilhaft sein. Für eine genaue Beschreibung klicke <a href=\"/Home/UserManual/#h2Calendar\" target=\"_blank\">hier</a>.";
    iNextButton = 1;
  } else if (iLevel === 24) {
    sText = "Ein Schlüssel zum Erfolg ist neben einer guten Vorbereitung auf das Spiel auch die Taktik. Um diese einzustellen, gehe jetzt zu \"Mannschaft->Taktik\".";
    iNextButton = 0;
  } else if (iLevel === 25) {
    sText = "Hier siehst du das Taktikboard. Lass dich von seiner Komplexität nicht abschrecken. Die wichtigsten Einstellungen sind die offensive Ausrichtung (orangener Schieber) und der Einsatz (magentafarbener Schieber) deiner Spieler.";
    iNextButton = 1;
  } else if (iLevel === 26) {
    sText = "Beginne nicht mit zu viel Einsatz ein Spiel, da deine Spieler ansonsten zu schnell zu viel Frische verlieren. Am besten, du lässt den Einsatz erstmal in der mittleren Position. Liegst du in der Schlussphase eines Spiels hinten, lohnt es sich mit mehr Einsatz zu spielen.";
    iNextButton = 1;
  } else if (iLevel === 27) {
    sText = "Ganz unten kannst du deinem Co-Trainer bereits vor dem Spiel mitteilen, welche Auswechslungen er für dich durchführen soll. Hast du also mal keine Zeit während eines Spiels dabei zu sein, kannst du trotzdem Frische Kräfte bringen. Dies kann unabhängig des Spielstandes sinnvoll sein.";
    sText += "</br></br>Eine Beschreibung der weiteren Taktikeinstellungen findest du <a href=\"/Home/UserManual/#h3Tactic\" target=\"_blank\">hier</a>.";
    iNextButton = 1;
  } else if (iLevel === 28) {
    sText = "Um deine Spieler sowie dein Personal, den Ausbau deines Stadions oder einen neuen Starstürmer bezahlen zu können, brauchst du Geld. Ein Weg, an frisches Kapital zu kommen sind Sponsoren.";
    sText += "</br></br>Gehe jetzt ins Menü \"Verein->Sponsoren\".";
    iNextButton = 0;
  } else if (iLevel === 29) {
    sText = "Im oberen Bereich kannst du deinen Haupt- bzw. Trikotsponsor auswählen. Zu Beginn des Spiels hast du bereits ein Angebot. Es ist nicht verkehrt, dieses erst einmal anzunehmen.";
    sText += "</br></br>Im Laufe der Zeit kommen noch weitere Angebote dazu. Für die nächste Saison musst du dann einen neuen Sponsor wählen.";
    iNextButton = 1;
  } else if (iLevel === 30) {
    sText = "Weiter unten hast du die Möglichkeit, insgesamt 12 Banden an Sponsoren zu vermieten. Im Laufe der Zeit bekommst du auch hier neue Angebote. Es lohnt sich also, auf dieser Seite hin und wieder mal vorbei zu schauen.";
    iNextButton = 1;
  } else if (iLevel === 31) {
    sText = "Zu guter letzt kannst du ganz unten noch für jeweils eine Saison zwei Spezialsponsoren wählen.";
    sText += "</br></br>Wenn du hier fertig bist, gehe zurück ins Hauptmenü.";
    iNextButton = 0;
  } else { // End
    sText = "Dies ist das Ende des Tutorials. Abschließend solltest du noch folgendes tun:";
    sText += "<ul>";
    sText += "<li>Dich auf dem Transfermarkt nach interessanten Spielern umsehen (\"Büro->Transfermarkt\")</li>";
    sText += "<li>Dein Stadion und dein Vereinsgelände ausbauen (\"Verein->Stadion\" und \"Verein->Vereinsgelände\")</li>";
    sText += "<li>Dir einen Merchandising-Vermarkter zulegen (\"Verein->Merchandising\")</li>";
    sText += "</ul>";
    sText += "</br>Für alles weitere, schau dir die <a href=\"/Home/UserManual\" target=\"_blank\">Anleitung</a> an oder stell deine Frage an <a href=\"mailto:mail@cornerkick-manager.de?subject=Cornerkick Frage\">mail@cornerkick-manager.de</a>. Viel Erfolg!";
    sText += "</br></br>Du kannst das Tutorial noch einmal starten, indem du oben rechts auf deine e-mail klickst und anschließend unter \"Optionen\" den Haken bei \"Tutorial zeigen\" setzt.";
    iNextButton = 0;
    setLevel(false, 0);
  }

  drawTutorialDialog(parent, iLevel, sText, sHeader, iNextButton, ttRef);
}

function drawTutorialDialog(parent, iLevel, sText, sHeader, iNextButton, ttRef) {
  // Remove existing tutorial dialogs
  var dgTt = document.getElementsByClassName("ui-dialog");
  for (var i = dgTt.length - 1; i >= 0; i--) {
    if (dgTt[i] && dgTt[i].parentElement && dgTt[i].contains(document.getElementById("divTutorial"))) {
      dgTt[i].parentElement.removeChild(dgTt[i]);
    }
  }

  var div0 = document.createElement("div");
  div0.id = "divTutorial";
  div0.title = "Tutorial - " + sHeader + " (" + (iLevel + 1).toString() + " / 33)";

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
  var buttons = [];

  if (iLevel === 0) {
    buttons.push({
      text: "nicht mehr anzeigen",
      class: "bnTutorial bnHide",
      icon: "ui-icon-closethick",
      click: function () {
        setLevel(false, iLevel);

        $(this).dialog('destroy').remove();
      }
    });
  } else {
    buttons.push({
      text: "von Vorne",
      class: "bnTutorial bnRestart",
      //icon: "ui-icon-closethick",
      click: function () {
        $.when(setLevel(true, 0)).done(function () {
          window.open('/Member/Desk', '_self', false);
          drawTutorial(parent, 0);
        });

        $(this).dialog('destroy').remove();
      }
    });

    buttons.push({
      text: "zurück",
      class: "bnTutorial bnPrev",
      //icon: "ui-icon-closethick",
      click: function () {
        $.when(setLevel(true, iLevel - 1)).done(function () {
          drawTutorial2(parent, iLevel - 1, ttRef);
        });

        $(this).dialog('destroy').remove();
      }
    });
  }

  if (iNextButton > 0) {
    var sNextButtonText = "überspringen";
    var sNextButtonIcon = "";
    if (iNextButton === 1) {
      sNextButtonText = "weiter";
      sNextButtonIcon = "ui-icon-check";
    }

    buttons.push({
      text: sNextButtonText,
      icon: sNextButtonIcon,
      class: "bnTutorial bnNext",
      id: "bnNext",
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
