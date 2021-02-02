<%@ Control CodeFile="CustomizationDialogs.ascx.cs" Language="C#" AutoEventWireup="true" CodeBehind="CustomizationDialogs.ascx.cs"
	Inherits="Customization_CustomizationDialogs" %>

<px:PXToolBar ID="PXToolBar1" runat="server" Height="18px" SkinID="Transparent" Style="z-index: 105;
                                     left: 0px; position: static; top: 0px" ImageSet="main">
	<Items>
		<px:PXToolBarButton Text="Customization" Key="Custom" RenderMenuButton="false">
			<MenuItems>
				<px:PXMenuItem Text="Select Project..." ShowSeparator="True">
					<AutoCallBack Target="DsControlProps" Command="ActionSelectWorkingProject" />
				</px:PXMenuItem>

				<px:PXMenuItem Text="Inspect Element (Ctrl+Alt+Click)" NavigateUrl="javascript:StartCapture()">
				</px:PXMenuItem>

				<px:PXMenuItem Text="Edit Project...">
					<AutoCallBack Target="DsControlProps" Command="menuEditProj" Enabled="True" />
				</px:PXMenuItem>

				<px:PXMenuItem Text="Manage Customizations..." NavigateUrl="~/pages/sm/sm204505.aspx"
					CommandArgument="OpenManager" Target="_blank" OpenFrameset="True" />
			</MenuItems>
		</px:PXToolBarButton>
	</Items>
</px:PXToolBar>
<style type="text/css">
	.designMode .selector:hover,
	.designMode .checkBox:hover,
	.designMode .editor:hover,
	.designMode .dropDown:hover,
	.designMode .toolsBtn:hover,
	.designMode .branch-selector:hover
	{
		/*box-shadow: 0px 0px 3px 2px lightsteelblue ;*/
		/*box-shadow: 0px 0px 3px 2px cornflowerblue ;*/
		box-shadow: 0px 0px 3px 2px rgb(81,168,253);
		cursor: help !important;
	}
	.designMode .branch-selector:hover input,
	.designMode .branch-selector:hover button
	{
				cursor: help !important;
	}

	.designMode .controlCont:hover,
	.designMode .checkBox:hover,
	.designMode .editor:hover,
	.designMode .dropDown:hover,
	.designMode .toolsBtn:hover
	{
		border-color: navy;
	}

	.designMode .GridHeader:hover
	{
		/*box-shadow: 0px 0px 3px 2px deepskyblue inset;*/
		box-shadow: 0px 0px 3px 2px rgb(81,168,253) inset;
		/*box-shadow: 0px 0px 3px 2px lightsteelblue inset;*/
		cursor: help !important;
	}

	.designMode,
	.designMode .toolBtnNormal:hover,
	.designMode .toolBtnDisabled:hover,
	.designMode label:hover,
	.designMode .GridMain:hover,
	.designMode .FormView:hover
	{
		cursor: help !important;
	}

