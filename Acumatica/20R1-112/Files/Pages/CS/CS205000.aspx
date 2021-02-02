<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CS205000.aspx.cs" Inherits="Page_CS205000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.CSAttributeMaint" PrimaryView="Attributes">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Attributes" Caption="Attribute Summary">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
			<px:PXSelector ID="edAttributeID" runat="server" DataField="AttributeID" AutoRefresh="True" DataSourceID="ds">
			    <GridProperties FastFilterFields="description" />
			</px:PXSelector>
			<px:PXTextEdit ID="edDescription" runat="server" AllowNull="False" DataField="Description" />
			<px:PXDropDown CommitChanges="True" ID="edControlType" runat="server" AllowNull="False" DataField="ControlType" />
            <px:PXCheckBox ID="chkIsInternal" runat="server" DataField="IsInternal" />
            <px:PXCheckBox ID="chkContainsPersonalData" runat="server" DataField="ContainsPersonalData" />
			<px:PXTextEdit ID="edEntryMask" runat="server" DataField="EntryMask" />
			<px:PXTextEdit ID="edRegExp" runat="server" DataField="RegExp" />
			<px:PXSelector ID="SchemaObject" runat="server" DataField="ObjectName" AutoRefresh="True" CommitChanges="true" />
			<px:PXDropDown ID="SchemaField" runat="server" DataField="FieldName" AutoRefresh="True"  CommitChanges="True"  />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" SkinID="Details" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top" Caption="Attribute Details">
		<Levels>
			<px:PXGridLevel DataMember="AttributeDetails">
				<Columns>
					<px:PXGridColumn DataField="ValueID"  Width="100px" CommitChanges="true" />
					<px:PXGridColumn DataField="Description" Width="250px" />
					<px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
                    <px:PXGridColumn DataField="Disabled" TextAlign="Center" Type="CheckBox" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
	    <AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<Mode AllowUpload="True" />
	</px:PXGrid>
</asp:Content>