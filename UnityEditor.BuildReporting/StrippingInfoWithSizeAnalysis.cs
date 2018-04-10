using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnityEditor.BuildReporting
{
	internal abstract class StrippingInfoWithSizeAnalysis : StrippingInfo
	{
		protected Dictionary<string, int> folderSizes = new Dictionary<string, int>();

		protected Dictionary<string, int> moduleSizes = new Dictionary<string, int>();

		protected Dictionary<string, int> assemblySizes = new Dictionary<string, int>();

		protected Dictionary<string, int> objectSizes = new Dictionary<string, int>();

		protected abstract Dictionary<string, string> GetModuleArtifacts();

		protected abstract Dictionary<string, string> GetSymbolArtifacts();

		protected abstract Dictionary<string, int> GetFunctionSizes();

		protected abstract void AddPlatformSpecificCodeOutputModules();

		private static Dictionary<string, string> GetIl2CPPAssemblyMapArtifacts(string path)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			string[] array = File.ReadAllLines(path);
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text = array2[i];
				string[] array3 = text.Split(new char[]
				{
					'\t'
				});
				if (array3.Length == 3)
				{
					dictionary[array3[0]] = array3[2];
				}
			}
			return dictionary;
		}

		private static void OutputSizes(Dictionary<string, int> sizes, int totalLines)
		{
			List<KeyValuePair<string, int>> list = sizes.ToList<KeyValuePair<string, int>>();
			list.Sort((KeyValuePair<string, int> firstPair, KeyValuePair<string, int> nextPair) => nextPair.Value.CompareTo(firstPair.Value));
			foreach (KeyValuePair<string, int> current in list)
			{
				if (current.Value < 10000)
				{
					break;
				}
				Console.WriteLine(string.Concat(new string[]
				{
					current.Value.ToString("D6"),
					" ",
					((double)current.Value * 100.0 / (double)totalLines).ToString("F2"),
					"% ",
					current.Key
				}));
			}
		}

		private static void PrintSizesDictionary(Dictionary<string, int> sizes, int maxSize)
		{
			List<KeyValuePair<string, int>> list = sizes.ToList<KeyValuePair<string, int>>();
			list.Sort((KeyValuePair<string, int> pair1, KeyValuePair<string, int> pair2) => pair2.Value.CompareTo(pair1.Value));
			int num = 0;
			while (num < maxSize && num < list.Count)
			{
				Console.WriteLine(list[num].Value.ToString("D8") + " " + list[num].Key);
				num++;
			}
		}

		private static void PrintSizesDictionary(Dictionary<string, int> sizes)
		{
			StrippingInfoWithSizeAnalysis.PrintSizesDictionary(sizes, 500);
		}

		public void Analyze()
		{
			bool flag = Unsupported.IsDeveloperBuild();
			Dictionary<string, string> symbolArtifacts = this.GetSymbolArtifacts();
			Dictionary<string, string> moduleArtifacts = this.GetModuleArtifacts();
			Dictionary<string, string> il2CPPAssemblyMapArtifacts = StrippingInfoWithSizeAnalysis.GetIl2CPPAssemblyMapArtifacts("Temp/StagingArea/Data/il2cppOutput/Symbols/MethodMap.tsv");
			int num = 0;
			Dictionary<string, int> functionSizes = this.GetFunctionSizes();
			foreach (KeyValuePair<string, int> current in functionSizes)
			{
				if (symbolArtifacts.ContainsKey(current.Key))
				{
					string text = symbolArtifacts[current.Key].Replace('\\', '/');
					if (flag)
					{
						if (!this.objectSizes.ContainsKey(text))
						{
							this.objectSizes[text] = 0;
						}
						Dictionary<string, int> dictionary;
						string key;
						(dictionary = this.objectSizes)[key = text] = dictionary[key] + current.Value;
					}
					if (text.LastIndexOf('/') != -1)
					{
						string text2 = text.Substring(0, text.LastIndexOf('/'));
						if (!this.folderSizes.ContainsKey(text2))
						{
							this.folderSizes[text2] = 0;
						}
						Dictionary<string, int> dictionary;
						string key2;
						(dictionary = this.folderSizes)[key2 = text2] = dictionary[key2] + current.Value;
					}
				}
				if (moduleArtifacts.ContainsKey(current.Key))
				{
					string text3 = moduleArtifacts[current.Key];
					text3 = text3.Substring(0, text3.Length - "Module_Dynamic.bc".Length);
					text3 = StrippingInfo.ModuleName(text3);
					if (!this.moduleSizes.ContainsKey(text3))
					{
						this.moduleSizes[text3] = 0;
					}
					Dictionary<string, int> dictionary;
					string key3;
					(dictionary = this.moduleSizes)[key3 = text3] = dictionary[key3] + current.Value;
					num += current.Value;
				}
				if (il2CPPAssemblyMapArtifacts.ContainsKey(current.Key))
				{
					string text4 = il2CPPAssemblyMapArtifacts[current.Key];
					if (!this.assemblySizes.ContainsKey(text4))
					{
						this.assemblySizes[text4] = 0;
					}
					Dictionary<string, int> dictionary;
					string key4;
					(dictionary = this.assemblySizes)[key4 = text4] = dictionary[key4] + current.Value;
				}
			}
			this.AddPlatformSpecificCodeOutputModules();
			int num2 = this.totalSize;
			foreach (KeyValuePair<string, int> current2 in this.moduleSizes)
			{
				if (this.modules.Contains(current2.Key))
				{
					num2 -= current2.Value;
				}
			}
			this.moduleSizes["Unaccounted"] = num2;
			base.AddModule("Unaccounted");
			foreach (KeyValuePair<string, int> current3 in this.moduleSizes)
			{
				base.AddModuleSize(current3.Key, current3.Value);
			}
			int num3 = 0;
			foreach (KeyValuePair<string, int> current4 in this.assemblySizes)
			{
				base.RegisterDependency("IL2CPP Generated", current4.Key);
				this.sizes[current4.Key] = current4.Value;
				num3 += current4.Value;
				base.SetIcon(current4.Key, "class/DefaultAsset");
			}
			base.RegisterDependency("IL2CPP Generated", "IL2CPP Unaccounted");
			this.sizes["IL2CPP Unaccounted"] = this.moduleSizes["IL2CPP Generated"] - num3;
			base.SetIcon("IL2CPP Unaccounted", "class/DefaultAsset");
			if (flag)
			{
				Console.WriteLine("Code size per module: ");
				StrippingInfoWithSizeAnalysis.PrintSizesDictionary(this.moduleSizes);
				Console.WriteLine("\n\n");
				Console.WriteLine("Code size per source folder: ");
				StrippingInfoWithSizeAnalysis.PrintSizesDictionary(this.folderSizes);
				Console.WriteLine("\n\n");
				Console.WriteLine("Code size per object file: ");
				StrippingInfoWithSizeAnalysis.PrintSizesDictionary(this.objectSizes);
				Console.WriteLine("\n\n");
				Console.WriteLine("Code size per function: ");
				StrippingInfoWithSizeAnalysis.PrintSizesDictionary(functionSizes);
				Console.WriteLine("\n\n");
			}
		}
	}
}
