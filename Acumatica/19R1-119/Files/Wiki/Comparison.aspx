<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Comparison.aspx.cs" Inherits="Wiki_Compare"
	EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Untitled Page</title>
</head>
<body onload="startScrollingDetection()">

	<script language="javascript" type="text/javascript">
	var scrollTopPos = 0;
	var scrollLeftPos = 0;
	function startScrollingDetection()
	{
		setInterval("normalizeScrolls()", 100);
	}
	
	function normalizeScrolls()
	{
		var div1 = document.getElementById("PXFormView1_content1");
		var div2 = document.getElementById("PXFormView1_content2");
		
		if(div1.scrollTop != scrollTopPos && div2.scrollHeight - div2.clientHeight >= div1.scrollTop)
		{
			scrollTopPos = div1.scrollTop;
			div2.scrollTop = scrollTopPos;
		}
		else if(div2.scrollTop != scrollTopPos && div1.scrollHeight - div1.clientHeight >= div2.scrollTop)
		{
			scrollTopPos = div2.scrollTop;
			div1.scrollTop = scrollTopPos;
		}
		
		
		if(div1.scrollLeft != scrollLeftPos && div2.scrollWidth - div2.clientWidth >= div1.scrollLeft)
		{
			scrollLeftPos = div1.scrollLeft;
			div2.scrollLeft = scrollLeftPos;
		}
		else if(div2.scrollLeft != scrollLeftPos && div1.scrollWidth - div1.clientWidth >= div2.scrollLeft)
		{
			scrollLeftPos = div2.scrollLeft;
			div1.scrollLeft = scrollLeftPos;
		}
	}
	
	function adjustTableSize()
	{
		var form = document.getElementById("PXFormView1");
		var div = document.getElementById("PXFormView1_content1");
		var headerheight = 25 + 10 + 13;
		if(form.clientHeight - headerheight > 25)
		{
			div.style.height = form.clientHeight - headerheight + "px";
			div = document.getElementById("PXFormView1_content2");
			div.style.height = form.clientHeight - headerheight + "px";
		}
	}
	
	var currentAnchor = -1;
	function tlbBtnClick(sender, e)
	{
		var div = document.getElementById("PXFormView1_content1");
		if(e.button.key == "nextdiff")
		{			
			if(currentAnchor >= document.anchors.length - 1)
				return;
			currentAnchor++;
			if(px.IsIE && currentAnchor == 0)
				currentAnchor++;
		}
		else if(e.button.key == "prevdiff")
		{
			if(currentAnchor <= 0)
				return;
			currentAnchor--;
		}
		else
			return; // some other button is pressed
		
		if(currentAnchor >= document.anchors.length)
			return;
		document.location.href = "#" + document.anchors[currentAnchor].name;
		if(!px.IsIE)
			div.scrollTop -= 10;
	}
	</script>

	<form id="form1" runat="server">
		<px_pt:PageTitle ID="usrCaption" runat="server" EnableTheming="true" />
		<px:PXToolBar ID="PXToolBar1" runat="server" Width="100%">
			<ClientEvents ButtonClick="tlbBtnClick" />
			<Items>
				<px:PXToolBarButton Tooltip="Previous Difference" Key="prevdiff">
					<Images Normal="main@PagePrev" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Tooltip="Next Difference" Key="nextdiff">
					<Images Normal="main@PageNext" />
				</px:PXToolBarButton>
				<px:PXToolBarButton Text="Statistics..." PopupPanel="PXSmartPanel1">
				</px:PXToolBarButton>
			</Items>
		</px:PXToolBar>
		<px:PXFormView ID="PXFormView1" runat="server" CaptionVisible="False" Height="150px"
			Style="position: static; margin-top: 5px; margin-bottom: 5px; padding-left: 5px;
			padding-right: 5px" Width="100%" AllowFocus="False">
			<Template>
				<div style="text-align: left; width: 100%; white-space: nowrap; margin-bottom: 3px">
					<px:PXLabel ID="PXLabel1" runat="server" Text="Deleted content: "></px:PXLabel>
					<span style="background-color: #FF7863">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span>
					&nbsp;&nbsp;
					<px:PXLabel ID="PXLabel2" runat="server" Text="Added content: "></px:PXLabel>
					<span style="background-color: #54C954">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span>
					&nbsp;&nbsp;
					<px:PXLabel ID="PXLabel3" runat="server" Text="Changed content: "></px:PXLabel>
					<span style="background-color: #75C5FF">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</span>
				</div>
				<table cellpadding="0px" cellspacing="0px" border="0px" style="width: 100%; table-layout: fixed;">
					<tr>
						<td class="FormRoundL" style="height: 20px">
							&nbsp;
						</td>
						<td class="FormRoundM" style="text-align: left; vertical-align: middle; overflow: hidden;
							height: 20px">
							<px:PXLabel ID="ver1" runat="server" Text="id1"></px:PXLabel>
						</td>
						<td class="FormRoundR" style="height: 20px">
							&nbsp;
						</td>
						<td class="FormRoundL" style="height: 20px">
							&nbsp;
						</td>
						<td class="FormRoundM" style="text-align: left; vertical-align: middle; overflow: hidden;
							height: 20px">
							<px:PXLabel ID="ver2" runat="server" Text="id2"></px:PXLabel>
						</td>
						<td class="FormRoundR" style="height: 20px">
							&nbsp;
						</td>
					</tr>
					<tr>
						<td id="rowCell" style="border-left: Solid 1px Gray; border-right: Solid 1px Gray;
							border-bottom: Solid 1px Gray" colspan="3">
							<div id="content1" runat="server" style="position: static; width: 100%; overflow: scroll;
								white-space: nowrap; background-color: White">
							</div>
						</td>
						<td style="border-right: Solid 1px Gray; border-bottom: Solid 1px Gray; height: 50px;"
							colspan="3">
							<div id="content2" runat="server" style="position: static; width: 100%; overflow: scroll;
								white-space: nowrap; background-color: White">
							</div>
						</td>
					</tr>
				</table>
			</Template>
			<AutoSize Container="Window" Enabled="True" MinHeight="150" MinWidth="350" />
			<ContentStyle BorderStyle="None">
			</ContentStyle>
		</px:PXFormView>
		<px_pf:PageFooter ID="usrFooter" runat="server" EnableTheming="true" />
		<px:PXSmartPanel ID="PXSmartPanel1" runat="server" Caption="Statistics" CaptionVisible="True"
			DesignView="Content" Height="254px" Style="position: static" Width="403px">
			<px:PXPanel ID="pnlStatLeft" runat="server" Caption="Left" Height="90px" Style="left: 7px;
				position: absolute; top: 7px" Width="185px">
				<table cellpadding="0" cellspacing="0" style="margin-top: 5px" width="100%">
					<tr>
						<td style="width: 80%; height: 20px">
							<px:PXLabel ID="PXLabel4" runat="server">Words :</px:PXLabel>
						</td>
						<td style="width: 20%; text-align: right; height: 20px">
							<px:PXLabel ID="lblWords1" runat="server">0</px:PXLabel>
						</td>
					</tr>
					<tr>
						<td style="width: 80%; height: 20px">
							<px:PXLabel ID="PXLabel6" runat="server">Characters (no spaces) :</px:PXLabel>
						</td>
						<td style="width: 20%; text-align: right; height: 20px">
							<px:PXLabel ID="lblChars1" runat="server">0</px:PXLabel>
						</td>
					</tr>
					<tr>
						<td style="width: 80%; height: 20px">
							<px:PXLabel ID="PXLabel8" runat="server">Characters (with spaces) :</px:PXLabel>
						</td>
						<td style="width: 20%; text-align: right; height: 20px">
							<px:PXLabel ID="lblCharsWithSpaces1" runat="server">0</px:PXLabel>
						</td>
					</tr>
					<tr>
						<td style="width: 80%; height: 20px">
							<px:PXLabel ID="PXLabel5" runat="server">Symbols :</px:PXLabel>
						</td>
						<td style="width: 20%; text-align: right; height: 20px">
							<px:PXLabel ID="lblSymbols1" runat="server">0</px:PXLabel>
						</td>
					</tr>
				</table>
			</px:PXPanel>
			<px:PXPanel ID="pnlStatRight" runat="server" Caption="Right" Height="90px" Style="left: 208px;
				position: absolute; top: 7px" Width="185px">
				<table cellpadding="0" cellspacing="0" style="margin-top: 5px" width="100%">
					<tr>
						<td style="width: 80%; height: 20px">
							<px:PXLabel ID="PXLabel10" runat="server">Words :</px:PXLabel>
						</td>
						<td style="width: 20%; text-align: right; height: 20px">
							<px:PXLabel ID="lblWords2" runat="server">0</px:PXLabel>
						</td>
					</tr>
					<tr>
						<td style="width: 80%; height: 20px">
							<px:PXLabel ID="PXLabel12" runat="server">Characters (no spaces) :</px:PXLabel>
						</td>
						<td style="width: 20%; text-align: right; height: 20px">
							<px:PXLabel ID="lblChars2" runat="server">0</px:PXLabel>
						</td>
					</tr>
					<tr>
						<td style="width: 80%; height: 20px">
							<px:PXLabel ID="PXLabel14" runat="server">Characters (with spaces) :</px:PXLabel>
						</td>
						<td style="width: 20%; text-align: right; height: 20px">
							<px:PXLabel ID="lblCharsWithSpaces2" runat="server">0</px:PXLabel>
						</td>
					</tr>
					<tr>
						<td style="width: 80%; height: 20px">
							<px:PXLabel ID="PXLabel7" runat="server">Symbols :</px:PXLabel>
						</td>
						<td style="width: 20%; text-align: right; height: 20px">
							<px:PXLabel ID="lblSymbols2" runat="server">0</px:PXLabel>
						</td>
					</tr>
				</table>
			</px:PXPanel>
			<px:PXButton ID="btnClose" runat="server" Style="left: 314px; position: absolute;
				top: 228px" Text="Close" Width="80px" DialogResult="OK">
			</px:PXButton>
			<px:PXPanel ID="PXPanel1" runat="server" Caption="Changes" Height="75px" Style="left: 7px;
				position: absolute; top: 122px" Width="387px">
				<table cellpadding="0" cellspacing="0" width="100%" style="margin-top: 5px">
					<tr>
						<td style="width: 25%">
							<table cellpadding="0" cellspacing="0" width="100%">
								<tr>
									<td style="width: 80%; height: 20px">
										<px:PXLabel ID="PXLabel9" runat="server">Lines updated :</px:PXLabel>
									</td>
									<td style="width: 20%; text-align: right; height: 20px">
										<px:PXLabel ID="lblLinesUpdated" runat="server">0</px:PXLabel>
									</td>
								</tr>
								<tr>
									<td style="width: 80%; height: 20px">
										<px:PXLabel ID="PXLabel13" runat="server">Lines added :</px:PXLabel>
									</td>
									<td style="width: 20%; text-align: right; height: 20px">
										<px:PXLabel ID="lblLinesAdded" runat="server">0</px:PXLabel>
									</td>
								</tr>
								<tr>
									<td style="width: 80%; height: 20px">
										<px:PXLabel ID="PXLabel16" runat="server">Lines deleted :</px:PXLabel>
									</td>
									<td style="width: 20%; text-align: right; height: 20px">
										<px:PXLabel ID="lblLinesDeleted" runat="server">0</px:PXLabel>
									</td>
								</tr>
							</table>
						</td>
						<td style="width: 10%">
						</td>
						<td style="width: 45%">
							<table cellpadding="0" cellspacing="0" width="100%">
								<tr>
									<td style="width: 80%; height: 20px">
										<px:PXLabel ID="PXLabel11" runat="server">Symbols in updated lines changed :</px:PXLabel>
									</td>
									<td style="width: 20%; text-align: right; height: 20px">
										<px:PXLabel ID="lblSymbolsUpdated" runat="server">0</px:PXLabel>
									</td>
								</tr>
								<tr>
									<td style="width: 80%; height: 20px">
										<px:PXLabel ID="PXLabel17" runat="server">Symbols in updated lines added :</px:PXLabel>
									</td>
									<td style="width: 20%; text-align: right; height: 20px">
										<px:PXLabel ID="lblSymbolsAdded" runat="server">0</px:PXLabel>
									</td>
								</tr>
								<tr>
									<td style="width: 80%; height: 20px">
										<px:PXLabel ID="PXLabel19" runat="server">Symbols in updated lines deleted :</px:PXLabel>
									</td>
									<td style="width: 20%; text-align: right; height: 20px">
										<px:PXLabel ID="lblSymbolsDeleted" runat="server">0</px:PXLabel>
									</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</px:PXPanel>
		</px:PXSmartPanel>
	</form>

	<script language="javascript" type="text/javascript">
		
	PXFormView.prototype.onAfterResize = function()
	{
		adjustTableSize();
	}
	
	</script>

</body>
</html>
