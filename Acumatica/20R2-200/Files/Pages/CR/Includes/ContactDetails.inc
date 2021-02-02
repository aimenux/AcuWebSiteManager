<px:PXTabItem Text="Contacts" LoadOnDemand="true">
	<Template>
		<px:PXGrid ID="grdContacts" runat="server" DataSourceID="ds" Width="100%" SkinID="Inquire" AllowSearch="True"
			SyncPosition="true" ActionsPosition="Top" AllowFilter="True" FastFilterFields="DisplayName,Salutation,EMail,Phone1">
			<Levels>
				<px:PXGridLevel DataMember="Contacts">
					<Columns>
						<px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
						<px:PXGridColumn DataField="DisplayName" LinkCommand="ViewContact" />
						<px:PXGridColumn DataField="Salutation" />
						<px:PXGridColumn DataField="IsPrimary" TextAlign="Center" Type="CheckBox"  />
						<px:PXGridColumn DataField="EMail" />
						<px:PXGridColumn DataField="Phone1" />

						<%-- hidden by default --%>
						<px:PXGridColumn DataField="OwnerID" DisplayMode="Text" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="FullName" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="ClassID" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="LastModifiedDateTime" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Source" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="AssignDate" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="DuplicateStatus" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Phone2" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Phone3" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="DateOfBirth" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Fax" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Gender" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Method" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="NoCall" TextAlign="Center" Type="CheckBox" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="NoEMail" TextAlign="Center" Type="CheckBox" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="NoFax" TextAlign="Center" Type="CheckBox" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="NoMail" TextAlign="Center" Type="CheckBox" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="NoMarketing" TextAlign="Center" Type="CheckBox" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="NoMassMail" TextAlign="Center" Type="CheckBox" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="CampaignID" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Phone1Type" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Phone2Type" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Phone3Type" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="FaxType" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="MaritalStatus" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Spouse" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Status" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Resolution" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="LanguageID" Visible="false" SyncVisible="false" />

						<px:PXGridColumn DataField="Address__CountryID" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Address__State" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Address__City" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Address__AddressLine1" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Address__AddressLine2" Visible="false" SyncVisible="false" />
						<px:PXGridColumn DataField="Address__PostalCode" Visible="false" SyncVisible="false" />

						<%-- hidden at all --%>
						<px:PXGridColumn DataField="CanBeMadePrimary" TextAlign="Center" Type="CheckBox"/>
					</Columns>
					<Layout FormViewHeight="" />
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
			<ActionBar DefaultAction="cmdViewContact" PagerVisible="False">
				<Actions>
					<FilterBar ToolBarVisible="Top" Order="0" GroupIndex="3" />
				</Actions>
				<CustomItems>
					<px:PXToolBarButton ImageKey="AddNew" Tooltip="Add New Contact" DisplayStyle="Image">
						<AutoCallBack Command="CreateContact" Target="ds" />
						<PopupCommand Command="refreshContact" Target="ds"/>
					</px:PXToolBarButton>
					<px:PXToolBarButton Text="Contact Details" Key="cmdViewContact" Visible="False">
						<AutoCallBack Command="ViewContact" Target="ds" />
						<PopupCommand Command="refreshContact" Target="ds" />
					</px:PXToolBarButton>
					<px:PXToolBarButton DependOnGrid="grdContacts" StateColumn="CanBeMadePrimary" >
						<AutoCallBack Command="MakeContactPrimary" Target="ds" />
					</px:PXToolBarButton>
				</CustomItems>
			</ActionBar>
			<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		</px:PXGrid>
	</Template>
</px:PXTabItem>