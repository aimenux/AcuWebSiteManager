<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="PM102010.aspx.cs" Inherits="Page_PM102010"
	Title="Project Access Maintenance" %>
	

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.PMAccessDetail"
		PrimaryView="Project">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" Caption="Project" DataMember="Project" DefaultControlID="edContractCD" TemplateContainer="">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSegmentMask ID="edContractCD" runat="server" DataField="ContractCD" FilterByAllFields="True">
			    <AutoCallBack Command="Cancel" Target="ds"/>
			</px:PXSegmentMask>
		    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False"/>
			<px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="XL" />
		    <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False"/>
		    <px:PXSegmentMask ID="edTemplateID" runat="server" DataField="TemplateID" Enabled="False"/>
			<px:PXLayoutRule runat="server" StartRow="True"  LabelsWidth="S" />
		    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" AdjustPageSize="Auto" Caption="Restriction Groups" AllowSearch="True" SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="Groups">
				<Mode AllowAddNew="False" AllowDelete="False" />
				<Columns>
				    <px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" RenderEditorText="True" AllowCheckAll="True"/>
				    <px:PXGridColumn DataField="GroupName"/>
				    <px:PXGridColumn DataField="Description"/>
				    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox"/>
				    <px:PXGridColumn DataField="GroupType" Label="Visible To Entities" RenderEditorText="True"/>
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
				    <px:PXCheckBox SuppressLabel="True" ID="chkSelected" runat="server" DataField="Included" />
				    <px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" />
				    <px:PXTextEdit  ID="edDescription" runat="server" DataField="Description" />
				    <px:PXCheckBox SuppressLabel="True" ID="chkActive" runat="server" Checked="True" DataField="Active" />
				    <px:PXDropDown  ID="edGroupType" runat="server" DataField="GroupType" Enabled="False" />
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
