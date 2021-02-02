<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM106001.aspx.cs" Inherits="Page_SM106001"
	Title="Account Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.GL.GLAccessByPrinter"
		PrimaryView="Printers">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SavePrinter" />
			<px:PXDSCallbackCommand Name="FirstPrinter" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="FirstPrinter" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="PrevPrinter" HideText="True"/>
			<px:PXDSCallbackCommand Name="NextPrinter" HideText="True"/>
			<px:PXDSCallbackCommand Name="LastPrinter" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" Caption="Printer" DataMember="Printers"
		DefaultControlID="edPrinterID" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth="S" ControlSize="M" />
			<px:PXSelector ID="edPrinterID" runat="server" DataField="PrinterID" DisplayMode="Text" CommitChanges="true" >
                <AutoCallBack Command="CancelPrinter" Target="ds" />
			</px:PXSelector>
			<px:PXTextEdit runat="server" DataField="DeviceHubID" Enabled="false" ID="edDeviceHubID" />
            <px:PXTextEdit runat="server" DataField="Description" Enabled="false" ID="edPrinterDescription" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Width="100%" AllowPaging="True" Height="300px"
		AdjustPageSize="Auto" Caption="Restriction Groups" AllowSearch="True" SkinID="Details"
		TabIndex="200">
		<Levels>
			<px:PXGridLevel DataMember="Groups">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
					<px:PXGridColumn DataField="Included" DefValueText="False" TextAlign="Center" Type="CheckBox"
						RenderEditorText="True" AllowCheckAll="True" />
					<px:PXGridColumn DataField="GroupName" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="Active" DefValueText="True" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="GroupType" RenderEditorText="True" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth="SM" ControlSize="M" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Included" />
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active" />
					<px:PXDropDown ID="edGroupType" runat="server" DataField="GroupType" Enabled="False" />
				</RowTemplate>
				<Layout FormViewHeight=""></Layout>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" AllowFormEdit="True" />
		<ActionBar>
			<Actions>
				<AddNew Enabled="False" />
				<Delete Enabled="False" />
			</Actions>
		</ActionBar>
	</px:PXGrid>
</asp:Content>
