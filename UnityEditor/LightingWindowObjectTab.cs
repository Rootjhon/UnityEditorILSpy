using System;
using System.Linq;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;
using UnityEngineInternal;

namespace UnityEditor
{
	internal class LightingWindowObjectTab
	{
		private class Styles
		{
			public static readonly GUIContent[] ObjectPreviewTextureOptions = new GUIContent[]
			{
				EditorGUIUtility.TextContent("UV Charts"),
				EditorGUIUtility.TextContent("Realtime Albedo"),
				EditorGUIUtility.TextContent("Realtime Emissive"),
				EditorGUIUtility.TextContent("Realtime Indirect"),
				EditorGUIUtility.TextContent("Realtime Directionality"),
				EditorGUIUtility.TextContent("Baked Lightmap"),
				EditorGUIUtility.TextContent("Baked Directionality"),
				EditorGUIUtility.TextContent("Baked Shadowmask"),
				EditorGUIUtility.TextContent("Baked Albedo"),
				EditorGUIUtility.TextContent("Baked Emissive"),
				EditorGUIUtility.TextContent("Baked UV Charts"),
				EditorGUIUtility.TextContent("Baked Texel Validity")
			};

			public static readonly GUIContent TextureNotAvailableRealtime = EditorGUIUtility.TextContent("The texture is not available at the moment.");

			public static readonly GUIContent TextureNotAvailableBaked = EditorGUIUtility.TextContent("The texture is not available at the moment.\nPlease try to rebake the current scene or turn on Auto, and make sure that this object is set to Lightmap Static if it's meant to be baked.");

			public static readonly GUIContent TextureNotAvailableBakedShadowmask = EditorGUIUtility.TextContent("The texture is not available at the moment.\nPlease make sure that Mixed Lights affect this GameObject and that it is set to Lightmap Static.");

			public static readonly GUIContent TextureLoading = EditorGUIUtility.TextContent("Loading...");
		}

		private GITextureType[] kObjectPreviewTextureTypes = new GITextureType[]
		{
			GITextureType.Charting,
			GITextureType.Albedo,
			GITextureType.Emissive,
			GITextureType.Irradiance,
			GITextureType.Directionality,
			GITextureType.Baked,
			GITextureType.BakedDirectional,
			GITextureType.BakedShadowMask,
			GITextureType.BakedAlbedo,
			GITextureType.BakedEmissive,
			GITextureType.BakedCharting,
			GITextureType.BakedTexelValidity
		};

		private ZoomableArea m_ZoomablePreview;

		private GUIContent m_SelectedObjectPreviewTexture;

		private int m_PreviousSelection;

		private AnimBool m_ShowClampedSize = new AnimBool();

		private VisualisationGITexture m_CachedTexture;

		public void OnEnable(EditorWindow window)
		{
			this.m_ShowClampedSize.value = false;
			this.m_ShowClampedSize.valueChanged.AddListener(new UnityAction(window.Repaint));
		}

		public void OnDisable()
		{
		}

