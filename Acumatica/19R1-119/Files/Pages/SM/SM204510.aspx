<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204510.aspx.cs" Inherits="Page_SM204510"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <label class="projectLink transparent border-box">Edit Project Items</label>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Projects"
		TypeName="PX.SM.ProjectMaintenance" PageLoadBehavior="SearchSavedKeys">
		<CallbackCommands>
			<%--<px:PXDSCallbackCommand DependOnGrid="gridVersions" Name="Cancel" />--%>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
		    
			<%--<px:PXDSCallbackCommand Name="copy" CommitChanges="true" StartNewGroup="True" />--%>
			<%--<px:PXDSCallbackCommand Name="saveAs" CommitChanges="true" Visible="False" />--%>
			<%--<px:PXDSCallbackCommand Name="viewObject" CommitChanges="true" Visible="false" DependOnGrid="grid" />--%>
			<px:PXDSCallbackCommand Name="actionFileShow" Visible="false" BlockPage="true" CommitChanges="false" DependOnGrid="grid" RepaintControls="Bound"></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionFileSave" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" CommitChangesIDs="FormEditFile"></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionEditItem" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionOpenScreen" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionFileNew" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionPublish" BlockPage="true" CommitChanges="true" RepaintControls="Bound" Visible="False" />
			<px:PXDSCallbackCommand Name="actionGenInq" Visible="false" BlockPage="true" CommitChanges="false" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionFileCheckIn" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionNextRow" Visible="false" BlockPage="false" CommitChanges="false" RepaintControls="None" RepaintControlsIDs="form,PanelSource" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionSiteMap" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionGI" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionReports" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionShowDlgAddFile" Visible="False" BlockPage="true" CommitChanges="False" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionShowDlgCheckinFiles" Visible="False" BlockPage="true" CommitChanges="False" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionRemoveConflicts" Visible="False" BlockPage="true" CommitChanges="True" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionAddCodeFile" Visible="False" BlockPage="true" CommitChanges="False" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionRefreshDbTables" Visible="False" BlockPage="true" CommitChanges="False" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="actionNewFile" Visible="False" CommitChanges="False" RepaintControls="Bound" />
			<px:PXDSCallbackCommand Name="actionNewProject" Visible="False" CommitChanges="True" RepaintControls="Bound" />
			<px:PXDSCallbackCommand Name="actionConvertControls"  BlockPage="true" CommitChanges="True" RepaintControls="All" />
		
              
            <px:PXDSCallbackCommand Name="actionUsrField" Visible="False"  BlockPage="true" CommitChanges="True" PostData="Content" RepaintControlsIDs="FormUsrField,form" />
			<px:PXDSCallbackCommand Name="actionSavePackage" Visible="False" /> 

	
	<%--			<px:PXDSCallbackCommand Name="actionTableEdit" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
