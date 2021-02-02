<%@ Page Title="Aspx Editor" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master"
	AutoEventWireup="true" CodeFile="SM204590.aspx.cs" Inherits="Pages_SM_SM204590"
	ValidateRequest="False" EnableViewState="False" EnableViewStateMac="False" %>

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
	<script type="text/javascript" language="javascript">
		var IsCodeEditorWindow = true;
		function FireResize()
		{
			px_cm.notifyOnResize();
		}
	</script>
	

	<script src='<%=GetScriptName("codemirror.js")%>' type="text/javascript">
	</script>

	
		<script type="text/javascript" language="javascript">

		window.IsEditorActive = false;
		window.proxy = null;

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


		}

		function SetCode(v)
		{
			if (window.proxy.editor != null) {
				window.proxy.setCode(v);
				return;
			                                 }
			window.setTimeout(function() { SetCode(v); }, 10);
		}


		function JumpLine(file, line) {

			if (!IsEditorActive)
				return false;
		
			var combo = GetObject("edFileName");
			var comboValue = combo.getValue();
			if (comboValue != file) {
				combo.updateValue(file);
				return false; 
		
			                        }

			var lineHandle = proxy.nthLine(line);
			if (!!lineHandle)
				proxy.jumpToLine(lineHandle);

			return true;
		                              }


		var px_all2;
		function IndexObjects()
		{
			if(px_all2)
				return;
			px_all2 = {};
			for(var n in px_all)
			{
				var names = n.split("_");
				var s = names[names.length - 1];
				px_all2[s] = px_all[n];
			}
		}

		function GetObject(id)
		{
			IndexObjects();
			return px_all2[id];
		}

		function HideCompilerPanel()
		{
			var c = GetObject("PanelCompiler");
			c.hide();
	

		}
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

	<px:PXDataSource ID="ds" runat="server" Visible="true" TypeName="PX.SM.GraphAspxEdit"
		PrimaryView="Filter" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="actionCheckin" BlockPage="True" CommitChanges="True"
			                        RepaintControls="None" RepaintControlsIDs="FormEditContent" CommitChangesIDs="FormEditContent"/>
			

			
		</CallbackCommands>
        
        <DataTrees>
			<px:PXTreeDataMember TreeView="SiteMapTree" TreeKeys="NodeID" />
		</DataTrees>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="FormEditContent" runat="server" DataMember="Filter" 
		Style="visibility: hidden; height: 0px; "
		DataSourceID="ds" Width="100%"  Caption="Project" SkinID="Transparent"
		
		>
		<Template>

			<px:PXTextEdit ID="EventEditBox" runat="server" DataField="Source" 
				SuppressLabel="true"
				Style="width: 100%; height: 100px;" 
				TextMode="MultiLine" Font-Names="Courier New" Font-Size="10pt"
				Wrap="False" SelectOnFocus="False">
				<ClientEvents Initialize="ActivateCsEditor" />
			</px:PXTextEdit>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXSmartPanel ID="PanelSource" runat="server" Style="width: 100%; height: 300px;" SkinID="Flat"
		RenderVisible="True" Position="Original" AllowMove="False" AllowResize="False"
		AutoSize-Enabled="True" AutoSize-Container="Window" Overflow="Hidden">
		<div id="SourcePlacehoder" style="width: 100%; height: 100%;">
		</div>
	</px:PXSmartPanel>
    
    
</asp:Content>
