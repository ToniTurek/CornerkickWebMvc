var selectedDate = new Date();
$(document).ready(function() {
    $('#datepicker').datepicker({
        inline: true,
        onSelect: function(dateText, inst) {
            var d = new Date(dateText);
            $('#calendar').fullCalendar('gotoDate', d);
            selectedDate = d;
        }
    });
});

$(document).ready(function() {            
    $('#calendar').fullCalendar({
        theme: true,
        header: {
            left: '',
            center: '',
            right: ''
        },
        defaultView: 'agendaDay',
        editable: false,
        events: "/Home/GetEvents/",

        eventClick: function(calEvent, jsEvent, view) {
            var del = confirm('Delete this event?');
            if (!del)
                return;

            $.post('/Home/DeleteEvent', {'eventId' : calEvent.id },
                function(msg) {
                    if (msg != null)
                        if (msg.toLower() == 'exp') {
                        document.location = "/";
                        return;
                    }                            
                    $('#calendar').fullCalendar('refetchEvents');
                }
            );

        }
    });
});

function AddEvent() {
    var title = $('#eventTitle').val();
    if (title == null || title == '') {
        alert('Insert event title!');
        return;
    }
    var fromHours = $('#fromHours').val();
    var fromMinutes = $('#fromMinutes').val();
    var toHours = $('#toHours').val();
    var toMinutes = $('#toMinutes').val();

    var url = '/Home/AddEvent';
    var data = {
        dateStamp: Math.round(selectedDate.getTime()/1000),
        fromHours: fromHours,
        fromMinutes: fromMinutes,
        toHours: toHours,
        toMinutes: toMinutes,
        title: title,
        rnd: Math.random()
    };

    params = jQuery.param(data);
    type = "POST"

    $.ajax({
        url: url,
        type: "POST",
        data: params,
        success: function(msg) {
            if (msg != null)
                if (msg.toLower() == 'exp') {
                    document.location = "/";
                    return;
                }
            $('#eventTitle').val('');
            $('#calendar').fullCalendar('refetchEvents');
        }
    });
}
