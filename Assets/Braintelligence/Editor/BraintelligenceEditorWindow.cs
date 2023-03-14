using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Braintelligence.Editor
{
    public static class EditorSettings
    {
        public const string APIVersion = "v1";
        public const string SDKVersion = "1.0";
        public const string PathKey = "BrainIntelligence.InstallationPath";
        public const string ObjectName = "Braintelligence";
        public const string SettingsName = "BraintelligenceSettings.asset";
    }
    
    public class BraintelligenceEditorWindow : EditorWindow
    {
        private InstallState _installState;
        private LoginState _loginState;
        private GameSelectionState _gameSelectionState;

        public void CreateGUI()
        {
            Log.Initialize(Log.Level.Info);
            string[] guids = AssetDatabase.FindAssets("BraintelligenceEditorWindow t:VisualTreeAsset");
            if (guids.Length == 0)
            {
                Log.Info("Could not locate EditorWindow UXML file.");
                return;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            VisualTreeAsset visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(assetPath);
            TemplateContainer template = visualTreeAsset.Instantiate();
            rootVisualElement.Add(template);
            _installState = new InstallState(rootVisualElement)
            {
                Finished = InstallationFinished
            };
            _loginState = new LoginState(rootVisualElement)
            {
                Finished = LoginFinished
            };
            _gameSelectionState = new GameSelectionState(rootVisualElement);

            _installState.Init();
        }
        
        private void InstallationFinished()
        {
           // _loginState.Init();
           Close();
        }
        
        private void LoginFinished(string token)
        {
            _gameSelectionState.Init();
        }

        [MenuItem("Window/Braintelligence")]
        public static void ShowWindow()
        {
            EditorWindow win = GetWindow(typeof(BraintelligenceEditorWindow));
            win.minSize = new Vector2(512, 512);
            win.Show();
        }
    }
}