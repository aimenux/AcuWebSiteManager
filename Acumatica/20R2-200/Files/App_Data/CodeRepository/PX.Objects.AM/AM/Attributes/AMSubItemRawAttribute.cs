using PX.Data;
using PX.Objects.IN;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Allows entry of the Sub item CD value while keeping the validcomborequired default from the INSubItem segmented on the fly setting
    /// </summary>
    [PXDBString(30, InputMask = "", IsUnicode = true)]
    [PXUIField(DisplayName = "Subitem", FieldClass = "INSUBITEM", Visibility = PXUIVisibility.SelectorVisible)]
    public class AMSubItemRawAttribute : PX.Objects.IN.SubItemRawExtAttribute
    {
        /// <summary>
        /// Replaces the SubItemRawExtAttribute call with the same signature only removing the fixed ValidComboRequied = false setting.
        /// </summary>
        public AMSubItemRawAttribute(System.Type inventoryItem)
            : base()
        {
          if (inventoryItem == null)
          {
              return;
          }

          this._Attributes.Add((PXEventSubscriberAttribute) new PXDimensionSelectorAttribute("INSUBITEM", 
              BqlCommand.Compose(typeof (Search<,>), 
              typeof (INSubItem.subItemCD), 
              typeof (Where2<,>), 
              typeof (Match<>), 
              typeof (Current<AccessInfo.userName>), 
              typeof (And<>), 
              typeof (Where<,,>), 
              typeof (Optional<>), 
              inventoryItem, 
              typeof (IsNull), 
              typeof (Or<>), 
              typeof (Where<>), 
              typeof (Match<>), 
              typeof (Optional<>), inventoryItem)));

          this._SelAttrIndex = this._Attributes.Count - 1;
        }
    }
}