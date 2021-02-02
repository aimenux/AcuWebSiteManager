<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PO506000.aspx.cs"
    Inherits="Page_PO506000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="landedCostDocsList" TypeName="PX.Objects.PO.POLandedCostProcess"/>
       
    
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top"
        AutoAdjustColumns="True" AllowSearch="true" DataSourceID="ds" Caption="Documents" FastFilterFields="DocType,RefNbr,VendorID,CuryID"
        BatchUpdate="True" SkinID="PrimaryInquire" SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="landedCostDocsList">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" Enabled="False" />
                    <px:PXSelector ID="edRefNbr" runat="server" AllowEdit="True" DataField="RefNbr" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" AllowNull="False" AllowShowHide="False" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="RefNbr" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocType" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorID" DisplayFormat="&gt;AAAAAAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorID_Vendor_acctName" />
                    <px:PXGridColumn AllowUpdate="False" DataField="VendorLocationID" DisplayFormat="&gt;AAAAAA" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DocDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryID" DisplayFormat="&gt;LLLLL" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryLineTotal" TextAlign="Right" />
                    <px:PXGridColumn AllowUpdate="False" DataField="CuryTaxTotal" TextAlign="Right" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="viewDocument"/>
           
        
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
