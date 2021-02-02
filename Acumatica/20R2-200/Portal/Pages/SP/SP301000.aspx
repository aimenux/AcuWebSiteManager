<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SP301000.aspx.cs"
    Inherits="Pages_SP301000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.CT.ContractMaint"
        PrimaryView="Contracts" BorderStyle="NotSet" PageLoadBehavior="GoFirstRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" PopupVisible="true" Visible="False" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Cancel" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Previous" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Next" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="Delete" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" Visible="False" />


            <px:PXDSCallbackCommand Name="Last" PostData="Self" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewUsage" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="WatchersGrid" Name="ShowContact" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewInvoice" Visible="False" DependOnGrid="InvoicesGrid" />
            <px:PXDSCallbackCommand Name="Renew" Visible="False" />
            <px:PXDSCallbackCommand Name="ViewContract" Visible="False" DependOnGrid="RenewalHistoryGrid" />
            <px:PXDSCallbackCommand Name="Bill" Visible="false" />
            <px:PXDSCallbackCommand Name="Activate" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="Terminate" Visible="false" />
            <px:PXDSCallbackCommand Name="Upgrade" Visible="false" />
            <px:PXDSCallbackCommand Name="UndoBilling" Visible="false" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="viewLicense" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="createNewLicense" Visible="False" />
            <px:PXDSCallbackCommand DependOnGrid="grid" Name="editLicense" Visible="False" />

            <px:PXDSCallbackCommand Name="Setup" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="SetupAndActivate" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="ActivateUpgrade" Visible="false" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="TogglePinActivity" Visible="False" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Contracts" Caption="Contract Info"
        NoteIndicator="False" FilesIndicator="False" ActivityIndicator="False" ActivityField="NoteActivity" LinkIndicator="False"
        NotifyIndicator="False" EmailingGraph="PX.Objects.CR.CREmailActivityMaint,PX.Objects" DefaultControlID="edContractCD">
        <Template>
             
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM"/>
            <px:PXSegmentMask ID="edContractCD" runat="server" DataField="ContractCD" CommitChanges="True"/>
            <px:PXSegmentMask CommitChanges="True" ID="edTemplateID" runat="server" DataField="TemplateID" Enabled="False" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" AllowNull="False" Enabled="False" />
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXSegmentMask CommitChanges="True" ID="edCustomerID" runat="server" DataField="CustomerID" Enabled="False" />
            <px:PXSegmentMask ID="edLocation" runat="server" DataField="LocationID" CommitChanges="true" Enabled="False" />
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" Enabled="False" />
            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXNumberEdit runat="server" ID="edBalance" DataField="Balance" Enabled="False" />
            <px:PXNumberEdit runat="server" ID="edFinanceVisible" DataField="FinanceVisible" />
        </Template>
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="511px" DataSourceID="ds" DataMember="CurrentContract">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="Summary" RepaintOnDemand="false">
                <Template>
                    
                    <px:PXLayoutRule ID="PXLayoutRule6" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                    <px:PXLayoutRule ID="PXLayoutRule10" runat="server" StartGroup="True" GroupCaption="Contract Settings" ControlSize="SM" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="edStartDate" runat="server" DataField="StartDate" Enabled="False" Size="SM" />
                    <px:PXDateTimeEdit CommitChanges="True" ID="edExpireDate" runat="server" DataField="ExpireDate" Enabled="False" Size="SM" />
                    <px:PXSelector ID="edCuryID" runat="server" DataField="CuryID" Enabled="False" />
                    <px:PXLayoutRule ID="PXLayoutRule7" runat="server" Merge="true" LabelsWidth="SM" />
                    <px:PXNumberEdit ID="edGracePeriod" runat="server" AllowNull="False" DataField="GracePeriod" LabelsWidth="SM" Size="XXS" Enabled="False" />
                    <px:PXTextEdit ID="edDays" DataField="Days" runat="server" SkinID="Label" SuppressLabel="true" Enabled="False" Size="XXS" />
                    
                    <px:PXLayoutRule ID="PXLayoutRule8" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="true" GroupCaption="Billing Information" />
                    <px:PXFormView ID="PXFormView1" runat="server" DataMember="Billing" DataSourceID="ds" RenderStyle="Simple">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                            <px:PXSelector ID="edAccountID" DataField="AccountID" runat="server" CommitChanges="true" Enabled="False" AllowEdit="False" />
                            <px:PXSelector ID="edLocationID" DataField="LocationID" runat="server" AutoRefresh="true" CommitChanges="true" Enabled="False" />
                        </Template>
                    </px:PXFormView>

                    <px:PXFormView ID="billingForm" runat="server" DataMember="Billing" DataSourceID="ds" RenderStyle="Fieldset" Caption="Billing Schedule">
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="SM" />
                            <px:PXDateTimeEdit runat="server" ID="edStartBilling" DataField="StartBilling" Enabled="False" Size="SM" />
                            <px:PXDropDown CommitChanges="True" ID="edType" runat="server" DataField="Type" Enabled="False" />
                            <px:PXDateTimeEdit ID="edLastDate" runat="server" DataField="LastDate" Enabled="False" Size="SM" />
                            <px:PXDateTimeEdit ID="edNextDate" runat="server" DataField="NextDate" Enabled="False" Size="SM" />
                        </Template>
                    </px:PXFormView>
                    <px:PXLayoutRule ID="PXLayoutRule12" runat="server" StartColumn="true" />
                 </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Details" BindingContext="form" VisibleExp="DataControls[&quot;edFinanceVisible&quot;].Value == 1" RepaintOnDemand="false">
                <Template>
                    <px:PXFormView ID="PXFormView4" runat="server" DataMember="CurrentContract" DataSourceID="ds" Style="z-index: 100" Width="100%">
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule13" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="SM" />
                            <px:PXDateTimeEdit runat="server" DataField="EffectiveFrom" ID="edEffectiveFrom" Enabled="False" Size="SM" />
                            <px:PXSelector runat="server" ID="edDiscountID" DataField="DiscountID" CommitChanges="true" Enabled="False" />
                            <px:PXLayoutRule ID="PXLayoutRule14" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXNumberEdit runat="server" ID="edPendingSetup" DataField="PendingSetup" Enabled="false" />
                            <px:PXNumberEdit runat="server" ID="edPendingRecurring" DataField="PendingRecurring" Enabled="false" />
                            <px:PXNumberEdit runat="server" ID="edPendingRenewal" DataField="PendingRenewal" Enabled="false" />
                            <px:PXNumberEdit runat="server" ID="TotalPending" DataField="TotalPending" Enabled="false" />
                            <px:PXLayoutRule ID="PXLayoutRule15" runat="server" StartColumn="true" />
                            <px:PXNumberEdit runat="server" ID="edCurrentSetup" DataField="CurrentSetup" Enabled="false" />
                            <px:PXNumberEdit runat="server" ID="edCurrentRecurring" DataField="CurrentRecurring" Enabled="false" />
                            <px:PXNumberEdit runat="server" ID="edCurrentRenewal" DataField="CurrentRenewal" Enabled="false" />
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid runat="server" ID="detGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="Inquire" RepaintColumns="True">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="ContractDetails">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule16" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXSelector ID="edContractItemID" runat="server" DataField="ContractItemID" AutoRefresh="true" />
                                    <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
                                    <px:PXNumberEdit ID="edQty" DataField="Qty" runat="server" CommitChanges="true" />
                                    <px:PXTextEdit runat="server" ID="edChange" DataField="Change" />
                                    <px:PXNumberEdit runat="server" ID="edBasePriceVal" DataField="ContractItem__BasePriceVal" />
                                    <px:PXNumberEdit runat="server" ID="edBaseDiscountPct" DataField="BaseDiscountPct" />
                                    <px:PXNumberEdit runat="server" ID="edRecurringDiscountPct" DataField="RecurringDiscountPct" />
                                    <px:PXNumberEdit runat="server" ID="edRenewalDiscountPct" DataField="RenewalDiscountPct" />
                                    <px:PXNumberEdit runat="server" ID="edFixedRecurringPriceVal" DataField="ContractItem__FixedRecurringPriceVal" />
                                    <px:PXNumberEdit runat="server" ID="edRenewalPriceVal" DataField="ContractItem__RenewalPriceVal" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ContractItemID" Width="135px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Description" Width="300px" />
                                    <px:PXGridColumn DataField="Qty" Width="90px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Change" Width="90px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ContractItem__BasePriceVal" Width="90px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="BaseDiscountPct" Width="80px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ContractItem__FixedRecurringPriceVal" Width="90px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RecurringDiscountPct" Width="80px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ContractItem__RenewalPriceVal" Width="90px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RenewalDiscountPct" Width="80px" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" AllowUpdate="False" AutoInsert="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Details" BindingContext="form" VisibleExp="DataControls[&quot;edFinanceVisible&quot;].Value == 2" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid runat="server" ID="detGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="Inquire" RepaintColumns="True">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="ContractDetails">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule117" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXSelector ID="edContractItemID1" runat="server" DataField="ContractItemID" AutoRefresh="true" />
                                    <px:PXTextEdit ID="edDescription1" runat="server" DataField="Description" />
                                    <px:PXNumberEdit ID="edQty1" DataField="Qty" runat="server" CommitChanges="true" />                                    
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ContractItemID" Width="135px" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="Description" Width="300px" />
                                    <px:PXGridColumn DataField="Qty" Width="90px" TextAlign="Right" />                                    
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" AllowUpdate="False" AutoInsert="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>

            <px:PXTabItem Text="Recurring Summary" BindingContext="form" VisibleExp="DataControls[&quot;edFinanceVisible&quot;].Value == 1" RepaintOnDemand="false">
                <Template>
                    <px:PXFormView ID="PXFormView3" runat="server" DataMember="CurrentContract" DataSourceID="ds" Style="z-index: 100" Width="100%">
                        <ContentStyle BackColor="Transparent" BorderStyle="None">
                        </ContentStyle>
                        <Template>
                            <px:PXLayoutRule ID="PXLayoutRule17" runat="server" StartColumn="true" LabelsWidth="SM" ControlSize="XM" />
                            <px:PXNumberEdit runat="server" ID="edTotalRecurring" DataField="TotalRecurring" Enabled="false" />
                            <px:PXNumberEdit runat="server" ID="edTotalUsage" DataField="TotalUsage" Enabled="false" />
                            <px:PXNumberEdit runat="server" ID="edTotalDue" DataField="TotalDue" Enabled="false" />
                        </Template>
                    </px:PXFormView>
                    <px:PXGrid runat="server" ID="detGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="Inquire">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="RecurringDetails">
                                <Columns>
                                    <px:PXGridColumn DataField="ContractItemID" Width="135px" />
                                    <px:PXGridColumn DataField="Description" Width="300px" />
                                    <px:PXGridColumn DataField="InventoryItem__InventoryCD" Width="120px" />
                                    <px:PXGridColumn DataField="InventoryItem__SalesUnit" Width="63px" />
                                    <px:PXGridColumn DataField="ContractItem__RecurringType" Width="63px" />
                                    <px:PXGridColumn DataField="Qty" Width="90px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ContractItem__FixedRecurringPriceVal" Width="90px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="RecurringDiscountPct" Width="80px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="ContractItem__UsagePriceVal" Width="90px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="Used" Width="80px" TextAlign="Right" />
                                    <px:PXGridColumn DataField="UsedTotal" Width="80px" TextAlign="Right" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" AllowUpdate="False" AutoInsert="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
			
			<px:PXTabItem Text="Contract History" BindingContext="form" VisibleExp="DataControls[&quot;edFinanceVisible&quot;].Value == 1" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="RenewalHistoryGrid" runat="server" Height="350px" Width="100%" Style="z-index: 100" AdjustPageSize="Auto"
                        AllowSearch="true" DataSourceID="ds" SkinID="Inquire">
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowSort="False" AllowUpdate="False" />
                        <Levels>
                            <px:PXGridLevel DataMember="RenewalHistory">
                                <Columns>
                                    <px:PXGridColumn DataField="Action" Width="115px" />
                                    <px:PXGridColumn DataField="Date" Width="130px" />
                                    <px:PXGridColumn DataField="CreatedByID"  Width="115px" />
                                    <px:PXGridColumn DataField="ChildContractID" Width="130px"/>
                                </Columns>
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule9" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXDropDown ID="edAction" runat="server" DataField="Action" />
                                    <px:PXTextEdit ID="edDate" runat="server" DataField="Date" />
                                    <px:PXSelector ID="edUser" runat="server" DataField="CreatedByID" />
                                    <px:PXSelector ID="edChildId" runat="server" DataField="ChildContractID" />
                                </RowTemplate>
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="AR History" BindingContext="form" VisibleExp="DataControls[&quot;edFinanceVisible&quot;].Value == 1" RepaintOnDemand="false">
                <Template>
                    <px:PXGrid ID="InvoicesGrid" runat="server" Height="350px" Width="100%" Style="z-index: 100" AllowPaging="True" AdjustPageSize="Auto"
                        AllowSearch="true" DataSourceID="ds" SkinID="Inquire">
                        <AutoSize Enabled="True" />
                        <Levels>
                            <px:PXGridLevel DataMember="Invoices">
                                <RowTemplate>
                                    <px:PXLayoutRule ID="PXLayoutRule5" runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
                                    <px:PXDropDown ID="edDocType" runat="server" DataField="DocType" />
                                    <px:PXSelector ID="edRefNbr" runat="server" AllowEdit="True" DataField="RefNbr"  LinkCommand="PrintSelectedDocument"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="DocType" />
                                    <px:PXGridColumn DataField="RefNbr" Width="90px" LinkCommand="PrintSelectedDocument"/>
                                    <px:PXGridColumn DataField="DocDate" Width="60px" />
                                    <px:PXGridColumn DataField="DueDate" Width="90px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="Status" />
                                    <px:PXGridColumn AllowNull="False" DataField="CuryOrigDocAmt" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn AllowNull="False" AllowUpdate="False" DataField="CuryDocBal" TextAlign="Right" Width="100px" />
                                    <px:PXGridColumn DataField="PaymentMethodID" Width="90px" />
                                </Columns>
                                <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" AllowUpdate="False" AutoInsert="False" />
                            </px:PXGridLevel>
                        </Levels>
						<ActionBar DefaultAction="cmdViewDoc">
						<CustomItems>
							<px:PXToolBarButton Text="Print Selected Document" Key="cmdViewDoc" Visible="False">
								<AutoCallBack Command="PrintSelectedDocument" Target="ds" />
							</px:PXToolBarButton>
                
						</CustomItems>
					</ActionBar>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>

