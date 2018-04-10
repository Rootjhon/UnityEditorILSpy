using System;
using UnityEngine;

namespace UnityEditor
{
	internal class ClothInspectorState : ScriptableSingleton<ClothInspectorState>
	{
		[SerializeField]
		public ClothInspector.DrawMode DrawMode = ClothInspector.DrawMode.MaxDistance;

		[SerializeField]
		public bool ManipulateBackfaces = false;

		[SerializeField]
		public bool PaintMaxDistanceEnabled = true;

		[SerializeField]
		public bool PaintCollisionSphereDistanceEnabled = false;

		[SerializeField]
		public float PaintMaxDistance = 0.2f;

		[SerializeField]
		public float PaintCollisionSphereDistance = 0f;

		[SerializeField]
		public ClothInspector.ToolMode ToolMode = ClothInspector.ToolMode.Paint;

		[SerializeField]
		public ClothInspector.CollToolMode CollToolMode = ClothInspector.CollToolMode.Select;

		[SerializeField]
		public float BrushRadius = 0.075f;

		[SerializeField]
		public bool SetSelfAndInterCollision = false;

		[SerializeField]
		public ClothInspector.CollisionVisualizationMode VisualizeSelfOrInterCollision = ClothInspector.CollisionVisualizationMode.SelfCollision;

		[SerializeField]
		public float SelfCollisionDistance = 0.1f;

		[SerializeField]
		public float SelfCollisionStiffness = 0.2f;

		[SerializeField]
		public float InterCollisionDistance = 0.1f;

		[SerializeField]
		public float InterCollisionStiffness = 0.2f;

		[SerializeField]
		public float ConstraintSize = 0.05f;
	}
}
