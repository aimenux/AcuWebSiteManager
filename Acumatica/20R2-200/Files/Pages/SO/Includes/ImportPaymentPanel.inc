<px:PXSmartPanel ID="ImportPaymentSmartPanel" runat="server" Caption="Import CC Payment" CaptionVisible="True" LoadOnDemand="True" Key="ImportExternalTran"
	AutoCallBack-Target="ImportPaymentFormView" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">

	<px:PXLayoutRule runat="server" StartColumn="True" />

	<px:PXFormView ID="ImportPaymentFormView" runat="server" DataSourceID="ds" Width="100%" DataMember="ImportExternalTran" CaptionVisible="False" SkinID="Transparent">
		<Template>
			<px:PXLayoutRule ID="ImportPaymentFormLayout" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXTextEdit ID="edTranNumber" runat="server" DataField="TranNumber" CommitChanges="True" />
			<px:PXSelector ID="edProcessingCenterID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="ProcessingCenterID" />
		</Template>
	</px:PXFormView>

	<px:PXPanel runat="server" ID="ImportPaymentSmartButtons" SkinID="Buttons">
		<px:PXButton ID="ImportPaymentCreateButton" runat="server" CommitChanges="True" CommandName="ImportDocumentPaymentCreate" CommandSourceID="ds" Height="20" SyncVisible="true" DialogResult="OK" />
		<px:PXButton ID="ImportPaymentCancelButton" runat="server" DialogResult="Cancel" Text="Cancel" Height="20" />
	</px:PXPanel>

</px:PXSmartPanel>