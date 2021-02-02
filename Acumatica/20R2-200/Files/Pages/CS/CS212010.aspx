<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" CodeFile="CS212010.aspx.cs" Inherits="Pages_CS_CS212010" %>

<asp:Content ID="Content1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" 
		Width="100%" PrimaryView="ViewImportStatus" 
		TypeName="PX.Web.Customization.API.ApiImportGraph" 
		PageLoadBehavior="GoFirstRecord">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="actionPreviewDataset" PostData="Self" BlockPage="True" CommitChanges="True" RepaintControls="Bound" />
			<px:PXDSCallbackCommand Name="actionImportValidItems" PostData="Self" BlockPage="True" CommitChanges="True" RepaintControls="Bound" />
			<px:PXDSCallbackCommand Name="actionImportAllItems" PostData="Self" BlockPage="True" CommitChanges="True" RepaintControls="Bound" />
			<px:PXDSCallbackCommand DependOnGrid="GridBatches" Name="actionEditorScreen" PostData="Self" BlockPage="True" CommitChanges="True" RepaintControls="Bound" />
<%--			<px:PXDSCallbackCommand Name="createDataset" PostData="Self" BlockPage="True" CommitChanges="True" RepaintControls="Bound" />
--%>
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="phF" Runat="Server">

	<script type="text/javascript" >

	var px_all2;
	function IndexObjects() {
		if (px_all2)
			return;
		px_all2 = {};
		for (var n in px_all) {
			var names = n.split("_");
			var s = names[names.length - 1];
			px_all2[s] = px_all[n];
		}
	}

	function GetObject(id) {
		IndexObjects();
		return px_all2[id];
	}

	function OnUploadFile() 
	{

		var btn = GetObject("PXButton1");

		btn.exec({});
		
	}
</script>
	<px:PXUploadDialog ID="UploadDialog" runat="server" Height="280px" AllowedFileTypes=".xlsx"
		Width="405px" AutoSaveFile="False" Caption="Upload Data to Import (*.xlsx)" 
		onfileuploadfinished="UploadDialog_FileUploadFinished" UploadEvents-HideAfterUpload="OnUploadFile">

	</px:PXUploadDialog>


	
	<px:PXFormView ID="PXFormView1" runat="server" Height="150px" Width="100%" 
		DataMember="ViewImportStatus" DataSourceID="ds" >
		<Template>
		
			
			<px:PXLabel ID="lblScreenId" runat="server" EnableClientScript="False" 
				style="z-index:100;position:absolute;left:12px; top:19px; height: 22px;">ScreenId:</px:PXLabel>
			<px:PXSelector ID="edScreenId" runat="server" DataField="MappingID" 
				  
				LabelID="lblScreenId"  
				style="z-index:101;position:absolute;left:129px; top:15px; width: 291px;" 
				TabIndex="-1" TextMode="ReadOnly" AutoGenerateColumns="True">

				<AutoCallBack Enabled="True" Target="PXFormView1" Command="Save" 
					ActiveBehavior="True">
					<Behavior BlockPage="True" CommitChanges="True" RepaintControls="Bound" />
				</AutoCallBack>
			</px:PXSelector>		
		
			
			<px:PXSelector ID="PXSelector1" runat="server" DataField="TableName" 
				   
				 
				style="z-index:101;position:absolute;left:6px; top:126px; width: 291px;" 
				TabIndex="-1" TextMode="ReadOnly">
				<GridProperties>
					<Columns>
						<px:PXGridColumn DataField="TableName"  Width="200px">
							<Header Text="TableName">
							</Header>
						</px:PXGridColumn>
					</Columns>
				<PagerSettings Mode="NextPrevFirstLast" /></GridProperties>
								<AutoCallBack Enabled="True" Target="PXFormView1" 
					Command="Save" ActiveBehavior="True">
									<Behavior BlockPage="True" CommitChanges="True" RepaintControls="Bound" />
				</AutoCallBack>

			</px:PXSelector>			
			
			
			<px:PXLabel runat="server" EnableClientScript="False" ID="lblFileName" 
				style="z-index:100;position:absolute;left:559px; top:60px;">FileName:</px:PXLabel>
		
		
			<px:PXTextEdit runat="server"  LabelID="lblStatus" 
				DataField="Status" TabIndex="11" Width="297px" ID="edStatus" 
				style="z-index:103;position:absolute;left:620px; top:14px;">
			</px:PXTextEdit>

			
			
