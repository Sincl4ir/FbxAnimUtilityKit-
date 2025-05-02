#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Pampero.Tools.FbxUtilties
{
    public class AnimatorOverrideControllerMigrator : BaseFbxTool
    {
        private const string MENU_ITEM_TITLE = "Pampero/Fbx and Animation Utility Kit/Tools/Animator Override Controller Migrator";
        private const string WINDOW_TITLE = "Override Controller Migrator";
        private const string ANIM_CONTROLLER = "Animator Controller Override";
        private const string CLIPS_PROPERTY = "selectedClips";
        private const string SELECTED_CLIPS_TITLE = "Selected Animation Clips";

        public override string ApplyButtonText => "Replace Animation Clips";
        public override string WindowTitle => "Animator Override Controller Migrator";
        public override string WindowDescription => "Attempts to find and replace the original animation clips within the override controller with the provided replacements based on their names.";

        protected override string GetDragAndDropTitle => "Drag and Drop Animation Clips Below";
        protected override string GetFlushButtonTitle => "Flush Animation Clip List";


        [SerializeField] private List<AnimationClip> _selectedClips = new();

        private AnimatorOverrideController _animatorOverride;
        private SerializedObject _serializedObject;
        private SerializedProperty _selectedClipsProperty;
        private ReorderableList _reorderableList;

        [MenuItem(MENU_ITEM_TITLE)]
        public static void ShowWindow()
        {
            GetWindow<AnimatorOverrideControllerMigrator>(WINDOW_TITLE);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _serializedObject = new SerializedObject(this);
            _selectedClipsProperty = _serializedObject.FindProperty(CLIPS_PROPERTY);

            _reorderableList = new ReorderableList(_serializedObject, _selectedClipsProperty, true, true, true, true)
            {
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _selectedClipsProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                },
                drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, SELECTED_CLIPS_TITLE),
                onAddCallback = (ReorderableList list) => _selectedClips.Add(null),
                onRemoveCallback = (ReorderableList list) => _selectedClips.RemoveAt(list.index)
            };
        }

        protected override void DrawCustomFields()
        {
            EditorGUILayout.Space(4);
            _animatorOverride = (AnimatorOverrideController)EditorGUILayout.ObjectField(ANIM_CONTROLLER, _animatorOverride, typeof(AnimatorOverrideController), false);

            EditorGUILayout.Space(10);

            _serializedObject.Update();
            HandleReordableList();
            _serializedObject.ApplyModifiedProperties();
        }

        private void HandleReordableList()
        {
            if (_reorderableList.count <= 0) { return; }
            float scrollHeight = Mathf.Min(200, _reorderableList.count * 20 + 10);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(scrollHeight));
            _reorderableList.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }

        protected override void HandleDragAndDrop()
        {
            Rect dragArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUI.Box(dragArea, GetDragAndDropTitle, EditorStyles.helpBox);

            Event evt = Event.current;
            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (dragArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (Object draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is AnimationClip clip && !_selectedClips.Contains(clip))
                            {
                                _selectedClips.Add(clip);
                            }
                        }
                        _serializedObject.Update();
                    }
                    evt.Use();
                }
            }
        }

        protected override void FlushButton()
        {
            base.FlushButton();
            _selectedClips.Clear();
            _serializedObject.Update();
        }

        private bool TryReplaceAnimationClipsInOverrideController(AnimatorOverrideController overrideController)
        {
            if (overrideController is null) { return false; }
            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);

            foreach (var kvp in overrides)
            {
                if (kvp.Value != null && TryFindReplacementClip(kvp.Value.name, out var replacementClip))
                {
                    overrideController[kvp.Key] = replacementClip;
                    Debug.Log($"Replaced override clip '{kvp.Value.name}' with '{replacementClip.name}'");
                }
            }

            return true;
        }

        private bool TryFindReplacementClip(string clipName, out AnimationClip animationClip)
        {
            animationClip = _selectedClips.FirstOrDefault(clip => clip.name == clipName);
            return animationClip != null;
        }

        protected override bool TryApplyModification(GameObject fbx, ModelImporter modelImporter)
        {
            return TryReplaceAnimationClipsInOverrideController(_animatorOverride);
        }
    }
}
//EOF.
#endif