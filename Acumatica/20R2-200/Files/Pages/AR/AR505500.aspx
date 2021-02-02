<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR505500.aspx.cs" Inherits="Page_AR505500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" PrimaryView="Filter" TypeName="PX.Objects.AR.ARSPCommissionProcess" Visible="True" Width="100%" PageLoadBehavior="GoLastRecord" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Commission Period">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
            <px:PXMaskEdit ID="edCommnPeriodID" runat="server" DataField="CommnPeriodID" Enabled="False" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDateUI" Enabled="False" />
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDateUI" Enabled="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" DataSourceID="ds" Caption="Salespersons' Commissions" SkinID="PrimaryInquire">
        <Levels>
            <px:PXGridLevel DataMember="ToProcess">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" AllowMove="False" AllowSort="False" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="SalespersonID" />
                    <px:PXGridColumn DataField="SalespersonID_SalesPerson_descr" />
                    <px:PXGridColumn DataField="DocCount" TextAlign="Right" />
                    <px:PXGridColumn DataField="CommnblAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="CommnAmt" TextAlign="Right" />
                    <px:PXGridColumn DataField="AveCommnPct" TextAlign="Right" />
                    <px:PXGridColumn DataField="MinCommnPct" TextAlign="Right" />
                    <px:PXGridColumn DataField="CommnPct" TextAlign="Right" />
                </Columns>
                <RowTemplate>
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
