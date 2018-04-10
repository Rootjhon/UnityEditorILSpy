using System;
using System.Threading;
using UnityEngine;

namespace UnityEditor
{
	internal class WaveformPreview : IDisposable
	{
		public enum ChannelMode
		{
			MonoSum,
			Separate,
			SpecificChannel
		}

		[Flags]
		protected enum MessageFlags
		{
			None = 0,
			Size = 1,
			Length = 2,
			Start = 4,
			Optimization = 8,
			TextureChanged = 16,
			ContentsChanged = 32,
			Looping = 64
		}

		private static int s_BaseTextureWidth = 4096;

		private static Material s_Material;

		public UnityEngine.Object presentedObject;

		protected double m_Start;

		protected double m_Length;

		protected bool m_ClearTexture = true;

		private Texture2D m_Texture;

		private Vector2 m_Size;

		private int m_Channels;

		private int m_Samples;

		private int m_SpecificChannel;

		private WaveformPreview.ChannelMode m_ChannelMode;

		private bool m_Looping;

		private bool m_Optimized;

		private bool m_Dirty;

		private bool m_Disposed;

		private WaveformPreview.MessageFlags m_Flags;

		public event Action updated
		{
			add
			{
				Action action = this.updated;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action>(ref this.updated, (Action)Delegate.Combine(action2, value), action);
				}
				while (action != action2);
			}
			remove
			{
				Action action = this.updated;
				Action action2;
				do
				{
					action2 = action;
					action = Interlocked.CompareExchange<Action>(ref this.updated, (Action)Delegate.Remove(action2, value), action);
				}
				while (action != action2);
			}
		}

		public double start
		{
			get
			{
				return this.m_Start;
			}
		}

		public double length
		{
			get
			{
				return this.m_Length;
			}
		}

		public Color backgroundColor
		{
			get;
			set;
		}

		public Color waveColor
		{
			get;
			set;
		}

		public bool optimized
		{
			get
			{
				return this.m_Optimized;
			}
			set
			{
				if (this.m_Optimized != value)
				{
					if (value)
					{
						this.m_Dirty = true;
					}
					this.m_Optimized = value;
					this.m_Flags |= WaveformPreview.MessageFlags.Optimization;
				}
			}
		}

		public bool looping
		{
			get
			{
				return this.m_Looping;
			}
			set
			{
				if (this.m_Looping != value)
				{
					this.m_Dirty = true;
					this.m_Looping = value;
					this.m_Flags |= WaveformPreview.MessageFlags.Looping;
				}
			}
		}

		protected Vector2 Size
		{
			get
			{
				return this.m_Size;
			}
		}

		protected WaveformPreview(UnityEngine.Object presentedObject, int samplesAndWidth, int channels)
		{
			this.presentedObject = presentedObject;
			this.optimized = true;
			this.m_Samples = samplesAndWidth;
			this.m_Channels = channels;
			this.backgroundColor = new Color(0.156862751f, 0.156862751f, 0.156862751f, 1f);
			this.waveColor = new Color(1f, 0.549019635f, 0f, 1f);
			this.UpdateTexture(samplesAndWidth, channels);
		}

		protected internal WaveformPreview(UnityEngine.Object presentedObject, int samplesAndWidth, int channels, bool deferTextureCreation)
		{
			this.presentedObject = presentedObject;
			this.optimized = true;
			this.m_Samples = samplesAndWidth;
			this.m_Channels = channels;
			this.backgroundColor = new Color(0.156862751f, 0.156862751f, 0.156862751f, 1f);
			this.waveColor = new Color(1f, 0.549019635f, 0f, 1f);
			if (!deferTextureCreation)
			{
				this.UpdateTexture(samplesAndWidth, channels);
			}
		}

		protected static bool HasFlag(WaveformPreview.MessageFlags flags, WaveformPreview.MessageFlags test)
		{
			return (flags & test) != WaveformPreview.MessageFlags.None;
		}

		public void Dispose()
		{
			if (!this.m_Disposed)
			{
				this.m_Disposed = true;
				this.InternalDispose();
				if (this.m_Texture != null)
				{
					UnityEngine.Object.Destroy(this.m_Texture);
				}
				this.m_Texture = null;
			}
		}

		protected virtual void InternalDispose()
		{
		}

