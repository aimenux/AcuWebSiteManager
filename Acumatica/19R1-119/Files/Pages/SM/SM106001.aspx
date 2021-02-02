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
		DefaultControlID="edPrinterName" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth="S" ControlSize="M" />
			<px:PXSelector ID="edPrinterName" runat="server" DataField="PrinterName">
                <AutoCallBack Command="CancelPrinter" Target="ds">
				</AutoCallBack>
			</px:PXSelector>
            <px:PXTextEdit runat="server" DataField="Description" Enabled="false" ID="edDescription">
            </px:PXTextEdit>
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
						Width="40px" RenderEditorText="True" AllowCheckAll="True">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="GroupName" Width="150px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Description" Width="200px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Active" DefValueText="True" TextAlign="Center" Type="CheckBox"
						Width="90px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="GroupType" RenderEditorText="True" Width="171px">
					</px:PXGridColumn>
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat='server' StartColumn='True' LabelsWidth="SM" ControlSize="M" />
					<px:PXCheckBox ID="chkSelected" runat="server" DataField="Included">
					</px:PXCheckBox>
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName">
					</px:PXSelector>
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description">
					</px:PXTextEdit>
					<px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active">
					</px:PXCheckBox>
					<px:PXDropDown ID="edGroupType" runat="server" DataField="GroupType" Enabled="False">
					</px:PXDropDown>
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
