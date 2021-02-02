<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="IN401500.aspx.cs" Inherits="Page_IN401500"
    Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Header" TypeName="PX.Objects.IN.Matrix.Graphs.MatrixItemsStatusInquiry">
        <CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Visible="false" Name="MatrixGridCellChanged" DependOnGrid="MatrixMatrix" />
			<px:PXDSCallbackCommand CommitChanges="True" Visible="false" Name="ViewAllocationDetails" DependOnGrid="MatrixMatrix" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Header">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" />
            <px:PXSelector runat="server" DataField="TemplateItemID" Size="M" CommitChanges="True" ID="edTemplateItemID" AllowEdit="True" edit="1" />
            <px:PXSelector runat="server" DataField="ColAttributeID" Size="M" CommitChanges="True" ID="edColAttributeID" AutoRefresh="True" />
            <px:PXSelector runat="server" DataField="RowAttributeID" Size="M" CommitChanges="True" ID="edRowAttributeID" AutoRefresh="True" />
            <px:PXSelector runat="server" DataField="SiteID" Size="M" CommitChanges="True" ID="edSiteID" AllowEdit="True" edit="1" />
            <px:PXSelector runat="server" DataField="LocationID" Size="M" CommitChanges="True" ID="edLocationID" AutoRefresh="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" />
            <px:PXDropDown runat="server" ID="edDisplayPlanType" DataField="DisplayPlanType" Size="M" CommitChanges="True" AllowNull="False"  />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="MatrixAttributes" runat="server" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" Height="34px" Width="100%"
        AutoGenerateColumns="Recreate" RepaintColumns="True" GenerateColumnsBeforeRepaint="True" OnAfterSyncState="MatrixAttributes_AfterSyncState">
        <Levels>
            <px:PXGridLevel DataMember="AdditionalAttributes" />
        </Levels>
        <ActionBar ActionsText="True" ActionsVisible="False" />
        <Mode AllowAddNew="False" AllowDelete="False" />
    </px:PXGrid>
    <px:PXGrid ID="MatrixMatrix" runat="server" DataSourceID="ds" SkinID="Inquire" SyncPosition="True" Width="100%" AllowFilter="true"
        AutoGenerateColumns="Recreate" RepaintColumns="True" GenerateColumnsBeforeRepaint="True" OnAfterSyncState="MatrixMatrix_AfterSyncState">
        <Levels>
            <px:PXGridLevel DataMember="Matrix" />
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" />
		<ClientEvents AfterCellChange="matrixGrid_cellClick" />
    </px:PXGrid>

	<script type="text/javascript">
		// Sets column number when user clicks matrix cell
		var matrixGridOldColumnName = null;
		function matrixGrid_cellClick(grid, ev) {
			var ds = px_alls["ds"];
			var columnName = ev.cell.column.dataField;
			if (ds != null && columnName != null && (matrixGridOldColumnName == null || matrixGridOldColumnName != columnName)) {
				ds.executeCallback("MatrixGridCellChanged", columnName);
				matrixGridOldColumnName = columnName;
			}
		}
	</script>

</asp:Content>