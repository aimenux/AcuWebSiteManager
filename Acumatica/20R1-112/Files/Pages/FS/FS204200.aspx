<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS204200.aspx.cs" Inherits="Page_FS204200" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="VehicleTypeRecords" SuspendUnloading="False" 
        TypeName="PX.Objects.FS.VehicleTypeMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds"
        Style="z-index: 100" Width="100%" DataMember="VehicleTypeRecords" 
        TabIndex="500" DefaultControlID="edVehicleTypeCD" FilesIndicator="true">
		<Template>
            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" StartRow="True">
            </px:PXLayoutRule>
            <px:PXSelector ID="edVehicleTypeCD" runat="server"  AutoRefresh="True"
                DataField="VehicleTypeCD">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
            </px:PXTextEdit>
        </Template>
		<AutoSize Container="Window" Enabled="True"/>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab runat="server" Height="150px" Style="z-index: 100" Width="100%" DataMember="VehicleTypeSelected" DataSourceID="ds">
        <AutoSize Enabled="True" Container="Window" MinWidth="300" MinHeight="250"></AutoSize>        
        <Items>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="AttributesGrid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100;
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

