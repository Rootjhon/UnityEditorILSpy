using System;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal abstract class ColliderEditorBase : Editor
	{
		public bool editingCollider
		{
			get
			{
				return EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this);
			}
		}

		protected virtual GUIContent editModeButton
		{
			get
			{
				return EditorGUIUtility.IconContent("EditCollider");
			}
		}

		protected virtual void OnEditStart()
		{
		}

		protected virtual void OnEditEnd()
		{
		}

		public virtual void OnEnable()
		{
			EditMode.editModeStarted += new Action<IToolModeOwner, EditMode.SceneViewEditMode>(this.OnEditModeStart);
			EditMode.editModeEnded += new Action<IToolModeOwner>(this.OnEditModeEnd);
		}

		public virtual void OnDisable()
		{
			EditMode.editModeStarted -= new Action<IToolModeOwner, EditMode.SceneViewEditMode>(this.OnEditModeStart);
			EditMode.editModeEnded -= new Action<IToolModeOwner>(this.OnEditModeEnd);
		}

		protected void InspectorEditButtonGUI()
		{
			EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.Collider, "Edit Collider", this.editModeButton, this);
		}

		internal override Bounds GetWorldBoundsOfTarget(UnityEngine.Object targetObject)
		{
			Bounds result;
			if (targetObject is Collider2D)
			{
				result = ((Collider2D)targetObject).bounds;
			}
			else if (targetObject is Collider)
			{
				result = ((Collider)targetObject).bounds;
			}
			else
			{
				result = base.GetWorldBoundsOfTarget(targetObject);
			}
			return result;
		}

		protected void OnEditModeStart(IToolModeOwner owner, EditMode.SceneViewEditMode mode)
		{
			if (mode == EditMode.SceneViewEditMode.Collider && owner == this)
			{
				this.OnEditStart();
			}
		}

		protected void OnEditModeEnd(IToolModeOwner owner)
		{
			if (owner == this)
			{
				this.OnEditEnd();
			}
		}
	}
}
