<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SF205020.aspx.cs"
    Inherits="Page_SF205020" Title="Real-Time Sync" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Salesforce.SFSetupMaint" PrimaryView="EntitySetup" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True"
        AdjustPageSize="Auto" DataSourceID="ds" SkinID="Primary" TabIndex="2500">
        <Levels>
            <px:PXGridLevel DataMember="EntitySetup">
                <Columns>
                    <px:PXGridColumn DataField="EntityType" Width="120px" DisplayMode="Text" TextAlign="Left" CommitChanges="True" />
                    <px:PXGridColumn DataField="ImportScenario" Width="220px" />
                    <px:PXGridColumn DataField="ExportScenario" Width="220px" />
                    <px:PXGridColumn DataField="MaxAttemptCount" Width="110px" />
					<px:PXGridColumn DataField="SyncSortOrder" Width="110px" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
    </px:PXGrid>
</asp:Content>
