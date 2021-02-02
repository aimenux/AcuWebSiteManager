<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="EP208000.aspx.cs"
    Inherits="Page_EP208000" Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.Objects.EP.EquipmentMaint" PrimaryView="Equipment">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
            <px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="true" />
            <px:PXDSCallbackCommand Name="Last" PostData="Self" />
            <px:PXDSCallbackCommand CommitChanges="True" PopupCommand="Refresh" Name="ExtendToSMEquipment" PopupCommandTarget="form" RepaintControls="All" />
            <px:PXDSCallbackCommand Name="ViewInSMEquipment" CommitChanges="True" PopupCommand="Refresh" PopupCommandTarget="form" RepaintControls="All" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100" Width="100%" DataMember="Equipment" DefaultControlID="edEquipmentCD"
        Caption="Equipment Summary">
        <Template>
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
            <px:PXSelector ID="edEquipmentCD" runat="server" DataField="EquipmentCD" AutoGenerateColumns="True" DataSourceID="ds" />
            <px:PXLayoutRule runat="server" ColumnSpan="2" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" />
            <px:PXDropDown ID="edStatus" runat="server" DataField="Status" />
        </Template>
    </px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Width="100%" Height="511px" DataSourceID="ds" DataMember="EquipmentProperties">
        <Items>
            <px:PXTabItem Text="General Info">
                <Template>
                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                    <px:PXSelector ID="edFixedAssetID" runat="server" DataField="FixedAssetID" />
                    <px:PXSegmentMask ID="edRunRateItemID" runat="server" DataField="RunRateItemID" CommitChanges="True" />
                    <px:PXSegmentMask ID="edStandbyRateItemID" runat="server" DataField="SetupRateItemID" CommitChanges="True" />
                    <px:PXSegmentMask ID="edSuspendRateItemID" runat="server" DataField="SuspendRateItemID" CommitChanges="True" />
                    <px:PXNumberEdit ID="edRunRate" runat="server" DataField="RunRate" />
                    <px:PXNumberEdit ID="edStandbyRate" runat="server" DataField="SetupRate" />
                    <px:PXNumberEdit ID="edSuspendRate" runat="server" DataField="SuspendRate" />
                    <px:PXSegmentMask ID="edDefAccountID" runat="server" DataField="DefaultAccountID" CommitChanges="true">
                        
                    </px:PXSegmentMask>
                    <px:PXSegmentMask ID="edDefaultSubID" runat="server" DataField="DefaultSubID" /></Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Project Rates">
                <Template>
                    <px:PXGrid runat="server" ID="ProjectRatesGrid" Width="100%" DataSourceID="ds" Height="100%" SkinID="DetailsInTab" AdjustPageSize="Auto"
                        AllowPaging="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Rates" DataKeyNames="EquipmentID,ProjectID">
                                <RowTemplate>
                                    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                                    <px:PXNumberEdit ID="edEquipmentID" runat="server" DataField="EquipmentID" />
                                    <px:PXSegmentMask ID="edProjectID" runat="server" DataField="ProjectID" />
                                    </RowTemplate>
                                <Columns>
                                    <px:PXGridColumn DataField="ProjectID" Label="Project" AutoCallBack="True" />
                                    <px:PXGridColumn DataField="PMProject__Description" />
                                    <px:PXGridColumn DataField="RunRate" Label="Run Rate" />
                                    <px:PXGridColumn DataField="SetupRate" Label="Run Rate" />
                                    <px:PXGridColumn DataField="SuspendRate" Label="Run Rate" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Attributes">
                <Template>
                    <px:PXGrid ID="PXGridAnswers" runat="server" DataSourceID="ds" Width="100%" Height="100%" SkinID="DetailsInTab" MatrixMode="True">
                        <Levels>
                            <px:PXGridLevel DataMember="Answers" DataKeyNames="AttributeID,EntityType,EntityID">
                                <Columns>
                                    <px:PXGridColumn DataField="AttributeID" TextAlign="Left" AllowShowHide="False" />
    								<px:PXGridColumn DataField="isRequired" TextAlign="Center" Type="CheckBox" AllowResize="False" />
                                    <px:PXGridColumn DataField="Value" RenderEditorText="True" />
                                </Columns>
                            </px:PXGridLevel>
                        </Levels>
                        <AutoSize Enabled="True" />
                        <Mode AllowAddNew="False" AllowColMoving="False" AllowDelete="False" />
                    </px:PXGrid>
                </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="150" />
    </px:PXTab>
</asp:Content>
