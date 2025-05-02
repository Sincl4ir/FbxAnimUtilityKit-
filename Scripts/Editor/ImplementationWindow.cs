#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pampero.Tools.FbxUtilties
{
    public class ImplementationWindow : EditorWindow
    {
        protected const string MENU_ITEM_TITLE = "Pampero/Fbx and Animation Utility Kit/UtilityKitWindow";
        protected const string WINDOW_TITLE = "Fbx and Animation Utility Kit";
        protected const int DEFAULT_PADDING = 5;

        protected List<BaseFbxTool> _tools = new List<BaseFbxTool>();
        protected int _selectedTabIndex = 0;

        [MenuItem(MENU_ITEM_TITLE)]
        public static void ShowWindow()
        {
            ImplementationWindow window = GetWindow<ImplementationWindow>(WINDOW_TITLE);
            window.Show();
        }

        private void OnEnable()
        {
            _tools.Add(ScriptableObject.CreateInstance<AnimationMaskTool>());
            _tools.Add(ScriptableObject.CreateInstance<AnimationRootNodeTool>());
            _tools.Add(ScriptableObject.CreateInstance<AnimationLoopTool>());
            _tools.Add(ScriptableObject.CreateInstance<AnimationDuplicatorTool>());
            _tools.Add(ScriptableObject.CreateInstance<AnimatorControllerMigrator>());
            _tools.Add(ScriptableObject.CreateInstance<AnimatorOverrideControllerMigrator>());
        }

        private void OnDisable()
        {
            // Clean up tools to avoid duplication
            foreach (var tool in _tools)
            {
                DestroyImmediate(tool);
            }

            _tools.Clear();
        }

        private void OnGUI()
        {
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.Space(DEFAULT_PADDING);
            GUILayout.Label("Select Tool: ", GUILayout.Width(100)); 
            string[] toolNames = _tools.Select(tool => tool.WindowTitle).ToArray();
            _selectedTabIndex = EditorGUILayout.Popup(_selectedTabIndex, toolNames);
            GUILayout.Space(DEFAULT_PADDING);
            GUILayout.EndHorizontal();

            GUILayout.Space(DEFAULT_PADDING);

            // Display the UI for the selected tool
            GUILayout.BeginHorizontal();
            GUILayout.Space(DEFAULT_PADDING);
            _tools[_selectedTabIndex].OnGUI();
            GUILayout.Space(DEFAULT_PADDING);
            GUILayout.EndHorizontal();

            GUILayout.Space(DEFAULT_PADDING);
        }
    }
}
//EOF
#endif