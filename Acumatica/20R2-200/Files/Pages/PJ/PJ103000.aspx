<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="PJ103000.aspx.cs" Inherits="Page_PJ103000" Title="Photo Log Preferences" %>

<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="PhotoLogSetup"
        TypeName="PX.Objects.PJ.PhotoLogs.PJ.Graphs.PhotoLogSetupMaint" BorderStyle="NotSet"/>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="487px" Style="z-index: 100"  DataMember="PhotoLogSetup"
        Width="100%">
        <AutoSize Container="Window" Enabled="True" MinHeight="200" />
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="L" />
                    <px:PXSelector ID="edPhotoLogNumberingId" runat="server" DataField="PhotoLogNumberingId" AllowEdit="True" />
                    <px:PXSelector ID="edPhotoNumberingId" runat="server" DataField="PhotoNumberingId" AllowEdit="True" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Status">
                <Template>
                    <px:PXGrid ID="gridStatuses" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                               Width="100%" ActionsPosition="Top" SkinID="Details" Caption="Status" TabIndex="1100"
                               AllowPaging="True" AdjustPageSize="Auto">
                        <Levels>
                            <px:PXGridLevel DataMember="PhotoLogStatuses">
                                <Columns>
                                    <px:PXGridColumn DataField="Name" />
                                    <px:PXGridColumn DataField="Description" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="gridAttributes" runat="server" SkinID="Details" ActionsPosition="Top"
                        DataSourceID="ds" Width="100%" BorderWidth="0px" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Attributes">
                                <RowTemplate>
                                    <px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="AttributeID" AutoCallBack="True" LinkCommand="CRAttribute_ViewDetails" />
                                    <px:PXGridColumn AllowNull="False" DataField="Description" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
                                    <px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" />
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="False" />
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
