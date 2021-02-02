<%@ Page Language="C#" AutoEventWireup="true" EnableViewState="false" CodeFile="ProjectBrowser.aspx.cs" Inherits="Frames_ProjectBrowser" ValidateRequest="False" EnableViewStateMac="False" %>
<%@ Import Namespace="PX.Web.Customization" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" onkeypress="   if (event.charCode == 32 && event.ctrlKey) DoPublish();">
<head runat="server">
	<title>Customization Project</title>
	<link rel="icon" type="image/png" href="~/Icons/site.png" />
	<link rel="shortcut icon" type="image/x-icon" href="~/Icons/site.ico" />
       
	<script type="text/javascript">
	    var projectFrame = new Object();
	    var framesetOpen = true;
		// main frame load event handler
	    window.onload = function ()
	    {
	       // PXToolBar.prototype.AutoOpen = true;
	    	//var frame = px.searchFrame(window.top, "main");
	    	if (window['px_cm'])
	    	{
	    		window.px_cm.registerAfterLoad(function ()
	    		{
	    	        var tb = px_alls['ToolBar'];
		           // tb.AutoOpen = true;
	    			if (tb && tb.items && tb.items.length > 1)
	    			{
	    				var item = tb.items[1];
	    				if (item)
	    				{
	    					item.element.onclick = function ()
	    					{
	    						var frMain = px.searchFrame(window.top, "main");
	    						var ds = frMain && frMain['px_alls'] && frMain['px_alls']['ds'];
	    						if (ds.pressSave)
	    						{
	    							ds.pressSave();
	    						}
	    					};
	    				}
	    			}
	    		});
	    	}
	    }

        function DoPublish()
	    {
            function publishCallback()
            {
                var ds = px_alls['ds'];
                ds.executeCallback("actionPublishSingle");                
            }


            var frMain = px.searchFrame(window.top, "main");
            var ds = frMain && frMain['px_alls'] && frMain['px_alls']['ds'];
            if (ds.pressSave)
            {
                ds.pressSave(publishCallback);
            }
            else
            {
                publishCallback();
            }

        }
		projectFrame.afterMainLoad = function ()
		{
			var frMain = px.searchFrame(window.top, "main"), mainDoc;
			try { mainDoc = frMain && frMain.document; } catch (ex) { }
			if (mainDoc == null) return;

			if (window.__projectName)
				window.document.title = window.__projectName + " - Customization Project";// mainDoc.title;

			// change the adderes bar
			var path = frMain.location.pathname;
			if (path)
			{
				var project = window.__projectName ? ("Project=" + window.__projectName) : null;
				__px(frMain).updateBrowserUrl(true, project);
			}
			projectFrame.syncPosition(frMain.__nodeGuid, frMain.__queryString);

		  
		    frMain.document.documentElement.onkeypress = 
                function(event)
                {
                    //debugger;
                    if (event.charCode == 32 && event.ctrlKey)
                        window.top.DoPublish();
                };

		}

		projectFrame.hideMenu_Click = function ()
		{
			var ft = px.elemByID("frameT");
			if (ft)
			{
				var hideImg = px.elemByID("hideFrame"), splitter = px_all["sp1"];
				var cells = ft.rows[0].cells, visible = (cells[1].style.display == "");
				var panelL = px_all["panelL"], panelLT = px_all["panelLT"];

				splitter.setEnabled(!visible);
				cells[0].style.display = visible ? "none" : "";
				cells[1].style.display = visible ? "none" : "";
				if (px.isRTL())
					cells[2].style.paddingRight = visible ? "22px" : "";
				else
					cells[2].style.paddingLeft = visible ? "22px" : "";

				projectFrame.setHiddenCss(ft, "menuHidden", visible);
				if (visible) px.setTitle(hideImg.parentNode, showNavigationPane);
				else px.setTitle(hideImg.parentNode, hideNavigationPane);

				px_cm.repaintImage(hideImg, visible ? "control@ShowFrame" : "control@HideFrame");
				px_cm.notifyOnResize(visible);
			}
		}

		projectFrame.setHiddenCss = function (elem, cssName, set)
		{
			if (set)
			{
				if (elem.className.indexOf(cssName) < 0)
					elem.className += ((elem.className.length > 0) ? " " : "") + cssName;
			}
			else elem.className = elem.className.replace(cssName, "").replace(/\s+$/, '');
		}

		projectFrame.refreshMenu = function ()
		{
			var tree = px_alls["treeProj"];
			if (tree) tree.refresh();
		}

		projectFrame.syncPosition = function (nodeID, queryString)
		{
			var tree = px_alls["treeProj"];
			if (queryString)
			{
				var n = projectFrame.findNode(tree.nodes, queryString);
				if (n) { n.select(); n.activate(); n.ensureVisible(); }
			}
			else tree.sync(nodeID, true);
		}

		projectFrame.findNode = function (nodes, queryString)
		{
			if (nodes == null) return null;

			queryString = queryString.toLowerCase();
			var node;
			for (var i = 0; i < nodes.length; i++)
			{
				node = nodes.getNode(i);
				if (node.navigateUrl && node.navigateUrl.toLowerCase().indexOf(queryString) > 0)
					return node;
				if (node.hasChild)
				{
					node = projectFrame.findNode(node.childNodes, queryString);
					if (node != null) return node;
				}
			}
			return null;
		}

		function afterCallback(context, ev)
		{
			var publish = "actionPublish@";
			if (ev.command.substring(0, publish.length) == publish)
			{
				var panel = window['px_alls'] && window['px_alls']['PanelCompiler'];
				if (panel && panel.getVisible && panel.getVisible())
				{
					panel.setLoaded(false);
					panel.load();
				}
			}
		}
	</script>
    <style type="text/css">
        .browser-title {
            color: white;
            font-size: 22px;
            padding-left: 10px;
        }

        .browser-navigation {
            color: white;
            padding-right: 20px;
        }

        .nodeHint {
            color: #aaa;
            font-style: italic;
            font-weight: normal !important;
            
        }

    </style>
