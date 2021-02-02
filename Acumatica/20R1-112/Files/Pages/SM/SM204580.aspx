<%@ Page Title="Code Editor" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master"
	AutoEventWireup="true" CodeFile="SM204580.aspx.cs" Inherits="Pages_SM_SM204580"
	EnableViewStateMac="False" EnableViewState="False" ValidateRequest="False" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	<style type="text/css">
		.CodeMirror-wrapping
		{
			background-color: White;
			border: 1px solid gray;
			margin: 0px;
			border-right-width: 0px;
			border-left-width: 0px;
		}
		.CodeMirror-line-numbers
		{
			font-family: monospace;
			font-size: 10pt;
			color: gray;
			padding: .4em;
			background-color: #eee;
			border-right: 1px solid gray;
			text-align: right;
			padding-left: 15pt;
		}
	</style>
	<script type="text/javascript">
	//	window.isClientDirty = true;
		var IsCodeEditorWindow = true;
		function FireResize()
		{
			px_cm.notifyOnResize();
		}
	</script>
	<script src='<%=GetScriptName("codemirror.js")%>' type="text/javascript">
	</script>
	<script type="text/javascript">

		window.IsEditorActive = false;
		window.proxy = null;
		window.dsInitPostponed = false;

		function ActivateCsEditor(editor)
		{
			if (window.IsEditorActive)
				return;
			window.IsEditorActive = true;

			var config = {
				parserfile: ["<%=GetScriptName("tokenizecsharp.js")%>",
					"<%=GetScriptName("parsecsharp.js")%>"],

				stylesheet: "<%=GetScriptName("csharpcolors.css")%>",
				//path: "cseditor/",
				height: "100%",
				autoMatchParens: true,
				textWrapping: false,
				lineNumbers: true,
				tabMode: "shift",
				enterMode: "keep",
				content: editor.getValue(),
				basefiles: [
					"<%=GetScriptName("util.js")%>",
					"<%=GetScriptName("stringstream.js")%>",
					"<%=GetScriptName("select.js")%>",
					"<%=GetScriptName("undo.js")%>",
					"<%=GetScriptName("editor.js")%>",
					"<%=GetScriptName("tokenize.js")%>"]

				//  "util.js", "stringstream.js", "select.js", "undo.js", "editor.js", "tokenize.js"],
			};

			var p = document.getElementById("SourcePlacehoder");

			var replace = function(newElement)
			{
				p.appendChild(newElement);

			};
			window.proxy = new CodeMirror(replace, config);
		   
			editor.onCallback = function()
			{
				if (window.proxy.editor != null)
				{
					var code = window.proxy.getCode();
					editor.updateValue(code);

				}
			};

			editor.baseRepaintText = editor.repaintText;
			editor.repaintText = function(v)
			{
				if (v === null || v === undefined)
					v = "";
				SetCode(v);
				editor.baseRepaintText(v);
			};

			window.proxy.wrapping.id = "csproxywrapping";
			if (window.dsInitPostponed) OnDsInit(window.px_alls['ds']);
		}
		
		function OnDsInit(ds)
		{
			if (window.proxy == undefined)
			{
				window.dsInitPostponed = true;
				return;
			}
			window.proxy.win.document.documentElement.addEventListener('keydown', function (e)
			{
				if ((e.keyCode == 83 || e.keyCode == 115) && (e.ctrlKey))
				{
					ds.pressSave();
					__px(ds).cancelEvent(e);
					return false;
				}

				if (e.keyCode == 32 && e.ctrlKey)
				{
					window.top.DoPublish();
					return false;
				}

				switch (e.keyCode)
				{
					case 8: case 46: case 13:
						ds.setClientChanged();
						break;
				}
			});
			window.proxy.win.document.documentElement.addEventListener('keypress', function (e)
			{
				if ((e.charCode == 115 || e.charCode == 83) && (e.ctrlKey || e.metaKey))
				{
					ds.pressSave();
					__px(ds).cancelEvent(e);
					return false;
				}
				//if (e.charCode == 32 && e.ctrlKey) {
				//    window.top.DoPublish();
				//    return false;
				//}
				ds.setClientChanged();
			});

			window.proxy.options.initCallback = function ()
			{
				window.proxy.editor.subscribe('paste', function (e)
				{
					ds.setClientChanged();
				});
				window.proxy.editor.subscribe('keydown', function (e)
				{
					if (e.keyCode == 86 && e.ctrlKey && !e.altKey && !e.shiftKey)
						ds.setClientChanged();
				});
			};
		}

		function SetCode(v) {
			if (window.proxy.editor != null) {
				window.proxy.setCode(v);
				return;
			}
			window.setTimeout(function () { SetCode(v); }, 10);
		}

		function JumpLine(file, line)
		{
			if (!IsEditorActive)
				return false;

			//var combo = GetObject("edFileName");
			//var comboValue = combo.getValue();
			//if (comboValue != file) {
			//	combo.updateValue(file);

			//	return false;

			//                        }

			var lineHandle = proxy.nthLine(line);
			if (!!lineHandle)
				proxy.jumpToLine(lineHandle);

			return true;
		}

	    //var px_all2;
		//function IndexObjects()
		//{
		//	if(px_all2)
		//		return;
		//	px_all2 = {};
		//	for(var n in px_all)
		//	{
		//		var names = n.split("_");
		//		var s = names[names.length - 1];
		//		px_all2[s] = px_all[n];
		//	}
		//}

		//function GetObject(id)
		//{
		//	IndexObjects();
		//	return px_all2[id];
		//}

		//function HideCompilerPanel()
		//{
		//	var c = GetObject("PanelCompiler");
		//	c.hide();
		//}
	</script>

	<px:PXFormView runat="server" SkinID="transparent" ID="formTitle" 
		DataSourceID="ds" DataMember="ViewPageTitle" Width="100%">
		<Template>
			<px:PXTextEdit runat="server" ID="PageTitle" DataField="PageTitle" SelectOnFocus="False"
				SkinID="Label" SuppressLabel="true"
				Width="90%"
				style="padding: 10px">
				<font size="14pt" names="Arial,sans-serif;"/>
			</px:PXTextEdit>
		</Template>
	</px:PXFormView>
	

	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.SM.GraphCodeFiles"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
