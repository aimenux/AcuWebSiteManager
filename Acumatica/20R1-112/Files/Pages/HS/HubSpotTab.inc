<px:PXTabItem Text="HubSpot">
	<Template>
		<px:PXGrid ID="hsSyncGrid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="Inquire" SyncPosition="true">
			<Levels>
				<px:PXGridLevel DataMember="HubSpotSyncRecs" DataKeyNames="SyncRecordID">
					<Columns>
						<px:PXGridColumn DataField="SYProvider__Name" Width="200px" />
						<px:PXGridColumn DataField="RemoteID" Width="200px" CommitChanges="True" LinkCommand="GoToHubSpot" />
						<px:PXGridColumn DataField="SyncStatus" Width="120px" />
						<px:PXGridColumn DataField="LastSource" Width="100px" />
						<px:PXGridColumn DataField="LastOperation" Width="100px" />
						<px:PXGridColumn DataField="LastErrorMessageSimplified" Width="230" />
						<px:PXGridColumn DataField="LastAttemptTS" Width="120px" DisplayFormat="g" />
						<px:PXGridColumn DataField="RemoteTS" Width="120px" DisplayFormat="g" />
						<px:PXGridColumn DataField="AttemptCount" Width="120px" />
						<px:PXGridColumn DataField="HSEntitySetup__ImportScenario" Width="150" />
						<px:PXGridColumn DataField="HSEntitySetup__ExportScenario" Width="150" />
						<px:PXGridColumn DataField="SyncRecordID" Visible="False" SyncVisible="False" SyncVisibility="False" />
						<px:PXGridColumn DataField="LastErrorMessage" Visible="False" SyncVisible="False" SyncVisibility="False" />
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
			<ActionBar>
				<CustomItems>
					<px:PXToolBarButton Key="SyncHubSpot">
						<AutoCallBack Command="SyncHubSpot" Target="ds"/>
					</px:PXToolBarButton>
					<px:PXToolBarButton Key="PushToHubSpot">
						<AutoCallBack Command="PushToHubSpot" Target="ds"/>
					</px:PXToolBarButton>
					<px:PXToolBarButton Key="PullFromHubSpot">
						<AutoCallBack Command="PullFromHubSpot" Target="ds"/>
					</px:PXToolBarButton>
				</CustomItems>
			</ActionBar>
			<Mode InitNewRow="true" />
			<AutoSize Enabled="True" MinHeight="150" />
		</px:PXGrid>
	</Template>
</px:PXTabItem>