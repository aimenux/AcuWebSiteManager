<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="SM206550.aspx.cs" Inherits="Page_SM206550" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">   
     <px:PXDataSource id="ds" width="100%" runat="server" typename="PX.Api.ImageRecognition.ImageRecognitionModelMaint" primaryview="Mapping" Visible="True">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="Insert" PostData="Self" />
            <px:PXDSCallbackCommand Name="Save" CommitChanges="True"/>            
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">     
   <px:PXFormView ID="form" runat="server" DataSourceID="ds" Width="100%" Visible="true"
    DataMember="Mapping" AllowCollapse="false">
        <Template>                
                <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XM" />
                <px:PXLayoutRule runat="server" Merge="True" />
                <px:PXSelector ID="edMappingID" runat="server" DataField="MappingID"  CommitChanges="True" />                
                <px:PXCheckBox runat="server" Checked="True" DataField="Active" ID="chkIsActive" />                
                <px:PXLayoutRule runat="server" />
                <px:PXTextEdit runat="server" DataField="ActionName" ID="edActionName" AllowNull="False" />                
                <px:PXSelector ID="edScreenID0" runat="server" DataField="ScreenID"  CommitChanges="True" />
                <px:PXSelector CommitChanges="True" runat="server" DataField="ModelID" 
                    ID="edModelID" DataSourceID="ds" />
                
        </Template>
    </px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXTab ID="tab" runat="server" Height="300px" Style="z-index: 100" Width="100%">
        <Items>
            <px:PXTabItem Text="Mapping Details">
                <Template>
                    <px:PXGrid ID="gridMapping" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AllowPaging="False" AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab"
						AutoAdjustColumns="True" MatrixMode="true" SyncPosition="true" KeepPosition="true">						
						<Levels>
							<px:PXGridLevel DataMember="FieldMapping">
								<Mode InitNewRow="True" />
								<RowTemplate>
									<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
									<pxa:PXFormulaCombo ID="edValue" runat="server" DataField="Value" EditButton="True"
										FieldsAutoRefresh="True" />
								</RowTemplate>
								<Columns>
									<px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
									<px:PXGridColumn DataField="ViewName" Width="150px" AutoCallBack="true" Type="DropDownList" />
									<px:PXGridColumn DataField="FieldName" Width="150px" AutoCallBack="true" Type="DropDownList"/>
									<px:PXGridColumn DataField="MappedFieldName" Width="150px" AutoCallBack="true" Type="DropDownList" />
								</Columns>
							</px:PXGridLevel>
						</Levels>
						<CallbackCommands>
							<Save PostData="Page" />							
						</CallbackCommands>
						<AutoSize Enabled="True" MinHeight="150" />
						<Mode AllowUpload="True" />
					</px:PXGrid>
                </Template>
            </px:PXTabItem>
            <px:PXTabItem Text="Action screens">
                    <Template>
                            <px:PXGrid ID="gridMapping" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                                Width="100%" AllowPaging="False" AdjustPageSize="Auto" AllowSearch="True" SkinID="DetailsInTab"
                                AutoAdjustColumns="True" MatrixMode="true" SyncPosition="true" KeepPosition="true">						
                                <Levels>
                                    <px:PXGridLevel DataMember="TargetScreens">
                                        <Mode InitNewRow="True" />
                                        <RowTemplate>
                                            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="M" ControlSize="XM" />
                                            <pxa:PXFormulaCombo ID="edValue1" runat="server" DataField="Value" EditButton="True"
                                                FieldsAutoRefresh="True" />
                                        </RowTemplate>
                                        <Columns>
                                            <px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" Width="60px" />
                                            <px:PXGridColumn DataField="ScreenID" Width="150px" AutoCallBack="true" DisplayFormat="CC.CC.CC.CC"  />
                                            <px:PXGridColumn DataField="ViewName" Width="150px" AutoCallBack="true" Type="DropDownList"/>                                            
                                        </Columns>
                                    </px:PXGridLevel>
                                </Levels>
                                <CallbackCommands>
                                    <Save PostData="Page" />							
                                </CallbackCommands>
                                <AutoSize Enabled="True" MinHeight="150" />
                                <Mode AllowUpload="True" />
                            </px:PXGrid>
                        </Template>
            </px:PXTabItem>
        </Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
    </px:PXTab>
</asp:Content>