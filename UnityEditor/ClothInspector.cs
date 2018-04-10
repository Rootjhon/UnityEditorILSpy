using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(Cloth))]
	internal class ClothInspector : Editor
	{
		public enum DrawMode
		{
			MaxDistance = 1,
			CollisionSphereDistance
		}

		public enum ToolMode
		{
			Select,
			Paint
		}

		public enum CollToolMode
		{
			Select,
			Paint,
			Erase
		}

		private enum RectSelectionMode
		{
			Replace,
			Add,
			Substract
		}

		public enum CollisionVisualizationMode
		{
			SelfCollision,
			InterCollision
		}

		private static class Styles
		{
			public static readonly GUIContent editConstraintsLabel;

			public static readonly GUIContent editSelfInterCollisionLabel;

			public static readonly GUIContent selfInterCollisionParticleColor;

			public static readonly GUIContent selfInterCollisionBrushColor;

			public static readonly GUIContent clothSelfCollisionAndInterCollision;

			public static readonly GUIContent paintCollisionParticles;

			public static readonly GUIContent selectCollisionParticles;

			public static readonly GUIContent brushRadiusString;

			public static readonly GUIContent selfAndInterCollisionMode;

			public static readonly GUIContent backFaceManipulationMode;

			public static readonly GUIContent manipulateBackFaceString;

			public static readonly GUIContent selfCollisionString;

			public static readonly GUIContent setSelfAndInterCollisionString;

			public static readonly int clothEditorWindowWidth;

			public static GUIContent[] toolContents;

			public static GUIContent[] toolIcons;

			public static GUIContent[] drawModeStrings;

			public static GUIContent[] toolModeStrings;

			public static GUIContent[] collToolModeIcons;

			public static GUIContent[] collVisModeStrings;

			public static GUIContent paintIcon;

			public static EditMode.SceneViewEditMode[] sceneViewEditModes;

			public static GUIContent selfCollisionDistanceGUIContent;

			public static GUIContent selfCollisionStiffnessGUIContent;

			static Styles()
			{
				ClothInspector.Styles.editConstraintsLabel = EditorGUIUtility.TextContent("Edit Constraints");
				ClothInspector.Styles.editSelfInterCollisionLabel = EditorGUIUtility.TextContent("Edit Collision Particles");
				ClothInspector.Styles.selfInterCollisionParticleColor = EditorGUIUtility.TextContent("Visualization Color");
				ClothInspector.Styles.selfInterCollisionBrushColor = EditorGUIUtility.TextContent("Brush Color");
				ClothInspector.Styles.clothSelfCollisionAndInterCollision = EditorGUIUtility.TextContent("Cloth Self-Collision And Inter-Collision");
				ClothInspector.Styles.paintCollisionParticles = EditorGUIUtility.TextContent("Paint Collision Particles");
				ClothInspector.Styles.selectCollisionParticles = EditorGUIUtility.TextContent("Select Collision Particles");
				ClothInspector.Styles.brushRadiusString = EditorGUIUtility.TextContent("Brush Radius");
				ClothInspector.Styles.selfAndInterCollisionMode = EditorGUIUtility.TextContent("Paint or Select Particles");
				ClothInspector.Styles.backFaceManipulationMode = EditorGUIUtility.TextContent("Back Face Manipulation");
				ClothInspector.Styles.manipulateBackFaceString = EditorGUIUtility.TextContent("Manipulate Backfaces");
				ClothInspector.Styles.selfCollisionString = EditorGUIUtility.TextContent("Self Collision");
				ClothInspector.Styles.setSelfAndInterCollisionString = EditorGUIUtility.TextContent("Self-Collision and Inter-Collision");
				ClothInspector.Styles.clothEditorWindowWidth = 300;
				ClothInspector.Styles.toolContents = new GUIContent[]
				{
					EditorGUIUtility.IconContent("EditCollider"),
					EditorGUIUtility.IconContent("EditCollider")
				};
				ClothInspector.Styles.toolIcons = new GUIContent[]
				{
					EditorGUIUtility.TextContent("Select"),
					EditorGUIUtility.TextContent("Paint")
				};
				ClothInspector.Styles.drawModeStrings = new GUIContent[]
				{
					EditorGUIUtility.TextContent("Fixed"),
					EditorGUIUtility.TextContent("Max Distance"),
					EditorGUIUtility.TextContent("Surface Penetration")
				};
				ClothInspector.Styles.toolModeStrings = new GUIContent[]
				{
					EditorGUIUtility.TextContent("Select"),
					EditorGUIUtility.TextContent("Paint"),
					EditorGUIUtility.TextContent("Erase")
				};
				ClothInspector.Styles.collToolModeIcons = new GUIContent[]
				{
					EditorGUIUtility.TextContent("Select"),
					EditorGUIUtility.TextContent("Paint"),
					EditorGUIUtility.TextContent("Erase")
				};
				ClothInspector.Styles.collVisModeStrings = new GUIContent[]
				{
					EditorGUIUtility.TextContent("Self Collision"),
					EditorGUIUtility.TextContent("Inter Collision")
				};
				ClothInspector.Styles.paintIcon = EditorGUIUtility.IconContent("ClothInspector.PaintValue", "Change this vertex coefficient value by painting in the scene view.");
				ClothInspector.Styles.sceneViewEditModes = new EditMode.SceneViewEditMode[]
				{
					EditMode.SceneViewEditMode.ClothConstraints,
					EditMode.SceneViewEditMode.ClothSelfAndInterCollisionParticles
				};
				ClothInspector.Styles.selfCollisionDistanceGUIContent = EditorGUIUtility.TextContent("Self Collision Distance");
				ClothInspector.Styles.selfCollisionStiffnessGUIContent = EditorGUIUtility.TextContent("Self Collision Stiffness");
				ClothInspector.Styles.toolContents[0].tooltip = EditorGUIUtility.TextContent("Edit cloth constraints").text;
				ClothInspector.Styles.toolContents[1].tooltip = EditorGUIUtility.TextContent("Edit cloth self or inter collision").text;
				ClothInspector.Styles.toolIcons[0].tooltip = EditorGUIUtility.TextContent("Select cloth particles for use in self or inter collision").text;
				ClothInspector.Styles.toolIcons[1].tooltip = EditorGUIUtility.TextContent("Paint cloth particles for use in self or inter collision").text;
				ClothInspector.Styles.collToolModeIcons[0].tooltip = EditorGUIUtility.TextContent("Select cloth particles.").text;
				ClothInspector.Styles.collToolModeIcons[1].tooltip = EditorGUIUtility.TextContent("Paint cloth particles.").text;
				ClothInspector.Styles.collToolModeIcons[2].tooltip = EditorGUIUtility.TextContent("Erase cloth particles.").text;
			}
		}

		private bool[] m_ParticleSelection;

		private bool[] m_ParticleRectSelection;

		private bool[] m_SelfAndInterCollisionSelection;

		private Vector3[] m_ClothParticlesInWorldSpace;

		private Vector3 m_BrushPos;

		private Vector3 m_BrushNorm;

		private int m_BrushFace = -1;

		private int m_MouseOver = -1;

		private Vector3[] m_LastVertices;

		private Vector2 m_SelectStartPoint;

		private Vector2 m_SelectMousePoint;

		private bool m_RectSelecting = false;

		private bool m_DidSelect = false;

		private float[] m_MaxVisualizedValue = new float[3];

		private float[] m_MinVisualizedValue = new float[3];

		private ClothInspector.RectSelectionMode m_RectSelectionMode = ClothInspector.RectSelectionMode.Add;

		private int m_NumVerts = 0;

		private const float kDisabledValue = 3.40282347E+38f;

		private static Texture2D s_ColorTexture = null;

		private static bool s_BrushCreated = false;

		public static PrefColor s_BrushColor = new PrefColor("Cloth/Brush Color 2", 0f, 0f, 0f, 0.2f);

		public static PrefColor s_SelfAndInterCollisionParticleColor = new PrefColor("Cloth/Self or Inter Collision Particle Color 2", 0.5686275f, 0.956862748f, 0.545098066f, 0.5f);

		public static PrefColor s_UnselectedSelfAndInterCollisionParticleColor = new PrefColor("Cloth/Unselected Self or Inter Collision Particle Color 2", 0.1f, 0.1f, 0.1f, 0.5f);

		public static PrefColor s_SelectedParticleColor = new PrefColor("Cloth/Selected Self or Inter Collision Particle Color 2", 0.2509804f, 0.627451f, 1f, 0.5f);

		public static ClothInspector.ToolMode[] s_ToolMode;

		private SerializedProperty m_SelfCollisionDistance;

		private SerializedProperty m_SelfCollisionStiffness;

		private int m_NumSelection = 0;

		private SkinnedMeshRenderer m_SkinnedMeshRenderer;

		private ClothInspectorState state
		{
			get
			{
				return ScriptableSingleton<ClothInspectorState>.instance;
			}
		}

		private ClothInspector.DrawMode drawMode
		{
			get
			{
				return this.state.DrawMode;
			}
			set
			{
				if (this.state.DrawMode != value)
				{
					this.state.DrawMode = value;
					base.Repaint();
				}
			}
		}

		private Cloth cloth
		{
			get
			{
				return (Cloth)base.target;
			}
		}

		public bool editingConstraints
		{
			get
			{
				return EditMode.editMode == EditMode.SceneViewEditMode.ClothConstraints && EditMode.IsOwner(this);
			}
		}

		public bool editingSelfAndInterCollisionParticles
		{
			get
			{
				return EditMode.editMode == EditMode.SceneViewEditMode.ClothSelfAndInterCollisionParticles && EditMode.IsOwner(this);
			}
		}

		private GUIContent GetDrawModeString(ClothInspector.DrawMode mode)
		{
			return ClothInspector.Styles.drawModeStrings[(int)mode];
		}

		private GUIContent GetCollVisModeString(ClothInspector.CollisionVisualizationMode mode)
		{
			return ClothInspector.Styles.collVisModeStrings[(int)mode];
		}

		private bool IsMeshValid()
		{
			bool result;
			if (this.cloth.vertices.Length != this.m_NumVerts)
			{
				this.InitInspector();
				result = true;
			}
			else
			{
				result = (this.m_NumVerts != 0);
			}
			return result;
		}

		private Texture2D GenerateColorTexture(int width)
		{
			Texture2D texture2D = new Texture2D(width, 1, TextureFormat.RGBA32, false);
			texture2D.hideFlags = HideFlags.HideAndDontSave;
			texture2D.wrapMode = TextureWrapMode.Clamp;
			texture2D.hideFlags = HideFlags.DontSave;
			Color[] array = new Color[width];
			for (int i = 0; i < width; i++)
			{
				array[i] = this.GetGradientColor((float)i / (float)(width - 1));
			}
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		public override void OnInspectorGUI()
		{
			base.serializedObject.Update();
			if (base.targets.Length <= 1)
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				GUILayout.FlexibleSpace();
				EditMode.DoInspectorToolbar(ClothInspector.Styles.sceneViewEditModes, ClothInspector.Styles.toolContents, this);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			if (this.editingSelfAndInterCollisionParticles)
			{
				if (this.state.SetSelfAndInterCollision || this.state.CollToolMode == ClothInspector.CollToolMode.Paint || this.state.CollToolMode == ClothInspector.CollToolMode.Erase)
				{
					if (this.cloth.selfCollisionDistance > 0f)
					{
						this.state.SelfCollisionDistance = this.cloth.selfCollisionDistance;
						this.m_SelfCollisionDistance.floatValue = this.cloth.selfCollisionDistance;
					}
					else
					{
						this.cloth.selfCollisionDistance = this.state.SelfCollisionDistance;
						this.m_SelfCollisionDistance.floatValue = this.state.SelfCollisionDistance;
					}
					if (this.cloth.selfCollisionStiffness > 0f)
					{
						this.state.SelfCollisionStiffness = this.cloth.selfCollisionStiffness;
						this.m_SelfCollisionStiffness.floatValue = this.cloth.selfCollisionStiffness;
					}
					else
					{
						this.cloth.selfCollisionStiffness = this.state.SelfCollisionStiffness;
						this.m_SelfCollisionStiffness.floatValue = this.cloth.selfCollisionStiffness;
					}
					Rect rect = GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, new GUILayoutOption[]
					{
						GUILayout.ExpandWidth(true),
						GUILayout.Height(17f)
					});
					EditorGUI.LabelField(rect, ClothInspector.Styles.selfCollisionString, EditorStyles.boldLabel);
					Rect rect2 = GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, new GUILayoutOption[]
					{
						GUILayout.ExpandWidth(true),
						GUILayout.Height(17f)
					});
					EditorGUI.PropertyField(rect2, this.m_SelfCollisionDistance, ClothInspector.Styles.selfCollisionDistanceGUIContent);
					Rect rect3 = GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, new GUILayoutOption[]
					{
						GUILayout.ExpandWidth(true),
						GUILayout.Height(17f)
					});
					EditorGUI.PropertyField(rect3, this.m_SelfCollisionStiffness, ClothInspector.Styles.selfCollisionStiffnessGUIContent);
					GUILayout.Space(10f);
				}
				if (Physics.interCollisionDistance > 0f)
				{
					this.state.InterCollisionDistance = Physics.interCollisionDistance;
				}
				else
				{
					Physics.interCollisionDistance = this.state.InterCollisionDistance;
				}
				if (Physics.interCollisionStiffness > 0f)
				{
					this.state.InterCollisionStiffness = Physics.interCollisionStiffness;
				}
				else
				{
					Physics.interCollisionStiffness = this.state.InterCollisionStiffness;
				}
			}
			Editor.DrawPropertiesExcluding(base.serializedObject, new string[]
			{
				"m_SelfAndInterCollisionIndices",
				"m_VirtualParticleIndices",
				"m_SelfCollisionDistance",
				"m_SelfCollisionStiffness"
			});
			base.serializedObject.ApplyModifiedProperties();
			MeshRenderer component = this.cloth.GetComponent<MeshRenderer>();
			if (component != null)
			{
				Debug.LogWarning("MeshRenderer will not work with a cloth component! Use only SkinnedMeshRenderer. Any MeshRenderer's attached to a cloth component will be deleted at runtime.");
			}
		}

		public bool Raycast(out Vector3 pos, out Vector3 norm, out int face)
		{
			Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			GameObject gameObject = this.cloth.gameObject;
			MeshCollider component = gameObject.GetComponent<MeshCollider>();
			RaycastHit raycastHit;
			bool result;
			if (component.Raycast(ray, out raycastHit, float.PositiveInfinity))
			{
				norm = raycastHit.normal;
				pos = raycastHit.point;
				face = raycastHit.triangleIndex;
				result = true;
			}
			else
			{
				norm = Vector2.zero;
				pos = Vector3.zero;
				face = -1;
				result = false;
			}
			return result;
		}

		private void UpdatePreviewBrush()
		{
			this.Raycast(out this.m_BrushPos, out this.m_BrushNorm, out this.m_BrushFace);
		}

		private void DrawBrush()
		{
			if (this.m_BrushFace >= 0)
			{
				Handles.color = ClothInspector.s_BrushColor;
				Handles.DrawSolidDisc(this.m_BrushPos, this.m_BrushNorm, this.state.BrushRadius);
			}
		}

		internal override Bounds GetWorldBoundsOfTarget(UnityEngine.Object targetObject)
		{
			Cloth cloth = (Cloth)targetObject;
			SkinnedMeshRenderer component = cloth.GetComponent<SkinnedMeshRenderer>();
			return (!(component == null)) ? component.bounds : base.GetWorldBoundsOfTarget(targetObject);
		}

		private bool SelectionMeshDirty()
		{
			bool result;
			if (this.m_LastVertices != null)
			{
				Vector3[] vertices = this.cloth.vertices;
				Transform actualRootBone = this.m_SkinnedMeshRenderer.actualRootBone;
				if (this.m_LastVertices.Length != vertices.Length)
				{
					result = true;
					return result;
				}
				for (int i = 0; i < this.m_LastVertices.Length; i++)
				{
					Vector3 rhs = actualRootBone.rotation * vertices[i] + actualRootBone.position;
					if (!(this.m_LastVertices[i] == rhs))
					{
						result = true;
						return result;
					}
				}
			}
			result = false;
			return result;
		}

		private void GenerateSelectionMesh()
		{
			if (this.IsMeshValid())
			{
				Vector3[] vertices = this.cloth.vertices;
				int num = vertices.Length;
				this.m_ParticleSelection = new bool[num];
				this.m_ParticleRectSelection = new bool[num];
				this.m_LastVertices = new Vector3[num];
				Transform actualRootBone = this.m_SkinnedMeshRenderer.actualRootBone;
				for (int i = 0; i < num; i++)
				{
					this.m_LastVertices[i] = actualRootBone.rotation * vertices[i] + actualRootBone.position;
				}
			}
		}

		private void InitSelfAndInterCollisionSelection()
		{
			int num = this.cloth.vertices.Length;
			this.m_SelfAndInterCollisionSelection = new bool[num];
			for (int i = 0; i < num; i++)
			{
				this.m_SelfAndInterCollisionSelection[i] = false;
			}
			List<uint> list = new List<uint>(num);
			list.Clear();
			this.cloth.GetSelfAndInterCollisionIndices(list);
			num = list.Count;
			for (int j = 0; j < num; j++)
			{
				this.m_SelfAndInterCollisionSelection[(int)((UIntPtr)list[j])] = true;
			}
		}

		private void InitClothParticlesInWorldSpace()
		{
			Vector3[] vertices = this.cloth.vertices;
			int num = vertices.Length;
			this.m_ClothParticlesInWorldSpace = new Vector3[num];
			Transform actualRootBone = this.m_SkinnedMeshRenderer.actualRootBone;
			Quaternion rotation = actualRootBone.rotation;
			Vector3 position = actualRootBone.position;
			for (int i = 0; i < num; i++)
			{
				this.m_ClothParticlesInWorldSpace[i] = rotation * vertices[i] + position;
			}
		}

		private void DrawSelfAndInterCollisionParticles()
		{
			Transform actualRootBone = this.m_SkinnedMeshRenderer.actualRootBone;
			Vector3[] vertices = this.cloth.vertices;
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			float size = this.state.SelfCollisionDistance;
			if (this.state.VisualizeSelfOrInterCollision == ClothInspector.CollisionVisualizationMode.SelfCollision)
			{
				size = this.state.SelfCollisionDistance;
			}
			else if (this.state.VisualizeSelfOrInterCollision == ClothInspector.CollisionVisualizationMode.InterCollision)
			{
				size = this.state.InterCollisionDistance;
			}
			int num = this.m_SelfAndInterCollisionSelection.Length;
			for (int i = 0; i < num; i++)
			{
				Vector3 vector = this.m_ClothParticlesInWorldSpace[i] - this.m_BrushPos;
				bool flag = Vector3.Dot(actualRootBone.rotation * this.cloth.normals[i], Camera.current.transform.forward) <= 0f;
				if (flag || this.state.ManipulateBackfaces)
				{
					if (this.m_SelfAndInterCollisionSelection[i] && !this.m_ParticleSelection[i])
					{
						Handles.color = ClothInspector.s_SelfAndInterCollisionParticleColor;
					}
					else if (!this.m_SelfAndInterCollisionSelection[i] && !this.m_ParticleSelection[i])
					{
						Handles.color = ClothInspector.s_UnselectedSelfAndInterCollisionParticleColor;
					}
					if (this.m_ParticleSelection[i] && this.m_NumSelection > 0 && this.state.CollToolMode == ClothInspector.CollToolMode.Select)
					{
						Handles.color = ClothInspector.s_SelectedParticleColor;
					}
					if (vector.magnitude < this.state.BrushRadius && flag && (this.state.CollToolMode == ClothInspector.CollToolMode.Paint || this.state.CollToolMode == ClothInspector.CollToolMode.Erase))
					{
						Handles.color = ClothInspector.s_SelectedParticleColor;
					}
					Handles.SphereHandleCap(controlID, this.m_ClothParticlesInWorldSpace[i], actualRootBone.rotation, size, EventType.Repaint);
				}
			}
		}

		private void InitInspector()
		{
			this.InitBrushCollider();
			this.InitSelfAndInterCollisionSelection();
			this.InitClothParticlesInWorldSpace();
			this.m_NumVerts = this.cloth.vertices.Length;
		}

		private void OnEnable()
		{
			if (ClothInspector.s_ColorTexture == null)
			{
				ClothInspector.s_ColorTexture = this.GenerateColorTexture(100);
			}
			this.m_SkinnedMeshRenderer = this.cloth.GetComponent<SkinnedMeshRenderer>();
			this.InitInspector();
			this.GenerateSelectionMesh();
			this.m_SelfCollisionDistance = base.serializedObject.FindProperty("m_SelfCollisionDistance");
			this.m_SelfCollisionStiffness = base.serializedObject.FindProperty("m_SelfCollisionStiffness");
			SceneView.onPreSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Combine(SceneView.onPreSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnPreSceneGUICallback));
		}

		private void InitBrushCollider()
		{
			if (this.cloth != null)
			{
				GameObject gameObject = this.cloth.gameObject;
				MeshCollider component = gameObject.GetComponent<MeshCollider>();
				if (component != null)
				{
					if (component.hideFlags == (HideFlags.HideInHierarchy | HideFlags.HideInInspector))
					{
						UnityEngine.Object.DestroyImmediate(component);
					}
				}
				MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
				meshCollider.hideFlags = (HideFlags.HideInHierarchy | HideFlags.HideInInspector);
				meshCollider.sharedMesh = this.m_SkinnedMeshRenderer.sharedMesh;
				ClothInspector.s_BrushCreated = true;
			}
		}

		public void OnDestroy()
		{
			SceneView.onPreSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Remove(SceneView.onPreSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnPreSceneGUICallback));
			if (ClothInspector.s_BrushCreated)
			{
				if (this.cloth != null)
				{
					GameObject gameObject = this.cloth.gameObject;
					MeshCollider component = gameObject.GetComponent<MeshCollider>();
					if (component != null)
					{
						if (component.hideFlags == (HideFlags.HideInHierarchy | HideFlags.HideInInspector))
						{
							UnityEngine.Object.DestroyImmediate(component);
						}
					}
					ClothInspector.s_BrushCreated = false;
				}
			}
		}

		private float GetCoefficient(ClothSkinningCoefficient coefficient)
		{
			ClothInspector.DrawMode drawMode = this.drawMode;
			float result;
			if (drawMode != ClothInspector.DrawMode.MaxDistance)
			{
				if (drawMode != ClothInspector.DrawMode.CollisionSphereDistance)
				{
					result = 0f;
				}
				else
				{
					result = coefficient.collisionSphereDistance;
				}
			}
			else
			{
				result = coefficient.maxDistance;
			}
			return result;
		}

		private Color GetGradientColor(float val)
		{
			Color result;
			if (val < 0.3f)
			{
				result = Color.Lerp(Color.red, Color.magenta, val / 0.2f);
			}
			else if (val < 0.7f)
			{
				result = Color.Lerp(Color.magenta, Color.yellow, (val - 0.2f) / 0.5f);
			}
			else
			{
				result = Color.Lerp(Color.yellow, Color.green, (val - 0.7f) / 0.3f);
			}
			return result;
		}

		private void OnDisable()
		{
			SceneView.onPreSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Remove(SceneView.onPreSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnPreSceneGUICallback));
		}

		private float CoefficientField(float value, float useValue, bool enabled, ClothInspector.DrawMode mode)
		{
			GUIContent drawModeString = this.GetDrawModeString(mode);
			using (new EditorGUI.DisabledScope(!enabled))
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				EditorGUI.showMixedValue = (useValue < 0f);
				EditorGUI.BeginChangeCheck();
				useValue = (float)((!EditorGUILayout.Toggle(GUIContent.none, useValue != 0f, new GUILayoutOption[0])) ? 0 : 1);
				if (EditorGUI.EndChangeCheck())
				{
					if (useValue > 0f)
					{
						value = 0f;
					}
					else
					{
						value = 3.40282347E+38f;
					}
					this.drawMode = mode;
				}
				GUILayout.Space(-152f);
				EditorGUI.showMixedValue = false;
				using (new EditorGUI.DisabledScope(useValue != 1f))
				{
					float num = value;
					EditorGUI.showMixedValue = (value < 0f);
					EditorGUI.BeginChangeCheck();
					int keyboardControl = GUIUtility.keyboardControl;
					if (useValue > 0f)
					{
						num = EditorGUILayout.FloatField(drawModeString, value, new GUILayoutOption[0]);
					}
					else
					{
						EditorGUILayout.FloatField(drawModeString, 0f, new GUILayoutOption[0]);
					}
					bool flag = EditorGUI.EndChangeCheck();
					if (flag)
					{
						value = num;
						if (value < 0f)
						{
							value = 0f;
						}
					}
					if (flag || keyboardControl != GUIUtility.keyboardControl)
					{
						this.drawMode = mode;
					}
				}
			}
			if (useValue > 0f)
			{
				float num2 = this.m_MinVisualizedValue[(int)mode];
				float num3 = this.m_MaxVisualizedValue[(int)mode];
				if (num3 - num2 > 0f)
				{
					this.DrawColorBox(null, this.GetGradientColor((value - num2) / (num3 - num2)));
				}
				else
				{
					this.DrawColorBox(null, this.GetGradientColor((float)((value > num2) ? 1 : 0)));
				}
			}
			else
			{
				this.DrawColorBox(null, Color.black);
			}
			EditorGUI.showMixedValue = false;
			GUILayout.EndHorizontal();
			return value;
		}

		private float PaintField(float value, ref bool enabled, ClothInspector.DrawMode mode)
		{
			GUIContent drawModeString = this.GetDrawModeString(mode);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			enabled = GUILayout.Toggle(enabled, ClothInspector.Styles.paintIcon, "MiniButton", new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(false)
			});
			bool flag;
			float num;
			using (new EditorGUI.DisabledScope(!enabled))
			{
				EditorGUI.BeginChangeCheck();
				flag = EditorGUILayout.Toggle(GUIContent.none, value < 3.40282347E+38f, new GUILayoutOption[0]);
				if (EditorGUI.EndChangeCheck())
				{
					if (flag)
					{
						value = 0f;
					}
					else
					{
						value = 3.40282347E+38f;
					}
					this.drawMode = mode;
				}
				GUILayout.Space(-162f);
				using (new EditorGUI.DisabledScope(!flag))
				{
					num = value;
					int keyboardControl = GUIUtility.keyboardControl;
					EditorGUI.BeginChangeCheck();
					if (flag)
					{
						num = EditorGUILayout.FloatField(drawModeString, value, new GUILayoutOption[0]);
					}
					else
					{
						EditorGUILayout.FloatField(drawModeString, 0f, new GUILayoutOption[0]);
					}
					if (num < 0f)
					{
						num = 0f;
					}
					if (EditorGUI.EndChangeCheck() || keyboardControl != GUIUtility.keyboardControl)
					{
						this.drawMode = mode;
					}
				}
			}
			if (flag)
			{
				float num2 = this.m_MinVisualizedValue[(int)mode];
				float num3 = this.m_MaxVisualizedValue[(int)mode];
				if (num3 - num2 > 0f)
				{
					this.DrawColorBox(null, this.GetGradientColor((value - num2) / (num3 - num2)));
				}
				else
				{
					this.DrawColorBox(null, this.GetGradientColor((float)((value > num2) ? 1 : 0)));
				}
			}
			else
			{
				this.DrawColorBox(null, Color.black);
			}
			GUILayout.EndHorizontal();
			return num;
		}

		private void SelectionGUI()
		{
			ClothSkinningCoefficient[] coefficients = this.cloth.coefficients;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			int num5 = 0;
			bool flag = true;
			for (int i = 0; i < this.m_ParticleSelection.Length; i++)
			{
				if (this.m_ParticleSelection[i])
				{
					if (flag)
					{
						num = coefficients[i].maxDistance;
						num2 = (float)((num >= 3.40282347E+38f) ? 0 : 1);
						num3 = coefficients[i].collisionSphereDistance;
						num4 = (float)((num3 >= 3.40282347E+38f) ? 0 : 1);
						flag = false;
					}
					if (coefficients[i].maxDistance != num)
					{
						num = -1f;
					}
					if (coefficients[i].collisionSphereDistance != num3)
					{
						num3 = -1f;
					}
					if (num2 != (float)((coefficients[i].maxDistance >= 3.40282347E+38f) ? 0 : 1))
					{
						num2 = -1f;
					}
					if (num4 != (float)((coefficients[i].collisionSphereDistance >= 3.40282347E+38f) ? 0 : 1))
					{
						num4 = -1f;
					}
					num5++;
				}
			}
			float num6 = this.CoefficientField(num, num2, num5 > 0, ClothInspector.DrawMode.MaxDistance);
			if (num6 != num)
			{
				for (int j = 0; j < coefficients.Length; j++)
				{
					if (this.m_ParticleSelection[j])
					{
						coefficients[j].maxDistance = num6;
					}
				}
				this.cloth.coefficients = coefficients;
				Undo.RegisterCompleteObjectUndo(base.target, "Change Cloth Coefficients");
			}
			float num7 = this.CoefficientField(num3, num4, num5 > 0, ClothInspector.DrawMode.CollisionSphereDistance);
			if (num7 != num3)
			{
				for (int k = 0; k < coefficients.Length; k++)
				{
					if (this.m_ParticleSelection[k])
					{
						coefficients[k].collisionSphereDistance = num7;
					}
				}
				this.cloth.coefficients = coefficients;
				Undo.RegisterCompleteObjectUndo(base.target, "Change Cloth Coefficients");
			}
			using (new EditorGUI.DisabledScope(true))
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				if (num5 > 0)
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label(num5 + " selected", new GUILayoutOption[0]);
				}
				else
				{
					GUILayout.Label("Select cloth vertices to edit their constraints.", new GUILayoutOption[0]);
					GUILayout.FlexibleSpace();
				}
				GUILayout.EndHorizontal();
			}
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Backspace)
			{
				for (int l = 0; l < coefficients.Length; l++)
				{
					if (this.m_ParticleSelection[l])
					{
						ClothInspector.DrawMode drawMode = this.drawMode;
						if (drawMode != ClothInspector.DrawMode.MaxDistance)
						{
							if (drawMode == ClothInspector.DrawMode.CollisionSphereDistance)
							{
								coefficients[l].collisionSphereDistance = 3.40282347E+38f;
							}
						}
						else
						{
							coefficients[l].maxDistance = 3.40282347E+38f;
						}
					}
				}
				this.cloth.coefficients = coefficients;
			}
		}

		private void CollSelectionGUI()
		{
			if (this.IsMeshValid())
			{
				bool flag = false;
				bool showMixedValue = false;
				int num = 0;
				int num2 = this.m_ParticleRectSelection.Length;
				for (int i = 0; i < num2; i++)
				{
					if (this.m_ParticleRectSelection[i])
					{
						if (!flag)
						{
							this.state.SetSelfAndInterCollision = this.m_SelfAndInterCollisionSelection[i];
							flag = true;
						}
						else if (this.state.SetSelfAndInterCollision != this.m_SelfAndInterCollisionSelection[i])
						{
							showMixedValue = true;
							this.state.SetSelfAndInterCollision = false;
						}
						num++;
					}
				}
				this.m_NumSelection = num;
				if (this.m_NumSelection == 0)
				{
					this.state.SetSelfAndInterCollision = false;
				}
				using (new EditorGUI.DisabledScope(this.m_NumSelection == 0))
				{
					EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
					EditorGUI.showMixedValue = showMixedValue;
					EditorGUI.BeginChangeCheck();
					bool setSelfAndInterCollision = EditorGUILayout.Toggle(GUIContent.none, this.state.SetSelfAndInterCollision, new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck())
					{
						this.state.SetSelfAndInterCollision = setSelfAndInterCollision;
						for (int j = 0; j < num2; j++)
						{
							if (this.m_ParticleRectSelection[j])
							{
								this.m_SelfAndInterCollisionSelection[j] = this.state.SetSelfAndInterCollision;
							}
						}
						Undo.RegisterCompleteObjectUndo(base.target, "Change Cloth Particles Selected for self or inter collision");
					}
					EditorGUILayout.LabelField(ClothInspector.Styles.setSelfAndInterCollisionString, new GUILayoutOption[0]);
					EditorGUI.showMixedValue = false;
					EditorGUILayout.EndHorizontal();
				}
			}
		}

		private void EditBrushSize()
		{
			EditorGUI.BeginChangeCheck();
			float brushRadius = EditorGUILayout.FloatField(ClothInspector.Styles.brushRadiusString, this.state.BrushRadius, new GUILayoutOption[0]);
			bool flag = EditorGUI.EndChangeCheck();
			if (flag)
			{
				this.state.BrushRadius = brushRadius;
				if (this.state.BrushRadius < 0f)
				{
					this.state.BrushRadius = 0f;
				}
			}
		}

		private void PaintGUI()
		{
			this.state.PaintMaxDistance = this.PaintField(this.state.PaintMaxDistance, ref this.state.PaintMaxDistanceEnabled, ClothInspector.DrawMode.MaxDistance);
			this.state.PaintCollisionSphereDistance = this.PaintField(this.state.PaintCollisionSphereDistance, ref this.state.PaintCollisionSphereDistanceEnabled, ClothInspector.DrawMode.CollisionSphereDistance);
			if (this.state.PaintMaxDistanceEnabled && !this.state.PaintCollisionSphereDistanceEnabled)
			{
				this.drawMode = ClothInspector.DrawMode.MaxDistance;
			}
			else if (!this.state.PaintMaxDistanceEnabled && this.state.PaintCollisionSphereDistanceEnabled)
			{
				this.drawMode = ClothInspector.DrawMode.CollisionSphereDistance;
			}
			using (new EditorGUI.DisabledScope(true))
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				GUILayout.Label("Set constraints to paint onto cloth vertices.", new GUILayoutOption[0]);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			this.EditBrushSize();
		}

		private int GetMouseVertex(Event e)
		{
			int result;
			if (Tools.current != Tool.None)
			{
				result = -1;
			}
			else if (this.m_LastVertices == null)
			{
				result = -1;
			}
			else
			{
				Vector3[] normals = this.cloth.normals;
				ClothSkinningCoefficient[] coefficients = this.cloth.coefficients;
				Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
				float num = 1000f;
				int num2 = -1;
				Quaternion rotation = this.m_SkinnedMeshRenderer.actualRootBone.rotation;
				for (int i = 0; i < coefficients.Length; i++)
				{
					Vector3 lhs = this.m_LastVertices[i] - ray.origin;
					float sqrMagnitude = Vector3.Cross(lhs, ray.direction).sqrMagnitude;
					if ((Vector3.Dot(rotation * normals[i], Camera.current.transform.forward) <= 0f || this.state.ManipulateBackfaces) && sqrMagnitude < num && sqrMagnitude < 0.00250000018f)
					{
						num = sqrMagnitude;
						num2 = i;
					}
				}
				result = num2;
			}
			return result;
		}

		private void DrawConstraints()
		{
			if (this.SelectionMeshDirty())
			{
				this.GenerateSelectionMesh();
			}
			Transform actualRootBone = this.m_SkinnedMeshRenderer.actualRootBone;
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			ClothSkinningCoefficient[] coefficients = this.cloth.coefficients;
			int num = coefficients.Length;
			float num2 = 0f;
			float num3 = 0f;
			for (int i = 0; i < num; i++)
			{
				float coefficient = this.GetCoefficient(coefficients[i]);
				if (coefficient < 3.40282347E+38f)
				{
					if (coefficient < num2)
					{
						num2 = coefficient;
					}
					if (coefficient > num3)
					{
						num3 = coefficient;
					}
				}
			}
			this.m_MaxVisualizedValue[(int)this.drawMode] = num3;
			this.m_MinVisualizedValue[(int)this.drawMode] = num2;
			Vector3[] normals = this.cloth.normals;
			for (int j = 0; j < num; j++)
			{
				bool flag = Vector3.Dot(actualRootBone.rotation * normals[j], Camera.current.transform.forward) <= 0f;
				if (flag || this.state.ManipulateBackfaces)
				{
					float num4 = this.GetCoefficient(coefficients[j]);
					Color color;
					if (num4 >= 3.40282347E+38f)
					{
						color = Color.black;
					}
					else
					{
						if (num3 - num2 != 0f)
						{
							num4 = (num4 - num2) / (num3 - num2);
						}
						else
						{
							num4 = 0f;
						}
						color = this.GetGradientColor(num4);
					}
					Handles.color = color;
					Vector3 vector = this.m_ClothParticlesInWorldSpace[j] - this.m_BrushPos;
					if (this.m_ParticleSelection[j] && this.state.CollToolMode == ClothInspector.CollToolMode.Select)
					{
						Handles.color = ClothInspector.s_SelectedParticleColor;
					}
					if (vector.magnitude < this.state.BrushRadius && flag && this.state.ToolMode == ClothInspector.ToolMode.Paint)
					{
						Handles.color = ClothInspector.s_SelectedParticleColor;
					}
					Handles.SphereHandleCap(controlID, this.m_ClothParticlesInWorldSpace[j], actualRootBone.rotation, this.state.ConstraintSize, EventType.Repaint);
				}
			}
		}

		private bool UpdateRectParticleSelection()
		{
			bool result;
			if (!this.IsMeshValid())
			{
				result = false;
			}
			else
			{
				bool flag = false;
				Vector3[] normals = this.cloth.normals;
				ClothSkinningCoefficient[] coefficients = this.cloth.coefficients;
				float x = Mathf.Min(this.m_SelectStartPoint.x, this.m_SelectMousePoint.x);
				float x2 = Mathf.Max(this.m_SelectStartPoint.x, this.m_SelectMousePoint.x);
				float y = Mathf.Min(this.m_SelectStartPoint.y, this.m_SelectMousePoint.y);
				float y2 = Mathf.Max(this.m_SelectStartPoint.y, this.m_SelectMousePoint.y);
				Ray ray = HandleUtility.GUIPointToWorldRay(new Vector2(x, y));
				Ray ray2 = HandleUtility.GUIPointToWorldRay(new Vector2(x2, y));
				Ray ray3 = HandleUtility.GUIPointToWorldRay(new Vector2(x, y2));
				Ray ray4 = HandleUtility.GUIPointToWorldRay(new Vector2(x2, y2));
				Plane plane = new Plane(ray2.origin + ray2.direction, ray.origin + ray.direction, ray.origin);
				Plane plane2 = new Plane(ray3.origin + ray3.direction, ray4.origin + ray4.direction, ray4.origin);
				Plane plane3 = new Plane(ray.origin + ray.direction, ray3.origin + ray3.direction, ray3.origin);
				Plane plane4 = new Plane(ray4.origin + ray4.direction, ray2.origin + ray2.direction, ray2.origin);
				Quaternion rotation = this.m_SkinnedMeshRenderer.actualRootBone.rotation;
				int num = coefficients.Length;
				for (int i = 0; i < num; i++)
				{
					Vector3 point = this.m_LastVertices[i];
					bool flag2 = Vector3.Dot(rotation * normals[i], Camera.current.transform.forward) <= 0f;
					bool flag3 = plane.GetSide(point) && plane2.GetSide(point) && plane3.GetSide(point) && plane4.GetSide(point);
					flag3 = (flag3 && (this.state.ManipulateBackfaces || flag2));
					if (this.m_ParticleRectSelection[i] != flag3)
					{
						this.m_ParticleRectSelection[i] = flag3;
						flag = true;
					}
				}
				result = flag;
			}
			return result;
		}

		private void ApplyRectSelection()
		{
			if (this.IsMeshValid())
			{
				int num = this.cloth.coefficients.Length;
				for (int i = 0; i < num; i++)
				{
					ClothInspector.RectSelectionMode rectSelectionMode = this.m_RectSelectionMode;
					if (rectSelectionMode != ClothInspector.RectSelectionMode.Replace)
					{
						if (rectSelectionMode != ClothInspector.RectSelectionMode.Add)
						{
							if (rectSelectionMode == ClothInspector.RectSelectionMode.Substract)
							{
								this.m_ParticleSelection[i] = (this.m_ParticleSelection[i] && !this.m_ParticleRectSelection[i]);
							}
						}
						else
						{
							this.m_ParticleSelection[i] |= this.m_ParticleRectSelection[i];
						}
					}
					else
					{
						this.m_ParticleSelection[i] = this.m_ParticleRectSelection[i];
					}
				}
			}
		}

		private bool RectSelectionModeFromEvent()
		{
			Event current = Event.current;
			ClothInspector.RectSelectionMode rectSelectionMode = ClothInspector.RectSelectionMode.Replace;
			if (current.shift)
			{
				rectSelectionMode = ClothInspector.RectSelectionMode.Add;
			}
			if (current.alt)
			{
				rectSelectionMode = ClothInspector.RectSelectionMode.Substract;
			}
			bool result;
			if (this.m_RectSelectionMode != rectSelectionMode)
			{
				this.m_RectSelectionMode = rectSelectionMode;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		internal void SendCommandsOnModifierKeys()
		{
			SceneView.lastActiveSceneView.SendEvent(EditorGUIUtility.CommandEvent("ModifierKeysChanged"));
		}

		private void SelectionPreSceneGUI(int id)
		{
			Event current = Event.current;
			EventType typeForControl = current.GetTypeForControl(id);
			switch (typeForControl)
			{
			case EventType.MouseDown:
			{
				if (current.alt || current.control || current.command || current.button != 0)
				{
					return;
				}
				GUIUtility.hotControl = id;
				int mouseVertex = this.GetMouseVertex(current);
				if (mouseVertex != -1)
				{
					if (current.shift)
					{
						this.m_ParticleSelection[mouseVertex] = !this.m_ParticleSelection[mouseVertex];
					}
					else
					{
						for (int i = 0; i < this.m_ParticleSelection.Length; i++)
						{
							this.m_ParticleSelection[i] = false;
						}
						this.m_ParticleSelection[mouseVertex] = true;
					}
					this.m_DidSelect = true;
					base.Repaint();
				}
				else
				{
					this.m_DidSelect = false;
				}
				this.m_SelectStartPoint = current.mousePosition;
				current.Use();
				return;
			}
			case EventType.MouseUp:
				if (GUIUtility.hotControl == id && current.button == 0)
				{
					GUIUtility.hotControl = 0;
					if (this.m_RectSelecting)
					{
						EditorApplication.modifierKeysChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.modifierKeysChanged, new EditorApplication.CallbackFunction(this.SendCommandsOnModifierKeys));
						this.m_RectSelecting = false;
						this.RectSelectionModeFromEvent();
						this.ApplyRectSelection();
					}
					else if (!this.m_DidSelect)
					{
						if (!current.alt && !current.control && !current.command)
						{
							ClothSkinningCoefficient[] coefficients = this.cloth.coefficients;
							for (int j = 0; j < coefficients.Length; j++)
							{
								this.m_ParticleSelection[j] = false;
							}
						}
					}
					GUIUtility.keyboardControl = 0;
					SceneView.RepaintAll();
				}
				return;
			case EventType.MouseMove:
				IL_25:
				if (typeForControl != EventType.ExecuteCommand)
				{
					return;
				}
				if (this.m_RectSelecting && current.commandName == "ModifierKeysChanged")
				{
					this.RectSelectionModeFromEvent();
					this.UpdateRectParticleSelection();
				}
				return;
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == id)
				{
					if (!this.m_RectSelecting && (current.mousePosition - this.m_SelectStartPoint).magnitude > 2f)
					{
						if (!current.alt && !current.control && !current.command)
						{
							EditorApplication.modifierKeysChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.modifierKeysChanged, new EditorApplication.CallbackFunction(this.SendCommandsOnModifierKeys));
							this.m_RectSelecting = true;
							this.RectSelectionModeFromEvent();
						}
					}
					if (this.m_RectSelecting)
					{
						this.m_SelectMousePoint = new Vector2(Mathf.Max(current.mousePosition.x, 0f), Mathf.Max(current.mousePosition.y, 0f));
						this.RectSelectionModeFromEvent();
						this.UpdateRectParticleSelection();
						current.Use();
					}
				}
				return;
			}
			goto IL_25;
		}

		private void GetBrushedConstraints(Event e)
		{
			if (this.IsMeshValid())
			{
				Vector3[] vertices = this.cloth.vertices;
				Vector3[] normals = this.cloth.normals;
				ClothSkinningCoefficient[] coefficients = this.cloth.coefficients;
				Quaternion rotation = this.m_SkinnedMeshRenderer.actualRootBone.rotation;
				int num = vertices.Length;
				for (int i = 0; i < num; i++)
				{
					Vector3 vector = this.m_ClothParticlesInWorldSpace[i] - this.m_BrushPos;
					bool flag = Vector3.Dot(rotation * normals[i], Camera.current.transform.forward) <= 0f;
					if (vector.magnitude < this.state.BrushRadius && (flag || this.state.ManipulateBackfaces))
					{
						bool flag2 = false;
						if (this.state.PaintMaxDistanceEnabled && coefficients[i].maxDistance != this.state.PaintMaxDistance)
						{
							coefficients[i].maxDistance = this.state.PaintMaxDistance;
							flag2 = true;
						}
						if (this.state.PaintCollisionSphereDistanceEnabled && coefficients[i].collisionSphereDistance != this.state.PaintCollisionSphereDistance)
						{
							coefficients[i].collisionSphereDistance = this.state.PaintCollisionSphereDistance;
							flag2 = true;
						}
						if (flag2)
						{
							Undo.RegisterCompleteObjectUndo(base.target, "Paint Cloth Constraints");
							this.cloth.coefficients = coefficients;
							base.Repaint();
						}
					}
				}
			}
		}

		private void GetBrushedParticles(Event e)
		{
			if (this.IsMeshValid())
			{
				Vector3[] vertices = this.cloth.vertices;
				Vector3[] normals = this.cloth.normals;
				Quaternion rotation = this.m_SkinnedMeshRenderer.actualRootBone.rotation;
				int num = vertices.Length;
				for (int i = 0; i < num; i++)
				{
					Vector3 vector = this.m_ClothParticlesInWorldSpace[i] - this.m_BrushPos;
					bool flag = Vector3.Dot(rotation * normals[i], Camera.current.transform.forward) <= 0f;
					if (vector.magnitude < this.state.BrushRadius && (flag || this.state.ManipulateBackfaces))
					{
						if (e.button == 0)
						{
							if (this.state.CollToolMode == ClothInspector.CollToolMode.Paint)
							{
								this.m_SelfAndInterCollisionSelection[i] = true;
							}
							else if (this.state.CollToolMode == ClothInspector.CollToolMode.Erase)
							{
								this.m_SelfAndInterCollisionSelection[i] = false;
							}
						}
						int controlID = GUIUtility.GetControlID(FocusType.Passive);
						float size = this.cloth.selfCollisionDistance;
						if (this.state.VisualizeSelfOrInterCollision == ClothInspector.CollisionVisualizationMode.SelfCollision)
						{
							size = this.cloth.selfCollisionDistance;
						}
						else if (this.state.VisualizeSelfOrInterCollision == ClothInspector.CollisionVisualizationMode.InterCollision)
						{
							size = Physics.interCollisionDistance;
						}
						Handles.color = ClothInspector.s_SelectedParticleColor;
						Handles.SphereHandleCap(controlID, this.m_ClothParticlesInWorldSpace[i], rotation, size, EventType.Repaint);
						base.Repaint();
					}
				}
				Undo.RegisterCompleteObjectUndo(base.target, "Paint Collision");
			}
		}

		private void PaintPreSceneGUI(int id)
		{
			if (this.IsMeshValid())
			{
				Event current = Event.current;
				EventType typeForControl = current.GetTypeForControl(id);
				if (typeForControl == EventType.MouseDown || typeForControl == EventType.MouseDrag)
				{
					ClothSkinningCoefficient[] coefficients = this.cloth.coefficients;
					if (GUIUtility.hotControl == id || (!current.alt && !current.control && !current.command && current.button == 0))
					{
						if (typeForControl == EventType.MouseDown)
						{
							GUIUtility.hotControl = id;
						}
						if (this.editingSelfAndInterCollisionParticles)
						{
							this.GetBrushedParticles(current);
						}
						if (this.editingConstraints)
						{
							this.GetBrushedConstraints(current);
						}
						current.Use();
					}
				}
				else if (typeForControl == EventType.MouseUp)
				{
					if (GUIUtility.hotControl == id && current.button == 0)
					{
						GUIUtility.hotControl = 0;
						current.Use();
					}
				}
			}
		}

		private void OnPreSceneGUICallback(SceneView sceneView)
		{
			if (base.targets.Length <= 1)
			{
				if (this.editingConstraints || this.editingSelfAndInterCollisionParticles)
				{
					this.OnPreSceneGUI();
				}
			}
		}

		private void OnPreSceneGUI()
		{
			if (this.IsMeshValid())
			{
				Tools.current = Tool.None;
				if (this.state.ToolMode == (ClothInspector.ToolMode)(-1))
				{
					this.state.ToolMode = ClothInspector.ToolMode.Select;
				}
				if (this.m_ParticleSelection == null)
				{
					this.GenerateSelectionMesh();
				}
				else
				{
					ClothSkinningCoefficient[] coefficients = this.cloth.coefficients;
					if (this.m_ParticleSelection.Length != coefficients.Length)
					{
						this.InitInspector();
					}
				}
				Handles.BeginGUI();
				int controlID = GUIUtility.GetControlID(FocusType.Passive);
				Event current = Event.current;
				EventType typeForControl = current.GetTypeForControl(controlID);
				if (typeForControl != EventType.Layout)
				{
					if (typeForControl == EventType.MouseMove || typeForControl == EventType.MouseDrag)
					{
						int mouseOver = this.m_MouseOver;
						this.m_MouseOver = this.GetMouseVertex(current);
						if (this.m_MouseOver != mouseOver)
						{
							SceneView.RepaintAll();
						}
					}
				}
				else
				{
					HandleUtility.AddDefaultControl(controlID);
				}
				if (this.editingConstraints)
				{
					ClothInspector.ToolMode toolMode = this.state.ToolMode;
					if (toolMode != ClothInspector.ToolMode.Select)
					{
						if (toolMode == ClothInspector.ToolMode.Paint)
						{
							this.PaintPreSceneGUI(controlID);
						}
					}
					else
					{
						this.SelectionPreSceneGUI(controlID);
					}
				}
				if (this.editingSelfAndInterCollisionParticles)
				{
					ClothInspector.CollToolMode collToolMode = this.state.CollToolMode;
					if (collToolMode != ClothInspector.CollToolMode.Select)
					{
						if (collToolMode == ClothInspector.CollToolMode.Paint || collToolMode == ClothInspector.CollToolMode.Erase)
						{
							this.PaintPreSceneGUI(controlID);
						}
					}
					else
					{
						this.SelectionPreSceneGUI(controlID);
					}
				}
				Handles.EndGUI();
			}
		}

		public void OnSceneGUI()
		{
			if (this.editingConstraints)
			{
				this.OnSceneEditConstraintsGUI();
			}
			else if (this.editingSelfAndInterCollisionParticles)
			{
				this.OnSceneEditSelfAndInterCollisionParticlesGUI();
			}
		}

		private void OnSceneEditConstraintsGUI()
		{
			if (Event.current.type == EventType.Repaint && this.state.ToolMode == ClothInspector.ToolMode.Paint)
			{
				this.UpdatePreviewBrush();
				this.DrawBrush();
			}
			if (Selection.gameObjects.Length <= 1)
			{
				if (Event.current.type == EventType.Repaint)
				{
					this.DrawConstraints();
				}
				Event current = Event.current;
				if (current.commandName == "SelectAll")
				{
					if (current.type == EventType.ValidateCommand)
					{
						current.Use();
					}
					if (current.type == EventType.ExecuteCommand)
					{
						int num = this.cloth.vertices.Length;
						for (int i = 0; i < num; i++)
						{
							this.m_ParticleSelection[i] = true;
						}
						SceneView.RepaintAll();
						this.state.ToolMode = ClothInspector.ToolMode.Select;
						current.Use();
					}
				}
				Handles.BeginGUI();
				if (this.m_RectSelecting && this.state.ToolMode == ClothInspector.ToolMode.Select && Event.current.type == EventType.Repaint)
				{
					EditorStyles.selectionRect.Draw(EditorGUIExt.FromToRect(this.m_SelectStartPoint, this.m_SelectMousePoint), GUIContent.none, false, false, false, false);
				}
				Handles.EndGUI();
				SceneViewOverlay.Window(new GUIContent("Cloth Constraints"), new SceneViewOverlay.WindowFunction(this.ConstraintEditing), 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTarget);
			}
		}

		private void OnSceneEditSelfAndInterCollisionParticlesGUI()
		{
			if (Selection.gameObjects.Length <= 1)
			{
				this.DrawSelfAndInterCollisionParticles();
				if (Event.current.type == EventType.Repaint && (this.state.CollToolMode == ClothInspector.CollToolMode.Paint || this.state.CollToolMode == ClothInspector.CollToolMode.Erase))
				{
					this.UpdatePreviewBrush();
					this.DrawBrush();
				}
				Handles.BeginGUI();
				if (this.m_RectSelecting && this.state.CollToolMode == ClothInspector.CollToolMode.Select && Event.current.type == EventType.Repaint)
				{
					EditorStyles.selectionRect.Draw(EditorGUIExt.FromToRect(this.m_SelectStartPoint, this.m_SelectMousePoint), GUIContent.none, false, false, false, false);
				}
				Handles.EndGUI();
				SceneViewOverlay.Window(ClothInspector.Styles.clothSelfCollisionAndInterCollision, new SceneViewOverlay.WindowFunction(this.SelfAndInterCollisionEditing), 100, SceneViewOverlay.WindowDisplayOption.OneWindowPerTarget);
			}
		}

		public void VisualizationMenuSetMaxDistanceMode()
		{
			this.drawMode = ClothInspector.DrawMode.MaxDistance;
			if (!this.state.PaintMaxDistanceEnabled)
			{
				this.state.PaintCollisionSphereDistanceEnabled = false;
				this.state.PaintMaxDistanceEnabled = true;
			}
		}

		public void VisualizationMenuSetCollisionSphereMode()
		{
			this.drawMode = ClothInspector.DrawMode.CollisionSphereDistance;
			if (!this.state.PaintCollisionSphereDistanceEnabled)
			{
				this.state.PaintCollisionSphereDistanceEnabled = true;
				this.state.PaintMaxDistanceEnabled = false;
			}
		}

		public void VisualizationMenuToggleManipulateBackfaces()
		{
			this.state.ManipulateBackfaces = !this.state.ManipulateBackfaces;
		}

		public void VisualizationMenuSelfCollision()
		{
			this.state.VisualizeSelfOrInterCollision = ClothInspector.CollisionVisualizationMode.SelfCollision;
		}

		public void VisualizationMenuInterCollision()
		{
			this.state.VisualizeSelfOrInterCollision = ClothInspector.CollisionVisualizationMode.InterCollision;
		}

		public void DrawColorBox(Texture gradientTex, Color col)
		{
			if (!GUI.enabled)
			{
				col = new Color(0.3f, 0.3f, 0.3f, 1f);
				EditorGUI.showMixedValue = false;
			}
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Space(5f);
			Rect rect = GUILayoutUtility.GetRect(new GUIContent(), GUIStyle.none, new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(true),
				GUILayout.Height(10f)
			});
			GUI.Box(rect, GUIContent.none);
			rect = new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f);
			if (gradientTex)
			{
				GUI.DrawTexture(rect, gradientTex);
			}
			else
			{
				EditorGUIUtility.DrawColorSwatch(rect, col, false);
			}
			GUILayout.EndVertical();
		}

		private bool IsConstrained()
		{
			ClothSkinningCoefficient[] coefficients = this.cloth.coefficients;
			ClothSkinningCoefficient[] array = coefficients;
			int i = 0;
			bool result;
			while (i < array.Length)
			{
				ClothSkinningCoefficient clothSkinningCoefficient = array[i];
				if (clothSkinningCoefficient.maxDistance < 3.40282347E+38f)
				{
					result = true;
				}
				else
				{
					if (clothSkinningCoefficient.collisionSphereDistance >= 3.40282347E+38f)
					{
						i++;
						continue;
					}
					result = true;
				}
				return result;
			}
			result = false;
			return result;
		}

		private void ConstraintEditing(UnityEngine.Object unused, SceneView sceneView)
		{
			GUILayout.BeginVertical(new GUILayoutOption[]
			{
				GUILayout.Width((float)ClothInspector.Styles.clothEditorWindowWidth)
			});
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Visualization ", new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(false)
			});
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			if (EditorGUILayout.DropdownButton(this.GetDrawModeString(this.drawMode), FocusType.Passive, EditorStyles.toolbarDropDown, new GUILayoutOption[0]))
			{
				Rect last = GUILayoutUtility.topLevel.GetLast();
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(this.GetDrawModeString(ClothInspector.DrawMode.MaxDistance), this.drawMode == ClothInspector.DrawMode.MaxDistance, new GenericMenu.MenuFunction(this.VisualizationMenuSetMaxDistanceMode));
				genericMenu.AddItem(this.GetDrawModeString(ClothInspector.DrawMode.CollisionSphereDistance), this.drawMode == ClothInspector.DrawMode.CollisionSphereDistance, new GenericMenu.MenuFunction(this.VisualizationMenuSetCollisionSphereMode));
				genericMenu.AddSeparator("");
				genericMenu.AddItem(new GUIContent("Manipulate Backfaces"), this.state.ManipulateBackfaces, new GenericMenu.MenuFunction(this.VisualizationMenuToggleManipulateBackfaces));
				genericMenu.DropDown(last);
			}
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label(this.m_MinVisualizedValue[(int)this.drawMode].ToString(), new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(false)
			});
			this.DrawColorBox(ClothInspector.s_ColorTexture, Color.clear);
			GUILayout.Label(this.m_MaxVisualizedValue[(int)this.drawMode].ToString(), new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(false)
			});
			GUILayout.Label("Unconstrained ", new GUILayoutOption[0]);
			GUILayout.Space(-24f);
			GUILayout.BeginHorizontal(new GUILayoutOption[]
			{
				GUILayout.Width(20f)
			});
			this.DrawColorBox(null, Color.black);
			GUILayout.EndHorizontal();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			if (Tools.current != Tool.None)
			{
				this.state.ToolMode = (ClothInspector.ToolMode)(-1);
			}
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			this.state.ToolMode = (ClothInspector.ToolMode)GUILayout.Toolbar((int)this.state.ToolMode, ClothInspector.Styles.toolIcons, new GUILayoutOption[0]);
			using (EditorGUI.ChangeCheckScope changeCheckScope = new EditorGUI.ChangeCheckScope())
			{
				if (changeCheckScope.changed)
				{
					GUIUtility.keyboardControl = 0;
					SceneView.RepaintAll();
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			ClothInspector.ToolMode toolMode = this.state.ToolMode;
			if (toolMode != ClothInspector.ToolMode.Select)
			{
				if (toolMode == ClothInspector.ToolMode.Paint)
				{
					Tools.current = Tool.None;
					this.PaintGUI();
				}
			}
			else
			{
				Tools.current = Tool.None;
				this.SelectionGUI();
			}
			if (!this.IsConstrained())
			{
				EditorGUILayout.HelpBox("No constraints have been set up, so the cloth will move freely. Set up vertex constraints here to restrict it.", MessageType.Info);
			}
			GUILayout.EndVertical();
		}

		private void SelectManipulateBackFaces()
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUI.BeginChangeCheck();
			bool manipulateBackfaces = EditorGUILayout.Toggle(GUIContent.none, this.state.ManipulateBackfaces, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				this.state.ManipulateBackfaces = manipulateBackfaces;
			}
			EditorGUILayout.LabelField(ClothInspector.Styles.manipulateBackFaceString, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
		}

		private void ResetParticleSelection()
		{
			int num = this.m_ParticleRectSelection.Length;
			for (int i = 0; i < num; i++)
			{
				this.m_ParticleRectSelection[i] = false;
				this.m_ParticleSelection[i] = false;
			}
		}

		private void SelfAndInterCollisionEditing(UnityEngine.Object unused, SceneView sceneView)
		{
			GUILayout.BeginVertical(new GUILayoutOption[]
			{
				GUILayout.Width((float)ClothInspector.Styles.clothEditorWindowWidth)
			});
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Visualization ", new GUILayoutOption[]
			{
				GUILayout.ExpandWidth(false)
			});
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			if (EditorGUILayout.DropdownButton(this.GetCollVisModeString(this.state.VisualizeSelfOrInterCollision), FocusType.Passive, EditorStyles.toolbarDropDown, new GUILayoutOption[0]))
			{
				Rect last = GUILayoutUtility.topLevel.GetLast();
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(this.GetCollVisModeString(ClothInspector.CollisionVisualizationMode.SelfCollision), this.state.VisualizeSelfOrInterCollision == ClothInspector.CollisionVisualizationMode.SelfCollision, new GenericMenu.MenuFunction(this.VisualizationMenuSelfCollision));
				genericMenu.AddItem(this.GetCollVisModeString(ClothInspector.CollisionVisualizationMode.InterCollision), this.state.VisualizeSelfOrInterCollision == ClothInspector.CollisionVisualizationMode.InterCollision, new GenericMenu.MenuFunction(this.VisualizationMenuInterCollision));
				genericMenu.DropDown(last);
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			if (Tools.current != Tool.None)
			{
				this.state.ToolMode = (ClothInspector.ToolMode)(-1);
			}
			ClothInspector.CollToolMode collToolMode = this.state.CollToolMode;
			this.state.CollToolMode = (ClothInspector.CollToolMode)GUILayout.Toolbar((int)this.state.CollToolMode, ClothInspector.Styles.collToolModeIcons, new GUILayoutOption[0]);
			if (this.state.CollToolMode != collToolMode)
			{
				GUIUtility.keyboardControl = 0;
				SceneView.RepaintAll();
			}
			ClothInspector.CollToolMode collToolMode2 = this.state.CollToolMode;
			if (collToolMode2 != ClothInspector.CollToolMode.Select)
			{
				if (collToolMode2 == ClothInspector.CollToolMode.Paint || collToolMode2 == ClothInspector.CollToolMode.Erase)
				{
					Tools.current = Tool.None;
					this.ResetParticleSelection();
					this.EditBrushSize();
				}
			}
			else
			{
				Tools.current = Tool.None;
				this.CollSelectionGUI();
			}
			this.SelectManipulateBackFaces();
			int num = 0;
			int num2 = this.m_SelfAndInterCollisionSelection.Length;
			for (int i = 0; i < num2; i++)
			{
				if (this.m_SelfAndInterCollisionSelection[i])
				{
					num++;
				}
			}
			if (num > 0)
			{
				List<uint> list = new List<uint>(num);
				list.Clear();
				uint num3 = 0u;
				while ((ulong)num3 < (ulong)((long)num2))
				{
					if (this.m_SelfAndInterCollisionSelection[(int)((UIntPtr)num3)])
					{
						list.Add(num3);
					}
					num3 += 1u;
				}
				this.cloth.SetSelfAndInterCollisionIndices(list);
			}
			GUILayout.EndVertical();
		}

		static ClothInspector()
		{
			// Note: this type is marked as 'beforefieldinit'.
			ClothInspector.ToolMode[] expr_9E = new ClothInspector.ToolMode[2];
			expr_9E[0] = ClothInspector.ToolMode.Paint;
			ClothInspector.s_ToolMode = expr_9E;
		}
	}
}
