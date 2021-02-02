<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="AM101000.aspx.cs" Inherits="Page_AM101000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" TypeName="PX.Objects.AM.BOMSetup" BorderStyle="NotSet" PrimaryView="AMBSetupRecord">
        <CallbackCommands>
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="phF" runat="Server">
    <px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="500px" Style="z-index: 100" Width="100%" DataMember="AMBSetupRecord"
        DefaultControlID="edDefaultOrderType">
        <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
        <Items>
            <px:PXTabItem Text="General Settings">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="XM" ControlSize="XM" />
                    <px:PXPanel ID="pnlNumberingSettings" runat="server" Caption="Numbering Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXSelector ID="edBOMNumberingID" runat="server" DataField="BOMNumberingID" AllowEdit="True"/>
                        <px:PXSelector ID="edECRNumberingID" runat="server" DataField="ECRNumberingID" AllowEdit="True"/>
                        <px:PXSelector ID="edECONumberingID" runat="server" DataField="ECONumberingID" AllowEdit="True"/>
                    </px:PXPanel>
                    <px:PXPanel ID="pnlDataEntrySettings" runat="server" Caption="Data Entry Settings" RenderStyle="Fieldset">
                        <px:PXLayoutRule ID="PXLayoutRule4" runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXMaskEdit ID="edDefaultRevisionID" runat="server" DataField="DefaultRevisionID" />
                        <px:PXDropDown ID="edDupInvBOM" runat="server" DataField="DupInvBOM"/>
                        <px:PXDropDown ID="edDupInvOper" runat="server" DataField="DupInvOper"/>
                        <px:PXSelector ID="edWcID" runat="server" DataField="WcID" AllowEdit="True"/>
                        <px:PXDropDown ID="edOperationTimeFormat" runat="server" DataField="OperationTimeFormat"/>
                        <px:PXDropDown ID="edProductionTimeFormat" runat="server" DataField="ProductionTimeFormat"/>
                        <px:PXCheckBox ID="edAllowEmptyBOMSubItemID" runat="server" DataField="AllowEmptyBOMSubItemID"/>
                        <px:PXCheckBox ID="edForceECR" runat="server" DataField="ForceECR"/>
                    </px:PXPanel>
                    <px:PXPanel ID="PXPanel1" runat="server" Caption="Cost Roll" RenderStyle="Fieldset">
                        <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartColumn="True" LabelsWidth="XM" ControlSize="XM" />
                        <px:PXCheckBox ID="edAllowArchiveWithoutUpdatePending" runat="server" DataField="AllowArchiveWithoutUpdatePending"/>
                        <px:PXCheckBox ID="edAutoArchiveWhenUpdatePending" runat="server" DataField="AutoArchiveWhenUpdatePending"/>
                    </px:PXPanel>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="ECR Approval">
				<Template>
                    <px:PXPanel ID="pnlECRApproval" runat="server" DataMember="">
                        <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XM" />
                        <px:PXCheckBox ID="chkECRRequestApproval" runat="server" AlignLeft="True" Checked="True" DataField="ECRRequestApproval" CommitChanges="True" />				        
                    </px:PXPanel>
                    <px:PXGrid ID="gridECRApproval" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" >
                        <AutoSize Enabled="True"/>
					    <Levels>
						    <px:PXGridLevel DataMember="ECRSetupApproval">
							    <RowTemplate>
								    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
								    <px:PXSelector ID="edECRAssignmentMapID" runat="server" DataField="AssignmentMapID" TextField="Name" AllowEdit="True" edit="1" CommitChanges="True" />
                                    <px:PXSelector ID="edECRAssignmentNotificationID" runat="server" DataField="AssignmentNotificationID" AllowEdit="True" />
                                </RowTemplate>
							    <Columns>
								    <px:PXGridColumn DataField="AssignmentMapID" Width="250px" RenderEditorText="True" TextField="AssignmentMapID_EPAssignmentMap_Name" />
                                    <px:PXGridColumn DataField="AssignmentNotificationID" Width="250px" RenderEditorText="True" />
							    </Columns>
						    </px:PXGridLevel>
					    </Levels>                        
					</px:PXGrid>
                </Template>
			</px:PXTabItem>
            <px:PXTabItem Text="ECO Approval">
                <Template>
                    <px:PXPanel ID="pnlECOApproval" runat="server" DataMember="">
                        <px:PXLayoutRule runat="server" LabelsWidth="S" ControlSize="XM" />
                        <px:PXCheckBox ID="chkECORequestApproval" runat="server" AlignLeft="True" Checked="True" DataField="ECORequestApproval" CommitChanges="True" />				        
                    </px:PXPanel>
                    <px:PXGrid ID="gridECOApproval" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" >
                        <AutoSize Enabled="True"/>
                        <Levels>
                            <px:PXGridLevel DataMember="ECOSetupApproval">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True"  LabelsWidth="M" ControlSize="XM" />
                                    <px:PXSelector ID="edECOAssignmentMapID" runat="server" DataField="AssignmentMapID" TextField="Name" AllowEdit="True" edit="1" CommitChanges="True" />
                                    <px:PXSelector ID="edECOAssignmentNotificationID" runat="server" DataField="AssignmentNotificationID" AllowEdit="True" />
                                </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="AssignmentMapID" Width="250px" RenderEditorText="True" TextField="AssignmentMapID_EPAssignmentMap_Name" />
                                    <px:PXGridColumn DataField="AssignmentNotificationID" Width="250px" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>                        
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize MinHeight="480" Container="Window" Enabled="True" />
    </px:PXTab>
</asp:Content>