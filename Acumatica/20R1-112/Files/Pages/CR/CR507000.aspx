<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR507000.aspx.cs" Inherits="Page_CR507000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CR.CRCaseReleaseProcess"
		PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Items_ViewDetails" DependOnGrid="grid" Visible="false" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Selection" DataMember="Filter">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM"
				ControlSize="M" />
			<px:PXSelector CommitChanges="True" ID="edCaseClassID" runat="server" DataField="CaseClassID"
				FilterByAllFields="True" />
			<px:PXSelector CommitChanges="True" ID="edCustomerClassID" runat="server" DataField="CustomerClassID"
				FilterByAllFields="True" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="s"
				ControlSize="M" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID"
				FilterByAllFields="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edContractID" runat="server" DataField="ContractID"
				FilterByAllFields="True" /></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" SkinID="Inquire" Height="150px"
		Width="100%" Caption="Cases" AllowPaging="true">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn AllowNull="False" AllowCheckAll="True" DataField="Selected" SyncVisible="False"
						Visible="True" AllowShowHide="False" TextAlign="Center" Type="CheckBox" Width="20px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CaseCD" LinkCommand="Items_ViewDetails" />
					<px:PXGridColumn AllowUpdate="False" DataField="Subject" />
					<px:PXGridColumn AllowUpdate="False" DataField="CaseClassID" DisplayFormat="&gt;aaaaaaaaaa"
						Width="90px" />
					<px:PXGridColumn DataField="Customer__AcctName" />
					<px:PXGridColumn DataField="Customer__ClassID" />
					<px:PXGridColumn AllowUpdate="False" DataField="ContractID" DisplayFormat="CCCCCCCCCC"
						Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LocationID" DisplayFormat="&gt;AAAAAA"
						Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="TimeBillable" />
					<px:PXGridColumn AllowUpdate="False" DataField="OverTimeBillable" />
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" />
					<px:PXGridColumn AllowUpdate="False" DataField="ResolutionDate" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdItemDetails" PagerVisible="False">
			<PagerSettings Mode="NextPrevFirstLast" />
			<CustomItems>
				<px:PXToolBarButton Key="cmdItemDetails" Tooltip="Case Details" Visible="false">
					<AutoCallBack Command="Items_ViewDetails" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>
					<ActionBar GroupIndex="0" />
					<Images Normal="main@DataEntry" />
				</px:PXToolBarButton>
			</CustomItems>
			<Actions>
				<FilterShow Enabled="False" />
				<FilterSet Enabled="False" />
			</Actions>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
