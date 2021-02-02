<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN102000.aspx.cs"
    Inherits="Page_IN102000" Title="Warehouse Access Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INAccess" PrimaryView="Group">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Delete" Visible="false" />
            <px:PXDSCallbackCommand Name="CopyPaste" Visible="false" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="formGroup" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Group" Caption="Restriction Group"
        DefaultControlID="edGroupName" TabIndex="5700">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
            <px:PXSelector ID="edGroupName" runat="server" DataField="GroupName" DataSourceID="ds">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXDropDown ID="edGroupType" runat="server" AllowNull="False" DataField="GroupType" />
            <px:PXCheckBox ID="chkActive" runat="server" DataField="Active" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="168%" Style="z-index: 100" Width="100%">
        <Items>
            <px:PXTabItem Text="Users">
                <Template>
                    <px:PXGrid ID="gridUsers" BorderWidth="0px" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%"
                        AllowPaging="True" AdjustPageSize="Auto" AllowSearch="True" SkinID="Details" FastFilterFields="Username,Comment">
                        <Levels>
                            <px:PXGridLevel DataMember="Users">
                                <Mode AllowAddNew="True" AllowDelete="False" />
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edUsername" runat="server" DataField="Username" TextField="Username" />
                                    <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" />
                                    <px:PXTextEdit ID="edComment" runat="server" DataField="Comment" />
                                    <px:PXCheckBox ID="chkIncluded" runat="server" DataField="Included" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="40px" AllowCheckAll="True" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Username" Width="300px" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="FullName" Width="200px" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="Comment" Width="300px" AllowUpdate="False" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode AllowDelete="False" />
                        <EditPageParams>
                            <px:PXControlParam ControlID="gridUsers" Name="Username" PropertyName="DataValues[&quot;Username&quot;]" Type="String" />
                        </EditPageParams>
                        <ActionBar>
                            <Actions>
                                <Delete Enabled="False" />
                                <AddNew Enabled="False" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Warehouses">
                <Template>
                    <px:PXGrid SkinID="Details" ID="gridSite" BorderWidth="0px" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                        Width="100%" AllowPaging="True" AdjustPageSize="Auto" ActionsPosition="Top" AllowSearch="True" FastFilterFields="SiteCD,Descr">
                        <Levels>
                            <px:PXGridLevel DataMember="Site">
                                <Mode AllowAddNew="True" AllowDelete="False" />
                                <Columns>
                                    <px:PXGridColumn DataField="Included" TextAlign="Center" Type="CheckBox" Width="40px" AllowCheckAll="True" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="SiteCD" DisplayFormat="&gt;aaaaaaaaaa" Width="81px" RenderEditorText="true" />
                                    <px:PXGridColumn AllowUpdate="False" DataField="Descr" Width="351px" />
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSegmentMask ID="edSiteCD" runat="server" DataField="SiteCD" />
                                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" Enabled="False" />
                                    <px:PXCheckBox ID="chkSiteIncluded" runat="server" DataField="Included" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode AllowDelete="False" />
                        <ActionBar>
                            <Actions>
                                <Delete Enabled="False" />
                                <AddNew Enabled="False" />
                            </Actions>
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
    </px:PXTab>
</asp:Content>
