<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CT401000.aspx.cs" Inherits="Page_CT401000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter"
		TypeName="PX.Objects.CT.ExpiringContractsEng">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edBeginDate">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Expiration Window" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="BeginDate" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edEndDate" runat="server" DataField="EndDate" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkShowAutoRenewable" runat="server" DataField="ShowAutoRenewable" />
			<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" />
			<px:PXSegmentMask CommitChanges="True" ID="edTemplateID" runat="server" DataField="TemplateID" />
    	</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" Caption="Contracts" SkinID="PrimaryInquire" RestrictFields="True" SyncPosition="true" FastFilterFields="ContractCD,Description,CustomerID,Customer__AcctName">
		<Levels>
			<px:PXGridLevel DataMember="Contracts">
				<Columns>
					<px:PXGridColumn DataField="ContractCD" Width="120px" LinkCommand="viewContract" />
					<px:PXGridColumn DataField="Description" Width="240px" />
					<px:PXGridColumn DataField="TemplateID" Width="120px" />
					<px:PXGridColumn AllowNull="False" DataField="Type" Width="120px" RenderEditorText="True" />
					<px:PXGridColumn DataField="CustomerID" Width="120px" />
					<px:PXGridColumn AllowUpdate="False" DataField="Customer__AcctName" Width="200px" />
					<px:PXGridColumn AllowNull="False" DataField="Status" Width="117px" RenderEditorText="True" />
					<px:PXGridColumn DataField="StartDate" Width="90px" />
					<px:PXGridColumn DataField="ExpireDate" Width="90px" />
					<px:PXGridColumn AllowNull="False" DataField="AutoRenew" TextAlign="Center" Type="CheckBox" Width="120px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ContractBillingSchedule__LastDate" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ContractBillingSchedule__NextDate" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
