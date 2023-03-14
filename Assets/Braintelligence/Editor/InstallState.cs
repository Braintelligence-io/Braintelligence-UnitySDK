using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Braintelligence.Editor
{
    public class InstallState
    {
        public Action Finished;

        private readonly VisualElement _root;
        private readonly Label _label;
        private readonly Button _button;

        public InstallState(VisualElement root)
        {
            _root = root.Q<VisualElement>("install");
            _label = _root.Q<Label>("install-label");
            _label.text = "Select a folder to create prefab and settings";
            _button = _root.Q<Button>("install-button");
            _root.style.display = DisplayStyle.None;
        }

        public void Init()
        {
            _root.style.display = DisplayStyle.Flex;

            _button.RegisterCallback<ClickEvent>(OnInstallButtonClicked);
            string prefabPath = EditorPrefs.GetString(EditorSettings.PathKey);
            prefabPath = Path.Combine(prefabPath, $"{EditorSettings.ObjectName}.prefab");
            BraintelligenceClient client = AssetDatabase.LoadAssetAtPath<BraintelligenceClient>(prefabPath);
            if (ReferenceEquals(client, null) == false)
            {
                Close();
            }
        }

        private void OnInstallButtonClicked(ClickEvent evt)
        {
            string path = EditorUtility.OpenFolderPanel("Select Installation Folder", "Assets", string.Empty);
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            GameObject go = new(EditorSettings.ObjectName, typeof(BraintelligenceClient));
            BraintelligenceClient client = go.GetComponent<BraintelligenceClient>();
            ClientSettings settings = ScriptableObject.CreateInstance<ClientSettings>();
            string settingsPath = Path.Combine(path, EditorSettings.SettingsName);
            settingsPath = AssetDatabase.GenerateUniqueAssetPath(settingsPath);
            AssetDatabase.CreateAsset(settings, settingsPath);
            Selection.activeObject = settings;

            SerializedObject so = new(client);
            SerializedProperty property = so.FindProperty("_settings");
            property.objectReferenceValue = settings;
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            string prefabPath = Path.Combine(path, $"{EditorSettings.ObjectName}.prefab");
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction, out bool success);

            if (success)
            {
                _label.text = "Prefab and settings created successfully.\n" +
                              $"API: {EditorSettings.APIVersion}\n" +
                              $"SDK: {EditorSettings.SDKVersion}";
                _button.UnregisterCallback<ClickEvent>(OnInstallButtonClicked);
                _button.RegisterCallback<ClickEvent>(OnNextButtonClicked);
                _button.text = "Next";
                EditorPrefs.SetString(EditorSettings.PathKey, path);
            }
            else
                _label.text = "Could not create files. Check console for more information";

            Object.DestroyImmediate(go);
        }

        private void OnNextButtonClicked(ClickEvent evt)
        {
            Close();
        }

        private void Close()
        {
            _button.UnregisterCallback<ClickEvent>(OnNextButtonClicked);
            _root.style.display = DisplayStyle.None;
            Finished?.Invoke();
        }
    }
}