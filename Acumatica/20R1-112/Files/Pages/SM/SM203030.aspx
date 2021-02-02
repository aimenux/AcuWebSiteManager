<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	CodeFile="SM203030.aspx.cs" Inherits="Page_SM203030" Title="Trace"
	ValidateRequest="false" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">	
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Message" TypeName="PX.SM.TraceMaint" Visible="True">		
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Send" CommitChanges="true"  />
		</CallbackCommands>
	</px:PXDataSource>	
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="server">
<px:PXFormView ID="mailForm" runat="server" DataSourceID="ds" Style="z-index: 100;"
		Width="100%"  SkinID="Transparent" DataMember="Message">
	<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	<Template>
		<px:PXLabel ID="lblMailTo" runat="server" Style="z-index: 100; left: 9px; position: absolute;
			top: 9px">Mail To :</px:PXLabel>				
		<px:PXTextEdit ID="edMailTo" runat="server" DataField="MailTo" LabelID="lblMailTo"
			Style="z-index: 101; left: 63px; position: absolute; top: 9px" TabIndex="10" Width="162px">
		</px:PXTextEdit>
		<px:PXLabel ID="lblSubject" runat="server" Style="z-index: 102; left: 9px; position: absolute;
			top: 36px">Subject :</px:PXLabel>
		<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" LabelID="lblSubject"
			 Style="z-index: 103; left: 63px; position: absolute; top: 36px"
			TabIndex="11" Width="821px">
		</px:PXTextEdit>
		<px:PXLabel ID="lblBody" runat="server" Style="z-index: 104; left: 9px; position: absolute;
			top: 63px">Body :</px:PXLabel>
		<px:PXWikiEdit ID="edBody" runat="server" DataField="Body" Height="228px" LabelID="lblBody"
			Style="z-index: 105; left: 63px; position: absolute; top: 63px" TabIndex="12"
			Width="828px" >
		</px:PXWikiEdit>
	</Template>
</px:PXFormView>
</asp:Content>
