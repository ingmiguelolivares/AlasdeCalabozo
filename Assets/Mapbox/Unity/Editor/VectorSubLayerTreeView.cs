namespace Mapbox.Editor
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor.IMGUI.Controls;
	using UnityEditor;
	using Mapbox.Unity.Map;
	using UITreeView = UnityEditor.IMGUI.Controls.TreeView<int>;
	using UITreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
	using UITreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;

	public class VectorSubLayerTreeView : UITreeView
	{
		public SerializedProperty Layers;

		public VectorSubLayerTreeView(UITreeViewState state)
			: base(state)
		{
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			Reload();
		}

		protected override UITreeViewItem BuildRoot()
		{
			// The root item is required to have a depth of -1, and the rest of the items increment from that.
			var root = new UITreeViewItem { id = -1, depth = -1, displayName = "Root" };

			var items = new List<UITreeViewItem>();
			var index = 0;

			if (Layers != null)
			{
				for (int i = 0; i < Layers.arraySize; i++)
				{
					var name = Layers.GetArrayElementAtIndex(i).FindPropertyRelative("coreOptions.sublayerName").stringValue;
					items.Add(new UITreeViewItem { id = index, depth = 0, displayName = name });
					index++;
				}
			}

			// Utility method that initializes the TreeViewItem.children and .parent for all items.
			SetupParentsAndChildrenFromDepths(root, items);

			// Return root of the tree
			return root;
		}

		protected override bool CanRename(UITreeViewItem item)
		{
			return true;
		}

		protected override void RenameEnded(RenameEndedArgs args)
		{
			if (Layers != null)
			{
				var layer = Layers.GetArrayElementAtIndex(args.itemID);
				if (string.IsNullOrEmpty(args.newName.Trim()))
				{
					layer.FindPropertyRelative("coreOptions.sublayerName").stringValue = args.originalName;
				}
				else
				{
					layer.FindPropertyRelative("coreOptions.sublayerName").stringValue = args.newName;
				}
			}
		}
	}
}
