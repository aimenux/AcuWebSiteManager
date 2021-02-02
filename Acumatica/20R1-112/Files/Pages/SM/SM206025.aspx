<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM206025.aspx.cs" Inherits="Page_SM206025"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
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
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Mappings"
		TypeName="PX.Api.SYImportMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="fillSource" Visible="False" />
			<px:PXDSCallbackCommand Name="fillDestination" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridMapping" Name="rowUp" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridMapping" Name="rowDown" Visible="False" />
			<px:PXDSCallbackCommand Name="insertFrom" Visible="False" />
			<px:PXDSCallbackCommand DependOnGrid="gridMapping" Name="rowInsert" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewScreen" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="viewSubstitutions" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXSmartPanel ID="pnlInsertFrom" runat="server" CaptionVisible="True" Caption="Choose scenario to insert steps from"
		ForeColor="Black" Style="position: static" Height="100px" Width="400px" Key="InsertFromFilter"
		DesignView="Content">
		<px:PXFormView ID="frmInsertFrom" runat="server" DataMember="InsertFromFilter" 
			DataSourceID="ds" SkinID="Transparent" Width="342px">
			<Template>
				<px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="S" 
					StartColumn="True">
				</px:PXLayoutRule>
				<px:PXSelector ID="edMappingID" runat="server" CommitChanges="True" 
					DataField="MappingID" DataSourceID="ds">
				</px:PXSelector>
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK" />
			        <px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Close" />
			    </px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
		Caption="Scenario Summary" DataMember="Mappings" FilesIndicator="True" NoteIndicator="True">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector runat="server" DataField="Name" ID="edName" AutoRefresh="True"
				AutoAdjustColumns="True" DataSourceID="ds" />
            <px:PXSelector ID="edScreenID0" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="ProviderID" 
				ID="edProviderID" DataSourceID="ds" />
			<px:PXSelector CommitChanges="True" runat="server" DataField="ProviderObject" ID="edObjectName"
				AutoRefresh="True" ValueField="Name" DataSourceID="ds" />
            <px:PXDropDown runat="server" DataField="SyncType" ID="edSyncType" AllowNull="False" />
           <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXCheckBox runat="server" Checked="True" DataField="IsActive" ID="chkIsActive" CommitChanges="True" AlignLeft="True" />
            <px:PXCheckBox ID="chkVisible" runat="server" DataField="Visible" CommitChanges="true" />
            <px:PXLayoutRule runat="server" />
            <px:PXTextEdit runat="server" DataField="SitemapTitle" ID="edSitemapTitle" />
            <px:PXSelector runat="server" DataField="WorkspaceID" ID="edWorkspaceID" DisplayMode="Text"/>
            <px:PXSelector runat="server" DataField="SubcategoryID" ID="edSubcategoryID" DisplayMode="Text"/>
            <px:PXSelector runat="server" DataField="FormatLocale" ID="edFormatLocale" 
                           TextField="CultureReadableName" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" StartColumn="True"/>
            <px:PXCheckBox runat="server" DataField="ProcessInParallel" ID="chkIsParallel" CommitChanges="True">
            </px:PXCheckBox>
            <px:PXCheckBox runat="server" DataField="BreakOnError" ID="chkBreakOnError" />
            <px:PXCheckBox runat="server" DataField="BreakOnTarget" ID="chkBreakOnTarget" />
            <px:PXCheckBox runat="server" DataField="DiscardResult" ID="chkDiscardResult" />
            
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="168%" Style="z-index: 100" Width="100%">
		<Items>
			<px:PXTabItem Text="Mapping">
				<Template>
					<px:PXGrid ID="gridMapping" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AllowPaging="False" AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab"
						AutoAdjustColumns="True" MatrixMode="true" SyncPosition="true" KeepPosition="true">
						<ActionBar>
							<Actions>
								<NoteShow MenuVisible="False" ToolBarVisible="False" />
                                <ExportExcel MenuVisible="False" ToolBarVisible="False" />
                                <Upload MenuVisible="False" ToolBarVisible="False" />
							</Actions>
							<CustomItems>
								<px:PXToolBarButton CommandName="rowInsert" CommandSourceID="ds" Text="Insert" />
							    <pxa:PXGridCombo ListItems="Show All Commands,Hide Service Commands" ParameterName="whatToShow"
									AutoCallback="True" DataMember="WhatToShowFilter" DataField="WhatToShow" Width="180px" />
								<px:PXToolBarButton CommandName="rowUp" CommandSourceID="ds" Tooltip="Move Row Up">
									<Images Normal="main@ArrowUp" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="rowDown" CommandSourceID="ds" Tooltip="Move Row Down">
									<Images Normal="main@ArrowDown" />
								</px:PXToolBarButton>
								<px:PXToolBarButton CommandName="insertFrom" CommandSourceID="ds" Text="Insert From..." />
                                <px:PXToolBarButton CommandName="viewSubstitutions" CommandSourceID="ds" Text="Substitution Lists" />
							</CustomItems>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="FieldMappings">
								<Mode InitNewRow="True" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<pxa:PXFormulaCombo ID="edValue" runat="server" DataField="Value" EditButton="True"
										FieldsAutoRefresh="True" OnExternalFieldsNeeded="edValue_ExternalFieldsNeeded"
										OnInternalFieldsNeeded="edValue_InternalFieldsNeeded" IsSubstitutionKeysVisible="True"
                                        OnSubstitutionKeysNeeded="edValue_SubstitutionKeysNeeded" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="ObjectNameHidden" Width="150px" />
									<px:PXGridColumn DataField="ObjectName" Width="150px" AutoCallBack="True" />
									<px:PXGridColumn DataField="FieldNameHidden" Width="150px" />
									<px:PXGridColumn DataField="FieldName" Width="150px" AutoCallBack="true" Type="DropDownList" />
									<px:PXGridColumn AllowNull="False" DataField="NeedCommit" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="Value" EditorID="edValue" RenderEditorText="True" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="IgnoreError" TextAlign="Center" Type="CheckBox" />
									<px:PXGridColumn DataField="LineNbr" Visible="false" AllowShowHide="False" ForceExport="true" />
									<px:PXGridColumn DataField="OrderNumber" Visible="false" AllowShowHide="False" ForceExport="true" />
									<px:PXGridColumn DataField="IsVisible" Visible="false" AllowShowHide="False" ForceExport="true" />
									<px:PXGridColumn DataField="ParentLineNbr" Visible="false" AllowShowHide="False"
										ForceExport="true" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<CallbackCommands>
							<Save PostData="Page" />
							<FetchRow PostData="Page" />
							<ExportExcel PostData="Page" />
						</CallbackCommands>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode AllowUpload="True" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Source Restrictions">
				<Template>
					<px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab" MatrixMode="true">
						<ActionBar>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="Conditions">
								<Mode InitNewRow="True" />
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px"
										AutoCallBack="true" />
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" />
									<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="Value" Width="200px" />
									<px:PXGridColumn DataField="Value2" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="60px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Target Restrictions">
				<Template>
					<px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab" MatrixMode="true">
						<ActionBar>
						</ActionBar>
						<Levels>
							<px:PXGridLevel DataMember="MatchingConditions">
								<Mode InitNewRow="True" />
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px"
										AutoCallBack="true" />
									<%--<px:PXGridColumn DataField="ObjectNameHidden" Width="150px" />
									<px:PXGridColumn DataField="ObjectName" Width="200px" Type="DropDownList" AutoCallBack="True" />--%>
									<px:PXGridColumn DataField="FieldNameHidden" Width="150px" />
									<px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									<px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" />
									<px:PXGridColumn AllowNull="False" DataField="IsRelative" TextAlign="Center" Type="CheckBox"
										Width="60px" />
									<px:PXGridColumn DataField="Value" Width="200px" />
									<px:PXGridColumn DataField="Value2" Width="200px" />
									<px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList"
										Width="60px" />
									<px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="60px" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<AutoSize Enabled="True" MinHeight="150" />
					</px:PXGrid>
				</Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
</asp:Content>
