using UnityEngine;
using UnityEngine.UI;

namespace Jarvis3DCommandCenter
{
    [DisallowMultipleComponent]
    public sealed class TaskCard : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Image cardBackground;

        public void Configure(Text title, Text body, Image bg)
        {
            titleText = title;
            bodyText = body;
            cardBackground = bg;
        }

        public void Bind(MockTaskState task)
        {
            if (task == null)
            {
                return;
            }

            if (titleText != null)
            {
                titleText.text = $"{task.TaskId} · {task.StatusLabel}";
            }

            if (bodyText != null)
            {
                bodyText.text =
                    $"Command: {task.Command}\n" +
                    $"Selected App: {task.SelectedApp}\n" +
                    $"Risk Level: {task.RiskLabel}\n" +
                    $"Approval Required: {(task.ApprovalRequired ? "YES" : "NO")}\n" +
                    $"Status: {task.StatusLabel}\n" +
                    $"Started At: {task.StartedAt}\n" +
                    $"Completed At: {task.CompletedAt}\n" +
                    $"Result Summary: {task.ResultSummary}\n" +
                    $"Next Step: {task.NextStep}\n" +
                    $"Failure Reason: {task.FailureReason}";
            }

            if (cardBackground != null)
            {
                cardBackground.color = ResolveStatusColor(task.Status);
            }
        }

        private static Color ResolveStatusColor(TaskStatus status)
        {
            switch (status)
            {
                case TaskStatus.Completed:
                    return new Color(0.11f, 0.25f, 0.18f, 0.94f);
                case TaskStatus.Failed:
                case TaskStatus.Rejected:
                    return new Color(0.28f, 0.12f, 0.14f, 0.95f);
                case TaskStatus.WaitingApproval:
                    return new Color(0.25f, 0.20f, 0.11f, 0.95f);
                case TaskStatus.Running:
                    return new Color(0.12f, 0.20f, 0.28f, 0.95f);
                default:
                    return new Color(0.10f, 0.16f, 0.25f, 0.93f);
            }
        }
    }
}

