<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SP700003.aspx.cs" Inherits="Pages_SP700003"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="true" Width="100%" TypeName="SP.Objects.IN.SOOrderInquire"
		PrimaryView="Items">
        <CallbackCommands>
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="PrintOrder" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewShipment" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="Reorder" CommitChanges="true" />
			<px:PXDSCallbackCommand DependOnGrid="grid" Name="CancelOrder" CommitChanges="true" RepaintControls="All"
				RepaintControlsIDs="grid" PopupCommand="Refresh" PopupCommandTarget="grid" StateColumn="IsEnabledCancelOrder" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phF" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" ActionsPosition="Top" 
        AllowPaging="true" AdjustPageSize="auto" SkinID="PrimaryInquire" FastFilterFields="OrderNbr" FilesIndicator="False" 
        NoteIndicator="False" SyncPosition="true">
		<Levels>
			<px:PXGridLevel DataMember="Items">
				<Columns>
				    <px:PXGridColumn DataField="IsEnabledCancelOrder" Width="100px" Visible="False"/>
                    <px:PXGridColumn DataField="OrderNbr" Width="100px" LinkCommand="PrintOrder"/>
                    <px:PXGridColumn DataField="Status" Width="100px" />
                    <px:PXGridColumn DataField="OrderDate" Width="160px" />
                    <px:PXGridColumn DataField="RequestDate" Width="160px" />
                    <px:PXGridColumn DataField="OrderQty" Width="60px"/>
                    <px:PXGridColumn DataField="CuryOrderTotal" Width="60px"/>
                    <px:PXGridColumn DataField="CuryID" Width="100px"/>
                    <px:PXGridColumn DataField="CustomerLocationID" Width="100px" />
                    <px:PXGridColumn DataField="CreatedByIDForFilter" Width="100px" Visible="false"/>
                    <px:PXGridColumn DataField="Users__FullName" Width="100px" />
                    <px:PXGridColumn DataField="SOOrderType__Descr" Width="100px" />
                    <%--<px:PXGridColumn DataField="Contact__UserID" Width="100px" />--%>
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" />
		<ActionBar DefaultAction="cmdPrintOrder" PagerVisible="False">
			<PagerSettings Mode="NextPrevFirstLast" />
			<CustomItems>
				<px:PXToolBarButton Key="cmdPrintOrder" Visible="false">
					<AutoCallBack Command="PrintOrder" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	
				</px:PXToolBarButton>
                
                <px:PXToolBarButton Key="cmdViewShipment" Visible="false">
					<AutoCallBack Command="ViewShipment" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	
				</px:PXToolBarButton>

                <px:PXToolBarButton Key="cmdViewShipment" Visible="false">
					<AutoCallBack Command="Reorder" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	
				</px:PXToolBarButton>
                
                <px:PXToolBarButton Key="cmdCancelOrder" DependOnGrid="grid" StateColumn="IsEnabledCancelOrder" Visible="false">
					<AutoCallBack Command="CancelOrder" Target="ds">
						<Behavior CommitChanges="True" />
					</AutoCallBack>	
				</px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<CallbackCommands>
			<Refresh CommitChanges="True" PostData="Page" />
		</CallbackCommands>
	</px:PXGrid>
</asp:Content>


