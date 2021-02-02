<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR101000.aspx.cs" Inherits="Page_CR101000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Width="100%" PrimaryView="CRSetupRecord"
		TypeName="PX.Objects.CR.CRSetupMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" ContainerID="tab" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="643px" DataSourceID="ds" DataMember="CRSetupRecord"
		LoadOnDemand="True" EmailingGraph="">
		<Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
		<Items>
			<px:PXTabItem Text="General Settings">
				<Template>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXLayoutRule runat="server" GroupCaption="Numbering Sequences" StartGroup="True" />
					<px:PXSelector ID="edOpportunityNumberingID" runat="server" DataField="OpportunityNumberingID"
						AllowEdit="True" />
				    <px:PXSelector ID="edCaseNumberingID" runat="server" DataField="CaseNumberingID"
						AllowEdit="True" />
					<px:PXSelector ID="edMassMailNumberingID" runat="server" DataField="MassMailNumberingID"
						AllowEdit="True" />
					<px:PXSelector ID="edCampaignNumberingID" runat="server" DataField="CampaignNumberingID"
						AllowEdit="True" />	
					<px:PXSelector ID="edQuoteNumberingID" runat="server" DataField="QuoteNumberingID"
				                   AllowEdit="True" />	
                    				
                    <px:PXLayoutRule runat="server" GroupCaption="Data Entry Settings" StartGroup="True" />
					<px:PXSelector ID="edDefaultLeadClassID" runat="server" DataField="DefaultLeadClassID" AllowEdit="True" />
					<px:PXSelector ID="edDefaultContactClassID" runat="server" DataField="DefaultContactClassID" AllowEdit="True" />
					<px:PXSelector ID="edDefaultCustomerClassID" runat="server" DataField="DefaultCustomerClassID"
						AllowEdit="True" />	
					<px:PXSelector ID="edDefaultOpportunityClassID" runat="server" DataField="DefaultOpportunityClassID"
						AllowEdit="True" />
					<px:PXSelector ID="edDefaultCaseClassID" runat="server" DataField="DefaultCaseClassID"
						AllowEdit="True">
						<GridProperties>
							<Layout ColumnsMenu="False" />
						</GridProperties>
					</px:PXSelector>
				
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                    <px:PXLayoutRule runat="server" GroupCaption="Miscellaneous Settings" StartGroup="True"/>
					<px:PXPanel ID="PXPanel1" runat="server" RenderSimple="True" RenderStyle="Simple">
						<px:PXLayoutRule runat="server" LabelsWidth="M" ControlSize="SM" StartColumn="True"/>  
						<px:PXLayoutRule ID="PXLayoutRule2" runat="server" />
						<px:PXCheckBox ID="chkCopyNotes" runat="server" Checked="True" DataField="CopyNotes" Size="M"/>
						<px:PXCheckBox ID="chkCopyFiles" runat="server" Checked="True" DataField="CopyFiles" Size="M"/>	
						<px:PXLayoutRule ID="PXLayoutRule10" runat="server" />
						
						<px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="True" />
						<px:PXSelector ID="edDefaultRateTypeID" runat="server" AllowEdit="True" DataField="DefaultRateTypeID" Size="sM" style="margin-right: 20px;">
							<GridProperties>
								<Layout ColumnsMenu="False" />
							</GridProperties>
						</px:PXSelector>
						<px:PXCheckBox ID="chkAllowOverrideRate" runat="server" DataField="AllowOverrideRate">
						</px:PXCheckBox>
						<px:PXLayoutRule runat="server" />
						
						<px:PXLayoutRule runat="server" Merge="True" />
						<px:PXSelector ID="edDefaultCuryID" runat="server" AllowEdit="True" DataField="DefaultCuryID"
							Size="sM" style="margin-right: 20px;">
							<GridProperties>
								<Layout ColumnsMenu="False" />
							</GridProperties>
						</px:PXSelector>
						<px:PXCheckBox ID="chkAllowOverrideCury" runat="server" DataField="AllowOverrideCury">
						</px:PXCheckBox>
						<px:PXLayoutRule runat="server" />
					</px:PXPanel>

                    <px:PXLayoutRule runat="server" GroupCaption="Assignment Settings" StartGroup="True" />
					<px:PXSelector ID="edLeadDefaultAssignmentMapID" runat="server" DataField="LeadDefaultAssignmentMapID"
						TextField="Name" AllowEdit="True">
						<GridProperties>
							<Layout ColumnsMenu="False"></Layout>
						</GridProperties>
					</px:PXSelector>
					<px:PXSelector ID="edContactDefaultAssignmentMapID" runat="server" DataField="ContactDefaultAssignmentMapID"
						TextField="Name" AllowEdit="True">
						<GridProperties>
							<Layout ColumnsMenu="False"></Layout>
						</GridProperties>
					</px:PXSelector>
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector Size="xm" ID="edDefaultBAccountAssignmentMapID" runat="server" DataField="DefaultBAccountAssignmentMapID"
						TextField="Name" AllowEdit="True">
						<GridProperties>
							<Layout ColumnsMenu="False"></Layout>
						</GridProperties>
					</px:PXSelector>
					<px:PXLayoutRule runat="server" />
					<px:PXLayoutRule runat="server" Merge="True" />
					<px:PXSelector Size="xm" ID="edDefaultOpportunityAssignmentMapID" runat="server"
						DataField="DefaultOpportunityAssignmentMapID" TextField="Name" AllowEdit="True" />
					<px:PXLayoutRule runat="server" />
					<px:PXSelector ID="edDefaultCaseAssignmentMapID" runat="server" DataField="DefaultCaseAssignmentMapID"
						TextField="Name" AllowEdit="True" />


                    <px:PXLayoutRule runat="server" GroupCaption="Quote Approval Settings" StartGroup="True" />
                    <px:PXSelector ID="edQuoteApprovalMapID" runat="server" DataField="QuoteApprovalMapID" TextField="Name" AllowEdit="True" DataSourceID="ds" edit="1" />
                    <px:PXSelector ID="edQuoteApprovalNotificationID" runat="server" DataField="QuoteApprovalNotificationID" TextField="Name" AllowEdit="True"/>
					

					

				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Duplicate Validation Settings">
                <Template>
                    <px:PXPanel ID="validationForm" runat="server" Style="z-index: 100" Width="100%" CaptionVisible="False" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
                        <px:PXLayoutRule ID="PXLayoutRule7" runat="server" StartColumn="True" LabelsWidth="XL" ControlSize="XXS" />
                        <px:PXNumberEdit ID="edLeadValidationThreshold" Size="xxs" runat="server" DataField="LeadValidationThreshold" Decimals="2" ValueType="Decimal" CommitChanges="true"/>
                        <px:PXNumberEdit ID="edAccountValidationThreshold" Size="xxs" runat="server" DataField="AccountValidationThreshold" Decimals="2" ValueType="Decimal"/>
                        <px:PXNumberEdit ID="edLeadToAccountValidationThreshold" Size="xxs" runat="server" DataField="LeadToAccountValidationThreshold" Decimals="2" ValueType="Decimal"/>
                        <px:PXNumberEdit ID="edCloseLeadsWithoutActivitiesScore" Size="xxs" runat="server" DataField="CloseLeadsWithoutActivitiesScore" Decimals="2" ValueType="Decimal"/>
						<px:PXCheckBox ID="lblContactEmailUnique" runat="server" DataField="ContactEmailUnique" CommitChanges="true"/>
                        <px:PXCheckBox ID="lblValidateContactDuplicatesOnEntry" runat="server" Size="XL" DataField="ValidateContactDuplicatesOnEntry"/>
                        <px:PXCheckBox ID="lblValidateAccountDuplicatesOnEntry" runat="server" DataField="ValidateAccountDuplicatesOnEntry"/>
                    </px:PXPanel>
                        <px:PXTab runat="server" RepaintOnDemand="false" ID="validationTabs">
                            <Items>
                            <px:PXTabItem Text="Lead and Contact Validation Rules">
                                <Template>
                                    <px:PXGrid ID="gridContactValidationRules" SkinID="Details" runat="server" ActionsPosition="Top"
						                AllowSearch="True" DataSourceID="ds" Height="100px" Width="100%" BorderWidth="0px"  MatrixMode="true" FilesIndicator="false" NoteIndicator="false">
                                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" Container="Parent"/>
						                <Levels>
							                <px:PXGridLevel DataMember="LeadContactValidationRules">
								                <Columns>
									                <px:PXGridColumn DataField="MatchingField" AutoCallBack="True" />
									                <px:PXGridColumn DataField="ScoreWeight" />
									                <px:PXGridColumn DataField="TransformationRule" />
								                </Columns>
							                </px:PXGridLevel>
						                </Levels>
                                        <AutoSize Enabled="True" MinHeight="200" />
					                </px:PXGrid>
                                </Template>
                            </px:PXTabItem>
                            <px:PXTabItem Text="Lead and Account Validation Rules">
                                <Template>
                                    <px:PXGrid ID="gridLeadAccountValidationRules" SkinID="Details" runat="server" ActionsPosition="Top"
						                AllowSearch="True" DataSourceID="ds" Height="100px" Width="100%" BorderWidth="0px"  MatrixMode="true" FilesIndicator="false" NoteIndicator="false">
                                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100"/>
						                <Levels>
							                <px:PXGridLevel DataMember="LeadAccountValidationRules">
								                <Columns>
									                <px:PXGridColumn DataField="MatchingField" AutoCallBack="True" />
									                <px:PXGridColumn DataField="ScoreWeight" />
									                <px:PXGridColumn DataField="TransformationRule" />
								                </Columns>
							                </px:PXGridLevel>
						                </Levels>
                                        <AutoSize Enabled="True" MinHeight="200" />
					                </px:PXGrid>
                                </Template>
                            </px:PXTabItem>
                            <px:PXTabItem Text="Account Validation Rules">
                                <Template>
                                    <px:PXGrid ID="gridAccountValidationRules" SkinID="Details" runat="server" ActionsPosition="Top"
						                AllowSearch="True" DataSourceID="ds" Height="100px" Width="100%" BorderWidth="0px"  MatrixMode="true" FilesIndicator="false" NoteIndicator="false">
                                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100"/>
						                <Levels>
							                <px:PXGridLevel DataMember="AccountValidationRules">
								                <Columns>
									                <px:PXGridColumn DataField="MatchingField" AutoCallBack="True" />
									                <px:PXGridColumn DataField="ScoreWeight" />
									                <px:PXGridColumn DataField="TransformationRule" />
								                </Columns>
							                </px:PXGridLevel>
						                </Levels>
                                        <AutoSize Enabled="True" MinHeight="200" />
					                </px:PXGrid>
                                </Template>
                            </px:PXTabItem>
                            </Items>
                            <AutoSize Enabled="True" MinHeight="100" MinWidth="100" Container="Parent"/>
                        </px:PXTab>
                    </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Mailing Settings">
				<Template>
					<px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal" Height="500px">
						<AutoSize Enabled="true" />
						<Template1>
							<px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" Height="200px" Caption="Default Sources" AdjustPageSize="Auto" AllowPaging="True" TabIndex="300" DataSourceID="ds">
								<AutoCallBack Target="gridNR" Command="Refresh" />
								<Levels>
									<px:PXGridLevel DataMember="Notifications">
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
											<px:PXMaskEdit ID="edNotificationCD" runat="server" DataField="NotificationCD" />
											<px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name" />
											<px:PXDropDown ID="edFormat" runat="server" DataField="Format" />
											<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
											<px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID" />
											<px:PXSelector ID="edEMailAccount" runat="server" DataField="EMailAccountID" DisplayMode="Text" />
										</RowTemplate>
										<Columns>
											<px:PXGridColumn DataField="NotificationCD" />
											<px:PXGridColumn DataField="EMailAccountID" DisplayMode="Text" />
											<px:PXGridColumn DataField="ReportID" AutoCallBack="True" />
											<px:PXGridColumn DataField="NotificationID" AutoCallBack="True" />
											<px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True" />
											<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
										</Columns>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template1>
						<Template2>
							<px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" Width="100%" Caption="Default Recipients" AdjustPageSize="Auto" AllowPaging="True" TabIndex="400" DataSourceID="ds">
								<Parameters>
									<px:PXSyncGridParam ControlID="gridNS" />
								</Parameters>
								<CallbackCommands>
									<Save CommitChangesIDs="gridNR" RepaintControls="None" RepaintControlsIDs="ds" />
									<FetchRow RepaintControls="None" />
								</CallbackCommands>
								<Levels>
									<px:PXGridLevel DataMember="Recipients" DataKeyNames="RecipientID">
										<Columns>
											<px:PXGridColumn DataField="ContactType" RenderEditorText="True" AutoCallBack="True">
											</px:PXGridColumn>
											<px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
											<px:PXGridColumn DataField="ContactID">
												<NavigateParams>
													<px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
												</NavigateParams>
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True">
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox">
											</px:PXGridColumn>
											<px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox">
											</px:PXGridColumn>
										</Columns>
										<RowTemplate>
											<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
											<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True"
												ValueField="DisplayName" AllowEdit="True">
											</px:PXSelector>
										</RowTemplate>
									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>
						</Template2>
					</px:PXSplitContainer>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
