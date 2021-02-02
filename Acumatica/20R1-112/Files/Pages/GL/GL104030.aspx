<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL104030.aspx.cs" Inherits="Page_GL104030"
	Title="Account Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLAccessBySub"
		PrimaryView="Sub">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveSub" />
			<px:PXDSCallbackCommand Name="FirstSub" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="PrevSub" HideText="True"/>
			<px:PXDSCallbackCommand Name="NextSub" HideText="True"/>
			<px:PXDSCallbackCommand Name="LastSub" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"  TabIndex="100" Width="100%"
		Caption="Subaccount" DataMember="Sub" DefaultControlID="edSubCD">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth='SM' ControlSize="M" />
			<px:PXSegmentMask ID="edSubCD" runat="server" DataField="SubCD">
				<AutoCallBack Command="CancelSub" Target="ds">
				</AutoCallBack>
			</px:PXSegmentMask>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server"  Height="150px" Width="100%"
		AllowPaging="True" AdjustPageSize="Auto" Caption="Restriction Groups" AllowSearch="True"
		SkinID="Details" TabIndex="200">
		<Levels>
			<px:PXGridLevel DataMember="Groups">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
					<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
					<px:PXGridColumn  DataField="GroupName" />
					<px:PXGridColumn  DataField="Description" />
					<px:PXGridColumn  DataField="Active" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="GroupType" Type="DropDownList" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth='SM' ControlSize="M" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Included" />
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
					<px:PXDropDown ID="edGroupType" runat="server"  DataField="GroupType"
						Enabled="False" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" />
		<ActionBar>
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
