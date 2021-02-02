<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Announce.aspx.cs" Inherits="Frames_Announce" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Untitled Page</title>
	<meta http-equiv="content-script-type" content="text/javascript">
</head>
<body>
	<form id="form1" runat="server">
		<div style="padding-left: 4px; padding-right: 4px">
			<px:PXFormView ID="frmBottom" runat="server" Width="100%" Caption="Announcements"
				Height="100%" AllowCollapse="False">
				<Template>
					<div style="margin: 5px">
						<asp:Label ID="Label1" runat="server" Text="Welcome! <br><br>We are building an on-demand, Multi-tenant Accounting and ERP application for the SMB market that works with just an Internet connection and a standard web browser (no plug-ins) yet delivers a Windows like user experience and on-premise like performance. <br><br>While products like NetSuite and Intacct might appeal to early adopters, they are clearly inferior when compared to mainstream SMB / ERP market products. NetSuite and Intacct are harder to use, and have immature feature sets compared to on-premise competitors. For example they have some multi-currency, but do not support financial statement translation or VAT taxes. <br><br>We are designing our product so it will be a no compromises alternative from a feature, usability and performance perspective compared to on-premise products, yet also have the great advantages offered by on-demand - ane where we think we can deliver some cool innovations. <br><br><i>We think this is pretty exciting stuff; no one has achieved this - please stay tuned…</i> <br><br>Sincerely, <br>The Acumatica Team."></asp:Label>
					</div>
				</Template>
				<AutoSize Enabled="True" Container="Window" />
			</px:PXFormView>
		</div>
	</form>
</body>
</html>
