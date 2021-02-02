<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP501000.aspx.cs"
    Inherits="Page_EP501000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" Visible="True" PrimaryView="EPDocumentList" TypeName="PX.Objects.EP.EPDocumentRelease"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" ActionsPosition="Top"
        AutoAdjustColumns="True" AllowSearch="True" DataSourceID="ds" SkinID="PrimaryInquire" Caption="Expense Claims" FastFilterFields="RefNbr,EmployeeID,EmployeeID_description">
        <Levels>
            <px:PXGridLevel DataMember="EPDocumentList">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXCheckBox ID="chkSelected" runat="server" DataField="Selected" />
                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" AllowEdit="True" />
                    <px:PXDropDown ID="edStatus" runat="server" DataField="Status" Enabled="False" />
                    <px:PXDateTimeEdit ID="edDocDate" runat="server" DataField="DocDate" Enabled="False" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
                    <px:PXSegmentMask ID="edEmployeeID" runat="server" DataField="EmployeeID" Enabled="False" AllowEdit="True" />
                    <px:PXDateTimeEdit ID="edApproveDate" runat="server" DataField="ApproveDate" Enabled="False" />
                    <px:PXSelector ID="edApproverID" runat="server" DataField="ApproverID" Enabled="False" TextField="Username" />
                    <px:PXSelector ID="edDepartmentID" runat="server" DataField="DepartmentID" Enabled="False" />
                    <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" Enabled="False" />
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXTextEdit ID="edDocDesc" runat="server" DataField="DocDesc" Enabled="False" />
                    <px:PXNumberEdit ID="edCuryDocBal" runat="server" DataField="CuryDocBal" Enabled="False" />
                    <px:PXTextEdit ID="edEPEmployee_acctName" runat="server" DataField="EmployeeID_EPEmployee_acctName" Enabled="False" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowCheckAll="True" DataField="Selected" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn  DataField="RefNbr" />
                    <px:PXGridColumn  DataField="Status" RenderEditorText="True" />
                    <px:PXGridColumn  DataField="DocDate" />
                    <px:PXGridColumn  DataField="CuryDocBal" TextAlign="Right" />
                    <px:PXGridColumn  DataField="CuryID" />
                    <px:PXGridColumn  DataField="EmployeeID" RenderEditorText="True" />
                    <px:PXGridColumn  DataField="EmployeeID_description" />
                    <px:PXGridColumn  DataField="DepartmentID" />
                    <px:PXGridColumn  DataField="BranchID" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
