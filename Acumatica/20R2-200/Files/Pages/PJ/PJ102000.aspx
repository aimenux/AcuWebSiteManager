<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="PJ102000.aspx.cs" Inherits="Page_PJ102000" Title="Drawing Log Preferences" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="DrawingLogSetup"
        TypeName="PX.Objects.PJ.DrawingLogs.PJ.Graphs.DrawingLogsSetupMaint" BorderStyle="NotSet">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="PasteLine" Visible="False" CommitChanges="true" DependOnGrid="gridDisciplines" />
            <px:PXDSCallbackCommand Name="ResetOrder" Visible="False" CommitChanges="true" DependOnGrid="gridDisciplines" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="487px" Style="z-index: 100"  DataMember="DrawingLogSetup"
        Width="100%">
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Items>
            <px:PXTabItem Text="General">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="L" ControlSize="L" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Drawing Log Settings" />
                    <px:PXSelector ID="edDrawingLogNumberingSequenceId" runat="server" DataField="DrawingLogNumberingSequenceId" AllowEdit="True" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Disciplines">
                <Template>
                    <px:PXGrid ID="gridDisciplines" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                               Width="100%" ActionsPosition="Top" SkinID="Details" Caption="Disciplines" TabIndex="1100"
                               AllowPaging="True" AdjustPageSize="Auto" NewRowActive="True">
                        <Levels>
                            <px:PXGridLevel DataMember="DrawingLogDisciplines">
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" TextAlign="Center" Type="CheckBox" Width="60px" AllowDragDrop="true" />
                                    <px:PXGridColumn DataField="Name" Width="200px" AllowDragDrop="true" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Visible="False" Width="0px" />
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" Visible="False" Width="0px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <CallbackCommands PasteCommand="PasteLine">
                            <Save PostData="Container" />
                        </CallbackCommands>
                        <Mode AllowDragRows="true" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Status">
                <Template>
                    <px:PXGrid ID="gridStatuses" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                               Width="100%" ActionsPosition="Top" SkinID="Details" Caption="Statuses" TabIndex="1100"
                               AllowPaging="True" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="DrawingLogStatuses">
                                <Columns>
                                    <px:PXGridColumn DataField="Name" Width="200px" />
                                    <px:PXGridColumn DataField="Description" Width="200px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="AttributesGrid" runat="server" SkinID="Details" ActionsPosition="Top"
                        DataSourceID="ds" Width="100%" BorderWidth="0px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Attributes">
                                <RowTemplate>
                                    <px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="AttributeID" Width="81px" AutoCallBack="True" LinkCommand="CRAttribute_ViewDetails" />
                                    <px:PXGridColumn AllowNull="False" DataField="Description" Width="351px" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="81px" />
                                    <px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" Width="81px" />
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" Width="100px" RenderEditorText="False" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>
