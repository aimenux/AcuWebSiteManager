<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU201010.aspx.cs" Inherits="Page_AU201010"
	Title="Automation Step Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXFormView runat="server" SkinID="transparent" ID="formTitle" 
                   DataSourceID="ds" DataMember="ViewPageTitle" Width="100%">
        <Template>
            <px:PXTextEdit runat="server" ID="PageTitle" DataField="PageTitle" SelectOnFocus="False"
                           SkinID="Label" SuppressLabel="true"
                           Width="90%"
                           style="padding: 10px">
                <font size="14pt" names="Arial,sans-serif;"/>
            </px:PXTextEdit>
        </Template>
    </px:PXFormView>
	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUScreenConditionMaint"
		PrimaryView="Conditions" >
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Cancel" />
            <px:PXDSCallbackCommand CommitChanges="True" Name="Save" /> 
		</CallbackCommands>
	</pxa:AUDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXSplitContainer runat="server" ID="splitConditions" SplitterPosition="250" Style="border-top: solid 1px #BBBBBB;" >
			<Template1>
			    <px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds" Height="150" Style="z-index: 100"
				Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details" SyncPosition="true" AllowPaging="false" Caption="Conditions" CaptionVisible="true">
			        <Mode AllowFormEdit="False" InitNewRow="True" />
			        <AutoCallBack Enabled="true" Command="Refresh" Target="gridFilters"/>	
			        <AutoSize Container="Parent" Enabled="True" MinHeight="150" />
			        <ActionBar>
			            <Actions>
			                <AdjustColumns  ToolBarVisible = "false" />
			                <ExportExcel  ToolBarVisible ="false" />
			            </Actions>
			        </ActionBar>
					<Levels>
						<px:PXGridLevel DataMember="Conditions" SortOrder="Order">							        
							<Columns>									        
								<px:PXGridColumn DataField="ConditionName" AutoCallBack="true" Width="150" CommitChanges="true"/>                                    
							</Columns>
						</px:PXGridLevel>
					</Levels>
					<AutoSize Enabled="True" MinHeight="150" />
				</px:PXGrid>               
			</Template1>
            <Template2>
                <px:PXGrid ID="gridFilters" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
                           Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
                           MatrixMode="true" AllowPaging="false" AutoAdjustColumns="true" Caption="Condition Details" CaptionVisible="true"
                           OnEditorsCreated="grd_EditorsCreated_RelativeDates">					
                    <ActionBar>
                        <Actions>
                            <AdjustColumns  ToolBarVisible = "false" />
                            <ExportExcel  ToolBarVisible ="false" />
                        </Actions>
                    </ActionBar> 
                    <Levels>
                        <px:PXGridLevel DataMember="Filters" >
                            <Mode InitNewRow="True" />
                            <Columns>
                                <px:PXGridColumn DataField="IsActive" Type="CheckBox" Width="60px" TextAlign="Center"  />
                                <px:PXGridColumn DataField="OpenBrackets" Type="DropDownList" Width="50px" />
                                <px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
                                <px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" CommitChanges="True"/>
                                <px:PXGridColumn DataField="Value" Width="200px" AllowSort="False" MatrixMode="true" AllowStrings="True" DisplayMode="Value"/>
                                <px:PXGridColumn DataField="Value2" Width="200px" AllowSort="False" MatrixMode="true" AllowStrings="True" DisplayMode="Value"/>
                                <px:PXGridColumn DataField="CloseBrackets" Type="DropDownList" Width="50px" />
                                <px:PXGridColumn DataField="Operator" Type="DropDownList" Width="50px" />
                            </Columns>
                        </px:PXGridLevel>
                    </Levels>
                    <AutoSize Enabled="True" MinHeight="150" />      
                    <CallbackCommands>
                        <InitRow CommitChanges="true" />
                    </CallbackCommands>
                </px:PXGrid>
            </Template2>
			<AutoSize Enabled="true" Container="Window" />	                    					                          
		</px:PXSplitContainer>	     		
</asp:Content>
