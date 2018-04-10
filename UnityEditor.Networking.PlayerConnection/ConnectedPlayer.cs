using System;
using UnityEngine;

namespace UnityEditor.Networking.PlayerConnection
{
	[Serializable]
	public class ConnectedPlayer
	{
		[SerializeField]
		private int m_PlayerId;

		[SerializeField]
		private string m_PlayerName;

		[Obsolete("Use playerId instead (UnityUpgradable) -> playerId", true)]
		public int PlayerId
		{
			get
			{
				return this.m_PlayerId;
			}
		}

		public int playerId
		{
			get
			{
				return this.m_PlayerId;
			}
		}

		public string name
		{
			get
			{
				return this.m_PlayerName;
			}
		}

		public ConnectedPlayer()
		{
		}

		public ConnectedPlayer(int playerId)
		{
			this.m_PlayerId = playerId;
		}

		public ConnectedPlayer(int playerId, string name)
		{
			this.m_PlayerId = playerId;
			this.m_PlayerName = name;
		}
	}
}
