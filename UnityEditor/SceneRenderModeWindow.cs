using System;
using UnityEngine;

namespace UnityEditor
{
	internal class SceneRenderModeWindow : PopupWindowContent
	{
		private class Styles
		{
			public static readonly GUIStyle sMenuItem = "MenuItem";

			public static readonly GUIStyle sSeparator = "sv_iconselector_sep";

			public static readonly GUIContent sShadedHeader = EditorGUIUtility.TextContent("Shading Mode");

			public static readonly GUIContent sMiscellaneous = EditorGUIUtility.TextContent("Miscellaneous");

			public static readonly GUIContent sDeferredHeader = EditorGUIUtility.TextContent("Deferred");

			public static readonly GUIContent sGlobalIlluminationHeader = EditorGUIUtility.TextContent("Global Illumination");

			public static readonly GUIContent sRealtimeGIHeader = EditorGUIUtility.TextContent("Realtime Global Illumination");

			public static readonly GUIContent sBakedGIHeader = EditorGUIUtility.TextContent("Baked Global Illumination");

			public static readonly GUIContent sMaterialValidationHeader = EditorGUIUtility.TextContent("Material Validation");

			public static readonly GUIContent sResolutionToggle = EditorGUIUtility.TextContent("Show Lightmap Resolution");

			public static DrawCameraMode[] sRenderModeUIOrder = new DrawCameraMode[]
			{
				DrawCameraMode.Textured,
				DrawCameraMode.Wireframe,
				DrawCameraMode.TexturedWire,
				DrawCameraMode.ShadowCascades,
				DrawCameraMode.RenderPaths,
				DrawCameraMode.AlphaChannel,
				DrawCameraMode.Overdraw,
				DrawCameraMode.Mipmaps,
				DrawCameraMode.SpriteMask,
				DrawCameraMode.DeferredDiffuse,
				DrawCameraMode.DeferredSpecular,
				DrawCameraMode.DeferredSmoothness,
				DrawCameraMode.DeferredNormal,
				DrawCameraMode.Systems,
				DrawCameraMode.Clustering,
				DrawCameraMode.LitClustering,
				DrawCameraMode.RealtimeCharting,
				DrawCameraMode.RealtimeAlbedo,
				DrawCameraMode.RealtimeEmissive,
				DrawCameraMode.RealtimeIndirect,
				DrawCameraMode.RealtimeDirectionality,
				DrawCameraMode.BakedLightmap,
				DrawCameraMode.BakedDirectionality,
				DrawCameraMode.ShadowMasks,
				DrawCameraMode.BakedAlbedo,
				DrawCameraMode.BakedEmissive,
				DrawCameraMode.BakedCharting,
				DrawCameraMode.BakedTexelValidity,
				DrawCameraMode.BakedIndices,
				DrawCameraMode.LightOverlap,
				DrawCameraMode.ValidateAlbedo,
				DrawCameraMode.ValidateMetalSpecular
			};

