using System;
using System.Linq;
using System.Text;
using UnityEditor.Networking.PlayerConnection;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking.PlayerConnection;

namespace UnityEditor
{
	internal class PlayerConnectionLogReceiver : ScriptableSingleton<PlayerConnectionLogReceiver>
	{
		internal enum ConnectionState
		{
			Disconnected,
			CleanLog,
			FullLog
		}

		private const string prefsKey = "PlayerConnectionLoggingState";

		[SerializeField]
		private PlayerConnectionLogReceiver.ConnectionState state = PlayerConnectionLogReceiver.ConnectionState.Disconnected;

		private static Guid logMessageId
		{
			get
			{
				return new Guid("394ada03-8ba0-4f26-b001-1a6cdeb05a62");
			}
		}

		private static Guid cleanLogMessageId
		{
			get
			{
				return new Guid("3ded2dda-cdf2-46d8-a3f6-01741741e7a9");
			}
		}

		internal PlayerConnectionLogReceiver.ConnectionState State
		{
			get
			{
				return this.state;
			}
			set
			{
				if (this.state != value)
				{
					PlayerConnectionLogReceiver.ConnectionState connectionState = this.state;
					if (connectionState != PlayerConnectionLogReceiver.ConnectionState.CleanLog)
					{
						if (connectionState == PlayerConnectionLogReceiver.ConnectionState.FullLog)
						{
							ScriptableSingleton<EditorConnection>.instance.Unregister(PlayerConnectionLogReceiver.logMessageId, new UnityAction<MessageEventArgs>(this.LogMessage));
						}
					}
					else
					{
						ScriptableSingleton<EditorConnection>.instance.Unregister(PlayerConnectionLogReceiver.cleanLogMessageId, new UnityAction<MessageEventArgs>(this.LogMessage));
					}
					this.state = value;
					PlayerConnectionLogReceiver.ConnectionState connectionState2 = this.state;
					if (connectionState2 != PlayerConnectionLogReceiver.ConnectionState.CleanLog)
					{
						if (connectionState2 == PlayerConnectionLogReceiver.ConnectionState.FullLog)
						{
							ScriptableSingleton<EditorConnection>.instance.Register(PlayerConnectionLogReceiver.logMessageId, new UnityAction<MessageEventArgs>(this.LogMessage));
						}
					}
					else
					{
						ScriptableSingleton<EditorConnection>.instance.Register(PlayerConnectionLogReceiver.cleanLogMessageId, new UnityAction<MessageEventArgs>(this.LogMessage));
					}
					EditorPrefs.SetInt("PlayerConnectionLoggingState", (int)this.state);
				}
			}
		}

		private void OnEnable()
		{
			this.State = (PlayerConnectionLogReceiver.ConnectionState)EditorPrefs.GetInt("PlayerConnectionLoggingState", 1);
		}

		private void LogMessage(MessageEventArgs messageEventArgs)
		{
			byte[] bytes = messageEventArgs.data.Skip(4).ToArray<byte>();
			string text = Encoding.UTF8.GetString(bytes);
			LogType logType = (LogType)messageEventArgs.data[0];
			if (!Enum.IsDefined(typeof(LogType), logType))
			{
				logType = LogType.Log;
			}
			StackTraceLogType stackTraceLogType = Application.GetStackTraceLogType(logType);
			Application.SetStackTraceLogType(logType, StackTraceLogType.None);
			string connectionIdentifier = ProfilerDriver.GetConnectionIdentifier(messageEventArgs.playerId);
			text = "<i>" + connectionIdentifier + "</i> " + text;
			Debug.unityLogger.Log(logType, text);
			Application.SetStackTraceLogType(logType, stackTraceLogType);
		}
	}
}
