using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal class NodeAdapter
	{
		private static List<MethodInfo> s_TypeAdapters;

		private static Dictionary<int, MethodInfo> s_NodeAdapterDictionary;

		public bool CanAdapt(object a, object b)
		{
			bool result;
			if (a == b)
			{
				result = false;
			}
			else if (a == null || b == null)
			{
				result = false;
			}
			else
			{
				MethodInfo adapter = this.GetAdapter(a, b);
				if (adapter == null)
				{
					Debug.Log(string.Concat(new object[]
					{
						"adapter node not found for: ",
						a.GetType(),
						" -> ",
						b.GetType()
					}));
				}
				result = (adapter != null);
			}
			return result;
		}

		public bool Connect(object a, object b)
		{
			MethodInfo adapter = this.GetAdapter(a, b);
			bool result;
			if (adapter == null)
			{
				Debug.LogError(string.Concat(new object[]
				{
					"Attempt to connect 2 unadaptable types: ",
					a.GetType(),
					" -> ",
					b.GetType()
				}));
				result = false;
			}
			else
			{
				object obj = adapter.Invoke(this, new object[]
				{
					this,
					a,
					b
				});
				result = (bool)obj;
			}
			return result;
		}

		private IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly, Type extendedType)
		{
			return from m in (from t in assembly.GetTypes()
			where t.IsSealed && !t.IsGenericType && !t.IsNested
			select t).SelectMany((Type t) => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
			where m.IsDefined(typeof(ExtensionAttribute), false) && m.GetParameters()[0].ParameterType == extendedType
			select m;
		}

		public MethodInfo GetAdapter(object a, object b)
		{
			MethodInfo result;
			if (a == null || b == null)
			{
				result = null;
			}
			else
			{
				if (NodeAdapter.s_NodeAdapterDictionary == null)
				{
					NodeAdapter.s_NodeAdapterDictionary = new Dictionary<int, MethodInfo>();
					AppDomain currentDomain = AppDomain.CurrentDomain;
					Assembly[] assemblies = currentDomain.GetAssemblies();
					for (int i = 0; i < assemblies.Length; i++)
					{
						Assembly assembly = assemblies[i];
						foreach (MethodInfo current in this.GetExtensionMethods(assembly, typeof(NodeAdapter)))
						{
							ParameterInfo[] parameters = current.GetParameters();
							if (parameters.Length == 3)
							{
								string text = parameters[1].ParameterType + parameters[2].ParameterType.ToString();
								NodeAdapter.s_NodeAdapterDictionary.Add(text.GetHashCode(), current);
							}
						}
					}
				}
				string text2 = a.GetType().ToString() + b.GetType();
				MethodInfo methodInfo;
				result = ((!NodeAdapter.s_NodeAdapterDictionary.TryGetValue(text2.GetHashCode(), out methodInfo)) ? null : methodInfo);
			}
			return result;
		}

		public MethodInfo GetTypeAdapter(Type from, Type to)
		{
			if (NodeAdapter.s_TypeAdapters == null)
			{
				NodeAdapter.s_TypeAdapters = new List<MethodInfo>();
				AppDomain currentDomain = AppDomain.CurrentDomain;
				Assembly[] assemblies = currentDomain.GetAssemblies();
				for (int i = 0; i < assemblies.Length; i++)
				{
					Assembly assembly = assemblies[i];
					try
					{
						Type[] types = assembly.GetTypes();
						for (int j = 0; j < types.Length; j++)
						{
							Type type = types[j];
							MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
							MethodInfo[] array = methods;
							for (int k = 0; k < array.Length; k++)
							{
								MethodInfo methodInfo = array[k];
								object[] customAttributes = methodInfo.GetCustomAttributes(typeof(TypeAdapter), false);
								if (customAttributes.Any<object>())
								{
									NodeAdapter.s_TypeAdapters.Add(methodInfo);
								}
							}
						}
					}
					catch (Exception message)
					{
						Debug.Log(message);
					}
				}
			}
			MethodInfo result;
			foreach (MethodInfo current in NodeAdapter.s_TypeAdapters)
			{
				if (current.ReturnType == to)
				{
					ParameterInfo[] parameters = current.GetParameters();
					if (parameters.Length == 1)
					{
						if (parameters[0].ParameterType == from)
						{
							result = current;
							return result;
						}
					}
				}
			}
			result = null;
			return result;
		}
	}
}
