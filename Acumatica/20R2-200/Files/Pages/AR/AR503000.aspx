<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR503000.aspx.cs" Inherits="Page_AR503000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter" TypeName="PX.Objects.AR.ARStatementProcess" PageLoadBehavior="PopulateSavedValues" />
</asp:Content>
<asp:Content ID="cont15" ContentPlaceHolderID="phF" runat="server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edStatementDate">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXDateTimeEdit CommitChanges="True" ID="edStatementDate" runat="server" DataField="StatementDate" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" DataSourceID="ds" SkinID="PrimaryInquire" FastFilterFields="StatementCycleId, Descr">
        <Levels>
            <px:PXGridLevel DataMember="CyclesList">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowMove="False" AllowNull="False" AllowSort="False" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="StatementCycleId" />
                    <px:PXGridColumn DataField="Descr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="LastStmtDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="LastFinChrgDate" />
                    <px:PXGridColumn AllowNull="False" DataField="PrepareOn" />
                    <px:PXGridColumn AllowUpdate="False" DataField="NextStmtDate" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