			public static readonly GUIContent[] sRenderModeOptions = new GUIContent[]
			{
				EditorGUIUtility.TextContent("Shaded"),
				EditorGUIUtility.TextContent("Wireframe"),
				EditorGUIUtility.TextContent("Shaded Wireframe"),
				EditorGUIUtility.TextContent("Shadow Cascades"),
				EditorGUIUtility.TextContent("Render Paths"),
				EditorGUIUtility.TextContent("Alpha Channel"),
				EditorGUIUtility.TextContent("Overdraw"),
				EditorGUIUtility.TextContent("Mipmaps"),
				EditorGUIUtility.TextContent("Albedo"),
				EditorGUIUtility.TextContent("Specular"),
				EditorGUIUtility.TextContent("Smoothness"),
				EditorGUIUtility.TextContent("Normal"),
				EditorGUIUtility.TextContent("UV Charts"),
				EditorGUIUtility.TextContent("Systems"),
				EditorGUIUtility.TextContent("Albedo"),
				EditorGUIUtility.TextContent("Emissive"),
				EditorGUIUtility.TextContent("Indirect"),
				EditorGUIUtility.TextContent("Directionality"),
				EditorGUIUtility.TextContent("Baked Lightmap"),
				EditorGUIUtility.TextContent("Clustering"),
				EditorGUIUtility.TextContent("Lit Clustering"),
				EditorGUIUtility.TextContent("Validate Albedo"),
				EditorGUIUtility.TextContent("Validate Metal Specular"),
				EditorGUIUtility.TextContent("Shadowmask"),
				EditorGUIUtility.TextContent("Light Overlap"),
				EditorGUIUtility.TextContent("Albedo"),
				EditorGUIUtility.TextContent("Emissive"),
				EditorGUIUtility.TextContent("Directionality"),
				EditorGUIUtility.TextContent("Texel Validity"),
				EditorGUIUtility.TextContent("Lightmap Indices"),
				EditorGUIUtility.TextContent("UV Charts"),
				EditorGUIUtility.TextContent("Sprite Mask")
			};
		}

		private readonly float m_WindowHeight = (float)SceneRenderModeWindow.sMenuRowCount * 16f + 15f + 22f;

		private const float m_WindowWidth = 205f;

		private static readonly int sRenderModeCount = SceneRenderModeWindow.Styles.sRenderModeOptions.Length;

		private static readonly int sMenuRowCount = SceneRenderModeWindow.sRenderModeCount + 7;

		private const int kMenuHeaderCount = 7;

		private const float kSeparatorHeight = 3f;

		private const float kFrameWidth = 1f;

		private const float kHeaderHorizontalPadding = 5f;

		private const float kHeaderVerticalPadding = 1f;

		private const float kShowLightmapResolutionHeight = 22f;

		private const float kTogglePadding = 7f;

		private readonly SceneView m_SceneView;

		public SceneRenderModeWindow(SceneView sceneView)
		{
			this.m_SceneView = sceneView;
		}

		public override Vector2 GetWindowSize()
		{
			return new Vector2(205f, this.m_WindowHeight);
		}

		public override void OnGUI(Rect rect)
		{
			if (!(this.m_SceneView == null) && this.m_SceneView.sceneViewState != null)
			{
				if (Event.current.type != EventType.Layout)
				{
					this.Draw(base.editorWindow, rect.width);
					if (Event.current.type == EventType.MouseMove)
					{
						Event.current.Use();
					}
					if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
					{
						base.editorWindow.Close();
						GUIUtility.ExitGUI();
					}
				}
			}
		}

		private void DrawSeparator(ref Rect rect)
		{
			Rect position = rect;
			position.x += 5f;
			position.y += 3f;
			position.width -= 10f;
			position.height = 3f;
			GUI.Label(position, GUIContent.none, SceneRenderModeWindow.Styles.sSeparator);
			rect.y += 3f;
		}

		private void DrawHeader(ref Rect rect, GUIContent label)
		{
			Rect position = rect;
			position.y += 1f;
			position.x += 5f;
			position.width = EditorStyles.miniLabel.CalcSize(label).x;
			position.height = EditorStyles.miniLabel.CalcSize(label).y;
			GUI.Label(position, label, EditorStyles.miniLabel);
			rect.y += 16f;
		}

