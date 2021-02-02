<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR507000.aspx.cs" Inherits="Page_AR507000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ARFinChargesApplyMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDocument" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" BorderStyle="None" Caption="Selection" DataMember="Filter" DefaultControlID="edFinChargeDate" MarkRequired="Dynamic">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edFinChargeDate" runat="server" DataField="FinChargeDate" />
            <px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
            <px:PXSelector CommitChanges="True" ID="edStatementCycle" runat="server" DataField="StatementCycle" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector ID="edCustomerClassID" runat="server" DataField="CustomerClassID" CommitChanges="True"/>
            <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" CommitChanges="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%" 
        FastFilterFields="RefNbr, CustomerID, CustomerID_BAccountR_acctName" SkinID="PrimaryInquire" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="ARFinChargeRecords">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" TextCase="Upper" />
                    <px:PXGridColumn DataField="DocType" Type="DropDownList" />
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument" />
                    <px:PXGridColumn DataField="DocDate" />
                    <px:PXGridColumn DataField="DueDate" />
                    <px:PXGridColumn DataField="CustomerID" DisplayFormat="CCCCCCCCCC" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CustomerID_BAccountR_acctName" />
                    <px:PXGridColumn DataField="CuryID" DisplayFormat="&gt;LLLLL" />
                    <px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" />
                    <px:PXGridColumn DataField="LastPaymentDate" />
                    <px:PXGridColumn DataField="LastChargeDate" />
                    <px:PXGridColumn AllowNull="False" DataField="OverdueDays" TextAlign="Right" />
                    <px:PXGridColumn DataField="FinChargeCuryID" DisplayFormat="&gt;LLLLL" />
                    <px:PXGridColumn DataField="FinChargeAmt" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ARAccountID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="ARSubID" DisplayFormat="&gt;AAAA-AA-AA-AAAA" />
                    <px:PXGridColumn DataField="FinChargeID" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
