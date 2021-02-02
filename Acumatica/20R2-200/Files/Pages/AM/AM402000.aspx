<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" 
    CodeFile="AM402000.aspx.cs" Inherits="Page_AM402000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AM.BOMWhereUsedInq" PrimaryView="Filter" 
        BorderStyle="NotSet" >
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewBOM" Visible="False" DependOnGrid="grid" />    
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="Filter" DefaultControlID="edInventoryID">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True"  LabelsWidth="S" ControlSize="M" />
            <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" CommitChanges="True" />
            <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" CommitChanges="True" />
            <px:PXCheckBox ID="edMultiLevel" runat="server"  DataField="MultiLevel" CommitChanges="True" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edSiteID" runat="server" DataField="SiteID" DisplayMode="Hint" AllowEdit="True" CommitChanges="True" AutoRefresh="true" />
            <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID" DisplayMode="Hint" AllowEdit="True" CommitChanges="True" AutoRefresh="true" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" SyncPosition="True" TabIndex="300" >
		<Levels>
			<px:PXGridLevel DataMember="BOMWhereUsedRecs" DataKeyNames="BOMID, SiteID" >
                <RowTemplate>
                    <px:PXSegmentMask ID="edGridInventoryID" runat="server" DataField="InventoryID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edGridSubItemID" runat="server" DataField="SubItemID"/>
                    <px:PXNumberEdit ID="edLevel" runat="server" DataField="Level" />
                    <px:PXSegmentMask ID="edParentInventoryID" runat="server" DataField="ParentInventoryID" AllowEdit="True" />
                    <px:PXSegmentMask ID="edGridParentSubItemID" runat="server" DataField="ParentSubItemID"/>
                    <px:PXNumberEdit ID="edQtyRequired" runat="server" DataField="QtyRequired" />
                    <px:PXNumberEdit ID="edBatchSize" runat="server" DataField="BatchSize" />
                    <px:PXTextEdit ID="edUOM" runat="server" DataField="UOM" />
                    <px:PXSegmentMask ID="edItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" />
                    <px:PXCheckBox ID="edIsStockItem" runat="server" DataField="IsStockItem"/>
                    <px:PXDropDown ID="edSource" runat="server" DataField="Source"/>
                    <px:PXTextEdit ID="edBOMID" runat="server" DataField="BOMID" AllowEdit="True" />
                    <px:PXTextEdit ID="edRevisionID" runat="server" DataField="RevisionID" />
                    <px:PXSelector ID="edGridSiteID" runat="server" DataField="SiteID"/>
                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description"/>
                    <px:PXTextEdit ID="edParentDescription" runat="server" DataField="ParentDescription"/>
                    <px:PXTextEdit ID="edParentItemClassID" runat="server" DataField="ParentItemClassID"/>
                    <px:PXNumberEdit ID="edSequence" runat="server" DataField="Sequence" />
                    <px:PXNumberEdit ID="edUnitCost" runat="server" DataField="UnitCost" />
                    <px:PXNumberEdit ID="edMatlPlanCost" runat="server" DataField="PlanCost"/>
                    <px:PXDropDown ID="edMaterialType" runat="server" AllowNull="False" DataField="MaterialType" />
                    <px:PXDropDown ID="edPhtmRtngIorE" runat="server" AllowNull="False" DataField="PhantomRouting" />
                    <px:PXCheckBox ID="chkBFlush1" runat="server" DataField="BFlush" />
                    <px:PXSelector ID="edCompBOMID" runat="server" DataField="CompBOMID" />
                    <px:PXSelector ID="edCompBOMRevisionID" runat="server" DataField="CompBOMRevisionID" />
                    <px:PXSegmentMask ID="edLocationID" runat="server" DataField="LocationID" />
                    <px:PXNumberEdit ID="edScrapFactor" runat="server" DataField="ScrapFactor" />
                    <px:PXTextEdit ID="edBubbleNbr" runat="server" DataField="BubbleNbr" />
                    <px:PXDateTimeEdit ID="edEffDate" runat="server" DataField="EffDate" />
                    <px:PXDateTimeEdit ID="edExpDate" runat="server" DataField="ExpDate" />
                    <px:PXTextEdit ID="edOperationIDmatl" runat="server" DataField="OperationID" />
                    <px:PXNumberEdit ID="edLineIDmatl" runat="server" DataField="LineID" />
                    <px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder" />
                    <px:PXDropDown ID="edSubcontractSource" runat="server" DataField="SubcontractSource" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="InventoryID" Width="130px" />
                    <px:PXGridColumn DataField="SubItemID" Width="70px" />
                    <px:PXGridColumn DataField="Level" TextAlign="Center" Width="50px" />
                    <px:PXGridColumn DataField="ParentInventoryID" Width="130px" />
                    <px:PXGridColumn DataField="ParentSubItemID" Width="70px" />
                    <px:PXGridColumn DataField="QtyRequired" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="BatchSize" TextAlign="Right" Width="100px" />
                    <px:PXGridColumn DataField="UOM" Width="70px" />
                    <px:PXGridColumn DataField="ItemClassID"/>
                    <px:PXGridColumn DataField="IsStockItem" TextAlign="Center" Type="CheckBox" Width="70px" />
                    <px:PXGridColumn DataField="Source" Width="120px" />
                    <px:PXGridColumn DataField="BOMID" Width="90px" />
                    <px:PXGridColumn DataField="RevisionID" Width="80px" />
                    <px:PXGridColumn DataField="SiteID" Width="130px" />
                    <px:PXGridColumn DataField="Description" Width="150px" />
                    <px:PXGridColumn DataField="ParentDescription" Width="150px" />
                    <px:PXGridColumn DataField="ParentItemClassID"/>
                    <px:PXGridColumn DataField="Sequence" TextAlign="Right" />
                    <px:PXGridColumn DataField="LineID" TextAlign="Right" Width="54px" />
                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="54px" />
                    <px:PXGridColumn DataField="UnitCost" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="PlanCost" TextAlign="Right" Width="100px" /> 
                    <px:PXGridColumn DataField="MaterialType" TextAlign="Left" Width="100px" />
                    <px:PXGridColumn DataField="PhantomRouting" TextAlign="Left" Width="110px" />
                    <px:PXGridColumn DataField="BFlush" TextAlign="Center" Type="CheckBox" Width="85px" />
                    <px:PXGridColumn DataField="CompBOMID" Width="90px" />
                    <px:PXGridColumn DataField="CompBOMRevisionID" Width="85px" />
                    <px:PXGridColumn DataField="LocationID" TextAlign="Right" Width="130px" />
                    <px:PXGridColumn DataField="ScrapFactor" TextAlign="Right" Width="108px" />
                    <px:PXGridColumn DataField="BubbleNbr" Width="90px" />
                    <px:PXGridColumn DataField="EffDate" Width="85px" />
                    <px:PXGridColumn DataField="ExpDate" Width="85px" />
                    <px:PXGridColumn DataField="SubcontractSource" Width="95px" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar ActionsText="True">
		</ActionBar>
	</px:PXGrid>
</asp:Content>