		public void ObjectPreview(Rect r)
		{
			if (r.height > 0f)
			{
				if (this.m_ZoomablePreview == null)
				{
					this.m_ZoomablePreview = new ZoomableArea(true);
					this.m_ZoomablePreview.hRangeMin = 0f;
					this.m_ZoomablePreview.vRangeMin = 0f;
					this.m_ZoomablePreview.hRangeMax = 1f;
					this.m_ZoomablePreview.vRangeMax = 1f;
					this.m_ZoomablePreview.SetShownHRange(0f, 1f);
					this.m_ZoomablePreview.SetShownVRange(0f, 1f);
					this.m_ZoomablePreview.uniformScale = true;
					this.m_ZoomablePreview.scaleWithWindow = true;
				}
				GUI.Box(r, "", "PreBackground");
				Rect position = new Rect(r);
				position.y += 1f;
				position.height = 18f;
				GUI.Box(position, "", EditorStyles.toolbar);
				Rect position2 = new Rect(r);
				position2.y += 1f;
				position2.height = 18f;
				position2.width = 120f;
				Rect rect = new Rect(r);
				rect.yMin += position2.height;
				rect.yMax -= 14f;
				rect.width -= 11f;
				int num = Array.IndexOf<GUIContent>(LightingWindowObjectTab.Styles.ObjectPreviewTextureOptions, this.m_SelectedObjectPreviewTexture);
				if (num < 0 || !LightmapVisualizationUtility.IsTextureTypeEnabled(this.kObjectPreviewTextureTypes[num]))
				{
					num = 0;
					this.m_SelectedObjectPreviewTexture = LightingWindowObjectTab.Styles.ObjectPreviewTextureOptions[num];
				}
				if (EditorGUI.DropdownButton(position2, this.m_SelectedObjectPreviewTexture, FocusType.Passive, EditorStyles.toolbarPopup))
				{
					GenericMenu genericMenu = new GenericMenu();
					for (int i = 0; i < LightingWindowObjectTab.Styles.ObjectPreviewTextureOptions.Length; i++)
					{
						if (LightmapVisualizationUtility.IsTextureTypeEnabled(this.kObjectPreviewTextureTypes[i]))
						{
							genericMenu.AddItem(LightingWindowObjectTab.Styles.ObjectPreviewTextureOptions[i], num == i, new GenericMenu.MenuFunction2(this.SelectPreviewTextureOption), LightingWindowObjectTab.Styles.ObjectPreviewTextureOptions.ElementAt(i));
						}
						else
						{
							genericMenu.AddDisabledItem(LightingWindowObjectTab.Styles.ObjectPreviewTextureOptions.ElementAt(i));
						}
					}
					genericMenu.DropDown(position2);
				}
				GITextureType gITextureType = this.kObjectPreviewTextureTypes[Array.IndexOf<GUIContent>(LightingWindowObjectTab.Styles.ObjectPreviewTextureOptions, this.m_SelectedObjectPreviewTexture)];
				if (this.m_CachedTexture.type != gITextureType || this.m_CachedTexture.contentHash != LightmapVisualizationUtility.GetSelectedObjectGITextureHash(gITextureType) || this.m_CachedTexture.contentHash == default(Hash128))
				{
					this.m_CachedTexture = LightmapVisualizationUtility.GetSelectedObjectGITexture(gITextureType);
				}
				if (this.m_CachedTexture.textureAvailability == GITextureAvailability.GITextureNotAvailable || this.m_CachedTexture.textureAvailability == GITextureAvailability.GITextureUnknown)
				{
					if (LightmapVisualizationUtility.IsBakedTextureType(gITextureType))
					{
						if (gITextureType == GITextureType.BakedShadowMask)
						{
							GUI.Label(rect, LightingWindowObjectTab.Styles.TextureNotAvailableBakedShadowmask);
						}
						else
						{
							GUI.Label(rect, LightingWindowObjectTab.Styles.TextureNotAvailableBaked);
						}
					}
					else
					{
						GUI.Label(rect, LightingWindowObjectTab.Styles.TextureNotAvailableRealtime);
					}
				}
				else if (this.m_CachedTexture.textureAvailability == GITextureAvailability.GITextureLoading && this.m_CachedTexture.texture == null)
				{
					GUI.Label(rect, LightingWindowObjectTab.Styles.TextureLoading);
				}
				else
				{
					LightmapType lightmapType = LightmapVisualizationUtility.GetLightmapType(gITextureType);
					Event current = Event.current;
					EventType type = current.type;
					if (type != EventType.ValidateCommand && type != EventType.ExecuteCommand)
					{
						if (type == EventType.Repaint)
						{
							Texture2D texture = this.m_CachedTexture.texture;
							if (texture && Event.current.type == EventType.Repaint)
							{
								Rect rect2 = new Rect(0f, 0f, (float)texture.width, (float)texture.height);
								rect2 = this.ResizeRectToFit(rect2, rect);
								rect2 = this.CenterToRect(rect2, rect);
								rect2 = this.ScaleRectByZoomableArea(rect2, this.m_ZoomablePreview);
								Rect position3 = new Rect(rect2);
								position3.x += 3f;
								position3.y += rect.y + 20f;
								Rect drawableArea = new Rect(rect);
								drawableArea.y += position2.height + 3f;
								float num2 = drawableArea.y - 14f;
								position3.y -= num2;
								drawableArea.y -= num2;
								FilterMode filterMode = texture.filterMode;
								texture.filterMode = FilterMode.Point;
								LightmapVisualizationUtility.DrawTextureWithUVOverlay(texture, Selection.activeGameObject, drawableArea, position3, gITextureType);
								texture.filterMode = filterMode;
							}
						}
					}
					else if (Event.current.commandName == "FrameSelected")
					{
						Vector4 lightmapTilingOffset = LightmapVisualizationUtility.GetLightmapTilingOffset(lightmapType);
						Vector2 vector = new Vector2(lightmapTilingOffset.z, lightmapTilingOffset.w);
						Vector2 lhs = vector + new Vector2(lightmapTilingOffset.x, lightmapTilingOffset.y);
						vector = Vector2.Max(vector, Vector2.zero);
						lhs = Vector2.Min(lhs, Vector2.one);
						float y = 1f - vector.y;
						vector.y = 1f - lhs.y;
						lhs.y = y;
						Rect shownArea = new Rect(vector.x, vector.y, lhs.x - vector.x, lhs.y - vector.y);
						shownArea.x -= Mathf.Clamp(shownArea.height - shownArea.width, 0f, 3.40282347E+38f) / 2f;
						shownArea.y -= Mathf.Clamp(shownArea.width - shownArea.height, 0f, 3.40282347E+38f) / 2f;
						float num3 = Mathf.Max(shownArea.width, shownArea.height);
						shownArea.height = num3;
						shownArea.width = num3;
						this.m_ZoomablePreview.shownArea = shownArea;
						Event.current.Use();
					}
					if (this.m_PreviousSelection != Selection.activeInstanceID)
					{
						this.m_PreviousSelection = Selection.activeInstanceID;
						this.m_ZoomablePreview.SetShownHRange(0f, 1f);
						this.m_ZoomablePreview.SetShownVRange(0f, 1f);
					}
					Rect rect3 = new Rect(r);
					rect3.yMin += position2.height;
					this.m_ZoomablePreview.rect = rect3;
					this.m_ZoomablePreview.BeginViewGUI();
					this.m_ZoomablePreview.EndViewGUI();
					GUILayoutUtility.GetRect(r.width, r.height);
				}
			}
		}

