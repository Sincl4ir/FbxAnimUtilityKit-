#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pampero.Tools.FbxUtilties
{
    public abstract class BaseFbxTool : EditorWindow
    {
        protected const string DRAG_AND_DROP_TITLE = "Drag and Drop FBX Files Below";
        protected const string FLUSH_BUTTON_TITLE = "Flush FBX List";
        protected const string EMPTY_FBX_WARNING = "Please select at least one FBX file.";
        protected const int DEFAULT_SPACE = 10;

        public abstract string ApplyButtonText { get; }
        protected virtual string GetDragAndDropTitle => DRAG_AND_DROP_TITLE;
        protected virtual string GetFlushButtonTitle => FLUSH_BUTTON_TITLE;

        protected List<GameObject> _selectedFbxList = new();
        protected Vector2 _scrollPosition;
        protected bool _drawDragAndDropFields = true;

        public abstract string WindowDescription { get; }
        public abstract string WindowTitle { get; }
        public virtual bool DrawCustomField => true;
        

        protected virtual void OnEnable()
        {
            titleContent = new GUIContent(WindowTitle);
        }

        public virtual void OnGUI()
        {
            //_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            GUILayout.BeginVertical();
            GUILayout.Space(DEFAULT_SPACE);
            DrawWindowTitle();
            GUILayout.Space(4);
            DrawWindowDescription();
            GUILayout.Space(DEFAULT_SPACE);

            if (DrawCustomField)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Space(DEFAULT_SPACE);
                DrawCustomFields();
                GUILayout.Space(DEFAULT_SPACE);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Space(DEFAULT_SPACE);
            }

            DragAndDropFields();
            GUILayout.Space(DEFAULT_SPACE);
            ApplyActionButton();
            GUILayout.EndVertical();
            //EditorGUILayout.EndScrollView();
        }

        protected virtual void DrawWindowTitle()
        {
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label(WindowTitle, titleStyle);
        }

        protected virtual void DrawWindowDescription()
        {
            var descStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label(WindowDescription, descStyle);
        }

        protected void ApplyActionButton()
        {
            if (GUILayout.Button(ApplyButtonText))
            {
                ApplyActionToAll();
            }
        }

        protected virtual void DrawCustomFields() { }

        protected void DragAndDropFields()
        {
            if (!_drawDragAndDropFields) { return; }
            GUILayout.Space(10);
            GUILayout.Label(GetDragAndDropTitle, EditorStyles.boldLabel);

            int listCount = _selectedFbxList.Count;
            if (listCount > 0)
            {
                float scrollHeight = Mathf.Min(200, listCount * 20 + 10); // Adjust height dynamically

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(scrollHeight));
                for (int i = listCount - 1; i >= 0; i--)
                {
                    EditorGUILayout.BeginHorizontal();
                    _selectedFbxList[i] = (GameObject)EditorGUILayout.ObjectField($"FBX File {listCount - i}", _selectedFbxList[i], typeof(GameObject), false);

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        _selectedFbxList.RemoveAt(i);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView(); // End scrolling area
            }

            GUILayout.Space(10);
            HandleDragAndDrop();
            GUILayout.Space(10);
            HandleFlushButton();
        }

        private void HandleFlushButton()
        {
            if (_selectedFbxList.Count <= 0) { return; }
            if (GUILayout.Button(GetFlushButtonTitle))
            {
                FlushButton();
            }
        }

        protected virtual void FlushButton()
        {
            _selectedFbxList.Clear();
        }

        protected virtual void HandleDragAndDrop()
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
                            if (draggedObject is GameObject fbx && IsFbxFile(fbx))
                            {
                                if (!_selectedFbxList.Contains(fbx))
                                {
                                    _selectedFbxList.Add(fbx);
                                }
                            }
                        }
                        Event.current.Use();
                    }
                }
            }
        }

        protected bool IsFbxFile(GameObject fbx)
        {
            string path = AssetDatabase.GetAssetPath(fbx);
            return path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase);
        }

        protected void ApplyActionToAll()
        {
            if (_selectedFbxList.Count == 0)
            {
                Debug.LogWarning(EMPTY_FBX_WARNING);
                return;
            }

            foreach (var selectedFbx in _selectedFbxList)
            {
                string fbxPath = AssetDatabase.GetAssetPath(selectedFbx);
                ModelImporter modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;

                if (modelImporter != null)
                {
                    if (!TryApplyModification(selectedFbx, modelImporter)) { continue; }
                    //AssetDatabase.ImportAsset(fbxPath);
                    modelImporter.SaveAndReimport();
                    Debug.Log($"✅ Action successfully applied to {fbxPath}");
                }
                else
                {
                    Debug.LogError($"Selected file {fbxPath} is not a valid FBX model.");
                }
            }
        }

        protected abstract bool TryApplyModification(GameObject fbx, ModelImporter modelImporter);
    }
}
//EOF.
#endif