<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Maintenance.aspx.cs" Inherits="Frames_Update" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Site maintenance</title>
	<meta http-equiv="content-script-type" content="text/javascript">
</head>
<body>
	<form id="frmBottom" runat="server">
		<div style="Width:99%; Height:100%; padding:5px;" >
			<div style="padding: 20px; background-color: Gray; color:White;">
				<asp:Label ID="lblMessage" runat="server" Font-Size="X-Large" Font-Bold="True" Style="margin-top: 10px;
					padding-bottom: 10px; position: static"></asp:Label>				
			</div>
			<div style="background-color:White; padding:1%; padding-right:2%; padding-left:4%; width:94%; ">
				<asp:Table id="statusTable" runat="server" BorderWidth="1" BorderColor="Black" cellpadding="5" cellspacing="0"  Width="100%" Visible="false" >
					<asp:TableRow>
						<asp:TableCell style="text-align:left; width:15%;">
							<asp:Label ID="lblPersentCaption" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static"></asp:Label>
						</asp:TableCell>						
						<asp:TableCell style="text-align:right; width:35%;">
							<asp:Label ID="lblPersent" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static" Text="0%" ></asp:Label>
						</asp:TableCell>
						<asp:TableCell style="width:50%;" />							
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell style="text-align:left; width:15%;">
							<asp:Label ID="lblActionCaption" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static"></asp:Label>
						</asp:TableCell>						
						<asp:TableCell style="text-align:right; width:35%;">
							<asp:Label ID="lblAction" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static"></asp:Label>
						</asp:TableCell>
						<asp:TableCell style="width:50%;" />
					</asp:TableRow>										
				</asp:Table>
				<asp:Table id="applicationTable" runat="server" BorderWidth="1" BorderColor="Black" cellpadding="5" cellspacing="0"  Width="100%" Visible="false" >
					<asp:TableRow>
						<asp:TableCell style="text-align:left; width:45%;">
							<asp:Label ID="lblDatabaseNameCaption" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static"></asp:Label>
						</asp:TableCell>						
						<asp:TableCell style="text-align:right; width:25%;">
							<asp:Label ID="lblDatabaseName" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static" Text="0%" ></asp:Label>
						</asp:TableCell>
						<asp:TableCell style="width:50%;" />							
					</asp:TableRow>
				</asp:Table>
				<asp:Table id="questionTable" runat="server" BorderWidth="1" BorderColor="Black" cellpadding="5" cellspacing="0"  Width="100%" Visible="false" >
					<asp:TableRow>
						<asp:TableCell style="text-align:left; width:45%;">
							<asp:Label ID="lblDatabaseVersionCaption" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static"></asp:Label>
						</asp:TableCell>						
						<asp:TableCell style="text-align:right; width:25%;">
							<asp:Label ID="lblDatabaseVersion" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static" Text="0%" ></asp:Label>
						</asp:TableCell>
						<asp:TableCell style="width:50%;" />							
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell style="text-align:left; width:45%;">
							<asp:Label ID="lblSiteVersionCaption" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static"></asp:Label>
						</asp:TableCell>						
						<asp:TableCell style="text-align:right; width:25%;">
							<asp:Label ID="lblSiteVersion" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static" Text="0%" ></asp:Label>
						</asp:TableCell>
						<asp:TableCell style="width:50%;" />							
					</asp:TableRow>
					<asp:TableRow>
						<asp:TableCell style="text-align:left; width:45%;">
							<asp:Label ID="lblQuestion" runat="server" Font-Size="Large" ForeColor="Black" Style="margin-top: 10px;
								padding-bottom: 10px; position: static"></asp:Label>
						</asp:TableCell>						
						<asp:TableCell style="text-align:right; width:25%;">
							<asp:Button ID="btnYes" runat="server" Width="100px" Font-Bold="True" Font-Size="Smaller" ForeColor="Black" 
								Style="margin-top: 0px;	padding-bottom: 0px; position: static" Text="OK"
								OnClick="btnYes_OnClick"></asp:Button>
							<%--&nbsp;--%>
							<%--<asp:Button ID="btnNo" runat="server" Width="100px" Font-Bold="True" Font-Size="Smaller" ForeColor="Black" 
								Style="margin-top: 0px;	padding-bottom: 0px; position: static" Text="No"
								OnClick="btnNo_OnClick"></asp:Button>--%>
						</asp:TableCell>
						<asp:TableCell style="width:50%;" />
					</asp:TableRow>										
				</asp:Table>
			</div>
		</div>
	</form>
</body>
</html>
