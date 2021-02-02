<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="GL509900.aspx.cs" Inherits="Page_GL509900" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="Filter" TypeName="PX.Objects.GL.GLHistoryValidate" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Caption="Selection">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" CommitChanges="true"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%"
        AllowPaging="True" AllowSearch="true" SkinID="PrimaryInquire"
        Caption="Ledgers" FastFilterFields="LedgerCD,Descr" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="LedgerList">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" AllowCheckAll="true" Type="CheckBox" />
                    <px:PXGridColumn DataField="LedgerCD" />
                    <px:PXGridColumn DataField="Descr" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
