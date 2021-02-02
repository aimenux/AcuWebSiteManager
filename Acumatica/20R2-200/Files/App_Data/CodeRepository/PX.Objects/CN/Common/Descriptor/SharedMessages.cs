using PX.Common;

namespace PX.Objects.CN.Common.Descriptor
{
    [PXLocalizable]
    public static class SharedMessages
    {
	    public const string Warning = "Warning";

	    public const string DefaultValue = "Default Value";
	    public const string DescriptionFieldPostfix = "_description";

	    public const string FieldIsEmpty = "Error: '{0}' cannot be empty.";
	    public const string CannotBeFound = "Error: '{0}' cannot be found in the system.";
	    public const string RequiredAttributesAreEmpty = "There are empty required attributes: {0}";
	}
}