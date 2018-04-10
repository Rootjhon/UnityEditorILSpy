using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal class EmissionModuleUI : ModuleUI
	{
		private class Texts
		{
			public GUIContent rateOverTime = EditorGUIUtility.TextContent("Rate over Time|The number of particles emitted per second.");

			public GUIContent rateOverDistance = EditorGUIUtility.TextContent("Rate over Distance|The number of particles emitted per distance unit.");

			public GUIContent burst = EditorGUIUtility.TextContent("Bursts|Emission of extra particles at specific times during the duration of the system.");

			public GUIContent burstTime = EditorGUIUtility.TextContent("Time|When the burst will trigger.");

			public GUIContent burstCount = EditorGUIUtility.TextContent("Count|The number of particles to emit.");

			public GUIContent burstCycleCount = EditorGUIUtility.TextContent("Cycles|How many times to emit the burst. Use the dropdown to repeat infinitely.");

			public GUIContent burstCycleCountInfinite = EditorGUIUtility.TextContent("Infinite");

			public GUIContent burstRepeatInterval = EditorGUIUtility.TextContent("Interval|Repeat the burst every N seconds.");
		}

		private class ModeCallbackData
		{
			public SerializedProperty modeProp;

			public int selectedState;

			public ModeCallbackData(int i, SerializedProperty p)
			{
				this.modeProp = p;
				this.selectedState = i;
			}
		}

		public SerializedMinMaxCurve m_Time;

		public SerializedMinMaxCurve m_Distance;

		private const int k_MaxNumBursts = 8;

		private const float k_BurstDragWidth = 15f;

		private SerializedProperty m_BurstCount;

		private SerializedProperty m_Bursts;

		private List<SerializedMinMaxCurve> m_BurstCountCurves = new List<SerializedMinMaxCurve>();

		private ReorderableList m_BurstList;

		private static EmissionModuleUI.Texts s_Texts;

		[CompilerGenerated]
		private static GenericMenu.MenuFunction2 <>f__mg$cache0;

		public EmissionModuleUI(ParticleSystemUI owner, SerializedObject o, string displayName) : base(owner, o, "EmissionModule", displayName)
		{
			this.m_ToolTip = "Emission of the emitter. This controls the rate at which particles are emitted as well as burst emissions.";
		}

		protected override void Init()
		{
			if (this.m_BurstCount == null)
			{
				if (EmissionModuleUI.s_Texts == null)
				{
					EmissionModuleUI.s_Texts = new EmissionModuleUI.Texts();
				}
				this.m_Time = new SerializedMinMaxCurve(this, EmissionModuleUI.s_Texts.rateOverTime, "rateOverTime");
				this.m_Distance = new SerializedMinMaxCurve(this, EmissionModuleUI.s_Texts.rateOverDistance, "rateOverDistance");
				this.m_BurstCount = base.GetProperty("m_BurstCount");
				this.m_Bursts = base.GetProperty("m_Bursts");
				this.m_BurstList = new ReorderableList(base.serializedObject, this.m_Bursts, false, true, true, true);
				this.m_BurstList.elementHeight = 16f;
				this.m_BurstList.onAddCallback = new ReorderableList.AddCallbackDelegate(this.OnBurstListAddCallback);
				this.m_BurstList.onCanAddCallback = new ReorderableList.CanAddCallbackDelegate(this.OnBurstListCanAddCallback);
				this.m_BurstList.onRemoveCallback = new ReorderableList.RemoveCallbackDelegate(this.OnBurstListRemoveCallback);
				this.m_BurstList.drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(this.DrawBurstListHeaderCallback);
				this.m_BurstList.drawElementCallback = new ReorderableList.ElementCallbackDelegate(this.DrawBurstListElementCallback);
			}
		}

		public override void OnInspectorGUI(InitialModuleUI initial)
		{
			ModuleUI.GUIMinMaxCurve(EmissionModuleUI.s_Texts.rateOverTime, this.m_Time, new GUILayoutOption[0]);
			ModuleUI.GUIMinMaxCurve(EmissionModuleUI.s_Texts.rateOverDistance, this.m_Distance, new GUILayoutOption[0]);
			this.DoBurstGUI(initial);
		}

		private void DoBurstGUI(InitialModuleUI initial)
		{
			while (this.m_BurstList.count > this.m_BurstCountCurves.Count)
			{
				SerializedProperty arrayElementAtIndex = this.m_Bursts.GetArrayElementAtIndex(this.m_BurstCountCurves.Count);
				this.m_BurstCountCurves.Add(new SerializedMinMaxCurve(this, EmissionModuleUI.s_Texts.burstCount, arrayElementAtIndex.propertyPath + ".countCurve", false, true));
			}
			EditorGUILayout.Space();
			Rect controlRect = ModuleUI.GetControlRect(13, new GUILayoutOption[0]);
			GUI.Label(controlRect, EmissionModuleUI.s_Texts.burst, ParticleSystemStyles.Get().label);
			this.m_BurstList.displayAdd = (this.m_Bursts.arraySize < 8);
			this.m_BurstList.DoLayoutList();
		}

		private void OnBurstListAddCallback(ReorderableList list)
		{
			ReorderableList.defaultBehaviours.DoAddButton(list);
			this.m_BurstCount.intValue++;
			SerializedProperty arrayElementAtIndex = this.m_Bursts.GetArrayElementAtIndex(list.index);
			SerializedProperty serializedProperty = arrayElementAtIndex.FindPropertyRelative("countCurve.minMaxState");
			SerializedProperty serializedProperty2 = arrayElementAtIndex.FindPropertyRelative("countCurve.scalar");
			SerializedProperty serializedProperty3 = arrayElementAtIndex.FindPropertyRelative("cycleCount");
			serializedProperty.intValue = 0;
			serializedProperty2.floatValue = 30f;
			serializedProperty3.intValue = 1;
			SerializedProperty serializedProperty4 = arrayElementAtIndex.FindPropertyRelative("countCurve.minCurve");
			SerializedProperty serializedProperty5 = arrayElementAtIndex.FindPropertyRelative("countCurve.maxCurve");
			serializedProperty4.animationCurveValue = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			serializedProperty5.animationCurveValue = AnimationCurve.Linear(0f, 1f, 1f, 1f);
			this.m_BurstCountCurves.Add(new SerializedMinMaxCurve(this, EmissionModuleUI.s_Texts.burstCount, arrayElementAtIndex.propertyPath + ".countCurve", false, true));
		}

		private bool OnBurstListCanAddCallback(ReorderableList list)
		{
			return !this.m_ParticleSystemUI.multiEdit;
		}

		private void OnBurstListRemoveCallback(ReorderableList list)
		{
			for (int i = list.index; i < this.m_BurstCountCurves.Count; i++)
			{
				this.m_BurstCountCurves[i].RemoveCurveFromEditor();
			}
			this.m_BurstCountCurves.RemoveRange(list.index, this.m_BurstCountCurves.Count - list.index);
			AnimationCurvePreviewCache.ClearCache();
			ReorderableList.defaultBehaviours.DoRemoveButton(list);
			this.m_BurstCount.intValue--;
		}

		private void DrawBurstListHeaderCallback(Rect rect)
		{
			rect.width -= 15f;
			rect.width /= 4f;
			rect.x += 15f;
			EditorGUI.LabelField(rect, EmissionModuleUI.s_Texts.burstTime, ParticleSystemStyles.Get().label);
			rect.x += rect.width;
			EditorGUI.LabelField(rect, EmissionModuleUI.s_Texts.burstCount, ParticleSystemStyles.Get().label);
			rect.x += rect.width;
			EditorGUI.LabelField(rect, EmissionModuleUI.s_Texts.burstCycleCount, ParticleSystemStyles.Get().label);
			rect.x += rect.width;
			EditorGUI.LabelField(rect, EmissionModuleUI.s_Texts.burstRepeatInterval, ParticleSystemStyles.Get().label);
			rect.x += rect.width;
		}

		private void DrawBurstListElementCallback(Rect rect, int index, bool isActive, bool isFocused)
		{
			SerializedProperty arrayElementAtIndex = this.m_Bursts.GetArrayElementAtIndex(index);
			SerializedProperty floatProp = arrayElementAtIndex.FindPropertyRelative("time");
			SerializedProperty serializedProperty = arrayElementAtIndex.FindPropertyRelative("cycleCount");
			SerializedProperty floatProp2 = arrayElementAtIndex.FindPropertyRelative("repeatInterval");
			rect.width -= 45f;
			rect.width /= 4f;
			ModuleUI.FloatDraggable(rect, floatProp, 1f, 15f, "n3");
			rect.x += rect.width;
			rect = ModuleUI.GUIMinMaxCurveInline(rect, this.m_BurstCountCurves[index], 15f);
			rect.x += rect.width;
			rect.width -= 13f;
			if (!serializedProperty.hasMultipleDifferentValues && serializedProperty.intValue == 0)
			{
				rect.x += 15f;
				rect.width -= 15f;
				EditorGUI.LabelField(rect, EmissionModuleUI.s_Texts.burstCycleCountInfinite, ParticleSystemStyles.Get().label);
			}
			else
			{
				ModuleUI.IntDraggable(rect, null, serializedProperty, 15f);
			}
			rect.width += 13f;
			Rect popupRect = ModuleUI.GetPopupRect(rect);
			EmissionModuleUI.GUIMMModePopUp(popupRect, serializedProperty);
			rect.x += rect.width;
			ModuleUI.FloatDraggable(rect, floatProp2, 1f, 15f, "n3");
			rect.x += rect.width;
		}

		private static void SelectModeCallback(object obj)
		{
			EmissionModuleUI.ModeCallbackData modeCallbackData = (EmissionModuleUI.ModeCallbackData)obj;
			modeCallbackData.modeProp.intValue = modeCallbackData.selectedState;
		}

		private static void GUIMMModePopUp(Rect rect, SerializedProperty modeProp)
		{
			if (EditorGUI.DropdownButton(rect, GUIContent.none, FocusType.Passive, ParticleSystemStyles.Get().minMaxCurveStateDropDown))
			{
				GUIContent[] array = new GUIContent[]
				{
					new GUIContent("Infinite"),
					new GUIContent("Count")
				};
				GenericMenu genericMenu = new GenericMenu();
				for (int i = 0; i < array.Length; i++)
				{
					GenericMenu arg_7D_0 = genericMenu;
					GUIContent arg_7D_1 = array[i];
					bool arg_7D_2 = modeProp.intValue == i;
					if (EmissionModuleUI.<>f__mg$cache0 == null)
					{
						EmissionModuleUI.<>f__mg$cache0 = new GenericMenu.MenuFunction2(EmissionModuleUI.SelectModeCallback);
					}
					arg_7D_0.AddItem(arg_7D_1, arg_7D_2, EmissionModuleUI.<>f__mg$cache0, new EmissionModuleUI.ModeCallbackData(i, modeProp));
				}
				genericMenu.ShowAsContext();
				Event.current.Use();
			}
		}

		public override void UpdateCullingSupportedString(ref string text)
		{
			this.Init();
			if (this.m_Distance.scalar.hasMultipleDifferentValues || this.m_Distance.scalar.floatValue > 0f)
			{
				text += "\nDistance-based emission is being used in the Emission module.";
			}
		}
	}
}
