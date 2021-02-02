<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="PJ201000.aspx.cs" Inherits="Page_PJ201000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" AutoCallBack="True" Visible="True" Width="100%"
        PrimaryView="ProjectManagementClasses" TypeName="PX.Objects.PJ.ProjectManagement.PJ.Graphs.ProjectManagementClassMaint">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%"
        DataMember="ProjectManagementClasses" Caption="Request For Information Class Summary" FilesIndicator="True" NoteIndicator="True"
        ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edProjectManagementClassId">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XL" />
            <px:PXSelector ID="edProjectManagementClassId" runat="server" DataField="ProjectManagementClassId" Size="SM" FilterByAllFields="True" CommitChanges="True"/>
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" ID="CstPXLayoutRule3" Merge="True" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="USE FOR" />
            <px:PXCheckBox ID="chkUseForProjectIssue" runat="server" DataField="UseForProjectIssue" CommitChanges="True" />
            <px:PXCheckBox ID="chkUseForRequestForInformation" runat="server" DataField="UseForRequestForInformation" CommitChanges="True"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="253px" DataSourceID="ds" DataMember="ProjectManagementClassesCurrent"
        LoadOnDemand="True">
        <Items>
            <px:PXTabItem Text="Details" RepaintOnDemand="False">
                <Template>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="REQUEST FOR INFORMATION SETTINGS" />
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" LabelsWidth="M" ControlSize="M" />
                    <px:PXNumberEdit ID="edRequestForInformationResponseTimeFrame" runat="server"
                        DataField="RequestForInformationResponseTimeFrame" Size="XS" CommitChanges="True" />
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="PROJECT ISSUE SETTINGS" />
                    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" LabelsWidth="M" ControlSize="M" />
                    <px:PXNumberEdit ID="edProjectIssueResponseTimeFrame" runat="server"
                        DataField="ProjectIssueResponseTimeFrame" Size="XS" CommitChanges="True" />
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
            <px:PXTabItem Text="Priorities" RepaintOnDemand="False">
                <Template>
                    <px:PXGrid ID="PXGrid1" runat="server" SkinID="Details" ActionsPosition="Top"
                               DataSourceID="ds" Width="100%" BorderWidth="0px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="ProjectManagementClassPriority">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edSortOrder" runat="server" DataField="SortOrder" AllowEdit="True"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" Type="CheckBox" CommitChanges="True" TextAlign="Center"/>
                                    <px:PXGridColumn DataField="PriorityName" AllowNull="False"/>
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Center"/>
                                    <px:PXGridColumn DataField="IsDefault" Type="CheckBox" CommitChanges="True" TextAlign="Center"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
