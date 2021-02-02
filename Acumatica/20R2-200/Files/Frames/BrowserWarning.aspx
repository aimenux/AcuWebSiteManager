<%@ Page Language="C#" AutoEventWireup="true" CodeFile="BrowserWarning.aspx.cs" Inherits="Frames_BrowserWarning" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Browser not supported</title>
	<style type="text/css">
		.main
		{
			padding-left: 40px;
			padding-right: 20px;
			padding-top: 30px;
			font-family: Arial;
		}

		.errMsg
		{
			font-size: 12pt;
			line-height: 25px;
		}

		.img
		{
			float: left;
			margin-right: 10px;
		}

		.grayBox
		{
			border: solid 1px #CCC;
			background-color: #F9F9F9;
			padding-top: 20px;
			padding-bottom: 25px;
			padding-left: 10px;
			padding-right: 20px;
		}

		.userAgent
		{
			font-size: 8pt;
			display: inline-block;
			padding-top: 10px;
		}
	</style>
</head>
<body>
	<form id="form1" runat="server">
		<div class="main">
		<div class="grayBox">
			<div class="img">
				<asp:Image ID="imgMessage" runat="server" ImageUrl="~/App_Themes/Default/Images/Message/error2.gif" />
			</div>
			<div class="errMsg">
				<px:PXLabel ID="lblMessage" CssClass="errMsg" runat="server" Encode="false" />
                <px:PXLabel ID="lblUserAgent" CssClass="userAgent" runat="server" />
			</div>
		</div>
		</div>
	</form>
</body>
</html>
