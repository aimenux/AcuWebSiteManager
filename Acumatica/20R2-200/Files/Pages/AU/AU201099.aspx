<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true"
	ValidateRequest="false" CodeFile="AU201099.aspx.cs" Inherits="Page_AU201099"
	Title="Automation Step Maintenance" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
	<pxa:AUDataSource ID="ds" runat="server" Visible="True" Width="100%" TypeName="PX.SM.AUScreenDefinitionMaint"
		PrimaryView="Def">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Insert" PostData="Self" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" />
			<px:PXDSCallbackCommand Name="First" PostData="Self" StartNewGroup="True" />
			<px:PXDSCallbackCommand Name="Last" PostData="Self" />			
			<px:PXDSCallbackCommand Name="CopyPaste" Visible="False" />
			<px:PXDSCallbackCommand Name="ViewScreen" StartNewGroup="true" />
		</CallbackCommands>
		<DataTrees>
			<px:PXTreeDataMember TreeView="SiteMap" TreeKeys="NodeID" />
			<px:PXTreeDataMember TreeView="Graphs" TreeKeys="GraphName,IsNamespace" />
		</DataTrees>
	</pxa:AUDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
		Width="100%" Caption="Screen Definition" DataMember="Def" TemplateContainer="">
        <Template>
			<px:PXLayoutRule runat="server" StartColumn="True" ControlSize="M" 
				LabelsWidth="SM" />
			<px:PXTreeSelector ID="edScreenID" runat="server" DataField="ScreenID" PopulateOnDemand="True"
				ShowRootNode="False" TreeDataSourceID="ds" TreeDataMember="SiteMap" MinDropWidth="413">
				<DataBindings>
					<px:PXTreeItemBinding DataMember="SiteMap" TextField="Title" ValueField="ScreenID"
						ImageUrlField="Icon" />
				</DataBindings>
			</px:PXTreeSelector>
			<px:PXSelector ID="edProjectID" runat="server" DataField="ProjectID" DataSourceID="ds">				
			</px:PXSelector>			
        </Template>
		<Parameters>
			<px:PXControlParam ControlID="form" Name="WFScreenDefinition.screenID" PropertyName="NewDataKey[&quot;ScreenID&quot;]"
				Type="String" />
		</Parameters>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
	<px:PXTab ID="tab" runat="server" Height="168%" Style="z-index: 100" Width="100%"
		DataSourceID="ds" DataMember="CurrentDef">
		<Items>
		    <px:PXTabItem Text="Workflow Properties">
		        <Template>
		            <px:PXLayoutRule runat="server" StartColumn="True" ColumnSpan="2" ControlSize="XL" LabelsWidth="M" />
                    <px:PXCheckbox ID="edIsWorkflowEnabled" runat="server" DataField="IsWorkflowEnabled" />
                    <px:PXCheckbox ID="edIsFlowIdentifierRequired" runat="server" DataField="IsFlowIdentifierRequired" />
		            <px:PXDropDown ID="edFlowIdentifier" runat="server" DataField="FlowIdentifier" /> 
                    <px:PXDropDown ID="edStateIdentifier" runat="server" DataField="StateIdentifier" /> 
                </Template>
            </px:PXTabItem>
			<px:PXTabItem Text="Conditions">
			<Template>
			    <px:PXSplitContainer runat="server" ID="splitConditions" SplitterPosition="250" >
			        <Template1>
			            <px:PXGrid ID="gridConditions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details" SyncPosition="true" AllowPaging="false">
			                <AutoCallBack Enabled="true" Command="Refresh" Target="gridFilters"/>						        
					        <Levels>
						        <px:PXGridLevel DataMember="Conditions" >							        
							        <Columns>									        
									    <px:PXGridColumn DataField="ItemCD" Width="150px" AutoCallBack="true" />     
                                        <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox" Width="80px" />                                       
							        </Columns>
						        </px:PXGridLevel>
					        </Levels>
					        <AutoSize Enabled="True" MinHeight="150" />
                            <ActionBar ActionsVisible="true">
                                    <Actions> 
                                        <AdjustColumns  ToolBarVisible = "false" />
                                        <ExportExcel  ToolBarVisible ="false" />
                                        <Refresh ToolBarVisible ="False" />
                                    </Actions>
                            </ActionBar>  
				        </px:PXGrid>               
			        </Template1>
                    <Template2>
                        <px:PXGrid ID="gridFilters" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
						MatrixMode="true" AllowPaging="false">					        
					        <Levels>
						        <px:PXGridLevel DataMember="Filters" >
							        <Mode InitNewRow="True" />
							        <Columns>
									        <px:PXGridColumn AllowNull="False" DataField="IsActive" TextAlign="Center" Type="CheckBox"
										        Width="60px" />
									        <px:PXGridColumn AllowNull="False" DataField="OpenBrackets" Type="DropDownList" Width="100px"
										        AutoCallBack="true" />
									        <px:PXGridColumn DataField="FieldName" Width="200px" Type="DropDownList" AutoCallBack="true" />
									        <px:PXGridColumn AllowNull="False" DataField="Condition" Type="DropDownList" />
									        <px:PXGridColumn DataField="Value" Width="200px" />
									        <px:PXGridColumn DataField="Value2" Width="200px" />
									        <px:PXGridColumn AllowNull="False" DataField="CloseBrackets" Type="DropDownList"
										        Width="60px" />
									        <px:PXGridColumn AllowNull="False" DataField="Operator" Type="DropDownList" Width="60px" />
							        </Columns>
						        </px:PXGridLevel>
					        </Levels>
					        <AutoSize Enabled="True" MinHeight="150" />
                             <ActionBar ActionsVisible="true">
                                    <Actions> 
                                        <AdjustColumns  ToolBarVisible = "false" />
                                        <ExportExcel  ToolBarVisible ="false" />
                                        <Refresh ToolBarVisible ="False" />
                                    </Actions>
                            </ActionBar>  
				        </px:PXGrid>
                    </Template2>
			        <AutoSize Enabled="true" />	                    					                          
			    </px:PXSplitContainer>				
			</Template>
			</px:PXTabItem>
			<px:PXTabItem Text="Actions">
			<Template>
			    <px:PXSplitContainer runat="server" ID="splitActions" SplitterPosition="250"  >
			        <Template1>
			            <px:PXGrid ID="gridActions" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details" SyncPosition="true" AllowPaging="false">
			                <AutoCallBack Enabled="true" Command="Refresh" Target="gridActionAttributes"/>					        
					        <Levels>
						        <px:PXGridLevel DataMember="Buttons" >							        
							        <Columns>									        
									     <px:PXGridColumn DataField="ItemCD" Width="150px" AutoCallBack="true" />    
                                         <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox" Width="80px" />                                             
							        </Columns>
						        </px:PXGridLevel>
					        </Levels>
					        <AutoSize Enabled="True" MinHeight="150" />
                            <ActionBar ActionsVisible="true">
                                    <Actions> 
                                        <AdjustColumns  ToolBarVisible = "false" />
                                        <ExportExcel  ToolBarVisible ="false" />
                                        <Refresh ToolBarVisible ="False" />
                                    </Actions>
                            </ActionBar>  
				        </px:PXGrid>               
			        </Template1>
                    <Template2>
                        <px:PXGrid ID="gridActionAttributes" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
						MatrixMode="true" AllowPaging="false">					        
					        <Levels>
						        <px:PXGridLevel DataMember="ButtonAttributes" >
							        <Mode InitNewRow="True" />
							        <Columns>
									        <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox"
										        Width="80px" />
									        <px:PXGridColumn DataField="AttributeName" Width="200px" />									        
									        <px:PXGridColumn DataField="OriginalValue" Width="200px" />
									        <px:PXGridColumn DataField="OverrideValue" Width="200px" />
							        </Columns>
						        </px:PXGridLevel>
					        </Levels>
					        <AutoSize Enabled="True" MinHeight="150" />
                            <ActionBar ActionsVisible="false"/>                              
				        </px:PXGrid>
                    </Template2>
			        <AutoSize Enabled="true" />	                    					                          
			    </px:PXSplitContainer>
			</Template>
			</px:PXTabItem>	
            <px:PXTabItem Text="Reports">
			<Template>
			    <px:PXSplitContainer runat="server" ID="splitReports" SplitterPosition="250"  >
			        <Template1>
			            <px:PXGrid ID="gridReports" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details" SyncPosition="true" AllowPaging="false">
			                <AutoCallBack Enabled="true" Command="Refresh" Target="gridReportAttributes"/>					        
					        <Levels>
						        <px:PXGridLevel DataMember="Reports" >							        
							        <Columns>									        
									     <px:PXGridColumn DataField="ItemCD" Width="150px" AutoCallBack="true" />    
                                         <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox" Width="80px" />                                             
							        </Columns>
						        </px:PXGridLevel>
					        </Levels>
					        <AutoSize Enabled="True" MinHeight="150" />
                            <ActionBar ActionsVisible="true">
                                    <Actions> 
                                        <AdjustColumns  ToolBarVisible = "false" />
                                        <ExportExcel  ToolBarVisible ="false" />
                                        <Refresh ToolBarVisible ="False" />
                                    </Actions>
                            </ActionBar>  
				        </px:PXGrid>               
			        </Template1>
                    <Template2>
                        <px:PXGrid ID="gridReportAttributes" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
						MatrixMode="true" AllowPaging="false">					        
					        <Levels>
						        <px:PXGridLevel DataMember="ReportAttributes" >
							        <Mode InitNewRow="True" />
							        <Columns>
									        <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox"
										        Width="80px" />
									        <px:PXGridColumn DataField="AttributeName" Width="200px" />									        
									        <px:PXGridColumn DataField="OriginalValue" Width="200px" />
									        <px:PXGridColumn DataField="OverrideValue" Width="200px" />
							        </Columns>
						        </px:PXGridLevel>
					        </Levels>
					        <AutoSize Enabled="True" MinHeight="150" />
                            <ActionBar ActionsVisible="false"/>                              
				        </px:PXGrid>
                    </Template2>
			        <AutoSize Enabled="true" />	                    					                          
			    </px:PXSplitContainer>
			</Template>
			</px:PXTabItem>	
            <px:PXTabItem Text="Related">
			<Template>
			    <px:PXSplitContainer runat="server" ID="splitInquiries" SplitterPosition="250"  >
			        <Template1>
			            <px:PXGrid ID="gridInquiries" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details" SyncPosition="true" AllowPaging="false">
			                <AutoCallBack Enabled="true" Command="Refresh" Target="gridInquiryAttributes" ActiveBehavior="True">
			                    <Behavior  RepaintControlsIDs="gridInquiryNavAttributes"></Behavior>					        
                            </AutoCallBack>
					        <Levels>
						        <px:PXGridLevel DataMember="Inquiries" >							        
							        <Columns>									        
									     <px:PXGridColumn DataField="ItemCD" Width="150px" AutoCallBack="true" />    
                                         <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox" Width="80px" />                                             
							        </Columns>
						        </px:PXGridLevel>
					        </Levels>
					        <AutoSize Enabled="True" MinHeight="150" />
                            <ActionBar ActionsVisible="true">
                                    <Actions> 
                                        <AdjustColumns  ToolBarVisible = "false" />
                                        <ExportExcel  ToolBarVisible ="false" />
                                        <Refresh ToolBarVisible ="False" />
                                    </Actions>
                            </ActionBar>  
				        </px:PXGrid>               
			        </Template1>
                    <Template2>
                        <px:PXSplitContainer runat="server" ID="splitInquiryAttributes" SplitterPosition="350" SkinID="Horizontal"  >
                            <Template1>
                                <px:PXGrid ID="gridInquiryAttributes" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						            Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
						            MatrixMode="true" AllowPaging="false">					        
					                    <Levels>
						                    <px:PXGridLevel DataMember="InquiryAttributes" >
							                    <Mode InitNewRow="True" />
							                    <Columns>
									                    <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox"
										                    Width="80px" />
									                    <px:PXGridColumn DataField="AttributeName" Width="200px" />									        
									                    <px:PXGridColumn DataField="OriginalValue" Width="200px" />
									                    <px:PXGridColumn DataField="OverrideValue" Width="200px" />
							                    </Columns>
						                    </px:PXGridLevel>
					                    </Levels>
					                    <AutoSize Enabled="True" MinHeight="150" />
                                        <ActionBar ActionsVisible="false"/>                              
				                    </px:PXGrid>
                            </Template1>
                            <Template2>
                                <px:PXGrid ID="gridInquiryNavAttributes" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						            Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
						            MatrixMode="true" AllowPaging="false" Caption="Navigation Parameters" CaptionVisible="true">					        
					                    <Levels>
						                    <px:PXGridLevel DataMember="InquiryNavAttributes" >
							                    <Mode InitNewRow="True" />
							                    <Columns>
									                    <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox"
										                    Width="80px" />
									                    <px:PXGridColumn DataField="AttributeName" Width="200px" />
									                    <px:PXGridColumn DataField="Value" Width="200px" />
							                    </Columns>
						                    </px:PXGridLevel>
					                    </Levels>
					                    <AutoSize Enabled="True" MinHeight="150" />
                                        <ActionBar ActionsVisible="False">
                                            <Actions> 
                                                <AdjustColumns  ToolBarVisible = "false" />
                                                <ExportExcel  ToolBarVisible ="false" />
                                                <Refresh ToolBarVisible ="False" />
                                            </Actions>      
                                        </ActionBar>
				                    </px:PXGrid>
                            </Template2>
                            <AutoSize Enabled="True" MinHeight="450" />
                        </px:PXSplitContainer>                          
                    </Template2>
			        <AutoSize Enabled="true" />	                    					                          
			    </px:PXSplitContainer>
			</Template>
			</px:PXTabItem>
            <px:PXTabItem Text="Popup Forms">
			<Template>
			    <px:PXSplitContainer runat="server" ID="splitPopups" SplitterPosition="250"  >
			        <Template1>
			            <px:PXGrid ID="gridPopups" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details" SyncPosition="true" AllowPaging="false">
			                <AutoCallBack Enabled="true" Command="Refresh" Target="gridPopupAttributes" ActiveBehavior="True" >
                                <Behavior   RepaintControlsIDs="gridPopupFields"></Behavior>	
                            </AutoCallBack>			                
                            <Levels>
						        <px:PXGridLevel DataMember="Popups" >							        
							        <Columns>									        
									     <px:PXGridColumn DataField="ItemCD" Width="150px" AutoCallBack="true" />    
                                         <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox" Width="80px" />                                             
							        </Columns>
						        </px:PXGridLevel>
					        </Levels>
					        <AutoSize Enabled="True" MinHeight="150" />
                            <ActionBar ActionsVisible ="true">
                                    <Actions> 
                                        <AdjustColumns  ToolBarVisible = "false" />
                                        <ExportExcel  ToolBarVisible ="false" />
                                        <Refresh ToolBarVisible ="False" />
                                    </Actions>
                            </ActionBar>  
				        </px:PXGrid>               
			        </Template1>
                    <Template2>
                        <px:PXTab runat="server" ID="tabPopup" Style="z-index: 100" Width="100%">
                            <AutoSize Enabled="True" MinHeight="150" />                            
                            <Items>
                                  <px:PXTabItem Text="Properties">
                                      <Template>
                                           <px:PXGrid ID="gridPopupAttributes" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						                    Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details"
						                    MatrixMode="true" AllowPaging="false">					        
					                            <Levels>
						                            <px:PXGridLevel DataMember="PopupAttributes" >
							                            <Mode InitNewRow="True" />
							                            <Columns>
									                            <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox"
										                            Width="80px" />
									                            <px:PXGridColumn DataField="AttributeName" Width="200px" />									        
									                            <px:PXGridColumn DataField="OriginalValue" Width="200px" />
									                            <px:PXGridColumn DataField="OverrideValue" Width="200px" />
							                            </Columns>
						                            </px:PXGridLevel>
					                            </Levels>
					                            <AutoSize Enabled="True" MinHeight="150" />
                                               <ActionBar ActionsVisible="false"/>                                                                                                         
				                            </px:PXGrid>
                                      </Template>
                                  </px:PXTabItem>
                                  <px:PXTabItem Text="Fields">
                                      <Template>
                                          <px:PXSplitContainer runat="server" ID="splitPopupFields" SplitterPosition="200" SkinID="Horizontal" Width="100%" >
                                              <Template1>
                                                   <px:PXGrid ID="gridPopupFields" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						                            Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details" SyncPosition="true" AllowPaging="false">
			                                            <AutoCallBack Enabled="true" Command="Refresh" Target="gridPopupFieldAttributes"/>					        
					                                    <Levels>
						                                    <px:PXGridLevel DataMember="PopupFields"  >						                                        							        
							                                    <Columns>									        
									                                 <px:PXGridColumn DataField="ItemCD" Width="150px" Type="DropDownList" AutoCallBack="true" />                                                                      
                                                                     <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox" Width="80px" />                                             
							                                    </Columns>
						                                    </px:PXGridLevel>
					                                    </Levels>
					                                    <AutoSize Enabled="True" MinHeight="150" />
                                                        <ActionBar ActionsVisible ="true">
                                                                <Actions> 
                                                                    <AdjustColumns  ToolBarVisible = "false" />
                                                                    <ExportExcel  ToolBarVisible ="false" />
                                                                    <Refresh ToolBarVisible ="False" />
                                                                </Actions>
                                                        </ActionBar>  
				                                    </px:PXGrid>   
                                              </Template1>
                                              <Template2>
                                                  <px:PXGrid ID="gridPopupFieldAttributes" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100"
						                                Width="100%" AdjustPageSize="Auto" BorderWidth="0px" AllowSearch="True" SkinID="Details" AllowPaging="false"
						                                MatrixMode="true">					        
					                                        <Levels>
						                                        <px:PXGridLevel DataMember="PopupFieldAttributes" >
							                                        <Mode InitNewRow="True" />
							                                        <Columns>
									                                        <px:PXGridColumn AllowNull="False" DataField="IsOverride" TextAlign="Center" Type="CheckBox"
										                                        Width="80px" />
									                                        <px:PXGridColumn DataField="AttributeName" Width="200px" />									        
									                                        <px:PXGridColumn DataField="OriginalValue" Width="200px" />
									                                        <px:PXGridColumn DataField="OverrideValue" Width="200px" />
							                                        </Columns>
						                                        </px:PXGridLevel>
					                                        </Levels>
					                                        <AutoSize Enabled="True" MinHeight="150" />
                                                            <ActionBar ActionsVisible="false"/>                                                              
				                                        </px:PXGrid>
                                              </Template2>
                                               <AutoSize Enabled="true" />
                                          </px:PXSplitContainer>
                                      </Template>
                                  </px:PXTabItem>
                            </Items>
                        </px:PXTab>
                       
                    </Template2>
			        <AutoSize Enabled="true" />	                    					                          
			    </px:PXSplitContainer>
			</Template>
			</px:PXTabItem>		
		</Items>
        <AutoSize Container="Window" Enabled="True" MinHeight="250" MinWidth="300" />
	</px:PXTab>
    		
</asp:Content>
