<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP509900.aspx.cs" Inherits="Page_AP509900" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APIntegrityCheck" PrimaryView="Filter" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edVendorClassID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID" CommitChanges="true"/>
            <px:PXSelector CommitChanges="True" ID="edVendorClassID" runat="server" DataField="VendorClassID" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" Caption="Vendors" AllowPaging="true" AdjustPageSize="Auto" SkinID="PrimaryInquire"
        AllowSearch="True" FastFilterFields="AcctCD,AcctName" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="APVendorList">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn DataField="AcctCD" LinkCommand="ViewVendor" />
                    <px:PXGridColumn DataField="VendorClassID" />
                    <px:PXGridColumn DataField="AcctName" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
