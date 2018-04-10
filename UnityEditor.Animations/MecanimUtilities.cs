using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Animations
{
	internal class MecanimUtilities
	{
		public static bool StateMachineRelativePath(AnimatorStateMachine parent, AnimatorStateMachine toFind, ref List<AnimatorStateMachine> hierarchy)
		{
			hierarchy.Add(parent);
			bool result;
			if (parent == toFind)
			{
				result = true;
			}
			else
			{
				ChildAnimatorStateMachine[] childStateMachines = AnimatorStateMachine.StateMachineCache.GetChildStateMachines(parent);
				for (int i = 0; i < childStateMachines.Length; i++)
				{
					if (MecanimUtilities.StateMachineRelativePath(childStateMachines[i].stateMachine, toFind, ref hierarchy))
					{
						result = true;
						return result;
					}
				}
				hierarchy.Remove(parent);
				result = false;
			}
			return result;
		}

		internal static bool AreSameAsset(UnityEngine.Object obj1, UnityEngine.Object obj2)
		{
			return AssetDatabase.GetAssetPath(obj1) == AssetDatabase.GetAssetPath(obj2);
		}

		internal static void DestroyBlendTreeRecursive(BlendTree blendTree)
		{
			for (int i = 0; i < blendTree.children.Length; i++)
			{
				BlendTree blendTree2 = blendTree.children[i].motion as BlendTree;
				if (blendTree2 != null && MecanimUtilities.AreSameAsset(blendTree, blendTree2))
				{
					MecanimUtilities.DestroyBlendTreeRecursive(blendTree2);
				}
			}
			Undo.DestroyObjectImmediate(blendTree);
		}
	}
}