--%>			<px:PXDSCallbackCommand Name="actionTableNew" Visible="false" BlockPage="true" CommitChanges="true" RepaintControls="Bound" ></px:PXDSCallbackCommand>
			
												
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" Width="100%" Caption="Selected Project" AllowAutoHide="false"
		DataMember="Projects" DataSourceID="ds" style="display: none;">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" 
				ControlSize="M" Merge="True" />
			<px:PXSelector ID="edName" runat="server" DataField="Name" 
				AutoGenerateColumns="True" DataSourceID="ds" NullText="<NEW>" Enabled="False" />
				
			<%--<px:PXButton runat="server" PopupPanel="PanelNewProject" Text="New..."></px:PXButton>
			<px:PXButton runat="server" PopupPanel="UploadPackagePanel" Text="From File..."></px:PXButton>--%>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" 
				ControlSize="M" />
			<px:PXTextEdit runat="server" DataField="Description" ID="edDescription" /></Template>
	</px:PXFormView>

	<px:PXSplitContainer runat="server" SkinID="Horizontal" ID="sp" SplitterPosition="450">
		<AutoSize Enabled="true" Container="Window" />
		<NormalStyle BorderWidth="0px" />
		<Template1>
			<px:PXGrid ID="grid" runat="server" Width="100%" Height="200px"
				SkinID="Inquire" AutoAdjustColumns="True" SyncPosition="True" FilesIndicator="True" NoteIndicator="False">
				<AutoSize Enabled="true" />
				<Mode AllowAddNew="False" />
				<ActionBar Position="None">
					<Actions>
						<NoteShow MenuVisible="False" ToolBarVisible="False" />
						<ExportExcel MenuVisible="False" ToolBarVisible="False" />
					</Actions>
					<CustomItems>
						<px:PXToolBarButton Text="Add">
							<ActionBar ToolBarVisible="External"></ActionBar>
							<MenuItems>
								<px:PXMenuItem Text="Database Table Field" PopupPanel="PanelUsrField">
								</px:PXMenuItem>
							</MenuItems>
						</px:PXToolBarButton>
					</CustomItems>
				</ActionBar>
				<Levels>
					<px:PXGridLevel DataMember="Objects">
						<RowTemplate>
							<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
							<px:PXTextEdit SuppressLabel="True" Height="100%" runat="server" ID="edSource" TextMode="MultiLine"
								DataField="Content" Font-Size="10pt" Font-Names="Courier New" Wrap="False" SelectOnFocus="False">
								<AutoSize Enabled="True" />
							</px:PXTextEdit>
						</RowTemplate>
						<Columns>
							<px:PXGridColumn DataField="Name" Width="108px" />
							<px:PXGridColumn DataField="Type" Width="108px" />
							<px:PXGridColumn DataField="Description" Width="108px" />
							<px:PXGridColumn AllowNull="False" DataField="IsDisabled" TextAlign="Center" Type="CheckBox" />
							<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username"
								Width="108px" />
							<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" Width="90px" />
							<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_Username"
								Width="108px" />
							<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedDateTime" Width="90px" />
						</Columns>
					</px:PXGridLevel>
				</Levels>
				<CallbackCommands>
					<Save RepaintControls="None" RepaintControlsIDs="grid,ds" CommitChanges="False" CommitChangesIDs="grid" />
				</CallbackCommands>
				<AutoCallBack Target="ds" Command="actionNextRow">
				</AutoCallBack>
			</px:PXGrid>
		</Template1>
		<Template2>
 			<px:PXFormView runat="server" ID="PanelSource" Caption="Source" Width="100%" DataMember="EditObject"
				OverflowY="Hidden"  AllowPaging="False"  SkinID="Inside" FilesIndicator="False" NoteIndicator="False">
				<AutoSize Enabled="True" MinHeight="150" />
				<Parameters>
					<px:PXFieldValueParam ControlID="grid" FieldName="ObjectID" Name="CustObject.objectID" />
				</Parameters>
				<Template>
					<px:PXTextEdit spellcheck='false' runat="server" ID="edSource" TextMode="MultiLine" Width="100%" 
						Height="300px" DataField="Content" Font-Size="10pt" Font-Names="Courier New" Wrap="False" 
						Style="font-family: Monospace;overflow: scroll;resize: none;border-width: 1px 0px 0px 0px" 
						SelectOnFocus="False" SuppressLabel="True">
						<AutoSize Enabled="True" />
					</px:PXTextEdit>
				</Template>
			</px:PXFormView>   
		</Template2>
	</px:PXSplitContainer>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server"> 
</asp:Content>

