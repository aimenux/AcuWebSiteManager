<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN204010.aspx.cs"
    Inherits="Page_IN204010" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INSiteBuildingMaint" PrimaryView="Buildings">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="ValidateAddresses" Visible="False" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="ViewOnMap" Visible="False" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Buildings" Caption="Warehouse Building Summary"
        NoteIndicator="True" FilesIndicator="True" ActivityIndicator="True" ActivityField="NoteActivity" DefaultControlID="edSiteCD"
        TabIndex="6900" AllowCollapse="true" Expanded="True">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
            <px:PXSelector ID="edBuildingCD" runat="server" DataField="BuildingCD" AutoRefresh="True" />
            <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Warehouses">
                <Template>
                    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100; left: 0px; top: 0px;" Width="100%"
                        ActionsPosition="Top" TabIndex="150" SkinID="DetailsWithFilter" AdjustPageSize="Auto" AllowPaging="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Sites">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" />
                                    <px:PXSegmentMask ID="edSiteCD" runat="server" DataField="SiteCD" TextMode="Search">
                                        <GridProperties FastFilterFields="Descr" />
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edSiteDescr" runat="server" DataField="Descr" Enabled="False"/>
                                    <px:PXCheckBox ID="edActive" runat="server" DataField="IsActive" Enabled="False"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="SiteCD" DisplayFormat="&gt;aaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"/>
                                    <px:PXGridColumn DataField="Descr"/>
                                    <px:PXGridColumn DataField="Active" DataType="Boolean" AllowNull="False" DefValueText="True" TextAlign="Center" Type="CheckBox"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowUpload="False" AllowAddNew="False" AllowUpdate="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Address Information">
                <Template>
                    <px:PXFormView ID="addrForm" runat="server" Caption="Address" DataMember="Address" DataSourceID="ds" RenderStyle="Simple" SkinID="Transparent">
                        <Template>
                            <px:PXLayoutRule runat="server" StartGroup="True" ControlSize="XM" LabelsWidth="SM" />
                            <px:PXCheckBox ID="edIsValidated" runat="server" DataField="IsValidated" Enabled="False" />
                            <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="AddressLine1" />
                            <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="AddressLine2" />
                            <px:PXTextEdit ID="edCity" runat="server" DataField="City" />
                            <px:PXSelector ID="edCountryID" runat="server" DataField="CountryID" CommitChanges="True" AllowEdit="True" />
                            <px:PXSelector ID="edState" runat="server" DataField="State" AutoRefresh="True" AllowEdit="True" >
                                <CallBackMode PostData="Container" />
                            </px:PXSelector>
                            <px:PXLayoutRule runat="server" Merge="True" />
                            <px:PXMaskEdit ID="edPostalCode" runat="server" DataField="PostalCode" Size="s" CommitChanges="True" />
                            <px:PXButton ID="btnViewMainOnMap" runat="server" CommandName="ViewOnMap" CommandSourceID="ds" Size="xs" Text="View on Map">
                            </px:PXButton>
                            <px:PXLayoutRule runat="server" />
                        </Template>
                    </px:PXFormView>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
    <%--Change ID dialog--%>
    <px:PXSmartPanel ID="pnlChangeID" runat="server" Caption="Specify New ID"
        CaptionVisible="true" DesignView="Hidden" LoadOnDemand="true" Key="ChangeIDDialog" CreateOnDemand="false" AutoCallBack-Enabled="true"
        AutoCallBack-Target="formChangeID" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page"
        AcceptButtonID="btnOK">
        <px:PXFormView ID="formChangeID" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" CaptionVisible="False"
            DataMember="ChangeIDDialog">
            <ContentStyle BackColor="Transparent" BorderStyle="None" />
            <Template>
                <px:PXLayoutRule ID="rlAcctCD" runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" />
                <px:PXTextEdit ID="edAcctCD" runat="server" DataField="CD" />
            </Template>
        </px:PXFormView>
        <px:PXPanel ID="pnlChangeIDButton" runat="server" SkinID="Buttons">
            <px:PXButton ID="btnOK" runat="server" DialogResult="OK" Text="OK">
                <AutoCallBack Target="formChangeID" Command="Save" />
            </px:PXButton>
            <px:PXButton ID="btnCancel" runat="server" DialogResult="Cancel" Text="Cancel" />
        </px:PXPanel>
    </px:PXSmartPanel>
</asp:Content>
