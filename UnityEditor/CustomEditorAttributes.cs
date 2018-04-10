using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEditor
{
	internal class CustomEditorAttributes
	{
		private class MonoEditorType
		{
			public Type m_InspectedType;

			public Type m_InspectorType;

			public Type m_RenderPipelineType;

			public bool m_EditorForChildClasses;

			public bool m_IsFallback;
		}

		private static readonly List<CustomEditorAttributes.MonoEditorType> kSCustomEditors = new List<CustomEditorAttributes.MonoEditorType>();

		private static readonly List<CustomEditorAttributes.MonoEditorType> kSCustomMultiEditors = new List<CustomEditorAttributes.MonoEditorType>();

		private static bool s_Initialized;

		internal static Type FindCustomEditorType(UnityEngine.Object o, bool multiEdit)
		{
			return CustomEditorAttributes.FindCustomEditorTypeByType(o.GetType(), multiEdit);
		}

		internal static Type FindCustomEditorTypeByType(Type type, bool multiEdit)
		{
			if (!CustomEditorAttributes.s_Initialized)
			{
				Assembly[] loadedAssemblies = EditorAssemblies.loadedAssemblies;
				for (int i = loadedAssemblies.Length - 1; i >= 0; i--)
				{
					CustomEditorAttributes.Rebuild(loadedAssemblies[i]);
				}
				CustomEditorAttributes.s_Initialized = true;
			}
			Type result;
			if (type == null)
			{
				result = null;
			}
			else
			{
				List<CustomEditorAttributes.MonoEditorType> source = (!multiEdit) ? CustomEditorAttributes.kSCustomEditors : CustomEditorAttributes.kSCustomMultiEditors;
				for (int j = 0; j < 2; j++)
				{
					for (Type type2 = type; type2 != null; type2 = type2.BaseType)
					{
						Type inspected1 = type2;
						int pass1 = j;
						IEnumerable<CustomEditorAttributes.MonoEditorType> enumerable = from x in source
						where CustomEditorAttributes.IsAppropriateEditor(x, inspected1, type != inspected1, pass1 == 1)
						select x;
						if (GraphicsSettings.renderPipelineAsset != null)
						{
							Type type3 = GraphicsSettings.renderPipelineAsset.GetType();
							foreach (CustomEditorAttributes.MonoEditorType current in enumerable)
							{
								if (current.m_RenderPipelineType == type3)
								{
									result = current.m_InspectorType;
									return result;
								}
							}
						}
						CustomEditorAttributes.MonoEditorType monoEditorType = enumerable.FirstOrDefault((CustomEditorAttributes.MonoEditorType x) => x.m_RenderPipelineType == null);
						if (monoEditorType != null)
						{
							result = monoEditorType.m_InspectorType;
							return result;
						}
					}
				}
				result = null;
			}
			return result;
		}

		private static bool IsAppropriateEditor(CustomEditorAttributes.MonoEditorType editor, Type parentClass, bool isChildClass, bool isFallback)
		{
			return (!isChildClass || editor.m_EditorForChildClasses) && isFallback == editor.m_IsFallback && (parentClass == editor.m_InspectedType || (parentClass.IsGenericType && parentClass.GetGenericTypeDefinition() == editor.m_InspectedType));
		}

		internal static void Rebuild(Assembly assembly)
		{
			Type[] typesFromAssembly = AssemblyHelper.GetTypesFromAssembly(assembly);
			Type[] array = typesFromAssembly;
			for (int i = 0; i < array.Length; i++)
			{
				Type type = array[i];
				object[] customAttributes = type.GetCustomAttributes(typeof(CustomEditor), false);
				object[] array2 = customAttributes;
				int j = 0;
				while (j < array2.Length)
				{
					CustomEditor customEditor = (CustomEditor)array2[j];
					CustomEditorAttributes.MonoEditorType monoEditorType = new CustomEditorAttributes.MonoEditorType();
					if (customEditor.m_InspectedType == null)
					{
						Debug.Log("Can't load custom inspector " + type.Name + " because the inspected type is null.");
					}
					else if (!type.IsSubclassOf(typeof(Editor)))
					{
						if (!(type.FullName == "TweakMode") || !type.IsEnum || !(customEditor.m_InspectedType.FullName == "BloomAndFlares"))
						{
							Debug.LogWarning(type.Name + " uses the CustomEditor attribute but does not inherit from Editor.\nYou must inherit from Editor. See the Editor class script documentation.");
						}
					}
					else
					{
						monoEditorType.m_InspectedType = customEditor.m_InspectedType;
						monoEditorType.m_InspectorType = type;
						monoEditorType.m_EditorForChildClasses = customEditor.m_EditorForChildClasses;
						monoEditorType.m_IsFallback = customEditor.isFallback;
						CustomEditorForRenderPipelineAttribute customEditorForRenderPipelineAttribute = customEditor as CustomEditorForRenderPipelineAttribute;
						if (customEditorForRenderPipelineAttribute != null)
						{
							monoEditorType.m_RenderPipelineType = customEditorForRenderPipelineAttribute.renderPipelineType;
						}
						CustomEditorAttributes.kSCustomEditors.Add(monoEditorType);
						if (type.GetCustomAttributes(typeof(CanEditMultipleObjects), false).Length > 0)
						{
							CustomEditorAttributes.kSCustomMultiEditors.Add(monoEditorType);
						}
					}
					IL_16B:
					j++;
					continue;
					goto IL_16B;
				}
			}
		}
	}
}