<asp:Content ID="dialogs" ContentPlaceHolderID="phDialogs" runat="Server">

	<px:PXSmartPanel runat="server" ID="PanelEditFile" Width="1000px" Height="400px" CaptionVisible="True"
		Caption="Edit File" AutoCallBack-Enabled="True" AutoCallBack-Target="ds" AutoCallBack-Command="actionFileShow"
		ShowMaximizeButton="True" ClientEvents-AfterShow="ResizeControls" Overflow="Hidden"
		Key="FilterFileEdit">
		<px:PXToolBar ID="PXToolBar1" runat="server" Width="100%" SkinID="Transparent">
			<Items>
				<px:PXToolBarButton Text="Save">
					<AutoCallBack Target="ds" Command="actionFileSave" />
				</px:PXToolBarButton>
			</Items>
		</px:PXToolBar>
		<px:PXFormView runat="server" ID="FormEditFile" DataMember="FilterFileEdit" Width="100%"
			Height="100%" CaptionVisible="False" Overflow="Hidden" OverflowY="Hidden">
			<AutoSize Enabled="true" />
			<Template>
				<px:PXTextEdit runat="server" ID="edContent" DataField="Content" Style="font-family: Monospace;
					overflow: scroll;" Wrap="false" Width="99%" Height="100%" TextMode="MultiLine"
					Font-Size="10pt" SelectOnFocus="False" SuppressLabel="True">
					<AutoSize Enabled="true" />
				</px:PXTextEdit>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="PanelEditTable" Width="500px" Height="400px"
		CaptionVisible="True" Caption="Edit SQL Script" ShowMaximizeButton="True" ClientEvents-AfterShow="ResizeControls"
		Overflow="Hidden" Key="FilterDbTable" AutoCallBack-Target="FormEditTable"
		AutoCallBack-Command="Refresh" DesignView="Content">
		<px:PXFormView runat="server" ID="FormEditTable" DataMember="FilterDbTable" Width="100%"
			Height="100%" CaptionVisible="False" Overflow="Hidden" OverflowY="Hidden" 
			DataSourceID="ds" TemplateContainer="" SkinID="Transparent">
			<AutoSize Enabled="true" />
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="S" 
					LabelsWidth="S">
				</px:PXLayoutRule>
				<px:PXSelector runat="server" ID="edTable" DataField="TableName" 
					AutoGenerateColumns="True" DataSourceID="ds" CommitChanges="True" />
				<px:PXLayoutRule runat="server" ControlSize="S" LabelsWidth="S" 
					StartColumn="True">
				</px:PXLayoutRule>
				<px:PXCheckBox ID="edScript" runat="server" DataField="CreateSchema" />
				<px:PXLayoutRule runat="server" StartColumn="True">
				</px:PXLayoutRule>
				<px:PXButton ID="PXButton4" runat="server" DialogResult="OK" Height="20px" 
					Text="Save" Width="79px" />
				<px:PXLayoutRule runat="server" ColumnSpan="3" StartRow="True">
				</px:PXLayoutRule>
				<px:PXLabel runat="server" ID="lblContent" Text="Custom Script"></px:PXLabel>
				<px:PXLayoutRule runat="server" ColumnSpan="3" StartRow="True" >
				</px:PXLayoutRule>
				<px:PXTextEdit ID="edContent" runat="server" DataField="CustomScript" 
					Font-Size="10pt" Height="320px" LabelID="lblContent" SelectOnFocus="False" TextMode="MultiLine" Width="470px" Wrap="False">
				</px:PXTextEdit>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="FilterSelectFile" Width="600px" Height="400px" CaptionVisible="True"
		Caption="Add Files" ShowMaximizeButton="True" Overflow="Hidden" AutoCallBack-Enabled="True" AutoCallBack-Target="ds"
		AutoCallBack-Command="actionShowDlgAddFile">
		<div style="width: 100%; padding: 10px;">
			<px:PXButton runat="server" ID="BtnAddFiles" DialogResult="OK">
				<AutoCallBack Enabled="True" Target="ds" Command="ActionFileNew">
				</AutoCallBack>
			</px:PXButton>
		</div>
		<px:PXGrid runat="server" ID="gridAddFile" DataSourceID="ds" Width="100%" BatchUpdate="True">
			<Levels>
				<px:PXGridLevel DataMember="ViewSelectFile">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox">
						</px:PXGridColumn>
						<px:PXGridColumn DataField="Path" Width="400px">
						</px:PXGridColumn>
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" Container="Parent"></AutoSize>
		</px:PXGrid>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="PanelCheckinFiles" Width="600px" Height="400px" CaptionVisible="True" Key="ViewCheckinFiles"
		Caption="Checkin Files" ShowMaximizeButton="True" Overflow="Hidden" AutoCallBack-Enabled="True" AutoCallBack-Target="ds"
		AutoCallBack-Command="actionShowDlgCheckinFiles">
		<px:PXToolBar runat="server" ID="tbr" Width="100%">
			<Items>
				<px:PXToolBarButton>
					<AutoCallBack Enabled="True" Target="ds" Command="actionFileCheckIn">
					</AutoCallBack>
				</px:PXToolBarButton>
				<px:PXToolBarButton>
					<AutoCallBack Enabled="True" Target="ds" Command="actionRemoveConflicts">
					</AutoCallBack>
				</px:PXToolBarButton>
			</Items>
		</px:PXToolBar>
		<px:PXGrid runat="server" ID="gridCheckinFiles" DataSourceID="ds" Width="100%" BatchUpdate="True" AutoAdjustColumns="True">
			<Levels>
				<px:PXGridLevel DataMember="ViewCheckinFiles">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox">
						</px:PXGridColumn>
						<px:PXGridColumn DataField="Conflict" Type="CheckBox">
						</px:PXGridColumn>
						<px:PXGridColumn DataField="Path" Width="400px">
						</px:PXGridColumn>
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" Container="Parent"></AutoSize>
		</px:PXGrid>
	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="PanelSelectReport" Width="400px" Height="80px"
		CaptionVisible="True" Caption="Select Report" Overflow="Hidden" Key="FilterSelectReport">
		<px:PXFormView runat="server" ID="PXFormView1" DataMember="FilterSelectReport" Width="100%"
			Height="100%" CaptionVisible="False" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM">
				</px:PXLayoutRule>
				<px:PXSelector runat="server" ID="edName" AutoGenerateColumns="True" DataField="Name" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="OK" />
					<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>			

			</Template>
		</px:PXFormView>

	</px:PXSmartPanel>
	<px:PXSmartPanel runat="server" ID="PanelSiteMap" Width="600px" Height="400px" CaptionVisible="True"
		Caption="Site Map" ShowMaximizeButton="True" Overflow="Hidden" Key="ViewSelectSiteMap" AutoCallBack-Target="GridSiteMap"
		AutoCallBack-Command="Refresh">
		<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Save"  Style="margin-bottom: 10px;"/>
		
		<px:PXGrid ID="GridSiteMap" runat="server" SkinID="Details" Width="100%" Height="300px" BatchUpdate="True" AutoAdjustColumns="True">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="ViewSelectSiteMap">
					<Columns>
						<px:PXGridColumn DataField="IsInProject" Width="100px" Type="CheckBox" />
						<px:PXGridColumn DataField="Name" Width="300px" />
						<px:PXGridColumn DataField="Url" Width="300px" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
			<Mode AllowAddNew="False" AllowDelete="False" />
		</px:PXGrid>
	</px:PXSmartPanel>
    
   	<px:PXSmartPanel runat="server" ID="PanelGI" Width="600px" Height="400px" CaptionVisible="True"
		Caption="Generic Inq" ShowMaximizeButton="True" Overflow="Hidden" Key="ViewSelectGI" AutoCallBack-Target="GridGI"
		AutoCallBack-Command="Refresh">
		<px:PXButton ID="GIButtonOK" runat="server" DialogResult="OK" Text="Save"  Style="margin-bottom: 10px;"/>
		
		<px:PXGrid ID="GridGI" runat="server" SkinID="Details" Width="100%" Height="300px" BatchUpdate="True" AutoAdjustColumns="True">
			<AutoSize Enabled="true" />
			<Levels>
				<px:PXGridLevel DataMember="ViewSelectGI">
					<Columns>
						<px:PXGridColumn DataField="IsInProject" Width="100px" Type="CheckBox" />
						<px:PXGridColumn DataField="Name" Width="300px" />

					</Columns>
				</px:PXGridLevel>
			</Levels>
			<Mode AllowAddNew="False" AllowDelete="False" />
		</px:PXGrid>
	</px:PXSmartPanel>
    
    
    

