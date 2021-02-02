<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FS300900.aspx.cs" Inherits="Page_FS300900" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" ng-app="DB" >

    <head id="Head1" runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=10, user-scalable=yes">

    <title>Routes on the Map</title>

	</head>
	<body>   
		<!-- Main Container -->
		<div id="main-container">
			<!-- Title -->
			<form id="form1" runat="server">
				<px_pt:PageTitle ID="pageTitle" runat="server" CustomizationAvailable="false" HelpAvailable="false"/>
			</form>
			<!-- Routes -->
			<div class="container">
				<div id="routes-container">
				</div>
			</div>
			<!-- End Routes -->
		</div>
		<!-- End Main Container -->

		<!-- Teemplates ToolTip -->
		<%= infoRoute %>

		<!-- Global Variables -->
		<script type="text/javascript">
			var pageUrl= "<%= pageUrl %>";
			var baseUrl= window.location.protocol + "//" + window.location.host + "<%= applicationName %>" + "/(W(10000))/";
			var startDate = "<%= startDate %>";
			var RefNbr = "<%= RefNbr %>";
			var MapApiKey = "<%= apiKey %>";
			var mapCallBack = function () {};
			var mapClass;
		</script>

		<!-- Configuration files -->
		<script src="../../Shared/definition/GoogleMaps/ID.js" type="text/javascript"></script>
		<script src="../../Shared/definition/GoogleMaps/Cfg.js" type="text/javascript"></script>
		<script src="../../../../Scripts/jquery-3.1.1.min.js" type="text/javascript"></script>
		<script src="../../Shared/lib/plugins/dateformat.js" type="text/javascript"></script>
		<script src="../../Shared/lib/plugins/string.js" type="text/javascript"></script>

		<!-- The line below must be kept intact for Sencha Cmd to build your application -->
    	<script  id="microloader" data-app="706b980f-7d1a-42ae-8a2a-c6b676a81c8e" src="microloader.js"></script>
	</body>
</html>
