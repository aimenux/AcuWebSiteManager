<px:PXSmartPanel 
	ID="spMergeParamsDlg" runat="server" Height="400"
	Width="600" Caption="Please resolve the conflicts" CaptionVisible="True" Key="MergeParam" LoadOnDemand="True" ShowAfterLoad="true"
	AutoCallBack-Enabled="true" AutoCallBack-Target="formMerge" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
	AcceptButtonID="btnOk" CancelButtonID="btnCancel">

	<px:PXFormView ID="formMerge" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" SkinID="Transparent"
					DataMember="MergeParam">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True"/>
			<px:PXSelector CommitChanges="True" ID="edTargetEntityID" runat="server" DataField="TargetEntityID" FilterByAllFields="True" TextMode="Search" AutoRefresh="True"/>
		</Template>
	</px:PXFormView>

	<px:PXGrid ID="grdFields" runat="server" Width="100%" DataSourceID="ds" MatrixMode="True" AutoAdjustColumns="true">
		<Levels>
			<px:PXGridLevel DataMember="PopupConflicts">
				<Columns>
					<px:PXGridColumn DataField="DisplayName"/>
					<px:PXGridColumn DataField="Value" AutoCallBack="True"/>
				</Columns>
				<Layout ColumnsMenu="False"/>
				<Mode AllowAddNew="false" AllowDelete="false"/>
			</px:PXGridLevel>
		</Levels>
		<ActionBar>
			<Actions>
				<ExportExcel Enabled="False"/>
				<AddNew Enabled="False"/>
				<FilterShow Enabled="False"/>
				<FilterSet Enabled="False"/>
				<Save Enabled="False"/>
				<Delete Enabled="False"/>
				<NoteShow Enabled="False"/>
				<Search Enabled="False"/>
				<AdjustColumns Enabled="False"/>
			</Actions>
		</ActionBar>
		<CallbackCommands>
			<Save PostData="Page"/>
		</CallbackCommands>
		<AutoSize Enabled="true"/>
	</px:PXGrid>
	<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
		<px:PXButton ID="btnMergeOK" runat="server" Text="OK" DialogResult="OK"/>
		<px:PXButton ID="btnMergeCancel" runat="server" Text="Cancel" DialogResult="Cancel"/>
	</px:PXPanel>
</px:PXSmartPanel>