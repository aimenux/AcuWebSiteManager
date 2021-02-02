<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="FS201900.aspx.cs" Inherits="Page_FS201900" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        BorderStyle="NotSet" PrimaryView="GeoZoneRecords" 
        SuspendUnloading="False" 
        TypeName="PX.Objects.FS.GeoZoneMaint">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" DataMember="GeoZoneRecords" TabIndex="7900" DefaultControlID="GeoZoneCD">
        <Template>
            <px:PXLayoutRule runat="server" StartRow="True" ControlSize="M" LabelsWidth="SM">
            </px:PXLayoutRule>
            <px:PXSelector runat="server" DataField="GeoZoneCD" DataSourceID="ds" 
                ID="GeoZoneCD" AutoRefresh="True">
            </px:PXSelector>
            <px:PXTextEdit runat="server" DataField="Descr" ID="Descr">
            </px:PXTextEdit>
            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" DisplayMode="Hint" Enable="True">
            </px:PXSelector>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds" 
        DataMember="GeoZoneEmpRecords">
		<Items>
			<px:PXTabItem Text="Employees">
			    <Template>
                    <px:PXGrid ID="PXGrid1" runat="server" TabIndex="4900" DataSourceID="ds" 
                        SkinID="DetailsInTab" Height="100%" Width="100%">
                        <Levels>
                            <px:PXGridLevel 
                                DataMember="GeoZoneEmpRecords" DataKeyNames="GeoZoneID,EmployeeID">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="EmployeeID" runat="server" DataField="EmployeeID" Size="M" 
                                        AutoRefresh="True" AllowEdit="True">
                                    </px:PXSegmentMask>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="EmployeeID" TextAlign="Left" AutoCallBack = "True">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="EmployeeID_EPEmployee_acctName">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowUpload="True" />
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Postal Codes">
			    <Template>
                    <px:PXGrid ID="PXGrid2" runat="server" DataSourceID="ds" SkinID="DetailsInTab" 
                        TabIndex="2700" Height="100%" Width="100%">
                        <Levels>
                            <px:PXGridLevel DataKeyNames="GeoZoneID,PostalCode" 
                                DataMember="GeoZonePostalCodeRecords">
                                <RowTemplate>
                                    <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode">
                                    </px:PXMaskEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="PostalCode" CommitChanges="True">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowUpload="True" />
                    </px:PXGrid>
                </Template>
			</px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" />
	</px:PXTab>
</asp:Content>
