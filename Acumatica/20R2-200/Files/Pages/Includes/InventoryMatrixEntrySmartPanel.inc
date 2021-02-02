<px:PXSmartPanel ID="InventoryMatrixEntrySmartPanel" runat="server" Caption="Add Matrix Item: Table View" CaptionVisible="True" LoadOnDemand="True" Key="Header"
	AutoCallBack-Target="MatrixEntryFormView" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" Width="100%">

	<px:PXLayoutRule runat="server" StartColumn="True" />

    <px:PXFormView ID="MatrixEntryFormView" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Header"
        CaptionVisible="False" SkinID="Transparent">
        <Template>
            <px:PXLayoutRule ID="InventoryMatrixEntrySmartPanelFormLayout" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSelector ID="edEntryTemplateItemID" runat="server" DataField="TemplateItemID" CommitChanges="true" />
			<px:PXTextEdit ID="edEntryDescription" runat="server" DataField="Description" />
			<px:PXSelector ID="selEntrySiteID" runat="server" DataField="SiteID" CommitChanges="true" />
        </Template>
    </px:PXFormView>
    
	<px:PXLayoutRule runat="server" StartColumn="True" />

    <px:PXGrid ID="MatrixItems" runat="server" DataSourceID="ds" SkinID="DetailsInTab" SyncPosition="True" Height="400px"
		RepaintColumns="True" OnAfterSyncState="MatrixItems_AfterSyncState">
        <Levels>
			<px:PXGridLevel DataMember="MatrixItems">
				<RowTemplate>
					<px:PXSegmentMask ID="matrixItemsInventoryCD" runat="server" DataField="InventoryCD" AllowEdit="True" />
					<px:PXSegmentMask ID="matrixItemsDfltSiteID" runat="server" DataField="DfltSiteID" AllowEdit="True" />
					<px:PXSegmentMask ID="matrixItemsItemClassID" runat="server" DataField="ItemClassID" AllowEdit="True" />
					<px:PXSelector ID="matrixItemsTaxCategoryID" runat="server" DataField="TaxCategoryID" AllowEdit="True" />
                </RowTemplate>
                <Columns>
					<px:PXGridColumn DataField="Qty" TextAlign="Right" />
					<px:PXGridColumn DataField="InventoryCD" DisplayFormat="&gt;CCCCC-CCCCCCCCCCCCCCC" />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn DataField="New" Type="CheckBox" AllowShowHide="False" />
					<px:PXGridColumn DataField="AttributeValue0" CommitChanges="true" />
					<px:PXGridColumn DataField="StkItem" Type="CheckBox" />
					<px:PXGridColumn DataField="BasePrice" AllowShowHide="False" />
					<px:PXGridColumn DataField="ItemClassID" />
					<px:PXGridColumn DataField="TaxCategoryID" />
                </Columns>
			</px:PXGridLevel>
        </Levels>
		<Mode InitNewRow="True" />
    </px:PXGrid>

    <px:PXPanel runat="server" ID="InventoryMatrixEntrySmartButtons" SkinID="Buttons">
		<px:PXButton ID="InventoryMatrixEntrySmartPanelButtonToLookup" runat="server" DialogResult="Yes" Text="Open Matrix View" Height="20" />
		<px:PXButton ID="InventoryMatrixEntrySmartPanelButtonOK" runat="server" CommitChanges="True" DialogResult="OK" Text="Add and close" Height="20">
			<AutoCallBack Command="Commit" Target="ds">
				<Behavior CommitChanges="True" />
			</AutoCallBack>
		</px:PXButton>
		<px:PXButton ID="InventoryMatrixEntrySmartPanelButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" Height="20" />
	</px:PXPanel>
</px:PXSmartPanel>

<px:PXSmartPanel ID="InventoryMatrixLookupSmartPanel" runat="server" Caption="Add Matrix Item: Matrix View" CaptionVisible="True" LoadOnDemand="True" Key="Matrix"
	AutoCallBack-Target="MatrixFormView" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" Width="100%">

	<px:PXLayoutRule runat="server" StartColumn="True" />

    <px:PXFormView ID="MatrixFormView" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Header"
        CaptionVisible="False" SkinID="Transparent">
        <Template>
            <px:PXLayoutRule ID="InventoryMatrixLookupSmartPanelFormLayout" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXSelector ID="edTemplateItemID" runat="server" DataField="TemplateItemID" CommitChanges="true" />
			<px:PXSelector ID="selColumnAttributeID" runat="server" DataField="ColAttributeID" CommitChanges="true" AutoRefresh="true" />
			<px:PXSelector ID="selRowAttributeID" runat="server" DataField="RowAttributeID" CommitChanges="true" AutoRefresh="true" />
            <px:PXCheckBox ID="edShowAvailable" runat="server" DataField="ShowAvailable" AlignLeft="true" CommitChanges="true" />
			<px:PXSelector ID="selSiteID" runat="server" DataField="SiteID" CommitChanges="true" />
        </Template>
    </px:PXFormView>
    
	<px:PXLayoutRule runat="server" StartColumn="True" />

    <px:PXGrid ID="MatrixAttributes" runat="server" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" Height="50px" Width="600px"
		AutoGenerateColumns="Recreate" RepaintColumns="True" GenerateColumnsBeforeRepaint="True" OnAfterSyncState="MatrixAttributes_AfterSyncState">
        <Levels>
            <px:PXGridLevel DataMember="AdditionalAttributes" />
        </Levels>
        <Mode AllowAddNew="False" AllowDelete="False" />
    </px:PXGrid>
	
	<px:PXLayoutRule runat="server" StartColumn="True" StartRow="true" />

    <px:PXGrid ID="MatrixMatrix" runat="server" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" Height="400px" StatusField="MatrixAvailability"
		AutoGenerateColumns="Recreate" RepaintColumns="True" GenerateColumnsBeforeRepaint="True" OnAfterSyncState="MatrixMatrix_AfterSyncState">
        <Levels>
            <px:PXGridLevel DataMember="Matrix" />
        </Levels>
        
        <Mode AllowAddNew="False" AllowDelete="False" />
		<ClientEvents AfterCellChange="matrixGrid_cellClick" />
    </px:PXGrid>

    <px:PXPanel runat="server" ID="InventoryMatrixLookupSmartButtons" SkinID="Buttons">
		<px:PXButton ID="InventoryMatrixLookupSmartPanelButtonOK" runat="server" CommitChanges="True" DialogResult="OK" Text="Add and close" Height="20">
			<AutoCallBack Command="Commit" Target="ds">
				<Behavior CommitChanges="True" />
			</AutoCallBack>
		</px:PXButton>
		<px:PXButton ID="InventoryMatrixLookupSmartPanelButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" Height="20" />
	</px:PXPanel>
</px:PXSmartPanel>

<script type="text/javascript">
	// Updates availability when user clicks matrix cell
	var matrixGridOldColumnName = null;
	function matrixGrid_cellClick(grid, ev) {
		var ds = px_alls["ds"];
		var columnName = ev.cell.column.dataField;
		if (ds != null && columnName != null && (matrixGridOldColumnName == null || matrixGridOldColumnName != columnName)) {
			var showavail = px_alls["edShowAvailable"];
			if (showavail != null && showavail.getValue() == true) {
				ds.executeCallback("MatrixGridCellChanged", columnName); matrixGridOldColumnName = columnName;
			}
		}
	}
</script>