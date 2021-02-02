<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Trace.aspx.cs" Inherits="Frames_Trace" %>
<%@ Register TagPrefix="px" TagName="TraceItem" Src="~/Controls/TraceItem.ascx" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Error Trace</title>
	<meta http-equiv="content-script-type" content="text/javascript">
	<style type="text/css"> 
		body {font-family:"Verdana";font-weight:normal;font-size: .7em;color:black;} 
		b {font-family:"Verdana";font-weight:bold;color:black;margin-top: -5px}
		H1 { font-family:"Verdana";font-weight:normal;font-size:18pt;color:maroon; font-weight:600; padding-left:10px; padding-top:10px }
		.version {color: gray;}
		.controls { padding-right:10px; background-color:White;text-align:right; padding-bottom:10px; }
		.button { width:auto; border: 1px ridge #C9C9C9; cursor:pointer; padding:2px; font-weight:bold; color:navy; }
	</style> 
	 <script type="text/javascript">
	 	function repaintImage(elem, url)
	 	{
	 		var ar = url.split('@'), innerDiv = elem.getElementsByTagName("div")[0];
	 		var css1 = "sprite-icon " + ar[0] + "-icon", css = elem.className;
	 		var css2 = ar[0] + "-icon-img " + ar[0] + "-" + ar[1];

	 		var i1 = css.indexOf("sprite-"), i2 = css.lastIndexOf("-icon");
	 		var newCss = [css.substring(0, i1), css1, css.substring(i2 + 6)];
	 		newCss = newCss.join(" ").trim();

	 		if (newCss != css) elem.className = newCss;
	 		elem.setAttribute("icon", ar[1]);
	 		if (innerDiv.className != css2) innerDiv.className = css2;

	 		return elem;
	 	}

 		function SendScript(sender)
 		{
 			var parent = sender.parentNode;

 			var tagList = parent.getElementsByTagName('input');
 			var elem = tagList.item(0);
 			elem.click();
 		}

 		function Togle(imgBtn)
 		{
 			var parent = imgBtn.parentNode;
 			var elem = parent.getElementsByTagName("div")[2];

 			if (elem.style.display == 'none')
 			{
 				elem.style.display = '';
 				repaintImage(imgBtn, "tree@Collapse");
 			}
 			else
 			{
 				elem.style.display = 'none';
 				repaintImage(imgBtn, "tree@Expand");
 			}
 		}
	</script>
	<script type="text/javascript">
		function ExpandAll()
		{
			var tagList = document.getElementsByName('outputDiv');
			for (var i = 0; i < tagList.length; i++)
			{
				var elem = tagList.item(i);
				elem.style.display = '';
			}

			var tagList = document.getElementsByName('outputImg');
			for (var i = 0; i < tagList.length; i++)
			{
				var elem = tagList.item(i);
				repaintImage(elem, "tree@Collapse");
			}
		}
		function CollapseAll()
		{
			var tagList = document.getElementsByName('outputDiv');
			for (var i = 0; i < tagList.length; i++)
			{
				var elem = tagList.item(i);
				elem.style.display = 'none';
			}

			var tagList = document.getElementsByName('outputImg');
			for (var i = 0; i < tagList.length; i++)
			{
				var elem = tagList.item(i);
				repaintImage(elem, "tree@Expand");
			}
		}
	</script>
</head>
<body style=" background-color:white">
	<form id="frm" runat="server" class="allowSelect">
		<span>
			<H1><px:PXLabel runat="server" ID="lblTraceCaption" Text="Acumatica Trace:" SkinID="Transparent" /></H1>
			<hr width="99%" size="2" color="#999999" />			
		</span> 
		<div class="controls">
			<span aligm="center" class="button" onclick="ExpandAll()" >
					<px:PXImage runat="server" ID="imExpandAll" ImageUrl="main@ArrowDown" />
					<span style="text-decoration:underline">Expand All</span>
			</span>
			&nbsp
			<span aligm="center" class="button" onclick="CollapseAll()" >
					<px:PXImage runat="server" ID="imCollapseAll" ImageUrl="main@ArrowUp" />
					<span style="text-decoration:underline">Collapse All</span>
			</span>
		</div> 
		<div id="placeholder" runat="server" style="Width:100%; Height:100%; background-color:White;" >
		</div>
		<br />
		<span>
			<hr width="99%" size="2" color="#999999" />
			<b>Version:</b>
			<asp:Label ID="lblVersion" runat="server" Text="Acumatca 0.00" /> &nbsp&nbsp
			<px:PXLabel runat="server" ID="lblCustList" Style="font-weight:bold; color:black" Text="Customization:" />
			<asp:Label ID="lblCustomization" runat="server" />
		</span> 
	</form>
</body>
</html>
