using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace UnityEditor.Experimental.Build.Player
{
	[Serializable]
	public class TypeDB : ISerializable, IDisposable
	{
		private IntPtr m_Ptr;

		internal TypeDB()
		{
			this.m_Ptr = TypeDB.Internal_Create();
		}

		protected TypeDB(SerializationInfo info, StreamingContext context)
		{
			this.m_Ptr = TypeDB.Internal_Create();
			string data = (string)info.GetValue("typedb", typeof(string));
			this.DeserializeNativeTypeDB(data);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr Internal_Create();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Internal_Destroy(IntPtr ptr);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern int GetHash();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern string SerializeNativeTypeDB();

		[MethodImpl(MethodImplOptions.InternalCall)]
		private extern void DeserializeNativeTypeDB(string data);

		~TypeDB()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			if (this.m_Ptr != IntPtr.Zero)
			{
				TypeDB.Internal_Destroy(this.m_Ptr);
				this.m_Ptr = IntPtr.Zero;
			}
		}

		public override bool Equals(object obj)
		{
			TypeDB typeDB = obj as TypeDB;
			return typeDB != null && typeDB.GetHash() == this.GetHash();
		}

		public override int GetHashCode()
		{
			return this.GetHash();
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			string value = this.SerializeNativeTypeDB();
			info.AddValue("typedb", value);
		}
	}
}
