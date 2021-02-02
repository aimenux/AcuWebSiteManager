<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AR513000.aspx.cs" Inherits="Page_AR513000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="Filter" TypeName="PX.Objects.AR.ExternalTransactionValidation">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewDocument" Visible="False" DependOnGrid="grid">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ViewOrigDocument" Visible="False" DependOnGrid="grid">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ViewProcessingCenter" Visible="False" DependOnGrid="grid">
            </px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ViewExternalTransaction" Visible="False" DependOnGrid="grid">
            </px:PXDSCallbackCommand>
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Filter" TabIndex="12400">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" StartColumn="True"/>
            <px:PXSelector ID="edProcessingCenterID" runat="server" DataField="ProcessingCenterID" CommitChanges="True">
            </px:PXSelector>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="PrimaryInquire" 
        FastFilterFields="DocType,RefNbr" AllowPaging="true" AdjustPageSize="Auto">
		<Levels>
			<px:PXGridLevel DataMember="PaymentTrans">
                <Columns>
                    <px:PXGridColumn DataField="Selected" Label="Selected" TextAlign="Center" Type="CheckBox" AutoCallBack="true" AllowCheckAll="true" />
                    <px:PXGridColumn DataField="DocType" Width="100px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="RefNbr" LinkCommand="ViewDocument" Width="140px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="OrigDocType" Width="120px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="OrigRefNbr" LinkCommand="ViewOrigDocument" Width="140px">
                    </px:PXGridColumn>
					<px:PXGridColumn DataField="CCProcessingStatus" Width="200px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="LastActivityDate" Width="130px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="TranNumber" Width="160px">
                    </px:PXGridColumn>
                    <px:PXGridColumn DataField="CCProcessingCenterID" LinkCommand="ViewProcessingCenter" Width="130px">
                    </px:PXGridColumn>
                </Columns>
			</px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXGrid>
</asp:Content>