</head>
<body>
	<%= ClientSideAppsHelper.RenderScriptConfiguration() %>
	<form id="form1" runat="server">
		<table cellpadding="0" cellspacing="0" style="width:100%" class="toolSysTable">
			<tr>
			<td>
<%--				<a id="logoCell" runat="server" class="logo" target="main">
					<asp:Image id="logoImg" runat="server" ImageUrl="~/Icons/logo.png" CssClass="logoImg" />
				</a>--%>
                <span class="browser-title">Customization Project Editor</span>
			</td>
                <td style="text-align: right;">
                    <a href="javascript:window.history.back()" class="browser-navigation">Back</a>
                    <a href="javascript:location.reload()" class="browser-navigation">Reload</a>

                </td>
			</tr>
		</table>
		
		<px:PXDataSource ID="ds" ToolBarSkin="ModulesMenu" Visible="True" runat="server" 
			TypeName="PX.Web.Customization.ProjectBrowserMaint" PrimaryView="Filter" KeySeparatorChar="!">
		    <DataTrees>
			    <px:PXTreeDataMember TreeView="SiteMapTree" TreeKeys="NodeID" />
            </DataTrees>
			<CallbackCommands>
				<px:PXDSCallbackCommand Name="actionFileCheckIn" Visible="False"/>
				<px:PXDSCallbackCommand Name="actionRemoveConflicts" Visible="False"/>
			    <px:PXDSCallbackCommand Name="actionValidateInitials" Visible="False"/>
			    <px:PXDSCallbackCommand Name="actionISVInitialsSave" Visible="False"/>
			    <px:PXDSCallbackCommand Name="actionISVInitialsSaveClose" Visible="False"/>
			    <px:PXDSCallbackCommand Name="actionISVInitialsCancel" Visible="False"/>
			</CallbackCommands>	
                    <ClientEvents CommandPerformed="afterCallback"/>
              
            <DataTrees>
			    <px:PXTreeDataMember TreeView="FilesTree" TreeKeys="FileKey" />
		    </DataTrees>	
        </px:PXDataSource>

		<table id="frameT" runat="server" style="width:100%;height:100px" cellspacing="0" cellpadding="0">
			<tr>
				<td class="leftFrame" runat="server" style="width: 19%">
					<label runat="server" id="projectLink" class="projectLink border-box">Project</label>
					<px:PXFormView runat="server" ID="form" DataMember="Filter" Width="100%" Height="50px">
					</px:PXFormView>

					<px:PXSmartPanel ID="panelProj" runat="server" SkinID="Frame" RenderVisible="true">
						<px:PXTreeView runat="server" ID="treeProj" SkinID="Menu" Target="main" ShowRootNode="False" 
							DataSourceID="ds" PreserveExpanded="true" ShowDefaultImages="false" ShowLines="false" 
							CssClass="tree menuTree" RenderHtmlText ="True">
							<DataBindings>
								<px:PXTreeItemBinding DataMember="SiteMapTree" TextField="Title" ValueField="NodeID" ImageUrlField="" NavigateUrlField="Url" />
							</DataBindings>
						</px:PXTreeView>
					</px:PXSmartPanel>
				</td>                
				
				<td>
					<px:PXSplitter ID="sp1" runat="server" SkinID="Frame" Style="height: 100%" Orientation="Vertical" 
						Panel1MinSize="150" SaveSizeUnits="True" FixPanel1Size="true" AllowCollapse="false" Size="4">
						<AutoSize Enabled="true" Container="Window" />
					</px:PXSplitter>
				</td>

				<td class="rightFrame" style="width: 82%">
					<div style="position:relative">
						<div class="hideFrameBox" runat="server" ID="hideFrameBox" onclick="projectFrame.hideMenu_Click()" title="Hide Navigation Pane"> 
							<px:PXImage runat="server" ID="hideFrame" ImageUrl="control@HideFrame" />
						</div>
					</div>
				    
					<px:PXSmartPanel ID="panelF" runat="server" SkinID="Frame" IFrameName="main" RenderIFrame="true" 
						Overflow="Auto" CssClass="SmartPanelF screenFrame" ClientEvents-AfterLoad="projectFrame.afterMainLoad">
					</px:PXSmartPanel>
					

				</td>
			</tr>
		</table>
         
		<script type="text/javascript" language="javascript">

			function FireResize()
			{
				px_cm.notifyOnResize();
			}

			function SetTooltip()
			{
			    var title = "Press Escape to Close";
			    var element = document.getElementById("PanelCompiler_cap");
                if(element)
			        __px(window).setTitle(element.firstChild, title);
			}

		</script>
		<px:PXSmartPanel ID="PanelCompiler" runat="server"
			Style="height: 300px; width: 100%;"
			CaptionVisible="True"
			Caption="Compilation"
			RenderIFrame="True"
			RenderVisible="False"
			IFrameName="Compiler"
			InnerPageUrl="~/Controls/Publish.aspx?compile=true&amp;silent=true"
			WindowStyle="Flat"
			AutoReload="True"
			AllowResize="False"
			AllowMove="True"
			Key="ViewPublishProject"
			Position="Original"
			ShowAfterLoad="True"
            ClientEvents-Initialize="SetTooltip"
			ClientEvents-AfterHide="FireResize" ClientEvents-AfterShow="FireResize"
			BlockPage="False" ClientEvents-BeforeLoad="FireResize" />
        
        
        


