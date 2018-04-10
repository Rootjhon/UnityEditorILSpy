using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	internal class StreamedAudioClipPreview : WaveformPreview
	{
		private static class AudioClipMinMaxOverview
		{
			private static Dictionary<AudioClip, float[]> s_Data = new Dictionary<AudioClip, float[]>();

			public static float[] GetOverviewFor(AudioClip clip)
			{
				float[] result;
				if (!StreamedAudioClipPreview.AudioClipMinMaxOverview.s_Data.ContainsKey(clip))
				{
					string assetPath = AssetDatabase.GetAssetPath(clip);
					if (assetPath == null)
					{
						result = null;
						return result;
					}
					AssetImporter atPath = AssetImporter.GetAtPath(assetPath);
					if (atPath == null)
					{
						result = null;
						return result;
					}
					StreamedAudioClipPreview.AudioClipMinMaxOverview.s_Data[clip] = AudioUtil.GetMinMaxData(atPath as AudioImporter);
				}
				result = StreamedAudioClipPreview.AudioClipMinMaxOverview.s_Data[clip];
				return result;
			}
		}

		private struct ClipPreviewDetails
		{
			public float[] preview;

			public int previewSamples;

			public double normalizedDuration;

			public double normalizedStart;

			public double deltaStep;

			public AudioClip clip;

			public int previewPixelsToRender;

			public double localStart;

			public double localLength;

			public bool looping;

			public ClipPreviewDetails(AudioClip clip, bool isLooping, int size, double localStart, double localLength)
			{
				if (size < 2)
				{
					throw new ArgumentException("Size has to be larger than 1");
				}
				if (localLength <= 0.0)
				{
					throw new ArgumentException("length has to be longer than zero", "localLength");
				}
				if (localStart < 0.0)
				{
					throw new ArgumentException("localStart has to be positive", "localStart");
				}
				if (clip == null)
				{
					throw new ArgumentNullException("clip");
				}
				this.clip = clip;
				this.preview = StreamedAudioClipPreview.AudioClipMinMaxOverview.GetOverviewFor(clip);
				if (this.preview == null)
				{
					throw new ArgumentException("Clip " + clip + "'s overview preview is null");
				}
				this.looping = isLooping;
				this.localStart = localStart;
				this.localLength = localLength;
				if (this.looping)
				{
					this.previewPixelsToRender = size;
				}
				else
				{
					double num = Math.Min((double)clip.length - localStart, localLength);
					this.previewPixelsToRender = (int)Math.Min((double)size, (double)size * Math.Max(0.0, num / localLength));
				}
				this.previewSamples = this.preview.Length / (clip.channels * 2);
				this.normalizedDuration = localLength / (double)clip.length;
				this.normalizedStart = localStart / (double)clip.length;
				this.deltaStep = (double)this.previewSamples * this.normalizedDuration / (double)(size - 1);
			}

			public bool IsCandidateForStreaming()
			{
				return (this.looping || this.localStart < (double)this.clip.length) && this.deltaStep < 0.5;
			}
		}

		private struct Segment
		{
			public WaveformStreamer streamer;

			public int streamingIndexOffset;

			public int textureOffset;

			public int segmentLength;
		}

		private class StreamingContext
		{
			public int index;
		}

		private Dictionary<WaveformStreamer, StreamedAudioClipPreview.StreamingContext> m_Contexts = new Dictionary<WaveformStreamer, StreamedAudioClipPreview.StreamingContext>();

		private StreamedAudioClipPreview.Segment[] m_StreamedSegments;

		private AudioClip m_Clip;

		public StreamedAudioClipPreview(AudioClip clip, int initialSize) : base(clip, initialSize, clip.channels)
		{
			this.m_ClearTexture = false;
			this.m_Clip = clip;
			this.m_Start = 0.0;
			this.m_Length = (double)clip.length;
		}

		protected override void InternalDispose()
		{
			base.InternalDispose();
			this.KillAndClearStreamers();
			this.m_StreamedSegments = null;
		}

		protected override void OnModifications(WaveformPreview.MessageFlags cFlags)
		{
			bool flag = false;
			if (WaveformPreview.HasFlag(cFlags, WaveformPreview.MessageFlags.TextureChanged) || WaveformPreview.HasFlag(cFlags, WaveformPreview.MessageFlags.Size) || WaveformPreview.HasFlag(cFlags, WaveformPreview.MessageFlags.Length) || WaveformPreview.HasFlag(cFlags, WaveformPreview.MessageFlags.Looping))
			{
				this.KillAndClearStreamers();
				if (base.length <= 0.0)
				{
					return;
				}
				StreamedAudioClipPreview.ClipPreviewDetails details = new StreamedAudioClipPreview.ClipPreviewDetails(this.m_Clip, base.looping, (int)base.Size.x, base.start, base.length);
				this.UploadPreview(details);
				if (details.IsCandidateForStreaming())
				{
					flag = true;
				}
			}
			if (!base.optimized)
			{
				this.KillAndClearStreamers();
				flag = false;
			}
			else if (WaveformPreview.HasFlag(cFlags, WaveformPreview.MessageFlags.Optimization) && !flag)
			{
				StreamedAudioClipPreview.ClipPreviewDetails clipPreviewDetails = new StreamedAudioClipPreview.ClipPreviewDetails(this.m_Clip, base.looping, (int)base.Size.x, base.start, base.length);
				if (clipPreviewDetails.IsCandidateForStreaming())
				{
					flag = true;
				}
			}
			if (flag)
			{
				this.m_StreamedSegments = this.CalculateAndStartStreamers(base.start, base.length);
				if (this.m_StreamedSegments != null && this.m_StreamedSegments.Length > 0)
				{
					StreamedAudioClipPreview.Segment[] streamedSegments = this.m_StreamedSegments;
					for (int i = 0; i < streamedSegments.Length; i++)
					{
						StreamedAudioClipPreview.Segment segment = streamedSegments[i];
						if (!this.m_Contexts.ContainsKey(segment.streamer))
						{
							this.m_Contexts.Add(segment.streamer, new StreamedAudioClipPreview.StreamingContext());
						}
					}
				}
			}
			base.OnModifications(cFlags);
		}

		private void KillAndClearStreamers()
		{
			foreach (KeyValuePair<WaveformStreamer, StreamedAudioClipPreview.StreamingContext> current in this.m_Contexts)
			{
				current.Key.Stop();
			}
			this.m_Contexts.Clear();
		}

		private StreamedAudioClipPreview.Segment[] CalculateAndStartStreamers(double localStart, double localLength)
		{
			double num = localStart;
			localStart %= (double)this.m_Clip.length;
			double num2 = localLength / (double)base.Size.x;
			StreamedAudioClipPreview.Segment[] result;
			if (!base.looping)
			{
				if (num > (double)this.m_Clip.length)
				{
					result = null;
				}
				else
				{
					double num3 = Math.Min((double)this.m_Clip.length - num, localLength);
					int num4 = (int)Math.Min((double)base.Size.x, (double)base.Size.x * Math.Max(0.0, num3 / localLength));
					if (num4 < 1)
					{
						result = null;
					}
					else
					{
						StreamedAudioClipPreview.Segment[] array = new StreamedAudioClipPreview.Segment[1];
						array[0].streamer = new WaveformStreamer(this.m_Clip, num, num3, num4, new Func<WaveformStreamer, float[], int, bool>(this.OnNewWaveformData));
						array[0].segmentLength = (int)base.Size.x;
						array[0].textureOffset = 0;
						array[0].streamingIndexOffset = 0;
						result = array;
					}
				}
			}
			else
			{
				StreamedAudioClipPreview.Segment[] array;
				if (localStart + localLength - num2 > (double)this.m_Clip.length)
				{
					double num5 = (double)base.Size.x / localLength;
					if (localLength >= (double)this.m_Clip.length)
					{
						double num6 = localLength / (double)this.m_Clip.length;
						WaveformStreamer streamer = new WaveformStreamer(this.m_Clip, 0.0, (double)this.m_Clip.length, (int)((double)base.Size.x / num6), new Func<WaveformStreamer, float[], int, bool>(this.OnNewWaveformData));
						double num7 = (double)this.m_Clip.length - localStart;
						double num8 = 0.0;
						array = new StreamedAudioClipPreview.Segment[Mathf.CeilToInt((float)((localStart + localLength) / (double)this.m_Clip.length))];
						for (int i = 0; i < array.Length; i++)
						{
							double num9 = Math.Min(num7 + num8, localLength) - num8;
							array[i].streamer = streamer;
							array[i].segmentLength = (int)(num9 * num5);
							array[i].textureOffset = (int)(num8 * num5);
							array[i].streamingIndexOffset = (int)(((double)this.m_Clip.length - num7) * num5);
							num8 += num7;
							num7 = (double)this.m_Clip.length;
						}
					}
					else
					{
						double num10 = (double)this.m_Clip.length - localStart;
						double num11 = localLength - num10;
						array = new StreamedAudioClipPreview.Segment[2];
						array[0].streamer = new WaveformStreamer(this.m_Clip, localStart, num10, (int)(num10 * num5), new Func<WaveformStreamer, float[], int, bool>(this.OnNewWaveformData));
						array[0].segmentLength = (int)(num10 * num5);
						array[0].textureOffset = 0;
						array[0].streamingIndexOffset = 0;
						array[1].streamer = new WaveformStreamer(this.m_Clip, 0.0, num11, (int)(num11 * num5), new Func<WaveformStreamer, float[], int, bool>(this.OnNewWaveformData));
						array[1].segmentLength = (int)(num11 * num5);
						array[1].textureOffset = (int)(num10 * num5);
						array[1].streamingIndexOffset = 0;
					}
				}
				else
				{
					array = new StreamedAudioClipPreview.Segment[1];
					array[0].streamer = new WaveformStreamer(this.m_Clip, localStart, localLength, (int)base.Size.x, new Func<WaveformStreamer, float[], int, bool>(this.OnNewWaveformData));
					array[0].segmentLength = (int)base.Size.x;
					array[0].textureOffset = 0;
					array[0].streamingIndexOffset = 0;
				}
				result = array;
			}
			return result;
		}

		private void UploadPreview(StreamedAudioClipPreview.ClipPreviewDetails details)
		{
			int channels = details.clip.channels;
			float[] array = new float[(int)((float)channels * base.Size.x * 2f)];
			if (details.localStart + details.localLength > (double)details.clip.length)
			{
				this.ResamplePreviewLooped(details, array);
			}
			else
			{
				this.ResamplePreviewConfined(details, array);
			}
			this.SetMMWaveData(0, array);
		}

		private void ResamplePreviewConfined(StreamedAudioClipPreview.ClipPreviewDetails details, float[] resampledPreview)
		{
			int channels = this.m_Clip.channels;
			int previewSamples = details.previewSamples;
			double deltaStep = details.deltaStep;
			double num = details.normalizedStart * (double)previewSamples;
			float[] preview = details.preview;
			if (deltaStep > 0.5)
			{
				int num2 = (int)num;
				int num3 = num2;
				for (int i = 0; i < details.previewPixelsToRender; i++)
				{
					for (int j = 0; j < channels; j++)
					{
						int num4 = num2;
						num3 = (int)num;
						float num5 = preview[2 * num4 * channels + j * 2];
						float num6 = preview[2 * num4 * channels + j * 2 + 1];
						while (++num4 < num3)
						{
							num5 = Mathf.Max(num5, preview[2 * num4 * channels + j * 2]);
							num6 = Mathf.Min(num6, preview[2 * num4 * channels + j * 2 + 1]);
						}
						resampledPreview[2 * i * channels + j * 2] = num6;
						resampledPreview[2 * i * channels + j * 2 + 1] = num5;
					}
					num += deltaStep;
					num2 = num3;
				}
			}
			else
			{
				for (int k = 0; k < details.previewPixelsToRender; k++)
				{
					int num7 = (int)(num - 1.0);
					int num8 = num7 + 1;
					float num9 = (float)(num - 1.0 - (double)num7);
					num7 = Mathf.Max(0, num7);
					num8 = Mathf.Min(num8, previewSamples - 1);
					for (int l = 0; l < channels; l++)
					{
						float num10 = preview[2 * num7 * channels + l * 2];
						float num11 = preview[2 * num7 * channels + l * 2 + 1];
						float num12 = preview[2 * num8 * channels + l * 2];
						float num13 = preview[2 * num8 * channels + l * 2 + 1];
						resampledPreview[2 * k * channels + l * 2] = num9 * num13 + (1f - num9) * num11;
						resampledPreview[2 * k * channels + l * 2 + 1] = num9 * num12 + (1f - num9) * num10;
					}
					num += deltaStep;
				}
			}
		}

		private void ResamplePreviewLooped(StreamedAudioClipPreview.ClipPreviewDetails details, float[] resampledPreview)
		{
			int num = details.preview.Length;
			int channels = this.m_Clip.channels;
			int previewSamples = details.previewSamples;
			double deltaStep = details.deltaStep;
			double num2 = details.normalizedStart * (double)previewSamples;
			float[] preview = details.preview;
			if (deltaStep > 0.5)
			{
				int num3 = (int)num2;
				int num4 = num3;
				for (int i = 0; i < details.previewPixelsToRender; i++)
				{
					for (int j = 0; j < channels; j++)
					{
						int num5 = num3;
						num4 = (int)num2;
						int num6 = (2 * num5 * channels + j * 2) % num;
						float num7 = preview[num6];
						float num8 = preview[num6 + 1];
						while (++num5 < num4)
						{
							num6 = (2 * num5 * channels + j * 2) % num;
							num7 = Mathf.Max(num7, preview[num6]);
							num8 = Mathf.Min(num8, preview[num6 + 1]);
						}
						resampledPreview[2 * i * channels + j * 2] = num8;
						resampledPreview[2 * i * channels + j * 2 + 1] = num7;
					}
					num2 += deltaStep;
					num3 = num4;
				}
			}
			else
			{
				for (int k = 0; k < details.previewPixelsToRender; k++)
				{
					int num9 = (int)(num2 - 1.0);
					int num10 = num9 + 1;
					float num11 = (float)(num2 - 1.0 - (double)num9);
					for (int l = 0; l < channels; l++)
					{
						int num12 = (2 * num9 * channels + l * 2) % num;
						float num13 = preview[num12];
						float num14 = preview[num12 + 1];
						int num15 = (2 * num10 * channels + l * 2) % num;
						float num16 = preview[num15];
						float num17 = preview[num15 + 1];
						resampledPreview[2 * k * channels + l * 2] = num11 * num17 + (1f - num11) * num14;
						resampledPreview[2 * k * channels + l * 2 + 1] = num11 * num16 + (1f - num11) * num13;
					}
					num2 += deltaStep;
				}
			}
		}

		private bool OnNewWaveformData(WaveformStreamer streamer, float[] data, int remaining)
		{
			StreamedAudioClipPreview.StreamingContext streamingContext = this.m_Contexts[streamer];
			int num = streamingContext.index / this.m_Clip.channels;
			for (int i = 0; i < this.m_StreamedSegments.Length; i++)
			{
				if (this.m_StreamedSegments[i].streamer == streamer && num >= this.m_StreamedSegments[i].streamingIndexOffset && this.m_StreamedSegments[i].segmentLength > num - this.m_StreamedSegments[i].streamingIndexOffset)
				{
					this.SetMMWaveData((this.m_StreamedSegments[i].textureOffset - this.m_StreamedSegments[i].streamingIndexOffset) * this.m_Clip.channels + streamingContext.index, data);
				}
			}
			streamingContext.index += data.Length / 2;
			return remaining != 0;
		}
	}
}
