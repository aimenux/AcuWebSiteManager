<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM300020.aspx.cs" Inherits="Page_AM300020" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="ProcessMatl" 
        TypeName="PX.Objects.AM.MatlWizard2" Visible="True">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Width="100%" AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" 
        SkinID="Inquire" SyncPosition="True" >
		<Levels>
			<px:PXGridLevel DataMember="ProcessMatl">
			    <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" />
                    <px:PXNumberEdit ID="edQtyReq" runat="server" DataField="QtyReq" />
                    <px:PXNumberEdit ID="edMatlQty" runat="server" DataField="MatlQty" />
                    <px:PXSelector ID="edUOM" runat="server" DataField="UOM" AutoRefresh="True" AllowEdit="True" >
                        <Parameters>
                            <px:PXControlParam ControlID="grid" Name="AMWrkMatl.inventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]"
                                Type="String" ></px:PXControlParam>
                        </Parameters>
                    </px:PXSelector>
                    <px:PXNumberEdit ID="edQtyAvail" runat="server" DataField="QtyAvail" />
                    <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" AllowEdit="True"/>
                    <px:PXSelector ID="edLocationID" runat="server" DataField="LocationID" AllowEdit="True"/>
                    <px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" AllowEdit="True" />
                    <px:PXSelector ID="edProdOrdID" runat="server" DataField="ProdOrdID" AllowEdit="True" />
                    <px:PXTextEdit ID="edOperationID" runat="server" AllowNull="False" DataField="OperationID" />
                    <px:PXCheckBox ID="edIsByproduct" runat="server" DataField="IsByproduct" />
                    <px:PXTextEdit ID="edInventoryID_description" runat="server" DataField="InventoryID_description" />
			        <px:PXNumberEdit ID="edUnreleasedBatchQty" runat="server" DataField="UnreleasedBatchQty" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" Width="70px" />
                    <px:PXGridColumn DataField="InventoryID" Width="130px" />
                    <px:PXGridColumn DataField="SubItemID" Width="81px"/>
                    <px:PXGridColumn DataField="QtyReq" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="MatlQty" TextAlign="Right" CommitChanges="True" Width="108px" />
                    <px:PXGridColumn DataField="UOM"  Width="75px" CommitChanges="True" />
                    <px:PXGridColumn DataField="QtyAvail" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="SiteID" Width="130px"/>
                    <px:PXGridColumn DataField="LocationID" Width="130px"/>
                    <px:PXGridColumn DataField="OrderType" />
                    <px:PXGridColumn DataField="ProdOrdID" Width="130px" />
                    <px:PXGridColumn DataField="OperationID" Width="70px" />
                    <px:PXGridColumn DataField="IsByproduct" TextAlign="Center" Type="CheckBox" Width="70px"/>
                    <px:PXGridColumn DataField="InventoryID_description" Width="200px"/>
                    <px:PXGridColumn DataField="UnreleasedBatchQty" CommitChanges="True" Width="108px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<ActionBar ActionsText="True">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