<%--	<px:PXSmartPanel ID="PanelTFS" runat="server" Caption="Source Control Binding"  
		CaptionVisible="True"  Width="442px" Key="ViewParentFolder" AutoRepaint="True">
		<px:PXFormView ID="ViewParentFolder" runat="server" DataMember="ViewParentFolder" DataSourceID="ds" 
			Width="396px" AutoRepaint="True" SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" ControlSize="M" LabelsWidth="S"/>
			
				<px:PXTextEdit ID="ParentFolder" runat="server" DataField="ParentFolder" Required="True" />
				<px:PXTextEdit ID="ProjectName" runat="server" DataField="ProjectName" Required="True" />
		       
                
			</Template>
		</px:PXFormView>
		
		<px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartRow="true" />
		<px:PXPanel ID="PanelTFSPXPanel1" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PanelTFSButtonOk" runat="server" DialogResult="OK" Text="OK">
		        <AutoCallBack Target="ViewParentFolder" Command="Save"/>
             </px:PXButton>
			<px:PXButton ID="PanelTFSButtonCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
    --%>
        
                
        
        
        

        
	<px:PXSmartPanel ID="PanelSaveTFS" runat="server" Caption="Save Project to Folder"  
		CaptionVisible="True" Key="ViewSolutionConfig__PanelSaveTFS" AutoRepaint="True">
		<px:PXFormView ID="PXFormView2" runat="server" DataMember="ViewSolutionConfig" DataSourceID="ds" 
			SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule  runat="server" StartColumn="True" ControlSize="XL" LabelsWidth="S"/>
			
                
                                 	
                <px:PXTreeSelector ID="PXTreeSelector1" runat="server" DataField="ParentFolder" PopulateOnDemand="True" InitialExpandLevel="0" 
				    ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="FilesTree" MinDropWidth="413"  AllowEditValue="True" SelectOnFocus="False" >
                    <Images >
                        <LeafImages Normal="tree@Folder" Selected="tree@Folder"></LeafImages>
                    </Images>
                    
				    <DataBindings>
					    <px:PXTreeItemBinding DataMember="FilesTree" TextField="FileName" DescriptionField="FilePath" ValueField="FileKey" />
				    </DataBindings>
				
			    </px:PXTreeSelector>
                
                
                <px:PXTextEdit ID="PXTextEdit2" runat="server" DataField="ProjectName" Required="True" />
		       
                
			</Template>
		</px:PXFormView>
		
		<px:PXPanel ID="PXPanel2" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PXButton3" runat="server" DialogResult="OK" Text="OK"/>
			<px:PXButton ID="PXButton4" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
            
        
        
        

	<px:PXSmartPanel ID="PanelOpenTFS" runat="server" Caption="Open Project from Folder"  
		CaptionVisible="True" Key="ViewSolutionConfig__PanelOpenTFS" AutoRepaint="True">
		<px:PXFormView ID="PXFormView3" runat="server" DataMember="ViewSolutionConfig" DataSourceID="ds" 
			SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule  runat="server" StartColumn="True" ControlSize="XL" LabelsWidth="S"/>
			
                
                                 	
                <px:PXTreeSelector ID="PXTreeSelector2" runat="server" DataField="ContainingFolder" PopulateOnDemand="True" InitialExpandLevel="0" 
				    ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="FilesTree" MinDropWidth="413"  AllowEditValue="True" SelectOnFocus="False" >
                    <Images >
                        <LeafImages Normal="tree@Folder" Selected="tree@Folder"></LeafImages>
                    </Images>
                    
				    <DataBindings>
					    <px:PXTreeItemBinding DataMember="FilesTree" TextField="FileName" DescriptionField="FilePath" ValueField="FileKey" />
				    </DataBindings>
				
			    </px:PXTreeSelector>
		       
                
			</Template>
		</px:PXFormView>
		
		<px:PXPanel ID="PXPanel3" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PXButton5" runat="server" DialogResult="OK" Text="OK"/>
			<px:PXButton ID="PXButton6" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
    
        
        
        

        
	<px:PXSmartPanel ID="PanelCreateLIB" runat="server" Caption="Create Extension Library"  
		CaptionVisible="True" Key="ViewSolutionConfig__PanelCreateLIB" AutoRepaint="True">
		<px:PXFormView ID="PXFormView1" runat="server" DataMember="ViewSolutionConfig" DataSourceID="ds" 
			SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule  runat="server" StartColumn="True" ControlSize="XL" LabelsWidth="S"/>
			
                
                                 	
                <px:PXTreeSelector ID="edPathSelector" runat="server" DataField="ParentFolder" PopulateOnDemand="True" InitialExpandLevel="0" 
				    ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="FilesTree" MinDropWidth="413" AllowEditValue="True" SelectOnFocus="False">
                    <Images >
                        <LeafImages Normal="tree@Folder" Selected="tree@Folder"></LeafImages>
                    </Images>
                    
				    <DataBindings>
					    <px:PXTreeItemBinding DataMember="FilesTree" TextField="FileName" DescriptionField="FilePath" ValueField="FileKey" />
				    </DataBindings>
				
			    </px:PXTreeSelector>
                
                
                <px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="ProjectName" Required="True" />
		       
                
			</Template>
		</px:PXFormView>
		
		<px:PXPanel ID="PXPanel12" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PXButton7" runat="server" DialogResult="OK" Text="OK"/>
			<px:PXButton ID="PXButton8" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
            
        
        
        

	<px:PXSmartPanel ID="PanelOpenLIB" runat="server" Caption="Bind to Extension Library" 
		CaptionVisible="True" Key="ViewSolutionConfig__PanelOpenLIB" AutoRepaint="True">
		<px:PXFormView ID="PXFormView22" runat="server" DataMember="ViewSolutionConfig" DataSourceID="ds" 
			SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule  runat="server" StartColumn="True" ControlSize="XL" LabelsWidth="120px"/>
			
                
                                 	
                <px:PXTreeSelector ID="edPathSelector" runat="server" DataField="ContainingFolder" PopulateOnDemand="True" InitialExpandLevel="0" 
				    ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="FilesTree" MinDropWidth="413"  AllowEditValue="True" SelectOnFocus="False" >
                    <Images >
                        <LeafImages Normal="tree@Folder" Selected="tree@Folder"></LeafImages>
                    </Images>
                    
				    <DataBindings>
					    <px:PXTreeItemBinding DataMember="FilesTree" TextField="FileName" DescriptionField="FilePath" ValueField="FileKey" />
				    </DataBindings>
				
			    </px:PXTreeSelector>
		       
                
			</Template>
		</px:PXFormView>
		
		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="OK"/>
			<px:PXButton ID="PXButton2" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>
    
        	 
