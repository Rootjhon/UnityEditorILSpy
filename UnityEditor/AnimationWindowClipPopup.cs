using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	[Serializable]
	internal class AnimationWindowClipPopup
	{
		[SerializeField]
		public AnimationWindowState state;

		[SerializeField]
		private int selectedIndex;

		public void OnGUI()
		{
			AnimationWindowSelectionItem selectedItem = this.state.selectedItem;
			if (!(selectedItem == null))
			{
				if (selectedItem.canChangeAnimationClip)
				{
					string[] clipMenuContent = this.GetClipMenuContent();
					EditorGUI.BeginChangeCheck();
					this.selectedIndex = EditorGUILayout.Popup(this.ClipToIndex(this.state.activeAnimationClip), clipMenuContent, EditorStyles.toolbarPopup, new GUILayoutOption[0]);
					if (EditorGUI.EndChangeCheck())
					{
						if (clipMenuContent[this.selectedIndex] == AnimationWindowStyles.createNewClip.text)
						{
							AnimationClip animationClip = AnimationWindowUtility.CreateNewClip(selectedItem.rootGameObject.name);
							if (animationClip)
							{
								AnimationWindowUtility.AddClipToAnimationPlayerComponent(this.state.activeAnimationPlayer, animationClip);
								this.state.selection.UpdateClip(this.state.selectedItem, animationClip);
								GUIUtility.ExitGUI();
							}
						}
						else
						{
							this.state.selection.UpdateClip(this.state.selectedItem, this.IndexToClip(this.selectedIndex));
						}
					}
				}
				else if (this.state.activeAnimationClip != null)
				{
					Rect controlRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, AnimationWindowStyles.toolbarLabel, new GUILayoutOption[0]);
					EditorGUI.LabelField(controlRect, CurveUtility.GetClipName(this.state.activeAnimationClip), AnimationWindowStyles.toolbarLabel);
				}
			}
		}

		private string[] GetClipMenuContent()
		{
			List<string> list = new List<string>();
			list.AddRange(this.GetClipNames());
			AnimationWindowSelectionItem selectedItem = this.state.selectedItem;
			if (selectedItem.rootGameObject != null && selectedItem.animationIsEditable)
			{
				list.Add("");
				list.Add(AnimationWindowStyles.createNewClip.text);
			}
			return list.ToArray();
		}

		private AnimationClip[] GetOrderedClipList()
		{
			AnimationClip[] array = new AnimationClip[0];
			if (this.state.activeRootGameObject != null)
			{
				array = AnimationUtility.GetAnimationClips(this.state.activeRootGameObject);
			}
			Array.Sort<AnimationClip>(array, (AnimationClip clip1, AnimationClip clip2) => CurveUtility.GetClipName(clip1).CompareTo(CurveUtility.GetClipName(clip2)));
			return array;
		}

		private string[] GetClipNames()
		{
			AnimationClip[] orderedClipList = this.GetOrderedClipList();
			string[] array = new string[orderedClipList.Length];
			for (int i = 0; i < orderedClipList.Length; i++)
			{
				array[i] = CurveUtility.GetClipName(orderedClipList[i]);
			}
			return array;
		}

		private AnimationClip IndexToClip(int index)
		{
			AnimationClip result;
			if (this.state.activeRootGameObject != null)
			{
				AnimationClip[] orderedClipList = this.GetOrderedClipList();
				if (index >= 0 && index < orderedClipList.Length)
				{
					result = orderedClipList[index];
					return result;
				}
			}
			result = null;
			return result;
		}

		private int ClipToIndex(AnimationClip clip)
		{
			int result;
			if (this.state.activeRootGameObject != null)
			{
				int num = 0;
				AnimationClip[] orderedClipList = this.GetOrderedClipList();
				AnimationClip[] array = orderedClipList;
				for (int i = 0; i < array.Length; i++)
				{
					AnimationClip y = array[i];
					if (clip == y)
					{
						result = num;
						return result;
					}
					num++;
				}
			}
			result = 0;
			return result;
		}
	}
}
