using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(Tilemap))]
	internal class TilemapEditor : Editor
	{
		private static class Styles
		{
			public static readonly GUIContent animationFrameRateLabel = EditorGUIUtility.TextContent("Animation Frame Rate|Frame rate for playing animated tiles in the tilemap");

			public static readonly GUIContent tilemapColorLabel = EditorGUIUtility.TextContent("Color|Color tinting all Sprites from tiles in the tilemap");

			public static readonly GUIContent tileAnchorLabel = EditorGUIUtility.TextContent("Tile Anchor|Anchoring position for Sprites from tiles in the tilemap");

			public static readonly GUIContent orientationLabel = EditorGUIUtility.TextContent("Orientation|Orientation for tiles in the tilemap");
		}

		private SerializedProperty m_AnimationFrameRate;

		private SerializedProperty m_TilemapColor;

		private SerializedProperty m_TileAnchor;

		private SerializedProperty m_Orientation;

		private SerializedProperty m_OrientationMatrix;

		private Tilemap tilemap
		{
			get
			{
				return base.target as Tilemap;
			}
		}

		private void OnEnable()
		{
			this.m_AnimationFrameRate = base.serializedObject.FindProperty("m_AnimationFrameRate");
			this.m_TilemapColor = base.serializedObject.FindProperty("m_Color");
			this.m_TileAnchor = base.serializedObject.FindProperty("m_TileAnchor");
			this.m_Orientation = base.serializedObject.FindProperty("m_TileOrientation");
			this.m_OrientationMatrix = base.serializedObject.FindProperty("m_TileOrientationMatrix");
		}

		private void OnDisable()
		{
			if (this.tilemap != null)
			{
				this.tilemap.ClearAllEditorPreviewTiles();
			}
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.UpdateIfRequiredOrScript();
			EditorGUILayout.PropertyField(this.m_AnimationFrameRate, TilemapEditor.Styles.animationFrameRateLabel, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_TilemapColor, TilemapEditor.Styles.tilemapColorLabel, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_TileAnchor, TilemapEditor.Styles.tileAnchorLabel, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_Orientation, TilemapEditor.Styles.orientationLabel, new GUILayoutOption[0]);
			GUI.enabled = (!this.m_Orientation.hasMultipleDifferentValues && Tilemap.Orientation.Custom == this.tilemap.orientation);
			if (base.targets.Length > 1)
			{
				EditorGUILayout.PropertyField(this.m_OrientationMatrix, true, new GUILayoutOption[0]);
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				Matrix4x4 orientationMatrix = TileEditor.TransformMatrixOnGUI(this.tilemap.orientationMatrix);
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(this.tilemap, "Tilemap property change");
					this.tilemap.orientationMatrix = orientationMatrix;
				}
			}
			GUI.enabled = true;
			base.serializedObject.ApplyModifiedProperties();
		}

		[MenuItem("GameObject/2D Object/Tilemap")]
		internal static void CreateRectangularTilemap()
		{
			GameObject gameObject = TilemapEditor.FindOrCreateRootGrid();
			string uniqueNameForSibling = GameObjectUtility.GetUniqueNameForSibling(gameObject.transform, "Tilemap");
			GameObject gameObject2 = new GameObject(uniqueNameForSibling, new Type[]
			{
				typeof(Tilemap),
				typeof(TilemapRenderer)
			});
			gameObject2.transform.SetParent(gameObject.transform);
			gameObject2.transform.position = Vector3.zero;
			Undo.RegisterCreatedObjectUndo(gameObject2, "Create Tilemap");
		}

		private static GameObject FindOrCreateRootGrid()
		{
			GameObject gameObject = null;
			if (Selection.activeObject is GameObject)
			{
				GameObject gameObject2 = (GameObject)Selection.activeObject;
				Grid componentInParent = gameObject2.GetComponentInParent<Grid>();
				if (componentInParent != null)
				{
					gameObject = componentInParent.gameObject;
				}
			}
			if (gameObject == null)
			{
				gameObject = new GameObject("Grid", new Type[]
				{
					typeof(Grid)
				});
				gameObject.transform.position = Vector3.zero;
				Grid component = gameObject.GetComponent<Grid>();
				component.cellSize = new Vector3(1f, 1f, 0f);
				Undo.RegisterCreatedObjectUndo(gameObject, "Create Grid");
			}
			return gameObject;
		}

		[MenuItem("CONTEXT/Tilemap/Refresh All Tiles")]
		internal static void RefreshAllTiles(MenuCommand item)
		{
			Tilemap tilemap = (Tilemap)item.context;
			tilemap.RefreshAllTiles();
			InternalEditorUtility.RepaintAllViews();
		}

		[MenuItem("CONTEXT/Tilemap/Compress Tilemap Bounds")]
		internal static void CompressBounds(MenuCommand item)
		{
			Tilemap tilemap = (Tilemap)item.context;
			tilemap.CompressBounds();
		}
	}
}
