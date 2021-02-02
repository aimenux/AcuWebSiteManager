<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS204400.aspx.cs" Inherits="Page_FS204400" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        BorderStyle="NotSet" PrimaryView="ManufacturerRecords" SuspendUnloading="False" 
        TypeName="PX.Objects.FS.ManufacturerMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="ViewMainOnMap" Visible="false" ></px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" 
        Style="z-index: 100" Width="100%" DataMember="ManufacturerRecords" TabIndex="500" DefaultControlID="edManufacturerCD">
		<Template>
            <px:PXLayoutRule runat="server" StartRow="True" 
                StartColumn="True" ControlSize="XM" LabelsWidth="SM">
            </px:PXLayoutRule>
            <px:PXSelector ID="edManufacturerCD" runat="server" DataField="ManufacturerCD">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
            </px:PXTextEdit>
            <px:PXSelector ID="edContactID" runat="server" AllowEdit="True" 
                CommitChanges="True" DataField="ContactID" DisplayMode="Text">
            </px:PXSelector>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="280px" DataSourceID="ds" DataMember="CurrentManufacturer">
        <Items>
            <px:PXTabItem Text="Manufacturer Details">
                <Template>
                    <px:PXFormView ID="edManufacturerCurrent" runat="server" DataMember="CurrentManufacturer" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXCheckBox ID="edAllowOverrideContactAddress" runat="server" DataField="AllowOverrideContactAddress" CommitChanges="true"/>
                        </Template>
                        <ContentStyle BackColor="Transparent"/>
                    </px:PXFormView>
                    <px:PXLayoutRule runat="server" StartRow="True"/>
                    <px:PXFormView ID="edManufacturer_Contact" runat="server" Caption="Main Contact" DataMember="Manufacturer_Contact" DataSourceID="ds" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />
                            <px:PXTextEdit ID="edFullName" runat="server" DataField="FullName" CommitChanges="true"/>
                            <px:PXTextEdit ID="edAttention" runat="server" DataField="Attention" />
                            <px:PXMailEdit ID="edEMail" runat="server" CommandSourceID="ds" DataField="EMail" CommitChanges="True"/>
                            <px:PXLinkEdit ID="edWebSite" runat="server" DataField="WebSite" CommitChanges="True"/>
                            
                            <px:PXLayoutRule runat="server" Merge="True" ControlSize="XM" />
                            <px:PXDropDown ID="edPhone1Type" runat="server" DataField="Phone1Type" SuppressLabel="True" CommitChanges="True" Width="133px"/>
                            <px:PXLabel ID="LPhone1" runat="server" SuppressLabel="true" />
                            <px:PXMaskEdit ID="edPhone1" runat="server" DataField="Phone1" LabelWidth="0px" Size="XM" LabelID="LPhone1" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule runat="server" Merge="True" ControlSize="XM" SuppressLabel="True" />
                            <px:PXDropDown ID="edPhone2Type" runat="server" DataField="Phone2Type" SuppressLabel="True" CommitChanges="True" Width="133px"/>
                            <px:PXLabel ID="LPhone2" runat="server" SuppressLabel="true" />
                            <px:PXMaskEdit ID="edPhone2" runat="server" DataField="Phone2" LabelWidth="0px" Size="XM" LabelID="LPhone2" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule runat="server" Merge="True" ControlSize="XM" SuppressLabel="True" />
                            <px:PXDropDown ID="edFaxType" runat="server" DataField="FaxType" SuppressLabel="True" CommitChanges="True" Width="133px" />
                            <px:PXLabel ID="LFax" runat="server" SuppressLabel="true" />
                            <px:PXMaskEdit ID="edFax" runat="server" DataField="Fax" LabelWidth="0px" Size="XM" LabelID="LFax" SuppressLabel="True" CommitChanges="true"/>
                            <px:PXLayoutRule runat="server" />                  
                       </Template>
                    </px:PXFormView>
                    
                    <px:PXLayoutRule runat="server" StartRow="True"/>    
                    <px:PXFormView ID="edManufacturer_Address" runat="server" Caption="Main Address" DataMember="Manufacturer_Address" DataSourceID="ds" RenderStyle="Fieldset">
                        <Template>
                            <px:PXLayoutRule runat="server" ControlSize="XM" LabelsWidth="SM" />                    
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" CommitChanges="true"/>
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
                       <ContentStyle BackColor="Transparent" />
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
    </px:PXTab>
</asp:Content>
