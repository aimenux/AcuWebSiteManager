<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master"
	AutoEventWireup="true" CodeFile="SM209020.aspx.cs" Inherits="Pages_SM_SM209020"
	ValidateRequest="False" EnableViewStateMac="False" EnableViewState="False" EnableEventValidation="False" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource runat="server" ID="ds" TypeName="PX.SM.AUTemplateController" PrimaryView="Filter"
		Visible="True" Width="100%" PageLoadBehavior="SearchSavedKeys">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView runat="server" ID="form" DataSourceID="ds" DataMember="Filter" Width="100%" Style="width: 100%" Caption="Template">
		<Template>
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" 
				StartColumn="True">
			</px:PXLayoutRule>
			<px:PXSelector ID="edTemplateId" runat="server" DataField="TemplateID" AutoGenerateColumns="True"
				ValueField="TemplateID" TextField="Description" NullText="<NEW>" DataSourceID="ds" />
			<px:PXTextEdit runat="server" ID="edScreenId" Enabled="False" DataField="ScreenID" />
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" 
				StartColumn="True">
			</px:PXLayoutRule>
			<px:PXTextEdit runat="server" ID="edDescription" DataField="Description" />
			<px:PXTextEdit runat="server" ID="edGraph" DataField="Graph" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid runat="server" ID="grid" DataSourceID="ds" AutoGenerateColumns="Recreate"
		SkinID="Details" Width="100%" Style="width: 100%;" AutoAdjustColumns="True" Caption="Values">
		<AutoSize Enabled="true" Container="Window"></AutoSize>
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
				</Columns>
				<Mode AllowUpload="True" />
			</px:PXGridLevel>
		</Levels>
	</px:PXGrid>
</asp:Content>
