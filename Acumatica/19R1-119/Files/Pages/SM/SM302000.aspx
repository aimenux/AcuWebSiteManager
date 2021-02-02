<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM302000.aspx.cs" Inherits="Page_SM302000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.PushNotifications.UI.PushNotificationMaint" SuspendUnloading="False" PrimaryView="Hooks">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="ViewInquiry" Visible="False" DependOnGrid="grid1" CommitChanges="true" />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="Hooks" TabIndex="5500">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" LabelsWidth="SM" Merge="true" />
		    <px:PXSelector ID="edName" runat="server" DataField="Name"/>
            <px:PXTextEdit ID="edHeaderName" runat="server" AlreadyLocalized="False" DataField="HeaderName" DefaultLocale=""/>
            <px:PXCheckBox Size="S" runat="server" DataField="Active" ID="PXCheckBox1" />
            <px:PXLayoutRule runat="server"/>
            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" LabelsWidth="SM" Merge="true" />
            <px:PXDropDown ID="edType" runat="server" DataField="Type" CommitChanges="true"/>
            <px:PXTextEdit ID="edHeaderValue" runat="server" AlreadyLocalized="False" DataField="HeaderValue" DefaultLocale=""/>
            <px:PXLayoutRule runat="server"/>
            <px:PXTextEdit ID="edAddress" runat="server" AlreadyLocalized="False" DataField="Address" DefaultLocale=""/>
		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds" DataMember="SourcesGI">
		<Items>
			<px:PXTabItem Text="Generic Inquiries">
				<Template>
	                <px:PXGrid ID="grid1" runat="server" DataSourceID="ds" Style="z-index: 100" 
		                Width="100%" Height="150px" SkinID="Details" TabIndex="5700" AutoAdjustColumns="true" SyncPosition="true">
                        <ActionBar>
                            <CustomItems>
                                <px:PXToolBarButton CommandSourceID="ds" CommandName="ViewInquiry" />
                            </CustomItems>
                        </ActionBar>
                        <Mode InitNewRow="True" />
		                <Levels>
			                <px:PXGridLevel  DataMember="SourcesGI">
                                <Columns>
                                    <px:PXGridColumn DataField="Active" Width="60" Type="CheckBox" TextAlign="Center" AllowResize="false"/>
                                    <px:PXGridColumn DataField="DesignID" />
                                </Columns>
			                </px:PXGridLevel>
		                </Levels>
		                <AutoSize Container="Window" Enabled="True" MinHeight="150" />
	                </px:PXGrid>
                </Template>
		    </px:PXTabItem>
            <px:PXTabItem Text="Built-In Definitions">
				<Template>
	                <px:PXGrid ID="grid2" runat="server" DataSourceID="ds" Style="z-index: 100" 
		                Width="100%" Height="150px" SkinID="Details" TabIndex="5700" AutoAdjustColumns="true">
		                <Levels>
			                <px:PXGridLevel  DataMember="SourcesInCode">
                                <Columns>
                                    <px:PXGridColumn DataField="Active" Width="60" Type="CheckBox" TextAlign="Center" AllowResize="false"/>
                                    <px:PXGridColumn DataField="InCodeClass"/>
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
</asp:Content>
