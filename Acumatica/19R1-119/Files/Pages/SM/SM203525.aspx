<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="SM203525.aspx.cs" Inherits="Page_SM203525"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="CalculationHistory" TypeName="PX.SM.SpaceUsageMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="calculateUsedSpaceCommand" StartNewGroup="True" PopupVisible="True" />
            <px:PXDSCallbackCommand Name="viewCompanyTables" Visible="False" DependOnGrid="gridCompanies"/>
            <px:PXDSCallbackCommand Name="viewSnapshotTables" Visible="False" DependOnGrid="gridSnapshots"/>
            <px:PXDSCallbackCommand Name="viewCompaniesByTable" Visible="False" DependOnGrid="gridTables"/>
            <px:PXDSCallbackCommand Name="ViewCompany" Visible="False" DependOnGrid="gridCompanies" />
            <px:PXDSCallbackCommand Name="ViewSnapshot" Visible="False" DependOnGrid="gridSnapshots" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="CalculationHistory" EmailingGraph="" Caption="Company Summary" TemplateContainer="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="200px" ControlSize="SM" />
            <px:PXTextEdit SkinID="Label" ID ="edCalculationDate" runat="server" DataField="CalculationDate" />
            <px:PXLabel runat="server" Text="Used Database Space:" />
            <px:PXPanel runat="server" RenderStyle="Simple" Style="padding-left:30px">
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="170px" ControlSize="SM" />
                <px:PXTextEdit SkinID="Label" ID ="PXTextEdit1" runat="server" DataField="UsedTotal" />
                <px:PXTextEdit SkinID="Label" ID ="edUsedByCompanies" runat="server" DataField="UsedByCompanies" />
                <px:PXTextEdit SkinID="Label" ID ="edUsedBySnapshots" runat="server" DataField="UsedBySnapshots" />
            </px:PXPanel>
            <px:PXTextEdit SkinID="Label" ID ="edFreeSpace" runat="server" DataField="FreeSpace" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XL" />
            <px:PXTextEdit SkinID="Label" ID ="edQuotaSize" runat="server" DataField="QuotaSize" />
            <px:PXLabel runat="server" />
            <px:PXLabel runat="server" Text="&nbsp;" />
            <px:PXLabel runat="server" />
            <px:PXLabel runat="server" Text="&nbsp;" />
            <px:PXTextEdit SkinID="Label" ID ="edCurrentStatus" runat="server" DataField="CurrentStatus" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXSmartPanel ID="pnlViewCompanyTables" runat="server" Height="600px" Width="1000px" Caption="Table Sizes by Tenant"
        CaptionVisible="true" Key="PopupCompanyTablesDefinition" AutoRepaint="true" AllowResize="false">
        <px:PXFormView ID="frmViewCompanyTables" runat="server" DataSourceID="ds" Width="100%"
            DataMember="PopupCompanyTablesDefinition" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                <px:PXTextEdit SkinID="Label" ID ="edCompany" runat="server" DataField="CompanyName" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                <px:PXTextEdit SkinID="Label" ID ="edSizeMB" runat="server" DataField="SizeMB" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="gridCompanyTables" runat="server" DataSourceID="ds" Style="position: static" FeedbackMode="DisableAll"
            Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="Details"
            AutoAdjustColumns="true" SyncPosition="true" FilesIndicator="false" NoteIndicator="false">
            <Levels>
                <px:PXGridLevel DataMember="PopupCompanyTablesDefinition">
                    <Columns>
                        <px:PXGridColumn AllowUpdate="False" DataField="TableName" Label="TableName" Width="200px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="CountOfCompanyRecords" Label="CountOfCompanyRecords" Width="200px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="FullSizeByCompanyMB" Label="FullSizeByCompanyMB" Width="200px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="150" />
            <ActionBar>
                <Actions>
                    <AddNew Enabled="False" />
                    <Delete Enabled="False" />
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" />
        </px:PXGrid>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlViewSnapshotTables" runat="server" Height="600px" Width="1000px" Caption="Table Sizes by Snapshot"
        CaptionVisible="true" Key="PopupSnapshotTablesDefinition" AutoRepaint="true" AllowResize="false">
        <px:PXFormView ID="frmViewSnapshotTables" runat="server" DataSourceID="ds" Width="100%"
            DataMember="PopupSnapshotTablesDefinition" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                <px:PXTextEdit SkinID="Label" ID ="edSnapshot" runat="server" DataField="SnapshotName" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                <px:PXTextEdit SkinID="Label" ID ="edSizeMB" runat="server" DataField="SizeMB" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="gridSnapshotTables" runat="server" DataSourceID="ds" Style="position: static" FeedbackMode="DisableAll"
            Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="Details"
            AutoAdjustColumns="true" SyncPosition="true" FilesIndicator="false" NoteIndicator="false">
            <Levels>
                <px:PXGridLevel DataMember="PopupSnapshotTablesDefinition">
                    <Columns>
                        <px:PXGridColumn AllowUpdate="False" DataField="TableName" Label="TableName" Width="200px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="CountOfCompanyRecords" Label="CountOfCompanyRecords" Width="200px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="FullSizeByCompanyMB" Label="FullSizeByCompanyMB" Width="200px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="150" />
            <ActionBar>
                <Actions>
                    <AddNew Enabled="False" />
                    <Delete Enabled="False" />
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" />
        </px:PXGrid>
    </px:PXSmartPanel>
    <px:PXSmartPanel ID="pnlViewCompaniesByTable" runat="server" Height="600px" Width="1000px" Caption="Used Space by Tenants and Snapshots"
        CaptionVisible="true" Key="PopupCompaniesByTableDefinition" AutoRepaint="true" AllowResize="false">
        <px:PXFormView ID="frmViewCompaniesByTable" runat="server" DataSourceID="ds" Width="100%"
            DataMember="PopupCompaniesByTableDefinition" SkinID="Transparent">
            <Template>
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                <px:PXTextEdit SkinID="Label" ID ="edTableName" runat="server" DataField="TableName" />
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                <px:PXTextEdit SkinID="Label" ID ="edSize" runat="server" DataField="Size" />
            </Template>
        </px:PXFormView>
        <px:PXGrid ID="gridCompaniesByTable" runat="server" DataSourceID="ds" Style="position: static" FeedbackMode="DisableAll"
            Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="Details"
            AutoAdjustColumns="true" SyncPosition="true" FilesIndicator="false" NoteIndicator="false">
            <Levels>
                <px:PXGridLevel DataMember="PopupCompaniesByTableDefinition">
                    <Columns>
                        <px:PXGridColumn AllowUpdate="False" DataField="Type" Label="Type" Width="200px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="Name" Label="Name" Width="200px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="CountOfCompanyRecords" Label="CountOfCompanyRecords" Width="200px" />
                        <px:PXGridColumn AllowUpdate="False" DataField="FullSizeByCompanyMB" Label="FullSizeByCompanyMB" Width="200px" />
                    </Columns>
                </px:PXGridLevel>
            </Levels>
            <AutoSize Enabled="True" MinHeight="150" />
            <ActionBar>
                <Actions>
                    <AddNew Enabled="False" />
                    <Delete Enabled="False" />
                </Actions>
            </ActionBar>
            <Mode AllowAddNew="False" />
        </px:PXGrid>
    </px:PXSmartPanel>
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" DataMember="CalculationHistory"
        EmailingGraph="">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
        <Items>
            <px:PXTabItem Text="Tenants">
                <Template>
                    <px:PXGrid ID="gridCompanies" runat="server" DataSourceID="ds" Style="position: static" FeedbackMode="DisableAll"
                        Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="DetailsInTab"
                        AutoAdjustColumns="true" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="Companies">
                                <Columns>
                                    <px:PXGridColumn AllowUpdate="False" DataField="Current" Label="Current" TextAlign="Center" Type="CheckBox" Width="70px" AllowResize="false" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CompanyID" Label="CompanyID" TextAlign="Left" Width="200px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CompanyCD" Label="CompanyCD" TextAlign="Left" Width="200px" LinkCommand="ViewCompany" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="LoginName" Label="LoginName" TextAlign="Left" Width="200px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Status" Label="Status" TextAlign="Left" Width="200px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="SizeMB" Label="SizeMB" TextAlign="Right" Width="200px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="False" />
                                <Delete Enabled="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Text="View Tables">
                                    <AutoCallBack Command="viewCompanyTables" Target="ds" />
                                    <ActionBar GroupIndex="2" Order="3" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Snapshots">
                <Template>
                    <px:PXGrid ID="gridSnapshots" runat="server" DataSourceID="ds" Style="position: static" FeedbackMode="DisableAll"
                        Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="DetailsInTab"
                        AutoAdjustColumns="true" SyncPosition="true" FilesIndicator="false" NoteIndicator="false">
                        <Levels>
                            <px:PXGridLevel DataMember="Snapshots">
                                <Columns>
                                    <px:PXGridColumn AllowUpdate="False" DataField="Name" Label="Name" Width="200px" LinkCommand="ViewSnapshot" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Description" Label="Description" Width="300px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CreatedDateTime" DisplayFormat="g" Label="Creation Date" Width="150px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="ExportMode" Label="ExportMode" Width="150px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="SizeInDbMB" Label="SizeInDbMB" Width="150px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="False" />
                                <Delete Enabled="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton>
                                    <AutoCallBack Command="viewSnapshotTables" Target="ds" />
                                    <ActionBar GroupIndex="2" Order="3" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Tables">
                <Template>
                    <px:PXGrid ID="gridTables" runat="server" DataSourceID="ds" Style="position: static" FeedbackMode="DisableAll"
                        Width="100%" ActionsPosition="Top" AdjustPageSize="Auto" AllowPaging="True" SkinID="DetailsInTab"
                        AutoAdjustColumns="true" SyncPosition="true" FilesIndicator="false" NoteIndicator="false">
                        <Levels>
                            <px:PXGridLevel DataMember="Tables">
                                <Columns>
                                    <px:PXGridColumn AllowUpdate="False" DataField="TableName" Label="TableName" Width="200px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="CountOfCompanyRecords" Label="CountOfCompanyRecords" Width="200px" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="FullSizeByCompanyMB" Label="FullSizeByCompanyMB" Width="200px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <ActionBar>
                            <Actions>
                                <AddNew Enabled="False" />
                                <Delete Enabled="False" />
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton Text="View Distribution By Companies/Snapshots">
                                    <AutoCallBack Command="viewCompaniesByTable" Target="ds" />
                                    <ActionBar GroupIndex="2" Order="3" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <Mode AllowAddNew="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>
