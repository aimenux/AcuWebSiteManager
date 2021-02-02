<px:PXSmartPanel ID="PanelCreateOpportunity" runat="server" Style="z-index: 108; position: absolute; left: 27px; top: 99px;" Caption="New Opportunity"
	CaptionVisible="True" LoadOnDemand="true" Key="OpportunityInfo" AutoCallBack-Enabled="true" AutoCallBack-Target="formCreateOpportunity" AutoCallBack-Command="Refresh"
	AcceptButtonID="PXButtonOK" CancelButtonID="PXButtonCancel" AutoReload="True" AllowResize="False">

	<px:PXTab ID="tabCreateOpportunity" runat="server" Width="100%" DataSourceID="ds" DataMember="OpportunityInfo">
	
		<Items>

			<px:PXTabItem Text="Main" RepaintOnDemand="True">
				<Template>
					<px:PXFormView ID="formCreateOpportunity" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="Services Settings" CaptionVisible="False" SkinID="Transparent"
									DataMember="OpportunityInfo">
						<Template>
							<px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM"/>
							<px:PXMaskEdit ID="edSubject" runat="server" DataField="Subject" TabIndex="10"/>
							<px:PXDateTimeEdit ID="edCloseDate" DataField="CloseDate" runat="server" ControlSize="XM" TabIndex="20"/>
							<px:PXSelector ID="edOpportunityClass" runat="server" DataField="OpportunityClass" CommitChanges="True" TabIndex="30"/>
						</Template>
					</px:PXFormView>
				</Template>
			</px:PXTabItem>

			<px:PXTabItem Text="Attributes" RepaintOnDemand="False">
				<Template>
					<px:PXGrid ID="grdOpportunityInfoAttributes" runat="server" Width="500px" DataSourceID="ds" MatrixMode="True" AutoAdjustColumns="true">
						<Levels>
							<px:PXGridLevel DataMember="OpportunityInfoAttributes">
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

	<px:PXPanel ID="CreateOpportunityBtn" runat="server" SkinID="Buttons">
		<px:PXButton ID="CreateOpportunityBtnReview" runat="server" Text="Create and review" DialogResult="Yes"></px:PXButton>
		<px:PXButton ID="CreateOpportunityBtnConvert" runat="server" Text="Create" DialogResult="OK"></px:PXButton>
		<px:PXButton ID="CreateOpportunityBtnCancel" runat="server" DialogResult="Cancel" Text="Cancel" Style="margin-left: 5px"/>
	</px:PXPanel>
</px:PXSmartPanel>