<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="IN205000.aspx.cs" Inherits="Page_IN205000" 
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="SubItemRecords" TypeName="PX.Objects.IN.INSubItemMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Cancel" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid ID="grid" runat="server" Height="400px" Width="100%"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary"
        FastFilterFields="SubItemCD,Descr">
		<Levels>
			<px:PXGridLevel DataMember="SubItemRecords">
                <Columns>
                    <px:PXGridColumn DataField="SubItemID" Width="50px" />
                    <px:PXGridColumn DataField="SubItemCD" Width="100px" />
                    <px:PXGridColumn DataField="Descr" Width="300px" />
                </Columns>
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                    <px:PXNumberEdit ID="edSubItemID" runat="server" DataField="SubItemID" />
                    <px:PXSegmentMask ID="edSubItemCD" runat="server" DataField="SubItemCD"  SelectMode="Segment"/>
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
                </RowTemplate>
			</px:PXGridLevel> 
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowFormEdit="True" AllowUpload="True" />
	</px:PXGrid>
</asp:Content>
