<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS200900.aspx.cs" Inherits="Page_FS200900" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="LicenseTypeRecords" 
        TypeName="PX.Objects.FS.LicenseTypeMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Height="273px" 
        Style="z-index: 100" Width="100%" DataMember="LicenseTypeRecords" FilesIndicator="true"
        NoteIndicator="True" TabIndex="500" DefaultControlID="edLicenseTypeCD">
		<Template>                        
            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" 
                StartColumn="True">
            </px:PXLayoutRule>            
            <px:PXSelector ID="edLicenseTypeCD" runat="server" DataField="LicenseTypeCD" 
                DataSourceID="ds" AutoRefresh="True">
            </px:PXSelector>
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr">
            </px:PXTextEdit>
            <px:PXDropDown ID="edOwnerType" runat="server" DataField="OwnerType">
            </px:PXDropDown>
            <px:PXLayoutRule runat="server" ControlSize="M" LabelsWidth="SM" 
                StartRow="True">
            </px:PXLayoutRule>
            <px:PXLabel Text=" " runat="server" />
            <px:PXGroupBox ID="edValidIn" runat="server" Caption="Validation Settings" DataField="ValidIn" CommitChanges="True">
                <Template>
                    <px:PXRadioButton ID="edValidIn_op0" runat="server" Text="All Locations" Value="ALL" GroupName="edValidIn" ></px:PXRadioButton>
                    <px:PXRadioButton ID="edValidIn_op2" runat="server" Text="Service Area" Value="GZN" GroupName="edValidIn" ></px:PXRadioButton>
                    <px:PXRadioButton ID="edValidIn_op1" runat="server" Text="Other" Value="CSC" GroupName="edValidIn" ></px:PXRadioButton>
                </Template>
            </px:PXGroupBox>

            <px:PXLayoutRule runat="server" GroupCaption="Service Area Setting"></px:PXLayoutRule>
            <px:PXSelector ID="edGeoZoneID" runat="server" DataField="GeoZoneID" DataSourceID="ds" LabelsWidth="M">
            </px:PXSelector>
            <px:PXLayoutRule runat="server" EndGroup="True">
            </px:PXLayoutRule> 

            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" StartGroup="true" GroupCaption="Other Zone Settings">
            </px:PXLayoutRule> 
            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" DataSourceID="ds" CommitChanges="True">
            </px:PXSelector>
            <px:PXSelector ID="edState" runat="server" DataField="State" DataSourceID="ds" AutoRefresh="True">
            </px:PXSelector>
            <px:PXTextEdit ID="edCity" runat="server" DataField="City">
            </px:PXTextEdit>
            <px:PXLayoutRule runat="server" EndGroup="True">
            </px:PXLayoutRule> 
        </Template>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXFormView>
</asp:Content>
