<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="AP205000.aspx.cs" Inherits="Page_AP205000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Sequence" TypeName="PX.Objects.AP.APDiscountSequenceMaint">
		<CallbackCommands>
            <px:PXDSCallbackCommand StartNewGroup="true" Name="UpdateDiscounts" PostData="Self" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">    
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Discount Sequence Summary"
        DataMember="Sequence" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity"
        DefaultControlID="edDiscountID" TabIndex="100" AllowCollapse="false">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edVendorID" runat="server" DataField="VendorID" CommitChanges="True"></px:PXSelector>
            <px:PXSelector ID="edDiscountID" runat="server" DataField="DiscountID" AutoRefresh="True" />
            <px:PXSelector ID="edDiscountSequenceID" runat="server" DataField="DiscountSequenceID" AutoRefresh="True" />
            <px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
            <px:PXCheckBox CommitChanges="True" ID="chkIsPromotion" runat="server" DataField="IsPromotion" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXDropDown CommitChanges="True" ID="edDiscountedFor" runat="server" AllowNull="False" DataField="DiscountedFor" />
            <px:PXDropDown CommitChanges="True" ID="edBreakBy" runat="server" AllowNull="False" DataField="BreakBy" SelectedIndex="1" />
            <px:PXLabel ID="PXLabel1" runat="server" />
            <px:PXLabel ID="PXLabel4" runat="server" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXCheckBox ID="chkProrate" runat="server" DataField="Prorate" AlignLeft="true" />
            <px:PXLabel ID="PXLabel2" runat="server" />
            <px:PXLabel ID="PXLabel3" runat="server" />
            <px:PXLabel ID="PXLabel5" runat="server" />
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXLayoutRule runat="server" StartColumn="True" SuppressLabel="True" />
            <px:PXFormView ID="DiscountForm" runat="server" DataMember="Discount" DataSourceID="ds" Caption="Hidden Form needed for VisibleExp of TabItems. Tabs are Hidden based on the values of Combo"
                Visible="False" TabIndex="300">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkShowListOfItems" runat="server" DataField="showListOfItems" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkShowLocations" runat="server" DataField="ShowLocations" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkShowInventoryPriceClass" runat="server" DataField="ShowInventoryPriceClass" />
                </Template>
            </px:PXFormView>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSmartPanel ID="PanelUpdate" runat="server" Key="UpdateSettings" Caption="Update Discounts" CaptionVisible="True" LoadOnDemand="true"
        DesignView="Content" HideAfterAction="false" AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel">
        <px:PXFormView ID="formUpdateSettings" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="UpdateSettings"
            CaptionVisible="False">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXDateTimeEdit CommitChanges="True" ID="edFilterDate" runat="server" DataField="FilterDate" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXTab ID="tab" runat="server" Height="540px" Style="z-index: 100" Width="100%">
        <Items>
            <px:PXTabItem Text="Discount Breakpoints">
                <Template>
                    <px:PXGrid runat="server" ID="grid" DataSourceID="ds" Height="144px" BorderStyle="None" SkinID="Details" Width="100%" AdjustPageSize="Auto" TabIndex="19500">
                        <Levels>
                            <px:PXGridLevel DataMember="Details" DataKeyNames="DiscountDetailsID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive"/>
                                    <px:PXNumberEdit ID="edQuantity" runat="server" DataField="Quantity" />
                                    <px:PXNumberEdit ID="edDiscountPercent" runat="server" DataField="DiscountPercent" />
                                    <px:PXNumberEdit ID="edLastDiscountPercent" runat="server" DataField="LastDiscountPercent" />
                                    <px:PXNumberEdit ID="edAmount" runat="server" DataField="Amount" />
                                    <px:PXNumberEdit ID="edPendingDiscountPercent" runat="server" DataField="PendingDiscountPercent" />
                                    <px:PXNumberEdit ID="edDiscount" runat="server" DataField="Discount" />
                                    <px:PXDateTimeEdit ID="edLastDate" runat="server" DataField="LastDate" />
                                    <px:PXNumberEdit ID="edFreeItemQty" runat="server" DataField="FreeItemQty" />
                                    <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" AllowNull="False" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edPendingQuantity" runat="server" DataField="PendingQuantity" />
                                    <px:PXNumberEdit ID="edPendingAmount" runat="server" DataField="PendingAmount" />
                                    <px:PXNumberEdit ID="edPendingDiscount" runat="server" DataField="PendingDiscount" />
                                    <px:PXNumberEdit ID="edPendingFreeItemQty" runat="server" DataField="PendingFreeItemQty" />
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edLastQuantity" runat="server" DataField="LastQuantity" />
                                    <px:PXNumberEdit ID="edLastAmount" runat="server" DataField="LastAmount" />
                                    <px:PXNumberEdit ID="edLastDiscount" runat="server" DataField="LastDiscount" />
                                    <px:PXNumberEdit ID="edLastFreeItemQty" runat="server" DataField="LastFreeItemQty" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Quantity" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Amount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Discount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="DiscountPercent" TextAlign="Right" />
                                    <px:PXGridColumn DataField="FreeItemQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PendingQuantity" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PendingAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PendingDiscount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PendingDiscountPercent" TextAlign="Right" />
                                    <px:PXGridColumn DataField="PendingFreeItemQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="StartDate" />
                                    <px:PXGridColumn DataField="LastQuantity" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LastAmount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LastDiscount" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LastDiscountPercent" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LastFreeItemQty" TextAlign="Right" />
                                    <px:PXGridColumn DataField="LastDate" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode InitNewRow="True" />
                        <LevelStyles>
                            <RowForm Height="160px" Width="900px" />
                        </LevelStyles>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Items" BindingContext="DiscountForm" VisibleExp="DataControls[&quot;chkShowListOfItems&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="itemsGrid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" AdjustPageSize="Auto">
                        <AutoSize Enabled="True" />
                        <Mode AllowUpload="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Items" DataKeyNames="DiscountID,InventoryID,DiscountSequenceID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edInventoryItem__Descr" runat="server" DataField="InventoryItem__Descr" Enabled="False" />
                                    <px:PXSelector ID="edUOM2" runat="server" DataField="UOM" />
                                    <px:PXNumberEdit ID="edAmount2" runat="server" DataField="Amount" />
                                    <px:PXNumberEdit ID="edQuantity2" runat="server" DataField="Quantity" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" CommitChanges="True" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="InventoryItem__Descr" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Locations" BindingContext="DiscountForm" VisibleExp="DataControls[&quot;chkShowLocations&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="locationsGrid" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" AdjustPageSize="Auto">
                        <AutoSize Enabled="True" />
                        <Mode AllowUpload="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Locations" DataKeyNames="DiscountID,DiscountSequenceID,LocationID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                     <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" AllowEdit="True">
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edLocation__Descr" runat="server" DataField="Location__Descr" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LocationID" CommitChanges="true" />
                                    <px:PXGridColumn DataField="Location__Descr" AllowUpdate="False"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
                        <px:PXTabItem Text="Item Price Classes" BindingContext="DiscountForm" VisibleExp="DataControls[&quot;chkShowInventoryPriceClass&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="inventoryPriceClassGrid" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="InventoryPriceClasses">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edInventoryPriceClassID" runat="server" DataField="InventoryPriceClassID" />
                                    <px:PXTextEdit ID="edINPriceClass__Description" runat="server" DataField="INPriceClass__Description" Enabled="False" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryPriceClassID" DisplayFormat="&gt;aaaaaaaaaa" AutoCallBack="true" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="INPriceClass__Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Enabled="True" Container="Window" />
    </px:PXTab>
</asp:Content>
