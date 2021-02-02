<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="TX502000.aspx.cs" Inherits="Page_TX502000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.TX.ReportTaxReview" PrimaryView="Period_Header" PageLoadBehavior="PopulateSavedValues">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="AdjustTax" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="VoidReport" PostData="Self" />
            <px:PXDSCallbackCommand Name="ClosePeriod" PostData="Self" />
            <px:PXDSCallbackCommand Name="ViewDocument" PostData="Self" Visible="false" DependOnGrid="grid" />
            <px:PXDSCallbackCommand Name="CheckDocument" PostData="Self" Visible="false" DependOnGrid="gridDocument" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" Width="100%" DataMember="Period_Header" Caption="Selection" TabIndex="4500">
        <Template>
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edOrganizationID" runat="server" DataField="OrganizationID" AllowEdit="true" CommitChanges="true" />
            <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" AllowEdit="true" CommitChanges="true" AutoRefresh="True"/>
            <px:PXSegmentMask ID="edVendorID" runat="server" DataField="VendorID" AllowEdit="true" CommitChanges="true" />
            <px:PXSelector Size="S" ID="edTaxPeriodID" runat="server" DataField="TaxPeriodID" CommitChanges="true" />
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXSelector ID="RevisionId" runat="server" DataField="RevisionId" Size="s" AutoRefresh="true" CommitChanges="true" />
            <px:PXCheckBox runat="server" ID="ShowDifference" DataField="ShowDifference" CommitChanges="true" />
            <px:PXLayoutRule Merge="False" runat="server" />
            <px:PXDateTimeEdit ID="edStartDate" runat="server" DataField="StartDate" />
            <px:PXDateTimeEdit ID="edEndDate" runat="server" DataField="EndDate" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataMember="Period_Details">
        <Items>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXGrid ID="grid" runat="server" Height="150px" Width="100%" ActionsPosition="Top" SkinID="Inquire">
                        <Levels>
                            <px:PXGridLevel DataMember="Period_Details">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="M" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edLineNbr" runat="server" DataField="LineNbr" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edSortOrder" runat="server" DataField="SortOrder" />
                                    <px:PXTextEdit SuppressLabel="True" ID="edLineDescr" runat="server" DataField="Descr" />
                                    <px:PXNumberEdit SuppressLabel="True" ID="edTaxHistory__ReportFiledAmt" runat="server" DataField="TaxHistory__ReportFiledAmt" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="LineNbr" TextAlign="Right" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Descr" />
                                    <px:PXGridColumn DataField="TaxHistory__ReportFiledAmt" TextAlign="Right" LinkCommand="ViewDocument" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="AP Documents">
                <Template>
                    <px:PXGrid ID="gridDocument" runat="server" Height="150px" Width="100%" BorderWidth="0px" ActionsPosition="Top" Caption="Details" SkinID="Inquire" SyncPosition="true">
                        <Levels>
                            <px:PXGridLevel DataMember="APDocuments">
                                <Columns>
                                    <px:PXGridColumn DataField="BranchID" />
                                    <px:PXGridColumn DataField="DocType" />
                                    <px:PXGridColumn DataField="RefNbr" LinkCommand="CheckDocument" />
                                    <px:PXGridColumn DataField="DocDate" />
                                    <px:PXGridColumn DataField="FinPeriodID" />
                                    <px:PXGridColumn DataField="VendorID" />
                                    <px:PXGridColumn DataField="CuryID" />
                                    <px:PXGridColumn DataField="CuryOrigDocAmt" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="CuryDocBal" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Status" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" />
    </px:PXTab>
</asp:Content>
