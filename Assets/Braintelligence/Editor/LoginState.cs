using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace Braintelligence.Editor
{
    public class LoginState
    {
        public Action<string> Finished;
        private const string BaseURL = "https://braintelligence.io/";
        private const string LoginUri = "";
        private readonly VisualElement _root;
        private readonly TextField _username;
        private readonly TextField _password;
        private readonly Button _submitButton;

        public static string Token { get; private set; }

        public LoginState(VisualElement root)
        {
            _root = root.Q<VisualElement>("login");
            _username = _root.Q<TextField>("username-textfield");
            _password = _root.Q<TextField>("password-textfield");
            _submitButton = _root.Q<Button>("login-button");
            _root.style.display = DisplayStyle.None;

        }

        public void Init()
        {
            _root.style.display = DisplayStyle.Flex;

            string path = EditorPrefs.GetString(EditorSettings.PathKey);
            path = Path.Combine(path, EditorSettings.SettingsName);
            ClientSettings settings = AssetDatabase.LoadAssetAtPath<ClientSettings>(path);
            if (string.IsNullOrWhiteSpace(settings.GameKey) == false)
            {
                Close();
            }

            _submitButton.RegisterCallback<ClickEvent>(OnSubmitClicked);
        }

        private void OnSubmitClicked(ClickEvent evt)
        {
            //TODO: Remove this statement later. 
            if (string.IsNullOrWhiteSpace(LoginUri))
            {
                Close();
                return;
            }
            
            Task.Run(PostCredentials);
        }

        private async Task PostCredentials()
        {
            using HttpClient client = new();
            client.BaseAddress = new Uri(BaseURL);
            FormUrlEncodedContent content = new(new[]
            {
                new KeyValuePair<string, string>("username", _username.value),
                new KeyValuePair<string, string>("password", _password.value),
            });
            HttpResponseMessage result = await client.PostAsync(LoginUri, content);
            string resultContent = await result.Content.ReadAsStringAsync();
            if (result.StatusCode == HttpStatusCode.OK)
            {
                Token = resultContent;
                Log.Info(resultContent);
                Close();
            }
            else
            {
                Log.Error($"Failed to login. Status code: {result.StatusCode}");
            }
        }

        private void Close()
        {
            _root.style.display = DisplayStyle.None;
            Finished?.Invoke(Token);
        }
    }
}