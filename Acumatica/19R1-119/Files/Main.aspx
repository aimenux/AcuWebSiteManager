<%@ Page Language="C#" AutoEventWireup="true"EnableViewState="false" CodeFile="Main.aspx.cs" Inherits="_Main" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
	<title>Acumatica</title>
	<link rel="icon" type="image/png" href="~/Icons/site.png" />
	<link rel="shortcut icon" type="image/x-icon" href="~/Icons/site.ico" />
</head>
<body>
	<form id="form1" runat="server" autocomplete="off" enctype="multipart/form-data">
		<px:PXSmartPanel ID="panelT" runat="server" SkinID="Frame" Overflow="Hidden" AutoSize-Enabled="false">
			<table cellpadding="0" cellspacing="0" style="width:100%" class="toolSysTable">
				<tr>
				<td>
					<a id="logoCell" runat="server" class="logo" target="main">
						<asp:Image id="logoImg" runat="server" AlternateText="logo" ImageUrl="Icons/logo.png" CssClass="logoImg" />
					</a>
				</td>
				<td class="fill" style="overflow: hidden; min-width: 200px;">
					<div class="outerFill"><div class="innerFill">
						<px:PXToolBar runat="server" ID="systemsBar" SkinID="SystemMenu" CanOverflow="true">
							<ClientEvents ButtonClick="MainFrame.systemsBar_Click" />
						</px:PXToolBar>
					</div></div>
				</td>
				<td>
					<px:PXToolBar runat="server" ID="toolsBar" SkinID="ToolsMenu" ImageSet="main" 
						RenderAsTable="true" OnCallBack="toolsBar_OnCallBack">
						<Items>
							<px:PXToolBarButton Key="events" Text="0" />
							<px:PXToolBarButton Key="businessDate" PopupPanel="pnlDate" Tooltip="Click to set business date." />
							<px:PXToolBarButton Key="userName" />
						</Items>
					</px:PXToolBar>
				</td>
				</tr>
			</table>
			<px:PXToolBar runat="server" ID="modulesBar" SkinID="ModulesMenu" CanOverflow="true">
				<ClientEvents ButtonClick="MainFrame.modulesBar_Click" />
			</px:PXToolBar>
		</px:PXSmartPanel>

		<table id="frameT" runat="server" style="width:100%;height:100px" cellspacing="0" cellpadding="0">
			<tr>
				<td class="leftFrame" runat="server" style="width: 20%">
					<a runat="server" id="moduleLink" class="moduleLink" target="main" href="frames/default.aspx">Default</a>
					<div class="searchFrame">
						<px:PXSearchBox ID="searchBox" runat="server" Target="main" ButtonText="Search" AutoComplete="true" />
					</div>
					<div runat="server" id="divNavPanel" style="padding: 4px; display: none" />
					
					<px:PXSmartPanel ID="panelL" runat="server" SkinID="Frame" CssClass="menuFrameO">
							<px:PXSmartPanel ID="menuPanel" runat="server" SkinID="Frame" CssClass="menuFrameI">
								<px:PXToolBar ID="subModulesBar" runat="server" SkinID="SubModulesMenu">
									<ClientEvents ButtonClick="MainFrame.subModulesBar_Click" />
								</px:PXToolBar>
							</px:PXSmartPanel>
					</px:PXSmartPanel>
				</td>                
				<td>
					<px:PXSplitter ID="sp1" runat="server" SkinID="Frame" Style="height: 100%" Orientation="Vertical" 
						Panel1MinSize = "205" SaveSizeUnits="True" FixPanel1Size="true" Size="4">
						<AutoSize Enabled="true" Container="Window" />
					</px:PXSplitter>
				</td>

				<td class="rightFrame" style="width: 81%">
					<div style="position:relative">
						<div class="hideFrameBox" runat="server" ID="hideFrameBox" onclick="MainFrame.hideMenu_Click()" title="Hide Navigation Pane"> 
							<px:PXImage runat="server" ID="hideFrame" ImageUrl="control@HideFrame" />
						</div>
					</div>

					<px:PXSmartPanel ID="panelR" runat="server" SkinID="Frame" CssClass="SmartPanelF screenFrame"
						IFrameName="main" RenderIFrame="true" ClientEvents-AfterLoad="MainFrame.afterMainLoad" Overflow="Auto">
					</px:PXSmartPanel>
				</td>
			</tr>
		</table>

		<px:PXSmartPanel ID="pnlDate" runat="server" DesignView="Hidden"
			CaptionVisible="False" Position="UnderOwner" ActiveControlID="edEffDate" 
			AcceptButtonID="btnDateOk" CancelButtonID="btnDateCancel" Overflow="Hidden">
			<px:PXLayoutRule runat="server" ID="ruleBD1" Merge="true" />
			<px:PXLabel ID="lblEffDate" runat="server" Text="Business Date:" Size="S" />
			<px:PXDateTimeEdit ID="edEffDate" runat="server" BorderColor="SteelBlue" BorderStyle="Solid"
				BorderWidth="1px" TabIndex="1" AllowNull="False" Size="S">
			</px:PXDateTimeEdit>
			<px:PXLayoutRule runat="server" ID="ruleBD2" />

			<px:PXPanel runat="server" ID="pnlDateButtons" SkinID="Buttons">
				<px:PXButton ID="btnDateOk" runat="server" Text="OK" Width="72px" TabIndex="2" DialogResult="OK" 
					AlignLeft="true" OnCallBack="onSetDate_CallBack" >
					<AutoCallBack Command="SetDate" Handler="MainFrame.onSetDateBusinessDate">
						<Behavior PostData="Container" ContainerID="pnlDate" />
					</AutoCallBack>
				</px:PXButton>
				<px:PXButton ID="btnDateCancel" runat="server" Text="Cancel" Width="72px" TabIndex="3" 
					DialogResult="Cancel" AlignLeft="true" />
			</px:PXPanel>
		</px:PXSmartPanel>
	</form>
</body>
</html>
