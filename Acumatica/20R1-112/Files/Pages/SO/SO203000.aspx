<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO203000.aspx.cs"
    Inherits="Page_SO203000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="true" Width="100%" TypeName="PX.Objects.SO.SOFlatPriceMaint" PrimaryView="Filter"
        PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="First" StartNewGroup="True" />
            <px:PXDSCallbackCommand StartNewGroup="true" Name="UpdateDiscounts" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="ViewSequence" DependOnGrid="grid" Visible="false" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection"
        DefaultControlID="edCustomerID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
            <px:PXSelector CommitChanges="True" ID="edDiscountID" runat="server" DataField="DiscountID" />
            <px:PXSelector CommitChanges="True" ID="edCuryID" runat="server" DataField="CuryID" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edExpireDate" runat="server" DataField="ExpireDate" />
            <px:PXCheckBox CommitChanges="True" ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
        </Template>
    </px:PXFormView>
    <px:PXSmartPanel ID="PanelUpdate" runat="server" Key="UpdateSettings" Caption="Update Discounts" CaptionVisible="True" LoadOnDemand="true" DesignView="Content"
        HideAfterAction="false" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
        <px:PXFormView ID="formUpdateSettings" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="UpdateSettings"
            CaptionVisible="False">
            <ContentStyle BackColor="Transparent" BorderStyle="None"/>
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edFilterDate" runat="server" DataField="FilterDate" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK"/>
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Style="z-index: 100" Width="100%" Caption="Flat-Price Details"
        AllowSearch="True" AdjustPageSize="Auto" SkinID="Details" AllowPaging="True">
        <Mode InitNewRow="true" />
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" />
                    <px:PXNumberEdit ID="edBreakQty" runat="server" DataField="BreakQty" />
                    <px:PXNumberEdit ID="edUnitPrice" runat="server" DataField="UnitPrice" />
                    <px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate" />
                    <px:PXDateTimeEdit ID="edExpireDate" runat="server" DataField="ExpireDate" />
                    <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
                    <px:PXCheckBox ID="chkIsUpdatable" runat="server" DataField="IsUpdatable" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCCCCCCCCCCCCCCCCCCCCCCCCCCC" Width="198px" AutoCallBack="true" />
                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryID_description" Width="198px" />
                    <px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="63px" />
                    <px:PXGridColumn AllowNull="False" DataField="BreakQty" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="UnitPrice" TextAlign="Right" Width="81px" />
                    <px:PXGridColumn DataField="EffectiveDate" Width="90px" />
                    <px:PXGridColumn DataField="ExpireDate" Width="90px" />
                    <px:PXGridColumn AllowNull="False" DataField="IsPending" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn AllowNull="False" DataField="IsUpdatable" TextAlign="Center" Type="CheckBox" Width="100px" />
                    <px:PXGridColumn DataField="DiscountSequenceID" DisplayFormat="&gt;CCCCCCCCCC" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <Mode AllowUpload="True" />
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar>
            <Actions>
                <NoteShow ToolBarVisible="False" MenuVisible="false" />
            </Actions>
            <CustomItems>
                <px:PXToolBarButton Text="View Discount Sequence" Key="cmdSequence">
                    <AutoCallBack Target="ds" Command="ViewSequence" />
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
    </px:PXGrid>
</asp:Content>
