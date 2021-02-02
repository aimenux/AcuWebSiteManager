<px:PXGrid ID="MatrixAttributes" runat="server" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" Height="34px" Width="100%"
	AutoGenerateColumns="Recreate" RepaintColumns="True" GenerateColumnsBeforeRepaint="True" OnAfterSyncState="MatrixAttributes_AfterSyncState">
	<Levels>
		<px:PXGridLevel DataMember="AdditionalAttributes" />
	</Levels>
	<ActionBar ActionsText="True" ActionsVisible="False" />
	<Mode AllowAddNew="False" AllowDelete="False" />
</px:PXGrid>
<px:PXGrid ID="MatrixMatrix" runat="server" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" Width="100%" AllowFilter="true"
	AutoGenerateColumns="Recreate" RepaintColumns="True" GenerateColumnsBeforeRepaint="True"
	OnColumnsGenerated="MatrixMatrix_ColumnsGenerated" OnRowDataBound="MatrixMatrix_RowDataBound" OnAfterSyncState="MatrixMatrix_AfterSyncState">
	<Levels>
		<px:PXGridLevel DataMember="Matrix" />
	</Levels>
	<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	<Mode AllowAddNew="False" AllowDelete="False" />
</px:PXGrid>
<px:PXSmartPanel ID="popupCreate" runat="server" CaptionVisible="True" Caption="Create Matrix Items"
	Height="600px" Width="1000px" DesignView="Content" Key="MatrixItemsForCreation" AutoReload="True" LoadOnDemand="True" AutoRepaint="True" CreateOnDemand="True">
	<px:PXGrid ID="grdCreate" runat="server" DataSourceID="ds" Style="z-index: 100;" Width="100%" Height="92%" SkinID="Inquire" SyncPosition="True"
		AutoGenerateColumns="Append" RepaintColumns="True" GenerateColumnsBeforeRepaint="True">
		<Levels>
			<px:PXGridLevel DataKeyNames="LineNbr" DataMember="MatrixItemsForCreation">
				<Columns>
					<px:PXGridColumn DataField="Selected" Type="CheckBox" TextAlign="Center" />
					<px:PXGridColumn DataField="InventoryCD" />
					<px:PXGridColumn DataField="Descr" />
					<px:PXGridColumn DataField="StkItem" Type="CheckBox" TextAlign="Center" />
					<px:PXGridColumn DataField="ItemClassID" />
					<px:PXGridColumn DataField="ItemType" />
					<px:PXGridColumn DataField="ValMethod" />
					<px:PXGridColumn DataField="LotSerClassID" />
					<px:PXGridColumn DataField="DfltSiteID" />
					<px:PXGridColumn DataField="TaxCategoryID" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<Mode AllowAddNew="False" AllowDelete="False" />
	</px:PXGrid>
	<px:PXPanel ID="pnlCreateButtons" runat="server" SkinID="Buttons">
		<px:PXButton ID="btnCreateUpdate" runat="server" Text="Confirmation" DialogResult="OK" />
	</px:PXPanel>
</px:PXSmartPanel>