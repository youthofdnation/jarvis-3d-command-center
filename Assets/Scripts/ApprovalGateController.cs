using System;
using UnityEngine;
using UnityEngine.UI;

namespace Jarvis3DCommandCenter
{
    [DisallowMultipleComponent]
    public sealed class ApprovalGateController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text titleText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text reasonText;
        [SerializeField] private Button approveButton;
        [SerializeField] private Button rejectButton;
        [SerializeField] private Button showReasonButton;

        private Action<bool, string> pendingDecision;
        private MockTaskState pendingTask;
        private bool reasonVisible;

        public void Configure(
            GameObject root,
            Text title,
            Text status,
            Text reason,
            Button approve,
            Button reject,
            Button showReason)
        {
            panelRoot = root;
            titleText = title;
            statusText = status;
            reasonText = reason;
            approveButton = approve;
            rejectButton = reject;
            showReasonButton = showReason;

            WireButtons();
            Hide();
        }

        public void Open(MockTaskState task, Action<bool, string> onDecision)
        {
            pendingTask = task;
            pendingDecision = onDecision;
            reasonVisible = false;

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = $"Approval Gate · {task.TaskId}";
            }

            if (statusText != null)
            {
                statusText.text = $"Status: Waiting Approval\nRisk: {task.RiskLabel}\nApp: {task.SelectedApp}";
            }

            if (reasonText != null)
            {
                reasonText.text =
                    "Show Reason를 누르면 승인 필요 사유를 표시합니다.\n" +
                    $"Route Reason: {task.RouteReason}\n" +
                    "Policy: MEDIUM/HIGH risk 작업은 승인 게이트를 통과해야 합니다.";
                reasonText.gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            pendingTask = null;
            pendingDecision = null;
            reasonVisible = false;
        }

        private void WireButtons()
        {
            if (approveButton != null)
            {
                approveButton.onClick.RemoveAllListeners();
                approveButton.onClick.AddListener(() => HandleDecision(true));
            }

            if (rejectButton != null)
            {
                rejectButton.onClick.RemoveAllListeners();
                rejectButton.onClick.AddListener(() => HandleDecision(false));
            }

            if (showReasonButton != null)
            {
                showReasonButton.onClick.RemoveAllListeners();
                showReasonButton.onClick.AddListener(ToggleReason);
            }
        }

        private void HandleDecision(bool approved)
        {
            var reason = approved ? "User approved operation" : "User denied approval";
            pendingDecision?.Invoke(approved, reason);
            Hide();
        }

        private void ToggleReason()
        {
            reasonVisible = !reasonVisible;
            if (reasonText != null)
            {
                reasonText.gameObject.SetActive(reasonVisible);
            }
        }
    }
}

