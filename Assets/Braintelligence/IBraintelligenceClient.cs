using System;

namespace Braintelligence
{
    public interface IBraintelligenceClient
    {
        event Action<IBraintelligenceClient> Connected;
        void SetTrigger(string trigger);
        void SetTrigger(params object[] objects);
    }
}