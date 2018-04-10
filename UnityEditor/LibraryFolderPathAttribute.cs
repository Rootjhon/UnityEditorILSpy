using System;
using UnityEngine;

namespace UnityEditor
{
	[AttributeUsage(AttributeTargets.Class)]
	internal class LibraryFolderPathAttribute : Attribute
	{
		public string folderPath
		{
			get;
			set;
		}

		public LibraryFolderPathAttribute(string relativePath)
		{
			if (string.IsNullOrEmpty(relativePath))
			{
				Debug.LogError("Invalid relative path! (its null or empty)");
			}
			else
			{
				if (relativePath[0] == '/')
				{
					throw new ArgumentException("Folder relative path cannot start with a slash.");
				}
				this.folderPath = "Library/" + relativePath;
			}
		}
	}
}
