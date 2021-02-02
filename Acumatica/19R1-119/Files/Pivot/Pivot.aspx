<%@ Page Language="C#" MasterPageFile="~/MasterPages/Workspace.master" AutoEventWireup="true"
  CodeFile="Pivot.aspx.cs" Inherits="Page_Pivot" Title="ProjectX" %>

<%@ MasterType VirtualPath="~/MasterPages/Workspace.master" %>
<%@ Register assembly="PX.Olap" namespace="PX.Olap" tagprefix="cc1" %>
<asp:Content ID="contDS" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXPivotDataSource ID="ds" runat="server" Visible="false" Width="100%" PageLoadBehavior="PopulateSavedValues" PivotControlID="pt">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXPivotDataSource>

	<style type="text/css">
		.phDS	{	margin: 0px; }
	</style>

	<script type="text/javascript">
		function tlbPivot_ButtonClick(tlb, ev)
		{
			var pivotID = ev.button.key;
			if (pivotID)
			{
				var hf = px.elemByID("__pivotTableID"), loc = window.location;
				var oldID = px.getQueryParam("pivotTableID", loc.href), pivot = px_alls["pt"];

				var activeBtn = tlb.getButton(oldID.split('=')[1]);
				if (activeBtn) activeBtn.pivotState = pivot.viewState.elemState.value;

				hf.value = pivotID;
				pivot.reload(ev.button.pivotState);

				var query = loc.search.replace(oldID, "pivotTableID=" + pivotID);
				try
				{
					window.top.history.replaceState({}, document.title, query);
				}
				catch (ex) { }
			}
		}

		function handle_SaveAs(data)
		{
			var cm = __px_callback(window), pn = cm.resultXml.findNode("PivotTables");
			var pivots = pn.getAttribute("Tables").split(';'), tlb = px_alls["tlbPivot"];

			var selectedKey;
			while (tlb.items.length > 2)
			{
				if (tlb.items[1].getPushed()) selectedKey = tlb.items[1].key;
				tlb.removeItem(1);
			}

			for (var i = 0; i < pivots.length; i++)
			{
				var pair = pivots[i].split('|');
				var item = tlb.createButton(tlb.items.length - 1, pair[0], null, true, "1");
				item.key = pair[1];
				if (item.key == selectedKey) item.setPushed(true);
				if (i == 0) item.element.setAttribute("first-tab", "1");
			}
		}
	</script>
	<div class="toolBarTabWrap">
		<px:PXToolBar ID="tlbPivot" runat="server" SkinID="Tab">
			<Items>
				<px:PXToolBarLabel Width="10px" AllowHide="false" />
			</Items>
			<ClientEvents ButtonClick="tlbPivot_ButtonClick" />
		</px:PXToolBar>
	</div>
</asp:Content>
<asp:Content ID="c1" ContentPlaceHolderID="phDialogs" runat="Server">
	<px:PXPivotTable runat="server" ID="pt" ShowZeroValues="true" Width="100%" Height="500px" style="top: 0px; left: 0px" DataSourceID="ds">
		<AutoSize Enabled="true" Container="Window" />
		<ToolBarItemsTop>
			<px:PXToolBarButton ImageKey="Refresh" CommandName="Refresh" />
			<px:PXToolBarButton ImageKey="Excel" CommandName="ExportExcel" />
		</ToolBarItemsTop>
	</px:PXPivotTable>
	<%--<asp:SqlDataSource ID="SqlDS" runat="server" ConnectionString="Provider=Microsoft.Jet.OLEDB.4.0;Data Source=&quot;C:\Program Files (x86)\Perpetuum Software\SharpShooter OLAP\Resources\Data\NWIND.MDB&quot;" DataSourceMode="DataSet" ProviderName="System.Data.OleDb" SelectCommand="SELECT Categories.CategoryName, Products.ProductName, Suppliers.CompanyName, Suppliers.City, Suppliers.Country, [Order Details].Quantity, [Order Details].UnitPrice, Categories.CategoryID, [Order Details].OrderID, [Order Details].ProductID, Products.ProductID AS Expr1, Suppliers.SupplierID FROM ([Order Details] INNER JOIN ((Categories INNER JOIN Products ON Categories.CategoryID = Products.CategoryID) INNER JOIN Suppliers ON Products.SupplierID = Suppliers.SupplierID) ON [Order Details].ProductID = Products.ProductID)"></asp:SqlDataSource>--%>

    <px:PXSmartPanel ID="pnlSavePivotAs" runat="server" Caption="Save Pivot As..." CaptionVisible="True" AutoReload="True" AllowResize="False">
        <px:PXLayoutRule runat="server" StartColumn="True" Merge="True"/>
        <px:PXLabel runat="server" Size="SM">Pivot Name: </px:PXLabel>
        <px:PXTextEdit ID="txtPivotName" runat="server" Size="L" />
        <px:PXLayoutRule runat="server" />
        <px:PXPanel runat="server" SkinID="Buttons">
            <px:PXButton ID="pnlbtnSave" runat="server" DialogResult="OK" Text="Save" />
            <px:PXButton ID="pnlbtnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