<%--	<script>
		function ResizeControls()
		{
			px_cm.notifyOnResize();
		}
	</script>--%>
<%--	<px:PXSmartPanel ID="pnlMakeCopy" runat="server" Caption="Create Project Copy" CaptionVisible="True"
		Key="MakeCopyDialog" DesignView="Placeholder">
		<px:PXFormView ID="formMakeCopy" runat="server" 
			SkinID="Transparent" DataMember="MakeCopyDialog" DataSourceID="ds" 
			TemplateContainer="">
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM">
				</px:PXLayoutRule>
				<px:PXTextEdit ID="edName" runat="server" DataField="Name" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="btnSave" runat="server" DialogResult="OK" Text="OK" />
				    <px:PXButton ID="btnCancelSaveAs" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>--%>
	
	
<%--	<px:PXSmartPanel ID="PanelNewFile" runat="server" Caption="Create Code File"  Key="FilterNewFile"
		CaptionVisible="True"  Width="442px">
		<px:PXFormView ID="FormNewFile" runat="server" CaptionVisible="False" DataMember="FilterNewFile"
			DataSourceID="ds"
			Width="396px" AutoRepaint="False" SkinID="Transparent">
			
		    <Template>
				<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
				<px:PXDropDown CommitChanges="True" ID="edFileTemplateName" runat="server" AllowNull="False"
					DataField="FileTemplateName" Required="True" />
				<px:PXTextEdit ID="edFileClassName" runat="server" DataField="FileClassName" Required="True" />
				<px:PXCheckBox runat="server" ID="edGenerateDac" DataField="GenerateDacMembers" />
				<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartRow="true" />
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="ButtonOk" runat="server" DialogResult="OK" Text="OK">
						<AutoCallBack Target="ds" Command="actionNewFile" />
					</px:PXButton>
					<px:PXButton ID="ButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>--%>

	<px:PXUploadFilePanel ID="UploadPackageDlg" runat="server" 
		CommandSourceID="ds"
		AllowedTypes=".zip" Caption="Open Package" PanelID="UploadPackagePanel" 
		OnUpload="uploadPanel_Upload"
		 CommandName="ReloadPage" />
		 
