using System;

namespace UnityEditor.Compilation
{
	public struct CompilerMessage
	{
		public string message;

		public string file;

		public int line;

		public int column;

		public CompilerMessageType type;
	}
}
