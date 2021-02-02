<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AuditItem.ascx.cs" Inherits="PX.Web.Controls.Controls_AuditItem" %>
<style type="text/css"> 
	body {font-family:"Verdana";font-weight:normal;font-size: .7em;color:black;} 
	pre {font-family:"Lucida Console"; background-color:#ffffcc }
	.error {margin-bottom: 10px; font-size:small; font-stretch:narrower; word-wrap: break-word; color:#4E4E4E }
	.type {margin-bottom: 10px; font-size:small; font-stretch:narrower; vertical-align:top; }
	.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }
	.data {margin-bottom: 10px; font-size:small; font-stretch:narrower; word-wrap:break-word; font-weight:normal; font-weight:bold; color:#4E4E4E}
	.container {width:100%; padding-left:10px; padding-right:5px; table-layout:fixed; border: 1px ridge #C9C9C9;}
	.button { width:auto; border: 1px ridge #C2C2C2; cursor:pointer; padding:2px; font-weight:bold; color:navy;}
</style> 
<asp:Panel ID="pnlTraceItem" runat="server" Width="100%">
	<table class="container" >
		<tr>
			<td style="width:100%;vertical-align:top; " >
				<px:PXPanel ID="PXPanel1" runat="server" ContentLayout-Orientation="Horizontal" ContentLayout-Layout="Stack" RenderStyle="Simple" ContentLayout-SpacingSize="Medium" Height="17px">
					<b>
						<asp:Label ID="lblDate" CssClass="data" runat="server" Text="Date:" />
						<asp:Label ID="txtDate" CssClass="error" runat="server" Text=""  />
					</b>
					<b>
						<asp:Label ID="lblUser" CssClass="data" runat="server" Text="User:" />
						<asp:Label ID="txtUser" CssClass="error" runat="server" Text=""  />
					</b>
					<b>
						<asp:Label ID="lblScreen" CssClass="data" runat="server" Text="Screen ID:" />
						<asp:Label ID="txtScreen" CssClass="error" runat="server" Text="" />
					</b>
				</px:PXPanel>
			</td>
		</tr>	
		<tr>
			<td colspan="1" style="width:100%"  >
				<px:PXImage runat="server" ID="outputImg" name="outputImg" ImageUrl="tree@Expand" onclick="Togle(this)" />
				<asp:Label ID="Label1" runat="server" Text="Changes:" />
				<br/>
				<div id="outputDiv" name="outputDiv" style="width:100%; display:none; word-wrap: break-word">
					<asp:Table id="tblDetails" runat="server" >
					</asp:Table>
				</div>
			</td>
		</tr>
	</table>
</asp:Panel>