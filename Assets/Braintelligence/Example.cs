using UnityEngine;

namespace Braintelligence
{
    public class Example : MonoBehaviour
    {
        private IBraintelligenceClient _client;
        private int _counter;
        private bool _connected;

        private void Awake()
        {
            BraintelligenceClient.Connect(ClientOnConnected);
        }

        private void ClientOnConnected(IBraintelligenceClient client)
        {
            _client = client;
            _client.SetTrigger("Awake");
            _client.SetTrigger("Hello");
            print("Braintelligence Local Server Connected!");
            _connected = true;
        }

        private void Update()
        {
            if (_connected == false) return;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _counter++;
                _client.SetTrigger("Player", "Space", _counter); //Time:Player:Space:1
            }
        }
    }
}