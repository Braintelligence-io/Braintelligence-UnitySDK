using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UnityEngine;

namespace Braintelligence
{
    [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
    public class BraintelligenceClient : MonoBehaviour, IBraintelligenceClient
    {
        public event Action Connected;

        public static IBraintelligenceClient Instance
        {
            get
            {
                ThrowIfNotInitialized();
                return _instance;
            }
        }

        private const float ScreenCaptureInterval = 1f;
        [SerializeField] private ClientSettings _settings;
        private static BraintelligenceClient _instance;
        private float _elapsed;
        private static bool _connected;
        private static Action _onConnectCallback;

        /// <summary>
        /// Connects to the Braintelligence client and executes the provided action when connected.
        /// </summary>
        /// <param name="connected">The callback to execute when connected. And return a reference
        /// to BrainIntelligence Client as a parameter</param>
        public static void Connect(Action connected)
        {
            BraintelligenceClient instance = GetInstance();
            instance.Connected -= _onConnectCallback;
            _onConnectCallback = connected;
            instance.Connected += _onConnectCallback;
            ClientSettings set = instance._settings;
            NetClient.Connect(set.LocalServerIP, set.LocalServerPort, set.GameKey, instance.OnConnected);
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
            _connected = false;
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
            _connected = true;
            SetTrigger("Session:Start");
            Connected?.Invoke();
        }

        private void Update()
        {
            if (NetClient.Update())
            {
                _elapsed += Time.deltaTime;
                if (_elapsed >= ScreenCaptureInterval)
                {
                    _elapsed = 0;
                    CaptureFrame();
                }
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

        /// <summary>
        /// Sends a trigger to the server.
        /// </summary>
        /// <param name="trigger">The trigger to send.</param>
        public void SetTrigger(string trigger)
        {
            if (_instance == null || _connected == false) return;
            NetClient.Send($"{Time.unscaledTime}:{trigger}");
        }


        /// <summary>
        /// Sends multiple objects as a trigger to the server.
        /// </summary>
        /// <param name="objects">The objects to send as part of the trigger.</param>
        public void SetTrigger(params object[] objects)
        {
            if (_instance == null || _connected == false) return;
            StringBuilder sb = new();
            sb.Append($"{Time.unscaledTime}");

            // Append each object to the trigger string.
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

        private static void ThrowIfNotInitialized()
        {
            if (_instance == null)
                throw new NullReferenceException(
                    $"{nameof(BraintelligenceClient)} is not initialized. Call Connect for initialization.");
        }
    }
}