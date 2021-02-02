<px:PXGrid ID="PXGridDuplicates" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" Height="200px" MatrixMode="True">
	<ActionBar>
		<CustomItems>
			<px:PXToolBarButton Text="Merge" Key="cmdMerge">
				<AutoCallBack Command="DuplicateMerge" Target="ds"/>
				<PopupCommand Command="Cancel" Target="ds"/>
			</px:PXToolBarButton>
			<px:PXToolBarButton Text="Attach to Account" Key="cmdAttachToAccount">
				<AutoCallBack Command="DuplicateAttach" Target="ds"/>
				<PopupCommand Command="Cancel" Target="ds"/>
			</px:PXToolBarButton>
		</CustomItems>
	</ActionBar>
	<Levels>
		<px:PXGridLevel DataMember="Duplicates">
			<Columns>
				<px:PXGridColumn DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True"/>
				<px:PXGridColumn DataField="DuplicateContact__ContactType" TextAlign="Left" AllowShowHide="False"/>
				<px:PXGridColumn DataField="DuplicateContact__DuplicateStatus" TextAlign="Left"/>
				<px:PXGridColumn DataField="DuplicateContact__LastModifiedDateTime" TextAlign="Left" Width="130px"/>
				<px:PXGridColumn DataField="DuplicateContact__DisplayName" TextAlign="Left" AllowShowHide="False" LinkCommand="ViewDuplicate"/>
				<px:PXGridColumn DataField="DuplicateRefContactID" TextAlign="Left" Width="110px" LinkCommand="ViewDuplicateRefContact" DisplayMode="Text"/>
				<px:PXGridColumn DataField="DuplicateContact__BAccountID" TextAlign="Left" />
				<px:PXGridColumn DataField="BAccountR__Type" TextAlign="Left" Width="160px"/>
				<px:PXGridColumn DataField="BAccountR__AcctName" TextAlign="Left"/>
				<px:PXGridColumn DataField="DuplicateContact__Email" TextAlign="Left" AllowShowHide="False"/>
			</Columns>
			<Layout FormViewHeight=""/>
		</px:PXGridLevel>
	</Levels>
	<AutoSize Enabled="True" MinHeight="200"/>
	<ActionBar>
		<Actions>
			<Search Enabled="False"/>
		</Actions>
	</ActionBar>
	<Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False"/>
</px:PXGrid>