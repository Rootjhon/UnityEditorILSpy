using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEditor.Connect;
using UnityEditor.Utils;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.CrashReporting
{
	internal class CrashReporting
	{
		private class UploadPlatformConfig
		{
			public string UsymtoolPath;

			public string LzmaPath;

			public string LogFilePath;
		}

		public static string ServiceBaseUrl
		{
			get
			{
				string configurationURL = UnityConnect.instance.GetConfigurationURL(CloudConfigUrl.CloudPerfEvents);
				string result;
				if (!string.IsNullOrEmpty(configurationURL))
				{
					result = configurationURL;
				}
				else
				{
					result = string.Empty;
				}
				return result;
			}
		}

		public static string NativeCrashSubmissionUrl
		{
			get
			{
				string result;
				if (!string.IsNullOrEmpty(CrashReporting.ServiceBaseUrl))
				{
					result = new Uri(new Uri(CrashReporting.ServiceBaseUrl), "symbolicate").ToString();
				}
				else
				{
					result = string.Empty;
				}
				return result;
			}
		}

		public static string SignedUrlSourceUrl
		{
			get
			{
				string result;
				if (!string.IsNullOrEmpty(CrashReporting.ServiceBaseUrl))
				{
					result = new Uri(new Uri(CrashReporting.ServiceBaseUrl), "url").ToString();
				}
				else
				{
					result = string.Empty;
				}
				return result;
			}
		}

		public static string ServiceTokenUrl
		{
			get
			{
				string result;
				if (!string.IsNullOrEmpty(CrashReporting.ServiceBaseUrl) && !string.IsNullOrEmpty(PlayerSettings.cloudProjectId))
				{
					result = new Uri(new Uri(CrashReporting.ServiceBaseUrl), string.Format("token/{0}", PlayerSettings.cloudProjectId)).ToString();
				}
				else
				{
					result = string.Empty;
				}
				return result;
			}
		}

		public static string GetUsymUploadAuthToken()
		{
			string text = string.Empty;
			RemoteCertificateValidationCallback serverCertificateValidationCallback = ServicePointManager.ServerCertificateValidationCallback;
			string result;
			try
			{
				string environmentVariable = Environment.GetEnvironmentVariable("USYM_UPLOAD_AUTH_TOKEN");
				if (!string.IsNullOrEmpty(environmentVariable))
				{
					result = environmentVariable;
					return result;
				}
				string accessToken = UnityConnect.instance.GetAccessToken();
				if (string.IsNullOrEmpty(accessToken))
				{
					result = string.Empty;
					return result;
				}
				string cloudProjectId = PlayerSettings.cloudProjectId;
				if (Application.platform != RuntimePlatform.OSXEditor)
				{
					ServicePointManager.ServerCertificateValidationCallback = ((object a, X509Certificate b, X509Chain c, SslPolicyErrors d) => true);
				}
				WebRequest webRequest = WebRequest.Create(CrashReporting.ServiceTokenUrl);
				webRequest.Timeout = 15000;
				webRequest.Headers.Add("Authorization", string.Format("Bearer {0}", accessToken));
				HttpWebResponse httpWebResponse = webRequest.GetResponse() as HttpWebResponse;
				string jsondata = string.Empty;
				using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
				{
					jsondata = streamReader.ReadToEnd();
				}
				JSONValue jSONValue = JSONParser.SimpleParse(jsondata);
				if (jSONValue.ContainsKey("AuthToken"))
				{
					text = jSONValue["AuthToken"].AsString();
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
			ServicePointManager.ServerCertificateValidationCallback = serverCertificateValidationCallback;
			result = text;
			return result;
		}

		private static CrashReporting.UploadPlatformConfig GetUploadPlatformConfig()
		{
			CrashReporting.UploadPlatformConfig uploadPlatformConfig = new CrashReporting.UploadPlatformConfig();
			if (Application.platform == RuntimePlatform.WindowsEditor)
			{
				uploadPlatformConfig.UsymtoolPath = Paths.Combine(new string[]
				{
					EditorApplication.applicationContentsPath,
					"Tools",
					"usymtool.exe"
				});
				uploadPlatformConfig.LzmaPath = Paths.Combine(new string[]
				{
					EditorApplication.applicationContentsPath,
					"Tools",
					"lzma.exe"
				});
				uploadPlatformConfig.LogFilePath = Paths.Combine(new string[]
				{
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"Unity",
					"Editor",
					"symbol_upload.log"
				});
			}
			else if (Application.platform == RuntimePlatform.OSXEditor)
			{
				uploadPlatformConfig.UsymtoolPath = Paths.Combine(new string[]
				{
					EditorApplication.applicationContentsPath,
					"Tools",
					"macosx",
					"usymtool"
				});
				uploadPlatformConfig.LzmaPath = Paths.Combine(new string[]
				{
					EditorApplication.applicationContentsPath,
					"Tools",
					"lzma"
				});
				uploadPlatformConfig.LogFilePath = Paths.Combine(new string[]
				{
					Environment.GetEnvironmentVariable("HOME"),
					"Library",
					"Logs",
					"Unity",
					"symbol_upload.log"
				});
			}
			else if (Application.platform == RuntimePlatform.LinuxEditor)
			{
				uploadPlatformConfig.UsymtoolPath = Paths.Combine(new string[]
				{
					EditorApplication.applicationContentsPath,
					"Tools",
					"usymtool"
				});
				uploadPlatformConfig.LzmaPath = Paths.Combine(new string[]
				{
					EditorApplication.applicationContentsPath,
					"Tools",
					"lzma-linux32"
				});
				uploadPlatformConfig.LogFilePath = Paths.Combine(new string[]
				{
					Environment.GetEnvironmentVariable("HOME"),
					".config",
					"unity3d",
					"symbol_upload.log"
				});
			}
			return uploadPlatformConfig;
		}

		public static void UploadSymbolsInPath(string authToken, string symbolPath, string includeFilter, string excludeFilter, bool waitForExit)
		{
			CrashReporting.UploadPlatformConfig uploadPlatformConfig = CrashReporting.GetUploadPlatformConfig();
			string arguments = string.Format("-symbolPath \"{0}\" -log \"{1}\" -filter \"{2}\" -excludeFilter \"{3}\"", new object[]
			{
				symbolPath,
				uploadPlatformConfig.LogFilePath,
				includeFilter,
				excludeFilter
			});
			ProcessStartInfo processStartInfo = new ProcessStartInfo
			{
				Arguments = arguments,
				CreateNoWindow = true,
				FileName = uploadPlatformConfig.UsymtoolPath,
				WorkingDirectory = Directory.GetParent(Application.dataPath).FullName,
				UseShellExecute = false
			};
			processStartInfo.EnvironmentVariables.Add("USYM_UPLOAD_AUTH_TOKEN", authToken);
			processStartInfo.EnvironmentVariables.Add("USYM_UPLOAD_URL_SOURCE", CrashReporting.SignedUrlSourceUrl);
			processStartInfo.EnvironmentVariables.Add("LZMA_PATH", uploadPlatformConfig.LzmaPath);
			Process process = new Process();
			process.StartInfo = processStartInfo;
			process.Start();
			if (waitForExit)
			{
				process.WaitForExit();
			}
		}
	}
}
