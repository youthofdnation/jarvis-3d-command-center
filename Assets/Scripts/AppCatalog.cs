using System.Collections.Generic;

namespace Jarvis3DCommandCenter
{
    public static class AppCatalog
    {
        public const string JarvisCore = "Jarvis Core";
        public const string StoreBot = "StoreBot";
        public const string WorkflowStudio = "Workflow Studio";
        public const string Gmail = "Gmail";
        public const string Calendar = "Calendar";
        public const string CctvOpsReport = "CCTV / Operation Report";
        public const string RagMemory = "RAG Memory";
        public const string LocalModel = "Local Model";
        public const string CloudModel = "Cloud Model";
        public const string Tasks = "Tasks";
        public const string ApprovalGate = "Approval Gate";
        public const string LogsAudit = "Logs / Audit";

        private static readonly Dictionary<string, string> DescriptionById = new Dictionary<string, string>
        {
            [StoreBot] = "StoreBot 테스트/고객응대 품질 평가 시뮬레이션 노드",
            [WorkflowStudio] = "문서/PPT/제출자료 초안 생성 워크플로우 노드",
            [Gmail] = "메일 요약/분류/초안 생성 시뮬레이션 노드",
            [Calendar] = "일정/마감일 조회 및 생성 시뮬레이션 노드",
            [CctvOpsReport] = "영상 기반 운영 리포트 및 VLM 분석 시뮬레이션 노드",
            [RagMemory] = "장기 메모리 및 컨텍스트 검색 노드",
            [LocalModel] = "로컬 모델 추론/평가 노드",
            [CloudModel] = "클라우드 모델 추론/평가 노드",
            [Tasks] = "작업 큐/진행 상태 관리 노드",
            [ApprovalGate] = "중/고위험 작업 승인 게이트",
            [LogsAudit] = "감사 로그/이벤트 타임라인 노드",
        };

        private static readonly Dictionary<string, string[]> MockTaskById = new Dictionary<string, string[]>
        {
            [StoreBot] = new[] { "FAQ 응답 품질 점검", "테스트 매장 QnA 시나리오 재생" },
            [WorkflowStudio] = new[] { "공공시장 제출자료 초안 생성", "소개자료 섹션별 자동 채움" },
            [Gmail] = new[] { "중요 메일 필터링", "메일 요약 카드 생성" },
            [Calendar] = new[] { "이번 주 마감 일정 스캔", "마감 리마인더 초안 생성" },
            [CctvOpsReport] = new[] { "영상 이벤트 요약", "운영 리포트 템플릿 채움" },
            [RagMemory] = new[] { "업무 컨텍스트 검색", "과거 태스크 참조 매핑" },
            [LocalModel] = new[] { "로컬 모델 추론 품질 측정", "지연시간 측정" },
            [CloudModel] = new[] { "클라우드 모델 응답 비교", "비용/품질 추정" },
            [Tasks] = new[] { "태스크 상태 갱신", "다음 실행 후보 제시" },
            [ApprovalGate] = new[] { "승인 요청 대기", "승인/거절 감사 기록" },
            [LogsAudit] = new[] { "이벤트 타임라인 업데이트", "실패 사유 감사 로그 저장" },
        };

        public static string GetDescription(string appId)
        {
            return DescriptionById.TryGetValue(appId, out var value) ? value : "설명 없음";
        }

        public static IReadOnlyList<string> GetMockTasks(string appId)
        {
            return MockTaskById.TryGetValue(appId, out var value) ? value : new string[] { "mock task 없음" };
        }
    }
}

