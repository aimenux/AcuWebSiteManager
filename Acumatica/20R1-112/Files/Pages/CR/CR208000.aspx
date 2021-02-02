<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="CR208000.aspx.cs" Inherits="Page_CR208000"
	Title="Untitled Page" EnableSessionState="True" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CustomerClass"
		TypeName="PX.Objects.CR.CRCustomerClassMaint">
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
		Caption="Business Account Class Summary" DataMember="CustomerClass" FilesIndicator="True"
		NoteIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edCRCustomerClassID">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M"
				ControlSize="M" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" Merge="True"/>
			<px:PXSelector ID="edCRCustomerClassID" runat="server" DataField="CRCustomerClassID"
				Size="SM" FilterByAllFields="True" />
            <px:PXCheckBox ID="chkInternal" runat="server" DataField="IsInternal"/>
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server"/>
			<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Size="XL" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="600px" Width="100%" DataMember="CustomerClassCurrent"
		ActivityField="NoteActivity" RepaintOnDemand="False" DataSourceID="ds" >
		<Items>			
			<px:PXTabItem Text="Details">
				<Template>
					<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="sm" ControlSize="XM" />
					<px:PXLayoutRule runat="server" GroupCaption="Data Entry Settings" StartGroup="True"/>
						<px:PXDropDown ID="edDefaultOwner" runat="server" DataField="DefaultOwner" CommitChanges="True"/>
						<px:PXSelector ID="edDefaultAssignmentMapID" runat="server" DataField="DefaultAssignmentMapID" AllowEdit="True" DisplayMode="Text"/>
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
                                    <px:PXGridColumn AllowNull="False" DataField="CSAttribute__ContainsPersonalData" TextAlign="Center" Type="CheckBox" Width="140px"/>
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
			<px:PXTabItem Text="Mailing Settings">
				<Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="350" SkinID="Horizontal" Height="500px">
                        <AutoSize Enabled="true" />
                        <Template1>
								<px:PXGrid ID="gridNS" runat="server" SkinID="DetailsInTab" Width="100%" Height="150px"
									Caption="Mailings" AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds" >
									<AutoCallBack Target="gridNR" Command="Refresh" />
									<AutoSize Enabled="True" />
									<Levels>
										<px:PXGridLevel DataMember="NotificationSources" DataKeyNames="SourceID,SetupID">
											<RowTemplate>
												<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
												<px:PXDropDown ID="edFormat" runat="server" DataField="Format"/>
												<px:PXSegmentMask ID="edNBranchID" runat="server" DataField="NBranchID"/> 
												<px:PXCheckBox ID="chkActive" runat="server" Checked="True" DataField="Active"/>
												<px:PXSelector ID="edSetupID" runat="server" DataField="SetupID"/>
												<px:PXSelector ID="edReportID" runat="server" DataField="ReportID" ValueField="ScreenID"/>
												<px:PXSelector ID="edNotificationID" runat="server" DataField="NotificationID" ValueField="Name"/> 
												<px:PXSelector Size="s" ID="edEMailAccountID" runat="server" DataField="EMailAccountID" DisplayMode="Text"/>
												
											</RowTemplate>
											<Columns>
												<px:PXGridColumn DataField="SetupID" AutoCallBack="True"/> 
												<px:PXGridColumn DataField="NBranchID" AutoCallBack="True" Label="Branch" />
												<px:PXGridColumn DataField="EMailAccountID" DisplayMode="Text"/>
												<px:PXGridColumn DataField="ReportID" AutoCallBack="True"/>
												<px:PXGridColumn DataField="NotificationID" AutoCallBack="True"/>	
												<px:PXGridColumn DataField="Format" RenderEditorText="True" AutoCallBack="True"/> 
												<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox"/>
												
											</Columns>
											<Layout FormViewHeight="" />
										</px:PXGridLevel>
									</Levels>
								</px:PXGrid>
                        </Template1>
                        <Template2>
								<px:PXGrid ID="gridNR" runat="server" SkinID="DetailsInTab" Width="100%" Caption="Recipients"
									AdjustPageSize="Auto" AllowPaging="True" DataSourceID="ds" >
									<Parameters>
										<px:PXSyncGridParam ControlID="gridNS" />
									</Parameters>
									<CallbackCommands>
										<Save CommitChangesIDs="gridNR" RepaintControls="None" RepaintControlsIDs="ds"/>
										<FetchRow RepaintControls="None" />
									</CallbackCommands>
									<Levels>
										<px:PXGridLevel DataMember="NotificationRecipients" DataKeyNames="NotificationID">
											<Columns>
												<px:PXGridColumn DataField="ContactType" AutoCallBack="True">
												</px:PXGridColumn>
												<px:PXGridColumn DataField="OriginalContactID" Visible="False" AllowShowHide="False" />
												<px:PXGridColumn DataField="ContactID">
													<NavigateParams>
														<px:PXControlParam Name="ContactID" ControlID="gridNR" PropertyName="DataValues[&quot;OriginalContactID&quot;]" />
													</NavigateParams>
												</px:PXGridColumn>
												<px:PXGridColumn DataField="Format" AutoCallBack="True">
												</px:PXGridColumn>
												<px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" >
												</px:PXGridColumn>
												<px:PXGridColumn DataField="Hidden" TextAlign="Center" Type="CheckBox" >
												</px:PXGridColumn>
											</Columns>
											<RowTemplate>
												<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
												<px:PXSelector ID="edContactID" runat="server" DataField="ContactID" AutoRefresh="True"
													ValueField="DisplayName" AllowEdit="True">
												</px:PXSelector>
											</RowTemplate>
											<Layout FormViewHeight="" />
										</px:PXGridLevel>
									</Levels>
									<AutoSize Enabled="True" />
								</px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
				</Template>
			</px:PXTabItem>		
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="600" MinWidth="100" />
	</px:PXTab>
</asp:Content>