</style>
<script type="text/javascript">

	function MonitorHotKey(e)
	{
		if (IsCaptureMode)
			return;

		if (e.ctrlKey && e.altKey && e.location == 1)
		{
			StartCapture();
		}
	}
	if (window['__px'] && __px(this)) __px(this).addEventHandler(document.documentElement, "keydown", MonitorHotKey, true);

    var IsCaptureMode = false;
    function StartCapture(e)
    {
    	if (IsCaptureMode)
    		return;
    	if (!window['__px'] || !__px(this)) return;

    	IsCaptureMode = true;
    	document.activeElement.blur();
    	document.body.className = "designMode";

    	var h = function EndCapture(e)
    	{
    		if (e.type == "mousedown") try { HandleClick(e); } catch (ex) { }
    		if (e.type == "contextmenu") { __px(this).cancelEvent(e, false); return; }

    		if (e.type == "keydown"
                && e.ctrlKey
                && e.altKey
                && (e.keyCode == 18 || e.keyCode == 17)) //alt || ctrl
    		    return;

    		IsCaptureMode = false;

    		__px(this).removeEventHandler(px.IsIE11 ? window.top : window, "blur", h, true);
    		__px(this).removeEventHandler(document.documentElement, "mousedown", h, true);
    		__px(this).removeEventHandler(document.documentElement, "keydown", h, true);
    		__px(this).removeEventHandler(document.documentElement, "keyup", h, true);

    		var me = this;
    		setTimeout(function ()
    		{
    			__px(me).removeEventHandler(document.documentElement, "contextmenu", h, true);
    		}, 10);
    		document.body.className = "";
    		__px(this).cancelEvent(e);
    	};

    	setTimeout(function ()
    	{
    	    __px(this).addEventHandler(px.IsIE11 ? window.top : window, "blur", h, true);
    	}, 0);
    	__px(this).addEventHandler(document.documentElement, "mousedown", h, true);
    	__px(this).addEventHandler(document.documentElement, "keydown", h, true);
    	__px(this).addEventHandler(document.documentElement, "keyup", h, true);
    	__px(this).addEventHandler(document.documentElement, "contextmenu", h, true);
    }

    function HandleClick(e)
    {
    	var targetId = "";
    	var dataCmd = "";
    	var dataField = "";
	    
    	for (var target = e.target;	target != null; target = target.parentNode)
    	{
    		if (target.nodeName == "LABEL")
    		{
    			targetId = target.getAttribute("for");
    			if (targetId != "" && targetId)
    				break;
    		}

    		var cmd = target.getAttribute("data-cmd");
    		if (cmd && cmd != "")
    			dataCmd = cmd;

    		if (target.id != "" && target.id != null)
    		{
    			targetId = target.id;
    			break;
    		}
    	}

    	if (targetId)
    	{
    		var gridColSep = targetId.indexOf("_colHS_0_");
    		if (gridColSep > 0)
    		{
    			var gridId = targetId.substring(0, gridColSep);
    			var colIndex = parseInt(targetId.substr(gridColSep + 9));
    			var grid = document.getElementById(gridId).object;
    			var col = grid.levels[0].getColumn(colIndex);
    			dataField = col.dataField;

    		}

    		if (dataCmd)
    			targetId += "|dataCmd=" + dataCmd;
    		
    		if (dataField)
    			targetId += "|dataField=" + dataField;
    		
    		SendCallback(targetId);
    		//var url = "<%=VirtualPathUtility.ToAbsolute("~/Pages/SM/SM204530.aspx")%>?clientID=" + targetId;
    		//__px(this).openUrl(url, "ed", false, true);
    		return true;
			}
			return false;
		}

		function SendCallback(id)
		{
			var hdn = document.getElementById("NavigateClientID");
			hdn.value = id;
			var btn = __px_alls(this).NavigateAspxHandler;
			btn.exec({});
		}

</script>
<input type="hidden" name="__CustomizationContext" id="__CustomizationContext" />
<input type="hidden" name="__NavigateClientID" id="NavigateClientID" />
<px:PXButton ID="NavigateAspxHandler" runat="server"
	Style="z-index: 100; left: 270px; position: absolute; top: 0px; visibility: hidden;"
	Text="NavigateHandler-Do not remove!">
	<AutoCallBack Enabled="True" Target="DsControlProps" Command="actionNavigateAspx">
	</AutoCallBack>
</px:PXButton>


<px:PXSmartPanel runat="server" ID="DlgNewProject"
		Caption="New Project" 
		CaptionVisible="True"
		AcceptButtonID="DlgNewProjectButtonOk" 
	 	CancelButtonID="DlgNewProjectButtonCancel" 
    AutoRepaint="True"
		>
	 
		<px:PXFormView runat="server" 
			DataSourceID="DsControlProps"
			DataMember="FilterWorkingProject" 
			SkinID="Transparent" 
			ID="FormNewProject">
			<Template>
				<px:PXLayoutRule runat="server" ControlSize="M" />
				<px:PXTextEdit runat="server" ID="edNewProject" DataField="NewProject" CommitChanges="True" />
			</Template>
		</px:PXFormView>

	 		<px:PXLayoutRule ID="PXLayoutRule1"  runat="server" StartRow="True" />
		<px:PXPanel ID="PXPanelBtn" runat="server" SkinID="Buttons">
		    <px:PXButton ID="DlgNewProjectButtonOk" runat="server" Text="OK" DialogResult="OK">
		        <AutoCallBack Enabled="True" Target="DsControlProps" Command="actionCreateNewProject"/>
                </px:PXButton>
		    <px:PXButton ID="DlgNewProjectButtonCancel" runat="server" Text="Cancel" CausesValidation="False" DialogResult="Cancel"/>
		
		</px:PXPanel>
	  
 </px:PXSmartPanel>