<%--			<px:PXListBox  runat="server" DataField="TableName"
				style="z-index:101;position:absolute;left:562px; top:9px; width: 291px;"
			  DataMember="ViewTableList" DataValueField="TableName" DataTextField="TableName" AppendDataBoundItems="True">
			</px:PXListBox>--%>
			<px:PXLabel runat="server" EnableClientScript="False" ID="lblStatus" 
				style="z-index:102;position:absolute;left:560px; top:13px; height: 13px;">Status:</px:PXLabel>
			

	
			

			
			<px:PXTextEdit runat="server"  LabelID="lblFileName" 
				DataField="FileName" TabIndex="10" ID="edFileName" 
				style="z-index:101;position:absolute;left:622px; top:56px; width: 294px;">
			</px:PXTextEdit>
			
			

			
			<px:PXButton ID="PXButton1" runat="server" 
				style="position:absolute;left:625px; top:82px; width: 69px; height: 12px;" 
				  Text="DataRows" >
				 <AutoCallBack Enabled="true" Target="PXFormView1" Command="Refresh" 
					 ActiveBehavior="True">
					 <Behavior BlockPage="True" RepaintControls="Bound" />
				 </AutoCallBack>

			</px:PXButton>

					
			
			<px:PXButton ID="PXButton2" runat="server" 
				style="position:absolute;left:622px; top:80px; width: 92px;" 
				 Target="_blank" Text="Upload" PopupPanel="UploadDialog">

			</px:PXButton>
			
			
		</Template>
	</px:PXFormView>

</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="phG" Runat="Server">

	<px:PXTab ID="PXTab1" runat="server" Height="400px" Width="100%">
	<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Items>
			<px:PXTabItem Text='Processed Batches'>
			
			<Template>
				<px:PXGrid ID="GridBatches" runat="server" Height="400px" Width="100%" AutoGenerateColumns="Recreate"
				DataSourceID="ds" 
				RepaintColumns="true"
				GenerateColumnsBeforeRepaint = "true">
				<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
				<Levels>
				<px:PXGridLevel DataMember="ViewBatches" ></px:PXGridLevel>
				
				</Levels>
				</px:PXGrid>			
			</Template>
			
			</px:PXTabItem>
			
			<px:PXTabItem Text='Imported Rows'>
			<Template>
				<px:PXGrid ID="PXGrid2" runat="server" Height="400px" Width="100%" AutoGenerateColumns="Recreate"
				DataSourceID="ds" 
				RepaintColumns="true"
				GenerateColumnsBeforeRepaint = "true">
				<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
				<Levels>
				<px:PXGridLevel DataMember="ViewBatchRows"></px:PXGridLevel>
				
				</Levels>
				</px:PXGrid>			
			</Template>
			
			</px:PXTabItem>
			
						
			<px:PXTabItem Text='Upload File Content'>
			<Template>
				<px:PXGrid ID="PXGrid2" runat="server" Height="400px" Width="100%" AutoGenerateColumns="Recreate"
				DataSourceID="ds" 
				RepaintColumns="true"
				GenerateColumnsBeforeRepaint = "true">
				<AutoSize Container="Parent" Enabled="True" MinHeight="200" />
				<Levels>
				<px:PXGridLevel DataMember="ViewDatasetContent"></px:PXGridLevel>
				
				</Levels>
				</px:PXGrid>			
			</Template>
			
			</px:PXTabItem>
		</Items>
	</px:PXTab>

		
<%--	<px:PXGrid ID="PXGrid1" runat="server" Height="400px" Width="100%" 
		DataSourceID="ds">
		<Levels>
<px:PXGridLevel DataMember="ViewLog">
	<Columns>
		<px:PXGridColumn DataField="LogId" DataType="Int32" TextAlign="Right">
			<Header Text="LogId">
			</Header>
		</px:PXGridColumn>
		<px:PXGridColumn DataField="BatchId" DataType="Int32" TextAlign="Right">
			<Header Text="BatchId">
			</Header>
		</px:PXGridColumn>
		<px:PXGridColumn DataField="RowId" DataType="Int32" TextAlign="Right">
			<Header Text="RowId">
			</Header>
		</px:PXGridColumn>
		<px:PXGridColumn DataField="Status" MaxLength="10">
			<Header Text="Status">
			</Header>
		</px:PXGridColumn>
		<px:PXGridColumn DataField="TableName" MaxLength="50" Width="200px">
			<Header Text="TableName">
			</Header>
		</px:PXGridColumn>
		<px:PXGridColumn DataField="SrcKeys" MaxLength="50" Width="200px">
			<Header Text="SrcKeys">
			</Header>
		</px:PXGridColumn>
		<px:PXGridColumn DataField="ImportedKeys" MaxLength="50" Width="200px">
			<Header Text="ImportedKeys">
			</Header>
		</px:PXGridColumn>
		<px:PXGridColumn DataField="Message" MaxLength="50" Width="200px">
			<Header Text="Message">
			</Header>
		</px:PXGridColumn>
		<px:PXGridColumn DataField="Errors" MaxLength="50" Width="200px">
			<Header Text="Errors">
			</Header>
		</px:PXGridColumn>
	</Columns>
<Layout FormViewHeight=""></Layout>
</px:PXGridLevel>
</Levels>
	</px:PXGrid>--%>
</asp:Content>