		private void Draw(EditorWindow caller, float listElementWidth)
		{
			Rect rect = new Rect(0f, 0f, listElementWidth, 16f);
			this.DrawHeader(ref rect, SceneRenderModeWindow.Styles.sShadedHeader);
			for (int i = 0; i < SceneRenderModeWindow.sRenderModeCount; i++)
			{
				DrawCameraMode drawCameraMode = SceneRenderModeWindow.Styles.sRenderModeUIOrder[i];
				switch (drawCameraMode)
				{
				case DrawCameraMode.Systems:
					this.DrawSeparator(ref rect);
					this.DrawHeader(ref rect, SceneRenderModeWindow.Styles.sGlobalIlluminationHeader);
					goto IL_10D;
				case DrawCameraMode.RealtimeAlbedo:
					this.DrawSeparator(ref rect);
					this.DrawHeader(ref rect, SceneRenderModeWindow.Styles.sRealtimeGIHeader);
					goto IL_10D;
				case DrawCameraMode.RealtimeEmissive:
				case DrawCameraMode.RealtimeIndirect:
				case DrawCameraMode.RealtimeDirectionality:
					IL_56:
					if (drawCameraMode == DrawCameraMode.ShadowCascades)
					{
						this.DrawSeparator(ref rect);
						this.DrawHeader(ref rect, SceneRenderModeWindow.Styles.sMiscellaneous);
						goto IL_10D;
					}
					if (drawCameraMode == DrawCameraMode.DeferredDiffuse)
					{
						this.DrawSeparator(ref rect);
						this.DrawHeader(ref rect, SceneRenderModeWindow.Styles.sDeferredHeader);
						goto IL_10D;
					}
					if (drawCameraMode != DrawCameraMode.ValidateAlbedo)
					{
						goto IL_10D;
					}
					this.DrawSeparator(ref rect);
					this.DrawHeader(ref rect, SceneRenderModeWindow.Styles.sMaterialValidationHeader);
					goto IL_10D;
				case DrawCameraMode.BakedLightmap:
					this.DrawSeparator(ref rect);
					this.DrawHeader(ref rect, SceneRenderModeWindow.Styles.sBakedGIHeader);
					goto IL_10D;
				}
				goto IL_56;
				IL_10D:
				using (new EditorGUI.DisabledScope(!this.IsModeEnabled(drawCameraMode)))
				{
					this.DoOneMode(caller, ref rect, drawCameraMode);
				}
			}
			bool disabled = this.m_SceneView.renderMode < DrawCameraMode.RealtimeCharting || !this.IsModeEnabled(this.m_SceneView.renderMode);
			this.DoResolutionToggle(rect, disabled);
		}

		private bool IsModeEnabled(DrawCameraMode mode)
		{
			return this.m_SceneView.IsCameraDrawModeEnabled(mode);
		}

		private void DoResolutionToggle(Rect rect, bool disabled)
		{
			GUI.Label(new Rect(1f, rect.y, 203f, 22f), "", EditorStyles.inspectorBig);
			rect.y += 3f;
			rect.x += 7f;
			using (new EditorGUI.DisabledScope(disabled))
			{
				EditorGUI.BeginChangeCheck();
				bool showResolution = GUI.Toggle(rect, LightmapVisualization.showResolution, SceneRenderModeWindow.Styles.sResolutionToggle);
				if (EditorGUI.EndChangeCheck())
				{
					LightmapVisualization.showResolution = showResolution;
					SceneView.RepaintAll();
				}
			}
		}

		private void DoOneMode(EditorWindow caller, ref Rect rect, DrawCameraMode drawCameraMode)
		{
			using (new EditorGUI.DisabledScope(!this.m_SceneView.CheckDrawModeForRenderingPath(drawCameraMode)))
			{
				EditorGUI.BeginChangeCheck();
				GUI.Toggle(rect, this.m_SceneView.renderMode == drawCameraMode, SceneRenderModeWindow.GetGUIContent(drawCameraMode), SceneRenderModeWindow.Styles.sMenuItem);
				if (EditorGUI.EndChangeCheck())
				{
					this.m_SceneView.renderMode = drawCameraMode;
					this.m_SceneView.Repaint();
					GUIUtility.ExitGUI();
				}
				rect.y += 16f;
			}
		}

		public static GUIContent GetGUIContent(DrawCameraMode drawCameraMode)
		{
			return SceneRenderModeWindow.Styles.sRenderModeOptions[(int)drawCameraMode];
		}
	}
}
