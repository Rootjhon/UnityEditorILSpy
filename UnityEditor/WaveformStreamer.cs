using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Bindings;

namespace UnityEditor
{
	[NativeType(Header = "Editor/Mono/Audio/WaveformStreamer.bindings.h")]
	internal sealed class WaveformStreamer
	{
		internal IntPtr m_Data;

		public bool done
		{
			get
			{
				return WaveformStreamer.Internal_WaveformStreamerQueryFinishedStatus(this.m_Data);
			}
		}

		public WaveformStreamer(AudioClip clip, double start, double duration, int numOutputSamples, Func<WaveformStreamer, float[], int, bool> onNewWaveformData)
		{
			this.m_Data = WaveformStreamer.Internal_WaveformStreamerCreate(this, clip, start, duration, numOutputSamples, onNewWaveformData);
		}

		private WaveformStreamer(AudioClip clip, double start, double duration, int numOutputSamples, Func<object, float[], int, bool> onNewWaveformData)
		{
			this.m_Data = WaveformStreamer.Internal_WaveformStreamerCreateUntyped(this, clip, start, duration, numOutputSamples, onNewWaveformData);
		}

		public void Stop()
		{
			WaveformStreamer.Internal_WaveformStreamerStop(this.m_Data);
		}

		~WaveformStreamer()
		{
			if (this.m_Data != IntPtr.Zero)
			{
				WaveformStreamer.Internal_WaveformStreamerDestroy(this.m_Data);
			}
		}

		internal static object CreateUntypedWaveformStreamer(AudioClip clip, double start, double duration, int numOutputSamples, Func<object, float[], int, bool> onNewWaveformData)
		{
			return new WaveformStreamer(clip, start, duration, numOutputSamples, onNewWaveformData);
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr Internal_WaveformStreamerCreate(WaveformStreamer instance, [NotNull] AudioClip clip, double start, double duration, int numOutputSamples, [NotNull] Func<WaveformStreamer, float[], int, bool> onNewWaveformData);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern bool Internal_WaveformStreamerQueryFinishedStatus(IntPtr streamer);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void Internal_WaveformStreamerStop(IntPtr streamer);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern IntPtr Internal_WaveformStreamerCreateUntyped(object instance, [NotNull] AudioClip clip, double start, double duration, int numOutputSamples, [NotNull] Func<object, float[], int, bool> onNewWaveformData);

		[MethodImpl(MethodImplOptions.InternalCall)]
		internal static extern void Internal_WaveformStreamerDestroy(IntPtr streamer);
	}
}
