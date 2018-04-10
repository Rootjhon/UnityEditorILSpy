using System;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditorInternal.VR
{
	[NativeType(CodegenOptions = CodegenOptions.Custom), RequiredByNativeCode]
	public struct VRDeviceInfoEditor
	{
		public string deviceNameKey;

		public string deviceNameUI;

		public string externalPluginName;

		public bool supportsEditorMode;

		public bool inListByDefault;
	}
}
