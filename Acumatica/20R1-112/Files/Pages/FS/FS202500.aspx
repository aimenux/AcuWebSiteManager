<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS202500.aspx.cs" Inherits="Page_FS202500" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="BranchLocationRecords" 
        TypeName="PX.Objects.FS.BranchLocationMaint" 
        SuspendUnloading="False">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="ViewMainOnMap" Visible="false" ></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand DependOnGrid="roomGrid" Name="OpenRoom" Visible="false" ></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="ValidateAddress" Visible="False"/>
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
    Width="100%" DataMember="BranchLocationRecords"
        TabIndex="1900" DefaultControlID="edBranchLocationCD" FilesIndicator="true">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" StartColumn="True" ControlSize="XM" LabelsWidth="SM">
            </px:PXLayoutRule>
            <px:PXSelector ID="edBranchLocationCD" runat="server" DataField="BranchLocationCD">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
            </px:PXTextEdit>
            <px:PXSelector ID="edBranchID" runat="server" DataField="BranchID" AllowEdit="True" AutoRefresh="True">
            </px:PXSelector>
            <px:PXCheckBox ID="edRoomFeatureEnabled" runat="server" DataField="RoomFeatureEnabled" AlignLeft="True" >
            </px:PXCheckBox>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" DataMember="CurrentBranchLocation" BindingContext="form">
        <Items>
            <px:PXTabItem Text="Branch Location Details">
                <Template>
                    <px:PXFormView ID="edBranchLocation_Contact" runat="server" Caption="Main Contact" DataMember="BranchLocation_Contact" DataSourceID="ds" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" CommitChanges="true"/>          
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXMailEdit ID="edEMail" runat="server" CommandSourceID="ds" DataField="EMail" CommitChanges="True"/>
                            <px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
                            <px:PXLayoutRule runat="server" Merge="True" ControlSize="SM" />
                            <px:PXDropDown ID="edPhone1Type" runat="server" DataField="Phone1Type" SuppressLabel="True" CommitChanges="True" Width="133px"/>
                            <px:PXLabel ID="LPhone1" runat="server" SuppressLabel="true" />
                            <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" LabelWidth="0px" Size="XM" LabelID="LPhone1" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule runat="server" Merge="True" ControlSize="SM" SuppressLabel="True" />
                            <px:PXDropDown ID="edPhone2Type" runat="server" DataField="Phone2Type" SuppressLabel="True" CommitChanges="True" Width="133px"/>
                            <px:PXLabel ID="LPhone2" runat="server" SuppressLabel="true" />
                            <px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" LabelWidth="0px" Size="XM" LabelID="LPhone2" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule runat="server" Merge="True" ControlSize="SM" SuppressLabel="True" />
                            <px:PXDropDown ID="edFaxType" runat="server" DataField="FaxType" SuppressLabel="True" CommitChanges="True" Width="133px" />
                            <px:PXLabel ID="LFax" runat="server" SuppressLabel="true" />
                            <px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" LabelWidth="0px" Size="XM" LabelID="LFax" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule runat="server" />         
                       </Template>
                        <ContentStyle BackColor="Transparent" />
                    </px:PXFormView>
                    <px:PXFormView ID="edBranchLocation_Address" runat="server" Caption="Main Address" DataMember="BranchLocation_Address" DataSourceID="ds" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" StartColumn="True" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" Caption="Contact" DataField="AddressLine1" CommitChanges="true"/>
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" CommitChanges="true"/>
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" CommitChanges="true"/>
                            <px:PXSelector ID="edCountryID" runat="server" AllowEdit="True" DataField="CountryID"
                                FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" CommitChanges="True" />
                            <px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="State" CommitChanges="true"
                                           FilterByAllFields="True" TextMode="Search" DisplayMode="Hint" />
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="s" CommitChanges="True" />
                            <px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewMainOnMap" CommandSourceID="ds" Text="View On Map" />
                            <px:PXLayoutRule runat="server" />
                       </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartColumn="True" GroupCaption="Financial Settings" ControlSize="XM" LabelsWidth="SM" StartGroup="True">
                    </px:PXLayoutRule>
                    <px:PXSegmentMask ID="edSubID" runat="server" DataField="SubID" DataSourceID="ds" AutoRefresh="True" AllowEdit="True">
                    </px:PXSegmentMask>
                    <px:PXLayoutRule runat="server" EndGroup="True"></px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" GroupCaption="Inventory Defaults" 
                        StartGroup="True" ControlSize="XM" LabelsWidth="SM">
                    </px:PXLayoutRule>
                    <px:PXSegmentMask ID="edDfltSiteID" runat="server" DataField="DfltSiteID" 
                        DataSourceID="ds">
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edDfltSubItemID" runat="server" DataField="DfltSubItemID" 
                        DataSourceID="ds" >
                    </px:PXSegmentMask>
                    <px:PXSelector ID="edDfltUOM" runat="server" DataField="DfltUOM" 
                        DataSourceID="ds" >
                    </px:PXSelector>
                    <px:PXLayoutRule runat="server" EndGroup="True">
                    </px:PXLayoutRule>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Rooms" BindingContext="form" VisibleExp="DataControls[&quot;edRoomFeatureEnabled&quot;].Value == true">
                <Template>
                    <px:PXGrid ID="roomGrid" runat="server" DataSourceID="ds" Style="z-index: 100" 
                        Width="100%" SkinID="Details" TabIndex="2300" 
                        NoteIndicator="False" FilesIndicator="False" AdjustPageSize="Auto" 
                        AllowPaging="True">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="BranchLocationID,RoomID" DataMember="RoomRecords">
                                <RowTemplate>
                                    <px:PXMaskEdit ID="edRoomID" runat="server" DataField="RoomID" CommitChanges="True">
                                    </px:PXMaskEdit>
                                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
                                    </px:PXTextEdit>
                                    <px:PXNumberEdit ID="edFloorNbr" runat="server" DataField="FloorNbr">
                                    </px:PXNumberEdit>
                                    <px:PXCheckBox ID="edSpecificUse" runat="server" DataField="SpecificUse" Text="Exclusive">
                                    </px:PXCheckBox>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="RoomID" LinkCommand="OpenRoom" CommitChanges="True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Descr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FloorNbr" TextAlign="Left">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SpecificUse" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
                        <ActionBar ActionsText="False">
                        </ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
    </px:PXTab>
</asp:Content>