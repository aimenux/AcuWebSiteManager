<px:PXSmartPanel ID="CreatePaymentSmartPanel" runat="server" Caption="Create Payment" CaptionVisible="True" LoadOnDemand="True" Key="QuickPayment"
	AutoCallBack-Target="CreatePaymentFormView" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page" Height="290px">

	<px:PXLayoutRule runat="server" StartColumn="True" />

	<px:PXFormView ID="CreatePaymentFormView" runat="server" DataSourceID="ds" Width="100%" DataMember="QuickPayment" CaptionVisible="False" SkinID="Transparent" Height="230px">
		<Template>
			<px:PXLayoutRule ID="CreatePaymentFormLayout" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXLayoutRule runat="server" Merge="True" />
			<px:PXNumberEdit ID="edCuryOrigDocAmt" runat="server" DataField="CuryOrigDocAmt" CommitChanges="True"/>
			<px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" SuppressLabel="True" />
			<px:PXLayoutRule runat="server" />
			<px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" />
			<px:PXSelector ID="selPaymentMethodID" runat="server" DataField="PaymentMethodID" CommitChanges="true" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox ID="chkNewCard" runat="server" CommitChanges="true" DataField="NewCard" />
			<px:PXCheckBox ID="chkSaveCard" runat="server" CommitChanges="true" DataField="SaveCard" />
            <px:PXLayoutRule runat="server" />
			<px:PXSelector CommitChanges="True" ID="edPMInstanceID" runat="server" DataField="PMInstanceID" TextField="Descr" AutoRefresh="True" AutoGenerateColumns="True" />
			<px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" AutoRefresh="True" />
			<px:PXSelector ID="edProcessingCenterID" runat="server" AutoRefresh="True" CommitChanges="True" DataField="ProcessingCenterID" />
			<px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" CommitChanges="True" />
		</Template>
	</px:PXFormView>

	<px:PXPanel runat="server" ID="CreatePaymentSmartButtons" SkinID="Buttons">
		<px:PXButton ID="CreatePaymentCaptureButton" runat="server" CommitChanges="True" CommandName="CreatePaymentCapture" CommandSourceID="ds" Height="20" SyncVisible="true" DialogResult="Yes" />
		<px:PXButton ID="CreatePaymentAuthorizeButton" runat="server" CommitChanges="True" CommandName="CreatePaymentAuthorize" CommandSourceID="ds" Height="20" SyncVisible="true" DialogResult="No" />
		<px:PXButton ID="CreatePaymentOKButton" runat="server" CommitChanges="True" CommandName="CreatePaymentOK" CommandSourceID="ds" Height="20" SyncVisible="true" DialogResult="OK" />
		<px:PXButton ID="CreatePaymentCancelButton" runat="server" DialogResult="Cancel" Text="Cancel" Height="20" />
	</px:PXPanel>

</px:PXSmartPanel>