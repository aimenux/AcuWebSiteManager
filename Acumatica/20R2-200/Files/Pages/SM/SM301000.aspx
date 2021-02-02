<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM301000.aspx.cs" Inherits="Page_SM301000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <script type="text/javascript">
        function reloadScreen() {
            var ds = __px_cm(window).findDataSource();
            if (ds) ds.executeCallback(ds.cancelCommandName);
        }
        $(function() {
            $(document)
                .ready(function() {
                    var chat = $.connection.refreshHub;
                    chat.client.refreshScreen = function(appToRefreshId) {
                        var appId = $("[id$=_edApplicationID_text]").val();
                        if (appId === appToRefreshId)
                            reloadScreen();
                    }
                    $.connection.hub.start()
                        .done(function() {
                            window.console.log("refreshHub connection started");
                        });
                });
        });
    </script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.OAuthClient.ApplicationMaint" PrimaryView="Applications" SuspendUnloading="False">
	    <CallbackCommands>
            <px:PXDSCallbackCommand Name="SignIn"  />
        </CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Applications" TabIndex="100">
		<Template>
			<px:PXLayoutRule runat="server" StartRow="True" ControlSize="XM" GroupCaption="Application" LabelsWidth="SM" StartColumn="True"/>
            <px:PXSelector ID="edApplicationID" runat="server" DataField="ApplicationID" TextField="ApplicationID" DataSourceID="ds"
		                   NullText="<NEW>" DisplayMode="Text" AutoRefresh="True">
            </px:PXSelector>
            <px:PXDropDown ID="edType" runat="server" DataField="Type">
            </px:PXDropDown>
            <px:PXTextEdit ID="edApplicationName" runat="server" DataField="ApplicationName" ValidateRequestMode="Inherit">
            </px:PXTextEdit>
            <px:PXTextEdit ID="edClientID" runat="server" DataField="ClientID" ValidateRequestMode="Inherit">
            </px:PXTextEdit>
            <px:PXTextEdit ID="edClientSecret" runat="server" DataField="ClientSecret" CommitChanges="True" ValidateRequestMode="Inherit">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" ControlSize="XM" GroupCaption="Authentication Token" LabelsWidth="SM" StartColumn="True">
            </px:PXLayoutRule>
            <px:PXTextEdit ID="edOAuthToken__Bearer" runat="server" DataField="OAuthToken__Bearer" ValidateRequestMode="Inherit">
            </px:PXTextEdit>
		    <px:PXDateTimeEdit ID="edOAuthToken__UtcExpiredOn" runat="server" DataField="OAuthToken__UtcExpiredOn" Size="M" ValidateRequestMode="Inherit">
            </px:PXDateTimeEdit>
            <px:PXLayoutRule runat="server" StartRow="True" ColumnSpan="2" ControlSize="XL" LabelsWidth="SM"></px:PXLayoutRule>
            <px:PXTextEdit runat="server" ID="edReturnUrl" DataField="ReturnUrl" Enabled="False"></px:PXTextEdit>
		</Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