<px:PXSmartPanel ID="ComboBoxValuesDictDialog" runat="server" Style="z-index: 108;left: 351px; position: absolute; top: 99px" 
                    Width="550px" Caption="Drop Down Values" CaptionVisible="true" 
	 Key="ComboBoxValues"				
	LoadOnDemand="true" 
	AutoCallBack-Target="gridCombos"
	AutoCallBack-Enabled="true"
		
	AutoReload="True"
    AutoRepaint="True"
    
                   >
    <div style="padding: 5px">
        <px:PXGrid ID="gridCombos" runat="server" DataSourceID="DsControlProps" Style="z-index: 100" SkinID="Details" AutoAdjustColumns="True" 
                    Width="100%">
            <AutoSize Enabled="True" MinHeight="243"></AutoSize>
            <ActionBar>
                <Actions>
                    <Refresh ToolBarVisible="False"></Refresh>
                    <ExportExcel ToolBarVisible="False"></ExportExcel>
                    <AdjustColumns ToolBarVisible="False"></AdjustColumns>
                    <AddNew ToolBarVisible="False"></AddNew>
                    <Delete ToolBarVisible="False"></Delete>
                </Actions>
            </ActionBar>
            <Levels>
                <px:PXGridLevel DataMember="ComboBoxValues">					
                    <Columns>
                        <px:PXGridColumn DataField="Value" Width="20px"/>
                        <px:PXGridColumn DataField="Description" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
        </px:PXGrid>
    </div>
    <px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
        <px:PXButton ID="PXButton5" runat="server" Text="Close" DialogResult="Cancel" />
    </px:PXPanel>
</px:PXSmartPanel>

<px:PXSmartPanel ID="WizardSelectProject" runat="server"
    Caption="Select Customization Project"
    CaptionVisible="True"
    Key="FilterWorkingProject"
		Overflow="Hidden"
    HideAfterAction="True"
    AutoReload="True"
    AutoRepaint="True"
    AcceptButtonID="SelectProjectOk"
    CancelButtonID="SelectProjectCancel">

	<px:PXFormView ID="FormSelectProject" runat="server" DataMember="FilterWorkingProject"
		DataSourceID="DsControlProps" AutoRepaint="False" SkinID="Transparent">

		<Template>
			<px:PXLayoutRule ID="PXLayoutRule6" runat="server" Merge="True" ControlSize="M" />
			<px:PXSelector ID="edWP" runat="server"
				AutoRefresh="True"
				DataField="Name"
				ValueField="Name" AutoGenerateColumns="True" AutoAdjustColumns="True" CommitChanges="True">
			</px:PXSelector>
		</Template>
	</px:PXFormView>

	<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartRow="True" />
	<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">

		<px:PXButton ID="SelectProjectOk" runat="server" DialogResult="OK" Text="OK" />
		<px:PXButton ID="SelectProjectCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		<px:PXButton runat="server" PopupPanel="DlgNewProject" Text="New..." ID="BtnNewProject" />
<%--		<px:PXButton ID="PXButton2" runat="server" Text="Manage..." NavigateUrl="~/pages/sm/sm204505.aspx"
			Target="_blank" OpenFrameset="True" />--%>
	</px:PXPanel>
