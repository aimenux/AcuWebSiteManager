<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PR209700.aspx.cs"
    Inherits="Page_PR209700" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" Width="100%" runat="server" TypeName="PX.Objects.PR.PRUnionMaint" PrimaryView="UnionLocal" Visible="True" />
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="UnionLocal" Width="100%" FastFilterFields="Description">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edUnionID" runat="server" DataField="UnionID" AutoRefresh="True" CommitChanges="True" Width="250px"/>
            <px:PXCheckBox ID="edIsActive" runat="server" DataField="IsActive" CommitChanges="True" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Width="250px" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXSelector ID="edVendorID" runat="server" DataField="VendorID" Width="350px" CommitChanges="true" />
            <px:PXSelector ID="edVendorLocationID" runat="server" DataField="VendorLocationID" Width="350px"/>
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="150px" DataSourceID="ds">
        <Items>
            <px:PXTabItem Text="Earning Rates">
                <Template>
                    <px:PXGrid ID="PXGrid1" runat="server" SkinID="DetailsInTab" SyncPosition="True" DataSourceID="ds" KeepPosition="True" Width="100%" Height="200px">
                        <Levels>
                            <px:PXGridLevel DataMember="EarningRates">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edRate" runat="server" DataField="Rate" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Rate" TextAlign="Right" CommitChanges="True" />
                                    <px:PXGridColumn DataField="EffectiveDate" Width="150px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Parent" Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Deductions and Benefits package">
                <Template>
                    <px:PXGrid runat="server" ID="PXGrid2" SkinID="DetailsInTab" SyncPosition="True" DataSourceID="ds" KeepPosition="True" Width="100%" Height="200px">
                        <Levels>
                            <px:PXGridLevel DataMember="DeductionsAndBenefitsPackage">
                                <RowTemplate>
                                    <px:PXNumberEdit ID="edDeductionAmount" runat="server" DataField="DeductionAmount" />
                                    <px:PXNumberEdit ID="edDeductionRate" runat="server" DataField="DeductionRate" />
									<px:PXNumberEdit ID="edBenefitAmount" runat="server" DataField="BenefitAmount" />
                                    <px:PXNumberEdit ID="edBenefitRate" runat="server" DataField="BenefitRate" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DeductionAndBenefitCodeID" CommitChanges="True" />
                                    <px:PXGridColumn DataField="LaborItemID" CommitChanges="true" />
                                    <px:PXGridColumn DataField="PRDeductCode__Description"/>
                                    <px:PXGridColumn DataField="PRDeductCode__ContribType" Width="300px" />
                                    <px:PXGridColumn DataField="PRDeductCode__DedCalcType" />
                                    <px:PXGridColumn DataField="DeductionAmount" />
                                    <px:PXGridColumn DataField="DeductionRate" />
                                    <px:PXGridColumn DataField="PRDeductCode__CntCalcType" />
                                    <px:PXGridColumn DataField="BenefitAmount" />
                                    <px:PXGridColumn DataField="BenefitRate" />
                                    <px:PXGridColumn DataField="EffectiveDate" Width="150px" CommitChanges="true" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Container="Parent" Enabled="True" MinHeight="150" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
