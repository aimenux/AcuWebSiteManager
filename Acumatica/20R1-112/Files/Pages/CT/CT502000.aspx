<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CT502000.aspx.cs" Inherits="Page_CT502000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.CT.RenewContracts" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edBeginDate">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edBeginDate" runat="server" DataField="RenewalDate" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID" />
			<px:PXSegmentMask CommitChanges="True" ID="edTemplateID" runat="server" DataField="TemplateID" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" Caption="Contracts" SkinID="PrimaryInquire" FastFilterFields="ContractID, Description, CustomerID, CustomerName" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" Width="30px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ContractID" DisplayFormat="&gt;aaaaaaaaaa" Visible="False" LinkCommand="editDetail" Width="120px" />
					<px:PXGridColumn AllowUpdate="False" DataField="Description" Width="251px" />
					<px:PXGridColumn AllowUpdate="False" DataField="TemplateID" DisplayFormat="CCCCCCCCCC" Width="120px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Type" Width="120px" RenderEditorText="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="CustomerID" DisplayFormat="CCCCCCCCCC" Width="120px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CustomerName" Width="200px" />
					<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" Width="90px" RenderEditorText="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="StartDate" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="ExpireDate" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastDate" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="NextDate" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="400" />
	</px:PXGrid>
</asp:Content>
