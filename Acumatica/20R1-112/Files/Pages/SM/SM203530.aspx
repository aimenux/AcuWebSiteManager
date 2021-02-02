<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM203530.aspx.cs" Inherits="Page_SM203530" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <script type="text/javascript">
        function commandResult(ds, context) {
            var grid = px_all[grdCompaniesID];
            var row = null;

            if (context.command == "MoveCompanyDown")
                row = grid.activeRow.nextRow();
            else if (context.command == "MoveCompanyUp")
                row = grid.activeRow.prevRow();
            if (row)
                row.activate();
        }
    </script>
    <px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="Companies" 
		TypeName="PX.SM.CompanyInquire">
        <ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="InsertCompanyCommand" CommitChanges="True" Visible="false" DependOnGrid="gridCompanies" />			
            <px:PXDSCallbackCommand Name="MoveCompanyUp" Visible="false" DependOnGrid="gridCompanies" />
            <px:PXDSCallbackCommand Name="MoveCompanyDown" Visible="false" DependOnGrid="gridCompanies" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="UPCompany_View" Visible="False" DependOnGrid="gridCompanies" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="gridCompanies" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="true" AdjustPageSize="Auto" DataSourceID="ds" SkinID="Inquire">
		<Levels>
			<px:PXGridLevel DataMember="Companies">
				<Columns>
					<px:PXGridColumn AllowUpdate="False" DataField="Current" Label="Is Current" TextAlign="Center" Type="CheckBox" Width="60px" />
					<px:PXGridColumn DataField="CompanyID" Label="Company ID" TextAlign="Right" LinkCommand="UPCompany_View" />
					<px:PXGridColumn AllowUpdate="False" DataField="CompanyCD" Label="Company CD" />
					<px:PXGridColumn DataField="LoginName" Label="Login Name" Width="300px" />
					<px:PXGridColumn DataField="Status" Label="Status" Width="150px" Type="DropDownList" />
				</Columns>
			</px:PXGridLevel>
		</Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <ActionBar DefaultAction="View">
            <Actions>
                <ExportExcel Enabled="False" />
            </Actions>
            <CustomItems>
				<px:PXToolBarButton Text="Insert Company">
				    <AutoCallBack Command="InsertCompanyCommand" Target="ds" />
				    <PopupCommand Command="Refresh" Target="gridCompanies" />
				</px:PXToolBarButton>
                <px:PXToolBarButton ImageSet="main" ImageKey="ArrowUp">
                    <AutoCallBack Command="MoveCompanyUp" Target="ds" />                    
                </px:PXToolBarButton>
                <px:PXToolBarButton ImageSet="main" ImageKey="ArrowDown">
                    <AutoCallBack Command="MoveCompanyDown" Target="ds" />                    
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="View" Key="View" Enabled="true">
                    <AutoCallBack Command="UPCompany_View" Target="ds" />
                    <PopupCommand Command="Refresh" Target="gridCompanies" />
                    <ActionBar GroupIndex="0" Order="0" />
                </px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<Mode AllowAddNew="false" AllowUpdate="false" AllowDelete="false" />
	</px:PXGrid>
</asp:Content>
