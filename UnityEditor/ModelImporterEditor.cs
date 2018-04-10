using System;
using UnityEngine;

namespace UnityEditor
{
	[CanEditMultipleObjects, CustomEditor(typeof(ModelImporter))]
	internal class ModelImporterEditor : AssetImporterTabbedEditor
	{
		public override bool showImportedObject
		{
			get
			{
				return base.activeTab is ModelImporterModelEditor;
			}
		}

		public override void OnEnable()
		{
			if (base.tabs == null)
			{
				base.tabs = new BaseAssetImporterTabUI[]
				{
					new ModelImporterModelEditor(this),
					new ModelImporterRigEditor(this),
					new ModelImporterClipEditor(this),
					new ModelImporterMaterialEditor(this)
				};
				this.m_TabNames = new string[]
				{
					"Model",
					"Rig",
					"Animation",
					"Materials"
				};
			}
			base.OnEnable();
		}

		public override void OnDisable()
		{
			BaseAssetImporterTabUI[] tabs = base.tabs;
			for (int i = 0; i < tabs.Length; i++)
			{
				BaseAssetImporterTabUI baseAssetImporterTabUI = tabs[i];
				baseAssetImporterTabUI.OnDisable();
			}
			base.OnDisable();
		}

		public override bool HasPreviewGUI()
		{
			return base.HasPreviewGUI() && base.targets.Length < 2;
		}

		public override GUIContent GetPreviewTitle()
		{
			ModelImporterClipEditor modelImporterClipEditor = base.activeTab as ModelImporterClipEditor;
			GUIContent result;
			if (modelImporterClipEditor != null)
			{
				result = new GUIContent(modelImporterClipEditor.selectedClipName);
			}
			else
			{
				result = base.GetPreviewTitle();
			}
			return result;
		}
	}
}
