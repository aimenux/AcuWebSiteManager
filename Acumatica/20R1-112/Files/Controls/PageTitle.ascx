<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageTitle.ascx.cs" Inherits="User_PageTitle" %>
<asp:Panel ID="pnlTitle" runat="server" CssClass="pageTitle">
	<px:PXPanel runat="server" ID="pnlTitleBottom" RenderStyle="Simple" Style="overflow:hidden;">
		<px:PXPanel runat="server" ID="pnlTBL" RenderStyle="Simple" ContentLayout-Layout="Stack" CssClass="panelTBL"
			ContentLayout-Orientation="Horizontal">
			<px:PXToolBar runat="server" ID="tlbPath" SkinID="Path" ImageSet="main" OnCallBack="tlbPath_CallBack">
				<Items>
					<px:PXToolBarButton Key="syncTOC" ImageKey="SyncTOC" ToolTip="Sync Navigation Pane" />
					<%--<px:PXToolBarLabel Text="Branch" Key="branchLabel" />--%>
					<px:PXToolBarContainer Key="branch">
						<Template>
							<px:PXDropDown runat="server" ID="cmdBranch" AllowNull="false" AutoSuggest="false" AutoPostBack="true" SelectButton="true" CallbackUpdatable="true" />
						</Template>
					</px:PXToolBarContainer>
					<%--<px:PXToolBarLabel Text=":" Key="branchSep" />--%>
					<px:PXToolBarContainer Key="title" >
						<Template>
							<a runat="server" id="lblScreenTitle" class="linkTitle">Screen Title</a>
						</Template>
					</px:PXToolBarContainer>
					<px:PXToolBarButton Key="favorites" ToolTip="Add to Favorites" ToggleMode="true">
						<Images Normal="main@FavoritesGray" /> 
						<PushedImages Normal="main@Favorites" />
						<AutoCallBack Command="AddFav" Handler="__tlbPath_FavChange" />
					</px:PXToolBarButton>
				</Items>
				<ClientEvents ButtonClick="__tlbPath_Click" MenuShow="__tlbPath_MenuShow" />
			</px:PXToolBar>
		</px:PXPanel>

		<px:PXPanel runat="server" ID="pnlTBR" RenderStyle="Simple" ContentLayout-Layout="Stack" CssClass="panelTBR" 
			ContentLayout-Orientation="Horizontal" ContentLayout-InnerSpacing="false">
			<px:PXDataViewBar runat="server" ID="tlbDataView" OnDataBound="tlbDataView_DataBound"/>
            <div id="PivotTablesContainer" runat="server" />
			<div id="CustomizationContainer" runat="server"/>
			<px:PXToolBar ID="tlbTools" runat="server" SkinID="Transparent" OnCallBack="tlbTools_CallBack" ImageSet="main">
				<Items>
					<px:PXToolBarButton Key="keyBtnRefresh" Tooltip="Click to refresh page." Text="Refresh" Visible="false" ImageKey="Refresh">
						<AutoCallBack Enabled="True" Argument="refresh" Command="refresh" />
					</px:PXToolBarButton>
					<px:PXToolBarButton Tooltip="View Help" Text="Help" Key="help"/>
				</Items>
				<Layout ItemsAlign="Right" />
				<ClientEvents Initialize="__setFileLinksEvents" />
			</px:PXToolBar>
		</px:PXPanel>
	</px:PXPanel>
</asp:Panel>

