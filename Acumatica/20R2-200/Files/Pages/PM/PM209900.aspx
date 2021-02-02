<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM209900.aspx.cs"
    Inherits="Page_PM209900" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.LaborCostRateMaint" PrimaryView="Filter"
        BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Filter">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" ColumnWidth="L" />
            <px:PXDropDown CommitChanges="True" ID="edType" runat="server" DataField="Type" />
            <px:PXSelector CommitChanges="True" ID="edProject" runat="server" DataField="ProjectID" />
            <px:PXSelector CommitChanges="True" ID="edTaskID" runat="server" DataField="TaskID" AutoRefresh="true" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edDate" runat="server" DataField="EffectiveDate" />
            <px:PXLayoutRule runat="server" StartColumn="True" ColumnWidth="L"/>
            <px:PXSelector CommitChanges="True" ID="edEmployee" runat="server" DataField="EmployeeID" />
            <px:PXSelector CommitChanges="True" ID="edInventory" runat="server" DataField="InventoryID" />
            <px:PXSelector CommitChanges="True" ID="edUnion" runat="server" DataField="UnionID" />           
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        SkinID="Details" Caption="Positions">
        <Levels>
            <px:PXGridLevel DataMember="Items">            
                <Columns>
                    <px:PXGridColumn DataField="Type" AutoCallBack="True" Type="DropDownList" />
                    <px:PXGridColumn DataField="UnionID" AutoCallBack="True" />
                    <px:PXGridColumn DataField="ProjectID" AutoCallBack="True" />
                    <px:PXGridColumn DataField="TaskID" AutoCallBack="True" />
                    <px:PXGridColumn DataField="EmployeeID" AutoCallBack="True" />
                    <px:PXGridColumn  DataField="EmployeeID_description" />
				    <px:PXGridColumn DataField="InventoryID" AutoCallBack="True" />
                    <px:PXGridColumn DataField="Description" />
                    <px:PXGridColumn DataField="EmploymentType" AutoCallBack="True" />
                    <px:PXGridColumn DataField="RegularHours" AutoCallBack="True" />
                    <px:PXGridColumn DataField="AnnualSalary" AutoCallBack="True" />
                    <px:PXGridColumn DataField="UOM"/>
                    <px:PXGridColumn DataField="Rate" />
                    <px:PXGridColumn DataField="CuryID" />
                    <px:PXGridColumn DataField="ExtRefNbr" />
                    <px:PXGridColumn DataField="EffectiveDate"/>
                </Columns>
                <RowTemplate>
                    <px:PXSegmentMask ID="edInventoryIDGrid" runat="server" DataField="InventoryID" />
                </RowTemplate>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowUpload="true" InitNewRow="true" />
    </px:PXGrid>
</asp:Content>
