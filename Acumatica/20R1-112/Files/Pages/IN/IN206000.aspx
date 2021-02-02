<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="IN206000.aspx.cs"
    Inherits="Page_IN206000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.IN.INPostClassMaint" PrimaryView="postclass">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="postclass"
        Caption="Posting Class Summary" NoteIndicator="True" FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity"
        DefaultControlID="edPostClassID">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
            <px:PXSelector ID="edPostClassID" runat="server" DataField="PostClassID" />
            <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="513px" DataSourceID="ds" DataMember="postclassaccounts" MarkRequired="Dynamic" >
        <Items>
            <px:PXTabItem Text="Posting Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                    <px:PXDropDown ID="edInvtAcctDefault" runat="server" AllowNull="False" DataField="InvtAcctDefault" CommitChanges="true" />
                    <px:PXSegmentMask ID="edInvtSubMask" runat="server" DataField="InvtSubMask" />
                    <px:PXDropDown ID="edSalesAcctDefault" runat="server" AllowNull="False" DataField="SalesAcctDefault" CommitChanges="true" />
                    <px:PXSegmentMask ID="edSalesSubMask" runat="server" DataField="SalesSubMask" CommitChanges="true" />
                    <px:PXDropDown ID="edCOGSAcctDefault" runat="server" AllowNull="False" DataField="COGSAcctDefault" CommitChanges="true" />
                    <px:PXCheckBox CommitChanges="True" SuppressLabel="True" ID="chkCOGSSubFromSales" runat="server" DataField="COGSSubFromSales" />
                    <px:PXSegmentMask ID="edCOGSSubMask" runat="server" DataField="COGSSubMask" />
                    <px:PXDropDown ID="edStdCstVarAcctDefault" runat="server" AllowNull="False" DataField="StdCstVarAcctDefault" CommitChanges="true" />
                    <px:PXSegmentMask ID="edStdCstVarSubMask" runat="server" DataField="StdCstVarSubMask" />
                    <px:PXDropDown ID="edStdCstRevAcctDefault" runat="server" AllowNull="False" DataField="StdCstRevAcctDefault" CommitChanges="true" />
                    <px:PXSegmentMask ID="edStdCstRevSubMask" runat="server" DataField="StdCstRevSubMask" />
                    <px:PXDropDown ID="edPOAccrualAcctDefault" runat="server" AllowNull="False" DataField="POAccrualAcctDefault" CommitChanges="true" />
                    <px:PXSegmentMask ID="edPOAccrualSubMask" runat="server" DataField="POAccrualSubMask" />
                    <px:PXDropDown ID="edPPVAcctDefault" runat="server" AllowNull="False" DataField="PPVAcctDefault" CommitChanges="true" />
                    <px:PXSegmentMask ID="edPPVSubMask" runat="server" DataField="PPVSubMask" />
                    <px:PXDropDown ID="edLCVarianceAcctDefault" runat="server" AllowNull="False" DataField="LCVarianceAcctDefault" />
                    <px:PXSegmentMask ID="edLCVarianceSubMask" runat="server" DataField="LCVarianceSubMask" />
                    <px:PXSelector ID="edPIReasonCode" runat="server" DataField="PIReasonCode" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="GL Accounts">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                    <px:PXSegmentMask ID="edInvtAcctID" runat="server" DataField="InvtAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edInvtSubID" runat="server" DataField="InvtSubID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INPostClass.invtAcctID" PropertyName="DataControls[&quot;edInvtAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edReasonCodeSubID" runat="server" DataField="ReasonCodeSubID" />
                    <px:PXSegmentMask ID="edSalesAcctID" runat="server" DataField="SalesAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edSalesSubID" runat="server" DataField="SalesSubID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INPostClass.salesAcctID" PropertyName="DataControls[&quot;edSalesAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edCOGSAcctID" runat="server" DataField="COGSAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edCOGSSubID" runat="server" DataField="COGSSubID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INPostClass.cOGSAcctID" PropertyName="DataControls[&quot;edCOGSAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edStdCstVarAcctID" runat="server" DataField="StdCstVarAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edStdCstVarSubID" runat="server" DataField="StdCstVarSubID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INPostClass.stdCstVarAcctID" PropertyName="DataControls[&quot;edStdCstVarAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edStdCstRevAcctID" runat="server" DataField="StdCstRevAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edStdCstRevSubID" runat="server" DataField="StdCstRevSubID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INPostClass.stdCstRevAcctID" PropertyName="DataControls[&quot;edStdCstRevAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edPOAccrualAcctID" runat="server" DataField="POAccrualAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edPOAccrualSubID" runat="server" DataField="POAccrualSubID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INPostClass.pOAccrualAcctID" PropertyName="DataControls[&quot;edPOAccrualAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edPPVAcctID" runat="server" DataField="PPVAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edPPVSubID" runat="server" DataField="PPVSubID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INPostClass.pPVAcctID" PropertyName="DataControls[&quot;edPPVAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edLCVarianceAcctID" runat="server" DataField="LCVarianceAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edLCVarianceSubID" runat="server" DataField="LCVarianceSubID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INPostClass.LCVarianceAcctID" PropertyName="DataControls[&quot;edLCVarianceAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
					<px:PXSegmentMask ID="edDeferralAcctID" runat="server" DataField="DeferralAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edDeferralSubID" runat="server" DataField="DeferralSubID" AutoRefresh="true">
                        <Parameters>
                            <px:PXControlParam ControlID="tab" Name="INPostClass.deferralAcctID" PropertyName="DataControls[&quot;edDeferralAcctID&quot;].Value"
                                Type="String" />
                        </Parameters>
                    </px:PXSegmentMask>
					<px:PXLayoutRule runat="server" GroupCaption="Payroll Settings" />
					<px:PXSegmentMask ID="edEarningsAcctID" runat="server" DataField="EarningsAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edEarningsSubID" runat="server" DataField="EarningsSubID" AutoRefresh="true" />
					<px:PXSegmentMask ID="edBenefitExpenseAcctID" runat="server" DataField="BenefitExpenseAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edBenefitExpenseSubID" runat="server" DataField="BenefitExpenseSubID" AutoRefresh="true" />
					<px:PXSegmentMask ID="edTaxExpenseAcctID" runat="server" DataField="TaxExpenseAcctID" CommitChanges="true" />
                    <px:PXSegmentMask ID="edTaxExpenseSubID" runat="server" DataField="TaxExpenseSubID" AutoRefresh="true" />
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
