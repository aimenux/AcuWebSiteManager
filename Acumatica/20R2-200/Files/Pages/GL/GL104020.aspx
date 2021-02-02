<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL104020.aspx.cs" Inherits="Page_GL104020"
	Title="Account Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLAccessByAccount"
		PrimaryView="Account">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveAccount" />
			<px:PXDSCallbackCommand Name="FirstAccount" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="PrevAccount" HideText="True"/>
			<px:PXDSCallbackCommand Name="NextAccount" HideText="True"/>
			<px:PXDSCallbackCommand Name="LastAccount" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"  Width="100%" Caption="Account"
		DataMember="Account" DefaultControlID="edAccountCD" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth='SM' ControlSize="M" />
			<px:PXSegmentMask ID="edAccountCD" runat="server" DataField="AccountCD" 
				>
				<AutoCallBack Command="CancelAccount" Target="ds">
				</AutoCallBack>
			</px:PXSegmentMask>
			<px:PXDropDown ID="edType" runat="server" DataField="Type" Enabled="False" />
			<px:PXLayoutRule runat="server" ColumnSpan="2" />
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth='SM' ControlSize="M" />
			<px:PXSelector ID="edAccountClassID" runat="server" DataField="AccountClassID" 
				Enabled="False"  />
			<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" 
				 />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server"  Height="150px" TabIndex="200"
		Width="100%" AllowPaging="True" AdjustPageSize="Auto" Caption="Restriction Groups"
		AllowSearch="True" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="Groups">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
					<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
					<px:PXGridColumn DataField="GroupName" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="GroupType" RenderEditorText="True" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth='SM' ControlSize="M"/>
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included" />
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" />
					<px:PXDropDown ID="edGroupType" runat="server" DataField="GroupType" Enabled="False" />
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
