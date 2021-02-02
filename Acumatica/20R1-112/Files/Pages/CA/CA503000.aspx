<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA503000.aspx.cs" Inherits="Page_CA503000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="PeriodsFilter" TypeName="PX.Objects.CA.CABalValidate" />
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="PeriodsFilter" Width="100%" >
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" CommitChanges="true"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" ActionsPosition="Top" AllowSearch="true" DataSourceID="ds" SkinID="PrimaryInquire" Caption="Cash Accounts"
        FastFilterFields="CashAccountCD, Descr">
        <Levels>
            <px:PXGridLevel DataMember="CABalValidateList">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" AllowCheckAll="True" AllowSort="False" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CashAccountCD" DisplayFormat="&gt;AAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Descr" />
                </Columns>
            </px:PXGridLevel>
        </Levels> 
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