		private void SelectPreviewTextureOption(object textureOption)
		{
			this.m_SelectedObjectPreviewTexture = (GUIContent)textureOption;
		}

		private Rect ResizeRectToFit(Rect rect, Rect to)
		{
			float a = to.width / rect.width;
			float b = to.height / rect.height;
			float num = Mathf.Min(a, b);
			float width = (float)((int)Mathf.Round(rect.width * num));
			float height = (float)((int)Mathf.Round(rect.height * num));
			return new Rect(rect.x, rect.y, width, height);
		}

		private Rect CenterToRect(Rect rect, Rect to)
		{
			float num = Mathf.Clamp((float)((int)(to.width - rect.width)) / 2f, 0f, 2.14748365E+09f);
			float num2 = Mathf.Clamp((float)((int)(to.height - rect.height)) / 2f, 0f, 2.14748365E+09f);
			return new Rect(rect.x + num, rect.y + num2, rect.width, rect.height);
		}

		private Rect ScaleRectByZoomableArea(Rect rect, ZoomableArea zoomableArea)
		{
			float num = -(zoomableArea.shownArea.x / zoomableArea.shownArea.width) * rect.width;
			float num2 = (zoomableArea.shownArea.y - (1f - zoomableArea.shownArea.height)) / zoomableArea.shownArea.height * rect.height;
			float width = rect.width / zoomableArea.shownArea.width;
			float height = rect.height / zoomableArea.shownArea.height;
			return new Rect(rect.x + num, rect.y + num2, width, height);
		}
	}
}
