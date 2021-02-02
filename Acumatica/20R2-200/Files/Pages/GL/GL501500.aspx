<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="GL501500.aspx.cs" Inherits="Page_GL501500"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Documents" TypeName="PX.Objects.GL.VoucherRelease"
        Visible="True">
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True"
        SkinID="PrimaryInquire" Caption="Batches" DataSourceID="ds" FastFilterFields="BatchNbr,Description" NoteIndicator="True" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Documents">
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" />
                    <px:PXGridColumn DataField="Module" />
                    <px:PXGridColumn DataField="BatchNbr" LinkCommand="EditDetail" />
                    <px:PXGridColumn DataField="LedgerID" />
                    <px:PXGridColumn DataField="DateEntered" />
                    <px:PXGridColumn DataField="LastModifiedByID_description" />
                    <px:PXGridColumn DataField="FinPeriodID" />
                    <px:PXGridColumn DataField="ControlTotal" TextAlign="Right" />
                    <px:PXGridColumn DataField="Description" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
        <Mode AllowUpdate="False" />
    </px:PXGrid>
</asp:Content>
