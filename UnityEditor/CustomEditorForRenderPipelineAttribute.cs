using System;

namespace UnityEditor
{
	[AttributeUsage(AttributeTargets.Class)]
	public class CustomEditorForRenderPipelineAttribute : CustomEditor
	{
		internal Type renderPipelineType;

		public CustomEditorForRenderPipelineAttribute(Type inspectedType, Type renderPipeline) : base(inspectedType)
		{
			this.renderPipelineType = renderPipeline;
		}

		public CustomEditorForRenderPipelineAttribute(Type inspectedType, Type renderPipeline, bool editorForChildClasses) : base(inspectedType, editorForChildClasses)
		{
			this.renderPipelineType = renderPipeline;
		}
	}
}
