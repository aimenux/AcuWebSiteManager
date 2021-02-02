<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR506500.aspx.cs" Inherits="Page_AR506500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PageLoadBehavior="GoLastRecord" PrimaryView="Filter" TypeName="PX.Objects.AR.ARSPCommissionReview" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Caption="Selection" Width="100%" DataMember="Filter" DefaultControlID="edCommnPeriodID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXSelector ID="edCommnPeriodID" runat="server" DataField="CommnPeriodID" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" SelectedIndex="1" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDateUI" Enabled="False" />
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDateUI" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Caption="Salespersons' Commissions" Width="100%" SkinID="PrimaryInquire"
        FastFilterFields="SalesPersonID, SalesPersonID_SalesPerson_descr" SyncPosition="true" >
        <Levels>
            <px:PXGridLevel DataMember="ToProcess">
                <Columns>
                    <px:PXGridColumn DataField="SalesPersonID" LinkCommand="EditDetail" />
                    <px:PXGridColumn DataField="Type" />
                    <px:PXGridColumn DataField="SalesPersonID_SalesPerson_descr" />
                    <px:PXGridColumn DataField="CommnblAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CommnAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="PRProcessedDate" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
