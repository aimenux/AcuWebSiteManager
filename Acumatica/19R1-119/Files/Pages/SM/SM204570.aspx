<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master"
	AutoEventWireup="true" CodeFile="SM204570.aspx.cs" Inherits="Pages_SM_SM204570"
	EnableViewState="False" EnableViewStateMac="False" ValidateRequest="False" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" runat="Server">
	<style type="text/css">
		.phDS
		{
			display: none;
		}
	</style>
	<px:PXDataSource ID="ds" runat="server" Visible="False" Width="100%" PrimaryView="Filter"
		TypeName="PX.SM.SourceBrowser" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="actionConvertPage" Visible="False" />
			<px:PXDSCallbackCommand Name="actionViewFile" Visible="False" RepaintControls="Bound" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView runat="server" ID="FormHidden" Style="display: none" DataSourceID="ds"
		DataMember="Filter" Width="100%" AllowAutoHide="False">
		<Template>
			<%--			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="L" />
			<px:PXTextEdit SuppressLabel="True" ID="edGeneratedDacSource" runat="server" DataField="GeneratedDacSource"
				TextMode="MultiLine" ReadOnly="True" />

			<px:PXTextEdit SuppressLabel="True" ID="edSourceFile" runat="server" DataField="SourceFile"
				TextMode="MultiLine" ReadOnly="True" />
			<px:PXTextEdit SuppressLabel="True" ID="EditGraphSource" runat="server" DataField="ReadonlyEventSource"
				Height="99px" TextMode="MultiLine" Font-Names="Courier New" Font-Size="10pt"
				ReadOnly="True" Wrap="False" SelectOnFocus="False">
				<Padding Left="10px" />
			</px:PXTextEdit>
			<px:PXTextEdit SuppressLabel="True" ID="edAspxCode" runat="server" DataField="AspxCode"
				TextMode="MultiLine" ReadOnly="True" />
			<px:PXTextEdit SuppressLabel="True" ID="EditTableOfContent" runat="server" DataField="TableOfContent"
				ReadOnly="True" />--%>

			<px:PXTextEdit SuppressLabel="True" ID="ActiveTabIndex" runat="server" DataField="ActiveTabIndex" ReadOnly="True" />
			<%--<px:PXTextEdit SuppressLabel="True" ID="SelectLocation" runat="server" DataField="SelectLocation" ReadOnly="True" />--%>
		</Template>
		<ClientEvents Initialize="InitEventEditor" />
	</px:PXFormView>
	<script type="text/javascript">
		//var px_all2;
		//function IndexObjects() {
		//	if (px_all2)
		//		return;
		//	px_all2 = {};
		//	for (var n in px_all) {
		//		var names = n.split("_");
		//		var s = names[names.length - 1];
		//		px_all2[s] = px_all[n];
		//	}
		//}

		//function GetObject(id) {
		//	IndexObjects();
		//	return px_all2[id];
		//}
		var IsInitEvents = false;
		function InitEventEditor(a, b) {
			
			if (IsInitEvents)
				return;

			IsInitEvents = true;
			a.events.addEventHandler("afterRepaint", UpdateSourceCode);

		}
		
		function SubscribeEventsFindFiles(a, b)
		{
			
			a.events.addEventHandler("afterRepaint", ShowSelectedLine);
			
		}
		
		function ShowSelectedLine()
		{
			var e = document.getElementById("SelectedLine");
			if (!e)
				return;

			expandParentRegions("SelectedLine");
			e.scrollIntoView();
		}

		//function UpdateCode(srcId, destId) {

		//	var target = document.getElementById(destId);
		//	if (!target) {
		//		alert("SourceCodePlaceholder not found " + destId);
		//		return;
		//	}
		//	var src = GetObject(srcId);
		//	if (!src) {
		//		alert("Edit control not found " + srcId);
		//		return;
		//	}

		//	var html = src.getValue();
		//	if (html == null)
		//		html = "";

		//	if ("outerHTML" in target) {
		//		var tag = target.nodeName;
		//		target.innerHTML = "<span></span>";
		//		target.firstChild.outerHTML = "<" + tag + ">" + html + "</" + tag + ">";

		//	}
		//	else {
		//		target.innerHTML = html;

		//	}

		//}

		var IsTabInitCompleted = false;
		function SelectActiveTab() {
			if (IsTabInitCompleted)
				return;

			var target = px_alls["PXTab1"];
			if (!target) {
				//alert("PXTab1 not found ");
				return;
			}
			var src = px_alls["ActiveTabIndex"];
			if (!src) {
				alert("ActiveTabIndex control not found ");
				return;
			}

			var v = src.getValue();
			if (v)
				target.setSelectedIndex(parseInt(v));

			var e = document.getElementById("SelectedAspxLine");
			if (e)
			{
				e.scrollIntoView();
				
			}

			IsTabInitCompleted = true;
		}



		function UpdateSourceCode() {
			//debugger;
			SelectActiveTab();
			//UpdateCode("EditGraphSource", "Pre1");
			//UpdateCode("edGeneratedDacSource", "Pre2");
			//UpdateCode("edSourceFile", "Pre3");
			//UpdateCode("edAspxCode", "Pre4");

			//UpdateCode("EditTableOfContent", "Span1");
			//ActivateCsEditor();

			//	target = document.getElementById("Span1");
			//	html = document.getElementById(editTableOfContentId).value;
			//	target.innerHTML = html;


		}

		//function ControlAutoResize(elem, ax, x, ay, y) {

		//	px_cm.registerAutoSize({ element: elem, ID: elem.id },
		//{
		//	autoSize: {
		//		enabled: true
		//	, container: 0
		//	, valign: ay
		//	, bottom: y
		//	, align: ax
		//	, right: x
		//	, dockMethod: 2
		//	}
		//});

		//}


		function toggleRegion(targetId) {
			var link = document.getElementById("link" + targetId);
			var target = document.getElementById(targetId);
			var disp = target.style.display;
			var isHidden = (disp == "none");
			target.style.display = isHidden ? "" : "none";
			link.className = isHidden ? "csharpregion-start" : "csharpregion";

		}

		function expandParentRegions(id) {
			var target = document.getElementById(id);
			while (target && target.style) {
				if (target.style.display == "none" ) {
					toggleRegion(target.id);
					//target.style.display = "";

				}

				target = target.parentNode;
				if(target.position == "relative")
					break;
			}
		}



	</script>
	<px:PXTab ID="PXTab1" runat="server" DataMember="Filter" DataSourceID="ds" Width="100%"
		AutoRepaint="True" SelectedIndex="3"
		Style="bottom: 0px;"
		Height="800px"
		AllowFocus="False" RepaintOnDemand="False"  AllowAutoHide="False">

		<AutoSize Enabled="True" Container="Window" />
		<%--<ClientEvents Initialize="UpdateSourceCode" />--%>


		<Items>
			<px:PXTabItem Text="Screen Aspx">
				<Template>
					<px:PXPanel ID="PXPanel4" runat="server">
						<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="S" StartColumn="True" />
						<px:PXSelector ID="PXSelector1" runat="server" CommitChanges="True" DataField="ScreenID" />
						<%--						<px:PXButton runat="server" ID="BtnConvert">
							<AutoCallBack Target="ds" Command="actionConvertPage"/>
						</px:PXButton>--%>
					</px:PXPanel>
					<px:PXHtmlView runat="server" ID="AspxCode" DataField="AspxCode" Width="100%" Height="300px"
						Style="white-space: pre; font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px;">
						<AutoSize Enabled="True" Container="Parent"></AutoSize>



					</px:PXHtmlView>
					<%--		<div id="PageAspxCodeContainer" runat="server" style="right: 0px; margin: 9px; position: absolute; left: 0px; top: 40px; bottom: 0px; font-size: 10pt; font-family: 'Courier New', Monospace; background-color: white; border: gray 1px solid; padding: 5px; cursor: text; overflow: auto;">
						<pre id="Pre4"></pre>
					</div>--%>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Business Logic">
				<Template>
					<px:PXPanel ID="PXPanel3" runat="server">
						<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="S" StartColumn="True" />
						<px:PXSelector ID="PXSelector2" runat="server" CommitChanges="True" DataField="GraphName" DataSourceID="ds" AutoAdjustColumns="True" />
					</px:PXPanel>
					<px:PXSplitContainer AllowAutoHide="False" ID="SplitSource" runat="server" Orientation="Vertical" Width="100%" Style="bottom: 0px;">
						<AutoSize Enabled="True" />
						<Template1>
							<px:PXHtmlView runat="server" ID="TableOfContent" DataField="TableOfContent" Width="100%" Height="300px" Style="white-space: nowrap; overflow: auto;">
								<AutoSize Enabled="True" Container="Parent"></AutoSize>
							</px:PXHtmlView>

							<%--				<div id="TableOfContentBounds" runat="server"
								style="overflow: auto; padding-left: 10px; padding-top: 10px; position: absolute; left: 0px; top: 0px; right: 0px; bottom: 0px; border: gray 0px solid; background-color: white; text-decoration: none;">
								<span id="Span1"></span>
							</div>--%>
						</Template1>

						<Template2>
							<px:PXHtmlView runat="server" ID="ReadonlyEventSource" DataField="ReadonlyEventSource" Width="100%" Height="300px"
								Style="white-space: pre; font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px;">
								<AutoSize Enabled="True" Container="Parent"></AutoSize>
							</px:PXHtmlView>
							<%--		<div id="SourceCodeContainer" runat="server"
								style="width: 100%; height: 100%; font-size: 10pt; font-family: 'Courier New', Monospace; background-color: white; border: gray 0px solid; cursor: text; overflow: auto;">
								<pre id="Pre1"></pre>
							</div>--%>
						</Template2>
					</px:PXSplitContainer>


				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Data Access">
				<Template>
					<px:PXPanel ID="PXPanel2" runat="server">
						<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="S" StartColumn="True" />
						<px:PXSelector ID="PXSelector3" runat="server" CommitChanges="True"
							DataField="TableName" />
					</px:PXPanel>
					<px:PXHtmlView runat="server" ID="GeneratedDacSource" DataField="GeneratedDacSource" Width="100%" Height="300px"
						Style="white-space: pre; font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px;">
						<AutoSize Enabled="True" Container="Parent"></AutoSize>
					</px:PXHtmlView>
					<%--			<div id="dacCodeContainer" runat="server"
						style="position: absolute; left: 0px; top: 40px; right: 0px; bottom: 0px; margin: 9px; font-size: 10pt; font-family: 'Courier New', Monospace; background-color: white; border: gray 1px solid; padding: 5px; cursor: text; overflow: auto;">
						<pre id="Pre2"></pre>
					</div>--%>
					<%--					<px:ClientScript runat="server" ID="ClientScript22">
						ControlAutoResize(this, 0, 15, 0, 15);
					</px:ClientScript>--%>
				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Find in Files">
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server">
						<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="L" LabelsWidth="S" />
						<px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="FindText" CommitChanges="True" />
						<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" LabelsWidth="SM" />
						<px:PXButton ID="PXButton1" runat="server" Text="Find">
							<AutoCallBack Command="Save" Target="PXTab1" />
						</px:PXButton>




					</px:PXPanel>
					<px:PXSplitContainer runat="server" Orientation="Horizontal" ID="Splitter">
						<AutoSize Enabled="True" Container="Parent" />
						<Template1>
							<px:PXGrid runat="server" ID="FormFindResults" DataSourceID="ds" Width="100%" Height="100%"
							
								SkinID="DetailsInTab"
								AutoAdjustColumns="True"
								SyncPosition="True">
								<ActionBar ActionsVisible="False"></ActionBar>
								<AutoCallBack Enabled="True" Target="ds" Command="actionViewFile"></AutoCallBack>
								<Levels>
									<px:PXGridLevel DataMember="ViewFindResults">
										<Columns>
											<px:PXGridColumn DataField="Name" Width="250px"  />
											<px:PXGridColumn DataField="Line" Width="50px" />
											<px:PXGridColumn DataField="Content" Width="600px" />
										</Columns>

									</px:PXGridLevel>
								</Levels>
								<AutoSize Enabled="True" />
							</px:PXGrid>

						</Template1>
						<Template2>

							<px:PXFormView ID="PXFormView1" runat="server" DataMember="filter" SkinID="Transparent" Width="100%" Height="300px">
								<ClientEvents Initialize="SubscribeEventsFindFiles"/>
								<Template>

									<px:PXHtmlView runat="server" ID="SourceFile" DataField="SourceFile" Width="100%" Height="300px" Style="white-space: pre; font-family: 'Courier New', monospace; font-size: 10pt; line-height: 16px;">
										<AutoSize Enabled="True" Container="Parent"></AutoSize>
									</px:PXHtmlView>
								</Template>

								<AutoSize Enabled="True" Container="Parent" />
							</px:PXFormView>

						</Template2>

					</px:PXSplitContainer>

				</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Website Sources">
				<Template>
					<px:PXPanel ID="PXPanel1" runat="server">
						<px:PXLayoutRule runat="server" />
						<px:PXLabel runat="server" Text="Website will be packed to .zip file and sent to client. The file size is about 40mb. This action takes about 1 min to complete." />
						<px:PXButton ID="BtnDownload" runat="server" Text="Download Website">
							<AutoCallBack Target="ds" Command="actionRemoteChekout" />
						</px:PXButton>


					</px:PXPanel>
				</Template>
			</px:PXTabItem>
		</Items>

	</px:PXTab>



</asp:Content>
