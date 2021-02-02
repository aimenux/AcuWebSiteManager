<px:PXLayoutRule runat="server" GroupCaption="Primary Contact" StartGroup="True" ControlSize="XM" LabelsWidth="SM" />
	<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
	<px:PXSelector ID="edPrmCntID" runat="server" DataField="PrimaryContactID" AutoRefresh="true" DisplayMode="Text" CommitChanges="True" AllowEdit="True" OnEditRecord="edPrmCntID_EditRecord" TabIndex="-1"/>

<px:PXFormView ID="frmPrimaryContact" runat="server" CaptionVisible="False" DataSourceID="ds" DataMember="PrimaryContactCurrent" SkinID="Transparent" TabIndex="400" >
	<Template>
		<px:PXLayoutRule runat="server" />
		<px:PXLayoutRule runat="server" Merge="True" ControlSize="XM" LabelsWidth="SM" />
			<px:PXLabel runat="server"  Text="Name:" />
			<px:PXTextEdit ID="edPrmCntFirstName" runat="server" CommitChanges="True" DataField="FirstName" SuppressLabel="True" Width="84px" Placeholder="First Name" />
			<px:PXTextEdit ID="edPrmCntLastName" runat="server" CommitChanges="True" DataField="LastName" SuppressLabel="True" Width="150px" Placeholder="Last Name" />
		<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
			<px:PXTextEdit ID="edPrmCntJobTitle" runat="server" DataField="Salutation" CommitChanges="True" />
			<px:PXMailEdit ID="edPrmCntEmail" runat="server" DataField="EMail" CommandName="NewMailActivity" CommandSourceID="ds" CommitChanges="True" />
		<px:PXLayoutRule runat="server" Merge="True" ControlSize="XM" LabelsWidth="SM" />
			<px:PXDropDown ID="ddPrmCntPhone1Type" runat="server" DataField="Phone1Type" CommitChanges="True" Size="S" SuppressLabel="True" TabIndex="-1" />
			<px:PXLabel ID="lblPrmCntPhone1" runat="server" Text=" " SuppressLabel="true" />
			<px:PXMaskEdit ID="mePrmCntPhone1" runat="server" DataField="Phone1" CommitChanges="True" SuppressLabel="True" LabelWidth="34px" />
		<px:PXLayoutRule runat="server" Merge="True" ControlSize="XM" LabelsWidth="SM" />
			<px:PXDropDown ID="ddPrmCntPhone2Type" runat="server" DataField="Phone2Type" CommitChanges="True" Size="S" SuppressLabel="True" TabIndex="-1" />
			<px:PXLabel ID="lblPrmCntPhone2" runat="server" Text=" " SuppressLabel="true" />
			<px:PXMaskEdit ID="mePrmCntPhone2" runat="server" DataField="Phone2" CommitChanges="True" SuppressLabel="True" LabelWidth="34px" />
		<px:PXLayoutRule runat="server" GroupCaption="Contact Personal Data Privacy" ControlSize="XM" LabelsWidth="SM" />
			<px:PXCheckBox ID="cbPrmCntConsent" runat="server" DataField="ConsentAgreement" AlignLeft="True" CommitChanges="True" TabIndex="-1" />
			<px:PXDateTimeEdit ID="edPrmCntConsentDate" runat="server" DataField="ConsentDate" CommitChanges="True" />
			<px:PXDateTimeEdit ID="edPrmCntConsentExpDate" runat="server" DataField="ConsentExpirationDate" CommitChanges="True" />
	</Template>
	<ContentLayout OuterSpacing="None" />
	<ContentStyle BackColor="Transparent" BorderStyle="None">
	</ContentStyle>
</px:PXFormView>