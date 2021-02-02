<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="CT202000.aspx.cs" Inherits="Page_CT202000"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CT.TemplateMaint" PrimaryView="Templates">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Templates" Caption="Contract Template Info" NoteIndicator="True"
        FilesIndicator="True" ActivityIndicator="true" ActivityField="NoteActivity" DefaultControlID="edContractCD">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSegmentMask ID="edContractCD" runat="server" DataField="ContractCD" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" AllowNull="False" DataField="Description" Size="XXL" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXCheckBox runat="server" ID="chkStatus" DataField="Status" Text="Active" FalseValue="D" TrueValue="A" AlignLeft="true"/>
            <%--<px:PXDropDown ID="edStatus" runat="server" AllowNull="False" DataField="Status" />--%>
        </Template>
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="450px" DataSourceID="ds" DataMember="CurrentTemplate">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Summary">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" StartGroup="True" GroupCaption="Contract Settings" />
                    <px:PXDropDown CommitChanges="True" ID="edType" runat="server" DataField="Type" Size="XM" AllowNull="false" />
                    <px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXNumberEdit ID="edDuration"  AllowNull="False" runat="server" DataField="Duration" Size="XXS" />
                    <px:PXDropDown ID="edDurationType" runat="server" DataField="DurationType" SuppressLabel="true" Size="M"  Width="194" AllowNull="false"  />
                    <px:PXLayoutRule runat="server" />
                    <px:PXCheckBox runat="server" ID="chkRefundable" DataField="Refundable" CommitChanges="true" />
                    <px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXNumberEdit ID="edRefundPeriod" runat="server" DataField="RefundPeriod" Size="XXS" />
                    <px:PXTextEdit ID="edDays1" DataField="Days" runat="server" SkinID="Label" SuppressLabel="true" Enabled="False"/>
                    <px:PXLayoutRule runat="server" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkAutoRenew" runat="server" DataField="AutoRenew" />
                    <px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXNumberEdit ID="edAutoRenewDays" runat="server" AllowNull="False" DataField="AutoRenewDays" Size="XXS" />
                    <px:PXTextEdit ID="edDaysBeforeExpiration" DataField="DaysBeforeExpiration" runat="server" SkinID="Label" SuppressLabel="true" Enabled="False"/>
                    <px:PXLayoutRule runat="server" />
                    <px:PXLayoutRule runat="server" Merge="true" />
                    <px:PXNumberEdit ID="edGracePeriod" runat="server" AllowNull="False" DataField="GracePeriod" Size="XXS" />
                    <px:PXTextEdit ID="edDays" DataField="Days" runat="server" SkinID="Label" SuppressLabel="true" Enabled="False"/>
                    <px:PXLayoutRule runat="server" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Size="M" CommitChanges="true" />
					<px:PXCheckBox SuppressLabel="True" ID="chkAllowOverride" runat="server"  DataField="AllowOverride" />
                    <px:PXCheckBox SuppressLabel="True" ID="chkAutomaticReleaseAR" runat="server" Checked="True" DataField="AutomaticReleaseAR" />
                    <px:PXDateTimeEdit runat="server" ID="edEffectiveFrom" DataField="EffectiveFrom" />
                    <px:PXDateTimeEdit runat="server" ID="edDiscontinueAfter" DataField="DiscontinueAfter" />

                    <px:PXLayoutRule ID="ColumnRHS" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XS"  />
                    <px:PXLayoutRule runat="server" StartGroup="true" GroupCaption="Billing Settings"   />
                    <px:PXFormView ID="billingForm" runat="server" DataMember="Billing" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                            <px:PXDropDown ID="edType" runat="server" DataField="Type" AllowNull="false"  />
                            <px:PXDropDown runat="server" ID="edBillTo" DataField="BillTo" AllowNull="false"  />
                        </Template>
                    </px:PXFormView>
					<px:PXDropDown ID="edScheduleStartsOn" runat="server" AllowNull="False" DataField="ScheduleStartsOn" Size="M"/>
					<px:PXDropDown ID="edDetailedBilling" runat="server" AllowNull="False" DataField="DetailedBilling" Size="M" />
					<px:PXFormView ID="descriptionFormulaView" runat="server" DataMember="Billing" DataSourceID="ds" RenderStyle="Simple" MarkRequired="Dynamic">
						<Template>
							<px:PXLayoutRule runat="server" ControlSize="L" LabelsWidth="SM" StartColumn="True"/>
							<pxa:CTFormulaInvoiceEditor ID="edDescriptionFormulaInv" runat="server" DataSourceID="ds" DataField="InvoiceFormula" Parameters="@ActionInvoice" />
							<pxa:CTFormulaTransactionEditor ID="edDescriptionFormulaTran" runat="server" DataSourceID="ds" DataField="TranFormula" Parameters="@Prefix,@ActionItem" />
						</Template>
					</px:PXFormView>
					<px:PXCheckBox SuppressLabel="True" ID="chkAllowOverrideFormulaDescription" runat="server"  DataField="AllowOverrideFormulaDescription"/>
                    <px:PXLayoutRule runat="server" GroupCaption="Case Billing Settings" ControlSize="M"  />
                    <px:PXSegmentMask ID="edCaseItemID" runat="server" DataField="CaseItemID" AllowEdit="true" />
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Details">
                <Template>
                    <px:PXGrid runat="server" ID="detGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab" BorderWidth="0px">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="ContractDetails">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <%--<px:PXDateTimeEdit ID="edEffectiveDate" runat="server" DataField="ContractItem__EffectiveDate" />--%>
                                    <%--<px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID" AllowEdit="true" />--%>
                                    <px:PXSegmentMask ID="edContractItemID" runat="server" DataField="ContractItemID" AllowEdit="true" AutoRefresh="true" CommitChanges="true" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                                    <%--<px:PXSelector ID="edUOM" runat="server" DataField="UOM" />--%>
                                    <%--<px:PXNumberEdit runat="server" ID="edDefaultQty" DataField="ContractItem__DefaultQty" Enabled="false"/>--%>
                                    <px:PXNumberEdit runat="server" ID="edPendingQty" DataField="Qty" CommitChanges="true"/>
                                    <px:PXNumberEdit runat="server" ID="edBasePriceVal" DataField="ContractItem__BasePriceVal" />
                                    <px:PXNumberEdit runat="server" ID="edFixedRecurringPriceVal" DataField="ContractItem__FixedRecurringPriceVal" />
                                    <px:PXNumberEdit runat="server" ID="edUsagePriceVal" DataField="ContractItem__UsagePriceVal" />
                                    <px:PXNumberEdit runat="server" ID="edRenewalPriceVal" DataField="ContractItem__RenewalPriceVal" />
                                </RowTemplate>
                                <Columns>
                                    <%--<px:PXGridColumn DataField="InventoryID" DisplayFormat="&gt;CCCCCCCCCCCCCCCCCCCC" Width="135px" AutoCallBack="True" />--%>
                                    <%--<px:PXGridColumn DataField="ContractItem__EffectiveDate" Width="100px" />--%>
                                    <px:PXGridColumn DataField="ContractItemID" Width="135px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Description" Width="350px" />
                                    <%--<px:PXGridColumn DataField="UOM" DisplayFormat="&gt;aaaaaa" Width="100px" />--%>
                                    <%--<px:PXGridColumn DataField="ContractItem__DefaultQty" Width="100px" />--%>
                                    <px:PXGridColumn DataField="Qty" Width="100px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ContractItem__BasePriceVal" Width="100px" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="ContractItem__FixedRecurringPriceVal" Width="100px" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="ContractItem__UsagePriceVal" Width="100px" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="ContractItem__RenewalPriceVal" Width="100px" TextAlign="Right"/>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Contracts">
                <Template>
                    <px:PXGrid ID="ContractsGrid" runat="server" SkinID="Inquire" DataSourceID="ds" Width="100%">
                        <AutoSize Enabled="true" />
                        <Levels>
                            <px:PXGridLevel DataMember="Contracts">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXSegmentMask runat="server" ID="edContractCD" DataField="ContractCD" AllowEdit="true" />
                                    <px:PXSelector runat="server" ID="edCustomerID" DataField="CustomerID" />
                                    <px:PXDropDown runat="server" ID="edStatus" DataField="Status" />
                                    <px:PXDateTimeEdit runat="server" ID="edStartDate" DataField="StartDate" />
                                    <px:PXDateTimeEdit runat="server" ID="edExpireDate" DataField="ExpireDate" />
                                    <px:PXTextEdit runat="server" ID="edContractDescription" DataField="Description"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ContractCD" Width="135px" AutoCallBack="true" />
                                    <px:PXGridColumn DataField="CustomerID" Width="100px" />
                                    <px:PXGridColumn DataField="Status"  Width="100px"/>
                                    <px:PXGridColumn DataField="StartDate"  Width="100px"/>
                                    <px:PXGridColumn DataField="ExpireDate"  Width="100px"/>
                                    <px:PXGridColumn DataField="Description" Width="100px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="SLA Terms">
                <Template>
                    <px:PXGrid ID="SLAMappingGrid" runat="server" SkinID="DetailsInTab" DataSourceID="ds" Width="100%">
                        <Mode AllowAddNew="False" />
                        <Mode AllowDelete="False" />
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="SLAMapping">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXMaskEdit SuppressLabel="True" ID="edPeriod" runat="server" DataField="Period" InputMask="### d\ays ## hrs ## mins" EmptyChar="0" /></RowTemplate>
                                <Columns>
                                    <px:PXGridColumn AllowNull="False" Type="DropDownList" DataField="Severity" Width="108px" />
                                    <px:PXGridColumn DataField="Period" TextAlign="Right" Width="108px" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar></ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid runat="server" ID="AttributeGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab" BorderWidth="0px" MatrixMode="True">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="AttributeGroup">
                                <RowTemplate>
                                    <px:PXSelector ID="edCRAttributeID" runat="server" DataField="AttributeID" AutoRefresh="true" AllowEdit="true" FilterByAllFields="True" />
                                </RowTemplate>
                                <Columns>
									<px:PXGridColumn DataField="IsActive" AllowNull="False" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn DataField="AttributeID" Width="108px" AutoCallBack="True" LinkCommand="CRAttribute_ViewDetails" />
                                    <px:PXGridColumn  DataField="Description" Width="351px" />
                                    <px:PXGridColumn DataField="SortOrder" TextAlign="Right" Width="108px" />
                                    <px:PXGridColumn  DataField="Required" TextAlign="Center" Type="CheckBox" />
                                    <px:PXGridColumn AllowNull="True" DataField="CSAttribute__IsInternal" TextAlign="Center" Type="CheckBox" />
					                <px:PXGridColumn AllowNull="False" DataField="ControlType" Type="DropDownList" Width="81px" />
                                    <px:PXGridColumn AllowNull="True" DataField="DefaultValue" Width="100px" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <ActionBar></ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
