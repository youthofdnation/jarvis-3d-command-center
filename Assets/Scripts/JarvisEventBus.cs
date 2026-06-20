using System;
using UnityEngine;

namespace Jarvis3DCommandCenter
{
    public sealed class JarvisEventBus : MonoBehaviour
    {
        public event Action<MockTaskState> OnTaskUpdated;
        public event Action<string> OnLog;
        public event Action<string> OnCommandProjected;

        public void PublishTaskUpdated(MockTaskState state)
        {
            OnTaskUpdated?.Invoke(state);
        }

        public void PublishLog(string line)
        {
            OnLog?.Invoke(line);
        }

        public void PublishCommandProjected(string command)
        {
            OnCommandProjected?.Invoke(command);
        }
    }
}

