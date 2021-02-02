<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="GenericInquiry.aspx.cs" Inherits="Page_GenericInquiry"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXGenericDataSource ID="ds" runat="server" Visible="True" Width="100%" FormID="form" GridID="grid" PageLoadBehavior="PopulateSavedValues">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" PostData="Self" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True"  />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
			<px:PXDSCallbackCommand Name="Insert" Visible="true" HideText="true" PopupCommand="Refresh" PopupCommandTarget="grid" />
			<px:PXDSCallbackCommand Name="Delete" HideText="true" />
			<px:PXDSCallbackCommand Name="editDetail" Visible="true" DependOnGrid="grid" />
			<px:PXDSCallbackCommand Name="actionsMenu" CommitChanges="true" />
			<px:PXDSCallbackCommand Name="Update" CommitChanges="true" Visible="false" />
			<px:PXDSCallbackCommand Name="Update All" CommitChanges="true" Visible="false" />
		</CallbackCommands>
	</px:PXGenericDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXGenericFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="Filter">
		<Parameters>
			<px:PXQueryStringParam Name="id" OnLoadOnly="True" QueryStringField="ID" Type="String" />
			<px:PXQueryStringParam Name="name" OnLoadOnly="True" QueryStringField="Name" Type="String" />
		</Parameters>
	</px:PXGenericFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<%--Do not change Grid ID, it's used in the data source explicitly--%> 
	<px:PXGrid ID="grid"
		runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" SkinID="PrimaryInquire"
		SyncPosition="True" SyncPositionWithGraph="True" AllowSearch="True" PreserveSortsAndFilters="True" PreservePageIndex="True"
		ShowFilterToolbar="true" AllowPivotTable="true" EditPivotTableUrl="~/Pages/SM/SM208020.aspx">
		<ActionBar PagerVisible="Bottom" DefaultAction="editDetail">
			<PagerSettings Mode="NumericCompact" />
			<CustomItems>
				<px:PXToolBarButton Key="editDetail" Visible="false">
					<AutoCallBack Command="editDetail" Target="ds" />
					<ActionBar GroupIndex="0" MenuVisible="false" />
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="Results">
				<Layout FormViewHeight=""></Layout>
				<Columns>
					<px:PXGridColumn AllowCheckAll="True" AllowShowHide="True" DataField="Selected"
						TextAlign="Center" Type="CheckBox" Width="40px" AllowSort="false" AllowMove="false" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
	
	<%-- Dialog for entering keys for a new record. --%>
	<px:PXSmartPanel ID="dlgEnterKeys" runat="server" Width="500px" Height="350px" Caption="Enter Keys" 
		CaptionVisible="True" Key="AddNewKeys" LoadOnDemand="True" ShowAfterLoad="True" AutoRepaint="True">
		<px:PXPanel ID="formPanel" runat="server" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
			<px:PXLayoutRule runat="server" StartColumn="True" />
			<px:PXLabel runat="server" ID="lblDlgCaption" Width="370px" Style="font-weight: bold">Please enter the keys for a new record.</px:PXLabel>
		</px:PXPanel>

		<px:PXGrid ID="grdKeys" runat="server" Height="250px" Width="100%" DataSourceID="ds" 
			AutoAdjustColumns="true" MatrixMode="True" SkinID="ShortList">
			<AutoSize Enabled="true" />
			<CallbackCommands>
				<Save CommitChanges="true" CommitChangesIDs="grdKeys" />
			</CallbackCommands>
			<Levels>
				<px:PXGridLevel DataMember="AddNewKeys">
					<Mode AllowAddNew="false" AllowDelete="false" AllowSort="false" InitNewRow="false" />
					<Layout ColumnsMenu="False" />
					<Columns>
						<px:PXGridColumn DataField="FieldName" />
						<px:PXGridColumn DataField="DisplayName" Width="150px" AllowShowHide="false" />
						<px:PXGridColumn DataField="Value" Width="190px" RenderEditorText="true" AutoCallBack ="True" AllowShowHide="false" />
					</Columns>
				</px:PXGridLevel>
			</Levels>
		</px:PXGrid>
		
		<px:PXPanel ID="btnsPanel" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnCancelAddNew" runat="server" DialogResult="Cancel" Text="Cancel" />
			<px:PXButton ID="btnFinishAddNew" runat="server" DialogResult="OK" Text="Finish" />
		</px:PXPanel>
	</px:PXSmartPanel>
	
	<%-- Mass Updates --%>
	<px:PXSmartPanel ID="dlgUpdateParams" runat="server" Width="500px" Height="350px" Caption="Values for Update" 
		CaptionVisible="True" Key="Fields" LoadOnDemand="True" ShowAfterLoad="true" AutoRepaint="True" >
		<%--AutoCallBack-Enabled="true" AutoCallBack-Target="grdFields" AutoCallBack-Command="Refresh"
		CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"	>--%>
        
		<px:PXPanel runat="server" RenderStyle="Simple" ContentLayout-OuterSpacing="Around">
			<px:PXLayoutRule runat="server" StartColumn="True" />
			<px:PXLabel runat="server" ID="lblMassUpdatesCaption" Width="370px" Style="font-weight: bold">Please select the fields you want to update.</px:PXLabel>
		</px:PXPanel>            

        <px:PXGrid ID="grdFields" runat="server" Height="250px" Width="100%" DataSourceID="ds" AutoAdjustColumns="true" MatrixMode="True">
            <CallbackCommands>
				<Save CommitChanges="true" CommitChangesIDs="grdFields" RepaintControls="None" RepaintControlsIDs="grdFields" />
				<FetchRow RepaintControls="None" />
			</CallbackCommands>
			<AutoSize Enabled="true" />
            <Levels>
                <px:PXGridLevel DataMember="Fields">
                    <Columns>
                        <px:PXGridColumn DataField="Selected" Width="30px" Type="CheckBox" AllowSort="false" AllowCheckAll="true" TextAlign="Center" CommitChanges="True" />
						<px:PXGridColumn DataField="FieldName" />
						<px:PXGridColumn DataField="DisplayName" Width="150px" />
                        <px:PXGridColumn DataField="Value" Width="190px" RenderEditorText="true" AutoCallBack="True" AllowShowHide="false" CommitChanges="true" />
                    </Columns>
                    <Layout ColumnsMenu="False" />
                    <Mode AllowAddNew="false" AllowDelete="false" />
                </px:PXGridLevel>
            </Levels>
            <ActionBar>
                <Actions>
                    <ExportExcel Enabled="False" />
                    <AddNew Enabled="False" />
                    <FilterShow Enabled="False" />
                    <FilterSet Enabled="False" />
                    <Save Enabled="False" />
                    <Delete Enabled="False" />
                    <NoteShow Enabled="False" />
                    <Search Enabled="False" />
                    <AdjustColumns Enabled="False" />
                </Actions>
            </ActionBar>
        </px:PXGrid>
		
		<px:PXPanel ID="massUpdatesBtnsPanel" runat="server" SkinID="Buttons">
			<px:PXButton ID="btnCancelMassUpdates" runat="server" DialogResult="Cancel" Text="Cancel" />
			<px:PXButton ID="btnFinishMassUpdates" runat="server" DialogResult="OK" Text="Finish" />
		</px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
