<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Search.aspx.cs" EnableEventValidation="false" Inherits="Search_Search" ViewStateMode="Disabled" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Search</title>
	<link href="~/App_Themes/Search.css" type="text/css" rel="stylesheet" />
	<script language="javascript" type="text/javascript">
		function txtSearch_KeyDown(sender, e)
		{
			if (e.keyCode == 13)
			{
				px.cancelEvent(e);
				px.doPost(e.srcElement.getAttribute("name"), "");
				return true;

			}
		}
		function txtSearch_Blur(sender, e)
		{
			var el = document.getElementById("PXFormView1_cellList");
			el.style.display = 'none';
		}
		function btnShowList_Click(sender, e)
		{
			var el = document.getElementById("PXFormView1_cellList");
			el.style.display = (el.style.display == 'none') ? '' : 'none';

		}

	</script>
</head>
<body>
	<form id="form1" runat="server">
		<px_pt:PageTitle ID="pageTitle" runat="server" EnableTheming="true" />
		<div>
			<px:PXFormView ID="PXFormView1" runat="server" CaptionVisible="False" Width="100%" AllowFocus="False"
				PostChangesOnly="false">
				<Template>
					<div style="padding: 5px; position: static; border-style: none;">
						<table runat="server" style="position: static; margin-left: 5px; border-color: #ECE9E8; width: 600px;">
							<tr>
								<td>
									<table style="position: static; border-color: #ECE9E8; height: 20px;">
										<tr>
											<td style="padding-right: 5px; padding-top: 10px; vertical-align: top">
												<px:PXLabel runat="server" ID="lblSearch" Text="Search :" Style="white-space: nowrap; margin-left: 1px; font-weight: bold; vertical-align: top"></px:PXLabel>
											</td>
											<td style="height: 20px; padding-right: 5px; white-space: nowrap">
												<table runat="server" style="width: 500px;" cellspacing="0" cellpadding="0">
													<tr>
														<td>
															<table id="innerTable" class="searchBox">
																<tr>
																	<td>
																		<table runat="server" id="filtertable" class="activeFilter" cellspacing="0" cellpadding="0">
																			<tr>
																				<td>
																					<px:PXLabel runat="server" ID="lblActiveModule" /></td>
																				<td>
																					<px:PXButton runat="server" ID="btnClearActiveModule" CssClass="clearButton"
																						 ImageSet="control" ImageKey="ClearN" />
																				</td>
																			</tr>
																		</table>
																	</td>
																	<td style="width: 100%">
																		<px:PXTextEdit ID="txtSearch" runat="server" CssClass="editor search" MaxLength="255">
																			<ClientEvents KeyDown="txtSearch_KeyDown" Blur="txtSearch_Blur" />
																		</px:PXTextEdit>
																	</td>
																	<td>
																		<px:PXButton runat="server" ID="btnShowList" Text="&#x25BC" CssClass="showListButton">
																			<ClientEvents Click="btnShowList_Click" Blur="txtSearch_Blur"></ClientEvents>
																		</px:PXButton>
																	</td>
																</tr>
															</table>
														</td>
														<td>
															<px:PXButton runat="server" ID="btnSearch" AutoPostBack="true" Text="Go" 
																CssClass="Button search">
															</px:PXButton>
														</td>
													</tr>
													<tr>
														<td id="cellList" runat="server" class="comboList" style="display: none;" colspan="2"></td>
													</tr>
												</table>
											</td>
										</tr>
									</table>
								</td>
							</tr>
						</table>
					</div>
					<div runat="server" id="divMessage" class="noresults">
						<asp:Label ID="lblMessage" runat="server" ForeColor="Sienna" Visible="False"></asp:Label>
					</div>
					<div runat="server" id="divTips" visible="False" class="tips">
						<asp:Label ID="lblSearchTips" runat="server" />
						<ul id="tiplist" class="tipsList">
							<li id="liCheckSpelling" runat="server" />
							<li id="liSimplifyQuery" runat="server" />
							<li id="liTryOtherWords" runat="server" />
						</ul>
					</div>

					<px:PXSmartPanel runat="server" ID="pnlResults" SkinID="Frame">
						<table id="resultsTable" runat="server" width="100%" style="padding-left: 10px">
						</table>
						<div id="pager" runat="server" class="searchpager">
							<asp:LinkButton id="linkPrev" runat="server" visible="False" RenderAsButton="false"   OnCommand="linkPrev_Command" >&#8592 PREV</asp:LinkButton>
							<asp:LinkButton id="linkNext" runat="server" visible="False" OnCommand="linkNext_Command" >NEXT &#8594;</asp:LinkButton>
                            <asp:HiddenField ID="pages" runat="server" EnableViewState="false" />
						</div>
						<div id="fullTextWarning" runat="server" class="fulltextWarning">
							<asp:Label ID="lblFullTextWarning" runat="server" Visible="False" />
						</div>
					</px:PXSmartPanel>
				</Template>
				<AutoSize Enabled="true" Container="Window" />
				<ContentStyle BackColor="White" BorderStyle="None" ForeColor="Black">
				</ContentStyle>
			</px:PXFormView>
		</div>
		<px_pf:PageFooter ID="pageFooter" runat="server" EnableTheming="true" />
	</form>
</body>
</html>
