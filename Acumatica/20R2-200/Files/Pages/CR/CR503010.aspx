<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR503010.aspx.cs" Inherits="Page_CR503010"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
		PrimaryView="Items" TypeName="PX.Objects.CR.AssignLeadMassProcess" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grdItems" CommitChanges="True" Name="Items_ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdItems" CommitChanges="True" Name="Items_BAccount_ViewDetails" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdItems" CommitChanges="True" Name="Items_BAccountParent_ViewDetails" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grdItems" runat="server" DataSourceID="ds" Height="150px" Width="100%"
		ActionsPosition="Top" Caption="Matching Records" AllowPaging="True" AdjustPageSize="auto" SkinID="PrimaryInquire" FastFilterFields="DisplayName,FullName" AutoGenerateColumns="AppendDynamic" RestrictFields="True">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowShowHide="False" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="40px" AutoCallBack="True" />
					<px:PXGridColumn DataField="DisplayName" LinkCommand="Items_ViewDetails" />
					<px:PXGridColumn DataField="Title" />
					<px:PXGridColumn DataField="FirstName" />
					<px:PXGridColumn DataField="LastName" />
					<px:PXGridColumn DataField="Salutation" />
                    <px:PXGridColumn DataField="DuplicateStatus" />
					<px:PXGridColumn DataField="BAccount__AcctCD" LinkCommand="Items_BAccount_ViewDetails" />
					<px:PXGridColumn DataField="FullName" />
					<px:PXGridColumn DataField="BAccountParent__AcctCD" LinkCommand="Items_BAccountParent_ViewDetails" />
					<px:PXGridColumn DataField="BAccountParent__AcctName" />
					<px:PXGridColumn DataField="Status" />
					<px:PXGridColumn DataField="Resolution" />
					<px:PXGridColumn DataField="ClassID" />
					<px:PXGridColumn DataField="Source" />
					<px:PXGridColumn DataField="State__name" />
					<px:PXGridColumn DataField="Address__CountryID" />
					<px:PXGridColumn DataField="Address__City" />
					<px:PXGridColumn DataField="Address__PostalCode" />
					<px:PXGridColumn DataField="Address__AddressLine1" />
					<px:PXGridColumn DataField="Address__AddressLine2" />
					<px:PXGridColumn DataField="EMail" />
					<px:PXGridColumn DataField="Phone1" DisplayFormat="+#(###) ###-####" />
					<px:PXGridColumn DataField="Phone2" DisplayFormat="+#(###) ###-####" />
					<px:PXGridColumn DataField="Phone3" DisplayFormat="+#(###) ###-####" />
					<px:PXGridColumn DataField="Fax" DisplayFormat="+#(###) ###-####" />
					<px:PXGridColumn DataField="WorkgroupID" />
					<px:PXGridColumn DataField="OwnerID" DisplayMode="Text" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastIncomingActivityDate" DisplayFormat="g" TimeMode="True" />
                    <px:PXGridColumn DataField="CRActivityStatistics__LastOutgoingActivityDate" DisplayFormat="g" TimeMode="True" />
					<px:PXGridColumn DataField="CreatedByID_Creator_Username" SyncVisible="False" SyncVisibility="False" Visible="False" />
					<px:PXGridColumn DataField="CreatedDateTime" />
					<px:PXGridColumn DataField="LastModifiedByID_Modifier_Username" Visible="False" SyncVisible="False" SyncVisibility="False"/>
					<px:PXGridColumn DataField="LastModifiedDateTime" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar PagerVisible="False"/>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
	</px:PXGrid>
</asp:Content>
