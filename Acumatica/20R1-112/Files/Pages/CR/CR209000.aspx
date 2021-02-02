    <%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR209000.aspx.cs" Inherits="Page_CR209000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="OpportunityClass"
		TypeName="PX.Objects.CR.CROpportunityClassMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Opportunity Class Summary" DataMember="OpportunityClass" FilesIndicator="True"
		NoteIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edCROpportunityClassID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="sm"
				ControlSize="XL" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="True"/>
			<px:PXSelector ID="edCROpportunityClassID" runat="server" DataField="CROpportunityClassID"
				Size="SM" FilterByAllFields="True" />
            <px:PXCheckBox ID="chkInternal" runat="server" DataField="IsInternal"/>
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server"/>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXCheckBox ID="chkShowContactActivities" runat="server" DataField="ShowContactActivities"/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="198px" DataSourceID="ds" DataMember="OpportunityClassProperties"
		LoadOnDemand="True">
		<Items>
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="sm" ControlSize="XM"/>
					<px:PXLayoutRule runat="server" GroupCaption="Data Entry Settings" StartGroup="True"/>
						<px:PXDropDown ID="edDefaultOwner" runat="server" DataField="DefaultOwner" CommitChanges="True"/>
						<px:PXSelector ID="edDefaultAssignmentMapID" runat="server" DataField="DefaultAssignmentMapID" AllowEdit="True" DisplayMode="Text"/>
						<px:PXSegmentMask ID="edDiscountAcctID" runat="server" DataField="DiscountAcctID"/>
						<px:PXSegmentMask ID="edDiscountSubID" runat="server" DataField="DiscountSubID" AllowEdit="true"/>
					<px:PXLayoutRule ID="PXLayoutRule3" runat="server" GroupCaption="Conversion Settings" StartGroup="True"/>
						<px:PXSelector ID="edTargetContactClassID" runat="server" DataField="TargetContactClassID"/>
						<px:PXSelector ID="edTargetBAccountClassID" runat="server" DataField="TargetBAccountClassID"/>
					<px:PXLayoutRule ID="PXLayoutRule4" runat="server" GroupCaption="Email Settings" StartGroup="True"/>
						<px:PXSelector ID="edDefaultEMailAccount" runat="server" DataField="DefaultEMailAccountID" DisplayMode="Text" />
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Attributes">
				<Template>
					<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
						border: 0px;" Width="100%" ActionsPosition="Top" SkinID="Details"  MatrixMode="True">
						<Levels>
							<px:PXGridLevel DataMember="Mapping">
								<RowTemplate>
									<px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="AttributeID" AutoCallBack="true" LinkCommand="CRAttribute_ViewDetails" />
									<px:PXGridColumn AllowNull="False" DataField="Description" />
									<px:PXGridColumn DataField="SortOrder" TextAlign="Right" SortDirection="Ascending" />
									<px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
					                <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" />
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="False" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Stages">
                <Template>
                    <px:PXGrid ID="gridStages" runat="server" DataSourceID="ds" MatrixMode="True" SkinID="Details" SyncPosition="True" Width="100%">
                        <Levels>
                            <px:PXGridLevel DataMember="OpportunityProbabilities" DataKeyNames="StageCode">
                                <RowTemplate>
                                    <px:PXMaskEdit ID="edStageCode" runat="server" AlreadyLocalized="False" DataField="StageCode" Enabled="False" DefaultLocale="">
                                    </px:PXMaskEdit>
                                    <px:PXTextEdit ID="edName" runat="server" AlreadyLocalized="False" DataField="Name" Enabled="False" DefaultLocale="">
                                    </px:PXTextEdit>
                                    <px:PXNumberEdit ID="edProbability" runat="server" AlreadyLocalized="False" DataField="Probability" Enabled="False" DefaultLocale="">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edSortOrder" runat="server" AlreadyLocalized="False" DataField="SortOrder" DefaultLocale="">
                                    </px:PXNumberEdit>
                                    <px:PXCheckBox ID="edIsActive" runat="server" AlreadyLocalized="False" DataField="IsActive" Text="Is Active">
                                    </px:PXCheckBox>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="StageCode" />
                                    <px:PXGridColumn DataField="Name" />
                                    <px:PXGridColumn DataField="Probability" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar DefaultAction="TestMethod">
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
