using System;

namespace Braintelligence
{
    public interface IBraintelligenceClient
    {
        event Action Connected;
        void SetTrigger(string trigger);
        void SetTrigger(params object[] objects);
    }
}