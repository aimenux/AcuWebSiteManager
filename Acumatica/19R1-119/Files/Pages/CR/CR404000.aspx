<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR404000.aspx.cs" Inherits="Page_CR404000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" TypeName="PX.Objects.CR.BAccountEnq"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="FilteredItems_BAccountParent_ViewDetails" />
			<pxa:PXExtendedDSCallbackCommand Name="FilteredItems_AddNew" ForDashboard="true" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="_EPCompanyTree_Tree_" TreeKeys="WorkgroupID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter" Caption="Selection">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" />
			<px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True" ControlSize="XM"/>
			<px:PXSelector AutoRefresh="True" CommitChanges="True" ID="edOwnerID" runat="server" DataField="OwnerID"/>
			<px:PXCheckBox CommitChanges="True" ID="chkMyOwner" runat="server" Checked="True" DataField="MyOwner" />
			<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="False" />
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" Merge="True" ControlSize="XM"/>
			<px:PXSelector AutoRefresh="True" CommitChanges="True" ID="edWorkGroupID" runat="server" DataField="WorkGroupID" />
			<px:PXCheckBox CommitChanges="True" ID="chkMyWorkGroup" runat="server" DataField="MyWorkGroup" />
			<px:PXLayoutRule ID="PXLayoutRule5"  runat="server" Merge="False" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" 
				ControlSize="S" SuppressLabel="True"/>
			<px:PXCheckBox CommitChanges="True" ID="ckbShowProspect" runat="server" DataField="ShowProspect" AlignLeft="True"/>
			<px:PXCheckBox CommitChanges="True" ID="chbShowCustomers" runat="server" DataField="ShowCustomer" AlignLeft="True" />
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" 
				ControlSize="S" SuppressLabel="True"/>
			<px:PXCheckBox CommitChanges="True" ID="ckbShowVendors" runat="server" DataField="ShowVendor" AlignLeft="True" />
			</Template>
	</px:PXFormView> 
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px"
		Width="100%" ActionsPosition="Top" Caption="Business Accounts" AllowPaging="true" AdjustPageSize="auto" 
		SkinID="Inquire" FastFilterFields="AcctCD,AcctName" AutoGenerateColumns="AppendDynamic" PageSize="40" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="FilteredItems">
				<Columns>
					<px:PXGridColumn DataField="Type" Width="80px" />
					<px:PXGridColumn DataField="AcctCD" Width="150px" LinkCommand="FilteredItems_ViewDetails"/>
					<px:PXGridColumn DataField="AcctName" Width="200px" />
					<px:PXGridColumn DataField="Status" Width="90px" />
					<px:PXGridColumn DataField="ClassID" Width="90px" />
					<px:PXGridColumn DataField="BAccountParent__AcctCD" Width="150px" LinkCommand="FilteredItems_BAccountParent_ViewDetails" />
					<px:PXGridColumn DataField="BAccountParent__AcctName" Width="200px" />
					<px:PXGridColumn DataField="State__name" Width="160px" />
                    <px:PXGridColumn DataField="Address__CountryID" Width="160px" />
					<px:PXGridColumn DataField="Address__City" Width="80px" />
                    <px:PXGridColumn DataField="Address__PostalCode" Width="80px" />
					<px:PXGridColumn DataField="Address__AddressLine1" Width="300px" />
					<px:PXGridColumn DataField="Address__AddressLine2" Width="300px" />
					<px:PXGridColumn DataField="Contact__EMail" Width="190px" />
					<px:PXGridColumn DataField="Contact__Phone1" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__Phone2" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__Phone3" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__Fax" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__WebSite" Width="140px" />
                    <px:PXGridColumn DataField="Contact__DuplicateStatus" Width="140px" />
					<px:PXGridColumn DataField="WorkgroupID" Width="110px" />
					<px:PXGridColumn DataField="OwnerID" Width="110px" DisplayMode="Text" />
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="CreatedDateTime" Width="90px" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="cmdItemDetails" PagerVisible="False">
			<CustomItems>
				<px:PXToolBarButton Text="Business Accounts Details" Tooltip="Business Accounts Details"
					Key="cmdItemDetails" Visible="false">
					<Images Normal="main@DataEntry" />
					<AutoCallBack Command="FilteredItems_ViewDetails" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	 
					<ActionBar GroupIndex="0" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar> 
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>
