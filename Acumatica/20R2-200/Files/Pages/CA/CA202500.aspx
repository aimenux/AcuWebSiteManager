<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA202500.aspx.cs" Inherits="Page_CA202500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CA.CACorpCardsMaint" PrimaryView="CreditCards">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="CreditCards" Width="100%" AllowAutoHide="false">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector runat="server" DataSourceID="ds" ID="edCorporateCreditCardCD" DataField="CorpCardCD" />
            <px:PXTextEdit runat="server" DataSourceID="ds" ID="edName" DataField="Name" />
            <px:PXTextEdit runat="server" DataSourceID="ds" ID="edCardNumber" DataField="CardNumber" />
            <px:PXSegmentMask CommitChanges="True" ID="edCashAccountID" runat="server" DataField="CashAccountID" />
            <px:PXCheckBox runat="server" DataSourceID="ds" ID="edIsActive" DataField="IsActive" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Details" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="EmployeeLinks">
                <RowTemplate>
                    <px:PXSelector ID="edEmployeeID" runat="server" DataField="EmployeeID" CommitChanges="True"/>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn runat="server" DataField="EmployeeID" CommitChanges="True" Width="100px" AutoCallBack="True"/>
                    <px:PXGridColumn runat="server" DataField="EmployeeID_EPEmployee_acctName" Width="200px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <ActionBar >
        </ActionBar>
    </px:PXGrid>
</asp:Content>