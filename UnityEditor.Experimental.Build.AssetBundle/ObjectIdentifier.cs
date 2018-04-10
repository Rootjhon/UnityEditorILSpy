using System;
using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Scripting;

namespace UnityEditor.Experimental.Build.AssetBundle
{
	[UsedByNativeCode]
	[Serializable]
	public struct ObjectIdentifier
	{
		[NativeName("guid")]
		internal GUID m_GUID;

		[NativeName("localIdentifierInFile")]
		internal long m_LocalIdentifierInFile;

		[NativeName("fileType")]
		internal FileType m_FileType;

		[NativeName("filePath")]
		internal string m_FilePath;

		public GUID guid
		{
			get
			{
				return this.m_GUID;
			}
		}

		public long localIdentifierInFile
		{
			get
			{
				return this.m_LocalIdentifierInFile;
			}
		}

		public FileType fileType
		{
			get
			{
				return this.m_FileType;
			}
		}

		public string filePath
		{
			get
			{
				return this.m_FilePath;
			}
		}

		public override string ToString()
		{
			return UnityString.Format("{{guid: {0}, fileID: {1}, type: {2}, path: {3}}}", new object[]
			{
				this.m_GUID,
				this.m_LocalIdentifierInFile,
				this.m_FileType,
				this.m_FilePath
			});
		}

		public static bool operator ==(ObjectIdentifier a, ObjectIdentifier b)
		{
			return !(a.m_GUID != b.m_GUID) && a.m_LocalIdentifierInFile == b.m_LocalIdentifierInFile && a.m_FileType == b.m_FileType && !(a.m_FilePath != b.m_FilePath);
		}

		public static bool operator !=(ObjectIdentifier a, ObjectIdentifier b)
		{
			return !(a == b);
		}

		public static bool operator <(ObjectIdentifier a, ObjectIdentifier b)
		{
			bool result;
			if (a.m_GUID == b.m_GUID)
			{
				result = (a.m_LocalIdentifierInFile < b.m_LocalIdentifierInFile);
			}
			else
			{
				result = (a.m_GUID < b.m_GUID);
			}
			return result;
		}

		public static bool operator >(ObjectIdentifier a, ObjectIdentifier b)
		{
			bool result;
			if (a.m_GUID == b.m_GUID)
			{
				result = (a.m_LocalIdentifierInFile > b.m_LocalIdentifierInFile);
			}
			else
			{
				result = (a.m_GUID > b.m_GUID);
			}
			return result;
		}

		public override bool Equals(object obj)
		{
			return obj is ObjectIdentifier && this == (ObjectIdentifier)obj;
		}

		public override int GetHashCode()
		{
			int num = this.m_GUID.GetHashCode();
			num = (num * 397 ^ this.m_LocalIdentifierInFile.GetHashCode());
			num = (num * 397 ^ (int)this.m_FileType);
			if (!string.IsNullOrEmpty(this.m_FilePath))
			{
				num = (num * 397 ^ this.m_FilePath.GetHashCode());
			}
			return num;
		}
	}
}