<div runat="server" id="pnlDialogs">
	<px:PXUploadDialog ID="dlgUploadXml" runat="server" DesignView="Hidden" Key="UploadImportedXml" Height="60px" Style="position: static" Width="450px" CreateOnDemand="Strict"
		Caption="Upload XML File" AutoSaveFile="false" SessionKey="XmlUploadEntity" AllowedTypes=".xml" IgnoreSize="false" RenderCheckIn="false" RenderComment="false" RenderLink="false" />

	<px:PXSmartPanel ID="pnlAbout" runat="server" CaptionVisible="True" DesignView="Hidden" ShowAfterLoad="true"
		Width="430px" Caption="About Acumatica" Overflow="Hidden" LoadOnDemand="true"	OnLoadContent="pnlAbout_LoadContent">
		<table style="width: 100%" border="0" cellpadding="3">
			<tr style="height: 60px">
				<td rowspan="3" style="width: 10px">
				</td>
				<td style="text-align: left">
					<asp:Image runat="server" ID="imgAcum" Width="20px" Height="20px" ImageUrl="~/Icons/site.png" />
					<px:PXLabel ID="lblAcumatica" runat="server" Style="font-size: 22px; font-weight: bold"
						ForeColor="#4E576A">
					</px:PXLabel><br />
					<px:PXLabel ID="lblVersion" runat="server" CssClass="labelH labelVersion">
				Version {0}
					</px:PXLabel><br />
					<px:PXLabel ID="lblUpdates" runat="server" style="font-weight: bold; white-space: nowrap; display:none">
				New version of Acumatica is available.
					</px:PXLabel>
                    <px:PXLabel ID="lblRestoredSnapshotIsUnsafe" runat="server" style="font-weight: bold; white-space: nowrap; display:none">
				        Unsafe snapshot.
					</px:PXLabel>
				</td>
			</tr>
			<tr style="height: 100px">
				<td style="vertical-align: top; padding-top: 10px">
					<px:PXLabel ID="lblCopyright1" runat="server">
				Copyright (c) 2005-2009 ProjectX, ltd. All rights reserved.
					</px:PXLabel>
					<br />
					<px:PXLabel ID="lblCopyright2" runat="server" Encode="false" />
					<br /> <br />
					<px:PXLabel ID="lblInstallationID" runat="server" Encode="false" />
				</td>
			</tr>
			<tr>
				<td style="text-align: right">
					<px:PXButton ID="btnCloseAbout" runat="server" Text="OK" Width="80px" DialogResult="OK">
					</px:PXButton>
				</td>
			</tr>
		</table>
	</px:PXSmartPanel>
	<px:PXSmartPanel ID="pnlAudit" runat="server" CaptionVisible="True" DesignView="Hidden"	Caption="Update History" Overflow="Hidden" 
		LoadOnDemand="true" OnLoadContent="pnlAudit_LoadContent" AutoReload="true">
		<px:PXLabel ID="lblWarning" runat="server" Text="Detailed information is not available for this entry." Visible="false" />
		<px:PXFormView ID="frmAudit" runat="server" SkinID="Transparent" >
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />				
				<px:PXLayoutRule ID="PXLayoutRule3" runat="server" Merge="true"/>
                <px:PXLabel ID="lblCreatedByID" runat="server" Text="Created By:" />
				<px:PXTextEdit ID="edCreatedByID" runat="server" Enabled="false" />
				<px:PXLayoutRule ID="PXLayoutRule4" runat="server"/>
				
				<px:PXLayoutRule ID="PXLayoutRule5" runat="server" Merge="true"/>
                <px:PXLabel ID="lblCreatedByScreenID" runat="server" Text="Created Through:" />
				<px:PXTextEdit ID="edCreatedByScreenID" runat="server" Enabled="false" />
				<px:PXLayoutRule ID="PXLayoutRule6" runat="server"/>

				<px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="true"/>
                <px:PXLabel ID="lblCreatedDateTime" runat="server" Text="Created On:" />
				<px:PXTextEdit ID="edCreatedDateTime" runat="server" Enabled="false"  />
				<px:PXLayoutRule ID="PXLayoutRule8" runat="server"/>

				<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
				<px:PXLayoutRule ID="PXLayoutRule9" runat="server" Merge="true"/>
                <px:PXLabel ID="lblLastModifiedByID" runat="server" Text="Last Modified By:" />
				<px:PXTextEdit ID="edLastModifiedByID" runat="server" Enabled="false"  />
				<px:PXLayoutRule ID="PXLayoutRule10" runat="server"/>

				<px:PXLayoutRule ID="PXLayoutRule11" runat="server" Merge="true"/>
                <px:PXLabel ID="lblLastModifiedByScreenID" runat="server" Text="Last Modified Through:" />
				<px:PXTextEdit ID="edLastModifiedByScreenID" runat="server" Enabled="false"  />
				<px:PXLayoutRule ID="PXLayoutRule12" runat="server"/>

				<px:PXLayoutRule ID="PXLayoutRule13" runat="server" Merge="true"/>
                <px:PXLabel ID="lblLastModifiedDateTime" runat="server" Text="Last Modified On:" />
				<px:PXTextEdit ID="edLastModifiedDateTime" runat="server" Enabled="false"  />
				<px:PXLayoutRule ID="PXLayoutRule14" runat="server"/>
			</Template>			
        </px:PXFormView>
		<px:PXPanel ID="pnlAuditButtons" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnAuditActivate" runat="server" Text="Enable Field Level Audit" Visible="False" />
			<px:PXButton ID="btnAuditClose" runat="server" DialogResult="Cancel" Text="Cancel" />						
		</px:PXPanel>
	</px:PXSmartPanel>
		<px:PXSmartPanel ID="shareColumnsDlg" runat="server" CommandName="preload" CommandSourceID="shareColumsDS"  OnLoadContent="shareColumnsDlg_LoadContent" 
		Width="635" Height="500" Key="ShareColumnsFilter" CaptionVisible="false" LoadOnDemand="true" AutoReload="true" 
		Style="display: none;" AutoCallBack-Enabled="true" AutoCallBack-Target="gridWizard" AutoCallBack-Command="WizCancel" >
		<px:PXDataSource ID="shareColumsDS" runat="server" TypeName="PX.Web.UI.SharedColumnSettings" PrimaryView="Parameters" IsDefaultDatasource="false" >
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Visible="False" Name="Done" />
		</CallbackCommands>
		</px:PXDataSource>
        <px:PXWizard ID="gridWizard" runat="server" CaptionVisible="true" SkinID="Flat"  DataMember="Parameters" DataSourceID="shareColumsDS" >
			<SaveCommand  Command="Done"  Target="shareColumsDS" />
			<NextCommand Command="Next" Target="shareColumsDS"/>
			<PrevCommand  Command="Prev" Target="shareColumsDS"/>
			<Pages>
				<px:PXWizardPage Caption="Share Column Configuration Page 1 of 2">
					<Template>
						<px:PXGrid ID="gridGrid" Caption="Table" SkinID="Primary" runat="server" DataSourceID="shareColumsDS" 
							AutoAdjustColumns="true" Width="100%"  AdjustPageSize="Auto">
							<Mode  AllowAddNew="false" AllowDelete="false" />
							<Levels>
								<px:PXGridLevel DataMember="GridList" DataKeyNames="Id" >
									<Columns>
										<px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="true" DataType="Boolean" Width="30px" AllowResize="false" />
										<px:PXGridColumn DataField="View" />
									</Columns>
									</px:PXGridLevel>
							 </Levels>
							<AutoSize  Enabled="true" MinHeight="200"/>
						</px:PXGrid>
					</Template>
				</px:PXWizardPage>
				<px:PXWizardPage Caption="Share Column Configuration Page 2 of 2">
					<Template>
						<px:PXPanel runat="server"  RenderSimple="True" RenderStyle="Simple" SkinID="Transparent">
						<px:PXLayoutRule runat="server"/>	
						<px:PXCheckBox runat="server" AlignLeft="true" ID="defaultCkbx" DataField="IsDefault" CommitChanges="true" />
						<px:PXCheckBox runat="server" AlignLeft="true" ID="overrideCkbx"  DataField="Override" CommitChanges="true" />
						<px:PXSelector ID="edRole" runat="server" DataField="RoleName" AllowAddNew="false" CommitChanges="true">
							<AutoCallBack Target="userGrid"></AutoCallBack>
						</px:PXSelector>

							</px:PXPanel>
						<px:PXGrid ID="userGrid" AutoAdjustColumns="true" Width="100%" SkinID="Details" runat="server" NoteIndicator="false" AdjustPageSize="Auto"
							 DataSourceID="shareColumsDS" FilesIndicator="false" AllowSearch="true">
							<ActionBar Position="TopAndBottom" />
							<Levels>
							<px:PXGridLevel DataMember="UserList" >
								<Columns>
									<px:PXGridColumn DataField="Included" Type="CheckBox" AllowCheckAll="true" DataType="Boolean" Width="30px" AllowResize="false" />
									<px:PXGridColumn DataField="Username" />
									<px:PXGridColumn DataField="DisplayName" />
									<px:PXGridColumn DataField="Email" />
									<px:PXGridColumn DataField="Guest" />
									<px:PXGridColumn DataField="State" />
								</Columns>
							</px:PXGridLevel>
						</Levels>	
							<Mode  AllowAddNew="false" AllowDelete="false"   />
							<AutoSize  Enabled="true" Container="Parent" />
						</px:PXGrid>

					</Template>
				</px:PXWizardPage>
			</Pages>
			<AutoSize Enabled="true" MinHeight="400" />
		</px:PXWizard>
    </px:PXSmartPanel>
    
    <px:PXSmartPanel runat="server" ID="PanelDynamicForm" Key="FilterPreview" 
                     CaptionVisible="True" 
                     DesignView="Hidden"
                     LoadOnDemand="True"
                     AutoReload="True"
                     AutoCallBack-Enabled="true" AutoCallBack-Target="FormPreview" AutoCallBack-Command="Refresh"
                     ShowAfterLoad="True"
                     Width="400"
                     OnLoadContent="PanelDynamicForm_LoadContent">
        <px:PXFormView runat="server" ID="FormPreview" DataMember="FilterPreview" Width="100%" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server"/>
                <%--                <px:PXTextEdit runat="server" ID="Subject" DataField="Subject"></px:PXTextEdit>--%>
            </Template>
           
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="PXButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>

    </px:PXSmartPanel>



</div>