<%--	<px:PXSmartPanel ID="PanelTFSConfig" runat="server" Caption="Source Control Setup"  
		CaptionVisible="True"  Width="442px" Key="ViewConfig" AutoRepaint="True">
		<px:PXFormView ID="ViewConfig" runat="server" DataMember="ViewConfig" DataSourceID="ds" 
			Width="396px" AutoRepaint="True" SkinID="Transparent">
			
		    <Template>
		    	
		    	<px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" ControlSize="L" LabelsWidth="XS"/>
				<px:PXTextEdit ID="Config" runat="server" DataField="Config" Required="True" />
		       
                
			</Template>
		</px:PXFormView>
		
		<px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartRow="true" />
		<px:PXPanel ID="PanelTFSPXPanel11" runat="server" SkinID="Buttons">
		    <px:PXButton ID="PanelTFSButtonOk1" runat="server" DialogResult="OK" Text="OK">
		        <AutoCallBack Target="ViewParentFolder" Command="Save"/>
             </px:PXButton>
			<px:PXButton ID="PanelTFSButtonCancel1" runat="server" DialogResult="Cancel" Text="Cancel" />
		</px:PXPanel>
	</px:PXSmartPanel>--%>
		
		<px:PXUploadFilePanel ID="UploadPackageDlg" runat="server" 
		CommandSourceID="ds"
		AllowedTypes=".zip" Caption="Open Package" PanelID="UploadPackagePanel" 
		OnUpload="uploadPanel_Upload"
		CommandName="ReloadPage" />
		

        <px:PXSmartPanel runat="server"
                         ID="PanelCheckinFiles"
                         Width="600px"
                         Height="400px"
                         CaptionVisible="True"
                         Key="ViewCheckinFiles"
                         Caption="Modified Files Detected"
                         ShowMaximizeButton="True"
                         Overflow="Hidden"
                         AutoCallBack-Enabled="True"
                         AutoCallBack-Target="ds"
                         AutoCallBack-Command="actionShowDlgCheckinFiles"> 
            <px:PXLabel runat="server"
                        Text="Some files have been modified in the file system. Please resolve the conflicts."
                        Style="color:black"/>
            <px:PXGrid runat="server"
                       ID="gridCheckinFiles"
                       DataSourceID="ds"
                       Width="100%"
                       BatchUpdate="True"
                       AutoAdjustColumns="True"
                       SkinID="Details">
                <Levels>
                    <px:PXGridLevel DataMember="ViewCheckinFiles">
                        <Columns>
                            <px:PXGridColumn DataField="Selected" Type="CheckBox"/>
                            <px:PXGridColumn DataField="Conflict" Type="CheckBox"/>
                            <px:PXGridColumn DataField="Path" Width="400px"/>
                        </Columns>
                    </px:PXGridLevel>
                </Levels>
                <AutoSize Enabled="True" Container="Parent"/>
                <ActionBar Position="None"/>
            </px:PXGrid>
            <px:PXPanel ID="PXPanel41"
                        runat="server"
                        SkinID="Buttons">
                <px:PXButton ID="PXButtonUpdate"
                             runat="server"
                             Text="Update Customization Project">
		            <AutoCallBack Enabled="True"
                                  Target="ds"
                                  Command="actionFileCheckIn"/>
                </px:PXButton>
                <px:PXButton ID="PXButton9"
                             runat="server"
                             DialogResult="Cancel"
                             Text="Discard all changes">
		            <AutoCallBack Enabled="True"
                                  Target="ds"
                                  Command="actionRemoveConflicts"/>
                </px:PXButton>
		    </px:PXPanel>
        </px:PXSmartPanel>
        

		
