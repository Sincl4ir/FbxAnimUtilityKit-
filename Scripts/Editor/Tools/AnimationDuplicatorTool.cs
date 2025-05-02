#if  UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pampero.Tools.FbxUtilties
{
    public class AnimationDuplicatorTool : BaseFbxTool
    {
        private const string MENU_ITEM_TITLE = "Pampero/Fbx and Animation Utility Kit/Tools/Duplicate FBX Animations";
        private const string WINDOW_TITLE = "Duplicate FBX Animations";

        public override string WindowTitle => "Duplicate FBX Animations";
        public override string WindowDescription => "Duplicates animation clips from an FBX.";
        public override string ApplyButtonText => "Duplicate Animations";
        public override bool DrawCustomField => false;

        [MenuItem(MENU_ITEM_TITLE)]
        public static void ShowWindow() => GetWindow<AnimationDuplicatorTool>(WINDOW_TITLE);

        protected override bool TryApplyModification(GameObject fbx, ModelImporter modelImporter)
        {
            string fbxPath = AssetDatabase.GetAssetPath(fbx);
            string directory = Path.GetDirectoryName(fbxPath);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);

            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip)
                {
                    // Skip preview animations
                    if (clip.name.StartsWith("__preview__"))
                    {
                        continue;
                    }

                    // Create a duplicate of the animation clip
                    AnimationClip duplicateClip = new AnimationClip();
                    EditorUtility.CopySerialized(clip, duplicateClip);

                    // Save the duplicate as an asset with a unique name
                    string newClipPath = Path.Combine(directory, clip.name + ".asset");
                    int fileIndex = 1;
                    while (AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath) != null)
                    {
                        newClipPath = Path.Combine(directory, $"{clip.name}_{fileIndex}.asset");
                        fileIndex++;
                    }

                    AssetDatabase.CreateAsset(duplicateClip, newClipPath);
                    AssetDatabase.Refresh();
                    AssetDatabase.ImportAsset(newClipPath);

                    Debug.Log($"Duplicated animation: {newClipPath}");
                }
            }

            AssetDatabase.SaveAssets();
            return true;
        }
    }
}
//EOF.
#endif