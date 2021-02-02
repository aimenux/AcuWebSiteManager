<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS205500.aspx.cs" Inherits="Page_FS205500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" 
        TypeName="PX.Objects.FS.SrvManagementEmployeeMaint" 
        PrimaryView="SrvManagementStaffRecords">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="addEmployee" Visible="False" RepaintControls="All" ></px:PXDSCallbackCommand>       
            <px:PXDSCallbackCommand Name="addVendor" Visible="False" RepaintControls="All" ></px:PXDSCallbackCommand> 
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="gridEmployees" runat="server" Height="400px" Width="100%" Style="z-index: 100"
		AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" 
        SkinID="Inquire" TabIndex="800">
		<Levels>
			<px:PXGridLevel DataMember="SrvManagementStaffRecords">
			    <RowTemplate>
                    <px:PXSegmentMask ID="edAcctCD" runat="server" DataField="AcctCD">
                    </px:PXSegmentMask>
                    <px:PXTextEdit ID="edAcctName" runat="server" DataField="AcctName">
                    </px:PXTextEdit>
                    <px:PXDropDown ID="edType" runat="server" DataField="Type">
                    </px:PXDropDown>
                    <px:PXSegmentMask ID="edBAccountStaffMember__ParentBAccountID" runat="server" 
                        DataField="BAccountStaffMember__ParentBAccountID">
                    </px:PXSegmentMask>
                    <px:PXTextEdit ID="edContact__EMail" runat="server" DataField="Contact__EMail">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edContact__Phone1" runat="server" 
                        DataField="Contact__Phone1">
                    </px:PXTextEdit>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="AcctCD" LinkCommand="Employee_ViewDetails">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AcctName">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Type">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="BAccountStaffMember__ParentBAccountID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Contact__EMail">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Contact__Phone1">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
        <ActionBar ActionsText="False" PagerVisible="False">
            <CustomItems>
                <px:PXToolBarButton Text="Add Employee">
                    <AutoCallBack Command="addEmployee" Target="ds" ></AutoCallBack>
                    <PopupCommand Command="Refresh" Target="grid" ></PopupCommand>                                                                        
                </px:PXToolBarButton>
                <px:PXToolBarButton Text="Add Vendor">
                    <AutoCallBack Command="addVendor" Target="ds" ></AutoCallBack>
                    <PopupCommand Command="Refresh" Target="grid" ></PopupCommand>                                                                        
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" ></Mode>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
