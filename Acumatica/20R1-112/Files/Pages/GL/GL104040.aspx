<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL104040.aspx.cs" Inherits="Page_GL104040"
	Title="Account Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLAccessDetail"
		PrimaryView="Segment" PageLoadBehavior="GoFirstRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveSegment" />
			<px:PXDSCallbackCommand Name="FirstSegment" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="PrevSegment" HideText="True"/>
			<px:PXDSCallbackCommand Name="NextSegment" HideText="True"/>
			<px:PXDSCallbackCommand Name="LastSegment" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server"   Width="100%"
		Caption="Account" DataMember="Segment">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector ID="edSegmentID" runat="server" DataField="SegmentID" >
				<AutoCallBack Command="CancelSegment" Target="ds">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXSelector ID="edValue" runat="server" DataField="Value"  MaxLength="30" AutoRefresh="true">
				<AutoCallBack Command="CancelSegment" Target="ds">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" Enabled="False" />
			<px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server"  Height="150px" AllowPaging="True" Width="100%"
		AdjustPageSize="Auto" ActionsPosition="Top" Caption="Restriction Groups" AllowSearch="True"
		SkinID="Details">
		<Levels>
			<px:PXGridLevel DataKeyNames="GroupName" DataMember="Groups">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
					<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" />
					<px:PXGridColumn DataField="GroupName" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="GroupType" Type="DropDownList" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included" />
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox ID="chkActive" runat="server" Checked="True"	DataField="Active" />
					<px:PXCheckBox ID="edGroupType" runat="server" DataField="GroupType" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" />
		<ActionBar>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
