using System;

namespace UnityEditor.Experimental.UIElements
{
	internal enum ImportErrorCode
	{
		InvalidRootElement,
		DuplicateUsingAlias,
		UnknownElement,
		UnknownAttribute,
		InvalidXml,
		InvalidCssInStyleAttribute,
		MissingPathAttributeOnUsing,
		UsingHasEmptyAlias,
		StyleReferenceEmptyOrMissingPathAttr,
		DuplicateSlotDefinition,
		SlotUsageInNonTemplate,
		SlotDefinitionHasEmptyName,
		SlotUsageHasEmptyName,
		DuplicateContentContainer
	}
}
