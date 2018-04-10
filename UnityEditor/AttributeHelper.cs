using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnityEditor
{
	internal class AttributeHelper
	{
		private struct MonoGizmoMethod
		{
			public MethodInfo drawGizmo;

			public Type drawnType;

			public int options;
		}

		private struct MonoCreateAssetItem
		{
			public string menuItem;

			public string fileName;

			public int order;

			public Type type;
		}

		internal class MethodWithAttribute
		{
			public MethodInfo info;

			public Attribute attribute;
		}

		internal class MethodInfoSorter
		{
			public List<AttributeHelper.MethodWithAttribute> MethodsWithAttributes
			{
				[CompilerGenerated]
				get
				{
					return this.<MethodsWithAttributes>k__BackingField;
				}
			}

			internal MethodInfoSorter(List<AttributeHelper.MethodWithAttribute> methodsWithAttributes)
			{
				this.<MethodsWithAttributes>k__BackingField = methodsWithAttributes;
			}

			public IEnumerable<MethodInfo> FilterAndSortOnAttribute<T>(Func<T, bool> filter, Func<T, IComparable> sorter) where T : Attribute
			{
				return from a in this.MethodsWithAttributes
				where filter((T)((object)a.attribute))
				select a into c
				orderby sorter((T)((object)c.attribute))
				select c into o
				select o.info;
			}
		}

		private static Dictionary<Type, AttributeHelper.MethodInfoSorter> m_Cache = new Dictionary<Type, AttributeHelper.MethodInfoSorter>();

		[RequiredByNativeCode]
		private static AttributeHelper.MonoGizmoMethod[] ExtractGizmos(Assembly assembly)
		{
			List<AttributeHelper.MonoGizmoMethod> list = new List<AttributeHelper.MonoGizmoMethod>();
			Type[] typesFromAssembly = AssemblyHelper.GetTypesFromAssembly(assembly);
			Type[] array = typesFromAssembly;
			for (int i = 0; i < array.Length; i++)
			{
				Type type = array[i];
				MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				for (int j = 0; j < methods.GetLength(0); j++)
				{
					MethodInfo methodInfo = methods[j];
					object[] customAttributes = methodInfo.GetCustomAttributes(typeof(DrawGizmo), false);
					object[] array2 = customAttributes;
					for (int k = 0; k < array2.Length; k++)
					{
						DrawGizmo drawGizmo = (DrawGizmo)array2[k];
						ParameterInfo[] parameters = methodInfo.GetParameters();
						if (parameters.Length != 2)
						{
							UnityEngine.Debug.LogWarning(string.Format("Method {0}.{1} is marked with the DrawGizmo attribute but does not take parameters (ComponentType, GizmoType) so will be ignored.", methodInfo.DeclaringType.FullName, methodInfo.Name));
						}
						else if (methodInfo.DeclaringType != null && methodInfo.DeclaringType.IsGenericTypeDefinition)
						{
							UnityEngine.Debug.LogWarning(string.Format("Method {0}.{1} is marked with the DrawGizmo attribute but is defined on a generic type definition, so will be ignored.", methodInfo.DeclaringType.FullName, methodInfo.Name));
						}
						else
						{
							AttributeHelper.MonoGizmoMethod item = default(AttributeHelper.MonoGizmoMethod);
							if (drawGizmo.drawnType == null)
							{
								item.drawnType = parameters[0].ParameterType;
							}
							else
							{
								if (!parameters[0].ParameterType.IsAssignableFrom(drawGizmo.drawnType))
								{
									UnityEngine.Debug.LogWarning(string.Format("Method {0}.{1} is marked with the DrawGizmo attribute but the component type it applies to could not be determined.", methodInfo.DeclaringType.FullName, methodInfo.Name));
									goto IL_1DD;
								}
								item.drawnType = drawGizmo.drawnType;
							}
							if (parameters[1].ParameterType != typeof(GizmoType) && parameters[1].ParameterType != typeof(int))
							{
								UnityEngine.Debug.LogWarning(string.Format("Method {0}.{1} is marked with the DrawGizmo attribute but does not take a second parameter of type GizmoType so will be ignored.", methodInfo.DeclaringType.FullName, methodInfo.Name));
							}
							else
							{
								item.drawGizmo = methodInfo;
								item.options = (int)drawGizmo.drawOptions;
								list.Add(item);
							}
						}
						IL_1DD:;
					}
				}
			}
			return list.ToArray();
		}

		[RequiredByNativeCode]
		private static string GetComponentMenuName(Type type)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(AddComponentMenu), false);
			string result;
			if (customAttributes.Length > 0)
			{
				AddComponentMenu addComponentMenu = (AddComponentMenu)customAttributes[0];
				result = addComponentMenu.componentMenu;
			}
			else
			{
				result = null;
			}
			return result;
		}

		[RequiredByNativeCode]
		private static int GetComponentMenuOrdering(Type type)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(AddComponentMenu), false);
			int result;
			if (customAttributes.Length > 0)
			{
				AddComponentMenu addComponentMenu = (AddComponentMenu)customAttributes[0];
				result = addComponentMenu.componentOrder;
			}
			else
			{
				result = 0;
			}
			return result;
		}

		[RequiredByNativeCode]
		private static AttributeHelper.MonoCreateAssetItem[] ExtractCreateAssetMenuItems(Assembly assembly)
		{
			List<AttributeHelper.MonoCreateAssetItem> list = new List<AttributeHelper.MonoCreateAssetItem>();
			Type[] typesFromAssembly = AssemblyHelper.GetTypesFromAssembly(assembly);
			Type[] array = typesFromAssembly;
			for (int i = 0; i < array.Length; i++)
			{
				Type type = array[i];
				CreateAssetMenuAttribute createAssetMenuAttribute = (CreateAssetMenuAttribute)Attribute.GetCustomAttribute(type, typeof(CreateAssetMenuAttribute));
				if (createAssetMenuAttribute != null)
				{
					if (!type.IsSubclassOf(typeof(ScriptableObject)))
					{
						UnityEngine.Debug.LogWarningFormat("CreateAssetMenu attribute on {0} will be ignored as {0} is not derived from ScriptableObject.", new object[]
						{
							type.FullName
						});
					}
					else
					{
						string menuItem = (!string.IsNullOrEmpty(createAssetMenuAttribute.menuName)) ? createAssetMenuAttribute.menuName : ObjectNames.NicifyVariableName(type.Name);
						string text = (!string.IsNullOrEmpty(createAssetMenuAttribute.fileName)) ? createAssetMenuAttribute.fileName : ("New " + ObjectNames.NicifyVariableName(type.Name) + ".asset");
						if (!Path.HasExtension(text))
						{
							text += ".asset";
						}
						list.Add(new AttributeHelper.MonoCreateAssetItem
						{
							menuItem = menuItem,
							fileName = text,
							order = createAssetMenuAttribute.order,
							type = type
						});
					}
				}
			}
			return list.ToArray();
		}

		internal static ArrayList FindEditorClassesWithAttribute(Type attribType)
		{
			ArrayList arrayList = new ArrayList();
			foreach (Type current in EditorAssemblies.loadedTypes)
			{
				if (current.GetCustomAttributes(attribType, false).Length != 0)
				{
					arrayList.Add(current);
				}
			}
			return arrayList;
		}

		internal static object InvokeMemberIfAvailable(object target, string methodName, object[] args)
		{
			MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			object result;
			if (method != null)
			{
				result = method.Invoke(target, args);
			}
			else
			{
				result = null;
			}
			return result;
		}

		internal static bool GameObjectContainsAttribute(GameObject go, Type attributeType)
		{
			Component[] components = go.GetComponents(typeof(Component));
			bool result;
			for (int i = 0; i < components.Length; i++)
			{
				Component component = components[i];
				if (!(component == null))
				{
					Type type = component.GetType();
					if (type.GetCustomAttributes(attributeType, true).Length > 0)
					{
						result = true;
						return result;
					}
				}
			}
			result = false;
			return result;
		}

		[DebuggerHidden]
		internal static IEnumerable<T> CallMethodsWithAttribute<T>(Type attributeType, params object[] arguments)
		{
			AttributeHelper.<CallMethodsWithAttribute>c__Iterator0<T> <CallMethodsWithAttribute>c__Iterator = new AttributeHelper.<CallMethodsWithAttribute>c__Iterator0<T>();
			<CallMethodsWithAttribute>c__Iterator.attributeType = attributeType;
			<CallMethodsWithAttribute>c__Iterator.arguments = arguments;
			AttributeHelper.<CallMethodsWithAttribute>c__Iterator0<T> expr_15 = <CallMethodsWithAttribute>c__Iterator;
			expr_15.$PC = -2;
			return expr_15;
		}

		private static bool AreSignaturesMatching(MethodInfo left, MethodInfo right)
		{
			bool result;
			if (left.IsStatic != right.IsStatic)
			{
				result = false;
			}
			else if (left.ReturnType != right.ReturnType)
			{
				result = false;
			}
			else
			{
				ParameterInfo[] parameters = left.GetParameters();
				ParameterInfo[] parameters2 = right.GetParameters();
				if (parameters.Length != parameters2.Length)
				{
					result = false;
				}
				else
				{
					for (int i = 0; i < parameters.Length; i++)
					{
						if (parameters[i].ParameterType != parameters2[i].ParameterType)
						{
							result = false;
							return result;
						}
					}
					result = true;
				}
			}
			return result;
		}

		internal static string MethodToString(MethodInfo method)
		{
			return ((!method.IsStatic) ? "" : "static ") + method.ToString();
		}

		internal static bool MethodMatchesAnyRequiredSignatureOfAttribute(MethodInfo method, Type attributeType)
		{
			List<MethodInfo> list = new List<MethodInfo>();
			MethodInfo[] methods = attributeType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
			bool result;
			for (int i = 0; i < methods.Length; i++)
			{
				MethodInfo methodInfo = methods[i];
				object[] customAttributes = methodInfo.GetCustomAttributes(typeof(RequiredSignatureAttribute), false);
				if (customAttributes.Length > 0)
				{
					if (AttributeHelper.AreSignaturesMatching(method, methodInfo))
					{
						result = true;
						return result;
					}
					list.Add(methodInfo);
				}
			}
			if (list.Count == 0)
			{
				UnityEngine.Debug.LogError(string.Concat(new object[]
				{
					AttributeHelper.MethodToString(method),
					" has an invalid attribute : ",
					attributeType,
					". ",
					attributeType,
					" must have at least one required signature declaration"
				}));
			}
			else if (list.Count == 1)
			{
				UnityEngine.Debug.LogError(string.Concat(new object[]
				{
					AttributeHelper.MethodToString(method),
					" does not match ",
					attributeType,
					" expected signature.\n Use ",
					AttributeHelper.MethodToString(list[0])
				}));
			}
			else
			{
				object[] expr_102 = new object[5];
				expr_102[0] = AttributeHelper.MethodToString(method);
				expr_102[1] = " does not match any of ";
				expr_102[2] = attributeType;
				expr_102[3] = " expected signatures.\n Valid signatures are: ";
				expr_102[4] = string.Join(" , ", (from a in list
				select AttributeHelper.MethodToString(a)).ToArray<string>());
				UnityEngine.Debug.LogError(string.Concat(expr_102));
			}
			result = false;
			return result;
		}

		internal static AttributeHelper.MethodInfoSorter GetMethodsWithAttribute<T>(BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) where T : Attribute
		{
			if (!AttributeHelper.m_Cache.ContainsKey(typeof(T)))
			{
				List<AttributeHelper.MethodWithAttribute> list = new List<AttributeHelper.MethodWithAttribute>();
				Assembly[] loadedAssemblies = EditorAssemblies.loadedAssemblies;
				for (int i = 0; i < loadedAssemblies.Length; i++)
				{
					Assembly assembly = loadedAssemblies[i];
					Type[] typesFromAssembly = AssemblyHelper.GetTypesFromAssembly(assembly);
					for (int j = 0; j < typesFromAssembly.Length; j++)
					{
						Type type = typesFromAssembly[j];
						MethodInfo[] methods = type.GetMethods(flags);
						for (int k = 0; k < methods.Length; k++)
						{
							MethodInfo methodInfo = methods[k];
							object[] customAttributes = methodInfo.GetCustomAttributes(typeof(T), false);
							if (customAttributes.Length > 0)
							{
								if (methodInfo.IsGenericMethod)
								{
									UnityEngine.Debug.LogError(AttributeHelper.MethodToString(methodInfo) + " is a generic method. " + typeof(T).ToString() + " can't be applied to it.");
								}
								else
								{
									object[] array = customAttributes;
									for (int l = 0; l < array.Length; l++)
									{
										T t = (T)((object)array[l]);
										if (AttributeHelper.MethodMatchesAnyRequiredSignatureOfAttribute(methodInfo, typeof(T)))
										{
											list.Add(new AttributeHelper.MethodWithAttribute
											{
												info = methodInfo,
												attribute = t
											});
										}
									}
								}
							}
						}
					}
				}
				AttributeHelper.m_Cache.Add(typeof(T), new AttributeHelper.MethodInfoSorter(list));
			}
			return AttributeHelper.m_Cache[typeof(T)];
		}
	}
}
