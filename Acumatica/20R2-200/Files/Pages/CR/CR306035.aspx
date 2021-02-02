<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CR306035.aspx.cs" Inherits="Page_CR306035" Title="Mail" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server"  Visible="True" Width="100%" TypeName="PX.Objects.CR.CRSMEmailMaint" PrimaryView="Email">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="CancelClose" Visible="True" />
			<px:PXDSCallbackCommand Name="Delete" PostData="Self" ClosePopup="true" Visible="true" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="frmViewMessage" runat="server" DataMember="Email" 
        DataSourceID="ds" OnDataBound="on_data_bound">			
            <ContentLayout SpacingSize="Small" AutoSizeControls="true" />
			<AutoSize Enabled="True" Container="Window" MinHeight="200" />
			<Template>
				<px:PXPanel>
					<Template>
						<px:PXLayoutRule ID="r1" runat="server" StartColumn="True" LabelsWidth="XS"	ColumnWidth="65%" />
						<px:PXTextEdit ID="edMailFrom" runat="server" DataField="MailFrom" Size="XL" />
						<px:PXTextEdit ID="edAddressTo" runat="server" DataField="MailTo" Size="XL" />
						<px:PXTextEdit ID="edAddressCc" runat="server" DataField="mailCc" Size="XL" />
						<px:PXTextEdit ID="edAddressBcc" runat="server" DataField="mailBcc" Size="XL" />
						<px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" Size="XL" />
						
						<px:PXLayoutRule ID="PXLayoutRule3" runat="server" ColumnWidth="35%" StartColumn="True"/>
						<px:PXDropDown ID="edMPStatus" runat="server" DataField="MPStatus" />	
						<px:PXHtmlView ID="edRedException" runat="server" DataField="RedException"
							TextMode="MultiLine" SkinID="Label" Width="350px"
                             Style="border: solid 1px Gray; background-color: White; height: 132px;"
							SuppressLabel="True">
						</px:PXHtmlView>
					</Template>
				</px:PXPanel>
                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="SM" />				
				<px:PXLabel ID="lm" runat="server" />
				<px:PXHtmlView ID="rteBody"  runat="server" DataField="Body" TextMode="MultiLine"  Width="100%" Height="100%">
					<AutoSize Enabled="True" Container="Parent" MinHeight="350" />
				</px:PXHtmlView>
			</Template>
		</px:PXFormView>
</asp:Content>

