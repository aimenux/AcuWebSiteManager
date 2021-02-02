<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CA208000.aspx.cs" Inherits="Page_CA208000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CA.CCUpdateExpirationDatesProcess" PrimaryView="Filter">
		<CallbackCommands>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter">
		<Template>
            <px:PXLayoutRule runat="server" LabelsWidth="S" StartColumn="True"/>
			<px:PXSelector ID="edProcessingCenterID" runat="server" DataField="ProcessingCenterID" AutoRefresh="True" DataSourceID="ds" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Details">
        <ActionBar>
            <Actions>
                <ExportExcel Enabled="false" MenuVisible="false" />               
            </Actions>
        </ActionBar>
		<Levels>
			<px:PXGridLevel DataMember="CustomerPaymentMethods">
                <Columns>
                    <px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" TextAlign="Center" />
				    <px:PXGridColumn DataField="BAccountID" />
				    <px:PXGridColumn DataField="CashAccountID" />
				    <px:PXGridColumn DataField="Descr" />
				    <px:PXGridColumn DataField="ExpirationDate" />
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False" AutoInsert="False" />
	</px:PXGrid>
</asp:Content>
