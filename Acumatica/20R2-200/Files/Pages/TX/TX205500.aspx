<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX205500.aspx.cs" Inherits="Page_TX205500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:PXDataSource EnableAttributes="true" ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="TxCategory" TypeName="PX.Objects.TX.TaxCategoryMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" DataMember="TxCategory" Caption="Tax Category" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity">
		<Template>
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
			<px:PXSelector ID="edTaxCategoryID" runat="server" DataField="TaxCategoryID" AutoRefresh="True"/>
			<px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
			<px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
			<px:PXCheckBox ID="chkTaxCatFlag" runat="server" DataField="TaxCatFlag" />
			<px:PXCheckBox ID="chkExempt" runat="server" DataField="Exempt" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="153px" Width="100%" 
    Caption="Applicable Taxes" ActionsPosition="top" AllowSearch="true" 
    SkinID="Details">
		<Levels>
			<px:PXGridLevel DataMember="Details">
				<Columns>
					<px:PXGridColumn DataField="TaxID" AllowShowHide="False" AutoCallBack="True"/>
					<px:PXGridColumn DataField="Tax__Descr" />
					<px:PXGridColumn DataField="Tax__TaxType"  Type ="DropDownList"/>
					<px:PXGridColumn DataField="Tax__TaxCalcRule" Type ="DropDownList"/>
					<px:PXGridColumn DataField="Tax__TaxApplyTermsDisc" Type ="DropDownList"/>
				</Columns>
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
					<px:PXSelector ID="edTaxID" runat="server" DataField="TaxID" AllowEdit="True" />
					<px:PXTextEdit Size="xm" ID="edTax__Descr" runat="server" DataField="Tax__Descr" />
					<px:PXDropDown Size="m" ID="edTax__TaxType" runat="server" DataField="Tax__TaxType" />
					<px:PXDropDown ID="edTax__TaxCalcRule" runat="server" DataField="Tax__TaxCalcRule" />
					<px:PXDropDown ID="edTax__TaxApplyTermsDisc" runat="server" DataField="Tax__TaxApplyTermsDisc" />
				</RowTemplate>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
