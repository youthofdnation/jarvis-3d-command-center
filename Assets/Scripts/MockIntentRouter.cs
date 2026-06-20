using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jarvis3DCommandCenter
{
    public sealed class MockIntentRouter : MonoBehaviour, IJarvisCommandRouter
    {
        private static readonly string[] StoreBotKeywords =
        {
            "storebot", "매장", "고객응대", "고객 응대", "faq", "응답 품질",
        };

        private static readonly string[] WorkflowKeywords =
        {
            "workflow", "문서", "ppt", "소개자료", "사업계획서", "제출자료", "초안",
        };

        private static readonly string[] GmailKeywords =
        {
            "gmail", "메일", "email", "이메일",
        };

        private static readonly string[] CalendarKeywords =
        {
            "calendar", "일정", "마감", "캘린더", "마감일",
        };

        private static readonly string[] CctvKeywords =
        {
            "cctv", "영상", "운영 리포트", "operation report", "vlm",
        };

        private static readonly string[] ModelKeywords =
        {
            "모델", "rag", "자체모델", "자체 모델", "llm", "라우팅 성능", "비교해줘",
        };

        private static readonly string[] HighRiskKeywords =
        {
            "메일 발송", "보내", "일정 생성", "db 변경", "db", "데이터베이스", "운영서버",
            "운영 서버", "서버 변경", "결제", "비용 발생", "외부 고객", "전달",
        };

        private static readonly string[] MediumRiskKeywords =
        {
            "초안", "요약", "정리", "파일 생성", "일정 초안", "메일 초안", "공공시장",
            "제출자료", "사업계획서", "리포트",
        };

        private static readonly HashSet<string> MediumApprovalApps = new HashSet<string>
        {
            AppCatalog.WorkflowStudio,
            AppCatalog.Gmail,
            AppCatalog.Calendar,
        };

        public MockRouteDecision RouteCommand(string command)
        {
            var normalized = Normalize(command);
            var selected = new List<string>();

            if (ContainsAny(normalized, StoreBotKeywords))
            {
                selected.Add(AppCatalog.StoreBot);
            }

            if (ContainsAny(normalized, WorkflowKeywords))
            {
                selected.Add(AppCatalog.WorkflowStudio);
            }

            if (ContainsAny(normalized, GmailKeywords))
            {
                selected.Add(AppCatalog.Gmail);
            }

            if (ContainsAny(normalized, CalendarKeywords))
            {
                selected.Add(AppCatalog.Calendar);
            }

            if (ContainsAny(normalized, CctvKeywords))
            {
                selected.Add(AppCatalog.CctvOpsReport);
            }

            if (ContainsAny(normalized, ModelKeywords))
            {
                selected.Add(AppCatalog.LocalModel);
                selected.Add(AppCatalog.CloudModel);
                selected.Add(AppCatalog.RagMemory);
            }

            if (selected.Count == 0)
            {
                selected.Add(AppCatalog.Tasks);
                selected.Add(AppCatalog.LogsAudit);
            }

            selected = selected.Distinct().ToList();

            var risk = EvaluateRisk(normalized, selected);
            var approvalRequired = EvaluateApprovalRequired(normalized, selected, risk);

            return new MockRouteDecision
            {
                Command = command ?? "",
                SelectedApps = selected,
                RiskLevel = risk,
                ApprovalRequired = approvalRequired,
                RoutingReason = BuildReason(selected, risk, approvalRequired),
            };
        }

        private static string Normalize(string command)
        {
            return (command ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static bool ContainsAny(string text, IEnumerable<string> keywords)
        {
            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    continue;
                }

                if (text.Contains(keyword.ToLowerInvariant()))
                {
                    return true;
                }
            }

            return false;
        }

        private static RiskLevel EvaluateRisk(string normalizedCommand, IReadOnlyCollection<string> selectedApps)
        {
            if (ContainsAny(normalizedCommand, HighRiskKeywords))
            {
                return RiskLevel.High;
            }

            if (ContainsAny(normalizedCommand, MediumRiskKeywords))
            {
                return RiskLevel.Medium;
            }

            if (selectedApps.Contains(AppCatalog.Gmail) || selectedApps.Contains(AppCatalog.Calendar))
            {
                return RiskLevel.Medium;
            }

            if (selectedApps.Contains(AppCatalog.WorkflowStudio) && normalizedCommand.Contains("초안"))
            {
                return RiskLevel.Medium;
            }

            return RiskLevel.Low;
        }

        private static bool EvaluateApprovalRequired(
            string normalizedCommand,
            IReadOnlyCollection<string> selectedApps,
            RiskLevel risk)
        {
            if (risk == RiskLevel.High)
            {
                return true;
            }

            if (risk == RiskLevel.Low)
            {
                return false;
            }

            if (selectedApps.Any(x => MediumApprovalApps.Contains(x)))
            {
                return true;
            }

            return normalizedCommand.Contains("초안") || normalizedCommand.Contains("메일");
        }

        private static string BuildReason(IReadOnlyList<string> apps, RiskLevel risk, bool approvalRequired)
        {
            var appLabel = string.Join(", ", apps);
            return $"Mock intent routed to [{appLabel}] · risk={risk} · approval={(approvalRequired ? "required" : "not_required")}";
        }
    }
}

