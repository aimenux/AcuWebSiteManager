<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="SM200540.aspx.cs" Inherits="Page_SM260000"
	Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <style type="text/css">        .nobr .GridRow 
        {
            white-space: nowrap !important;
        }</style>
    <script type="text/javascript">
        function commandResult(ds, context) {
            if (context.command == "Save") {
                var ds = px_all[context.id];
                var isSitemapAltered = (ds.callbackResultArg == "RefreshSitemap");
                if (isSitemapAltered) {
                    __refreshMainMenu();
                }
            }
        }
	</script>
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.TranslationMaint"
		PrimaryView="LanguageFilter">
	    <ClientEvents CommandPerformed="commandResult" />
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" BlockPage="true" />
            <px:PXDSCallbackCommand Name="deleteObsoleteStrings" Visible="False" DependOnGrid="gridValueObsolete" />
            <px:PXDSCallbackCommand Name="viewUsageDetails" Visible="False" DependOnGrid="gridValue" />
            <px:PXDSCallbackCommand Name="viewExceptionalUsageDetails" Visible="False" DependOnGrid="gridValueExceptional" />
            <px:PXDSCallbackCommand Name="viewUsageObsoleteDetails" Visible="False" DependOnGrid="gridValueObsolete" />
            <px:PXDSCallbackCommand Name="viewExceptionalUsageObsoleteDetails" Visible="False" DependOnGrid="gridValueExceptionalObsolete" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" LinkIndicator="True"
		DataMember="LanguageFilter" Caption="Target Locale" OnDataBound="form_DataBound">
		<Template>
		    
			<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDropDown ID="edLanguage" runat="server" DataField="Language" CommitChanges="True" AllowMultiSelect="True" />
			<px:PXLayoutRule runat="server" Merge="True" />
			<px:PXCheckBox CommitChanges="True" ID="chkShowLocalized" runat="server" DataField="ShowLocalized" />
            <px:PXCheckBox CommitChanges="True" ID="chkShowExcluded" runat="server" DataField="ShowExcluded" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
            <px:PXSelector ID="edScreen" runat="server" DataField="ScreenID"  DisplayMode="Text" FilterByAllFields="true" CommitChanges="True" />
            <px:PXCheckBox CommitChanges="True" ID="edShowUnboundOnly" runat="server" DataField="ShowUnboundOnly" />
            <px:PXDropDown ID="edShowType" runat="server" DataField="ShowType" CommitChanges="True" AllowMultiSelect="True" />

            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edCreated" runat="server" DataField="CreatedDateTime" OnValueChanged="resetGridPaging" />
            <px:PXDateTimeEdit CommitChanges="True" ID="edModified" runat="server" DataField="LastModifiedDateTime" OnValueChanged="resetGridPaging" />

		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%">
        <AutoSize Enabled="True" Container="Window" MinHeight="200" />
        <Items>
            <px:PXTabItem Text="Collected" Visible="True">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp1" SplitterPosition="300" SkinID="Horizontal">
                        <AutoSize Enabled="true" Container="Parent" />
                        <Template1>
                            <px:PXGrid ID="gridValue" runat="server" DataSourceID="ds" SyncPosition="True"
                                Width="100%" AutoAdjustColumns="True" AdjustPageSize="Auto" AllowPaging="True"
                                Caption="Default Values" AllowSearch="True" SkinID="Details" AutoSize="true" FastFilterFields="NeutralValue" CaptionVisible="True">
                                <Levels>
                                    <px:PXGridLevel DataMember="DeltaResourcesDistinct">
                                        <Columns>
                                            <px:PXGridColumn DataField="Id" Width="50" />
                                            <px:PXGridColumn DataField="NeutralValue" Width="90px" />
                                            <px:PXGridColumn DataField="IsNotLocalized" Width="15px" TextAlign="Center" Type="CheckBox" />
                                            <px:PXGridColumn DataField="LocalizedValue" Width="90px" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Mode AllowAddNew="False" AllowDelete="False" AllowUpload="true" />
                                <AutoSize Enabled="True" Container="Parent" />
                                <ActionBar PagerVisible="False">
                                    <Actions>
                                        <Save Enabled="False" />
                                        <AddNew Enabled="False" />
                                        <Delete Enabled="False" />
                                        <EditRecord Enabled="False" />
                                        <NoteShow Enabled="False" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton>
                                            <AutoCallBack Command="viewUsageDetails" Target="ds" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <AutoCallBack Command="Refresh" Target="gridValueExceptional" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridValueExceptional" runat="server" DataSourceID="ds" SyncPosition="True"
                                Width="100%" AutoAdjustColumns="True" AdjustPageSize="Auto" AllowPaging="False"
                                Caption="Key-Specific Values" SkinID="Details" AutoSize="true" CaptionVisible="True">
                                <AutoSize Enabled="True" Container="Parent" />
                                <Mode AllowAddNew="False" AllowDelete="False" />
                                <Levels>
                                    <px:PXGridLevel DataMember="ExceptionalResources">
                                        <Columns>
                                            <px:PXGridColumn DataField="IdRes" Width="50" />
                                            <px:PXGridColumn DataField="ResKey" Width="90px" />
                                            <px:PXGridColumn DataField="IsNotLocalized" Width="15px" TextAlign="Center" Type="CheckBox" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <ActionBar PagerVisible="False">
                                    <Actions>
                                        <Save Enabled="False" />
                                        <AddNew Enabled="False" />
                                        <Delete Enabled="False" />
                                        <EditRecord Enabled="False" />
                                        <NoteShow Enabled="False" />
                                        <ExportExcel MenuVisible="False" ToolBarVisible="False" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton>
                                            <AutoCallBack Command="viewExceptionalUsageDetails" Target="ds" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Obsolete" Visible="True">
                <Template>
                    <px:PXSplitContainer runat="server" ID="sp2" SplitterPosition="300" SkinID="Horizontal">
                        <AutoSize Enabled="true" Container="Parent" />
                        <Template1>
                            <px:PXGrid ID="gridValueObsolete" runat="server" DataSourceID="ds" SyncPosition="True"
                                Width="100%" AutoAdjustColumns="True" AdjustPageSize="Auto" AllowPaging="True"
                                Caption="Default Values" AllowSearch="True" SkinID="Details" AutoSize="true" FastFilterFields="NeutralValue" CaptionVisible="True">
                                <Levels>
                                    <px:PXGridLevel DataMember="DeltaResourcesDistinctObsolete">
                                        <Columns>
                                            <px:PXGridColumn DataField="Id" Width="50" />
                                            <px:PXGridColumn DataField="NeutralValue" Width="90px" />
                                            <px:PXGridColumn DataField="IsNotLocalized" Width="15px" TextAlign="Center" Type="CheckBox" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <Mode AllowAddNew="False" AllowDelete="False" AllowUpload="true" />
                                <AutoSize Enabled="True" Container="Parent" />
                                <ActionBar PagerVisible="False">
                                    <Actions>
                                        <Save Enabled="False" />
                                        <AddNew Enabled="False" />
                                        <Delete Enabled="False" />
                                        <EditRecord Enabled="False" />
                                        <NoteShow Enabled="False" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton>
                                            <AutoCallBack Command="viewUsageObsoleteDetails" Target="ds" />
                                        </px:PXToolBarButton>
                                        <px:PXToolBarButton>
                                            <AutoCallBack Command="deleteObsoleteStrings" Target="ds" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                                <AutoCallBack Command="Refresh" Target="gridValueExceptionalObsolete" />
                            </px:PXGrid>
                        </Template1>
                        <Template2>
                            <px:PXGrid ID="gridValueExceptionalObsolete" runat="server" DataSourceID="ds" SyncPosition="True"
                                Width="100%" AutoAdjustColumns="True" AdjustPageSize="Auto" AllowPaging="False"
                                Caption="Key-Specific Values" SkinID="Details" AutoSize="true" CaptionVisible="True">
                                <AutoSize Enabled="True" Container="Parent" />
                                <Mode AllowAddNew="False" AllowDelete="False" />
                                <Levels>
                                    <px:PXGridLevel DataMember="ExceptionalResourcesObsolete">
                                        <Columns>
                                            <px:PXGridColumn DataField="IdRes" Width="50" />
                                            <px:PXGridColumn DataField="ResKey" Width="90px" />
                                            <px:PXGridColumn DataField="IsNotLocalized" Width="15px" TextAlign="Center" Type="CheckBox" />
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <ActionBar PagerVisible="False">
                                    <Actions>
                                        <Save Enabled="False" />
                                        <AddNew Enabled="False" />
                                        <Delete Enabled="False" />
                                        <EditRecord Enabled="False" />
                                        <NoteShow Enabled="False" />
                                        <ExportExcel MenuVisible="False" ToolBarVisible="False" />
                                    </Actions>
                                    <CustomItems>
                                        <px:PXToolBarButton>
                                            <AutoCallBack Command="viewExceptionalUsageObsoleteDetails" Target="ds" />
                                        </px:PXToolBarButton>
                                    </CustomItems>
                                </ActionBar>
                            </px:PXGrid>
                        </Template2>
                    </px:PXSplitContainer>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
    
    <px:PXSmartPanel ID="edUsageDetails" runat="server" Caption="Usage Details" LoadOnDemand="True"
        ContentLayout-OuterSpacing="None" CaptionVisible="True" Key="UsageDetails"
        Width="500px" Height="400px" CommandSourceID="ds" AutoCallBack-Command="Refresh" AutoCallBack-Behavior-RepaintControlsIDs="edUsageGrid">
        <px:PXGrid ID="edUsageGrid" runat="server" DataSourceID="ds" SkinID="Details"  Width="100%" AutoAdjustColumns="True" SyncPosition="True">
            <Levels>
                <px:PXGridLevel DataMember="UsageDetails">
                    <Columns>
                        <px:PXGridColumn DataField="ScreenID" Width="100px" />
                        <px:PXGridColumn DataField="Title" Width="300px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="250" />
            <ActionBar PagerVisible="False">
                <Actions>                    
                    <AddNew Enabled="False" />
                    <Delete Enabled="False" />
                </Actions>
            </ActionBar>
        </px:PXGrid>
        <px:PXPanel ID="edUsagePanel" runat="server" SkinID="Buttons" Width="100%">
            <px:PXButton ID="edUsageOk" runat="server" DialogResult="OK" Text="Open UI Element" />
			<px:PXButton ID="edUsageCnl" runat="server" DialogResult="Cancel" Text="Close" />
		</px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
