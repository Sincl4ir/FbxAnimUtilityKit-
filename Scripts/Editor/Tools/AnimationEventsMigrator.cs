#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pampero.Tools.FbxUtilties
{
    public class AnimationEventsMigrator
    {
        private const string COPY_MENU_ITEM_TITLE = "Pampero/Fbx and Animation Utility Kit/Tools/Copy Animation Events";
        private const string ALLOCATE_MENU_ITEM_TITLE = "Pampero/Fbx and Animation Utility Kit/Tools/Allocate Animation Events";

        private static Dictionary<string, AnimationEvent[]> clipboard = new Dictionary<string, AnimationEvent[]>();

        [MenuItem(COPY_MENU_ITEM_TITLE)]
        public static void Copy()
        {
            // Clear the clipboard
            clipboard.Clear();

            // Get selected animations
            var selectedObjects = Selection.objects;
            foreach (var obj in selectedObjects)
            {
                if (obj is AnimationClip clip)
                {
                    // Copy animation events into the clipboard with the clip name as the key
                    var events = AnimationUtility.GetAnimationEvents(clip);
                    if (events.Length > 0)
                    {
                        clipboard[clip.name] = events;
                    }
                }
            }

            Debug.Log($"Copied {clipboard.Count} clips");
        }

        [MenuItem(COPY_MENU_ITEM_TITLE, true)]
        public static bool CopyValidation()
        {
            // Validate if the selected object is an AnimationClip
            return Selection.objects.Length > 0 && Selection.objects[0] is AnimationClip;
        }

        [MenuItem(ALLOCATE_MENU_ITEM_TITLE)]
        public static void Paste()
        {
            // Get selected animations
            var selectedObjects = Selection.objects;

            foreach (var obj in selectedObjects)
            {
                if (obj is AnimationClip clip && clipboard.ContainsKey(clip.name))
                {
                    // Paste animation events from the clipboard
                    Debug.Log($"Pasting events from {clipboard.ContainsKey(clip.name)} in {clip.name}");
                    AnimationUtility.SetAnimationEvents(clip, clipboard[clip.name]);
                    EditorUtility.SetDirty(clip);
                }
            }

            AssetDatabase.SaveAssets();
        }

        [MenuItem(ALLOCATE_MENU_ITEM_TITLE, true)]
        public static bool PasteValidation()
        {
            // Validate if the selected object is an AnimationClip
            return Selection.objects.Length > 0 && Selection.objects[0] is AnimationClip;
        }
    }
}
//EOF.
#endif