<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM506000.aspx.cs" Inherits="Page_PM506000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/TabDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.PM.ProFormaProcess">
		<CallbackCommands>
             <px:PXDSCallbackCommand DependOnGrid="grid" Name="ViewDocumentProject" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection"
        DefaultControlID="edAction">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" />
            <px:PXDropDown Size="m" CommitChanges="True" ID="edAction" runat="server" AllowNull="False" DataField="Action" />
            <px:PXLayoutRule runat="server" Merge="True" />
             <px:PXDateTimeEdit CommitChanges="True" Size="m" ID="PXDateTimeEdit1" runat="server" DataField="BeginDate" />
             <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXDateTimeEdit CommitChanges="True" Size="m" ID="PXDateTimeEdit2" runat="server" DataField="EndDate" />
            <px:PXLayoutRule runat="server" Merge="False" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" />
            <px:PXLayoutRule runat="server" Merge="False" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AdjustPageSize="Auto" 
        AllowSearch="True" DataSourceID="ds" BatchUpdate="True" SkinID="PrimaryInquire" Caption="Documents" SyncPosition="True" FastFilterFields="CustomerID, CustomerID_BAccountR_acctName" TabIndex="300">
        <Levels>
            <px:PXGridLevel DataMember="Items">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True" >
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ProjectID" LinkCommand="viewDocumentProject">
                    </px:PXGridColumn>
                    <px:PXGridColumn AllowUpdate="False" DataField="RefNbr" LinkCommand="viewDocumentRef">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="Status">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="InvoiceDate">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="FinPeriodID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CustomerID_BAccountR_acctName" >
                    </px:PXGridColumn>                    
                    <px:PXGridColumn DataField="CuryDocTotal">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CuryID">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DueDate">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="DiscDate">
                    </px:PXGridColumn>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
