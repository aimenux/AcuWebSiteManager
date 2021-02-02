<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS204700.aspx.cs" Inherits="Page_FS204700" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        TypeName="PX.Objects.FS.MasterContractMaint" PrimaryView="MasterContracts">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds"
        Style="z-index: 100" Width="100%" DataMember="MasterContracts" 
        TabIndex="5300" DefaultControlID="edMasterContractCD">
		<Template>
            <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" ControlSize="M">
            </px:PXLayoutRule>
            <px:PXSelector ID="edMasterContractCD" runat="server" 
                DataField="MasterContractCD">
            </px:PXSelector>
            <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="CustomerID" 
                CommitChanges="True">
            </px:PXSegmentMask>
            <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" 
                DataField="Descr" Size="XL">
            </px:PXTextEdit>
        </Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
