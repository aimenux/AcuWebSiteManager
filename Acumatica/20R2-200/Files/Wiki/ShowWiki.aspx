<%@ Page Language="C#" MasterPageFile="~/MasterPages/Workspace.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="ShowWiki.aspx.cs" Inherits="Page_ShowWiki"
    Title="Untitled Page" %>

<%@ Register TagPrefix="a" Namespace="System.Windows.Forms" Assembly="System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" %>
<%@ MasterType VirtualPath="~/MasterPages/Workspace.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
<style type="text/css">
        /*.collapsible.collapsed .hide{ display:none;}
        .collapsible:not(.collapsed) .show{ display:none;}*/
	h1.wikiH1, h2.wikiH2 {
		padding-top: initial;
		margin-top: 1.6em;
		position:relative;
	}
	[collapserange].anim {
		transition: height 0.15s ease-in;
	}

	[collapserange].folded {
		display: none;
	}
	[collapserange] {
		overflow-y: hidden;
	}

	[collapse] .fold-arrow > span {
		background-repeat: no-repeat;
		display: block;
		width: 24px;
		height: 24px;
		position: absolute;
		left: 0px;
		/*top: -262px;*/
	}

	[collapse] > .fold-wrap > .fold-arrow {
		/*overflow: hidden;*/
		width: 20px;
		height: 20px;
		position: absolute;
		transition: transform 0.15s linear;
		/*transform: rotateZ(90deg);*/
	}
	.fold-wrap{
		display:inline-block;
		position:relative;
		width:0px;
	}
	.fold-arrow{
		padding-right:5px;
	}
	h1[collapse] > .fold-wrap > .fold-arrow {
		top: 3px;
	}
	h2[collapse] > .fold-wrap > .fold-arrow {
		top: 1px;
	}
	.filler{
		float: right;
		height: 25px;
		width: 125px;
		/*background: pink;*/
	}
	h1 > .jumptopedit, h1 > .jumptop, h1 > .editwiki, h1 > .editwikitop {
		top: 7px;
	}
	h2 > .jumptopedit, h2 > .jumptop, h2 > .editwiki, h2 > .editwikitop {
		top: 5px;
	}
	h1:not(:hover) > .jumptopedit, h1:not(:hover) > .jumptop, h1:not(:hover) > .editwiki, h1:not(:hover) > .editwikitop,
	h2:not(:hover) > .jumptopedit, h2:not(:hover) > .jumptop, h2:not(:hover) > .editwiki, h2:not(:hover) > .editwikitop {
		display:none;
	}

	.jumptopedit, .jumptop, .editwiki, .editwikitop{
		position: absolute;
		font-size: small;
		font-weight: normal;
	}
	.jumptopedit, .jumptop, .editwiki{
		right: 5px;
	}
	.editwikitop {
		right: 85px;
	}


	[collapse] .fold-arrow:not(.tilt) {
		transform: rotateZ(-180deg);
	}

	[collapse]{
		position: relative;
		overflow: hidden;
	}

	.toggle-all.expand span.collapse, .toggle-all:not(.expand) span.expand{
		display:none;
	}
	.toggle-all
	{
		cursor: pointer;
	}
	.fold-arrow > span {
	  background-image: url(data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI+PHBhdGggIGZpbGw9IiM4MDgwODAiIGQ9Ik0xMiAxNS43bC02LTYgMS40LTEuNCA0LjYgNC42IDQuNi00LjZMMTggOS43bC02IDZ6Ii8+PC9zdmc+);
	}

	.HintBox, .WarnBox, .DangerBox, .GoodPracticeBox
	{
        width: unset;
		border-radius: 10px;
		border: none;
		padding: 15px;
	}
	.WarnBox, .WarnBox table.GrayBoxContent td.boxcontent, .WarnBox table.GrayBoxContent td.warncell
	{
		background-color: #fef9eb;
	}
	.HintBox, .HintBox table.GrayBoxContent td.boxcontent, .HintBox table.GrayBoxContent td.hintcell
	{
		background-color: #ebf4fc;
	}
	.DangerBox, .DangerBox table.GrayBoxContent td.boxcontent, .DangerBox table.GrayBoxContent td.dangercell
	{
		background-color: #ffebeb;
	}
	.GoodPracticeBox, .GoodPracticeBox table.GrayBoxContent td.boxcontent, .GoodPracticeBox table.GrayBoxContent td.goodpracticecell
	{
		background-color: #eefbee;
	}
	.WarnBox div.text-BoxWarn:before
	{
		color: #f5a623;
	}
    .DangerBox div.text-BoxWarn:before
	{
		color: #ff0000;
	}
	.WarnBox .GrayBoxContent td.warncell, .HintBox .GrayBoxContent td.hintcell, .DangerBox .GrayBoxContent td.dangercell, .GoodPracticeBox .GrayBoxContent td.goodpracticecell
	{
		padding: 0px 10px 0 0;
		vertical-align: top;
	}
	.WarnBox p, .HintBox p, .DangerBox p, .GoodPracticeBox p
	{
		margin: 0;
	}
	.WarnBox .text-icon, .DangerBox .text-icon
	{
		height: 22px;
		width: 22px;
		font-size: 22px;
	}
	.HintBox i.ac-info
	{
		color: #0278d7;
		font-size: 22px;
	}	
	.GoodPracticeBox i.ac-check_circle
	{
		color: #2fc728;
		font-size: 22px;
	}

    </style>
		<script type="text/javascript">
			var getButtons = function ()
			{
			    if (!px_alls) return;
				var buttons = px_alls['ToolBar'].items;
				if (!buttons) return;
				var expandAll = buttons.filter(function (_) { return _.commandName == 'expandAll' }).pop();
				var collapseAll = buttons.filter(function (_) { return _.commandName == 'collapseAll' }).pop();
				return (expandAll && collapseAll) ? { 'expand': expandAll, 'collapse': collapseAll } : undefined;
			}
			var updateToggler = function ()
			{
				var buttons = getButtons();
				if (!buttons) return;

				if (document.body.querySelectorAll('.collapsible.collapsed').length)
				{
					buttons.expand.setVisible(true);
					buttons.collapse.setVisible(false);
				} else if (document.body.querySelectorAll('.collapsible:not(.collapsed)').length)
				{
					buttons.expand.setVisible(false);
					buttons.collapse.setVisible(true);
				}
			}
			
			var toggle = function (e)
			{
				var target;
				var parent = e.target.parentNode;
				if (e.target.nodeName == 'A' && e.target.classList.contains('anchorlink') && !e.target.classList.contains('wikilink'))
				{
					var hash = e.target.href.split('#').pop();
					if (!hash) return;
					var sibling = document.body.querySelector('#' + hash);
					if (!sibling) return;
					target = sibling.nextElementSibling;
					if (!target.classList.contains('collapsed')) return;
				}
				else target = e.target;
				while (target && !(target.getAttribute && target.getAttribute('collapse'))) target = target.parentNode;
				if (!target) return;
				var parentAttr = target.getAttribute('parentsec');
				if (parentAttr)
				{
					var parentHeader = document.querySelector('[collapse="' + parentAttr + '"]');
					if (parentHeader && parentHeader.classList.contains('collapsed'))
					{
						toggle({ 'target': parentHeader });
					}
				}
				var attr = target.getAttribute('collapse');
				var div = document.body.querySelector('[collapserange="' + attr + '"]');
				if (!div) return;
				if (div.classList.contains('anim')) return;
				var span = target.querySelector('.fold-arrow');
				if (target.classList.contains('collapsed'))
				{
					//target.scrollIntoView();
					div.classList.remove('folded');
					target.classList.remove('collapsed');
					var height = div.offsetHeight;
					div.style.height = "0px";
					div.classList.add('anim');
					span.classList.remove('tilt');
					setTimeout(function ()
					{
						div.style.height = height + "px"; //needed for chrome & firefox
						//span.classList.remove('tilt');
						setTimeout(function ()
						{
							div.classList.remove('anim');
							div.style.height = "";
							updateToggler();
						}, 100);
					}, 10);//should be greater than zero for firefox, zero is enough for chrome
				} else
				{
					div.classList.add('anim');
					div.style.height = div.offsetHeight + "px";
					setTimeout(function ()
					{
						div.style.height = "0px"; //needed for chrome & firefox
						span.classList.add('tilt');
						setTimeout(function ()
						{
							div.classList.add('folded');
							div.classList.remove('anim');
							div.style.height = "";
							target.classList.add('collapsed');
							updateToggler();
						}, 100);
					}, 10);//should be greater than zero for firefox, zero is enough for chrome
					Array.prototype.forEach.call(document.querySelectorAll('[parentsec="' + attr + '"]'),
						function (item)
						{
							if (!item.classList.contains('collapsed'))
							{
								toggle({ 'target': item });
							}
						});
					
				}
			};

			function initializeDataSource(sender, ev)
			{
				px_alls['form'].focus();
				px_alls['ToolBar'].events.addEventHandler('buttonClick', function (id, ev)
				{
					if (ev.button.commandName == 'expandAll')
					{
						var items = document.body.querySelectorAll('.collapsible.collapsed');
						Array.prototype.forEach.call(items, function (elem) { toggle({ 'target': elem }) });
						ev.cancel = true;
					}
					else if(ev.button.commandName == 'collapseAll')
					{
						var items = document.body.querySelectorAll('.collapsible:not(.collapsed)');
						Array.prototype.forEach.call(items, function (elem) { toggle({ 'target': elem }) });
						ev.cancel = true;
					}
				})
				document.body.addEventListener('click', toggle);
				__px_cm(window).registerAfterLoad(updateToggler);
			}
		</script>
    
    <px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.SM.WikiShowReader"
        PrimaryView="Pages" style="float: left">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="getFile" Visible="False" />
            <px:PXDSCallbackCommand Name="viewProps" Visible="False" />
            <px:PXDSCallbackCommand Name="checkOut" Visible="False" />
            <px:PXDSCallbackCommand Name="undoCheckOut" Visible="False" />
        </CallbackCommands>
        <ClientEvents Initialize="initializeDataSource" />
    </px:PXDataSource>

    <px:PXDataSource ID="dsTemplate" runat="server" Visible="False"
        PrimaryView="Pages" TypeName="PX.SM.WikiNotificationTemplateMaintenanceNoRefresh">
        <DataTrees>
            <px:PXTreeDataMember TreeKeys="Key" TreeView="EntityItems" />
        </DataTrees>
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="cancel" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>

    <px:PXToolBar ID="toolbar1" runat="server" SkinID="Navigation" ImageSet="main">
        <Items>
            <px:PXToolBarButton Key="print" Text="Print" Target="main" Tooltip="Print Current Article" ImageKey="Print" DisplayStyle="Text" />
            <px:PXToolBarButton Key="export" Text="Export" ImageKey="Export" DisplayStyle="Text">
                <MenuItems>
                    <px:PXMenuItem Text="Plain Text">
                    </px:PXMenuItem>
                    <px:PXMenuItem Text="Word">
                    </px:PXMenuItem>
                </MenuItems>
            </px:PXToolBarButton>
        </Items>
        <Layout ItemsAlign="Left" />
        <ClientEvents ButtonClick="onButtonClick" />
    </px:PXToolBar>
    <div style="clear: left" />

    <div id="Summary" runat="server">
        <px:PXFormView ID="PXFormView3" runat="server" CaptionVisible="False" Style="margin: 15px; padding-top: 15px; padding-left: 15px; padding-bottom: 15px; position: static; background-color: #22b14c;"
            Width="890px" AllowFocus="False" RenderStyle="Simple" Visible="False">
            <Template>
                <px:PXLabel ID="UserMessage" runat="server" Text="Chto-to" Style="position: static; color: white; font-size: 14pt; height: 60px;" />
            </Template>
        </px:PXFormView>

        <px:PXFormView ID="PXFormView1" runat="server" CaptionVisible="False" Style="position: static;"
            Width="925px" AllowFocus="False" RenderStyle="Simple">
            <Template>
                <div style="padding: 5px; position: static;">
                    <div style="border-style: none;">
                        <table style="position: static; margin-left: 5px; border-color: #ECE9E8; height: 60px;" width="auto">
                            <tr>
                                <td style="height: 60px; width: auto;">
                                    <table style="position: static; margin-left: 5px; border-color: #ECE9E8; height: 60px;" width="auto">
                                        <tr>
                                            <td>
                                                <px:PXLabel runat="server" ID="PXKB" Text="KB:" Style="font-size: 18pt; text-wrap: none; white-space: nowrap" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="height: 12px;">
                                                <px:PXLabel runat="server" ID="PXCategori" Text="Category:" Style="text-wrap: none; white-space: nowrap" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="height: 12px;">
                                                <px:PXLabel runat="server" ID="PXProduct" Text="Applies to:" Style="text-wrap: none; white-space: nowrap" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>

                                <td style="height: 60px; width: 100%;" />

                                <td style="height: 70px; width: auto; border: solid; border-color: black; border-width: thin; margin-right: 5px; padding-right: 5px;">
                                    <table style="position: static; margin-left: 5px; border-color: #ECE9E8; height: 82px; margin-right: 5px; padding-right: 5px;" width="auto">
                                        <tr>
                                            <td style="height: 12px;">
                                                <px:PXLabel runat="server" ID="PXKBName" Text="Article:" Style="text-wrap: none; white-space: nowrap" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="height: 12px;">
                                                <px:PXLabel runat="server" ID="PXCreateDate" Text="Created Date: " Style="text-wrap: none; white-space: nowrap" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="height: 12px;">
                                                <px:PXLabel runat="server" ID="PXLastPublished" Text="Last Modified:" Style="text-wrap: none; white-space: nowrap" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="height: 12px;">
                                                <px:PXLabel runat="server" ID="PXViews" Text="Views:" Style="text-wrap: none; white-space: nowrap" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="height: 12px;">
                                                <px:PXLabel runat="server" ID="PXRating" Text="Rating:" Style="text-wrap: none; white-space: nowrap" />
                                                <px:PXImage runat="server" ID="PXImage1" ImageUrl="main@FavoritesGray" />
                                                <px:PXImage runat="server" ID="PXImage2" ImageUrl="main@FavoritesGray" />
                                                <px:PXImage runat="server" ID="PXImage3" ImageUrl="main@FavoritesGray" />
                                                <px:PXImage runat="server" ID="PXImage4" ImageUrl="main@FavoritesGray" />
                                                <px:PXImage runat="server" ID="PXImage5" ImageUrl="main@FavoritesGray" />
                                                <px:PXLabel runat="server" ID="PXdAvRate" Text="" Style="text-wrap: none; white-space: nowrap" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </Template>
        </px:PXFormView>
    </div>

    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;"
        Width="100%" DataMember="Pages" DataKeyNames="PageID" SkinID="Transparent" NoteIndicator="False" FilesIndicator="False">
        <Searches>
            <px:PXQueryStringParam Name="PageID" QueryStringField="PageID" Type="String" />
            <px:PXQueryStringParam Name="Language" QueryStringField="Language" Type="String" />
            <px:PXQueryStringParam Name="PageRevisionID" QueryStringField="PageRevisionID" Type="Int32" />
            <px:PXQueryStringParam Name="Wiki" QueryStringField="Wiki" Type="String" />
            <px:PXQueryStringParam Name="Art" QueryStringField="Art" Type="String" />
            <px:PXQueryStringParam Name="Parent" QueryStringField="From" Type="String" />
            <px:PXControlParam ControlID="form" Name="PageID" PropertyName="NewDataKey[&quot;PageID&quot;]" Type="String" />
        </Searches>
        <AutoSize Enabled="True" Container="Window" />
    </px:PXFormView>

    <div id="Rating" runat="server">
        <px:PXFormView ID="PXFormView2" runat="server" CaptionVisible="False" Style="position: static;"
            Width="100%" AllowFocus="False">
            <Template>
                <div style="padding: 5px; position: static;">
                    <div style="border-style: none;">
                        <table style="position: static; border-color: #ECE9E8; height: 20px;" cellpadding="0" cellspacing="0" width="100% ">
                            <tr>
                                <td style="margin-left: 15px; height: 12px; width: 60px;">
                                    <px:PXLabel runat="server" ID="lblRate" Text="Rate this article :" Style="margin-left: 10px; text-wrap: none; white-space: nowrap" />
                                </td>

                                <td style="height: 20px;">
                                    <px:PXDropDown ID="Rate" runat="server" Style="height: 20px; width: 110px; margin-left: 10px" OnCallBack="ddRate_PageRate">
                                        <AutoCallBack Command="ddRate_PageRate">
                                        </AutoCallBack>
                                    </px:PXDropDown>
                                </td>

                                <td style="height: 20px; white-space: nowrap;">
                                    <px:PXButton ID="Button" runat="server" Style="height: 20px; margin-left: 10px;" Text="Rate!" OnCallBack="Rate_PageRate">
                                        <AutoCallBack Command="Rate_PageRate">
                                        </AutoCallBack>
                                    </px:PXButton>
                                </td>

                                <td style="height: 20px; width: 100%;" />

                                <td style="height: 20px; width: 60px;">
                                    <px:PXButton ID="PXButton2" runat="server" Style="height: 20px;" Text="Feedback" OnCallBack="Feedback_Rate">
                                        <AutoCallBack Command="Feedback_Rate">
                                        </AutoCallBack>
                                    </px:PXButton>
                                </td>
                            </tr>
                        </table>
                    </div>
                </div>
            </Template>
        </px:PXFormView>
    </div>

    <div id="dvAnalytics" runat="server">
    </div>

    <div id="SmartPanels" runat="server">
       <px:PXSmartPanel ID="pnlGetLink" runat="server" Caption="This article URL" ForeColor="Black"
            Height="117px" Style="position: static" Width="353px" Position="UnderOwner">
            <px:PXLabel ID="lblLink" runat="server" Style="position: absolute; left: 9px; top: 9px;"
                Text="Internal Link :"></px:PXLabel>
            <px:PXTextEdit ID="edtLink" runat="server" Style="position: absolute; left: 81px; top: 9px;"
                Width="256px">
            </px:PXTextEdit>
            <px:PXLabel ID="lblUrl" runat="server" Style="position: absolute; left: 9px; top: 36px;"
                Text="External Link :"></px:PXLabel>
            <px:PXTextEdit ID="edtUrl" runat="server" Style="position: absolute; left: 81px; top: 36px;"
                Width="256px">
            </px:PXTextEdit>
            <px:PXLabel ID="lblPublicUrl" runat="server" Style="position: absolute; left: 9px; top: 63px;"
                Text="Public Link :"></px:PXLabel>
            <px:PXTextEdit ID="edPublicUrl" runat="server" Style="position: absolute; left: 81px; top: 63px;"
                Width="256px">
            </px:PXTextEdit>
            <px:PXButton ID="PXButton1" runat="server" DialogResult="Cancel" Style="left: 263px; position: absolute; top: 90px; height: 20px;"
                Text="Close" Width="80px">
            </px:PXButton>
        </px:PXSmartPanel>
        <px:PXSmartPanel ID="pnlWikiText" runat="server" CaptionVisible="True" Height="544px"
            Style="position: static" Width="814px">
            <px:PXTextEdit ID="edWikiText" runat="server" Height="536px" Style="position: static; color: Black;"
                TextMode="MultiLine" Width="806px" ReadOnly="True">
            </px:PXTextEdit>
        </px:PXSmartPanel>
    </div>

    <script type="text/javascript">
        px_callback.baseProcessRedirect = px_callback.processRedirect;
        px_callback.processRedirect = function (result, context) {
            var flag = true;
            if (context == null) context = this.context;
            if (context != null && context.context != null)
                if (context.context.command == "delete") {
                    __refreshMainMenu(); flag = false;
                }
            if (flag) this.baseProcessRedirect(result, context);
        }

        function onButtonClick(sender, e) {
            if (e.button.key == "print") {
                // printLink is defined on server
                window.open(printLink, '',
                    'scrollbars=yes,height=600,width=800,resizable=yes,toolbar=no,location=no,status=no,menubar=yes');
            }
        }
    </script>
</asp:Content>
