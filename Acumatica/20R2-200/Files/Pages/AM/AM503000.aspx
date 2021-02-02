<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM503000.aspx.cs" Inherits="Page_AM503000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" 
        TypeName="PX.Objects.AM.AMDocumentRelease" PrimaryView="AMDocumentList" Visible="True">
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" 
        AllowSearch="true" DataSourceID="ds" BatchUpdate="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" Caption="Documents" SyncPosition="True">
		<Levels>
			<px:PXGridLevel DataKeyNames="BatNbr" DataMember="AMDocumentList">
			    <RowTemplate>
			        <px:PXCheckBox ID="edSelected" runat="server" DataField="Selected"/>
			        <px:PXSelector ID="edBatNbr" runat="server" DataField="BatNbr" Enabled="False" AllowEdit="True"/>
			        <px:PXDropDown ID="edDocType" runat="server" DataField="DocType"/>
			        <px:PXDropDown ID="edStatus" runat="server" DataField="Status"/>
			        <px:PXDateTimeEdit ID="edTranDate" runat="server" DataField="TranDate"/>
			        <px:PXSelector ID="edFinPeriodID" runat="server" DataField="FinPeriodID"/>
			        <px:PXNumberEdit ID="edTotalQty" runat="server" DataField="TotalQty"/>
			        <px:PXNumberEdit ID="edTotalCost" runat="server" DataField="TotalCost"/>
			        <px:PXNumberEdit ID="edTotalAmount" runat="server" DataField="TotalAmount"/>
			        <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc"/>
                </RowTemplate>
			    <Columns>
                    <px:PXGridColumn DataField="Selected" AllowCheckAll="True" Width="30px" TextAlign="Center" Type="CheckBox" />
                    <px:PXGridColumn DataField="BatNbr" Width="120px" />
                    <px:PXGridColumn DataField="DocType" Width="120px"/>
                    <px:PXGridColumn DataField="Status" />
			        <px:PXGridColumn DataField="TranDate" Width="90px"/>
			        <px:PXGridColumn DataField="FinPeriodID"/>
			        <px:PXGridColumn DataField="TotalQty" TextAlign="Right" Width="100px"/>
			        <px:PXGridColumn DataField="TotalCost" TextAlign="Right" Width="100px"/>
                    <px:PXGridColumn DataField="TotalAmount" Width="100px" TextAlign="Right" />
			        <px:PXGridColumn DataField="TranDesc" Width="200px"/>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<ActionBar DefaultAction="ViewDocument"/>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXGrid>
</asp:Content>
