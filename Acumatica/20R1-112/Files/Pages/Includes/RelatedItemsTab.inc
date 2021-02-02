<px:PXTabItem Text="Related Items">
	<Template>
		<px:PXGrid ID="relatedItemsGrid" runat="server" DataSourceID="ds" Height="150px" Width="100%" ActionsPosition="Top" SkinID="Details" 
			SyncPosition="True" FilesIndicator="False" NoteIndicator="True" >
			<Levels>
				<px:PXGridLevel DataMember="RelatedItems" DataKeyNames="InventoryID,Relation" ImportDataMember="RelatedItems">
					<Columns>
						<px:PXGridColumn DataField="Relation" Width="100px" CommitChanges="true" Type="DropDownList" />
						<px:PXGridColumn DataField="Rank" Width="60px" CommitChanges="true"/>
						<px:PXGridColumn DataField="Tag" Width="180px" CommitChanges="true" Type="DropDownList" />
						<px:PXGridColumn DataField="RelatedInventoryID" Width="150px" CommitChanges="true" LinkCommand="ViewRelatedItem" />
						<px:PXGridColumn DataField="Desc" Width="250px" />
						<px:PXGridColumn DataField="UOM" Width="90px" DisplayFormat="&gt;aaaaaa" CommitChanges="true" />
						<px:PXGridColumn DataField="EffectiveDate" Width="100px" CommitChanges="true" />
						<px:PXGridColumn DataField="ExpirationDate" Width="100px" CommitChanges="true" />
						<px:PXGridColumn DataField="IsActive" Width="60px" Type="CheckBox" CommitChanges="true" />
					</Columns>
					<RowTemplate>
						<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
						<px:PXDropDown ID="edRelation" runat="server" DataField="Relation" CommitChanges="true" />
						<px:PXNumberEdit ID="edRank" runat="server" DataField="Rank" CommitChanges="true" />
						<px:PXDropDown ID="edTag" runat="server" DataField="Tag" CommitChanges="true" />
						<px:PXSelector ID="edRelatedInventoryID" runat="server" DataField="RelatedInventoryID" AllowEdit="True" CommitChanges="true" />
						<px:PXTextEdit ID="edRelatedInventoryDesc" runat="server" DataField="Desc" Enabled="False" />
						<px:PXSelector ID="edRelatedInventoryUOM" runat="server" Size="s" DataField="UOM" AutoRefresh="True" CommitChanges="true" />
						<px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="EffectiveDate" />
						<px:PXDateTimeEdit ID="edExpirationDate" runat="server" DataField="ExpirationDate" />
						<px:PXCheckBox ID="edRelatedItemIsActive" runat="server" DataField="IsActive" CommitChanges="true" />
					</RowTemplate>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
			<Mode InitNewRow="true" AllowUpload="True" />
			<AutoSize Enabled="True" MinHeight="150" />
		</px:PXGrid>
	</Template>
</px:PXTabItem>