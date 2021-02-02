<px:PXCheckBox ID="chkOverrideContact" runat="server" DataField="OverrideContact" CommitChanges="True" TabIndex="-1" />

<px:PXFormView ID="DefLocationContact" runat="server" DataMember="DefLocationContact" RenderStyle="Simple">
	<Template>
		<px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
			<px:PXTextEdit ID="edDefLocationContactFullName"	runat="server" DataField="FullName" />
			<px:PXTextEdit ID="edDefLocationContactAttention"	runat="server" DataField="Attention" />
		<px:PXLayoutRule runat="server" Merge="True"/>
			<px:PXDropDown ID="Phone1Type"						runat="server" DataField="Phone1Type" Size="S" SuppressLabel="True" TabIndex="-1" />
			<px:PXLabel ID="lblPhone1"							runat="server" Text=" " SuppressLabel="true" />
			<px:PXMaskEdit ID="PXMaskEdit1"						runat="server" DataField="Phone1" SuppressLabel="True" LabelWidth="34px" />
		<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXDropDown ID="Phone2Type"						runat="server" DataField="Phone2Type" Size="S" SuppressLabel="True" TabIndex="-1" />
			<px:PXLabel ID="lblPhone2"							runat="server" Text=" " SuppressLabel="true" />
			<px:PXMaskEdit ID="PXMaskEdit2"						runat="server" DataField="Phone2" SuppressLabel="True" LabelWidth="34px" />
		<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXDropDown ID="FaxType"							runat="server" DataField="FaxType" Size="S" SuppressLabel="True" TabIndex="-1" />
			<px:PXLabel ID="lblFax"								runat="server" Text=" " SuppressLabel="true" />
			<px:PXMaskEdit ID="PXMaskEdit3"						runat="server" DataField="Fax" SuppressLabel="True" LabelWidth="34px" />
		<px:PXLayoutRule runat="server" />
			<px:PXMailEdit ID="edDefLocationContactEMail"		runat="server" DataField="EMail" CommitChanges="True" />
			<px:PXLinkEdit ID="edDefLocationContactWebSite"		runat="server" DataField="WebSite" CommitChanges="True" />
	</Template>
</px:PXFormView>