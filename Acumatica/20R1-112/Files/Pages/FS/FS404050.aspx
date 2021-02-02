<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="FS404050.aspx.cs" Inherits="Page_FS404050" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" 
        PrimaryView="LocationRecords" TypeName="PX.Objects.FS.CustomerLocationInq">
		<CallbackCommands>
            <px:PXDSCallbackCommand Name="OpenRouteServiceContract" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenRouteSchedule" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenAppointmentByPickUpDeliveryItem" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenAppointmentByService" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenDocumentByPickUpDeliveryItem" Visible="False"></px:PXDSCallbackCommand>
            <px:PXDSCallbackCommand Name="OpenDocumentByService" Visible="False"></px:PXDSCallbackCommand>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" 
		Width="100%" DataMember="LocationRecords" TabIndex="1700">        
        <Template>
			<px:PXLayoutRule runat="server" ColumnWidth="S" StartColumn="True" LabelsWidth="S"/>
            <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" Width="150px" AutoRefresh="True" AllowEdit="True"/>
            <px:PXSelector ID="edLocationID" runat="server" DataField="LocationID" Width="150px" AutoRefresh="True" AllowEdit="True"/>
        </Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="391px" Style="z-index: 100" Width="100%" DataMember="LocationSelected" DataSourceID="ds">
        <AutoSize Enabled="True" Container="Window" MinWidth="300" MinHeight="250"></AutoSize>
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="General Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"></px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="Location Info" ></px:PXLayoutRule>
                    <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" ></px:PXTextEdit>
                    <px:PXCheckBox SuppressLabel="True" ID="edIsActive" runat="server" DataField="IsActive" ></px:PXCheckBox>
                    <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" StartGroup="True" GroupCaption="Location Contact" ></px:PXLayoutRule>
                    <px:PXTextEdit ID="edFullName" runat="server" DataField="Contact__FullName" ></px:PXTextEdit>
                    <px:PXTextEdit ID="edAttention" runat="server" DataField="Contact__Attention" ></px:PXTextEdit>
                    <px:PXTextEdit ID="edEMail" runat="server" DataField="Contact__EMail" ></px:PXTextEdit>
                    <px:PXTextEdit ID="edWebSite" runat="server" DataField="Contact__WebSite" ></px:PXTextEdit>
                    <px:PXTextEdit ID="edPhone1" runat="server" DataField="Contact__Phone1" ></px:PXTextEdit>
                    <px:PXTextEdit ID="edPhone2" runat="server" DataField="Contact__Phone2" ></px:PXTextEdit>
                    <px:PXTextEdit ID="edFax" runat="server" DataField="Contact__Fax" ></px:PXTextEdit>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Location Address" ></px:PXLayoutRule>
                    <px:PXCheckBox ID="edIsValidated" runat="server" DataField="Address__IsValidated"></px:PXCheckBox>
                    <px:PXTextEdit ID="edAddressLine1" runat="server" DataField="Address__AddressLine1" ></px:PXTextEdit>
                    <px:PXTextEdit ID="edAddressLine2" runat="server" DataField="Address__AddressLine2" ></px:PXTextEdit>
                    <px:PXTextEdit ID="edCity" runat="server" DataField="Address__City" ></px:PXTextEdit>
                    <px:PXSelector ID="edCountryID" runat="server" DataField="Address__CountryID" DisplayMode="Hint" Enable="True"></px:PXSelector>
                    <px:PXSelector ID="edState" runat="server" AutoRefresh="True" DataField="Address__State" LabelsWidth="SM" DisplayMode="Hint"></px:PXSelector>
                    <px:PXTextEdit Size="s" ID="edPostalCode" runat="server" DataField="Address__PostalCode" ></px:PXTextEdit>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" ></px:PXLayoutRule>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Customer"></px:PXLayoutRule>
                    <px:PXFormView ID="edCustomerRecords" runat="server" 
                    Caption="Customer Records" DataMember="CustomerRecords" 
                    DataSourceID="ds" RenderStyle="Simple" TabIndex="1500" >
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" ></px:PXLayoutRule>
                            <px:PXSegmentMask ID="edCustomerID" runat="server" DataField="AcctCD" AllowEdit="True"></px:PXSegmentMask>
                            <px:PXTextEdit ID="edCustomerName" runat="server" DataField="AcctName"></px:PXTextEdit>
                            <px:PXDropDown ID="edCustomerStatus" runat="server" DataField="Status" ></px:PXDropDown>
                        </Template>
                    </px:PXFormView>
                    <px:PXTextEdit ID="edBillingCycleCD" runat="server" DataField="FSBillingCycle__BillingCycleCD"></px:PXTextEdit>
                    <px:PXTextEdit ID="edBillingDescr" runat="server" DataField="FSBillingCycle__Descr" ></px:PXTextEdit>
                    <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Location Settings" ></px:PXLayoutRule>
                    <px:PXTextEdit ID="edTaxRegistrationID" runat="server" DataField="TaxRegistrationID" ></px:PXTextEdit>
                    <px:PXSelector ID="edCTaxZoneID" runat="server" DataField="CTaxZoneID" ></px:PXSelector>
                    <px:PXTextEdit ID="edCAvalaraExemptionNumber" runat="server" DataField="CAvalaraExemptionNumber" ></px:PXTextEdit>
                    <px:PXDropDown ID="edCAvalaraCustomerUsageType" runat="server" DataField="CAvalaraCustomerUsageType" ></px:PXDropDown>
                    <px:PXSelector ID="edCBranchID" runat="server" DataField="CBranchID" ></px:PXSelector>
                    <px:PXSelector ID="edCPriceClassID" runat="server" DataField="CPriceClassID" DisplayMode="Hint"></px:PXSelector>
                    <px:PXFormView ID="PricesClass1" runat="server" 
                    Caption="Price Class" DataMember="PriceClass1" 
                    DataSourceID="ds" RenderStyle="Simple" TabIndex="1500" DefaultControlID="edlocationID">
                        <Template>
                            <px:PXLayoutRule runat="server" LabelsWidth="SM" ControlSize="XM" ></px:PXLayoutRule>
                            <px:PXTextEdit ID="edCPriceClassDesc1" runat="server" DataField="Description" ></px:PXTextEdit>
                        </Template>
                    </px:PXFormView>
                    <px:PXSegmentMask ID="edCDefProjectID" runat="server" DataField="CDefProjectID" ></px:PXSegmentMask>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Route Contracts">
                <Template>
                    <px:PXGrid ID="PXGridRouteContracts" runat="server" DataSourceID="ds" Height="100%" SkinID="Inquire" TabIndex="28872" AutoAdjustColumns="True"
                        Width="100%" NoteIndicator="False" FilesIndicator="False" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="RouteContractSchedules" DataKeyNames="CustomerID,ServiceContractRefNbr,RefNbr">
                                <RowTemplate>
                                    <px:PXSelector ID="edServiceContractRefNbr" runat="server" DataField="ServiceContractRefNbr"></px:PXSelector>
                                    <px:PXTextEdit ID="edFSServiceContract__CustomerContractNbr" runat="server" DataField="FSServiceContract__CustomerContractNbr"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edServiceContractStatus" runat="server" DataField="FSServiceContract__Status"></px:PXTextEdit>
                                    <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr"></px:PXSelector>
                                    <px:PXCheckBox ID="edActive" runat="server" DataField="Active"></px:PXCheckBox>
                                    <px:PXTextEdit ID="edGlobalSequence" runat="server" DataField="FSScheduleRoute__GlobalSequence"></px:PXTextEdit>
                                    <px:PXSelector ID="edRouteCD" runat="server" DataField="FSRoute__RouteCD" DisplayMode="Hint"></px:PXSelector>
                                    <px:PXTextEdit ID="edRouteShort" runat="server" DataField="FSRoute__RouteShort"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edWeekCode" runat="server" DataField="WeekCode"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edRecurrenceDescription" runat="server" DataField="RecurrenceDescription"></px:PXTextEdit>
                                    <px:PXTextEdit ID="edLastGeneratedElementDate" runat="server" DataField="LastGeneratedElementDate"></px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ServiceContractRefNbr" LinkCommand="OpenRouteServiceContract"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceContract__CustomerContractNbr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSServiceContract__Status"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="RefNbr" LinkCommand="OpenRouteSchedule"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="Active" Type="CheckBox"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSScheduleRoute__GlobalSequence"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSRoute__RouteCD"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSRoute__RouteShort"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="RecurrenceDescription"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="WeekCode"></px:PXGridColumn>
                                    <px:PXGridColumn DataField="LastGeneratedElementDate"></px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False" AllowUpdate="False"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Pickup/Delivery Items">
                <Template>
                    <px:PXGrid ID="PXGridPickupDelivery" runat="server" DataSourceID="ds" Height="100%" SkinID="Inquire" TabIndex="4900" AutoAdjustColumns="True"
                        Width="100%" NoteIndicator="False" FilesIndicator="False" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel 
                                DataMember="PickupDeliveryItems" DataKeyNames="AppointmentID, SODetID, InventoryID">
                                <RowTemplate>
                                    <px:PXSelector ID="edFSAppointment__RefNbr" runat="server" DataField="FSAppointment__RefNbr">
                                    </px:PXSelector>
                                    <px:PXDropDown ID="edFSAppointment__Status" runat="server" DataField="FSAppointment__Status">
                                    </px:PXDropDown>
                                    <px:PXSelector ID="edSODetID3" runat="server" DataField="SODetID">
                                    </px:PXSelector>
                                    <px:PXSegmentMask ID="edPickupDeliveryServiceID" runat="server" DataField="PickupDeliveryServiceID" AutoRefresh="True">
                                    </px:PXSegmentMask>
                                    <px:PXDropDown ID="edServiceType" runat="server" DataField="ServiceType" Size="SM">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edPickupDeliveryInventoryID" runat="server" DataField="InventoryID" Size="SM">
                                    </px:PXSegmentMask>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID" AutoRefresh="True" NullText="<SPLIT>">
                                            <Parameters>
                                                <px:PXControlParam ControlID="PXGridPickupDelivery" Name="FSAppointmentInventoryItem.InventoryID" PropertyName="DataValues[&quot;InventoryID&quot;]" Type="String" />
                                            </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXTextEdit ID="edTranDesc3" runat="server" DataField="TranDesc">
                                    </px:PXTextEdit>
                                    <px:PXSegmentMask ID="edSiteID3" runat="server" DataField="SiteID">
                                    </px:PXSegmentMask>
                                    <px:PXSelector ID="edUOM3" runat="server" AutoRefresh="True" DataField="UOM">
                                    </px:PXSelector>
                                    <px:PXNumberEdit ID="edQty2" runat="server" DataField="Qty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edUnitPrice3" runat="server" DataField="UnitPrice">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edTranAmt2" runat="server" DataField="TranAmt">
                                    </px:PXNumberEdit>
                                    <px:PXSelector ID="edProjectTaskID3" runat="server" DataField="ProjectTaskID">
                                    </px:PXSelector>
                                    <px:PXSelector ID="edFSPostBatch__BatchNbr" runat="server" DataField="FSPostBatch__BatchNbr" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edMem_PostedIn" runat="server" DataField="FSPostDet__Mem_PostedIn">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edMem_DocType" runat="server" DataField="FSPostDet__Mem_DocType">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edMem_DocNbr" runat="server" DataField="FSPostDet__Mem_DocNbr">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="FSAppointment__RefNbr"  LinkCommand="OpenAppointmentByPickUpDeliveryItem">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSAppointment__ExecutionDate">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSAppointment__Status">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SODetID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="PickupDeliveryServiceID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ServiceType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InventoryID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranDesc">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SiteID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UOM">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UnitPrice" TextAlign="Right">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranAmt" TextAlign="Right">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ProjectTaskID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSPostBatch__BatchNbr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSPostDet__Mem_PostedIn">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSPostDet__Mem_DocType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSPostDet__Mem_DocNbr" LinkCommand="OpenDocumentByPickUpDeliveryItem">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Services">
                <Template>
                    <px:PXGrid ID="PXGridServices" runat="server" DataSourceID="ds" 
                        Height="100%" SkinID="Inquire" TabIndex="4200" AutoAdjustColumns="True"
                        Width="100%" NoteIndicator="False" FilesIndicator="False" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Services" DataKeyNames="AppointmentID,AppDetID,SODetID">
                                <RowTemplate>
                                    <px:PXSelector ID="edFSAppointment__RefNbr2" runat="server" DataField="FSAppointment__RefNbr">
                                    </px:PXSelector>
                                    <px:PXDropDown ID="edFSAppointment__Status2" runat="server" DataField="FSAppointment__Status">
                                    </px:PXDropDown>
                                    <px:PXSelector ID="edSODetID" runat="server" DataField="SODetID"></px:PXSelector>
                                    <px:PXDropDown ID="Status" runat="server" DataField="Status">
                                    </px:PXDropDown>
                                    <px:PXDropDown ID="edLineType" runat="server" DataField="LineType">
                                    </px:PXDropDown>
                                    <px:PXSegmentMask ID="edInventoryID" runat="server" DataField="InventoryID">
                                        <Parameters>
                                            <px:PXControlParam ControlID="PXGridServices" Name="FSAppointmentDetService.lineType" PropertyName="DataValues[&quot;LineType&quot;]"/>
                                        </Parameters>
                                    </px:PXSegmentMask>
                                    <px:PXDropDown ID="edBillingRule" runat="server" DataField="FSSODet__BillingRule" Size="SM"></px:PXDropDown>
                                    <px:PXTextEdit ID="edTranDesc" runat="server" DataField="TranDesc"></px:PXTextEdit>
                                    <px:PXCheckBox ID="edIsBillable" runat="server" DataField="IsBillable" Text="Is billable"></px:PXCheckBox>                                    
                                    <px:PXMaskEdit ID="edEstimatedDuration" runat="server" DataField="EstimatedDuration"></px:PXMaskEdit>
                                    <px:PXMaskEdit ID="edActualDuration" runat="server" DataField="ActualDuration"></px:PXMaskEdit>
                                    <px:PXNumberEdit ID="Qty" runat="server" DataField="Qty">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="edUnitPrice" runat="server" DataField="UnitPrice">
                                    </px:PXNumberEdit>
                                    <px:PXNumberEdit ID="TranAmt" runat="server" DataField="TranAmt">
                                    </px:PXNumberEdit>
                                    <px:PXSelector ID="edProjectTaskID" runat="server" DataField="ProjectTaskID" AutoRefresh="True" DisplayMode="Value">
                                    </px:PXSelector>                                    
                                    <px:PXSelector ID="edFSPostBatch__BatchNbr2" runat="server" DataField="FSPostBatch__BatchNbr" AllowEdit="True">
                                    </px:PXSelector>
                                    <px:PXTextEdit ID="edMem_PostedIn2" runat="server" DataField="FSPostDet__Mem_PostedIn">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edMem_DocType2" runat="server" DataField="FSPostDet__Mem_DocType">
                                    </px:PXTextEdit>
                                    <px:PXTextEdit ID="edMem_DocNbr2" runat="server" DataField="FSPostDet__Mem_DocNbr">
                                    </px:PXTextEdit>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="FSAppointment__RefNbr" LinkCommand="OpenAppointmentByService">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSAppointment__ExecutionDate">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSAppointment__Status">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="SODetID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Status">
                                    </px:PXGridColumn>                                    
                                    <px:PXGridColumn DataField="LineType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="InventoryID">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSSODet__BillingRule">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranDesc">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="IsBillable" TextAlign="Center" Type="CheckBox">
                                    </px:PXGridColumn>                                    
                                    <px:PXGridColumn DataField="EstimatedDuration">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ActualDuration">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="Qty" TextAlign="Right">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="UnitPrice" TextAlign="Right">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="TranAmt" TextAlign="Right">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="ProjectTaskID">                                 
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSPostBatch__BatchNbr">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSPostDet__Mem_PostedIn">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSPostDet__Mem_DocType">
                                    </px:PXGridColumn>
                                    <px:PXGridColumn DataField="FSPostDet__Mem_DocNbr" LinkCommand="OpenDocumentByService">
                                    </px:PXGridColumn>
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode AllowAddNew="False" AllowDelete="False"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
