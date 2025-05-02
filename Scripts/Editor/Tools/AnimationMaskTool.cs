#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Pampero.Tools.FbxUtilties
{
    public class AnimationMaskTool : BaseFbxTool
    {
        private const string MENU_ITEM_TITLE = "Pampero/Fbx and Animation Utility Kit/Tools/Set Avatar Mask on FBX Animations";
        private const string WINDOW_TITLE = "Set Avatar Mask on FBX Animations";
        private const string EMPTY_AVATAR_WARNING = "Please select an Avatar Mask.";
        private const string OBJECT_FIELD_LABEL = "Avatar Mask";

        private AvatarMask selectedMask;

        public override string ApplyButtonText => "Apply Mask to Animations";
        public override string WindowTitle => "Set Avatar Mask on FBX Animations";
        public override string WindowDescription => "Applies avatar masks to FBX file.";

        [MenuItem(MENU_ITEM_TITLE)]
        public static void ShowWindow() => GetWindow<AnimationMaskTool>(WINDOW_TITLE);

        protected override void DrawCustomFields()
        {
            selectedMask = (AvatarMask)EditorGUILayout.ObjectField(OBJECT_FIELD_LABEL, selectedMask, typeof(AvatarMask), false);
        }

        protected override bool TryApplyModification(GameObject fbx, ModelImporter modelImporter)
        {
            if (selectedMask == null)
            {
                Debug.LogWarning(EMPTY_AVATAR_WARNING);
                return false;
            }

            ModelImporterClipAnimation[] clips = modelImporter.clipAnimations.Length > 0
                ? modelImporter.clipAnimations
                : modelImporter.defaultClipAnimations;

            foreach (var clip in clips)
            {
                clip.maskType = ClipAnimationMaskType.CopyFromOther;
                clip.maskSource = selectedMask;
            }

            modelImporter.clipAnimations = clips;
            return true;
        }
    }
}
//EOF
#endif