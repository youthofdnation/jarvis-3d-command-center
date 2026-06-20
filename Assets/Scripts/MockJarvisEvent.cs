using System;
using System.Collections.Generic;

namespace Jarvis3DCommandCenter
{
    public enum RiskLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }

    public enum TaskStatus
    {
        Received = 0,
        Analyzing = 1,
        WaitingApproval = 2,
        Running = 3,
        Completed = 4,
        Failed = 5,
        Rejected = 6,
    }

    [Serializable]
    public sealed class MockRouteDecision
    {
        public string Command = "";
        public List<string> SelectedApps = new List<string>();
        public RiskLevel RiskLevel = RiskLevel.Low;
        public bool ApprovalRequired;
        public string RoutingReason = "";

        public string PrimaryApp
        {
            get
            {
                if (SelectedApps == null || SelectedApps.Count == 0)
                {
                    return "-";
                }
                return SelectedApps[0];
            }
        }
    }

    [Serializable]
    public sealed class MockTaskState
    {
        public string TaskId = "";
        public string Command = "";
        public string SelectedApp = "-";
        public string SelectedAppsJoined = "-";
        public RiskLevel RiskLevel = RiskLevel.Low;
        public bool ApprovalRequired;
        public TaskStatus Status = TaskStatus.Received;
        public string StartedAt = "-";
        public string CompletedAt = "-";
        public string ResultSummary = "-";
        public string NextStep = "-";
        public string FailureReason = "-";
        public string RouteReason = "-";
        public string ApprovalReason = "-";

        public string StatusLabel => Status.ToString();
        public string RiskLabel => RiskLevel.ToString().ToUpperInvariant();
    }

    public static class MockTime
    {
        public static string NowStamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}