		public void Render(Rect rect)
		{
			if (WaveformPreview.s_Material == null)
			{
				WaveformPreview.s_Material = (EditorGUIUtility.LoadRequired("Previews/PreviewAudioClipWaveform.mat") as Material);
			}
			WaveformPreview.s_Material.SetTexture("_WavTex", this.m_Texture);
			WaveformPreview.s_Material.SetFloat("_SampCount", (float)this.m_Samples);
			WaveformPreview.s_Material.SetFloat("_ChanCount", (float)this.m_Channels);
			WaveformPreview.s_Material.SetFloat("_RecPixelSize", 1f / rect.height);
			WaveformPreview.s_Material.SetColor("_BacCol", this.backgroundColor);
			WaveformPreview.s_Material.SetColor("_ForCol", this.waveColor);
			int value = -2;
			if (this.m_ChannelMode == WaveformPreview.ChannelMode.Separate)
			{
				value = -1;
			}
			else if (this.m_ChannelMode == WaveformPreview.ChannelMode.SpecificChannel)
			{
				value = this.m_SpecificChannel;
			}
			WaveformPreview.s_Material.SetInt("_ChanDrawMode", value);
			Graphics.DrawTexture(rect, this.m_Texture, WaveformPreview.s_Material);
		}

		public bool ApplyModifications()
		{
			bool result;
			if (this.m_Dirty || this.m_Texture == null)
			{
				this.m_Flags |= ((!this.UpdateTexture((int)this.m_Size.x, this.m_Channels)) ? WaveformPreview.MessageFlags.None : WaveformPreview.MessageFlags.TextureChanged);
				this.OnModifications(this.m_Flags);
				this.m_Flags = WaveformPreview.MessageFlags.None;
				this.m_Texture.Apply();
				this.m_Dirty = false;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		public void SetChannelMode(WaveformPreview.ChannelMode mode, int specificChannelToRender)
		{
			this.m_ChannelMode = mode;
			this.m_SpecificChannel = specificChannelToRender;
		}

		public void SetChannelMode(WaveformPreview.ChannelMode mode)
		{
			this.SetChannelMode(mode, 0);
		}

		private bool UpdateTexture(int width, int channels)
		{
			int num = width * channels;
			int textureHeight = 1 + num / WaveformPreview.s_BaseTextureWidth;
			Action<bool> action = delegate(bool clear)
			{
				if (this.m_Texture == null)
				{
					this.m_Texture = new Texture2D(WaveformPreview.s_BaseTextureWidth, textureHeight, TextureFormat.RGBAHalf, false, true);
					this.m_Texture.filterMode = FilterMode.Point;
					clear = false;
				}
				else
				{
					this.m_Texture.Resize(WaveformPreview.s_BaseTextureWidth, textureHeight);
				}
				if (clear)
				{
					Color[] pixels = this.m_Texture.GetPixels();
					for (int i = 0; i < pixels.Length; i++)
					{
						pixels[i] = Color.black;
					}
					this.m_Texture.SetPixels(pixels);
				}
			};
			bool result;
			if (width == this.m_Samples && channels == this.m_Channels && this.m_Texture != null)
			{
				result = false;
			}
			else
			{
				action(this.m_ClearTexture);
				this.m_Samples = width;
				this.m_Channels = channels;
				result = (this.m_Dirty = true);
			}
			return result;
		}

		public void OptimizeForSize(Vector2 newSize)
		{
			newSize = new Vector2(Mathf.Ceil(newSize.x), Mathf.Ceil(newSize.y));
			if (newSize.x != this.m_Size.x)
			{
				this.m_Size = newSize;
				this.m_Flags |= WaveformPreview.MessageFlags.Size;
				this.m_Dirty = true;
			}
		}

		protected virtual void OnModifications(WaveformPreview.MessageFlags changedFlags)
		{
		}

		public void SetTimeInfo(double start, double length)
		{
			if (start != this.m_Start || length != this.m_Length)
			{
				this.m_Start = start;
				this.m_Length = length;
				this.m_Dirty = true;
				this.m_Flags |= (WaveformPreview.MessageFlags.Length | WaveformPreview.MessageFlags.Start);
			}
		}

		public virtual void SetMMWaveData(int interleavedOffset, float[] data)
		{
			for (int i = 0; i < data.Length; i += 2)
			{
				int x = interleavedOffset % WaveformPreview.s_BaseTextureWidth;
				int y = interleavedOffset / WaveformPreview.s_BaseTextureWidth;
				this.m_Texture.SetPixel(x, y, new Color(data[i], data[i + 1], 0f, 0f));
				interleavedOffset++;
			}
			this.m_Dirty = true;
			this.m_Flags |= WaveformPreview.MessageFlags.ContentsChanged;
			if (this.updated != null)
			{
				this.updated();
			}
		}
	}
}
