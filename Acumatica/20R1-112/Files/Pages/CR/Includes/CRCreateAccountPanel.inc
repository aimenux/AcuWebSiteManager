<px:PXSmartPanel ID="PanelCreateAccount" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="New Account"
	CaptionVisible="True" LoadOnDemand="true" Key="AccountInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateAccount" AutoCallBack-Command="Refresh"
	AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel" AutoReload="True" AllowResize="False">
	
	<px:PXTab ID="tabCreateAccount" runat="server" Width="100%" DataSourceID="ds" DataMember="AccountInfo">
	
		<Items>

			<px:PXTabItem Text="Main" RepaintOnDemand="True">
				<Template>
					<px:PXFormView ID="formCreateAccount" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
									DataMember="AccountInfo">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM"/>
							<px:PXMaskEdit ID="edBAccountID" runat="server" DataField="BAccountID" CommitChanges="True" TabIndex="10"/>
							<px:PXTextEdit ID="edAccountName" runat="server" DataField="AccountName" CommitChanges="True" TabIndex="20"/>
							<px:PXSelector ID="edAccountClass" runat="server" DataField="AccountClass" CommitChanges="True" TabIndex="30"/>
							<px:PXCheckBox ID="edLinkContactToAccount" runat="server" DataField="LinkContactToAccount" CommitChanges="true" TabIndex="40"/>
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Attributes" RepaintOnDemand="False">
				<Template>
					<px:PXGrid ID="grdAccountInfoAttributes" runat="server" Width="500px" DataSourceID="ds" MatrixMode="True" AutoAdjustColumns="true">
						<Levels>
							<px:PXGridLevel DataMember="AccountInfoAttributes">
								<Columns>
									<px:PXGridColumn DataField="DisplayName" Width="250px"/>
									<px:PXGridColumn DataField="Value" AutoCallBack="True" CommitChanges="True" Width="250px"/>
								</Columns>
								<Layout ColumnsMenu="False" />
								<Mode AllowAddNew="false" AllowDelete="false" />
							</px:PXGridLevel>
						</Levels>
						<Mode InitNewRow="true" />
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>

		</Items>

	</px:PXTab>

	<px:PXPanel ID="CreateAccountBtn" runat="server" SkinID="Buttons">
		<px:PXButton ID="CreateAccountBtnReview" runat="server" Text="Create and review" DialogResult="Yes"></px:PXButton>
		<px:PXButton ID="CreateAccountBtnConvert" runat="server" Text="Create" DialogResult="OK"></px:PXButton>
		<px:PXButton ID="CreateAccountBtnCancel" runat="server" DialogResult="Cancel" Text="Cancel" Style="margin-left: 5px"/>
	</px:PXPanel>
</px:PXSmartPanel>