<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SO507000.aspx.cs"
    Inherits="Page_SO507000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.SO.SOPaymentProcess" PrimaryView="Filter">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="ViewRelatedDocument" />
		</CallbackCommands>
	</px:PXDataSource>
    
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" DataMember="Filter" Width="100%" Height="100px" DefaultControlID="ddAction">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXDropDown ID="ddAction" runat="server" AllowNull="False" DataField="Action" CommitChanges="true" />
			<px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" />
			<px:PXLayoutRule runat="server" StartColumn="True" />
			<px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="StartDate" ID="dtStartDate" />
			<px:PXDateTimeEdit CommitChanges="True" runat="server" DataField="EndDate" ID="dtEndDate" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" CaptionVisible="false"
        SyncPosition="True">
        <Levels>
            <px:PXGridLevel DataMember="Payments">
                <RowTemplate>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn AllowNull="False" DataField="Selected" TextAlign="Center" Type="CheckBox" AllowCheckAll="true" AllowMove="False" AllowShowHide="False" />
					<px:PXGridColumn DataField="DocType" />
                    <px:PXGridColumn DataField="RefNbr" DisplayFormat="&gt;CCCCCCCCCCCCCCC" RenderEditorText="True" CommitChanges="True" LinkCommand="ViewDocument" />
					<px:PXGridColumn DataField="DocDate" />
					<px:PXGridColumn DataField="FinPeriodID" />
					<px:PXGridColumn DataField="CustomerID" />
					<px:PXGridColumn DataField="CustomerID_BAccountR_acctName" />
					<px:PXGridColumn DataField="FundHoldExpDate" />
					<px:PXGridColumn DataField="Status" />
					<px:PXGridColumn DataField="RelatedTranProcessingStatus" />
					<px:PXGridColumn DataField="CuryID" />
					<px:PXGridColumn DataField="CuryOrigDocAmt" />
					<px:PXGridColumn DataField="ProcessingCenterID" />
					<px:PXGridColumn DataField="PaymentMethodID" />
					<px:PXGridColumn DataField="RelatedDocument" />
					<px:PXGridColumn DataField="RelatedDocumentNumber" RenderEditorText="True" CommitChanges="True" LinkCommand="ViewRelatedDocument" />
					<px:PXGridColumn DataField="RelatedDocumentStatus" />
					<px:PXGridColumn DataField="CuryRelatedDocumentAppliedAmount" />
					<px:PXGridColumn DataField="relatedDocumentCreditTerms" />
					<px:PXGridColumn DataField="ErrorDescription" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
		<ActionBar DefaultAction="viewDocument">
			<CustomItems>
                <px:PXToolBarButton DependOnGrid="grid">
                    <AutoCallBack Command="ViewRelatedDocument" Target="ds">
                        <Behavior CommitChanges="True" />
                    </AutoCallBack>
                </px:PXToolBarButton>
			</CustomItems>
		</ActionBar>
    </px:PXGrid>
</asp:Content>