<%--			<px:PXDSCallbackCommand Name="actionNewFile" 
                RepaintControls="Bound" 
                BlockPage="true"
				Visible="false" 
                CommitChanges="True" 
                 />--%>
<%--			<px:PXDSCallbackCommand Name="actionValidate" RepaintControls="None" BlockPage="true"
				CommitChanges="True" RepaintControlsIDs="FormFilter" PostData="Page" PostDataControls="True" />
			<px:PXDSCallbackCommand Name="actionVisualStudio" RepaintControls="None" BlockPage="true"
				CommitChanges="True" RepaintControlsIDs="FormFilter" PostData="Page" PostDataControls="True" />--%>
			<%--<px:PXDSCallbackCommand Visible="False" Name="actionNewFileDlg" PopupPanel="PanelNewFile" />--%>
		</CallbackCommands>
		<ClientEvents Initialize="OnDsInit" />
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="FormFilter" runat="server" DataMember="Filter" DataSourceID="ds"
		Width="100%"  Caption="Project" Style="display: none;visibility: hidden;" Height="0px">
		<Template>
	
			<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="SM" StartColumn="True"/>
			
<%--			<px:PXSelector CommitChanges="True" ID="edFileName" runat="server" DataField="ObjectName"
				AutoRefresh="True" AutoAdjustColumns="True" DataSourceID="ds" />--%>
		</Template>
	</px:PXFormView>
	<px:PXFormView ID="FormEditContent" runat="server" DataMember="Files" DataSourceID="ds"
		Style="position: absolute; left: 0px; top: 0px; width: 200px; display: none;
		visibility: hidden">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
			<px:PXTextEdit SuppressLabel="True" Height="20px" ID="EventEditBox" runat="server"
				DataField="FileContent" TextMode="MultiLine" Font-Names="Courier New" Font-Size="10pt"
				Wrap="False" SelectOnFocus="False">
				<ClientEvents Initialize="ActivateCsEditor" />
			</px:PXTextEdit></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSmartPanel ID="PanelSource" runat="server" Style="width: 100%; height: 300px;"
		RenderVisible="True" Position="Original" AllowMove="False" AllowResize="False"
		AutoSize-Enabled="True" AutoSize-Container="Window" Overflow="Hidden"
		 SkinID="Transparent"
		>
		<div id="SourcePlacehoder" style="width: 100%; height: 100%;">
		</div>
	</px:PXSmartPanel>
  

</asp:Content>

<asp:Content ID="Dialogs" ContentPlaceHolderID="phDialogs" runat="Server">
	<px:PXSmartPanel runat="server" ID="ViewBaseMethod" Width="600px" Height="400px" CaptionVisible="True"
		Caption="Select Methods to Override" ShowMaximizeButton="True" Overflow="Hidden" Key="ViewBaseMethod" AutoRepaint="True">

		<px:PXGrid runat="server"
			ID="gridAddFile" DataSourceID="ds"
			Width="100%" Height="200px" BatchUpdate="True"
			AutoAdjustColumns="True"
			SkinID="Details">
			<Levels>
				<px:PXGridLevel DataMember="ViewBaseMethod">
					<Columns>
						<px:PXGridColumn DataField="Selected" Type="CheckBox" Width="50px" />
						<px:PXGridColumn DataField="DeclaringType" Width="100px" />
						<px:PXGridColumn DataField="Name" Width="300px" />

					</Columns>
				</px:PXGridLevel>
			</Levels>
			<AutoSize Enabled="True" />
			<ActionBar Position="Top" ActionsVisible="False">
				<Actions>
					<AddNew MenuVisible="False" ToolBarVisible="False" />
					<Delete MenuVisible="False" ToolBarVisible="False" />
					<AdjustColumns ToolBarVisible="False" />
					<ExportExcel ToolBarVisible="False" />
				</Actions>

			</ActionBar>
		</px:PXGrid>

		<px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton1" runat="server" DialogResult="OK" Text="Save" />
			<px:PXButton ID="PXButton2" runat="server" DialogResult="No" Text="Cancel" CausesValidation="False" />
		</px:PXPanel>
	</px:PXSmartPanel>
    
    	<px:PXSmartPanel ID="DlgActionWizard" runat="server"
		CaptionVisible="True"
		Caption="Create Action"
		AutoRepaint="True"
		Key="ViewActionWizard">
		<px:PXFormView ID="FormActionWizard" runat="server"
			SkinID="Transparent" DataMember="ViewActionWizard">
			<Template>
				<px:PXLayoutRule ID="PXLayoutRule11" runat="server" StartColumn="True" />

				<px:PXTextEdit runat="server" ID="ActionName" DataField="ActionName" />
				<px:PXTextEdit runat="server" ID="DisplayName" DataField="DisplayName"/>
				

			</Template>
		</px:PXFormView>

		<px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartRow="True" />
		<px:PXPanel ID="PXPanel7" runat="server" SkinID="Buttons">
			<px:PXButton ID="PXButton13" runat="server" DialogResult="OK" Text="OK">
			</px:PXButton>

			<px:PXButton ID="PXButton14" runat="server" DialogResult="Cancel" Text="Cancel" CausesValidation="False">
			</px:PXButton>
		</px:PXPanel>

	</px:PXSmartPanel>

</asp:Content>
