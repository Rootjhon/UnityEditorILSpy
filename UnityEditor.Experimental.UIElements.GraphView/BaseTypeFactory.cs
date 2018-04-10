using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.Experimental.UIElements.GraphView
{
	internal abstract class BaseTypeFactory<TKey, TValue>
	{
		private readonly Dictionary<Type, Type> m_Mappings = new Dictionary<Type, Type>();

		private readonly Type m_FallbackType;

		private static readonly Type k_KeyType;

		private static readonly Type k_ValueType;

		public Type this[Type t]
		{
			get
			{
				Type result;
				try
				{
					result = this.m_Mappings[t];
				}
				catch (KeyNotFoundException innerException)
				{
					throw new KeyNotFoundException("Type " + t.Name + " is not registered in the factory.", innerException);
				}
				return result;
			}
			set
			{
				if (!t.IsSubclassOf(BaseTypeFactory<TKey, TValue>.k_KeyType) && !t.GetInterfaces().Contains(BaseTypeFactory<TKey, TValue>.k_KeyType))
				{
					throw new ArgumentException(string.Concat(new string[]
					{
						"The type passed as key (",
						t.Name,
						") does not implement or derive from ",
						BaseTypeFactory<TKey, TValue>.k_KeyType.Name,
						"."
					}));
				}
				if (!value.IsSubclassOf(BaseTypeFactory<TKey, TValue>.k_ValueType))
				{
					throw new ArgumentException(string.Concat(new string[]
					{
						"The type passed as value (",
						value.Name,
						") does not derive from ",
						BaseTypeFactory<TKey, TValue>.k_ValueType.Name,
						"."
					}));
				}
				this.m_Mappings[t] = value;
			}
		}

		static BaseTypeFactory()
		{
			BaseTypeFactory<TKey, TValue>.k_KeyType = typeof(TKey);
			BaseTypeFactory<TKey, TValue>.k_ValueType = typeof(TValue);
		}

		protected BaseTypeFactory() : this(typeof(TValue))
		{
		}

		protected BaseTypeFactory(Type fallbackType)
		{
			this.m_FallbackType = fallbackType;
		}

		public virtual TValue Create(TKey key)
		{
			Type type = null;
			Type type2 = key.GetType();
			while (type == null && type2 != null && type2 != typeof(TKey))
			{
				if (!this.m_Mappings.TryGetValue(type2, out type))
				{
					type2 = type2.BaseType;
				}
			}
			if (type == null)
			{
				type = this.m_FallbackType;
			}
			return this.InternalCreate(type);
		}

		protected virtual TValue InternalCreate(Type valueType)
		{
			return (TValue)((object)Activator.CreateInstance(valueType));
		}
	}
}
