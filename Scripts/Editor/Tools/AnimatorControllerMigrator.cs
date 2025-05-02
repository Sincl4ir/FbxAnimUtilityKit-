#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditorInternal;
using UnityEngine;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using BlendTree = UnityEditor.Animations.BlendTree;

namespace Pampero.Tools.FbxUtilties
{
    public class AnimatorControllerMigrator : BaseFbxTool
    {
        private const string MENU_ITEM_TITLE = "Pampero/Fbx and Animation Utility Kit/Tools/Animator Controller Migrator";
        private const string WINDOW_TITLE = "Animator Controller Migrator";
        private const string ANIM_CONTROLLER = "Animator Controller";
        private const string CLIPS_PROPERTY = "selectedClips";
        private const string SELECTED_CLIPS_TITLE = "Selected Animation Clips";

        public override string ApplyButtonText => "Replace Animation Clips";

        public override string WindowTitle => "Animator Controller Migrator";
        public override string WindowDescription => "Replace AnimationClips inside an Animator Controller by matching their names against " +
                                                    "a selected list of new clips. Supports replacing clips in states and nested BlendTrees.";

        protected override string GetDragAndDropTitle => "Drag and Drop Animation Clips Below";
        protected override string GetFlushButtonTitle => "Flush Animation Clip List";

        [SerializeField] private List<AnimationClip> _selectedClips = new();

        private AnimatorController _animatorController;
        private SerializedObject _serializedObject;
        private SerializedProperty _selectedClipsProperty;
        private ReorderableList _reorderableList;

        [MenuItem(MENU_ITEM_TITLE)]
        public static void ShowWindow() => GetWindow<AnimatorControllerMigrator>(WINDOW_TITLE);

        protected override void OnEnable()
        {
            base.OnEnable();
            _serializedObject = new SerializedObject(this);
            _selectedClipsProperty = _serializedObject.FindProperty(CLIPS_PROPERTY);

            _reorderableList = new ReorderableList(_serializedObject, _selectedClipsProperty, true, true, true, true)
            {
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = _selectedClipsProperty.GetArrayElementAtIndex(index);
                    rect.y += 2;
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                },
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, SELECTED_CLIPS_TITLE),
                onAddCallback = list => _selectedClips.Add(null),
                onRemoveCallback = list => _selectedClips.RemoveAt(list.index)
            };
        }

        protected override void DrawCustomFields()
        {
            EditorGUILayout.Space(4);
            _animatorController = (AnimatorController)EditorGUILayout.ObjectField(ANIM_CONTROLLER, _animatorController, typeof(AnimatorController), false);

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

        protected override bool TryApplyModification(GameObject fbx, ModelImporter modelImporter)
        {
            return TryReplaceAnimations();
        }

        private bool TryReplaceAnimations()
        {
            if (_animatorController is null) { return false; }
            ReplaceAnimationClips(_animatorController);
            return true;
        }

        protected override void HandleDragAndDrop()
        {
            Rect dragArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
            GUI.Box(dragArea, GetDragAndDropTitle, EditorStyles.helpBox);

            Event evt = Event.current;
            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dragArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is AnimationClip clip && !_selectedClips.Contains(clip))
                            _selectedClips.Add(clip);
                    }
                    _serializedObject.Update();
                }
                evt.Use();
            }
        }

        protected override void FlushButton()
        {
            base.FlushButton();
            _selectedClips.Clear();
            _serializedObject.Update();
        }

        private void ReplaceAnimationClips(AnimatorController controller)
        {
            foreach (var layer in controller.layers)
            {
                ReplaceClipsInStateMachine(layer.stateMachine);
            }
        }

        private void ReplaceClipsInStateMachine(AnimatorStateMachine stateMachine)
        {
            foreach (var state in stateMachine.states)
            {
                if (state.state.motion is AnimationClip currentClip && TryFindReplacementClip(currentClip.name, out var replacementClip))
                {
                    state.state.motion = replacementClip;
                    Debug.Log($"Replaced clip '{currentClip.name}' in state '{state.state.name}' with '{replacementClip.name}'");
                }
                else if (state.state.motion is BlendTree blendTree)
                {
                    ReplaceClipsInBlendTree(blendTree);
                }
            }

            foreach (var subStateMachine in stateMachine.stateMachines)
            {
                ReplaceClipsInStateMachine(subStateMachine.stateMachine);
            }
        }

        private void ReplaceClipsInBlendTree(BlendTree blendTree)
        {
            for (int i = 0; i < blendTree.children.Length; i++)
            {
                if (blendTree.children[i].motion is AnimationClip childClip && TryFindReplacementClip(childClip.name, out var replacementClip))
                {
                    blendTree.children[i].motion = replacementClip;
                    Debug.Log($"Replaced clip '{childClip.name}' in BlendTree '{blendTree.name}' with '{replacementClip.name}'");
                }
                else if (blendTree.children[i].motion is BlendTree childBlendTree)
                {
                    ReplaceClipsInBlendTree(childBlendTree);
                }
            }
        }

        private bool TryFindReplacementClip(string clipName, out AnimationClip animationClip)
        {
            animationClip = _selectedClips.FirstOrDefault(clip => clip.name == clipName);
            return animationClip != null;
        }
    }
}
//EOF.
#endif