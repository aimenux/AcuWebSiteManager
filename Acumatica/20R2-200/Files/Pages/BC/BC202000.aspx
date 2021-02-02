<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="BC202000.aspx.cs" Inherits="Page_BC202000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
		TypeName="PX.Commerce.Core.BCEntityMaint"
		PrimaryView="EntityFilter">

	   <CallbackCommands>                           
		   <%-- PXOrderSelect commands--%>
		   <px:PXDSCallbackCommand Name="PasteLine_BCEntityImportMapping" Visible="False" CommitChanges="true" DependOnGrid="gridImportMapping" />
		   <px:PXDSCallbackCommand Name="ResetOrder_BCEntityImportMapping" Visible="False" CommitChanges="true" DependOnGrid="gridImportMapping" />
		   <px:PXDSCallbackCommand Name="PasteLine_BCEntityExportMapping" Visible="False" CommitChanges="true" DependOnGrid="gridExportMapping" />
		   <px:PXDSCallbackCommand Name="ResetOrder_BCEntityExportMapping" Visible="False" CommitChanges="true" DependOnGrid="gridExportMapping" />
		   <px:PXDSCallbackCommand Name="PasteLine_BCEntityImportFilter" Visible="False" CommitChanges="true" DependOnGrid="gridImportConditions" />
		   <px:PXDSCallbackCommand Name="ResetOrder_BCEntityImportFilter" Visible="False" CommitChanges="true" DependOnGrid="gridImportConditions" />
		   <px:PXDSCallbackCommand Name="PasteLine_BCEntityExportFilter" Visible="False" CommitChanges="true" DependOnGrid="gridExportConditions" />
		   <px:PXDSCallbackCommand Name="ResetOrder_BCEntityExportFilter" Visible="False" CommitChanges="true" DependOnGrid="gridExportConditions" />
		   <%--PXOrderSelect commands --%>
	   </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="EntityFilter" Width="100%" Height="" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule StartColumn="True" LabelsWidth="S" ControlSize="M" runat="server" ID="PXLayoutRule1" StartRow="True"></px:PXLayoutRule>
			<px:PXDropDown AllowNull="True" NullText="&lt;SELECT>" CommitChanges="True" runat="server" ID="CstPXDropDown1" DataField="ConnectorType"></px:PXDropDown>
			<px:PXSelector AutoRefresh="True" NullText="&lt;SELECT>" CommitChanges="True" runat="server" ID="CstPXSelector25" DataField="BindingID"></px:PXSelector>
			<px:PXDropDown NullText="&lt;SELECT>" AllowNull="True" CommitChanges="True" runat="server" ID="CstPXDropDown27" DataField="EntityType"></px:PXDropDown>
			<px:PXFormView RenderStyle="Simple" DataMember="CurrentEntity" runat="server" ID="PXFormView1">
				<Template>
					<px:PXLayoutRule StartColumn="True" LabelsWidth="S" ControlSize="M" runat="server" ID="PXLayoutRule1" StartRow="True"></px:PXLayoutRule>
					<px:PXCheckBox Enabled="False" runat="server" ID="CstPXCheckBox10" DataField="IsActive"></px:PXCheckBox>
				</Template>
			</px:PXFormView>

			<px:PXLayoutRule ControlSize="" LabelsWidth="SM" runat="server" ID="CstPXLayoutRule7" StartColumn="True"></px:PXLayoutRule>
			<px:PXFormView RenderStyle="Simple" DataMember="CurrentEntity" runat="server" ID="PXFormView2">
				<Template>
					<px:PXLayoutRule runat="server" ID="CstPXLayoutRule440" StartRow="True" ControlSize="SM" LabelsWidth="M"></px:PXLayoutRule>
					<px:PXDropDown CommitChanges="True" Enabled="True" runat="server" ID="CstPXDropDown11" DataField="Direction"></px:PXDropDown>
					<px:PXDropDown CommitChanges="True" Enabled="True" runat="server" ID="CstPXDropDown12" DataField="PrimarySystem"></px:PXDropDown>
					<px:PXNumberEdit Size="SM" Enabled="True" runat="server" ID="CstPXNumberEdit5" DataField="MaxAttemptCount"></px:PXNumberEdit>
					<px:PXCheckBox runat="server" ID="CstPXCheckBox42" DataField="AutoMergeDuplicates"></px:PXCheckBox>
					<px:PXCheckBox runat="server" ID="CstPXCheckBox43" DataField="ParallelProcessing" />
				</Template>
			</px:PXFormView>

			<px:PXLayoutRule ColumnWidth="" runat="server" ID="CstPXLayoutRule9" StartColumn="True" ControlSize="SM" LabelsWidth="M"></px:PXLayoutRule>
			<px:PXFormView RenderStyle="Simple" DataMember="CurrentEntity" runat="server" ID="PXFormView3">
				<Template>
					<px:PXLayoutRule runat="server" ID="CstPXLayoutRule9" StartColumn="True" ControlSize="SM" LabelsWidth="M"></px:PXLayoutRule>
					<px:PXDropDown runat="server" ID="CstPXDropDown35" DataField="ImportRealTimeStatus"></px:PXDropDown>
					<px:PXDropDown runat="server" ID="CstPXDropDown34" DataField="ExportRealTimeStatus"></px:PXDropDown>
					<px:PXDropDown runat="server" ID="CstPXCheckBox17" DataField="RealTimeMode"></px:PXDropDown>
					<px:PXTextEdit runat="server" ID="edRealTimeBaseURL" DataField="RealTimeBaseURL"></px:PXTextEdit>
				</Template>
			</px:PXFormView>
		</Template>
		<AutoSize Container="Window" Enabled="True"></AutoSize>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab LoadOnDemand="False" DataMember="CurrentEntity" ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" AllowAutoHide="True">
		<Items>
			<px:PXTabItem RepaintOnDemand="False" LoadOnDemand="False" Text="Import Mapping">
				<Template>
					<px:PXGrid ID="gridImportMapping" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AllowFilter="true" SyncPosition="true" KeepPosition="true" MatrixMode="true">
						<Mode AllowColMoving="false" InitNewRow="True" AllowUpload="True" AllowDragRows="True" />
						<CallbackCommands PasteCommand="PasteLine_BCEntityImportMapping">
						   <Save PostData="Container" />
						</CallbackCommands>
						<Levels>
							<px:PXGridLevel DataMember="ImportMappings">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM"></px:PXLayoutRule>
									<pxa:PXFormulaCombo ID="edImportSourceField" runat="server" DataField="Value" EditButton="True" IsInternalVisible="false"
										FieldsAutoRefresh="True" OnInternalFieldsNeeded="edImportSourceField_InternalFieldsNeeded" OnExternalFieldsNeeded="edImportSourceField_ExternalFieldsNeeded">
									</pxa:PXFormulaCombo>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SortOrder" TextAlign="Center" Width="54px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" Type="CheckBox" DataField="IsActive" Width="60" TextAlign="Center" AllowDragDrop="true"></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="TargetObject" Width="200px" AllowDragDrop="true"></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="TargetField" Width="200px" AllowDragDrop="true"></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="SourceObject" Width="200px" AllowDragDrop="true"></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="SourceField" EditorID="edImportSourceField" RenderEditorText="True" Width="200px" AllowDragDrop="true"></px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="true" MinHeight="450"></AutoSize>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem RepaintOnDemand="False" LoadOnDemand="False" Text="Import Filtering">
				<Template>
					<px:PXGrid ID="gridImportConditions" runat="server" DataSourceID="ds" Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab" MatrixMode="true">
						<Mode AllowColMoving="false" InitNewRow="True" AllowUpload="True" AllowDragRows="True" />
						<CallbackCommands PasteCommand="PasteLine_BCEntityImportFilter">
						   <Save PostData="Container" />
						</CallbackCommands>
						<Levels>
							<px:PXGridLevel DataMember="ImportFilters">
								<Columns>
									<px:PXGridColumn DataField="SortOrder" TextAlign="Center" Width="54px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px" CommitChanges="true" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" Width="100px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox" Width="100px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="Value" Width="200px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="Value2" Width="200px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList" Width="100px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="100px" AllowDragDrop="true" ></px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150"></AutoSize>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem RepaintOnDemand="False" LoadOnDemand="False" Text="Export Mapping">
				<Template>
					<px:PXGrid ID="gridExportMapping" runat="server" DataSourceID="ds" Width="100%" SkinID="Details" AllowFilter="true" SyncPosition="true" KeepPosition="true" MatrixMode="true">
						<Mode AllowColMoving="false" InitNewRow="True" AllowUpload="True" AllowDragRows="True" />
						<CallbackCommands PasteCommand="PasteLine_BCEntityExportMapping">
						   <Save PostData="Container" />
						</CallbackCommands>
						<Levels>
							<px:PXGridLevel DataMember="ExportMappings">
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM"></px:PXLayoutRule>
									<pxa:PXFormulaCombo ID="edExportSourceField" runat="server" DataField="Value" EditButton="True" IsExternalVisible="false"
										FieldsAutoRefresh="True" OnInternalFieldsNeeded="edExportSourceField_InternalFieldsNeeded" OnExternalFieldsNeeded="edExportSourceField_ExternalFieldsNeeded">
									</pxa:PXFormulaCombo>
								</RowTemplate>
								<Columns>
									<px:PXGridColumn DataField="SortOrder" TextAlign="Center" Width="54px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" Type="CheckBox" DataField="IsActive" Width="60" TextAlign="Center" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="TargetObject" Width="200px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="TargetField" Width="200px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="SourceObject" Width="200px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="SourceField" EditorID="edExportSourceField" RenderEditorText="True" Width="200px"></px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Container="Window" Enabled="true" MinHeight="450"></AutoSize>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem RepaintOnDemand="False" LoadOnDemand="False" Text="Export Filtering">
				<Template>
					<px:PXGrid ID="gridExportConditions" runat="server" DataSourceID="ds" Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab" MatrixMode="true">
						<Mode AllowColMoving="false" InitNewRow="True" AllowUpload="True" AllowDragRows="True" />
						<CallbackCommands PasteCommand="PasteLine_BCEntityExportFilter">
						   <Save PostData="Container" />
						</CallbackCommands>
						<Levels>
							<px:PXGridLevel DataMember="ExportFilters">
								<Columns>
									<px:PXGridColumn DataField="SortOrder" TextAlign="Center" Width="54px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px" CommitChanges="true" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" Width="100px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox" Width="100px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="Value" Width="200px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn CommitChanges="True" DataField="Value2" Width="200px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList" Width="100px" AllowDragDrop="true" ></px:PXGridColumn>
									<px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="100px" AllowDragDrop="true" ></px:PXGridColumn>
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150"></AutoSize>
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150"></AutoSize>
	</px:PXTab>
	<px:PXSmartPanel CaptionVisible="True" AutoReload="True" AutoRepaint="True" runat="server" ID="CstSmartPanel44" Caption="Clear Sync Data" Key="DeleteConfirmationPanel">
		<px:PXFormView RenderStyle="Simple" CaptionVisible="False" runat="server" ID="CstFormView45" DataMember="DeleteConfirmationPanel">
			<Template>
				<px:PXLayoutRule runat="server" ID="CstPXLayoutRule47" StartRow="True"></px:PXLayoutRule>
				<px:PXLabel runat="server" ID="CstLabel51" Text="Are you sure you want to delete all synchronization data for the entity? This operation cannot be undone."></px:PXLabel>
				<px:PXLabel runat="server" ID="CstLabel52" Text="Please enter the Entity Name to confirm."></px:PXLabel>
				<px:PXTextEdit CommitChanges="True" runat="server" ID="CstPXTextEdit46" DataField="EntityName"></px:PXTextEdit>
			</Template>
		</px:PXFormView>
		<px:PXPanel runat="server" ID="CstPanel48" RenderStyle="Simple" SkinID="Buttons">
			<px:PXButton runat="server" ID="CstButton49" DialogResult="Yes" Text="Continue"></px:PXButton>
			<px:PXButton DialogResult="Cancel" runat="server" ID="CstButton50" Text="Cancel"></px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>
	<px:PXSmartPanel CaptionVisible="True" AutoReload="True" AutoRepaint="True" runat="server" ID="PXSmartPanel2" Caption="Start Real-Time Sync" Key="StartRealTimePanel">
		<px:PXFormView RenderStyle="Simple" CaptionVisible="False" runat="server" ID="PXFormView5" DataMember="StartRealTimePanel">
			<Template>
				<px:PXLayoutRule runat="server" LabelsWidth="M" ID="CstPXLayoutRule48" StartRow="True"></px:PXLayoutRule>
				<px:PXLabel runat="server" ID="CstLabel54" Text="This webhook URL will be used to receive push notifications from the external system."></px:PXLabel>
				<px:PXLabel runat="server" ID="CstLabel55" Text="Please make sure that the URL is correct and click Continue to proceed."></px:PXLabel>
				<px:PXTextEdit CommitChanges="True" runat="server" ID="CstPXTextEdit47" DataField="RealTimeURL"></px:PXTextEdit>								
			</Template>
		</px:PXFormView>
		<px:PXPanel runat="server" ID="PXPanel2" RenderStyle="Simple" SkinID="Buttons">
			<px:PXButton runat="server" ID="CstButton51" DialogResult="Yes" Text="Continue"></px:PXButton>
			<px:PXButton DialogResult="Cancel" runat="server" ID="CstButton52" Text="Cancel"></px:PXButton>
		</px:PXPanel>
	</px:PXSmartPanel>

</asp:Content>
