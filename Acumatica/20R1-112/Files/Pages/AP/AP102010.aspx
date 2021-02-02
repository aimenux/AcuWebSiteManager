<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP102010.aspx.cs" Inherits="Page_AP102010" Title="Vendor Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APAccessDetail" PrimaryView="Vendor">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveVendor" />
			<px:PXDSCallbackCommand Name="FirstVendor" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="PrevVendor" HideText="True"/>
			<px:PXDSCallbackCommand Name="NextVendor" HideText="True"/>
			<px:PXDSCallbackCommand Name="LastVendor" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Vendor" DataMember="Vendor" DefaultControlID="edAcctCD" TemplateContainer="" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" />
			<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" AllowEdit="True" DataSourceID="ds" FilterByAllFields="True">
				<AutoCallBack Command="CancelVendor" Target="ds">
				</AutoCallBack>
			</px:PXSegmentMask>
			<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" Enabled="False" />
			<px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowPaging="True" AdjustPageSize="Auto" Caption="Restriction Groups" AllowSearch="True" SkinID="Details"
		TabIndex="200">
		<Levels>
			<px:PXGridLevel DataMember="Groups">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
					<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" RenderEditorText="True" AllowCheckAll="True" />
					<px:PXGridColumn AllowUpdate="False" DataField="GroupName" />
					<px:PXGridColumn AllowUpdate="False" DataField="Description" />
					<px:PXGridColumn AllowUpdate="False" DataField="Active" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn AllowUpdate="False" DataField="GroupType" Label="Visible To Entities" RenderEditorText="True" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included" />
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" Checked="True" DataField="Active" />
					<px:PXDropDown ID="edGroupType" runat="server" DataField="GroupType" Enabled="False" />
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" />
	</px:PXGrid>
</asp:Content>
