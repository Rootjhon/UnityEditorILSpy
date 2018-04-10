using System;

namespace UnityEditor.Compilation
{
	public class AssemblyDefinitionException : Exception
	{
		public string[] filePaths
		{
			get;
			private set;
		}

		public AssemblyDefinitionException(string message, params string[] filePaths) : base(message)
		{
			this.filePaths = filePaths;
		}
	}
}
