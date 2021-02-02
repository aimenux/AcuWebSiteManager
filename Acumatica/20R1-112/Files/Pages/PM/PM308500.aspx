<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="PM308500.aspx.cs" Inherits="Page_PM308500"
    Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.PM.ChangeRequest.ChangeRequestEntry" PrimaryView="Document" BorderStyle="NotSet" EnableAttributes = True>
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand Name="CreateChangeOrder" />
            <px:PXDSCallbackCommand StartNewGroup="True" Name="Action" />
            <px:PXDSCallbackCommand Name="Report" CommitChanges="true" />
            <px:PXDSCallbackCommand Name="NewTask" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewEvent" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewActivity" Visible="False" CommitChanges="True" />
            <px:PXDSCallbackCommand Name="NewMailActivity" Visible="False" CommitChanges="True" PopupCommand="Cancel" PopupCommandTarget="ds" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Document" Caption="Change Request Summary" FilesIndicator="True"
        NoteIndicator="True" NotifyIndicator="true" ActivityIndicator="True" ActivityField="NoteActivity" LinkPage="">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S"/>
            <px:PXSelector ID="edRefNbr" runat="server" DataField="RefNbr" AutoRefresh="true" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
            <px:PXCheckBox ID="chkHold" runat="server" DataField="Hold" CommitChanges="true" />
            
            <px:PXDateTimeEdit ID="edDate" runat="server" DataField="Date" CommitChanges="true" />
            <px:PXNumberEdit ID="edDelayDays" runat="server" DataField="DelayDays" />
            <px:PXTextEdit ID="edExtRefNbr" runat="server" DataField="ExtRefNbr" />
            
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />

            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM"/>            
            <px:PXSegmentMask CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds" AllowEdit="True"/>
            <px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID" AllowEdit="true"/>
                          
             <px:PXSelector ID="edChangeOrderNbr" runat="server" DataField="ChangeOrderNbr" AllowEdit="true"/>
             <px:PXSelector ID="edCostChangeOrderNbr" runat="server" DataField="CostChangeOrderNbr" AllowEdit="true"/>
                                   
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="S"/>           
            <px:PXNumberEdit ID="edCostTotal" runat="server" DataField="CostTotal" Enabled="False" />
            <px:PXNumberEdit ID="edLineTotal" runat="server" DataField="LineTotal" Enabled="False" />
            <px:PXNumberEdit ID="edMarkupTotal" runat="server" DataField="MarkupTotal" Enabled="False" />            
            <px:PXNumberEdit ID="edGrosMarginPct" runat="server" DataField="GrossMarginPct" Enabled="False" />
            <px:PXNumberEdit ID="edPriceTotal" runat="server" DataField="PriceTotal" Enabled="False" />
        </Template>
    </px:PXFormView>    
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="511px" DataSourceID="ds" DataMember="DocumentSettings" >
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items >
            <px:PXTabItem Text="Estimation">
                <Template>
                     <px:PXGrid ID="DetailsGrid" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" Height="150px" SkinID="DetailsInTab" SyncPosition="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Details">
                                <RowTemplate>
                                    <px:PXSegmentMask ID="edCostTaskID" runat="server" DataField="CostTaskID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edRevenueTaskID" runat="server" DataField="RevenueTaskID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edCostAccountGroupID" runat="server" DataField="CostAccountGroupID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edRevenueAccountGroupID" runat="server" DataField="RevenueAccountGroupID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edRevenueCodeID" runat="server" DataField="RevenueCodeID" AutoRefresh="true"/>
                                    <px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" AutoRefresh="true" AllowEdit="true"/>
                                    <px:PXSelector ID="edRevenueInventoryID" runat="server" DataField="RevenueInventoryID" AutoRefresh="true" AllowEdit="true"/>
                                    <px:PXSegmentMask ID="edSubItemID" runat="server" DataField="SubItemID"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn CommitChanges="True" DataField="CostTaskID" />
                                    <px:PXGridColumn CommitChanges="True" DataField="CostAccountGroupID"  />
                                    <px:PXGridColumn CommitChanges="True" DataField="CostCodeID"  />
                                    <px:PXGridColumn CommitChanges="True" DataField="InventoryID"   />
                                    <px:PXGridColumn DataField="SubItemID" DisplayFormat="&gt;AA-A-A" CommitChanges="True" />
                                    <px:PXGridColumn DataField="Description"  />
                                    <px:PXGridColumn CommitChanges="true" DataField="Qty" TextAlign="Right"  />                                    
                                    <px:PXGridColumn CommitChanges="True" DataField="UOM"  />
                                    <px:PXGridColumn CommitChanges="true" DataField="UnitCost" TextAlign="Right"  />
                                    <px:PXGridColumn CommitChanges="true" DataField="ExtCost" TextAlign="Right"  />
                                    <px:PXGridColumn CommitChanges="true" DataField="PriceMarkupPct" TextAlign="Right"  />                                    
                                    <px:PXGridColumn CommitChanges="True" DataField="RevenueTaskID"  />
                                    <px:PXGridColumn CommitChanges="True" DataField="RevenueAccountGroupID"  />
                                    <px:PXGridColumn CommitChanges="True" DataField="RevenueCodeID"  />
                                    <px:PXGridColumn CommitChanges="True" DataField="RevenueInventoryID"  />
                                    <px:PXGridColumn CommitChanges="true" DataField="UnitPrice" TextAlign="Right"  />
                                    <px:PXGridColumn CommitChanges="true" DataField="ExtPrice" TextAlign="Right"  />
                                    <px:PXGridColumn CommitChanges="true" DataField="LineMarkupPct" TextAlign="Right"  />                                    
                                    <px:PXGridColumn CommitChanges="true" DataField="LineAmount" TextAlign="Right"  />
                                    <px:PXGridColumn CommitChanges="true" DataField="VendorID"  />
                                    <px:PXGridColumn CommitChanges="true" DataField="IsCommitment"  TextAlign="Center" Type="CheckBox" />                                    
                                   
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" MinHeight="150" />
                        <Mode InitNewRow="True" AllowUpload="True"/>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Detailed Description" LoadOnDemand="True">
                <Template>
                    <px:PXRichTextEdit ID="edText" runat="server" Style="border-width: 0px; width: 100%;" DataField="Text" 
						AllowLoadTemplate="false"  AllowDatafields="false"  AllowMacros="true" AllowSourceMode="true" AllowAttached="true" AllowSearch="true">
						<AutoSize Enabled="True" />
						<LoadTemplate TypeName="PX.SM.SMNotificationMaint" DataMember="Notifications" ViewName="NotificationTemplate" ValueField="notificationID" TextField="Name" DataSourceID="ds" Size="M"/>
					</px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Markups" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="gridMarkups" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="true" />
                        <Levels>
                            <px:PXGridLevel DataMember="Markups">
                            <RowTemplate>
                                    <px:PXSegmentMask ID="edMarkupTaskID" runat="server" DataField="TaskID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edMarkupAccountGroupID" runat="server" DataField="AccountGroupID" AutoRefresh="true"/>
                                    <px:PXSegmentMask ID="edMarkupCostCodeID" runat="server" DataField="CostCodeID" AutoRefresh="true"/>
                                    <px:PXSelector ID="edMarkupInventoryID" runat="server" DataField="InventoryID" AutoRefresh="true"/>
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="Type" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="Description" />
                                    <px:PXGridColumn DataField="Value" TextAlign="Right" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="Amount" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="MarkupAmount" TextAlign="Right"/>
                                    <px:PXGridColumn DataField="TaskID" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="AccountGroupID" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="CostCodeID" CommitChanges="True"/>
                                    <px:PXGridColumn DataField="InventoryID" CommitChanges="True"/>                                    
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Approval Details" LoadOnDemand="True">
                <Template>
                    <px:PXGrid ID="gridApproval" runat="server" DataSourceID="ds" Width="100%" SkinID="DetailsInTab" NoteIndicator="True">
                        <AutoSize Enabled="true" />
                        <Mode AllowAddNew="false" AllowDelete="false" AllowUpdate="false" />

                        <Levels>
                            <px:PXGridLevel DataMember="Approval">
                                <Columns>
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApproverEmployee__AcctName" />
                                    <px:PXGridColumn DataField="WorkgroupID" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctCD" />
                                    <px:PXGridColumn DataField="ApprovedByEmployee__AcctName" />
                                    <px:PXGridColumn DataField="ApproveDate" />
                                    <px:PXGridColumn DataField="Status" AllowNull="False" AllowUpdate="False" RenderEditorText="True" />
                                    <px:PXGridColumn DataField="Reason" AllowUpdate="False" />
                                    <px:PXGridColumn DataField="AssignmentMapID"  Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="RuleID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="StepID" Visible="false" SyncVisible="false" />
                                    <px:PXGridColumn DataField="CreatedDateTime" Visible="false" SyncVisible="false" />
                                </Columns>
                                <Layout FormViewHeight="" />
                            </px:PXGridLevel>
                        </Levels>
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
   <px:PXSmartPanel ID="panelReason" runat="server" Caption="Enter Reason" CaptionVisible="true" LoadOnDemand="true" Key="ReasonApproveRejectParams"
	AutoCallBack-Enabled="true" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" Width="600px"
	CallBackMode-PostData="Page" AcceptButtonID="btnReasonOk" CancelButtonID="btnReasonCancel" AllowResize="False">
	<px:PXFormView ID="PXFormViewPanelReason" runat="server" DataSourceID="ds" CaptionVisible="False" DataMember="ReasonApproveRejectParams">
		<ContentStyle BackColor="Transparent" BorderStyle="None" Width="100%" Height="100%"  CssClass="" /> 
		<Template>
			<px:PXLayoutRule ID="PXLayoutRulePanelReason" runat="server" StartColumn="True" />
			<px:PXPanel ID="PXPanelReason" runat="server" RenderStyle="Simple" >
				<px:PXLayoutRule ID="PXLayoutRuleReason" runat="server" StartColumn="True" SuppressLabel="True" />
				<px:PXTextEdit ID="edReason" runat="server" DataField="Reason" TextMode="MultiLine" LabelWidth="0" Height="200px" Width="600px" CommitChanges="True" />
			</px:PXPanel>
			<px:PXPanel ID="PXPanelReasonButtons" runat="server" SkinID="Buttons">
				<px:PXButton ID="btnReasonOk" runat="server" Text="OK" DialogResult="Yes" CommandSourceID="ds" />
				<px:PXButton ID="btnReasonCancel" runat="server" Text="Cancel" DialogResult="No" CommandSourceID="ds" />
			</px:PXPanel>
		</Template>
	</px:PXFormView>
</px:PXSmartPanel>
</asp:Content>
