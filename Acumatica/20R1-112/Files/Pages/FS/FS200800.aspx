<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS200800.aspx.cs" Inherits="Page_FS200800" Title="Untitled Page" %>
    <%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

        <asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
            <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" PrimaryView="EquipmentTypeRecords" TypeName="PX.Objects.FS.EquipmentTypeMaint">
                <CallbackCommands>
                    <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
                    <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
                    <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
                    <px:PXDSCallbackCommand Name="Last" PostData="Self" />
                </CallbackCommands>
            </px:PXDataSource>
        </asp:Content>
        <asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
            <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="EquipmentTypeRecords"
            TabIndex="900" DefaultControlID="edEquipmentTypeID" FilesIndicator="true">
                <Template>
                    <px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" LabelsWidth="SM">
                    </px:PXLayoutRule>
                    <px:PXSelector ID="edEquipmentTypeCD" runat="server" DataField="EquipmentTypeCD">
                    </px:PXSelector>
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
                    </px:PXTextEdit>
                    <px:PXCheckBox ID="edRequireBranchLocation" runat="server" 
                        DataField="RequireBranchLocation" Text="Require Branch Location">
                    </px:PXCheckBox>
                </Template>
            </px:PXFormView>
        </asp:Content>
        <asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
            <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="CurrentEquipmentTypeRecord" MarkRequired="Dynamic">
                <Items>
                    <px:PXTabItem Text="Attributes">
                    <Template>
                        <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
                            border: 0px;" Width="100%" ActionsPosition="Top" SkinID="Details"  MatrixMode="True">
                            <Levels>
                                <px:PXGridLevel DataMember="Mapping">
                                    <RowTemplate>
                                        <px:PXSelector CommitChanges="True" ID="edAttributeID" runat="server" DataField="AttributeID" AllowEdit="True" FilterByAllFields="True" />
                                    </RowTemplate>
                                    <Columns>
                                        <px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                                        <px:PXGridColumn DataField="AttributeID" AutoCallBack="true" LinkCommand="CRAttribute_ViewDetails" />
                                        <px:PXGridColumn AllowNull="False" DataField="Description" />
                                        <px:PXGridColumn DataField="SortOrder" TextAlign="Right" SortDirection="Ascending" />
                                        <px:PXGridColumn AllowNull="False" DataField="Required" TextAlign="Center" Type="CheckBox" />
                                        <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
                                        <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" />
                                        <px:PXGridColumn AllowNull="True" DataField="DefaultValue" RenderEditorText="False" />
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
        </asp:Content>