<%--			<px:PXSmartPanel ID="PanelCompiler" runat="server" 
	                 style="height:300px;width:90%;" 
	                 CaptionVisible="True" 
	                 Caption="Compilation" 

	                 RenderIFrame="True" 
	                 RenderVisible="False" 
	                 IFrameName="Compiler"
					InnerPageUrl="~/Controls/Publish.aspx?compile=true"
 
	                 WindowStyle="Flat" 
	                 AutoReload="True" 
	                 ShowMaximizeButton="True" 
	                 AllowMove="True" 
	                 Key="ViewPublishProject"
		
		/>--%>
		
    	<px:PXSmartPanel ID="PanelValidateExt" runat="server" Caption="Validate Extensions"  ShowMaximizeButton="True"
		CaptionVisible="True"  Width="800px" Height="600px" Key="ViewValidateExtensions" AutoRepaint="True">
    	    
         	<px:PXFormView ID="PXFormView4" runat="server" DataMember="ViewValidateExtensions" DataSourceID="ds" 
			Width="100%" AutoRepaint="True" SkinID="Transparent">
         		    <AutoSize Enabled="True"></AutoSize>
                      <Template>
            <px:PXHtmlView runat="server" DataField="Messages" Width="100%" ID="Msg"
				Style="white-space: pre; font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; "
                >
                
                <AutoSize Enabled="True"></AutoSize>
            </px:PXHtmlView>
			</Template>
		</px:PXFormView>
	</px:PXSmartPanel>
    
    <px:PXSmartPanel ID="PanelManageISVSolutionInitials" runat="server" Caption="Customization Project Prefix" CaptionVisible="True" 
                     Width="800px" Height="600px" Key="ViewManageISVSolutionInitials" AutoRepaint="True" >
        <px:PXFormView runat="server" ID="PXFormViewISVSolutionInitials" DataMember="ViewManageISVSolutionInitials" DataSourceID="ds" Height="100%"
                       Width="100%" AutoRepaint="True" SkinID="Transparent">
            <AutoSize Enabled="True"></AutoSize>
            <Template>
                <px:PXPanel runat="server" ID="layoutPanel" SkinID="Transparent" >
                <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" LabelsWidth="SM" ControlSize="S" />
                <px:PXLayoutRule runat="server" Merge="True"   />
                <px:PXTextEdit ID="edInitials" runat="server" DataField="Initials"></px:PXTextEdit>
                <px:PXButton ID="PXButtonValidateSolutionObjects" runat="server" Text="VALIDATE PROJECT ITEMS" >
                    <AutoCallBack Enabled="True" Target="ds" Command="actionValidateInitials"/>
                </px:PXButton>
                </px:PXPanel>
                <px:PXGroupBox runat="server" Caption="VALIDATION RESULT" RenderStyle="Fieldset"  />
                <px:PXHtmlView runat="server" ID="edMessages" DataField="Messages" Width="100%" TextMode="MultiLine"  Height="100%"
                               Style="white-space: pre; font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px; ">
                    <AutoSize Enabled="True" Container="Parent" MinHeight="350" />
                </px:PXHtmlView>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanelValidationISVButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="PXButtonISVSave" runat="server" Text="SAVE">
                <AutoCallBack Enabled="True" Target="ds" Command="actionISVInitialsSave"/>
            </px:PXButton>
            <px:PXButton ID="PXButtonISVSaveClose" runat="server" DialogResult="OK" Text="SAVE & CLOSE">
                <AutoCallBack Enabled="True" Target="ds" Command="actionISVInitialsSaveClose"/>
            </px:PXButton>
            <px:PXButton ID="PXButtonISVCancel" runat="server" DialogResult="Cancel" Text="CANCEL">
                <AutoCallBack Enabled="True" Target="ds" Command="actionISVInitialsCancel"/>
            </px:PXButton>
        </px:PXPanel>
    </px:PXSmartPanel>

	</form>
</body>
</html>
