using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jarvis3DCommandCenter
{
    [DisallowMultipleComponent]
    public sealed class JarvisCommandInput : MonoBehaviour
    {
        [SerializeField] private InputField commandInput;
        [SerializeField] private Button executeButton;
        [SerializeField] private Text projectedCommandText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text selectedAppText;
        [SerializeField] private Text routeText;
        [SerializeField] private Text appDetailText;
        [SerializeField] private ApprovalGateController approvalGate;
        [SerializeField] private MockIntentRouter routerBehaviour;
        [SerializeField] private AppNodeManager appNodeManager;
        [SerializeField] private TaskTimelineManager timelineManager;
        [SerializeField] private JarvisEventBus eventBus;

        [SerializeField] private float analyzingDelaySec = 0.7f;
        [SerializeField] private float runningDelaySec = 1.2f;

        private int taskSeq;
        private IJarvisCommandRouter intentRouter;

        public void Configure(
            InputField input,
            Button execute,
            Text projectedCommand,
            Text status,
            Text selectedApp,
            Text routeSummary,
            Text appDetail,
            ApprovalGateController gate,
            IJarvisCommandRouter router,
            AppNodeManager nodes,
            TaskTimelineManager timeline,
            JarvisEventBus bus)
        {
            commandInput = input;
            executeButton = execute;
            projectedCommandText = projectedCommand;
            statusText = status;
            selectedAppText = selectedApp;
            routeText = routeSummary;
            appDetailText = appDetail;
            approvalGate = gate;
            intentRouter = router;
            routerBehaviour = router as MockIntentRouter;
            appNodeManager = nodes;
            timelineManager = timeline;
            eventBus = bus;

            WireUi();
        }

        public void WireUi()
        {
            if (executeButton != null)
            {
                executeButton.onClick.RemoveAllListeners();
                executeButton.onClick.AddListener(SubmitCurrentInput);
            }

            if (appNodeManager != null)
            {
                appNodeManager.NodeSelected -= HandleNodeSelected;
                appNodeManager.NodeSelected += HandleNodeSelected;
            }
        }

        private void Update()
        {
            if (commandInput == null || !commandInput.isFocused)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SubmitCurrentInput();
            }
        }

        public void SubmitCurrentInput()
        {
            var command = commandInput != null ? commandInput.text : "";
            SubmitCommand(command);
        }

        public void SubmitDemoCommand(string command)
        {
            SubmitCommand(command);
        }

        public void SubmitCommand(string command)
        {
            var cmd = (command ?? "").Trim();
            if (string.IsNullOrWhiteSpace(cmd))
            {
                SetText(statusText, "명령을 입력해 주세요.");
                return;
            }

            if (commandInput != null)
            {
                commandInput.text = cmd;
            }

            StartCoroutine(RunMockFlow(cmd));
        }

        private IEnumerator RunMockFlow(string command)
        {
            var task = new MockTaskState
            {
                TaskId = BuildTaskId(),
                Command = command,
                Status = TaskStatus.Received,
                StartedAt = MockTime.NowStamp(),
                NextStep = "Intent Analysis",
            };

            SyncTask(task);
            eventBus?.PublishCommandProjected(command);
            eventBus?.PublishLog($"[{task.TaskId}] Received");
            SetText(projectedCommandText, $"Projected Command: {command}");
            SetText(statusText, "Received -> Analyzing");

            yield return new WaitForSeconds(analyzingDelaySec);

            task.Status = TaskStatus.Analyzing;
            task.NextStep = "Selected App Highlight";
            SyncTask(task);

            var decision = intentRouter != null
                ? intentRouter.RouteCommand(command)
                : new MockRouteDecision
                {
                    Command = command,
                    SelectedApps = new List<string> { AppCatalog.Tasks },
                    RiskLevel = RiskLevel.Low,
                    ApprovalRequired = false,
                    RoutingReason = "fallback router",
                };

            task.SelectedApp = decision.PrimaryApp;
            task.SelectedAppsJoined = string.Join(", ", decision.SelectedApps);
            task.RiskLevel = decision.RiskLevel;
            task.ApprovalRequired = decision.ApprovalRequired;
            task.RouteReason = decision.RoutingReason;
            task.NextStep = decision.ApprovalRequired ? "Approval Gate" : "Run Task";

            appNodeManager?.HighlightApps(decision.SelectedApps);
            if (decision.SelectedApps.Count > 0)
            {
                appNodeManager?.ForceSelect(decision.SelectedApps[0]);
            }

            SetText(selectedAppText, $"Selected App: {task.SelectedAppsJoined}");
            SetText(routeText, $"Risk: {task.RiskLabel}\nApproval Required: {(task.ApprovalRequired ? "YES" : "NO")}\n{task.RouteReason}");
            SyncTask(task);
            eventBus?.PublishLog($"[{task.TaskId}] Routed -> {task.SelectedAppsJoined} / risk={task.RiskLabel}");

            if (task.ApprovalRequired && approvalGate != null)
            {
                task.Status = TaskStatus.WaitingApproval;
                task.ApprovalReason = "MEDIUM/HIGH risk requires owner approval in PoC policy.";
                SyncTask(task);
                SetText(statusText, "Waiting Approval");
                eventBus?.PublishLog($"[{task.TaskId}] Waiting Approval");

                var completed = false;
                var approved = false;
                var decisionReason = "";
                approvalGate.Open(task, (ok, reason) =>
                {
                    approved = ok;
                    decisionReason = reason ?? "";
                    completed = true;
                });

                while (!completed)
                {
                    yield return null;
                }

                if (!approved)
                {
                    task.Status = TaskStatus.Rejected;
                    task.CompletedAt = MockTime.NowStamp();
                    task.ResultSummary = "Mock execution rejected at Approval Gate.";
                    task.FailureReason = string.IsNullOrWhiteSpace(decisionReason) ? "User denied approval" : decisionReason;
                    task.NextStep = "Modify command and retry";
                    SyncTask(task);
                    SetText(statusText, "Rejected");
                    eventBus?.PublishLog($"[{task.TaskId}] Rejected");
                    yield break;
                }

                eventBus?.PublishLog($"[{task.TaskId}] Approved");
            }

            task.Status = TaskStatus.Running;
            task.NextStep = "Finalize Result Card";
            SyncTask(task);
            SetText(statusText, "Running");
            eventBus?.PublishLog($"[{task.TaskId}] Running");

            yield return new WaitForSeconds(runningDelaySec);

            var failed = IsMockFailure(command);
            if (failed)
            {
                task.Status = TaskStatus.Failed;
                task.CompletedAt = MockTime.NowStamp();
                task.ResultSummary = "Mock execution failed.";
                task.FailureReason = "Mock failure scenario triggered by command keyword.";
                task.NextStep = "Check logs and rerun task";
                SyncTask(task);
                SetText(statusText, "Failed");
                eventBus?.PublishLog($"[{task.TaskId}] Failed");
                yield break;
            }

            task.Status = TaskStatus.Completed;
            task.CompletedAt = MockTime.NowStamp();
            task.ResultSummary = BuildResultSummary(task);
            task.FailureReason = "-";
            task.NextStep = "Connect real Jarvis evaluation API in Phase 2.";
            SyncTask(task);
            SetText(statusText, "Completed");
            eventBus?.PublishLog($"[{task.TaskId}] Completed");
        }

        private void HandleNodeSelected(AppNode node)
        {
            if (node == null)
            {
                return;
            }

            var detail = appNodeManager != null ? appNodeManager.BuildNodeDetailText(node.AppId) : node.Description;
            SetText(appDetailText, detail);
        }

        private string BuildTaskId()
        {
            taskSeq += 1;
            return $"task-{DateTime.Now:HHmmss}-{taskSeq:D2}";
        }

        private static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? "";
            }
        }

        private static bool IsMockFailure(string command)
        {
            var normalized = (command ?? "").Trim().ToLowerInvariant();
            return normalized.Contains("실패") || normalized.Contains("error") || normalized.Contains("fail");
        }

        private static string BuildResultSummary(MockTaskState task)
        {
            var app = task.SelectedApp ?? "";
            if (app == AppCatalog.StoreBot)
            {
                return "Mock evaluation completed. 12 FAQ scenarios checked. 2 need improvement.";
            }

            if (app == AppCatalog.WorkflowStudio)
            {
                return "Mock draft completed. 6 public-market submission sections generated.";
            }

            if (app == AppCatalog.Gmail)
            {
                return "Mock Gmail summary completed. 9 important mails extracted and grouped.";
            }

            if (app == AppCatalog.Calendar)
            {
                return "Mock calendar scan completed. 4 deadline events found for this week.";
            }

            if (app == AppCatalog.CctvOpsReport)
            {
                return "Mock VLM operation report completed. 3 anomalies summarized.";
            }

            if (task.SelectedAppsJoined.Contains(AppCatalog.LocalModel) || task.SelectedAppsJoined.Contains(AppCatalog.CloudModel))
            {
                return "Mock routing benchmark completed. Local vs Cloud model latency and quality compared.";
            }

            return "Mock task completed successfully.";
        }

        private void SyncTask(MockTaskState task)
        {
            timelineManager?.UpsertTask(task);
            eventBus?.PublishTaskUpdated(task);
        }
    }
}

