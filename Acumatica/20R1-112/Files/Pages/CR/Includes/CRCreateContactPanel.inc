<px:PXSmartPanel ID="PanelCreateContact" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="New Contact"
	CaptionVisible="True" LoadOnDemand="true" Key="ContactInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateContact" AutoCallBack-Command="Refresh"
	AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel" AutoReload="True" AllowResize="False">
	
	<px:PXTab ID="tabCreateContact" runat="server" Width="100%" DataSourceID="ds" DataMember="ContactInfo">
	
		<Items>

			<px:PXTabItem Text="Main" RepaintOnDemand="True">
				<Template>
					<px:PXFormView ID="formCreateContact" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
									DataMember="ContactInfo">
						<Template>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM"/>
							<px:PXTextEdit ID="edFirstName" runat="server" DataField="FirstName" CommitChanges="True" TabIndex="10"/>
							<px:PXTextEdit ID="edLastname" runat="server" DataField="Lastname" CommitChanges="True" TabIndex="20"/>
							<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" CommitChanges="True" TabIndex="30"/>
							<px:PXLayoutRule runat="server" Merge="True" />
								<px:PXDropDown ID="Phone1Type" runat="server" DataField="Phone1Type" Size="S" SuppressLabel="True" CommitChanges="True" TabIndex="-1"/>
								<px:PXLabel ID="lblPhone1" runat="server" Text=" " SuppressLabel="true" />
								<px:PXMaskEdit ID="Phone1" runat="server" DataField="Phone1" SuppressLabel="True" LabelWidth="84px" CommitChanges="True" TabIndex="50"/>
							<px:PXLayoutRule runat="server" />
							<px:PXTextEdit ID="edEmail" runat="server" DataField="Email" CommitChanges="True" TabIndex="60"/>
							<px:PXSelector ID="edContactClass" runat="server" DataField="ContactClass" CommitChanges="True" TabIndex="70"/>
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Attributes" RepaintOnDemand="False">
				<Template>
					<px:PXGrid ID="grdContactInfoAttributes" runat="server" Width="500px" DataSourceID="ds" MatrixMode="True" AutoAdjustColumns="true">
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
				</Template>
			</px:PXTabItem>

		</Items>

	</px:PXTab>

	<px:PXPanel ID="CreateContactBtn" runat="server" SkinID="Buttons">
		<px:PXButton ID="CreateContactBtnReview" runat="server" Text="Create and review" DialogResult="Yes"></px:PXButton>
		<px:PXButton ID="CreateContactBtnConvert" runat="server" Text="Create" DialogResult="OK"></px:PXButton>
		<px:PXButton ID="CreateContactBtnCancel" runat="server" DialogResult="Cancel" Text="Cancel" Style="margin-left: 5px"/>
	</px:PXPanel>

</px:PXSmartPanel>