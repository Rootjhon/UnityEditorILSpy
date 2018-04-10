using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Collections;

namespace UnityEditor.Media
{
	public class MediaEncoder : IDisposable
	{
		public IntPtr m_Ptr;

		public MediaEncoder(string filePath, VideoTrackAttributes videoAttrs, AudioTrackAttributes[] audioAttrs)
		{
			this.m_Ptr = this.Create(filePath, new VideoTrackAttributes[]
			{
				videoAttrs
			}, audioAttrs);
		}

		public MediaEncoder(string filePath, VideoTrackAttributes videoAttrs, AudioTrackAttributes audioAttrs) : this(filePath, videoAttrs, new AudioTrackAttributes[]
		{
			audioAttrs
		})
		{
		}

		public MediaEncoder(string filePath, VideoTrackAttributes videoAttrs) : this(filePath, videoAttrs, new AudioTrackAttributes[0])
		{
		}

		public MediaEncoder(string filePath, AudioTrackAttributes[] audioAttrs)
		{
			this.m_Ptr = this.Create(filePath, new VideoTrackAttributes[0], audioAttrs);
		}

		public MediaEncoder(string filePath, AudioTrackAttributes audioAttrs) : this(filePath, new AudioTrackAttributes[]
		{
			audioAttrs
		})
		{
		}

		~MediaEncoder()
		{
			this.Dispose();
		}

		public bool AddFrame(Texture2D texture)
		{
			return MediaEncoder.Internal_AddFrame(this.m_Ptr, texture);
		}

		public bool AddSamples(ushort trackIndex, NativeArray<float> interleavedSamples)
		{
			return MediaEncoder.Internal_AddSamples(this.m_Ptr, trackIndex, interleavedSamples.UnsafeReadOnlyPtr, interleavedSamples.Length);
		}

		public bool AddSamples(NativeArray<float> interleavedSamples)
		{
			return this.AddSamples(0, interleavedSamples);
		}

		public void Dispose()
		{
			if (this.m_Ptr != IntPtr.Zero)
			{
				MediaEncoder.Internal_Release(this.m_Ptr);
				this.m_Ptr = IntPtr.Zero;
			}
			GC.SuppressFinalize(this);
		}

		private IntPtr Create(string filePath, VideoTrackAttributes[] videoAttrs, AudioTrackAttributes[] audioAttrs)
		{
			IntPtr intPtr = MediaEncoder.Internal_Create(filePath, videoAttrs, audioAttrs);
			if (intPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException("MediaEncoder: Output file creation failed for " + filePath);
			}
			return intPtr;
		}

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern IntPtr Internal_Create(string filePath, VideoTrackAttributes[] videoAttrs, AudioTrackAttributes[] audioAttrs);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern void Internal_Release(IntPtr encoder);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool Internal_AddFrame(IntPtr encoder, Texture2D texture);

		[MethodImpl(MethodImplOptions.InternalCall)]
		private static extern bool Internal_AddSamples(IntPtr encoder, ushort trackIndex, IntPtr buffer, int sampleCount);
	}
}
