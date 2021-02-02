<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AP503500.aspx.cs" Inherits="Page_AP503500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.AP.APGenerateIntercompanyBills" PrimaryView="Filter">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="ViewARDocument" Visible="false" DependOnGrid="grid" CommitChanges="true"/>
			<px:PXDSCallbackCommand Name="ViewAPDocument" Visible="false" DependOnGrid="grid" CommitChanges="true"/>
		</CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Filter" Caption="Selection" DefaultControlID="edDate">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" />
            <px:PXDateTimeEdit CommitChanges="True" Size="m" ID="edDate" runat="server" DataField="Date" />
			<px:PXSegmentMask CommitChanges="True" ID="edVendorID" runat="server" DataField="VendorID" DataSourceID="ds" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
			<px:PXCheckBox CommitChanges="True" ID="chkCreateOnHold" runat="server" DataField="CreateOnHold" AlignLeft="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkCreateInSpecificPeriod" runat="server" DataField="CreateInSpecificPeriod" AlignLeft="True" />
			<px:PXSelector CommitChanges="True" ID="edFinPeriodID" runat="server" DataField="FinPeriodID" />
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" />
			<px:PXCheckBox CommitChanges="True" ID="chkCopyProjectInformation" runat="server" DataField="CopyProjectInformation" AlignLeft="True" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Caption="AR Documents" AllowPaging="true" AdjustPageSize="Auto" SkinID="PrimaryInquire" FastFilterFields="DocType, RefNbr"
        AllowSearch="true" SyncPosition="true">
        <Levels>
            <px:PXGridLevel DataMember="Documents">
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="True" AllowSort="False" AllowMove="False" AllowUpdate="False" AutoCallBack="True" />
                    <px:PXGridColumn DataField="VendorCD"/>
                    <px:PXGridColumn DataField="CustomerCD"/>
                    <px:PXGridColumn DataField="DocType"/>
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewARDocument"/>
                    <px:PXGridColumn DataField="DocDate"/>
                    <px:PXGridColumn DataField="FinPeriodID"/>
                    <px:PXGridColumn DataField="Status"/>
                    <px:PXGridColumn DataField="CuryOrigDocAmt"/>
                    <px:PXGridColumn DataField="CuryID"/>
                    <px:PXGridColumn DataField="DocDesc"/>
                    <px:PXGridColumn DataField="ProjectCD"/>
                    <px:PXGridColumn DataField="InvoiceNbr"/>
                    <px:PXGridColumn DataField="TermsID"/>
                    <px:PXGridColumn DataField="DueDate"/>
                    <px:PXGridColumn DataField="DiscDate"/>
                    <px:PXGridColumn DataField="CuryDocBal"/>
                    <px:PXGridColumn DataField="CuryTaxTotal"/>
                    <px:PXGridColumn DataField="CuryOrigDiscAmt"/>
                    <px:PXGridColumn DataField="IntercompanyAPDocumentNoteID" LinkCommand="ViewAPDocument"/>
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXGrid>
</asp:Content>
