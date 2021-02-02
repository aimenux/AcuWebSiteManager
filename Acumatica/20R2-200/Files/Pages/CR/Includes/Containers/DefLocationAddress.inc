<px:PXCheckBox ID="chkOverrideAddress" runat="server" DataField="OverrideAddress" CommitChanges="True" SuppressLabel="True" TabIndex="-1" />

<px:PXFormView ID="DefLocationAddress" runat="server" DataMember="DefLocationAddress" RenderStyle="Simple">
	<Template>
		<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
			<px:PXButton		ID="btnDefLocationAddressLookup"		runat="server" CommandName="DefLocationAddressLookup" CommandSourceID="ds" Size="xs" TabIndex="-1" />
			<px:PXButton		ID="btnViewDefLocationAddressOnMap"		runat="server" CommandName="ViewDefLocationAddressOnMap" CommandSourceID="ds" Size="xs" Text="View on Map" TabIndex="-1" />
			<px:PXTextEdit		ID="edDefLocationAddressAddressLine1"	runat="server" DataField="AddressLine1"	CommitChanges="True" />
			<px:PXTextEdit		ID="edDefLocationAddressAddressLine2"	runat="server" DataField="AddressLine2"	CommitChanges="True" />
			<px:PXTextEdit		ID="edDefLocationAddressCity"			runat="server" DataField="City"			CommitChanges="True" />
			<px:PXSelector		ID="edDefLocationAddressState"			runat="server" DataField="State"		CommitChanges="True" AutoRefresh="True" AllowAddNew="True" />
			<px:PXMaskEdit		ID="edPostalCode"						runat="server" DataField="PostalCode"	CommitChanges="True" />
			<px:PXSelector		ID="edDefLocationAddressCountryID"		runat="server" DataField="CountryID"	CommitChanges="True" AllowAddNew="True" />

		<px:PXLayoutRule runat="server" />
			<px:PXCheckBox		ID="edDefLocationAddressIsValidated"	runat="server" DataField="IsValidated"	Enabled="False" />
	</Template>
</px:PXFormView>