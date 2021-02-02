<%@ Page Title="Data Migration Tool" Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" CodeFile="CS212000.aspx.cs" Inherits="Pages_CS_CS212000" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" 
		Width="100%" PrimaryView="ViewScreen" 
		TypeName="PX.Api.ApiGraph" PageLoadBehavior="GoFirstRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand CommitChanges="True" Name="Save" BlockPage="True" />
			<px:PXDSCallbackCommand CommitChanges="True" Name="copyTable" BlockPage="True" DependOnGrid="Grid" PostData="Page" PostDataControls="True" RepaintControls="All" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXUploadDialog ID="UploadDialog" runat="server" Height="280px" AllowedFileTypes=".xlsx"
		Width="405px" AutoSaveFile="False" Caption="Upload Data to Import (*.xlsx)">
		
	</px:PXUploadDialog>
	<px:PXFormView ID="form" runat="server" DataMember="ViewScreen" 
		DataSourceID="ds" Height="150px" Width="100%" >

		
		<Template>
			<px:PXLabel runat="server" EnableClientScript="False" ID="lblScreenID" style="z-index:100;position:absolute;left:9px;top:9px;">ScreenID:</px:PXLabel>
			<px:PXSelector runat="server" DataField="ScreenID" 
				LabelID="lblScreenID"  TextMode="ReadOnly" 
				  TabIndex="-1" ID="edScreenID" 
				style="z-index:101;position:absolute;left:126px;top:9px; height: 19px; width: 108px;">
				<AutoCallBack Enabled="True" Target="form" ActiveBehavior="True" 
					Command="Refresh">
					<Behavior BlockPage="True" RepaintControls="Bound" />
				</AutoCallBack>
				<GridProperties>
					<Columns>
						<px:PXGridColumn DataField="ScreenID">
							<Header Text="ScreenID">
							</Header>
						</px:PXGridColumn>
					</Columns>
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
			</px:PXSelector>
			<px:PXLabel runat="server" EnableClientScript="False" ID="lblGraphName" style="z-index:102;position:absolute;left:9px;top:36px;">GraphName:</px:PXLabel>
			<px:PXTextEdit runat="server" LabelID="lblGraphName" DataField="GraphName" TabIndex="11" Width="108px" ID="edGraphName" style="z-index:103;position:absolute;left:126px;top:36px;">
			</px:PXTextEdit>
			<px:PXLabel runat="server" EnableClientScript="False" ID="lblPersistAction" style="z-index:104;position:absolute;left:9px;top:63px;">PersistAction:</px:PXLabel>
			<px:PXTextEdit runat="server" LabelID="lblPersistAction" DataField="PersistAction" TabIndex="12" Width="108px" ID="edPersistAction" style="z-index:105;position:absolute;left:126px;top:63px;">
			</px:PXTextEdit>
			
			<px:PXLabel runat="server" EnableClientScript="False" ID="lblPrimView" style="z-index:104;position:absolute;left:9px;top:93px;">Primary View:</px:PXLabel>
			<px:PXTextEdit runat="server" LabelID="lblPrimView" DataField="PrimaryView" TabIndex="12" Width="108px" ID="edPrimaryView" style="z-index:105;position:absolute;left:126px;top:93px;">
			</px:PXTextEdit>
			<px:PXButton ID="PXButton1" runat="server" 
				style="position:absolute;left:295px; top:15px; width: 92px;" 
				NavigateUrl="~/Controls/Dts.aspx" Target="_blank" Text="Export">
				<NavigateParams>
					<px:PXControlParam Name="screenID" ControlID="edScreenID" PropertyName="Text" Type="String" />
				</NavigateParams>
			</px:PXButton>
<%--			<px:PXButton ID="PXButton2" runat="server" 
				style="position:absolute;left:295px; top:55px; width: 92px;" 
				 Target="_blank" Text="Import" PopupPanel="UploadDialog">

</px:PXButton>--%>

		</Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" Runat="Server">
	
		<px:PXTab ID="PXTab1" runat="server" Height="150px" Width="100%">
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Items>
			<px:PXTabItem Text='Tables'>
				<Template>
