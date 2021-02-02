<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR307000.aspx.cs" Inherits="Page_AR307000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CurrentTransaction" TypeName="PX.Objects.AR.ExternalTransactionMaint" PageLoadBehavior="GoLastRecord">
	<CallbackCommands>
        <px:PXDSCallbackCommand Name="Save" CommitChanges="true" Visible="false"  />
        <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="false" />
        <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="false"/>
        <px:PXDSCallbackCommand Name="CopyPaste" PostData="Self" Visible="false"/>			
	</CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="CurrentTransaction" TabIndex="700">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" LabelsWidth="SM"/>            
            <px:PXSelector ID="edTransactionID" runat="server" DataField="TransactionID" AutoRefresh="true" >
                <GridProperties FastFilterFields="TranNumber,AuthNumber"/>
            </px:PXSelector>
		    <px:PXTextEdit ID="edTranNumber" runat="server" AlreadyLocalized="False" DataField="TranNumber"  Enabled="False">
            </px:PXTextEdit>
            <px:PXTextEdit ID="edAuthNumber" runat="server" AlreadyLocalized="False" DataField="AuthNumber" Enabled="False">
            </px:PXTextEdit>
            <px:PXDateTimeEdit ID="edLastActivityDate" runat="server" AlreadyLocalized="False" DataField="LastActivityDate"  Enabled="False">
            </px:PXDateTimeEdit>
            <px:PXDateTimeEdit ID="edExpirationDate" runat="server" AlreadyLocalized="False" DataField="ExpirationDate" Enabled="False">
            </px:PXDateTimeEdit>
            <px:PXDropDown ID="edDocType" runat="server" AlreadyLocalized="False" DataField="DocType" Enabled="False">
            </px:PXDropDown>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" Enabled="False" AllowEdit="True" >
            </px:PXSelector>
            <px:PXSelector ID="edOrigDocType" runat="server" AlreadyLocalized="False" DataField="OrigDocType" Enabled="False">
            </px:PXSelector>
            <px:PXSelector ID="edOrigRefNbr" runat="server" DataField="OrigRefNbr" Enabled="False"  AllowEdit="True" >                
            </px:PXSelector>
            <px:PXLayoutRule runat="server" StartColumn="True" ControlSize="XM">
            </px:PXLayoutRule>
            <px:PXSelector ID="edCustomerPaymentMethod__BAccountID" runat="server" DataField="Transaction.CustomerPaymentMethod__BAccountID" AutoRefresh="true" AllowEdit="true"> 
            </px:PXSelector>
            <px:PXSelector ID="edCustomerPaymentMethod__PaymentMethodID" runat="server" DataField="Transaction.CustomerPaymentMethod__PaymentMethodID" AutoRefresh="true" AllowEdit="true">
            </px:PXSelector>
            <px:PXSelector ID="edCustomerPaymentMethod__CCProcessingCenterID" runat="server" DataField="Transaction.CustomerPaymentMethod__CCProcessingCenterID">
            </px:PXSelector>
            <px:PXSelector ID="edPMInstanceID" runat="server" DataField="PMInstanceID" TextField="Descr" Enabled="False"  AllowEdit="True" >
            </px:PXSelector>
            <px:PXSelector ID="edCCProcessingCenter__CashAccountID" runat="server" DataField="Transaction.CCProcessingCenter__CashAccountID">
            </px:PXSelector>
            <px:PXSelector ID="edCashAccount__CuryID" runat="server" DataField="Transaction.CashAccount__CuryID" Size="XS">
            </px:PXSelector>
            <px:PXDropDown ID="edProcessingStatus" runat="server" DataField="Transaction.ProcessingStatus" Enabled="False">
            </px:PXDropDown>
            <px:PXLayoutRule runat="server" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXNumberEdit ID="edAmount" runat="server" AlreadyLocalized="False" DataField="Amount" Enabled="False">
            </px:PXNumberEdit>  
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server" >
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" Height="150px" SkinID="Details" TabIndex="2300">
		<Levels>
			<px:PXGridLevel DataKeyNames="TransactionID" DataMember="ccProcTran">
			    <RowTemplate>
                    <px:PXNumberEdit ID="edTranNbr" runat="server" AlreadyLocalized="False" DataField="TranNbr">
                    </px:PXNumberEdit>
                    <px:PXTextEdit ID="edProcessingCenterID" runat="server" AlreadyLocalized="False" DataField="ProcessingCenterID">
                    </px:PXTextEdit>
                    <px:PXDropDown ID="edTranType" runat="server" DataField="TranType">
                    </px:PXDropDown>
                    <px:PXDropDown ID="edTranStatus" runat="server" DataField="TranStatus">
                    </px:PXDropDown>
                    <px:PXNumberEdit ID="edAmount" runat="server" AlreadyLocalized="False" DataField="Amount">
                    </px:PXNumberEdit>
                    <px:PXNumberEdit ID="edRefTranNbr" runat="server" AlreadyLocalized="False" DataField="RefTranNbr">
                    </px:PXNumberEdit>
                    <px:PXTextEdit ID="edPCTranNumber" runat="server" AlreadyLocalized="False" DataField="PCTranNumber">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edAuthNumber" runat="server" AlreadyLocalized="False" DataField="AuthNumber">
                    </px:PXTextEdit>
                    <px:PXTextEdit ID="edPCResponseReasonText" runat="server" AlreadyLocalized="False" DataField="PCResponseReasonText">
                    </px:PXTextEdit>
                    <px:PXDateTimeEdit ID="edStartTime" runat="server" AlreadyLocalized="False" DataField="StartTime">
                    </px:PXDateTimeEdit>
                    <px:PXDropDown ID="edProcStatus" runat="server" DataField="ProcStatus">
                    </px:PXDropDown>
                    <px:PXDropDown ID="edCVVVerificationStatus" runat="server" DataField="CVVVerificationStatus">
                    </px:PXDropDown>
                </RowTemplate>
                <Columns>
                    <px:PXGridColumn DataField="TranNbr" TextAlign="Right">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ProcessingCenterID" Width="120px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TranType">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TranStatus">
                    </px:PXGridColumn>                    
                    <px:PXGridColumn DataField="Amount" TextAlign="Right" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="RefTranNbr" TextAlign="Right">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PCTranNumber" Width="180px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="AuthNumber" Width="180px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="PCResponseReasonText" Width="280px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="StartTime" Width="90px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="ProcStatus">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CVVVerificationStatus">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
