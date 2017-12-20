<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<asp:Content ID="Content3" ContentPlaceHolderID="HeaderContent" runat="server">
    <!--script type="text/javascript" src="<%= Url.Content("~/Scripts/Views/AuthIndex.js") %>"></script-->
<script language="javascript">
    var selectedDate = new Date();
    $(document).ready(function () {
        $('#datepicker').datepicker({
            inline: true,
            onSelect: function (dateText, inst) {
                var d = new Date(dateText);
                $('#calendar').fullCalendar('gotoDate', d);
                selectedDate = d;
            }
        });
    });

    $(document).ready(function () {
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

            eventClick: function (calEvent, jsEvent, view) {
                var del = confirm('Delete this event?');
                if (!del)
                    return;

                $.post('/Home/DeleteEvent', { 'eventId': calEvent.id },
                function (msg) {
                    if (msg != null)
                        //if (msg.toLower() == 'exp') {
                        if (msg == 'exp') {
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
        var currentTime = selectedDate.getTime();
        var localOffset = (-1)*selectedDate.getTimezoneOffset() * 60000;

        var data = {
            dateStamp: Math.round(new Date(currentTime+localOffset).getTime() / 1000),
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
            success: function (msg) {
                if (msg != null)
                    //if (msg.toString().toLower() == 'exp') {
                    if (msg == 'exp') {
                        document.location = "/";
                        return;
                    }
                $('#eventTitle').val('');
                $('#calendar').fullCalendar('refetchEvents');
            }
        });
    }
</script>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	My Calendar
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

        <div style="float:left">
            <div id="datepicker"></div><br />
            <div style="padding-bottom:5px">
                <div style="width:40px;float:left;">Title:</div><input type="text" style="width:185px" id="eventTitle" />
            </div>
            <div>
                <div style="width:40px;float:left;">Start:</div>
                <select id="fromHours">
                    <option>09</option>
                    <option>10</option>
                    <option>11</option>
                    <option>12</option>
                    <option>13</option>
                    <option>14</option>
                    <option>15</option>
                    <option>16</option>
                    <option>17</option>
                    <option>18</option>
                    <option>19</option>
                    <option>20</option>
                </select>:<select id="fromMinutes">
                    <option>00</option>
                    <option>15</option>
                    <option>30</option>
                    <option>45</option>
                </select>
            </div>
            <div>
                <div style="width:40px;float:left;">Finish:</div>                                           
                <select id="toHours">
                    <option>09</option>
                    <option>10</option>
                    <option>11</option>
                    <option>12</option>
                    <option>13</option>
                    <option>14</option>
                    <option>15</option>
                    <option>16</option>
                    <option>17</option>
                    <option>18</option>
                    <option>19</option>
                    <option>20</option>
                </select>:<select id="toMinutes">
                    <option>00</option>
                    <option>15</option>
                    <option>30</option>
                    <option>45</option>                
                </select>
            </div>
            <div>
                <div style="width:40px;float:left;">&nbsp;</div>
                <input type="button" value="Add" onclick="AddEvent()" />
            </div>
        </div>
        <div id='calendar' style="float:right; width:680px"></div>        
        <div>&nbsp;</div>
        <div style="clear:both">&nbsp;</div>

</asp:Content>
