<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP507000.aspx.cs" Inherits="Page_AP507000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.AP.AP1099SummaryEnq" PrimaryView="Year_Header" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Year_Header" Caption="Selection" DefaultControlID="edFinYear">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask ID="edOrganizationID" runat="server" DataField="OrganizationID" CommitChanges="True" />
            <px:PXSelector ID="edFinYear" runat="server" DataField="FinYear" AutoRefresh="True" CommitChanges="True" Size="S"/>
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="false" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Details" SkinID="PrimaryInquire" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="Year_Summary">
                <Columns>
                    <px:PXGridColumn DataField="BoxNbr" TextAlign="Right" />
                    <px:PXGridColumn DataField="Descr" />
                    <px:PXGridColumn DataField="AP1099History__HistAmt" TextAlign="Right" MatrixMode="True" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
