<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM207036.aspx.cs" Inherits="Page_SM207036"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
        <script type="text/javascript">
            function commandResult(ds, context) {
                
            }
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Api.SYExportProcessSingle"
		PrimaryView="MappingsSingle">
        <ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="prepareExport" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="prepare" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="rollback" DependOnGrid="grHistory" />
			<px:PXDSCallbackCommand Name="export" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="switchActivation" Visible="False" DependOnGrid="gridPreparedData" />
			<px:PXDSCallbackCommand Name="switchActivationUntilError" Visible="False" DependOnGrid="gridPreparedData" />
			<px:PXDSCallbackCommand Name="switchProcessing" Visible="False" DependOnGrid="gridPreparedData" />
			<px:PXDSCallbackCommand Name="showUploadPanel" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="clearErrors" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXUploadDialog ID="uplDlg" runat="server" AutoSaveFile="false" Caption="Upload New Revision"
		Key="UploadPanel" Height="110px" OnFileUploadFinished="upl_FileUploaded">
	</px:PXUploadDialog>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		DataMember="MappingsSingle" Caption="Selection" FilesIndicator="True" NoteIndicator="True" >
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXSelector runat="server" DataField="Name" AutoAdjustColumns="True" ID="edName" DataSourceID="ds">
				<AutoCallBack Command="Refresh" Target="form" />
			</px:PXSelector>
			<px:PXSelector ID="edScreenID" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
			
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
			<px:PXDropDown runat="server" Enabled="False" DataField="Status" AllowNull="False" ID="edStatus" />
			<px:PXNumberEdit runat="server" Enabled="False" DataField="NbrRecords" AllowNull="False" ID="edNbrRecords" Size="M" />
			
			<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartColumn="True" SuppressLabel="True" />
			<px:PXCheckBox runat="server" ID="chkDiscardPrevious" DataField="DiscardResult" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="History">
		<Items>
			<px:PXTabItem Text="Prepared Data">
				<Template>
					<px:PXGrid ID="gridPreparedData" runat="server" DataSourceID="ds" Height="411px" FeedbackMode="DisableAll"
						Style="z-index: 100" Width="100%" AutoGenerateColumns="AppendDynamic" AllowPaging="True"
						AdjustPageSize="Auto" SkinID="DetailsInTab" AutoSaveLayout="false">
						<Mode AllowAddNew="False" AllowDelete="False" />
						<Levels>
							<px:PXGridLevel DataMember="PreparedData">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXLayoutRule runat="server" Merge="True" />
									<px:PXCheckBox ID="chkIsActive" runat="server" Checked="True" DataField="IsActive" />
									<px:PXTextEdit Size="s" ID="edErrorMessage" runat="server" DataField="ErrorMessage" />
									<px:PXLayoutRule runat="server" Merge="False" />
									<px:PXCheckBox ID="chkIsProcessed" runat="server" DataField="IsProcessed" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="MappingID" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn DataField="LineNbr" Width="50px" />
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="IsProcessed" TextAlign="Center" Type="CheckBox" Width="60px" />
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
			<px:PXTabItem Text="History">
				<Template>
					<px:PXGrid ID="grHistory" runat="server" DataSourceID="ds" Height="460px" Style="z-index: 100" FeedbackMode="DisableAll"
						Width="100%" SkinID="DetailsInTab">
						<Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
						<ActionBar>
							<Actions>
								<AddNew MenuVisible="False" ToolBarVisible="False" />
								<Delete MenuVisible="False" ToolBarVisible="False" />
								<NoteShow MenuVisible="False" ToolBarVisible="False" />
							</Actions>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="History">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<px:PXDateTimeEdit ID="edStatusDate" runat="server" DataField="StatusDate" DisplayFormat="g" EditFormat="g" Enabled="False" />
									<px:PXDropDown ID="edStatusH" runat="server" DataField="Status" Enabled="False" />
									<px:PXNumberEdit ID="edNbrRecordsH" runat="server" DataField="NbrRecords" Enabled="False" /></RowTemplate>
								<Columns>
									<px:PXGridColumn AllowUpdate="False" DataField="Ticks" Visible="false" AllowShowHide="False" />
									<px:PXGridColumn AllowUpdate="False" DataField="StatusDate" DisplayFormat="g" Width="120px" />
									<px:PXGridColumn AllowUpdate="False" DataField="Status" Width="150px" />
									<px:PXGridColumn AllowUpdate="False" DataField="NbrRecords" TextAlign="Right" Width="100px" />
									<px:PXGridColumn AllowUpdate="False" DataField="ExportTimeStamp" TextAlign="Left" Width="200px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXFormView ID="DetailsForm" runat="server" DataSourceID="ds" Width="100%" DataMember="MappingsSingleDetails">
                        <Template>
                            
                            <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Info" LabelsWidth="SM" ControlSize="M" />
                            <px:PXSelector runat="server" DataField="ProviderID" ID="edProviderID" AllowEdit="True" DataSourceID="ds" />
                            <px:PXDropDown runat="server" DataField="SyncType" AllowNull="False" ID="edSyncType" />
                            <px:PXDateTimeEdit runat="server" EditFormat="g" DisplayFormat="g" Enabled="False" DataField="PreparedOn" ID="edPreparedOn" Size="M" />
                            <px:PXDateTimeEdit runat="server" EditFormat="g" DisplayFormat="g" Enabled="False" DataField="CompletedOn" ID="edCompletedOn" Size="M" />
                            
                            <px:PXLayoutRule runat="server" StartColumn="True" StartGroup="True" GroupCaption="Options" />
                            <px:PXFormView runat="server" DataSourceID="ds" ID="OperationForm" DataMember="Operation" RenderStyle="Simple">
                                <Template>
                                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXCheckBox ID="chkSkipHeadersr" runat="server" AlignLeft="True" Checked="True" DataField="SkipHeaders" />
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
