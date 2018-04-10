using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditorInternal.VR
{
	internal class PlayerSettingsEditorVR
	{
		private static class Styles
		{
			public static readonly GUIContent singlepassAndroidWarning = EditorGUIUtility.TextContent("Single Pass stereo rendering requires OpenGL ES 3. Please make sure that it's the first one listed under Graphics APIs.");

			public static readonly GUIContent singlepassAndroidWarning2 = EditorGUIUtility.TextContent("Multi Pass will be used on Android devices that don't support Single Pass.");

			public static readonly GUIContent singlePassInstancedWarning = EditorGUIUtility.TextContent("Single Pass Instanced is only supported on Windows. Multi Pass will be used on other platforms.");

			public static readonly GUIContent[] kDefaultStereoRenderingPaths = new GUIContent[]
			{
				new GUIContent("Multi Pass"),
				new GUIContent("Single Pass"),
				new GUIContent("Single Pass Instanced (Preview)")
			};

			public static readonly GUIContent[] kAndroidStereoRenderingPaths = new GUIContent[]
			{
				new GUIContent("Multi Pass"),
				new GUIContent("Single Pass (Preview)")
			};

			public static readonly GUIContent xrSettingsTitle = EditorGUIUtility.TextContent("XR Settings");

			public static readonly GUIContent supportedCheckbox = EditorGUIUtility.TextContent("Virtual Reality Supported");

			public static readonly GUIContent listHeader = EditorGUIUtility.TextContent("Virtual Reality SDKs");
		}

		private PlayerSettingsEditor m_Settings;

		private Dictionary<BuildTargetGroup, VRDeviceInfoEditor[]> m_AllVRDevicesForBuildTarget = new Dictionary<BuildTargetGroup, VRDeviceInfoEditor[]>();

		private Dictionary<BuildTargetGroup, ReorderableList> m_VRDeviceActiveUI = new Dictionary<BuildTargetGroup, ReorderableList>();

		private Dictionary<string, string> m_MapVRDeviceKeyToUIString = new Dictionary<string, string>();

		private Dictionary<string, string> m_MapVRUIStringToDeviceKey = new Dictionary<string, string>();

		private Dictionary<string, VRCustomOptions> m_CustomOptions = new Dictionary<string, VRCustomOptions>();

		private SerializedProperty m_StereoRenderingPath;

		private SerializedProperty m_AndroidEnableTango;

		private bool m_InstallsRequired = false;

		private bool m_VuforiaInstalled = false;

		internal int GUISectionIndex
		{
			get;
			set;
		}

		public PlayerSettingsEditorVR(PlayerSettingsEditor settingsEditor)
		{
			this.m_Settings = settingsEditor;
			this.m_StereoRenderingPath = this.m_Settings.serializedObject.FindProperty("m_StereoRenderingPath");
			this.m_AndroidEnableTango = this.m_Settings.FindPropertyAssert("AndroidEnableTango");
		}

		private void RefreshVRDeviceList(BuildTargetGroup targetGroup)
		{
			VRDeviceInfoEditor[] array = VREditor.GetAllVRDeviceInfo(targetGroup);
			array = (from d in array
			orderby d.deviceNameUI
			select d).ToArray<VRDeviceInfoEditor>();
			this.m_AllVRDevicesForBuildTarget[targetGroup] = array;
			for (int i = 0; i < array.Length; i++)
			{
				VRDeviceInfoEditor vRDeviceInfoEditor = array[i];
				this.m_MapVRDeviceKeyToUIString[vRDeviceInfoEditor.deviceNameKey] = vRDeviceInfoEditor.deviceNameUI;
				this.m_MapVRUIStringToDeviceKey[vRDeviceInfoEditor.deviceNameUI] = vRDeviceInfoEditor.deviceNameKey;
				VRCustomOptions vRCustomOptions;
				if (!this.m_CustomOptions.TryGetValue(vRDeviceInfoEditor.deviceNameKey, out vRCustomOptions))
				{
					Type type = Type.GetType("UnityEditorInternal.VR.VRCustomOptions" + vRDeviceInfoEditor.deviceNameKey, false, true);
					if (type != null)
					{
						vRCustomOptions = (VRCustomOptions)Activator.CreateInstance(type);
					}
					else
					{
						vRCustomOptions = new VRCustomOptionsNone();
					}
					vRCustomOptions.Initialize(this.m_Settings.serializedObject);
					this.m_CustomOptions.Add(vRDeviceInfoEditor.deviceNameKey, vRCustomOptions);
				}
			}
		}

		internal bool TargetGroupSupportsVirtualReality(BuildTargetGroup targetGroup)
		{
			if (!this.m_AllVRDevicesForBuildTarget.ContainsKey(targetGroup))
			{
				this.RefreshVRDeviceList(targetGroup);
			}
			VRDeviceInfoEditor[] array = this.m_AllVRDevicesForBuildTarget[targetGroup];
			return array.Length > 0;
		}

		internal bool TargetGroupSupportsAugmentedReality(BuildTargetGroup targetGroup)
		{
			return this.TargetGroupSupportsTango(targetGroup) || this.TargetGroupSupportsVuforia(targetGroup);
		}

		internal void XRSectionGUI(BuildTargetGroup targetGroup, int sectionIndex)
		{
			this.GUISectionIndex = sectionIndex;
			if (this.TargetGroupSupportsVirtualReality(targetGroup) || this.TargetGroupSupportsAugmentedReality(targetGroup))
			{
				if (VREditor.IsDeviceListDirty(targetGroup))
				{
					VREditor.ClearDeviceListDirty(targetGroup);
					this.m_VRDeviceActiveUI[targetGroup].list = VREditor.GetVREnabledDevicesOnTargetGroup(targetGroup);
				}
				this.CheckDevicesRequireInstall(targetGroup);
				if (this.m_Settings.BeginSettingsBox(sectionIndex, PlayerSettingsEditorVR.Styles.xrSettingsTitle))
				{
					if (EditorApplication.isPlaying)
					{
						EditorGUILayout.HelpBox("Changing XRSettings in not allowed in play mode.", MessageType.Info);
					}
					using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
					{
						this.DevicesGUI(targetGroup);
						this.ErrorOnVRDeviceIncompatibility(targetGroup);
						this.SinglePassStereoGUI(targetGroup, this.m_StereoRenderingPath);
						this.TangoGUI(targetGroup);
						this.VuforiaGUI(targetGroup);
						this.ErrorOnARDeviceIncompatibility(targetGroup);
					}
					this.InstallGUI(targetGroup);
				}
				this.m_Settings.EndSettingsBox();
			}
		}

		private void DevicesGUI(BuildTargetGroup targetGroup)
		{
			if (this.TargetGroupSupportsVirtualReality(targetGroup))
			{
				bool flag = VREditor.GetVREnabledOnTargetGroup(targetGroup);
				EditorGUI.BeginChangeCheck();
				flag = EditorGUILayout.Toggle(PlayerSettingsEditorVR.Styles.supportedCheckbox, flag, new GUILayoutOption[0]);
				if (EditorGUI.EndChangeCheck())
				{
					VREditor.SetVREnabledOnTargetGroup(targetGroup, flag);
				}
				if (flag)
				{
					this.VRDevicesGUIOneBuildTarget(targetGroup);
				}
			}
		}

		private void InstallGUI(BuildTargetGroup targetGroup)
		{
			if (this.m_InstallsRequired)
			{
				EditorGUILayout.Space();
				GUILayout.Label("XR Support Installers", EditorStyles.boldLabel, new GUILayoutOption[0]);
				EditorGUI.indentLevel++;
				if (!this.m_VuforiaInstalled)
				{
					if (EditorGUILayout.LinkLabel("Vuforia Augmented Reality", new GUILayoutOption[0]))
					{
						string playbackEngineDownloadURL = BuildPlayerWindow.GetPlaybackEngineDownloadURL("Vuforia-AR");
						Application.OpenURL(playbackEngineDownloadURL);
					}
				}
				EditorGUI.indentLevel--;
				EditorGUILayout.Space();
			}
		}

		private void CheckDevicesRequireInstall(BuildTargetGroup targetGroup)
		{
			this.m_InstallsRequired = false;
			if (!this.m_VuforiaInstalled)
			{
				VRDeviceInfoEditor[] allVRDeviceInfo = VREditor.GetAllVRDeviceInfo(targetGroup);
				for (int i = 0; i < allVRDeviceInfo.Length; i++)
				{
					if (allVRDeviceInfo[i].deviceNameKey.ToLower() == "vuforia")
					{
						this.m_VuforiaInstalled = true;
						break;
					}
				}
				if (!this.m_VuforiaInstalled)
				{
					this.m_InstallsRequired = true;
				}
			}
		}

		private static bool TargetSupportsSinglePassStereoRendering(BuildTargetGroup targetGroup)
		{
			return targetGroup == BuildTargetGroup.Standalone || targetGroup == BuildTargetGroup.Android || targetGroup == BuildTargetGroup.WSA || targetGroup == BuildTargetGroup.PS4;
		}

		private static bool TargetSupportsStereoInstancingRendering(BuildTargetGroup targetGroup)
		{
			return targetGroup == BuildTargetGroup.Standalone || targetGroup == BuildTargetGroup.WSA || targetGroup == BuildTargetGroup.PS4;
		}

		private static GUIContent[] GetStereoRenderingPaths(BuildTargetGroup targetGroup)
		{
			return (targetGroup != BuildTargetGroup.Android) ? PlayerSettingsEditorVR.Styles.kDefaultStereoRenderingPaths : PlayerSettingsEditorVR.Styles.kAndroidStereoRenderingPaths;
		}

		private void SinglePassStereoGUI(BuildTargetGroup targetGroup, SerializedProperty stereoRenderingPath)
		{
			if (PlayerSettings.virtualRealitySupported)
			{
				bool flag = PlayerSettingsEditorVR.TargetSupportsSinglePassStereoRendering(targetGroup);
				bool flag2 = PlayerSettingsEditorVR.TargetSupportsStereoInstancingRendering(targetGroup);
				int num = 1 + ((!flag) ? 0 : 1) + ((!flag2) ? 0 : 1);
				GUIContent[] array = new GUIContent[num];
				int[] array2 = new int[num];
				GUIContent[] stereoRenderingPaths = PlayerSettingsEditorVR.GetStereoRenderingPaths(targetGroup);
				int num2 = 0;
				array[num2] = stereoRenderingPaths[0];
				array2[num2++] = 0;
				if (flag)
				{
					array[num2] = stereoRenderingPaths[1];
					array2[num2++] = 1;
				}
				if (flag2)
				{
					array[num2] = stereoRenderingPaths[2];
					array2[num2++] = 2;
				}
				if (!flag2 && stereoRenderingPath.intValue == 2)
				{
					stereoRenderingPath.intValue = 1;
				}
				if (!flag && stereoRenderingPath.intValue == 1)
				{
					stereoRenderingPath.intValue = 0;
				}
				EditorGUILayout.IntPopup(stereoRenderingPath, array, array2, EditorGUIUtility.TextContent("Stereo Rendering Method*"), new GUILayoutOption[0]);
				if (stereoRenderingPath.intValue == 1 && targetGroup == BuildTargetGroup.Android)
				{
					GraphicsDeviceType[] graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
					if (graphicsAPIs.Length > 0 && graphicsAPIs[0] == GraphicsDeviceType.OpenGLES3)
					{
						EditorGUILayout.HelpBox(PlayerSettingsEditorVR.Styles.singlepassAndroidWarning2.text, MessageType.Info);
					}
					else
					{
						EditorGUILayout.HelpBox(PlayerSettingsEditorVR.Styles.singlepassAndroidWarning.text, MessageType.Warning);
					}
				}
				else if (stereoRenderingPath.intValue == 2 && targetGroup == BuildTargetGroup.Standalone)
				{
					EditorGUILayout.HelpBox(PlayerSettingsEditorVR.Styles.singlePassInstancedWarning.text, MessageType.Warning);
				}
			}
		}

		private void AddVRDeviceMenuSelected(object userData, string[] options, int selected)
		{
			BuildTargetGroup buildTargetGroup = (BuildTargetGroup)userData;
			List<string> list = VREditor.GetVREnabledDevicesOnTargetGroup(buildTargetGroup).ToList<string>();
			string item;
			if (!this.m_MapVRUIStringToDeviceKey.TryGetValue(options[selected], out item))
			{
				item = options[selected];
			}
			list.Add(item);
			this.ApplyChangedVRDeviceList(buildTargetGroup, list.ToArray());
		}

		private void AddVRDeviceElement(BuildTargetGroup target, Rect rect, ReorderableList list)
		{
			VRDeviceInfoEditor[] source = this.m_AllVRDevicesForBuildTarget[target];
			List<string> enabledDevices = VREditor.GetVREnabledDevicesOnTargetGroup(target).ToList<string>();
			string[] options = (from d in source
			select d.deviceNameUI).ToArray<string>();
			bool[] enabled = (from d in source
			select !enabledDevices.Any((string enabledDeviceName) => d.deviceNameKey == enabledDeviceName)).ToArray<bool>();
			EditorUtility.DisplayCustomMenu(rect, options, enabled, null, new EditorUtility.SelectMenuItemFunction(this.AddVRDeviceMenuSelected), target);
		}

		private void RemoveVRDeviceElement(BuildTargetGroup target, ReorderableList list)
		{
			List<string> list2 = VREditor.GetVREnabledDevicesOnTargetGroup(target).ToList<string>();
			list2.RemoveAt(list.index);
			this.ApplyChangedVRDeviceList(target, list2.ToArray());
		}

		private void ReorderVRDeviceElement(BuildTargetGroup target, ReorderableList list)
		{
			string[] devices = list.list.Cast<string>().ToArray<string>();
			this.ApplyChangedVRDeviceList(target, devices);
		}

		private void ApplyChangedVRDeviceList(BuildTargetGroup target, string[] devices)
		{
			if (this.m_VRDeviceActiveUI.ContainsKey(target))
			{
				if (target == BuildTargetGroup.iPhone)
				{
					if (devices.Contains("cardboard") && PlayerSettings.iOS.cameraUsageDescription == "")
					{
						PlayerSettings.iOS.cameraUsageDescription = "Used to scan QR codes";
					}
				}
				VREditor.SetVREnabledDevicesOnTargetGroup(target, devices);
				this.m_VRDeviceActiveUI[target].list = devices;
			}
		}

		private void DrawVRDeviceElement(BuildTargetGroup target, Rect rect, int index, bool selected, bool focused)
		{
			string text = (string)this.m_VRDeviceActiveUI[target].list[index];
			string text2;
			if (!this.m_MapVRDeviceKeyToUIString.TryGetValue(text, out text2))
			{
				text2 = text + " (missing from build)";
			}
			VRCustomOptions vRCustomOptions;
			if (this.m_CustomOptions.TryGetValue(text, out vRCustomOptions))
			{
				if (!(vRCustomOptions is VRCustomOptionsNone))
				{
					Rect position = new Rect(rect);
					position.width = (float)EditorStyles.foldout.border.left;
					position.height = (float)EditorStyles.foldout.border.top;
					bool hierarchyMode = EditorGUIUtility.hierarchyMode;
					EditorGUIUtility.hierarchyMode = false;
					vRCustomOptions.IsExpanded = EditorGUI.Foldout(position, vRCustomOptions.IsExpanded, "", false, EditorStyles.foldout);
					EditorGUIUtility.hierarchyMode = hierarchyMode;
				}
			}
			rect.xMin += (float)EditorStyles.foldout.border.left;
			GUI.Label(rect, text2, EditorStyles.label);
			rect.y += EditorGUIUtility.singleLineHeight + 2f;
			if (vRCustomOptions != null && vRCustomOptions.IsExpanded)
			{
				vRCustomOptions.Draw(rect);
			}
		}

		private float GetVRDeviceElementHeight(BuildTargetGroup target, int index)
		{
			ReorderableList reorderableList = this.m_VRDeviceActiveUI[target];
			string key = (string)reorderableList.list[index];
			float num = 0f;
			VRCustomOptions vRCustomOptions;
			if (this.m_CustomOptions.TryGetValue(key, out vRCustomOptions))
			{
				num = ((!vRCustomOptions.IsExpanded) ? 0f : (vRCustomOptions.GetHeight() + 2f));
			}
			return reorderableList.elementHeight + num;
		}

		private void SelectVRDeviceElement(BuildTargetGroup target, ReorderableList list)
		{
			string key = (string)this.m_VRDeviceActiveUI[target].list[list.index];
			VRCustomOptions vRCustomOptions;
			if (this.m_CustomOptions.TryGetValue(key, out vRCustomOptions))
			{
				vRCustomOptions.IsExpanded = false;
			}
		}

		private bool GetVRDeviceElementIsInList(BuildTargetGroup target, string deviceName)
		{
			string[] vREnabledDevicesOnTargetGroup = VREditor.GetVREnabledDevicesOnTargetGroup(target);
			return vREnabledDevicesOnTargetGroup.Contains(deviceName);
		}

		private void VRDevicesGUIOneBuildTarget(BuildTargetGroup targetGroup)
		{
			if (!this.m_VRDeviceActiveUI.ContainsKey(targetGroup))
			{
				ReorderableList reorderableList = new ReorderableList(VREditor.GetVREnabledDevicesOnTargetGroup(targetGroup), typeof(VRDeviceInfoEditor), true, true, true, true);
				reorderableList.onAddDropdownCallback = delegate(Rect rect, ReorderableList list)
				{
					this.AddVRDeviceElement(targetGroup, rect, list);
				};
				reorderableList.onRemoveCallback = delegate(ReorderableList list)
				{
					this.RemoveVRDeviceElement(targetGroup, list);
				};
				reorderableList.onReorderCallback = delegate(ReorderableList list)
				{
					this.ReorderVRDeviceElement(targetGroup, list);
				};
				reorderableList.drawElementCallback = delegate(Rect rect, int index, bool isActive, bool isFocused)
				{
					this.DrawVRDeviceElement(targetGroup, rect, index, isActive, isFocused);
				};
				reorderableList.drawHeaderCallback = delegate(Rect rect)
				{
					GUI.Label(rect, PlayerSettingsEditorVR.Styles.listHeader, EditorStyles.label);
				};
				reorderableList.elementHeightCallback = ((int index) => this.GetVRDeviceElementHeight(targetGroup, index));
				reorderableList.onSelectCallback = delegate(ReorderableList list)
				{
					this.SelectVRDeviceElement(targetGroup, list);
				};
				this.m_VRDeviceActiveUI.Add(targetGroup, reorderableList);
			}
			this.m_VRDeviceActiveUI[targetGroup].DoLayoutList();
			if (this.m_VRDeviceActiveUI[targetGroup].list.Count == 0)
			{
				EditorGUILayout.HelpBox("Must add at least one Virtual Reality SDK.", MessageType.Warning);
			}
		}

		private void ErrorOnVRDeviceIncompatibility(BuildTargetGroup targetGroup)
		{
			if (PlayerSettings.GetVirtualRealitySupported(targetGroup))
			{
				if (targetGroup == BuildTargetGroup.Android)
				{
					List<string> list = VREditor.GetVREnabledDevicesOnTargetGroup(targetGroup).ToList<string>();
					if (list.Contains("Oculus") && list.Contains("daydream"))
					{
						EditorGUILayout.HelpBox("To avoid initialization conflicts on devices which support both Daydream and Oculus based VR, build separate APKs with different package names, targeting only the Daydream or Oculus VR SDK in the respective APK.", MessageType.Warning);
					}
				}
			}
		}

		private void ErrorOnARDeviceIncompatibility(BuildTargetGroup targetGroup)
		{
			if (targetGroup == BuildTargetGroup.Android)
			{
				if (PlayerSettings.Android.androidTangoEnabled && PlayerSettings.GetPlatformVuforiaEnabled(targetGroup))
				{
					EditorGUILayout.HelpBox("Both ARCore and Vuforia XR Device support cannot be selected at the same time. Please select only one XR Device that will manage the Android device.", MessageType.Error);
				}
			}
		}

		internal bool TargetGroupSupportsTango(BuildTargetGroup targetGroup)
		{
			return targetGroup == BuildTargetGroup.Android;
		}

		internal bool TargetGroupSupportsVuforia(BuildTargetGroup targetGroup)
		{
			return targetGroup == BuildTargetGroup.Standalone || targetGroup == BuildTargetGroup.Android || targetGroup == BuildTargetGroup.iPhone || targetGroup == BuildTargetGroup.WSA;
		}

		internal void TangoGUI(BuildTargetGroup targetGroup)
		{
			if (this.TargetGroupSupportsTango(targetGroup))
			{
				EditorGUILayout.PropertyField(this.m_AndroidEnableTango, EditorGUIUtility.TextContent("ARCore Supported"), new GUILayoutOption[0]);
				if (PlayerSettings.Android.androidTangoEnabled)
				{
					EditorGUI.indentLevel++;
					if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
					{
						GUIContent gUIContent = EditorGUIUtility.TextContent("ARCore requires 'Minimum API Level' to be at least Android 7.0");
						EditorGUILayout.HelpBox(gUIContent.text, MessageType.Warning);
					}
					EditorGUI.indentLevel--;
				}
			}
		}

		internal void VuforiaGUI(BuildTargetGroup targetGroup)
		{
			if (this.TargetGroupSupportsVuforia(targetGroup) && this.m_VuforiaInstalled)
			{
				bool flag = VREditor.GetVREnabledOnTargetGroup(targetGroup) && this.GetVRDeviceElementIsInList(targetGroup, "Vuforia");
				using (new EditorGUI.DisabledScope(flag))
				{
					if (flag && !PlayerSettings.GetPlatformVuforiaEnabled(targetGroup))
					{
						PlayerSettings.SetPlatformVuforiaEnabled(targetGroup, true);
					}
					bool flag2 = PlayerSettings.GetPlatformVuforiaEnabled(targetGroup);
					EditorGUI.BeginChangeCheck();
					flag2 = EditorGUILayout.Toggle(EditorGUIUtility.TextContent("Vuforia Augmented Reality Supported"), flag2, new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck())
					{
						PlayerSettings.SetPlatformVuforiaEnabled(targetGroup, flag2);
					}
				}
				if (flag)
				{
					EditorGUILayout.HelpBox("Vuforia Augmented Reality is required when using the Vuforia Virtual Reality SDK.", MessageType.Info);
				}
			}
		}
	}
}
