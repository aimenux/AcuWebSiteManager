<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP502500.aspx.cs"
    Inherits="Page_AP502500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AP.APUpdateDiscounts" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Selection" DataMember="Filter"
        DefaultControlID="edPendingDiscountDate">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
            <px:PXSelector ID="edVendorID" runat="server" DataField="VendorID" CommitChanges="True" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edPendingDiscountDate" runat="server" DataField="PendingDiscountDate" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowPaging="True"
        Caption="Discount Sequences" SkinID="PrimaryInquire" FastFilterFields="DiscountID, DiscountSequenceID, Description">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="true" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DiscountID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="DiscountSequenceID" />
                    <px:PXGridColumn AllowUpdate="False" DataField="Description" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="DiscountedFor" RenderEditorText="true" />
                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="BreakBy" RenderEditorText="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="StartDate" />
                    <px:PXGridColumn AllowUpdate="False" DataField="EndDate" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
