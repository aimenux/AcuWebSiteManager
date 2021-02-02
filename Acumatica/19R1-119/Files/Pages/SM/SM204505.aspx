<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM204505.aspx.cs" Inherits="Page_SM204505"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" Visible="True" Width="100%" runat="server" PrimaryView="Projects" TypeName="PX.SM.ProjectList">
		<CallbackCommands>
			
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<%--<px:PXDSCallbackCommand Name="Delete" Visible="False" />--%>
			<%--<px:PXDSCallbackCommand Name="First" StartNewGroup="True" PostData="Self" />--%>
			<px:PXDSCallbackCommand Name="view" CommitChanges="True" DependOnGrid="grid" Visible="False" RepaintControls="All"/>
			<%--<px:PXDSCallbackCommand Name="import" CommitChanges="true" PopupPanel="UploadPackageDlg" />--%>
			<%--<px:PXDSCallbackCommand PopupPanel="UploadPackagePanel" Name="actionImport" />--%>
			<px:PXDSCallbackCommand Name="actionPublish" CommitChanges="true" RepaintControls="All" PopupPanel="PanelCompiler"/>
		    <px:PXDSCallbackCommand Name="actionPanelChooseProjects" Visible="False"/>
		    <px:PXDSCallbackCommand Name="actionPanelChooseProjectsAllMessages" Visible="False"/>
		    <px:PXDSCallbackCommand Name="actionPanelChooseProjectsCancel" Visible="False"/>
		    <px:PXDSCallbackCommand Name="actionPanelValidateExtOK" Visible="False"/>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">

	<px:PXGrid ID="grid" runat="server" 
		Height="400px" 
		Width="100%" 
		AllowPaging="True" 
		ActionsPosition="Top" 
		AutoAdjustColumns="True"
		AllowSearch="true" 
		SkinID="Primary" 
        DataSourceID="ds"
		SyncPosition="True"
		PageSize="50"
        BatchUpdate ="False"
        KeepPosition = "true"
        AdjustPageSize="Auto"
        >

		<Levels>
			<px:PXGridLevel DataMember="Projects">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
					<px:PXTextEdit ID="edName" runat="server" DataField="Name" />
					<px:PXCheckBox ID="chkIsWorking" runat="server" DataField="IsWorking" />
					<px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
					<px:PXSelector ID="edCreatedByID" runat="server" DataField="CreatedByID" Enabled="False"
						TextField="Username" />
					<px:PXDateTimeEdit ID="edCreatedDateTime" runat="server" DataField="CreatedDateTime"
						DisplayFormat="g" Enabled="False" />
					<px:PXSelector ID="edLastModifiedByID" runat="server" DataField="LastModifiedByID"
						Enabled="False" TextField="Username" />
					<px:PXDateTimeEdit ID="edLastModifiedDateTime" runat="server" DataField="LastModifiedDateTime"
						DisplayFormat="g" Enabled="False" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="IsWorking" Width="30px" Type="CheckBox" AllowCheckAll="True" TextAlign="Center"  AutoCallBack="True" />
					<px:PXGridColumn DataField="IsPublished" Width="60px" Type="CheckBox" TextAlign="Center" />
					<px:PXGridColumn DataField="Name" Width="108px" LinkCommand="view" />
					<px:PXGridColumn DataField="Level" Width="60px"/>
					<px:PXGridColumn DataField="ScreenNames"/>
					<px:PXGridColumn DataField="Description" Width="108px" />
					<px:PXGridColumn AllowUpdate="False" DataField="CreatedByID_Creator_Username"
						Width="108px" />
