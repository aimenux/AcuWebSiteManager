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
					<px:PXGridColumn DataField="Type" />
					<px:PXGridColumn DataField="AcctCD" LinkCommand="Items_ViewDetails"/>
					<px:PXGridColumn DataField="AcctName" />
					<px:PXGridColumn DataField="Status" />
					<px:PXGridColumn DataField="ClassID" />
					<px:PXGridColumn DataField="BAccountParent__AcctCD" LinkCommand="Items_BAccountParent_ViewDetails" />
					<px:PXGridColumn DataField="BAccountParent__AcctName" />
					<px:PXGridColumn DataField="State__name" />
                    <px:PXGridColumn DataField="Address__CountryID" />
					<px:PXGridColumn DataField="Address__City" />
					<px:PXGridColumn DataField="Address__AddressLine1" />
					<px:PXGridColumn DataField="Address__AddressLine2" />
					<px:PXGridColumn DataField="Contact__EMail" />
					<px:PXGridColumn DataField="Contact__Phone1" DisplayFormat="+#(###) ###-####" />
					<px:PXGridColumn DataField="Contact__Phone2" DisplayFormat="+#(###) ###-####" />
					<px:PXGridColumn DataField="Contact__Phone3" DisplayFormat="+#(###) ###-####" />
					<px:PXGridColumn DataField="Contact__Fax" DisplayFormat="+#(###) ###-####" />
					<px:PXGridColumn DataField="Contact__WebSite" />
                    <px:PXGridColumn DataField="Contact__DuplicateStatus" />
					<px:PXGridColumn DataField="TaxZoneID" />
					<px:PXGridColumn DataField="Location__CCarrierID" />
					<px:PXGridColumn DataField="WorkgroupID" />
					<px:PXGridColumn DataField="OwnerID" DisplayMode="Text" />
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="CreatedDateTime" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="LastModifiedDateTime" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
