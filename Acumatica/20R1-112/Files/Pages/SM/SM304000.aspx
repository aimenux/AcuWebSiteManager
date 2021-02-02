<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM304000.aspx.cs" Inherits="Page_SM304000" Title="Webhooks" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Api.Webhooks.Graph.WebhookMaint" SuspendUnloading="False" PrimaryView="Webhook">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ShowRequestDetails" Visible="False" CommitChanges="True" DependOnGrid="grid1" />
            <px:PXDSCallbackCommand Name="ClearRequestsLog" Visible="False" CommitChanges="True" DependOnGrid="grid1" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Webhook" TabIndex="1">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="140" ControlSize="XXL" />
            <px:PXSelector ID="PXSelector1" runat="server" DataField="Name" AutoRefresh="True" />
            <px:PXSelector ID="PXSelector2" runat="server" DataField="Handler" />

            <px:PXLayoutRule runat="server" ColumnSpan="2" LabelsWidth="140" ControlSize="XXL" />
            <px:PXTextEdit ID="PXTextEdit2" runat="server" DataField="Url" Enabled="False" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="1" ControlSize="XS" />
            <px:PXCheckBox ID="PXCheckBox1" runat="server" DataField="IsActive" Size="S" />
            <px:PXCheckBox ID="PXCheckBox2" runat="server" DataField="IsSystem" Size="S" Enabled="False" />
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="250px" DataSourceID="ds">
		<Items>
			<px:PXTabItem Text="Request History">
				<Template>
                    <px:PXFormView ID="form1" runat="server" DataMember="Webhook" RenderStyle="Simple" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="115" ControlSize="S" />
                            <px:PXDropDown ID="PXDropDown1" runat="server" DataField="RequestLogLevel" />

                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="250" ControlSize="XS" />
                            <px:PXNumberEdit ID="PXNumberEdit1" runat="server" DataField="RequestRetainCount" Width="50" />
                        </Template>
                    </px:PXFormView>
	                <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="250px"
                        SkinID="Details" TabIndex="1" SyncPosition="true" KeepPosition="true" AllowPaging="true">
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton CommandSourceID="ds" CommandName="ShowRequestDetails" />
                                <px:PXToolBarButton CommandSourceID="ds" CommandName="ClearRequestsLog" />
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
		                <Levels>
			                <px:PXGridLevel DataMember="WebhookRequest">
                                <Columns>
                                    <px:PXGridColumn DataField="Type" />
                                    <px:PXGridColumn DataField="ReceivedFrom" />
                                    <px:PXGridColumn DataField="ReceiveDate" />
                                    <px:PXGridColumn DataField="ResponseStatus" />
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
		                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	                </px:PXGrid>
                </Template>
		    </px:PXTabItem>
        </Items>
	    <AutoSize Container="Window" Enabled="True" MinHeight="400" />
    </px:PXTab>
    <px:PXSmartPanel ID="panelRequestDetails" runat="server" CaptionVisible="True" Caption="Request Details" Width="710px" Height="615px"
        Key="WebhookRequestCurrent" AutoCallBack-Target="formRequestDetails" AutoCallBack-Command="Refresh">
        <px:PXFormView ID="formRequestDetails" runat="server" SkinID="Transparent" DataMember="WebhookRequestCurrent" DataSourceID="ds" >			
            <AutoSize Enabled="True" Container="Parent" />
            <Template>
                <px:PXPanel runat="server" RenderStyle="Simple">
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True" LabelsWidth="XS" ControlSize="M" />
                    <px:PXTextEdit ID="PXTextEdit1" runat="server" DataField="Type" Enabled="False" />
                </px:PXPanel>
                <px:PXPanel runat="server">
                    <px:PXTextEdit ID="PXHtmlView1" runat="server" DataField="Request" Enabled="False" TextMode="MultiLine" Width="662" Height="215px" SuppressLabel="true" />
                </px:PXPanel>
                <px:PXLabel ID="PXLabel1" runat="server" />

                <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartRow="True" LabelsWidth="110" ControlSize="XS" />
                <px:PXTextEdit ID="PXNumberEdit1" runat="server" DataField="ResponseStatus" Enabled="False" />
                <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XS" />
                <px:PXTextEdit ID="PXNumberEdit2" runat="server" DataField="ProcessingTime" Enabled="False" />

                <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartRow="True" />
                <px:PXTab ID="tab" runat="server" Width="100%" Height="250px" DataMember="WebhookRequestCurrent" DataSourceID="ds">
                    <Items>
                        <px:PXTabItem Text="Response">
                            <Template>
                                <px:PXTextEdit ID="PXTextEdit2" runat="server" DataField="Response" Enabled="False" TextMode="MultiLine" Width="662" Height="215px" SuppressLabel="true" />
                            </Template>
                        </px:PXTabItem>
                        <px:PXTabItem Text="Exception Stack Trace">
                            <Template>
                                <px:PXTextEdit ID="PXTextEdit3" runat="server" DataField="Error" Enabled="False" TextMode="MultiLine" Width="662" Height="215px" SuppressLabel="true" />
                            </Template>
                        </px:PXTabItem>
                    </Items>
                    <AutoSize Container="Parent" Enabled="True" MinHeight="250" />
                </px:PXTab>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="panelButtons" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnClose" runat="server" DialogResult="Cancel" Text="Close" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