<px:PXGrid ID="Grid" runat="server" DataSourceID="ds" Width="100%" 
		Height="400px" AutoAdjustColumns="True">
		<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
						<Levels>
							<px:PXGridLevel  DataMember="ViewScreenDetais">
								<RowTemplate>

									<px:PXLabel ID="lblTableName" runat="server" EnableClientScript="False" 
										style="z-index:102; position:absolute;left:9px;top:36px;">Table Name:</px:PXLabel>
									<px:PXTextEdit ID="edTableName" runat="server" DataField="TableName" 
										LabelID="lblTableName"  
										style="z-index:103;position:absolute;left:126px;top:36px;" TabIndex="11" 
										Width="297px">
									</px:PXTextEdit>
									<px:PXLabel ID="lblParentTableName" runat="server" EnableClientScript="False" 
										style="z-index:104; position:absolute;left:9px;top:63px;">ScreenID:Parent Table:</px:PXLabel>
									<px:PXSelector ID="edParentTableName" runat="server" DataField="ParentTableName" 
										  
										 LabelID="lblParentTableName"
										AutoGenerateColumns="true"
										style="z-index:105; position:absolute;left:126px;top:63px;" TabIndex="-1" 
										TextMode="ReadOnly" Width="108px">
									</px:PXSelector>
									<px:PXLabel ID="lblViewName" runat="server" EnableClientScript="False" 
										style="z-index:106;position:absolute;left:9px;top:90px;">SelectParams:</px:PXLabel>
									<px:PXTextEdit ID="edViewName" runat="server" DataField="ViewName" 
										LabelID="lblViewName" 
										style="z-index:107; position:absolute;left:126px;top:90px;" TabIndex="13" 
										Width="297px" >
									</px:PXTextEdit>
									<px:PXLabel ID="lblSelectParams" runat="server" EnableClientScript="False" 
										style="z-index:108;position:absolute;left:9px;top:117px;">InsertAction:</px:PXLabel>
									<px:PXTextEdit ID="edSelectParams" runat="server" DataField="SelectParams" 
										LabelID="lblSelectParams" 
										style="z-index:109; position:absolute;left:126px;top:117px;" TabIndex="14" 
										Width="108px">
									</px:PXTextEdit>
									<px:PXLabel ID="lblInsertAction" runat="server" EnableClientScript="False" 
										style="z-index:110;position:absolute;left:9px;top:144px;"></px:PXLabel>
									<px:PXTextEdit ID="edInsertAction" runat="server" DataField="InsertAction" 
										LabelID="lblInsertAction" 
										style="z-index:111;position:absolute;left:126px;top:144px;" TabIndex="15" 
										Width="108px">
									</px:PXTextEdit>
									<px:PXLabel ID="lblInsertMethod" runat="server" EnableClientScript="False" 
										style="z-index:112;position:absolute;left:9px;top:171px;">InsertMethod:</px:PXLabel>
									<px:PXDropDown ID="edInsertMethod" runat="server" DataField="InsertMethod" 
										LabelID="lblInsertMethod" 
										style="z-index:113; position:absolute;left:126px;top:171px;" TabIndex="-1" 
										Width="117px">
										
									</px:PXDropDown>
									<px:PXLabel ID="lblImportFields" runat="server" EnableClientScript="False" 
										style="z-index:114; position:absolute;left:9px;top:198px;">ImportFields:</px:PXLabel>
									<px:PXTextEdit ID="edImportFields" runat="server" DataField="ImportFields" 
										LabelID="lblImportFields" 
										style="z-index:115; position:absolute;left:126px;top:198px;" TabIndex="17" 
										Width="108px">
									</px:PXTextEdit>
									<px:PXLabel ID="lblPrevTableName" runat="server" EnableClientScript="False" 
										style="z-index:100;position:absolute;left:9px;top:9px;">Prev Table:</px:PXLabel>
									<px:PXSelector ID="edPrevTableName" runat="server" DataField="PrevTableName" 
										  
										 LabelID="lblPrevTableName" 
										style="z-index:101;position:absolute;left:126px;top:9px;" TabIndex="-1" 
										TextMode="ReadOnly" Width="108px">
										<GridProperties>
											<Columns>
												<px:PXGridColumn DataField="TableName"  Width="200px">
													<Header Text="Table Name">
													</Header>
												</px:PXGridColumn>
											</Columns>
										<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
									</px:PXSelector>
								</RowTemplate>
								<Columns>

									<px:PXGridColumn DataField="TableName" Width="200px" >
										<Header Text="Table Name">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="ParentTableName" Width="108px">
										<Header Text="Parent Table">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="ViewName" Width="200px" >
										<Header Text="Table Name">
										</Header>
									</px:PXGridColumn>

									<px:PXGridColumn DataField="DisplayName" Width="200px" >
										<Header Text="Screen Container">
										</Header>
									</px:PXGridColumn>
									
									<px:PXGridColumn DataField="SelectParams" Width="108px">
										<Header Text="SelectParams">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="Searches" Width="100px" RenderEditorText="True">
										<Header Text="Searches">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="InsertAction" Width="108px">
										<Header Text="InsertAction">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="InsertMethod" RenderEditorText="True" Width="117px">
										<Header Text="InsertMethod">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="ImportFields" Width="108px">
										<Header Text="ImportFields">
										</Header>
									</px:PXGridColumn>
									<px:PXGridColumn DataField="PrevTableName" Width="108px">
										<Header Text="Prev Table">
										</Header>
									</px:PXGridColumn>
								</Columns>
								<Layout FormViewHeight="" />
							</px:PXGridLevel>
						</Levels>
					</px:PXGrid>			
				</Template>
			</px:PXTabItem>
			
			<px:PXTabItem Text='Fields'>
				<Template>
								
				<px:PXGrid ID="PXGrid2" runat="server" Height="400px" Width="100%" AutoGenerateColumns="Recreate"
				DataSourceID="ds" >
				<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
				<Levels>
				<px:PXGridLevel DataMember="ViewFields" ></px:PXGridLevel>
				
				</Levels>
				</px:PXGrid>
				
				</Template>
			</px:PXTabItem>
		</Items>
	</px:PXTab>
	
	
	
</asp:Content>

