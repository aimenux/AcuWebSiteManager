<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GL203000.aspx.cs" Inherits="Page_GL203000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="SubRecords" TypeName="PX.Objects.GL.SubAccountMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Cancel"  />
			<px:PXDSCallbackCommand Name="ViewRestrictionGroups" DependOnGrid="grid1" />
		</CallbackCommands>
	</px:PXDataSource>

</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid1" runat="server" Height="400px" Width="100%" SyncPosition="True"
		AdjustPageSize="Auto" AllowSearch="True" SkinID="Primary" AllowPaging="True"
		NoteIndicator="True" FastFilterFields="SubCD,Description">
		<Levels>
			<px:PXGridLevel DataMember="SubRecords">
				<Columns>
					<px:PXGridColumn DataField="SubID" TextAlign="Right" />
					<px:PXGridColumn DataField="SubCD" />
					<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
					<px:PXGridColumn DataField="Description" />
					<px:PXGridColumn DataField="Secured" TextAlign="Center" Type="CheckBox" />
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M"  />
					<px:PXSegmentMask ID="edSubCD" runat="server" DataField="SubCD" SelectMode="Segment" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
					<px:PXCheckBox ID="chkSecured" runat="server" DataField="Secured" Enabled="False" />
					<px:PXNumberEdit ID="edSubID" runat="server" DataField="SubID" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowFormEdit="True" AllowUpload="True" />
	</px:PXGrid>
</asp:Content>
