<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS203500.aspx.cs" Inherits="Page_FS203500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.FS.RoomMaint" 
        PrimaryView="RoomRecords">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds"
        Style="z-index: 100" Width="100%" TabIndex="800" DataMember="RoomRecords">
		<Template>
            <px:PXLayoutRule runat="server" StartColumn="True" StartRow="True" 
                ControlSize="SM" LabelsWidth="SM">
            </px:PXLayoutRule>
            <px:PXSelector ID="edBranchLocationID" runat="server" DataField="BranchLocationID" 
                AutoRefresh="True" AllowEdit="True" >
            </px:PXSelector>
            <px:PXSelector ID="edRoomID" runat="server" DataField="RoomID" AutoRefresh="True">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
            </px:PXTextEdit>
            <px:PXNumberEdit ID="edFloorNbr" runat="server" DataField="FloorNbr" Size="SM">
            </px:PXNumberEdit>
        </Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
</asp:Content>