<%--	<px:PXSmartPanel ID="PanelCreateSolution" runat="server" Caption="Create Solution" CaptionVisible="True" Key="ViewCreateSolution">
		<px:PXFormView ID="FromCreateSolution" runat="server" 
			SkinID="Transparent" DataMember="ViewCreateSolution" DataSourceID="ds" 
			>
			<Template>
				<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="L" LabelsWidth="S"/>
				
				<px:PXTextEdit ID="edFolder" runat="server" DataField="Folder" />
				<px:PXTextEdit ID="edName" runat="server" DataField="Name" />
				
				<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
					<px:PXButton ID="PanelCreateAddonSave" runat="server" DialogResult="OK" Text="OK" />
				    <px:PXButton ID="PanelCreateAddonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
				</px:PXPanel>

			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>--%>
	
	
		<px:PXSmartPanel runat="server" ID="PanelNewProject"
		Caption="New Project" 
		CaptionVisible="True"
		AcceptButtonID="DlgNewProjectButtonOk" 
	 	CancelButtonID="DlgNewProjectButtonCancel" 
		>
	 
		<px:PXFormView runat="server" 
			
			DataMember="FilterWorkingProject" 
			SkinID="Transparent" 
			ID="FormNewProject">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule1" runat="server" ControlSize="M" />
				<px:PXTextEdit runat="server" ID="edNewProject" DataField="NewProject"></px:PXTextEdit>
				
			
			</Template>
			
			
			</px:PXFormView>
	 		<px:PXLayoutRule ID="PXLayoutRule1"  runat="server" StartRow="True" />
		<px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">
	 	<px:PXButton ID="DlgNewProjectButtonOk" runat="server"  
			
			DialogResult="OK">
			<AutoCallBack Command="actionNewProject" Target="ds" >
			</AutoCallBack>
		</px:PXButton>
		<px:PXButton ID="DlgNewProjectButtonCancel" runat="server" CausesValidation="False" DialogResult="Cancel"
			
			Text="Cancel">
		</px:PXButton>
		</px:PXPanel>
	 
	 
	 </px:PXSmartPanel>
	 
	 
	<px:PXSmartPanel ID="PanelUsrField" runat="server" Caption="Add UsrField to Database Table"  
		CaptionVisible="True"  Width="442px">
		<px:PXFormView ID="FormUsrField" runat="server" DataMember="FilterUsrField" DataSourceID="ds"
			Width="396px" AutoRepaint="False" SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="S"/>
				<px:PXSelector runat="server" ID="edTable" DataField="TableName" />
				<px:PXTextEdit ID="edFieldName" runat="server" DataField="FieldName" Required="True" />
				<px:PXDropDown ID="edFieldType" runat="server" DataField="AltFieldType" CommitChanges="True"  Required="True" />
				<px:PXNumberEdit ID="DecimalLength" runat="server" DataField="DecimalLength"  />
				<px:PXNumberEdit ID="DecimalPrecision" runat="server" DataField="DecimalPrecision"  />
				<px:PXNumberEdit ID="MaxLength" runat="server" DataField="MaxLength"  />
				

			</Template>
		</px:PXFormView>
		
		<px:PXLayoutRule runat="server" StartRow="true" />
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="ButtonOk" runat="server" DialogResult="OK" Text="OK">
				<AutoCallBack Target="ds" Command="actionUsrField" />
			</px:PXButton>
			<px:PXButton ID="ButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
    	 
	<px:PXSmartPanel ID="PanelTFS" runat="server" Caption="Source Control Binding"  
		CaptionVisible="True"  Width="442px" Key="ViewParentFolder" AutoRepaint="True">
		<px:PXFormView ID="ViewParentFolder" runat="server" DataMember="ViewParentFolder" DataSourceID="ds" 
			Width="396px" AutoRepaint="True" SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="S"/>
			
				<px:PXTextEdit ID="ParentFolder" runat="server" DataField="ParentFolder" Required="True" />
				<px:PXTextEdit ID="ProjectName" runat="server" DataField="ProjectName" Required="True" />
		       
                
			</Template>
		</px:PXFormView>
		
		<px:PXLayoutRule runat="server" StartRow="true" />
		<px:PXPanel ID="PanelTFSPXPanel1" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PanelTFSButtonOk" runat="server" DialogResult="OK" Text="OK">
		        <AutoCallBack Target="ViewParentFolder" Command="Save"/>
             </px:PXButton>
			<px:PXButton ID="PanelTFSButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
    
        	 
<%--	<px:PXSmartPanel ID="PanelTFSConfig" runat="server" Caption="Source Control Setup"  
		CaptionVisible="True"  Width="442px" Key="ViewConfig" AutoRepaint="True">
		<px:PXFormView ID="ViewConfig" runat="server" DataMember="ViewConfig" DataSourceID="ds" 
			Width="396px" AutoRepaint="True" SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="L" LabelsWidth="XS"/>
				<px:PXTextEdit ID="Config" runat="server" DataField="Config" Required="True" />
		       
                
			</Template>
		</px:PXFormView>
		
		<px:PXLayoutRule runat="server" StartRow="true" />
		<px:PXPanel ID="PanelTFSPXPanel11" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PanelTFSButtonOk1" runat="server" DialogResult="OK" Text="OK">
		        <AutoCallBack Target="ViewParentFolder" Command="Save"/>
             </px:PXButton>
			<px:PXButton ID="PanelTFSButtonCancel1" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>--%>
    
    

</asp:Content>