<%--					<px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" Width="90px" />
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedByID_Modifier_Username"
						Width="108px" />--%>
					<px:PXGridColumn AllowUpdate="False" DataField="LastModifiedDateTime" Width="90px" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	    <ActionBar>
			<Actions>
				<ExportExcel ToolBarVisible="False"/>
				<AdjustColumns ToolBarVisible="False"/>
			</Actions>    
		</ActionBar>  
	</px:PXGrid>
	
		<px:PXUploadFilePanel ID="UploadPackageDlg" runat="server" 
		CommandSourceID="ds"
		AllowedTypes=".zip" Caption="Open Package" PanelID="UploadPackagePanel" 
		OnUpload="uploadPanel_Upload"
		 CommandName="ReloadPage" />
    
    <script type="text/javascript">
	    function ReloadPage()
	    {
	    	window.parent.location.href = window.parent.location.href;

	    }
    </script>
	<px:PXSmartPanel ID="PanelCompiler" runat="server" 
	                 style="height:300px;width:90%;" 
	                 CaptionVisible="True" 
	                 Caption="Compilation" 

	                 RenderIFrame="True" 
	                 RenderVisible="False" 
	                 IFrameName="Compiler"
 
	                 WindowStyle="Flat" 
	                 AutoReload="True" 
	                 ShowMaximizeButton="True" 
	                 AllowMove="True" 
	                 ClientEvents-AfterHide="ReloadPage"
		
		/>
    
    
    	<px:PXSmartPanel ID="PanelPublishExt" runat="server" Caption="Publish to Multiple Tenants"  
		CaptionVisible="True"   Key="ViewCompanyList" AutoRepaint="True">
	        
        <px:PXGrid ID="GridCompanyList" runat="server" SkinID="Attributes" Width="600px" Height="200px" BatchUpdate="True" AutoAdjustColumns="True">
			<Levels>
				<px:PXGridLevel DataMember="ViewCompanyList">
					<Columns>
						<px:PXGridColumn DataField="Selected" Width="100px" Type="CheckBox" />
						<px:PXGridColumn DataField="Name" Width="300px" />
						<px:PXGridColumn DataField="ID" Width="100px" />
						<px:PXGridColumn DataField="ParentID" Width="100px" />

					</Columns>
				</px:PXGridLevel>
			</Levels>
			<Mode AllowAddNew="False" AllowDelete="False" />
		</px:PXGrid>
            

		<px:PXFormView ID="ViewPublishOptions" runat="server" DataMember="ViewPublishOptions" DataSourceID="ds" 
			 AutoRepaint="True" SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule runat="server" ColumnWidth="600px" StartColumn="True" SuppressLabel="True"/>
			
				<px:PXCheckBox ID="PublishOnlyDB" runat="server" DataField="PublishOnlyDB"  />
				<px:PXCheckBox ID="DisableOptimization" runat="server" DataField="DisableOptimization"  />
                
			</Template>
		</px:PXFormView>
	
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PXButton5" runat="server" DialogResult="OK" Text="OK" PopupPanel="PanelCompiler"></px:PXButton>
			<px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
		
    <px:PXSmartPanel ID="PanelValidateExt" runat="server" Caption="Validation Results"  ShowMaximizeButton="True" CloseButtonDialogResult="OK"
                     CaptionVisible="True"  Width="800px" Height="600px" Key="ViewValidateExtensions" AutoRepaint="True" >
    	    
        <px:PXFormView ID="PXFormView4" runat="server" DataMember="ViewValidateExtensions" DataSourceID="ds" 
                       Width="100%" AutoRepaint="True" SkinID="Transparent">
            <AutoSize Enabled="True"></AutoSize>
            <Template>
                <px:PXHtmlView runat="server" DataField="Messages" Width="100%" ID="Msg"
                               Style="white-space: pre; font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; ">
                
                    <AutoSize Enabled="True"></AutoSize>
                </px:PXHtmlView>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonValidateOK" runat="server" DialogResult="OK" Text="OK" PopupPanel="PanelValidateExt">
                <AutoCallBack Enabled="True" Target="ds" Command="actionPanelValidateExtOK"/>
            </px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="PanelChooseProjects" runat="server" Caption="Choose projects"  ShowMaximizeButton="True" 
                     CaptionVisible="True"  Width="800px" Height="600px" Key="ProjectsChooser" AutoRepaint="True">
    	    
        <px:PXGrid ID="gridChooser" runat="server" 
		Height="400px" 
		Width="100%" 
		AutoAdjustColumns="True"
		AllowSearch="true" 
        AllowPaging="False"
		SkinID="Inquire" 
        DataSourceID="ds"

        >

		<Levels>
			<px:PXGridLevel DataMember="ProjectsChooser">
				<Columns>
					<px:PXGridColumn DataField="Selected" Width="30px" Type="CheckBox" AllowCheckAll="True" TextAlign="Center" AllowUpdate="True"/>
					<px:PXGridColumn DataField="IsPublished" Width="60px" Type="CheckBox" TextAlign="Center" AllowUpdate="False" />
					<px:PXGridColumn DataField="Name" Width="108px" AllowUpdate="False"/>
					<px:PXGridColumn DataField="Description" Width="108px"  AllowUpdate="False"/>
				</Columns>
			    <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
	</px:PXGrid>
        <px:PXPanel ID="PXPanelChooseProjects" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXPanelChooseProjectsOK" runat="server" DialogResult="OK" Text="VALIDATE" PopupPanel="PanelChooseProjects">
                <AutoCallBack Enabled="True" Target="ds" Command="actionPanelChooseProjects"/>
            </px:PXButton>
            <px:PXButton ID="PXPanelChooseProjectsAllMessagesOK" runat="server" DialogResult="OK" Text="VALIDATE AND SHOW ALL MESSAGES" PopupPanel="PanelChooseProjects">
                <AutoCallBack Enabled="True" Target="ds" Command="actionPanelChooseProjectsAllMessages"/>
            </px:PXButton>
            <px:PXButton ID="PXPanelChooseProjectsCancel" runat="server" DialogResult="Cancel" Text="CANCEL" PopupPanel="PanelChooseProjects">
                <AutoCallBack Enabled="True" Target="ds" Command="actionPanelChooseProjectsCancel"/>
            </px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>

</asp:Content>
