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
            //You can use retrieve the client's object and send message through it
            _client = client;
            //Send a trigger for tagging your data with game events
            _client.SetTrigger("Hello!");
            _connected = true;
        }

        private void Update()
        {
            if (_connected == false) return;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _counter++;
                //You can chain multiple events/Tags and it will packed like this:
                //Data will be packed like this: Enemy:Spawn:Count:1
                _client.SetTrigger("Enemy", "Spawned", "Count",  _counter); 
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                //There is a singleton instance, if you prefer to not holding a reference
                //Call the BraintelligenceClient.Connect() before accessing the instance
                BraintelligenceClient.Instance.SetTrigger("Get Instance Event");
            }
        }
    }
}