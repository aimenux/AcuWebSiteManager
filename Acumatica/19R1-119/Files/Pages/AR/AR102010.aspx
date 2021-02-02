<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AR102010.aspx.cs" Inherits="Page_AR102010"
	Title="Customer Access Maintenance" %>
	

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AR.ARAccessDetail"
		PrimaryView="Customer">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="SaveCustomer" />
			<px:PXDSCallbackCommand Name="FirstCustomer" StartNewGroup="True" HideText="True"/>
			<px:PXDSCallbackCommand Name="PrevCustomer" HideText="True"/>
			<px:PXDSCallbackCommand Name="NextCustomer" HideText="True"/>
			<px:PXDSCallbackCommand Name="LastCustomer" HideText="True"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" Caption="Customer" DataMember="Customer"
		DefaultControlID="edAcctCD" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" FilterByAllFields="True">
				<AutoCallBack Command="CancelCustomer" Target="ds">
				</AutoCallBack>
			</px:PXSegmentMask>
			<px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False">
			</px:PXDropDown>
			<px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName" Enabled="False">
			</px:PXTextEdit></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" AdjustPageSize="Auto"
		Caption="Restriction Groups" AllowSearch="True" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="Groups">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
					<px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="30px"
						RenderEditorText="True" AllowCheckAll="True">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="GroupName" Width="150px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Description" Width="200px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" Width="40px">
					</px:PXGridColumn>
					<px:PXGridColumn DataField="GroupType" Label="Visible To Entities" RenderEditorText="True"
						Width="100px">
					</px:PXGridColumn>
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included">
					</px:PXCheckBox>
					<px:PXSelector ID="edGroupName" runat="server" DataField="GroupName">
					</px:PXSelector>
					<px:PXTextEdit  ID="edDescription" runat="server" DataField="Description">
					</px:PXTextEdit>
					<px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" Checked="True"
						DataField="Active">
					</px:PXCheckBox>
					<px:PXDropDown  ID="edGroupType" runat="server" DataField="GroupType" Enabled="False">
					</px:PXDropDown>
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" />
		<Mode AllowAddNew="False" AllowDelete="False" />
	</px:PXGrid>
</asp:Content>
