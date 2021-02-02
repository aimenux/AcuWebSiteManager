<px:PXSmartPanel ID="PanelCreateBothContactAndAccount" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="New Account"
	CaptionVisible="True" LoadOnDemand="true" Key="AccountInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateContactForBoth" AutoCallBack-Command="Refresh"
	AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel" AutoReload="True" AllowResize="False">

	<px:PXTab ID="tabCreateAccount" runat="server" Width="100%" DataSourceID="ds" DataMember="ContactInfo">
	
		<Items>

			<px:PXTabItem Text="Main" RepaintOnDemand="True">
				<Template>

					<px:PXLayoutRule runat="server" GroupCaption="Business Account"/>
					<px:PXFormView ID="formCreateAccountForBoth" runat="server" DataSourceID="ds" Style="z-index: 100" Width="500px" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
									DataMember="AccountInfo">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM"/>
							<px:PXMaskEdit ID="edBAccountID" runat="server" DataField="BAccountID" CommitChanges="True" TabIndex="10"/>
							<px:PXTextEdit ID="edAccountName" runat="server" DataField="AccountName" CommitChanges="True" TabIndex="20"/>
							<px:PXSelector ID="edAccountClass" runat="server" DataField="AccountClass" CommitChanges="True" TabIndex="30"/>
							<px:PXCheckBox ID="edLinkContactToAccount" runat="server" DataField="LinkContactToAccount" CommitChanges="true" TabIndex="40"/>
						</Template>
					</px:PXFormView>

					<px:PXLayoutRule runat="server" GroupCaption="Contact"/>
					<px:PXFormView ID="formCreateContactForBoth" runat="server" DataSourceID="ds" Style="z-index: 100" Width="500px" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
									DataMember="ContactInfo">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM"/>
							<px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" CommitChanges="True" TabIndex="50"/>
							<px:PXTextEdit ID="edLastname" runat="server" DataField="Lastname" CommitChanges="True" TabIndex="60"/>
							<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" CommitChanges="True" TabIndex="70"/>
							<px:PXLayoutRule runat="server" Merge="True" />
								<px:PXDropDown ID="Phone1Type" runat="server" DataField="Phone1Type" Size="S" SuppressLabel="True" CommitChanges="True" TabIndex="-1"/>
								<px:PXLabel ID="lblPhone1" runat="server" Text=" " SuppressLabel="true" />
								<px:PXMaskEdit ID="Phone1" runat="server" DataField="Phone1" SuppressLabel="True" LabelWidth="84px" CommitChanges="True" TabIndex="90"/>
							<px:PXLayoutRule runat="server" />
							<px:PXTextEdit ID="edEmail" runat="server" DataField="Email" CommitChanges="True" TabIndex="100"/>
							<px:PXSelector ID="edContactClass" runat="server" DataField="ContactClass" CommitChanges="True" TabIndex="110"/>
						</Template>
					</px:PXFormView>

				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Attributes" RepaintOnDemand="False">
				<Template>

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Business Account"/>
						<px:PXGrid ID="grdAccountInfoAttributesBoth" runat="server" Width="500px" DataSourceID="ds" MatrixMode="True" AutoAdjustColumns="true">
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
					<px:PXLayoutRule runat="server" />

					<px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Contact"/>
						<px:PXGrid ID="grdContactInfoAttributesBoth" runat="server" Width="500px" DataSourceID="ds" MatrixMode="True" AutoAdjustColumns="true">
							<Levels>
								<px:PXGridLevel DataMember="ContactInfoAttributes">
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
					<px:PXLayoutRule runat="server" />

				</Template>
			</px:PXTabItem>

		</Items>

	</px:PXTab>


	<px:PXPanel ID="CreateContactAndAccountBtn" runat="server" SkinID="Buttons">
		<px:PXButton ID="CreateContactAndAccountBtnReview" runat="server" Text="Create and review" DialogResult="Yes"></px:PXButton>
		<px:PXButton ID="CreateContactAndAccountBtnConvert" runat="server" Text="Create" DialogResult="OK"></px:PXButton>
		<px:PXButton ID="CreateContactAndAccountBtnCancel" runat="server" DialogResult="Cancel" Text="Cancel" Style="margin-left: 5px"/>
	</px:PXPanel>
</px:PXSmartPanel>