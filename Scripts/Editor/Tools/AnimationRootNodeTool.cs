#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pampero.Tools.FbxUtilties
{
    public class AnimationRootNodeTool : BaseFbxTool
    {
        private const string MENU_ITEM_TITLE = "Pampero/Fbx and Animation Utility Kit/Tools/Set Root Node on FBX Animations";
        private const string WINDOW_TITLE = "Set Root Node on FBX Animations";
        private const string EMPTY_ROOT_NAME_WARNING = "Please enter a Root Node Name.";
        private const string NO_BONES_WARNING = "No bones found in FBX.";
        private const string POP_UP_LABEL = "Root Bone";
        private const string FBX_NO_CHILDS_WARNING = "Selected FBX has no child transforms.";

        private int _selectedBoneIndex = 0;
        private List<string> _boneNames = new();
        public override string ApplyButtonText => "Apply Root Node to Animations";
        public override string WindowTitle => "Set Root Node on FBX Animations";
        public override string WindowDescription => "Set the Motion Root Node on selected FBX files to define which bone is used for root motion";

        [MenuItem(MENU_ITEM_TITLE)]
        public static void ShowWindow() => GetWindow<AnimationRootNodeTool>(WINDOW_TITLE);

        protected override void DrawCustomFields()
        {
            if (_selectedFbxList.Count == 0)
            {
                EditorGUILayout.HelpBox("Please select at least one FBX file to load bones from.", MessageType.Info);
                return;
            }

            if (_selectedFbxList.Count == 0) { return; }

            GameObject firstFbx = _selectedFbxList[0];
            if (_boneNames.Count == 0 || GUI.changed)
            {
                LoadBonesFromSelectedFbx(firstFbx);
            }

            if (_boneNames.Count > 0)
            {
                _selectedBoneIndex = EditorGUILayout.Popup(POP_UP_LABEL, _selectedBoneIndex, _boneNames.ToArray());
            }
            else
            {
                GUILayout.Label(NO_BONES_WARNING, EditorStyles.helpBox);
            }
        }

        private void LoadBonesFromSelectedFbx(GameObject fbx)
        {
            _selectedBoneIndex = 0;
            _boneNames.Clear();

            if (fbx == null) { return; }

            if (fbx.transform.childCount == 0)
            {
                Debug.LogWarning(FBX_NO_CHILDS_WARNING);
                return;
            }

            // Get the first child of the FBX (the actual model root)
            Transform modelRoot = fbx.transform.GetChild(0);

            Transform[] transforms = modelRoot.GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                string fullPath = GetTransformPath(t, modelRoot);
                _boneNames.Add(fullPath);
            }
        }

        private string GetTransformPath(Transform target, Transform root)
        {
            if (target == root) { return target.name; }

            List<string> names = new();
            Transform current = target;
            while (current != null && current != root)
            {
                names.Insert(0, current.name);
                current = current.parent;
            }

            if (current == root)
            {
                names.Insert(0, root.name);
            }

            return string.Join("/", names);
        }

        protected override bool TryApplyModification(GameObject fbx, ModelImporter modelImporter)
        {
            if (_boneNames == null || _boneNames.Count == 0 || _selectedBoneIndex < 0 || _selectedBoneIndex >= _boneNames.Count)
            {
                Debug.LogWarning(EMPTY_ROOT_NAME_WARNING);
                return false;
            }

            Transform modelRoot = fbx.transform.childCount > 0 ? fbx.transform.GetChild(0) : null;

            if (modelRoot == null)
            {
                Debug.LogError($"FBX '{fbx.name}' has no children to search for bones.");
                return false;
            }

            string rootNodeName = _boneNames[_selectedBoneIndex]; 

            if (!CheckIfRootNodeExistInFbx(rootNodeName, modelRoot)) 
            {
                Debug.LogError($"Root bone '{rootNodeName}' not found in '{fbx.name}'. Skipping.");
                return false; 
            }

            modelImporter.motionNodeName = rootNodeName;
            Debug.Log($"Set motion node name to '{rootNodeName}' for {fbx.name}");
            return true;
        }

        private bool CheckIfRootNodeExistInFbx(string rootNodeName, Transform modelRoot)
        {
            string targetBoneName = rootNodeName.Split('/').Last();
            Transform[] allBones = modelRoot.GetComponentsInChildren<Transform>(true);
            Transform foundBone = null;

            foreach (var bone in allBones)
            {
                if (bone.name != targetBoneName) { continue; }

                string bonePath = GetTransformPath(bone, modelRoot);
                if (bonePath == rootNodeName)
                {
                    foundBone = bone;
                    break;
                }
            }

            return foundBone != null;
        }
    }
}
//EOF
#endif