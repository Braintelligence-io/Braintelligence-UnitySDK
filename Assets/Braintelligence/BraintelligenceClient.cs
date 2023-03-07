using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UnityEngine;

namespace Braintelligence
{
    [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
    public class BraintelligenceClient : MonoBehaviour, IBraintelligenceClient
    {
        private const float ScreenCaptureInterval = 1f;

        [SerializeField] private ClientSettings _settings;
        private static BraintelligenceClient _instance;
        public event Action<IBraintelligenceClient> Connected;
        private float _elapsed;

        public static void Connect(Action<IBraintelligenceClient> connected)
        {
            BraintelligenceClient instance = GetInstance();
            instance.Connected += connected;
            ClientSettings set = instance._settings;
            NetClient.Connect(set.LocalServerIP, set.LocalServerPort, set.GameKey, _instance.OnConnected);
        }

        private static BraintelligenceClient GetInstance()
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<BraintelligenceClient>();
            return _instance;
        }

        private void Awake()
        {
            Log.Initialize(_settings.LogLevel);

            if (ReferenceEquals(_instance, null))
            {
                _instance = this;
                DontDestroyOnLoad(_instance);
                if (ReferenceEquals(_instance._settings, null))
                {
                    throw new NullReferenceException($"Client Settings on {_instance.GetType().Name} is null.");
                }

                _elapsed = 0;
            }

            if (_instance != this)
            {
                Log.Error($"More than one instance of {GetType().Name} found in the scene!");
                Destroy(this);
            }
        }


        private void OnConnected()
        {
            SetTrigger("Session:Start");
            Connected?.Invoke(this);
        }

        private void Update()
        {
            NetClient.Update();
            _elapsed += Time.deltaTime;
            if (_elapsed >= ScreenCaptureInterval)
            {
                _elapsed = 0;
                CaptureFrame();
            }
        }


        private void OnDisable()
        {
            NetClient.Close($"{Time.unscaledTime}:Session:End");
        }

        private void OnDestroy()
        {
            Connected = null;
        }

        public void SetTrigger(string trigger)
        {
            NetClient.Send($"{Time.unscaledTime}:{trigger}");
        }

        public void SetTrigger(params object[] objects)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"{Time.unscaledTime}");
            for (int i = 0; i < objects.Length; i++)
            {
                sb.Append($":{objects[i]}");
            }

            NetClient.Send(sb.ToString());
        }

        private void CaptureFrame()
        {
            Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
            NetClient.Send(texture.EncodeToJPG());
            Destroy(texture);
        }
    }
}