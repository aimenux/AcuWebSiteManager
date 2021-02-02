<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR503310.aspx.cs" Inherits="Page_CR503310"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		TypeName="PX.Objects.CR.AssignBAccountMassProcess" PrimaryView="Items" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="False" DependOnGrid="grdItems" CommitChanges="True"
				Name="Items_ViewDetails" />
			<px:PXDSCallbackCommand Visible="false" DependOnGrid="gridItems" Name="Items_BAccountParent_ViewDetails" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
		Width="100%" Caption="Matching Records" AllowPaging="True" AdjustPageSize="auto"
		SkinID="PrimaryInquire" AutoCallback = "true" AutoGenerateColumns="AppendDynamic" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="40px" />
					<px:PXGridColumn DataField="Type" Width="80px" />
					<px:PXGridColumn DataField="AcctCD" Width="150px" LinkCommand="Items_ViewDetails"/>
					<px:PXGridColumn DataField="AcctName" Width="200px" />
					<px:PXGridColumn DataField="Status" Width="90px" />
					<px:PXGridColumn DataField="ClassID" Width="90px" />
					<px:PXGridColumn DataField="BAccountParent__AcctCD" Width="150px" LinkCommand="Items_BAccountParent_ViewDetails" />
					<px:PXGridColumn DataField="BAccountParent__AcctName" Width="200px" />
					<px:PXGridColumn DataField="State__name" Width="160px" />
                    <px:PXGridColumn DataField="Address__CountryID" Width="160px" />
					<px:PXGridColumn DataField="Address__City" Width="80px" />
					<px:PXGridColumn DataField="Address__AddressLine1" Width="300px" />
					<px:PXGridColumn DataField="Address__AddressLine2" Width="300px" />
					<px:PXGridColumn DataField="Contact__EMail" Width="190px" />
					<px:PXGridColumn DataField="Contact__Phone1" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__Phone2" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__Phone3" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__Fax" DisplayFormat="+#(###) ###-####" Width="130px" />
					<px:PXGridColumn DataField="Contact__WebSite" Width="140px" />
                    <px:PXGridColumn DataField="Contact__DuplicateStatus" Width="140px" />
					<px:PXGridColumn DataField="TaxZoneID" Width="80px" />
					<px:PXGridColumn DataField="Location__CCarrierID" Width="100px" />
					<px:PXGridColumn DataField="WorkgroupID" Width="110px" />
					<px:PXGridColumn DataField="OwnerID" Width="110px" DisplayMode="Text" />
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="CreatedDateTime" Width="90px" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Width="110px" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="LastModifiedDateTime" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
