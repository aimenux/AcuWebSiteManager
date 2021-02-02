<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TraceItem.ascx.cs" Inherits="PX.Web.Controls.Controls_TraceItem" %>
<style type="text/css"> 
	body {font-family:"Verdana";font-weight:normal;font-size: .7em;color:black;} 
	pre {font-family:"Lucida Console"; background-color:#ffffcc }
	.error {margin-bottom: 10px; font-size:small; font-stretch:narrower; }
	.type {margin-bottom: 10px; font-size:small; font-stretch:narrower; vertical-align:top; }
	.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }
	.error {margin-bottom: 10px; word-wrap: break-word}
	.data {margin-bottom: 10px;}
	.cantainer {width:100%; padding-left:10px; padding-right:5px; table-layout:fixed; border: 1px ridge #C9C9C9;}
	.button { width:auto; border: 1px ridge #C2C2C2; cursor:pointer; padding:2px; font-weight:bold; color:navy;}
	.details {width:100%;  word-wrap: break-word; font-family:"Lucida Console"; background-color:#ffffcc }
</style> 
<asp:Panel ID="pnlTraceItem" runat="server" Width="100%">
	<table class="cantainer" >
		<tr>
			<td style="width:10%;vertical-align:top; " >
				<b><asp:Label ID="lblType" CssClass="type" runat="server" Text="Type" ForeColor="DarkRed" /></b>
			</td>
			<td style="width:80%" colspan="3" >
				<b><asp:Label ID="lblCaption" CssClass="error" runat="server" Text="Message" /></b>
			</td>
			<td style="width:10%; text-align:right; padding:3px; vertical-align:top; ">
				<span  class="button" onclick="SendScript(this);" >
					<px:PXImage runat="server" ID="imgSend" ImageUrl="main@MailSend" />
					<span style="text-decoration:underline">Send</span>
				</span>
				<asp:Button ID="btnSend" name="btnSend" runat="server" onclick="btnSend_Click" style="display:none;" />
			</td>
		</tr>	
		<tr>
			<td style="width:10%" />
			<td style="width:15%"> <asp:Label ID="lblDate" CssClass="data" runat="server" Text="Raised At:" /> </td>
			<td style="width:10%"> <asp:Label ID="lblScreen" CssClass="data" runat="server" Text="Screen ID:" />	</td>	
			<td style="width:15%"> <asp:Label ID="lblSource" CssClass="data" runat="server" Text="Data Source ID:" />	</td>
			<td style="width:50%"/>
		</tr>
		<tr>
			<td colspan="5" style="width:100%"  >
				<px:PXImage runat="server" ID="outputImg" name="outputImg" ImageUrl="tree@Expand" onclick="Togle(this)" />
				<asp:Label ID="Label1" runat="server" Text="Details:" />
				<br/>
				<div id="outputDiv" name="outputDiv" class="details" style="display:none;">
					<asp:Label ID="lblDetails" runat="server" Font-Names="Lucida Console" Font-Size="1.0em" />
				</div>
			</td>
		</tr>
	</table>
</asp:Panel>