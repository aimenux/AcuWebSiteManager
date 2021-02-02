<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CS101000.aspx.cs" Inherits="Page_CS101000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CS.CompanySetup"
		PrimaryView="CompanyHeader">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="SetDefault" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="grdLocations" Name="ViewLocation" Visible="False" />
			<px:PXDSCallbackCommand Name="NewLocation" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewMainOnMap" Visible="false" />
			<px:PXDSCallbackCommand Name="ViewDefLocationOnMap" Visible="false" />
			<px:PXDSCallbackCommand DependOnGrid="grdEmployees" Name="ViewContact" Visible="False" />
			<px:PXDSCallbackCommand Name="NewContact" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="frmHeader" runat="server" Height="36px" Width="100%" Caption="Account Summary"
		DataMember="CompanyHeader" DataSourceID="ds" NoteIndicator="True" 
		TabIndex="1"  FilesIndicator="True" 
	DefaultControlID="">
		<Template>
			&nbsp;
			<px:PXLabel ID="lblAcctCD" runat="server" Style="z-index: 102; left: 9px; position: absolute;
				top: 9px">Account ID :</px:PXLabel>
			<px:PXSegmentMask ID="edAcctCD" runat="server" AllowNull="True" DataField="AcctCD"
				   LabelID="lblAcctCD"
				Style="z-index: 103; left: 126px; position: absolute; top: 9px" TabIndex="1"
				Width="81px">
				<GridProperties FastFilterFields="AcctName">
					
					<Layout ColumnsMenu="False" />
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
				<Items>
					<px:PXMaskItem EditMask="AlphaNumeric" EmptyChar="_" Length="10" Separator="-" TextCase="Upper" />
				</Items>
				<AutoCallBack Command="Save" Enabled="True" Target="frmHeader">
				</AutoCallBack>
			</px:PXSegmentMask>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="522px" Style="z-index: 100"
		Width="100%"  DataMember="BAccount" 
		TabIndex="5" >
		<Items>
			<px:PXTabItem Text="General Info">
				<Template>
					<px:PXPanel ID="pnlMainAddress" runat="server" Caption="Main Address" Height="180px"
						Style="margin-top: 5px; margin-left: 5px; left: 0px; position: absolute; top: 231px;"
						Width="459px">
						<px:PXFormView ID="frmDefAddress" runat="server" CaptionVisible="False" DataSourceID="ds"
							Style="z-index: 100; left: 0px; top: 0px; position: absolute;" Height="100%"
							Width="100%"  DataMember="DefAddress" 
							TabIndex="-1">
							<Template>
								<px:PXLabel ID="lblAddressLine1" runat="server" Style="z-index: 106; left: 9px; position: absolute;
									top: 9px">Address 1 :</px:PXLabel>
								<px:PXTextEdit ID="edAddressLine1" runat="server" AllowNull="True" DataField="AddressLine1"
									LabelID="lblAddressLine1"  Style="z-index: 107; left: 126px; position: absolute;
									top: 9px" TabIndex="18" Width="297px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblAddressLine2" runat="server" Style="z-index: 108; left: 9px; position: absolute;
									top: 36px">Address Line 2 :</px:PXLabel>
								<px:PXTextEdit ID="edAddressLine2" runat="server" AllowNull="True" DataField="AddressLine2"
									LabelID="lblAddressLine2"  Style="z-index: 109; left: 126px; position: absolute;
									top: 36px" TabIndex="19" Width="297px">
								</px:PXTextEdit>
								&nbsp;&nbsp;
								<px:PXLabel ID="lblCity" runat="server" Style="z-index: 112; left: 9px; position: absolute;
									top: 63px">City :</px:PXLabel>
								<px:PXTextEdit ID="edCity" runat="server" AllowNull="True" DataField="City" LabelID="lblCity"
									 Style="z-index: 113; left: 126px; position: absolute; top: 63px"
									TabIndex="20" Width="297px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblCountryID" runat="server" Style="z-index: 116; left: 9px; position: absolute;
									top: 90px">Country :</px:PXLabel>
								<px:PXSelector ID="edCountryID" runat="server" AllowNull="True" DataField="CountryID"
									   HintField="Description"
									HintLabelID="lblCountryIDH" LabelID="lblCountryID"  Style="z-index: 117;
									left: 126px; position: absolute; top: 90px" TabIndex="21" Width="54px" AllowEdit="True">
									<GridProperties>
										<Columns>
											<px:PXGridColumn DataField="CountryID"  SortDirection="Ascending">
												<Header Text="Country ID">
												</Header>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Description"  Width="120px">
												<Header Text="Country">
												</Header>
											</px:PXGridColumn>
										</Columns>
										<Layout ColumnsMenu="False" />
									<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
									<AutoCallBack Command="Save" Enabled="True" Target="frmDefAddress">
									</AutoCallBack>
								</px:PXSelector>
								<px:PXLabel ID="lblCountryIDH" runat="server" Style="z-index: 118; left: 189px; position: absolute;
									top: 90px"></px:PXLabel>
								<px:PXLabel ID="lblPostalCode" runat="server" Style="z-index: 119; left: 9px; position: absolute;
									top: 144px">Postal Code :</px:PXLabel>
								<px:PXMaskEdit ID="edPostalCode" runat="server" AllowNull="True" DataField="PostalCode"
									LabelID="lblPostalCode"  Style="z-index: 120; left: 126px; position: absolute;
									top: 144px" TabIndex="23" Width="117px">
								</px:PXMaskEdit>
								<px:PXLabel ID="lblState" runat="server" Style="z-index: 100; left: 9px; position: absolute;
									top: 117px">State :</px:PXLabel>
								<px:PXSelector ID="edState" runat="server" AllowNull="True" DataField="State" 
									  HintField="name" HintLabelID="lblStateH"
									LabelID="lblState"  Style="z-index: 101; left: 126px; position: absolute;
									top: 117px" TabIndex="22" Width="126px" AutoRefresh="True" AllowEdit="True">
									<GridProperties>
										<Columns>
											<px:PXGridColumn DataField="StateID"  SortDirection="Ascending" Width="200px">
												<Header Text="State ID">
												</Header>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Name"  Width="120px">
												<Header Text="State Name">
												</Header>
											</px:PXGridColumn>
										</Columns>
										<Layout ColumnsMenu="False" />
									<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
									<CallBackMode PostData="Container" />
									<Parameters>
										<px:PXControlParam ControlID="frmDefAddress" Name="Address.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
											Type="String" />
									</Parameters>
								</px:PXSelector>
								<px:PXLabel ID="lblStateH" runat="server" Style="z-index: 102; left: 261px; position: absolute;
									top: 117px"></px:PXLabel>
								<px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewMainOnMap" CommandSourceID="ds"
									Style="left: 333px; position: absolute; top: 144px" Text="View on Map" TabIndex="24">
								</px:PXButton>
							</Template>
							<ContentStyle BackColor="Transparent" BorderStyle="None">
							</ContentStyle>
						</px:PXFormView>
					</px:PXPanel>
					<px:PXPanel ID="pnlDefContact" runat="server" Caption="Company Main Info" Height="204px"
						Style="margin-top: 5px; margin-left: 5px; z-index: 100; left: 0px; position: absolute;
						top: 0px;" Width="459px">
						<px:PXFormView ID="frmDefContact" runat="server" CaptionVisible="False" 
							DataMember="DefContact" DataSourceID="ds" Height="100%" 
							Width="100%" Style="left: 0px; position: absolute; top: 0px" >
							<Template>
								<px:PXLabel ID="lblEMail" runat="server" Style="z-index: 112; left: 9px; position: absolute;
									top: 63px">Email :</px:PXLabel>
								<px:PXMailEdit ID="edEMail" runat="server" AllowNull="True" DataField="EMail" LabelID="lblEMail"
									 Style="z-index: 113; left: 126px; position: absolute; top: 63px"
									TabIndex="5" Width="297px" CommitChanges="True">
								</px:PXMailEdit>
								<px:PXLabel ID="lblFax" runat="server" Style="z-index: 114; left: 9px; position: absolute;
									top: 171px">Fax :</px:PXLabel>
								<px:PXMaskEdit ID="edFax" runat="server" AllowNull="True" DataField="Fax" LabelID="lblFax"
									Style="z-index: 115; left: 126px; position: absolute; top: 171px" TabIndex="9"
									Width="153px">
								</px:PXMaskEdit>
								<px:PXLabel ID="lblPhone1" runat="server" Style="z-index: 116; left: 9px; position: absolute;
									top: 117px">Phone 1 :</px:PXLabel>
								<px:PXMaskEdit ID="edPhone1" runat="server" AllowNull="True" DataField="Phone1" LabelID="lblPhone1"
									Style="z-index: 117; left: 126px; position: absolute; top: 117px" TabIndex="7"
									Width="153px">
								</px:PXMaskEdit>
								<px:PXLabel ID="lblFullName" runat="server" Style="z-index: 100; left: 9px; position: absolute;
									top: 9px">Print Name :</px:PXLabel>
								<px:PXTextEdit ID="edFullName" runat="server" AllowNull="True" DataField="FullName"
									LabelID="lblFullName"  Style="z-index: 101; left: 126px; position: absolute;
									top: 9px" TabIndex="3" Width="297px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblWebSite" runat="server" Style="z-index: 100; left: 9px; position: absolute;
									top: 90px">Web :</px:PXLabel>
								<px:PXLinkEdit ID="edWebSite" runat="server" AllowNull="True" DataField="WebSite"
									LabelID="lblWebSite"  Style="z-index: 101; left: 126px; position: absolute;
									top: 90px" TabIndex="6" Width="297px" CommitChanges="True">
								</px:PXLinkEdit>
								<px:PXLabel ID="lblPhone2" runat="server" Style="z-index: 102; left: 9px; position: absolute;
									top: 144px">Phone 2 :</px:PXLabel>
								<px:PXMaskEdit ID="edPhone2" runat="server" AllowNull="True" DataField="Phone2" LabelID="lblPhone2"
									Style="z-index: 103; left: 126px; position: absolute; top: 144px" TabIndex="8"
									Width="153px">
								</px:PXMaskEdit>
								<px:PXLabel ID="lblSalutation" runat="server" Style="z-index: 100; left: 9px; position: absolute;
									top: 36px">Salutation :</px:PXLabel>
								<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" LabelID="lblSalutation"
									 Style="z-index: 101; left: 126px; position: absolute; top: 36px"
									TabIndex="4" Width="297px">
								</px:PXTextEdit>
							</Template>
							<ContentStyle BackColor="Transparent" BorderStyle="None">
							</ContentStyle>
						</px:PXFormView>
					</px:PXPanel>					
					<px:PXPanel ID="pnlAccountSettings" runat="server" Caption="Account Settings" Height="411px"
						Width="423px" Style="margin-top: 5px; margin-left: 5px; left: 468px; position: absolute;
						top: 0px;">
						<px:PXFormView ID="frmCompanyCurrency" runat="server" CaptionVisible="False" DataSourceID="ds"
							Height="90px" Width="100%" Style="left: 0px; position: absolute; top: 117px"
							 DataMember="CompanyCurrency">
							<ContentStyle BackColor="Transparent" BorderStyle="None">
							</ContentStyle>
							<Template>
								<px:PXLabel ID="lblDescription" runat="server" Style="z-index: 102; left: 9px; position: absolute;
									top: 0px">Description :</px:PXLabel>
								<px:PXTextEdit ID="edDescription" runat="server" AllowNull="True" DataField="Description"
									LabelID="lblDescription"  Style="z-index: 103; left: 126px; position: absolute;
									top: 0px" TabIndex="15" Width="261px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblCurySymbol" runat="server" Style="z-index: 104; left: 9px; position: absolute;
									top: 27px">Cury Symbol :</px:PXLabel>
								<px:PXTextEdit ID="edCurySymbol" runat="server" AllowNull="True" DataField="CurySymbol"
									LabelID="lblCurySymbol"  Style="z-index: 105; left: 126px; position: absolute;
									top: 27px" TabIndex="16" Width="54px">
								</px:PXTextEdit>
								&nbsp;&nbsp;
								<px:PXLabel ID="lblDecimalPlaces" runat="server" Style="z-index: 108; left: 9px;
									position: absolute; top: 54px">Decimal Precision :</px:PXLabel>
								<px:PXNumberEdit ID="edDecimalPlaces" runat="server" DataField="DecimalPlaces" LabelID="lblDecimalPlaces"
									  Style="z-index: 109; left: 126px; position: absolute;
									top: 54px" TabIndex="17" Text="2" ValueType="Int16" Width="54px">
								</px:PXNumberEdit>
							</Template>
						</px:PXFormView>
						<px:PXFormView ID="frmCompanyBody" runat="server" CaptionVisible="False" DataSourceID="ds"
							Height="90px" Style="left: 0px; position: absolute; top: 27px" Width="100%" DataMember="CompanyHeader"
							 >
							<Template>
								<px:PXLabel ID="lblCompanyBaseCuryID" runat="server" Style="z-index: 100; left: 9px;
									position: absolute; top: 63px">Base Currency ID :</px:PXLabel>
								<px:PXSelector ID="edCompanyBaseCuryID" runat="server" AllowNull="True" DataField="CompanyBaseCuryID"
									   LabelID="lblCompanyBaseCuryID"
									 Style="z-index: 101; left: 126px; position: absolute; top: 63px"
									TabIndex="14" Width="90px">
									<AutoCallBack Command="Save" Enabled="True" Target="frmCompanyBody">
									</AutoCallBack>
									<GridProperties>
										<Columns>
											<px:PXGridColumn DataField="CuryID" >
												<Header Text="Cury ID">
												</Header>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Description"  Width="200px">
												<Header Text="Description">
												</Header>
											</px:PXGridColumn>
										</Columns>
										<Layout ColumnsMenu="False" />
									<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
								</px:PXSelector>
								<px:PXLabel ID="lblCompanyPhoneMask" runat="server" Style="z-index: 100; left: 9px;
									position: absolute; top: 9px">Phone Mask :</px:PXLabel>
								<px:PXTextEdit ID="edCompanyPhoneMask" runat="server" DataField="CompanyPhoneMask"
									LabelID="lblCompanyPhoneMask"  Style="z-index: 101; left: 126px;
									position: absolute; top: 9px" TabIndex="12" Width="243px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblCompanyCountryID" runat="server" Style="z-index: 102; left: 9px;
									position: absolute; top: 36px">Default Country :</px:PXLabel>
								<px:PXSelector ID="edCompanyCountryID" runat="server" DataField="CompanyCountryID"
									   HintField="description"
									HintLabelID="lblCompanyCountryIDH"  LabelID="lblCompanyCountryID"
									 Style="z-index: 103; left: 126px; position: absolute; top: 36px"
									TabIndex="13" Width="54px" AllowEdit="True">
									<GridProperties>
										<Columns>
											<px:PXGridColumn DataField="CountryID" >
												<Header Text="Country">
												</Header>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Description"  Width="200px">
												<Header Text="Country">
												</Header>
											</px:PXGridColumn>
										</Columns>
										<Layout ColumnsMenu="False" />
									<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
								</px:PXSelector>
								<px:PXLabel ID="lblCompanyCountryIDH" runat="server" Style="z-index: 104; left: 189px;
									position: absolute; top: 36px"></px:PXLabel>
							</Template>
							<ContentStyle BackColor="Transparent" BorderStyle="None">
							</ContentStyle>
						</px:PXFormView>
						<px:PXLabel ID="lblTaxRegistrationID" runat="server" Style="z-index: 103; left: 9px;
							position: absolute; top: 8px">Tax Registration ID :</px:PXLabel>
						<px:PXTextEdit ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID"
							LabelID="lblTaxRegistrationID"  Style="z-index: 104; left: 126px;
							position: absolute; top: 8px" TabIndex="11" Width="261px">
						</px:PXTextEdit>
					</px:PXPanel>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Default Location">
				<Template>
					<px:PXFormView ID="frmDefLocation" runat="server" 
						DataMember="DefLocation" DataSourceID="ds" Height="100%" Width="100%" CaptionVisible="False"
						 >
						<Template>
							<px:PXPanel ID="pnlLocationContact" runat="server" Caption="Location Info" Height="224px"
								Style="margin-top: 5px; z-index: 100; margin-left: 5px; left: 0px; position: absolute;
								top: 0px;" Width="459px">
								<px:PXFormView ID="frmDefLocationContact" runat="server" CaptionVisible="False" 
									DataMember="DefLocationContact" DataSourceID="ds" Height="100%" 
									Width="100%" Style="left: 0px; position: absolute; top: 0px" >
									<Template>
										<px:PXLabel ID="lblPhone1" runat="server" Style="z-index: 108; left: 9px; position: absolute;
											top: 144px">Phone 1 :</px:PXLabel>
										<px:PXMaskEdit ID="edPhone1" runat="server" AllowNull="True" DataField="Phone1" LabelID="lblPhone1"
											Style="z-index: 109; left: 126px; position: absolute; top: 144px" TabIndex="30"
											Width="153px">
										</px:PXMaskEdit>
										<px:PXLabel ID="lblFax" runat="server" Style="z-index: 110; left: 9px; position: absolute;
											top: 198px">Fax :</px:PXLabel>
										<px:PXMaskEdit ID="edFax" runat="server" AllowNull="True" DataField="Fax" LabelID="lblFax"
											Style="z-index: 111; left: 126px; position: absolute; top: 198px" TabIndex="32"
											Width="153px">
										</px:PXMaskEdit>
										<px:PXLabel ID="lblEMail" runat="server" Style="z-index: 112; left: 9px; position: absolute;
											top: 90px">Email :</px:PXLabel>
										<px:PXMailEdit ID="edEMail" runat="server" AllowNull="True" DataField="EMail" LabelID="lblEMail"
											 Style="z-index: 113; left: 126px; position: absolute; top: 90px"
											TabIndex="28" Width="297px" CommitChanges="True">
										</px:PXMailEdit>
										<px:PXLabel ID="lblFullName" runat="server" Style="z-index: 100; left: 9px; position: absolute;
											top: 36px">Print Name :</px:PXLabel>
										<px:PXTextEdit ID="edFullName" runat="server" AllowNull="True" DataField="FullName"
											LabelID="lblFullName"  Style="z-index: 101; left: 126px; position: absolute;
											top: 36px" TabIndex="26" Width="297px">
										</px:PXTextEdit>
										<px:PXLabel ID="lblWebSite" runat="server" Style="z-index: 100; left: 9px; position: absolute;
											top: 117px">Web :</px:PXLabel>
										<px:PXLinkEdit ID="edWebSite" runat="server" AllowNull="True" DataField="WebSite"
											LabelID="lblWebSite"  Style="z-index: 101; left: 126px; position: absolute;
											top: 117px" TabIndex="29" Width="297px" CommitChanges="True">
										</px:PXLinkEdit>
										<px:PXLabel ID="lblPhone2" runat="server" Style="z-index: 102; left: 9px; position: absolute;
											top: 171px">Phone 2 :</px:PXLabel>
										<px:PXMaskEdit ID="edPhone2" runat="server" AllowNull="True" DataField="Phone2" LabelID="lblPhone2"
											Style="z-index: 103; left: 126px; position: absolute; top: 171px" TabIndex="31"
											Width="153px">
										</px:PXMaskEdit>
										<px:PXLabel ID="lblSalutation" runat="server" Style="z-index: 100; left: 9px; position: absolute;
											top: 63px">Salutation :</px:PXLabel>
										<px:PXTextEdit ID="edSalutation" runat="server" DataField="Salutation" LabelID="lblSalutation"
											 Style="z-index: 101; left: 126px; position: absolute; top: 63px"
											TabIndex="27" Width="297px">
										</px:PXTextEdit>
									</Template>
									<ContentStyle BackColor="Transparent" BorderStyle="None">
									</ContentStyle>
								</px:PXFormView>
								<px:PXCheckBox ID="chkIsContactSameAsMain" runat="server" DataField="IsContactSameAsMain"
									Style="z-index: 100; left: 9px; position: absolute; top: 9px" TabIndex="25" Text="Same as Main"
									Width="126px">
									<AutoCallBack Command="Save" Enabled="True" Target="frmDefLocation">
									</AutoCallBack>
								</px:PXCheckBox>
							</px:PXPanel>
							<px:PXPanel ID="pnlLocationAddress" runat="server" Caption="Location Address" Height="198px"
								Width="459px" Style="margin-top: 5px; margin-left: 5px; left: 0px; position: absolute;
								top: 249px;">
								<px:PXLabel ID="lblAddressLine1" runat="server" Style="z-index: 111; left: 9px; position: absolute;
									top: 36px">Address 1 :</px:PXLabel>
								<px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" LabelID="lblAddressLine1"
									 Style="z-index: 112; left: 126px; position: absolute; top: 36px"
									TabIndex="35" Width="297px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblAddressLine2" runat="server" Style="z-index: 113; left: 9px; position: absolute;
									top: 63px">Address Line 2 :</px:PXLabel>
								<px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" LabelID="lblAddressLine2"
									 Style="z-index: 114; left: 126px; position: absolute; top: 63px"
									TabIndex="36" Width="297px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblCity" runat="server" Style="z-index: 117; left: 9px; position: absolute;
									top: 90px">City :</px:PXLabel>
								<px:PXTextEdit ID="edCity" runat="server" DataField="City" LabelID="lblCity" 
									Style="z-index: 118; left: 126px; position: absolute; top: 90px" TabIndex="37"
									Width="297px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblCountryID" runat="server" Style="z-index: 121; left: 9px; position: absolute;
									top: 117px">Country :</px:PXLabel>
								<px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" 
									  HintField="description" HintLabelID="lblCountryIDH"
									LabelID="lblCountryID"  Style="z-index: 122; left: 126px; position: absolute;
									top: 117px" TabIndex="38" Width="63px"  AllowEdit="True">
									<GridProperties>
										<Columns>
											<px:PXGridColumn DataField="CountryID" >
												<Header Text="Country ID">
												</Header>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Description"  Width="200px">
												<Header Text="Country">
												</Header>
											</px:PXGridColumn>
										</Columns>
										<Layout ColumnsMenu="False" />
									<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
									<AutoCallBack Command="Save" Enabled="True" Target="frmDefLocation">
									</AutoCallBack>
								</px:PXSelector>
								<px:PXLabel ID="lblCountryIDH" runat="server" Style="z-index: 123; left: 198px; position: absolute;
									top: 117px"></px:PXLabel>
								<px:PXLabel ID="lblPostalCode" runat="server" Style="z-index: 124; left: 9px; position: absolute;
									top: 171px">Postal Code :</px:PXLabel>
								<px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" LabelID="lblPostalCode"
									 Style="z-index: 125; left: 126px; position: absolute; top: 171px"
									TabIndex="40" Width="117px">
								</px:PXMaskEdit>
								<px:PXCheckBox ID="chkIsMain" runat="server" DataField="IsAddressSameAsMain" Style="z-index: 127;
									left: 9px; position: absolute; top: 9px" TabIndex="34" Text="Same as Main">
									<AutoCallBack Command="Save" Enabled="True" Target="frmDefLocation">
									</AutoCallBack>
								</px:PXCheckBox>
								<px:PXLabel ID="lblState" runat="server" Style="z-index: 100; left: 9px; position: absolute;
									top: 144px">State :</px:PXLabel>
								<px:PXSelector ID="edState" runat="server" DataField="State" 
									  HintField="name" HintLabelID="lblStateH"
									LabelID="lblState"  Style="z-index: 101; left: 126px; position: absolute;
									top: 144px" TabIndex="39" Width="126px" AutoRefresh="True" 
									AllowEdit="True">
									<GridProperties>
										<Columns>
											<px:PXGridColumn DataField="StateID"  Width="200px">
												<Header Text="State ID">
												</Header>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Name"  Width="120px">
												<Header Text="State Name">
												</Header>
											</px:PXGridColumn>
										</Columns>
										<Layout ColumnsMenu="False" />
									<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
									<CallBackMode PostData="Container" />
									<Parameters>
										<px:PXControlParam ControlID="frmDefLocation" Name="Address.countryID" PropertyName="DataControls[&quot;edCountryID&quot;].Value"
											Type="String" />
									</Parameters>
								</px:PXSelector>
								<px:PXLabel ID="lblStateH" runat="server" Style="z-index: 102; left: 261px; position: absolute;
									top: 144px"></px:PXLabel>
								<px:PXButton ID="btnViewDefLoactionOnMap" runat="server" CommandName="ViewDefLocationOnMap"
									CommandSourceID="ds" Style="left: 342px; position: absolute; top: 171px" Text="View on Map"
									TabIndex="41">
								</px:PXButton>
							</px:PXPanel>
							<px:PXPanel ID="pnlDefLocationInfo" runat="server" Caption="General Settings" Height="447px"
								Width="423px" Style="margin-top: 5px; margin-left: 5px; left: 468px; position: absolute;
								top: 0px;">
								<px:PXLabel ID="lblTaxRegistrationIDL" runat="server" Style="z-index: 104; left: 11px; position: absolute;
									top: 60px">Tax Registration ID :</px:PXLabel>
								<px:PXTextEdit ID="edTaxRegistrationIDL" runat="server" DataField="TaxRegistrationID" LabelID="lblTaxRegistrationIDL"
									 Style="z-index: 105; left: 128px; position: absolute; top: 60px"
									TabIndex="44" Width="234px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblVTaxZoneID" runat="server" Style="z-index: 100; left: 11px; position: absolute;
									top: 87px">Tax Zone ID :</px:PXLabel>
								<px:PXSelector ID="edVTaxZoneID" runat="server" DataField="VTaxZoneID" 
									   LabelID="lblVTaxZoneID"
									 Style="z-index: 101; left: 128px; position: absolute; top: 87px"
									TabIndex="45" Width="81px" AllowEdit="True">
									<GridProperties>
										<Columns>
											<px:PXGridColumn DataField="TaxZoneID" >
												<Header Text="Tax Zone ID">
												</Header>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="DfltTaxCategoryID" >
												<Header Text="Default Tax Category">
												</Header>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Descr"  Width="200px">
												<Header Text="Description">
												</Header>
											</px:PXGridColumn>
										</Columns>
										<Layout ColumnsMenu="False" />
									<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
								</px:PXSelector>
								<px:PXLabel ID="lblDescr" runat="server" Style="z-index: 100; left: 11px; position: absolute;
									top: 33px">Description :</px:PXLabel>
								<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" LabelID="lblDescr" 
									Style="z-index: 101; left: 128px; position: absolute; top: 33px" TabIndex="43"
									Width="234px">
								</px:PXTextEdit>
								<px:PXLabel ID="lblLocationCD" runat="server" Style="z-index: 100; left: 11px; position: absolute;
									top: 6px">Location ID:</px:PXLabel>
								<px:PXSegmentMask ID="edLocationCD" runat="server" DataField="LocationCD" 
									  LabelID="lblLocationCD" Style="z-index: 101;
									left: 128px; position: absolute; top: 6px" TabIndex="42" Width="135px" 
									AllowEdit="True">
									<GridProperties FastFilterFields="Descr">
										
										<Layout ColumnsMenu="False" />
									<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
									<Items>
										<px:PXMaskItem EditMask="AlphaNumeric" Length="6" Separator="-" TextCase="Upper" />
									</Items>
								</px:PXSegmentMask>
            		<px:PXLabel ID="lblCMPSiteID" runat="server" EnableClientScript="False" 
									style="z-index:100;position:absolute;left:11px; top:114px;">Warehouse:</px:PXLabel>
								<px:PXSegmentMask ID="edCMPSiteID" runat="server" DataField="CMPSiteID" 
									   HintField="descr" 
									HintLabelID="lblCMPSiteIDH" LabelID="lblCMPSiteID" 
									style="z-index:101;position:absolute;left:128px; top:114px;" TabIndex="10" 
									Width="81px" AllowEdit="True">
									<Items>
										<px:PXMaskItem Length="10" Separator="-" TextCase="Upper" />
									</Items>
									<GridProperties>
										
									<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
								</px:PXSegmentMask>
								<px:PXLabel ID="lblCMPSiteIDH" runat="server" EnableClientScript="False" 
									style="z-index:102;position:absolute;left:218px; top:114px;"></px:PXLabel>
								<px:PXDropDown ID="edCShipComplete" runat="server" AllowNull="False" DataField="CShipComplete"
									LabelID="lblCShipComplete " SelectedIndex="2" Style="z-index: 101; position: absolute;
									left: 128px; top: 141px;" TabIndex="160" Width="126px">
									
								</px:PXDropDown>
								<px:PXLabel ID="lblCShipComplete" runat="server" Style="z-index: 100; position: absolute;
									left: 11px; top: 141px;">Ship Complete :</px:PXLabel>								
							</px:PXPanel>
						</Template>
						<Activity Height="" HighlightColor="" SelectedColor="" Width="" />
						<ContentStyle BackColor="Transparent" BorderStyle="None">
						</ContentStyle>
						<AutoSize Enabled="True" MinHeight="500" MinWidth="400" />
					</px:PXFormView>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Company Locations">
				<Template>
					<px:PXGrid ID="grdLocations" runat="server" DataSourceID="ds" Height="100%" Width="100%"
						ActionsPosition="Top" BorderStyle="None" AllowSearch="True" BorderWidth="0px"
						TabIndex="46" SkinID="Details">
						<Levels>
							<px:PXGridLevel  DataMember="Locations">
								<RowTemplate>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="LocationID" DataType="Int32" TextAlign="Right" Width="90px"
										Visible="False" TextCase="Upper">
										<Header Text="LocationID">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" DataType="Boolean" DefValueText="True"
										TextAlign="Center" Type="CheckBox" Width="60px">
										<Header Text="Is Active">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="LocationCD" DisplayFormat="CCCCCC"  Width="90px">
										<Header Text="Location ID">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Descr"  Width="200px">
										<Header Text="Description">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="City"  Width="160px">
										<Header Text="City">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="CountryID"  RenderEditorText="True" AutoCallBack="True">
										<Header Text="Country">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="State"  Width="120px" RenderEditorText="True">
										<Header Text="State">
										</Header>
									</px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
						<Mode AllowColMoving="False" AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
						<LevelStyles>
							<RowForm Height="400px" Width="800px">
							</RowForm>
						</LevelStyles>
						<ActionBar DefaultAction="cmdViewLocation">
							<CustomItems>
								<px:PXToolBarButton Text="New Location">
								    <AutoCallBack Command="NewLocation" Enabled="True" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Location Details" Key="cmdViewLocation">
								    <AutoCallBack Command="ViewLocation" Enabled="True" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Set as Default">
									<AutoCallBack Command="SetDefault" Enabled="True" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Employees">
				<Template>
					<px:PXGrid ID="grdEmployees" runat="server" DataSourceID="ds" Height="100%" Width="100%"
						ActionsPosition="Top" NoteField="NoteText" BorderStyle="None" AllowSearch="True"
						Style="left: 0px; top: 0px" BorderWidth="0px" TabIndex="47" SkinID="Details">
						<Mode AllowColMoving="False" AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<LevelStyles>
							<RowForm Height="500px" Width="920px">
							</RowForm>
						</LevelStyles>
						<ActionBar DefaultAction="cmdViewContact">
							<CustomItems>
								<px:PXToolBarButton Text="New Employee">
									<AutoCallBack Command="NewContact" Enabled="True" Target="ds" />
								</px:PXToolBarButton>
								<px:PXToolBarButton Text="Employee Details" Key="cmdViewContact">
									<AutoCallBack Command="ViewContact" Enabled="True" Target="ds" />
								</px:PXToolBarButton>
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Employees" >
								<RowTemplate>
									<px:PXLabel ID="lblAcctCD" runat="server" Style="z-index: 100; position: absolute;
										left: 9px; top: 9px;">Employee ID :</px:PXLabel>
									<px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD" 
										  LabelID="lblAcctCD" Style="z-index: 101;
										position: absolute; left: 126px; top: 9px;" TabIndex="-1" Width="81px" AllowEdit="True">
										<Items>
											<px:PXMaskItem Length="10" Separator="-" />
										</Items>
										<GridProperties FastFilterFields="AcctName">
											
										<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
									</px:PXSegmentMask>
									<px:PXLabel ID="lblContact__DisplayName" runat="server" Style="z-index: 102; position: absolute;
										left: 9px; top: 36px;">Display Name :</px:PXLabel>
									<px:PXTextEdit ID="edContact__DisplayName" runat="server" DataField="Contact__DisplayName"
										Enabled="False" LabelID="lblContact__DisplayName"  Style="z-index: 103;
										position: absolute; left: 126px; top: 36px;" TabIndex="11" Width="500px">
									</px:PXTextEdit>
									<px:PXLabel ID="lblContact__Title" runat="server" Style="z-index: 104; position: absolute;
										left: 9px; top: 63px;">Title :</px:PXLabel>
									<px:PXDropDown ID="edContact__Title" runat="server" DataField="Contact__Title" Enabled="False"
										LabelID="lblContact__Title" Style="z-index: 105; position: absolute; left: 126px;
										top: 63px;" TabIndex="-1" Width="54px">
										
									</px:PXDropDown>
									<px:PXLabel ID="lblContact__Salutation" runat="server" Style="z-index: 106; position: absolute;
										left: 9px; top: 90px;">Salutation :</px:PXLabel>
									<px:PXTextEdit ID="edContact__Salutation" runat="server" DataField="Contact__Salutation"
										Enabled="False" LabelID="lblContact__Salutation"  Style="z-index: 107;
										position: absolute; left: 126px; top: 90px;" TabIndex="13" Width="500px">
									</px:PXTextEdit>
									<px:PXLabel ID="lblContact__EMail" runat="server" Style="z-index: 108; position: absolute;
										left: 9px; top: 117px;">Email :</px:PXLabel>
									<px:PXTextEdit ID="edContact__EMail" runat="server" DataField="Contact__EMail" Enabled="False"
										LabelID="lblContact__EMail"  Style="z-index: 109; position: absolute;
										left: 126px; top: 117px;" TabIndex="14" Width="500px">
									</px:PXTextEdit>
									<px:PXLabel ID="lblContact__WebSite" runat="server" Style="z-index: 110; position: absolute;
										left: 9px; top: 144px;">Web :</px:PXLabel>
									<px:PXTextEdit ID="edContact__WebSite" runat="server" DataField="Contact__WebSite"
										Enabled="False" LabelID="lblContact__WebSite"  Style="z-index: 111;
										position: absolute; left: 126px; top: 144px;" TabIndex="15" Width="500px">
									</px:PXTextEdit>
									<px:PXLabel ID="lblContact__FaxType" runat="server" Style="z-index: 112; position: absolute;
										left: 9px; top: 171px;">Fax Type :</px:PXLabel>
									<px:PXDropDown ID="edContact__FaxType" runat="server" AllowNull="False" DataField="Contact__FaxType"
										Enabled="False" LabelID="lblContact__FaxType" SelectedIndex="4" Style="z-index: 113;
										position: absolute; left: 126px; top: 171px;" TabIndex="-1" Width="135px">
										
									</px:PXDropDown>
									<px:PXLabel ID="lblContact__Phone1" runat="server" Style="z-index: 114; position: absolute;
										left: 9px; top: 198px;">Phone 1 :</px:PXLabel>
									<px:PXMaskEdit ID="edContact__Phone1" runat="server" DataField="Contact__Phone1"
										Enabled="False" LabelID="lblContact__Phone1" Style="z-index: 115; position: absolute;
										left: 126px; top: 198px;" TabIndex="17" Width="153px">
									</px:PXMaskEdit>
									<px:PXCheckBox ID="chkContact__IsActive" runat="server" Checked="True" DataField="Contact__IsActive"
										Enabled="False" Style="z-index: 116; position: absolute; left: 126px; top: 225px;"
										TabIndex="18" Text="Active">
									</px:PXCheckBox>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="EMPContact__IsActive"
										DataType="Boolean" DefValueText="True" TextAlign="Center" Type="CheckBox" Width="60px">
										<Header Text="Active">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="AcctCD" DisplayFormat="&gt;aaaaaaaaaa" 
										Width="80px">
										<Header Text="Employee ID">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="EMPContact__DisplayName" 
										Width="150px">
										<Header Text="Name">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="DepartmentID" DisplayFormat="&gt;aaaaaaaaaa" 
										Width="80px">
										<Header Text="Department ID">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="PositionID" DisplayFormat="&gt;aaaaaaaaaa" 
										Width="90px">
										<Header Text="Position ID">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="Address__City"  Width="80px">
										<Header Text="City">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="Address__State" DisplayFormat="&gt;aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
										 Width="80px">
										<Header Text="State">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="EMPContact__Phone1" DisplayFormat="+# (###) ###-#### Ext:####"
										 Width="120px">
										<Header Text="Phone 1">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="EMPContact__EMail" 
										Width="120px">
										<Header Text="Email">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn AllowUpdate="False" DataField="EMPContact__WebSite" 
										Width="150px">
										<Header Text="Web">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Status" DefValueText="A" >
										<Header Text="Status">
										</Header>
									</px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="533" MinWidth="600" />
	</px:PXTab>
</asp:Content>
