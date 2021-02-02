<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM303010.aspx.cs" Inherits="Page_SM303010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Owin.IdentityServerIntegration.DAC.OAuthClientMaint" PrimaryView="Clients">
        <CallbackCommands>
			<px:PXDSCallbackCommand Name="RevokeAccess" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="AddSharedSecret" Visible="False" CommitChanges="true" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Clients" TabIndex="2300">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="L" LabelsWidth="S" StartColumn="True" />
            <px:PXSelector ID="edClientID" runat="server" DataField="ClientID" DataSourceID="ds" NullText="<NEW>" DisplayMode="Text" AutoRefresh="True" FilterByAllFields="True">
                <GridProperties>
                    <Columns>
                        <px:PXGridColumn DataField="ClientID"  AllowShowHide="False" Visible="False" SyncVisible="False" />
                        <px:PXGridColumn DataField="ClientName" />
                        <px:PXGridColumn DataField="Flow" />
                        <px:PXGridColumn DataField="Enabled" Type = "CheckBox" TextAlign="Center" />
                        <px:PXGridColumn DataField="ClientIDString"/>
                    </Columns>
                </GridProperties>
            </px:PXSelector>
            <px:PXTextEdit ID="edClientName" runat="server" AlreadyLocalized="False" DataField="ClientName" DefaultLocale="" />
            <px:PXCheckBox ID="edEnabled" runat="server" AlreadyLocalized="False" DataField="Enabled" Text="Enabled" />
            <px:PXDropDown ID="edFlow" runat="server" DataField="Flow" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Secrets">
                <Template>
                    <px:PXGrid ID="clientSecrets" runat="server" DataSourceID="ds" Style="z-index: 100"
                        Width="100%" SkinID="DetailsInTab" AutoAdjustColumns="True" TabIndex="800">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="ClientID,SecretID" DataMember="ClientSecrets">
                                <RowTemplate>
                                    <px:PXDropDown ID="edType" runat="server" DataField="Type" />
                                    <px:PXTextEdit ID="edDescription" runat="server" AlreadyLocalized="False" DataField="Description" />
                                    <px:PXDateTimeEdit ID="edExpirationUtc" runat="server" AlreadyLocalized="False" DataField="ExpirationUtc" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Type" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="ExpirationUtc" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False"/>
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="False" MenuVisible="False" ToolBarVisible="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Text="Add Shared Secret" CommandSourceID="ds" CommandName="AddSharedSecret" />
                            </CustomItems>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Redirect URIs">
                <Template>
                    <px:PXGrid ID="redirectUris" runat="server" DataSourceID="ds" Style="z-index: 100"
                        Width="100%" SkinID="DetailsInTab" AutoAdjustColumns="True">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="ClientID,RedirectUriID" DataMember="RedirectUris">
                                <RowTemplate>
                                    <px:PXTextEdit ID="edRedirectUri" runat="server" AlreadyLocalized="False" DataField="RedirectUri" DefaultLocale="" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="RedirectUri" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>

    <px:PXSmartPanel ID="pnlAddSharedSecret" runat="server" Caption="Add Shared Secret"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="AddSharedSecretView" CreateOnDemand="false"
        AutoCallBack-Enabled="true" AutoCallBack-Target="formAddSharedSecret" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        ShowAfterLoad="True"
        AcceptButtonID="btnActionOK"
        CancelButtonID="btnActionCancel"
        >
        <px:PXFormView ID="formAddSharedSecret" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False" DataMember="AddSharedSecretView">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XL"/>
                <px:PXTextEdit ID="edDescription" runat="server" AlreadyLocalized="False" DataField="Description" DefaultLocale="" TextMode="MultiLine"/>
                <px:PXDateTimeEdit ID="edExpirationUtc" runat="server" AlreadyLocalized="False" DataField="ExpirationUtc" DefaultLocale="" Size="M"/>
                <px:PXLabel runat="server" Text="Copy and save the value of the secret." Style="padding-top: 4px; display: inline-block"/>
                <px:PXTextEdit ID="edValue" runat="server" AlreadyLocalized="False" DataField="Value" DefaultLocale="" Enabled="False"/>
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="PXPanel1" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnActionOK" runat="server" DialogResult="OK" Text="OK" />
            <px:PXButton ID="btnActionCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>