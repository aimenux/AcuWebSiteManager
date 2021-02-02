<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Menu.aspx.cs" Inherits="Menu"
	ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Menu Items</title>
	<meta http-equiv="content-script-type" content="text/javascript">
	<style type="text/css">
		.vertical
		{ 
			writing-mode: tb-rl; filter: flipv() fliph(); 
			padding:5px 0px; height:300px; 
			font-family: Tahoma, Verdana, Arial, Helvetica, sans-serif;
			font-size: 12pt; font-weight: bold;
			position: absolute; bottom: 0px;
		}
	</style>
	       
	<script type="text/javascript">
		function onActivePanelLoad(panel, args)
		{
			var template = 'navPanel_sp';
			var index = parseInt(panel.ID.substring(template.length, panel.ID.length));
			setTimeout(function() { setActiveBarItem(index); }, 1);
		}

		function setActiveBarItem(index)
		{
			var navPanel = px_all["navPanel"];
			navPanel.setActiveBarItem(index, null);
			updateSearchBoxNavigation(navPanel, index);
		}

		function onActiveBarChanging(navPanel, args) 
		{
			var panel = px_all["navPanel_sp" + args.newIndex];
			if (!panel.loaded) 
			{
				panel.events.addEventHandler("afterLoad", onActivePanelLoad);
				panel.load();
				args.cancel = true;
			}
			else
				updateSearchBoxNavigation(px_all["navPanel"], args.newIndex);
		}

		function treeClick() 
		{
			var navPanel = px_all["navPanel"];
			var i = navPanel.activeBarIndex;
			if (i != null) px.setCookie("activebar", i, "/");
		}

		function updateSearchBoxNavigation(navPanel, index) 
		{
			var searchBox = px_all["srch"];
			var tree = getTreeInPanel(index);
			var wikiurl = "../search/Search.aspx";

			if (tree != null && navPanel.bars[index].contentUrl.substr(0, wikiurl.length).toLowerCase() == wikiurl)
				searchBox.searchNavigateUrl = tree.searchUrl;
			else
				searchBox.searchNavigateUrl = searchBox.defaultSearchNavigateUrl;
		}

		function getTreeInPanel(index) 
		{
			var panel = px_all["navPanel_sp" + index];
			var idstart = "navPanel_tree";
			for (var i = 0; i < panel.element.childNodes.length; i++) 
			{
				if (panel.element.childNodes[i].object != null && panel.element.childNodes[i].object.__className == "PXWikiTree")
					return panel.element.childNodes[i].object;
			}
			return null;
		}

		function verifySync()
		{
			var indexStart = window.location.search.indexOf("syncID=");
			if (indexStart != -1)
			{
				indexStart += 7;
				var len = window.location.search.indexOf("&");
				if (len == -1) len = window.location.search.length - indexStart;

				var syncID = window.location.search.substr(indexStart, len);
				var frMain = px.searchFrame(window.top, "main");
				frMain.__syncTitleTree(syncID);
			}
		}

		function syncMenu_Click(btn, ev)
		{
			var frMain = px.searchFrame(window.top, "main");
			frMain.__syncTitleTree(frMain.__nodeGuid);
		}

		function createSVG(owner, text)
		{
			var fontSize = 12, fontFamily = "tahoma,verdana";
			var obj = document.createElement("object");
			var h = owner.clientHeight, w = owner.clientWidth;

			var swg = obj.cloneNode(true);
			swg.height = (h > w) ? h : w;
			swg.type = "image/svg+xml";
			swg.width = 20;
			swg.data = "data:image/svg+xml;charset/windows-1251,<svg xmlns='http://www.w3.org/2000/svg'><text x='" +
				(-swg.height + 10) + "' y='" + 16 + "' style='font-family:" + fontFamily + "; font-size:" +
				fontSize + "pt; font-weight: bold; text-anchor: top' transform='rotate(-90)'>" + text + "</text></svg>";
			owner.replaceChild(swg, owner.firstChild);
		}
	</script>

</head>
<body onload="verifySync()" style="overflow:hidden;">
	<form id="form1" runat="server">
	<table id="tblLogo" runat="server" width="100%" cellpadding="0" cellspacing="0">
		<tr>
			<td id="logoCell" runat="server" class="Logo" style="vertical-align: top;">
			</td>
		</tr>
		<tr>
			<td>
				<div style="padding: 8px 9px 2px 9px;">
					<px:PXSearchBox ID="srch" runat="server" Target="main" Font-Size="8pt" />
				</div>
			</td>
		</tr>
		<tr id="trNavPanel" runat="server">
			<td>
				<div style="padding: 4px" runat="server" id="divNavPanel">
				</div>
			</td>
		</tr>
	</table>
	<div style="position: relative">
		<table style="position: absolute; left: 0px; z-index: 1000" cellpadding="0" cellspacing="0">
			<tr><td style="padding-top: 3px">
				<px:PXButton runat="server" ID="btnSyncMenu" ToolTip="Sync Navigation Pane"
					SkinID="Transparent" RenderAsButton="false" Width="22px" Height="20px" ImageSet="main" ImageKey="SyncTOC">
					<ClientEvents Click="syncMenu_Click" />
				</px:PXButton>
			</td></tr>
		</table>
		<px:PXNavPanel ID="navPanel" runat="server" Height="280px" Width="100%" VisibleBars="3"
			OnBarItemDataBound="navPanel_BarItemDataBound" Style="position: relative" BorderWidth="0px"
			OnDataBound="navPanel_OnDataBound">
			<AutoSize Container="Window" Enabled="True" />
			<ClientEvents ActiveBarChanging="onActiveBarChanging" />
			<Styles> <Caption CustomAttr="padding-left: 22px" /> </Styles>
		</px:PXNavPanel>
	</div>
	<div id="vertText" class="vertical" style="display: none">Vertical text</div>   	
	</form>
</body>
</html>
