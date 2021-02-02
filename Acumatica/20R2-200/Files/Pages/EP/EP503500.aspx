<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP505010.aspx.cs"
    Inherits="Page_EP505010" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.TimeSheetRelease" PrimaryView="FilteredItems">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Cancel" />
            <px:PXDSCallbackCommand Name="Process" CommitChanges="true" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="ProcessAll" CommitChanges="true" />
            <px:PXDSCallbackCommand Visible="false" DependOnGrid="grid" Name="viewDetails" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" ActionsPosition="Top"
        Caption="Time Sheets" SkinID="Inquire">
        <Levels>
            <px:PXGridLevel DataMember="FilteredItems">
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="TimeSheetCD" LinkCommand="viewDetails" />
                    <px:PXGridColumn DataField="DocDate" />
                    <px:PXGridColumn DataField="EmployeeCD" />
                    <px:PXGridColumn DataField="EmployeeName" />
                    <px:PXGridColumn DataField="TimeSpent" TimeMode="true" />
                    <px:PXGridColumn DataField="OvertimeSpent" TimeMode="true" />
                    <px:PXGridColumn DataField="TimeBillable" TimeMode="true" />
                    <px:PXGridColumn DataField="OvertimeBillable" TimeMode="true" />
                    <px:PXGridColumn DataField="ApprovedBy" />
                    <px:PXGridColumn DataField="ApproveDate" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <ActionBar DefaultAction="cmdViewDetails">
            <CustomItems>
                <px:PXToolBarButton Text="View Details" Tooltip="Shows Time Sheet Details" Key="cmdViewDetails" Visible="false">
                    <AutoCallBack Command="viewDetails" Target="ds">
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXToolBarButton>
            </CustomItems>
        </ActionBar>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXGrid>
</asp:Content>
