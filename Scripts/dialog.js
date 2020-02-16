/*
 Required jquery files:
  <link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
  <script type="text/javascript" src="~/Scripts/jquery-2.0.0.min.js"></script>
  <script type="text/javascript" src="~/Scripts/jquery-ui-1.12.1.min.js"></script>
 */

function getDialog(parent, sText, sTitle, fctToExecute) {
  var div0 = document.createElement("div");
  div0.id = "divDialogYN";
  div0.title = sTitle;

  // Contract length
  var div1 = document.createElement("div");
  div1.style.position = "relative";
  div1.style.width = "90%";
  div1.style.height = "auto";
  div1.innerHTML = sText;
  div0.appendChild(div1);

  parent.appendChild(div0);

  $(div0).dialog({
    autoOpen: true,
    buttons: [
      {
        text: "Bestätigen",
        icon: "ui-icon-check",
        //class: "foo bar baz",
        id: "bnOk",
        click: function () {
          fctToExecute();
          $(this).dialog('destroy').remove();
        }
      },
      {
        text: "Abbrechen",
        icon: "ui-icon-closethick",
        click: function () {
          $(this).dialog('destroy').remove();
        }

        // Uncommenting the following line would hide the text,
        // resulting in the label being used as a tooltip
        //showText: false
      }
    ]
  });
}
