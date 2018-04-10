using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Experimental.UIElements
{
	[CustomEditor(typeof(VisualTreeAsset))]
	internal class VisualTreeAssetEditor : Editor
	{
		private Panel m_Panel;

		private VisualElement m_Tree;

		private VisualTreeAsset m_LastTree;

		internal override string targetTitle
		{
			get
			{
				if (!base.target)
				{
					base.serializedObject.Update();
					base.InternalSetTargets(base.serializedObject.targetObjects);
				}
				return base.targetTitle;
			}
		}

		public override bool HasPreviewGUI()
		{
			return true;
		}

		public override GUIContent GetPreviewTitle()
		{
			return GUIContent.Temp(this.targetTitle);
		}

		public void Render(VisualTreeAsset vta, Rect r, GUIStyle background)
		{
			if (Event.current.type == EventType.Repaint && (r.width >= 100f || r.height >= 100f))
			{
				bool flag = false;
				if (vta != this.m_LastTree || !this.m_LastTree)
				{
					this.m_LastTree = vta;
					this.m_Tree = vta.CloneTree(null);
					this.m_Tree.StretchToParentSize();
					flag = true;
				}
				if (this.m_Panel == null)
				{
					this.m_Panel = UIElementsUtility.FindOrCreatePanel(this.m_LastTree, ContextType.Editor, new DataWatchService());
					if (this.m_Panel.visualTree.styleSheets == null)
					{
						GUIView.AddDefaultEditorStyleSheets(this.m_Panel.visualTree);
						this.m_Panel.visualTree.LoadStyleSheetsFromPaths();
					}
					this.m_Panel.allowPixelCaching = false;
					flag = true;
				}
				if (flag)
				{
					this.m_Panel.visualTree.Clear();
					this.m_Panel.visualTree.Add(this.m_Tree);
				}
				this.m_Panel.visualTree.layout = r;
				this.m_Panel.visualTree.Dirty(ChangeType.Layout);
				this.m_Panel.visualTree.Dirty(ChangeType.Repaint);
				Matrix4x4 matrix = GUIClip.GetMatrix();
				Rect topRect = GUIClip.GetTopRect();
				EditorGUI.DrawRect(r, (!EditorGUIUtility.isProSkin) ? HostView.kViewColor : EditorGUIUtility.kDarkViewBackground);
				this.m_Panel.Repaint(Event.current);
				GUIClip.SetTransform(matrix, topRect);
			}
		}

		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			base.OnPreviewGUI(r, background);
			this.Render(base.target as VisualTreeAsset, r, background);
		}
	}
}
