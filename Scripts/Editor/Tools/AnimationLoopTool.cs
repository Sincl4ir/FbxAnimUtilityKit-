#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Pampero.Tools.FbxUtilties
{
    public class AnimationLoopTool : BaseFbxTool
    {
        private const string MENU_ITEM_TITLE = "Pampero/Fbx and Animation Utility Kit/Tools/Set Loop on FBX Animations";
        private const string WINDOW_TITLE = "Set Loop on FBX Animations";
        private const string LOOP_TOGGLE_LABEL = "Enable Looping";

        private bool loopAnimations = true;

        public override string ApplyButtonText => "Apply Loop Setting";
        public override string WindowTitle => "Set Loop on FBX Animations";
        public override string WindowDescription => "Set looping property on animation clips inside FBX files.";

        [MenuItem(MENU_ITEM_TITLE)]
        public static void ShowWindow() => GetWindow<AnimationLoopTool>(WINDOW_TITLE);

        protected override void DrawCustomFields()
        {
            loopAnimations = EditorGUILayout.Toggle(LOOP_TOGGLE_LABEL, loopAnimations);
        }

        protected override bool TryApplyModification(GameObject fbx, ModelImporter modelImporter)
        {
            ModelImporterClipAnimation[] clips = modelImporter.clipAnimations.Length > 0
                ? modelImporter.clipAnimations
                : modelImporter.defaultClipAnimations;

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i].loopTime = loopAnimations;
            }

            modelImporter.clipAnimations = clips;
            return true;
        }
    }
}
//EOF.
#endif