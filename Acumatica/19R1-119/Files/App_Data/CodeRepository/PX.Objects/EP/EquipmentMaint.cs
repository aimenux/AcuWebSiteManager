using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PX.Data;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	public class EquipmentMaint : PXGraph<EquipmentMaint, EPEquipment>
	{
		#region DAC Attributes Override

		[PXDBIdentity()]
		protected virtual void EPEquipment_EquipmentID_CacheAttached(PXCache sender)
		{ }

		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Equipment ID")]
		[PXSelector(typeof(Search<EPEquipment.equipmentCD>), typeof(EPEquipment.equipmentCD), typeof(EPEquipment.description), typeof(EPEquipment.status))]
		protected virtual void EPEquipment_EquipmentCD_CacheAttached(PXCache sender)
		{ }

		#endregion
        

		#region Views/Selects

		public PXSelect<EPEquipment> Equipment;
		public PXSelect<EPEquipment, Where<EPEquipment.equipmentID, Equal<Current<EPEquipment.equipmentID>>>> EquipmentProperties;
		public PXSelectJoin<EPEquipmentRate, InnerJoin<PMProject, On<PMProject.contractID, Equal<EPEquipmentRate.projectID>>>, Where<EPEquipmentRate.equipmentID, Equal<Current<EPEquipment.equipmentID>>>> Rates;

		[PXViewName(Messages.EquipmentAnswers)]
		public CRAttributeList<EPEquipment> Answers;

		#endregion

	}
}
