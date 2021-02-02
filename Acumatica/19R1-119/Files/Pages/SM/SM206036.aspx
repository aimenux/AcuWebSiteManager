<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM206036.aspx.cs" Inherits="Page_SM206036"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">
        function commandResult(ds, context)
        {
            if (context.command == "Save" || context.command == "Delete")
            {
                var ds = px_all[context.id];
                var isSitemapAltered = (ds.callbackResultArg == "RefreshSitemap");
                if (isSitemapAltered) __refreshMainMenu();
            }
        }
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Api.SYImportProcessSingle"
		PrimaryView="MappingsSingle">
        <ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
		    <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="prepareImport" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="prepare" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="import" />
			<px:PXDSCallbackCommand DependOnGrid="grHistory" Name="rollback" Visible="False" />
			<px:PXDSCallbackCommand Name="switchActivation" Visible="False" DependOnGrid="gridPreparedData" />
			<px:PXDSCallbackCommand Name="switchActivationUntilError" Visible="False" DependOnGrid="gridPreparedData" />
			<px:PXDSCallbackCommand Name="switchProcessing" Visible="False" DependOnGrid="gridPreparedData" />
			<px:PXDSCallbackCommand Name="showUploadPanel" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="clearErrors" Visible="False" />
            <px:PXDSCallbackCommand Name="RefreshFromFile" Visible="False" />
			<px:PXDSCallbackCommand Name="viewScreen" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
			<px:PXTreeDataMember TreeView="SiteMapTree" TreeKeys="NodeID" />
            <px:PXTreeDataMember TreeView="ImportSimpleSiteMap" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXUploadDialog ID="uplDlg" runat="server" AutoSaveFile="false" Caption="Upload New Revision"
		Key="UploadPanel" Height="110px" OnFileUploadFinished="upl_FileUploaded">
	</px:PXUploadDialog>

    <px:PXUploadDialog runat="server" ID="upDlgForFileCreation" AutoSaveFile="False" RenderCheckIn="False" Caption="Select File For Import"
        Key="CreateFromFilePanel" RenderComment="false" RenderLink="false" Width="400px" SessionKey="FileForImport" />
    
    <px:PXSmartPanel runat="server" ID="smPnlNewScenarioCreation" Caption="Provide New Scenario Properties" CaptionVisible="True"
        Key="NewScenarioProperties" Width="400px" CreateOnDemand="True" AutoCallBack-Target="frmNewScenarioCreation" AutoCallBack-Command="Refresh"
	    CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
        <px:PXFormView runat="server" ID="frmNewScenarioCreation" DataSourceID="ds" Width="100%" DataMember="NewScenarioProperties" RenderStyle="Simple">
            <CallbackCommands>
                <Save RepaintControls="None" />
                <Refresh RepaintControls="None" />
            </CallbackCommands>
            <Template>
                <px:PXLayoutRule runat="server" ID="rule1" ControlSize="M" LabelsWidth="S" />

                <px:PXTreeSelector runat="server" TreeDataSourceID="ds" ID="trNewScenarioCreation" 
                    DataField="ScreenID" PopulateOnDemand="True" ShowRootNode="False" TreeDataMember="ImportSimpleSiteMap" MinDropWidth="413">
                    <DataBindings>
                        <px:PXTreeItemBinding DataMember="ImportSimpleSiteMap" TextField="Title" ValueField="ScreenID"
                            ImageUrlField="Icon" />
                    </DataBindings>
                    <AutoCallBack Command="Save" Target="frmNewScenarioCreation">
				</AutoCallBack>
                </px:PXTreeSelector>
                <px:PXTextEdit runat="server" ID="scenarioName" DataField="Name" />
                <px:PXSelector runat="server" ID="providerType" DataField="ProviderType" TextField="Description" />
            </Template>
        </px:PXFormView>

        <px:PXPanel ID="pnlNewScenarioCreation" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOk" runat="server" DialogResult="OK" Text="OK">
                <AutoCallBack Target="form" Command="Save" />
            </px:PXButton>
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>

	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="MappingsSingle" Caption="Selection" FilesIndicator="True" NoteIndicator="True" OnDataBound="form_DataBound">
		<Template>
		    
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector runat="server" DataField="Name" AutoAdjustColumns="True"
				ID="edName" DataSourceID="ds">
				<AutoCallBack Command="Refresh" Target="form">
				</AutoCallBack>
			</px:PXSelector>
			<px:PXTreeSelector CommitChanges="True" ID="edScreenID" runat="server" DataField="ScreenID"
				PopulateOnDemand="True" ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="SiteMap"
				MinDropWidth="413">
				<DataBindings>
					<px:PXTreeItemBinding DataMember="SiteMap" TextField="Title" ValueField="ScreenID" ToolTipField="TitleWithPath"
						ImageUrlField="Icon" />
				</DataBindings>
			</px:PXTreeSelector>
			
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True">
			</px:PXLayoutRule>
			<px:PXDropDown runat="server" Enabled="False" DataField="Status" AllowNull="False"
				ID="edStatus" />
			<px:PXNumberEdit runat="server" Enabled="False" DataField="NbrRecords" AllowNull="False" ID="edNbrRecords" Size="M" />
			
            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" SuppressLabel="True" />
            <px:PXCheckBox runat="server" ID="mappingTabVisibility" DataField="IsSimpleMapping" />
            <px:PXCheckBox runat="server" ID="chkDiscardPrevious" DataField="DiscardResult" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" CheckChanges="false">
		<Items>
			<px:PXTabItem Text="Prepared Data">
				<Template>
					<px:PXGrid ID="gridPreparedData" runat="server" DataSourceID="ds" Height="411px"
						Style="z-index: 100" Width="100%" AutoGenerateColumns="AppendDynamic" AllowPaging="True"
						AdjustPageSize="Auto" SkinID="DetailsInTab" AutoSaveLayout="false">
						<Mode AllowAddNew="False" AllowDelete="False" />
						<Levels>
							<px:PXGridLevel DataMember="PreparedData">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXLayoutRule runat="server" Merge="True" />
									<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
									<px:PXTextEdit Size="s" ID="edErrorMessage" runat="server" DataField="ErrorMessage" AutoCallBack-Command="Refresh" AutoCallBack-Target="formOp1" />
									<px:PXLayoutRule runat="server" Merge="False" />
									<px:PXCheckBox ID="chkIsProcessed" runat="server" DataField="IsProcessed" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="MappingID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="LineNbr" Width="50px" />
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="IsProcessed" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="ErrorMessage" Width="100px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<ActionBar PagerVisible="False">
							<CustomItems>
								<px:PXToolBarButton Text="Switch Activation" CommandSourceID="ds" CommandName="switchActivation" />
							    <px:PXToolBarButton CommandSourceID="ds" CommandName="switchActivationUntilError" />
							    <px:PXToolBarButton Text="Switch Processing" CommandSourceID="ds" CommandName="switchProcessing" />
							    <px:PXToolBarButton Text="Clear Errors" CommandSourceID="ds" CommandName="clearErrors"
									Tooltip="Delete error messages for all the rows in prepared data." />
							</CustomItems>
						</ActionBar>
						<AutoSize MinHeight="150" Enabled="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Mapping" RepaintOnDemand="false" BindingContext="form" VisibleExp="DataControls[&quot;mappingTabVisibility&quot;].Value = 1">
                <Template>
                    <px:PXGrid runat="server" ID="mapping" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" AutoAdjustColumns="True"
                        MatrixMode="true" AllowPaging="False" SyncPosition="true" KeepPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="MappingsSimple">
								<Mode InitNewRow="True" />
                                <RowTemplate>
									<pxa:PXFormulaCombo ID="edValue" runat="server" DataField="Value" EditButton="True" FieldsAutoRefresh="True"
                                        OnExternalFieldsNeeded="edValue_ExternalFieldsNeeded" OnInternalFieldsNeeded="edValue_InternalFieldsNeeded" />
								</RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="Value" Width="250px" Type="DropDownList" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="ObjectName" Width="250px" Type="DropDownList" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="IsKey" Type="CheckBox" TextAlign="Center" />
                                    <px:PXGridColumn DataField="FieldName" Width="250px" Type="DropDownList" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton>
                                    <AutoCallBack Command="RefreshFromFile" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="History">
				<Template>
					<px:PXGrid ID="grHistory" runat="server" DataSourceID="ds" Height="460px" Style="z-index: 100"
						Width="100%" SkinID="DetailsInTab" AdjustPageSize="Auto">
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<ActionBar>
							<Actions>
								<AddNew MenuVisible="False" ToolBarVisible="False" />
								<Delete MenuVisible="False" ToolBarVisible="False" />
								<NoteShow MenuVisible="False" ToolBarVisible="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton CommandSourceID="ds" CommandName="rollback" Text="Rollback" />
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="History">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXDateTimeEdit ID="edStatusDate" runat="server" DataField="StatusDate" DisplayFormat="g"
										EditFormat="g" Enabled="False"  TimeMode="True"/>
									<px:PXDropDown ID="edStatusH" runat="server" DataField="Status" Enabled="False" />
									<px:PXNumberEdit ID="edNbrRecordsH" runat="server" DataField="NbrRecords" Enabled="False" /></RowTemplate>
								<Columns>
									<px:PXGridColumn AllowUpdate="False" DataField="Ticks" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn AllowUpdate="False" DataField="StatusDate" DisplayFormat="g" Width="120px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Status" Width="150px" />
									<px:PXGridColumn AllowUpdate="False" DataField="NbrRecords" TextAlign="Right" Width="100px" />
									<px:PXGridColumn AllowUpdate="False" DataField="ImportTimeStamp" TextAlign="Left"
										Width="200px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Description" Width="250px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXFormView ID="DetailsForm" runat="server" DataSourceID="ds" Width="100%" DataMember="MappingsSingleDetails" SkinID="Transparent">
                        <Template>
                            
                            <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Info" LabelsWidth="SM" ControlSize="M" />
                            <px:PXSelector runat="server" DataField="ProviderID" ID="edProviderID" AllowEdit="True" DataSourceID="ds" />
                            <px:PXDropDown runat="server" DataField="SyncType" AllowNull="False" ID="edSyncType" />
                            <px:PXDateTimeEdit runat="server" EditFormat="g" DisplayFormat="g" Enabled="False"
                                DataField="PreparedOn" ID="edPreparedOn" Size="M" />
                            <px:PXDateTimeEdit runat="server" EditFormat="g" DisplayFormat="g" Enabled="False"
                                DataField="CompletedOn" ID="edCompletedOn" Size="M" />
                            
                            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Site Map" />
                            <px:PXTreeSelector ID="edScreen" runat="server" DataField="SitemapParent" PopulateOnDemand="True" ShowRootNode="False"
                                TreeDataSourceID="ds" TreeDataMember="SiteMapTree" MinDropWidth="413" CommitChanges="true" AutoCallBack-Command="Refresh"
                                AutoCallBack-Target="gridPreparedData">
                                <DataBindings>
                                    <px:PXTreeItemBinding DataMember="SiteMapTree" TextField="Title" ValueField="NodeID" ImageUrlField="Icon" ToolTipField="TitleWithPath" />
                                </DataBindings>
                                <AutoCallBack Command="Save" Target="form">
                                </AutoCallBack>
                            </px:PXTreeSelector>
                            <px:PXTextEdit runat="server" DataField="SitemapTitle" ID="edSitemapTitle" />
                            
                            <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Options" />
                            <px:PXFormView runat="server" DataSourceID="ds" ID="OperationForm" DataMember="Operation" RenderStyle="Simple">
                                <Template>
                                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXCheckBox ID="chkBreakOnError" runat="server" AutoRefresh="true"
                                        Checked="True" DataField="BreakOnError" AlignLeft="True" />
                                    <px:PXCheckBox ID="chkBreakOnTarget" runat="server"
                                        Checked="True" DataField="BreakOnTarget" AlignLeft="True" />
                                    <px:PXCheckBox ID="chkValidate" runat="server" CommitChanges="True" DataField="Validate" AlignLeft="True">
                                        <AutoCallBack Command="Save" Target="DetailsForm" ActiveBehavior="True" Enabled="true">
                                            <Behavior CommitChanges="True" RepaintControls="Bound" RepaintControlsIDs="gridPreparedData" />
                                        </AutoCallBack>
                                    </px:PXCheckBox>
                                    <px:PXCheckBox ID="chkValidateAndSave" runat="server" DataField="ValidateAndSave" CommitChanges="true" AlignLeft="True" />
                                    <px:PXCheckBox ID="chkSkipHeadersr" runat="server" Checked="True" DataField="SkipHeaders" AlignLeft="True" />
                                </Template>
                            </px:PXFormView>

                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
