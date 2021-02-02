<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FS301300.aspx.cs" Inherits="Page_FS301300" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" ng-app="DB" >
<head id="Head1" runat="server">
    <meta http-equiv="content-type" content="text/html; charset=UTF-8">

    <title>Staff Calendar Board</title>
</head>
<body>   

    <!-- Main Container -->
    <div class="container-fluid main-container">

        <!-- Title -->
        <form id="form1" runat="server">
            <px_pt:PageTitle ID="pageTitle" runat="server" CustomizationAvailable="false" HelpAvailable="false"/>
        </form>
        <!-- End Title -->
        
        <!-- Scheduler -->
        <div class="row-fluid container">
            <div id="scheduler-container" class="span12">
            </div>
        </div>
        <!-- End Scheduler -->

        <!-- Teemplates ToolTip -->
        <%= appointmentBodyTemplate %>
        <%= toolTipTemplateServiceOrder %>
        <%= toolTipTemplateAppointment %>

        <!-- End ToolTip -->
    </div>
    <!-- End Main Container -->

    <!-- Variables Globales -->
    <script type="text/javascript">
        var pageUrl = "<%= pageUrl %>";
        var baseUrl= window.location.protocol + "//" + window.location.host + "<%= applicationName %>" + "/(W(10000))/";
        var startDate = "<%= startDate %>";
        var DefaultEmployee = "<%= DefaultEmployee %>";
        var ExternalEmployee = "<%= ExternalEmployee  %>";
        var RefNbr = "<%= RefNbr %>";
        var ExternalCustomerID = "<%= CustomerID %>";
        var ExternalSMEquipmentID = "<%= SMEquipmentID %>";
        var AppSource = "<%= AppSource %>";
    </script>

    <!--Definitions-->
    <script src="../../Shared/definition/Calendars/ID.js" type="text/javascript"></script>
    <script src="../../Shared/definition/Calendars/FieldsLabel.js" type="text/javascript"></script>
    <script src="../../Shared/definition/Calendars/FieldsName.js" type="text/javascript"></script>

    <script src="../../../../Scripts/jquery-3.1.1.min.js" type="text/javascript"></script>
    <script src="../../Shared/lib/plugins/dateformat.js" type="text/javascript"></script>

    <!-- The line below must be kept intact for Sencha Cmd to build your application -->
    <script  id="microloader" data-app="cc33f64b-33b9-4343-9765-941e40d8cb4e" src="microloader.js"></script>
</body>
</html>
