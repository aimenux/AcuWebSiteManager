<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA203000.aspx.cs" Inherits="Page_CA203000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="EntryType" TypeName="PX.Objects.CA.EntryTypeMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" Visible="False" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Delete" Visible="False" />
			<px:PXDSCallbackCommand Name="First" Visible="False" />
			<px:PXDSCallbackCommand Name="Previous" Visible="False" />
			<px:PXDSCallbackCommand Name="Next" Visible="False" />
			<px:PXDSCallbackCommand Name="Last" Visible="False" />
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" 
		AllowPaging="True" ActionsPosition="Top" AllowSearch="True" DataSourceID="ds" 
		SkinID="Primary" TabIndex="100" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataMember="EntryType">
				<Columns>
					<px:PXGridColumn DataField="EntryTypeId" AutoCallBack="True"/>
					<px:PXGridColumn DataField="DrCr" Type="DropDownList" />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn DataField="Module" Type="DropDownList" AutoCallBack="True" />
					<px:PXGridColumn DataField="ReferenceID" AutoCallBack="True" />
					<px:PXGridColumn DataField="BranchID" />
					<px:PXGridColumn DataField="AccountID" CommitChanges="True"/>
					<px:PXGridColumn DataField="SubID" />
					<px:PXGridColumn DataField="UseToReclassifyPayments" AutoCallBack="True" Type="CheckBox" TextAlign="Center"/>
					<px:PXGridColumn DataField="CashAccountID" AutoCallBack="True"/>
					<px:PXGridColumn DataField="Consolidate" Type="CheckBox" TextAlign="Center"/>
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXMaskEdit ID="edEntryTypeId" runat="server" DataField="EntryTypeId"  />
					<px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" AutoRefresh="True" />
					<px:PXSegmentMask Size="xs" ID="edAccountID" runat="server" DataField="AccountID" CommitChanges="true" />
					<px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" />
					<px:PXSelector CommitChanges="True" ID="edReferenceID" runat="server" DataField="ReferenceID" AutoRefresh="True" />
					<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
					<px:PXCheckBox ID="chkUseToReclassifyPayments" runat="server" DataField="UseToReclassifyPayments" CommitChanges="True" />
					<px:PXCheckBox ID="chkConsolidate" runat="server" DataField="Consolidate" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar>
		</ActionBar>

<AutoSize Enabled="True" Container="Window" MinHeight="200"></AutoSize>
	</px:PXGrid>
</asp:Content>