</px:PXSmartPanel>
	

    <iframe name="VisualStudioFrame" src="about:blank" style="display: none"></iframe>

    <style type="text/css">
        .SmartPanelM {
            background-color: white!important;
            border-color: white!important;
        }

        


    </style>
	<px:PXSmartPanel ID="PanelElemInfo" runat="server"
		Caption="Element Properties"
		CaptionVisible="True"
		ShowAfterLoad="true" AutoRepaint="True"
		Key="ViewElemInfo" Overflow="hidden">
		
		<px:PXFormView ID="FormElemInfo" runat="server" DataMember="ViewElemInfo" AllowCollapse="False"
			DataSourceID="DsControlProps" SkinID="Transparent">
			<Template>
				<px:PXLayoutRule runat="server" ControlSize="SM" LabelsWidth="S" Merge="True" />
				
				<px:PXTextEdit ID="edAspxControl" runat="server" DataField="AspxControl" Enabled="False"  />
				
				<%--Group box is only used to wrap the button, in order to show/hide the button based on 'IsComboBox' field.
					PXButton control does not allow this.--%>
				<px:PXGroupBox ID="panelPXBUtton1" runat="server" DataField="IsComboBox" RenderStyle="Simple">
					<Template>
						<px:PXButton ID="PXButton1" runat="server" DialogResult="None" Text="Drop Down Values" PopupPanel="ComboBoxValuesDictDialog" />
					</Template>
				</px:PXGroupBox>

				<px:PXLayoutRule runat="server" Merge="False"/>

				<px:PXTextEdit ID="CacheType" runat="server" DataField="CacheType" Enabled="False" />
				<px:PXTextEdit ID="FieldName" runat="server" DataField="FieldName" Enabled="False" />
                <px:PXTextEdit ID="ViewName" runat="server" DataField="ViewName" Enabled="False" />
				<px:PXTextEdit ID="GraphName" runat="server" DataField="GraphName" Enabled="False" />
                <px:PXTextEdit ID="ActionName" runat="server" DataField="ActionName" Enabled="False" />
				<%--<px:PXTextEdit ID="WorkingProject" runat="server" Enabled="False" DataField="WorkingProject" />--%>
			</Template>

		</px:PXFormView>

		<px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartRow="True" />
    <px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons" ContentLayout-LabelsWidth="0" ContentLayout-ContentAlign="Left" >



			<px:PXButton ID="PXButton3" runat="server" DialogResult="None" Text="Customize" Width="90px" >
				<AutoCallBack Target="DsControlProps" Command="menuLayoutEditor"/>
			</px:PXButton>

			<px:PXButton ID="ButtonGraphActions" runat="server" Text="Actions" Width="90px" RenderAsButton="false">
				<MenuItems>

					<px:PXMenuItem Text="Customize Business Logic...">
						<AutoCallBack Target="DsControlProps" Command="MenuGraphEditor" Enabled="True"></AutoCallBack>
					</px:PXMenuItem>

					<px:PXMenuItem Text="Customize Data Fields..." ShowSeparator="True">
						<AutoCallBack Target="DsControlProps" Command="MenuDacEditor" Enabled="True"></AutoCallBack>
					</px:PXMenuItem>

					<px:PXMenuItem Text="View Aspx Source...">
						<AutoCallBack Target="DsControlProps" Command="MenuLayoutSrc" Enabled="True"></AutoCallBack>
					</px:PXMenuItem>
					<px:PXMenuItem Text="View Business Logic Source...">
						<AutoCallBack Target="DsControlProps" Command="MenuGraphSrc" Enabled="True"></AutoCallBack>
					</px:PXMenuItem>
					<px:PXMenuItem Text="View Data Class Source...">
						<AutoCallBack Target="DsControlProps" Command="MenuDacSrc" Enabled="True"></AutoCallBack>
					</px:PXMenuItem>

					<%--                        <px:PXMenuItem Text="Open in Visual Studio" >
                            <AutoCallBack Target="DsControlProps" Command="MenuGraphVS" Enabled="True"/>
                        </px:PXMenuItem>--%>
				</MenuItems>
			</px:PXButton>
			<px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Cancel" 
				CausesValidation="False" Width="90px" />
		</px:PXPanel>
	</px:PXSmartPanel>



<px:PXDataSource ID="DsControlProps" runat="server" 
	TypeName="Customization.CustomizationGraph"
	PrimaryView="FirstSelect" CheckSession="false">
	<CallbackCommands>
					
		
		<px:PXDSCallbackCommand Name="actionCreateNewProject" 
			BlockPage="True" 
			CommitChanges="True" 	
			CommitChangesIDs="FormNewProject"  		
			RepaintControls="None" 
			RepaintControlsIDs="FormActions,FormSelectProject,FormNewProject" />			

	
			
	</CallbackCommands>


</px:PXDataSource>

	<px:PXSmartPanel ID="PXSmartPanel1" runat="server" 
		CreateOnDemand="false" 
        OnLoadContent="PXSmartPanel1_LoadContent"
		>
		<px:PXFormView ID="FormActions" runat="server" DataMember="FirstSelect"
			DataSourceID="DsControlProps">
			<Template>
				<px:PXLayoutRule runat="server"></px:PXLayoutRule>
				<px:PXTextEdit ID="edKey" runat="server" DataField="Key" >
				</px:PXTextEdit>
			</Template>

		</px:PXFormView>
	</px:PXSmartPanel>

