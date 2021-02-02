<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="GL201500.aspx.cs" Inherits="Page_GL201500" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<px:pxdatasource id="ds" width="100%" runat="server" typename="PX.Objects.GL.GeneralLedgerMaint" primaryview="LedgerRecords" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
		    <px:PXDSCallbackCommand Name="DeleteOrganizationLedgerLink" Visible="false"/>
		    <px:PXDSCallbackCommand Name="ViewOrganization" Visible="false"/>
		</CallbackCommands>
	</px:pxdatasource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="Ledger" runat="server" Width="100%" Caption="Ledger Summary" DataMember="LedgerRecords" DefaultControlID="edLedgerCD">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
            <px:PXSelector ID="edLedgerCD" runat="server" DataField="LedgerCD" />
            <px:PXTextEdit CommitChanges="True" ID="edDescr" runat="server" DataField="Descr" />
            <px:PXDropDown ID="edType" runat="server" DataField="BalanceType" CommitChanges="True" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" />
            <px:PXSelector ID="edBaseCuryID" runat="server" DataField="BaseCuryID" />
            <px:PXCheckBox ID="chkConsolAllowed" runat="server" DataField="ConsolAllowed" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="580px" Style="z-index: 100" Width="100%" DataMember="OrganizationLedgerLinkWithOrganizationSelect" DataSourceID="ds">
        <AutoSize Enabled="True" Container="Window" MinWidth="300" MinHeight="250"></AutoSize>
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Companies" >
                <Template>
                    <px:PXGrid ID="grdOrganizaionLinks" runat="server" Height="300px" Width="100%" AllowSearch="True" SkinID="DetailsInTab" DataSourceID="ds" SyncPosition="True" AdjustPageSize="Auto">
                        <ActionBar>
                            <Actions>
                                <EditRecord ToolBarVisible="False" />
                                <Delete ToolBarVisible="False"/>
                            </Actions>
                            <CustomItems>
                                <px:PXToolBarButton ImageKey="RecordDel" DisplayStyle="Image">
                                    <AutoCallBack Command="DeleteOrganizationLedgerLink" Target="ds" />
                                </px:PXToolBarButton>
                            </CustomItems>
                        </ActionBar>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <Levels>
                            <px:PXGridLevel DataMember="OrganizationLedgerLinkWithOrganizationSelect">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edOrganizarionCD" runat="server" DataField="OrganizationID" CommitChanges="True"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="OrganizationID" AutoCallBack="True" LinkCommand="ViewOrganization"/>
                                    <px:PXGridColumn DataField="Organization__OrganizationName"/>
                                    <px:PXGridColumn DataField="Organization__Active" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Organization__OrganizationType"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Branches">
                <Template>
                    <px:PXGrid ID="grdBranches" runat="server" Height="300px" Width="100%" AllowSearch="True" SkinID="DetailsInTab" DataSourceID="ds" AdjustPageSize="Auto">
                        <ActionBar>
                            <Actions>
                                <AddNew ToolBarVisible="False" />
                                <Delete ToolBarVisible="False" />
                            </Actions>
                        </ActionBar>
                        <AutoSize Enabled="True" MinHeight="100" MinWidth="100" />
                        <Levels>
                            <px:PXGridLevel DataMember="BranchesView">
                                <Columns>
                                    <px:PXGridColumn DataField="BranchCD"/>
                                    <px:PXGridColumn DataField="AcctName"/>
                                    <px:PXGridColumn DataField="Active" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="Organization__OrganizationName"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>
