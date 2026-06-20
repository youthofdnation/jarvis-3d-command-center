# Jarvis3DCommandCenter (Mock PoC)

## 개요

이 프로젝트는 `jarvis-router`의 코어를 옮기지 않고, **Unity 3D 공간형 Command Center 클라이언트**를 PoC로 보여주기 위한 mock 구현입니다.

- 실제 Jarvis backend/API/WebSocket 연결 없음
- 실제 Gmail/Calendar/DB/운영서버/결제 API 호출 없음
- Approval Gate/Task Timeline/Intent Routing 모두 mock 상태 전이

## 프로젝트 경로

```text
/Users/o/DEV/UNITY/jarvis-3d-command-center/Jarvis3DCommandCenter
```

## 씬 이름

- 목표 씬: `CommandCenterMockScene`
- 파일 경로: `Assets/Scenes/CommandCenterMockScene.unity`
- 생성 방법(Unity Editor 메뉴):
  - `Jarvis3D/Create Mock Command Center Scene`

## 핵심 스크립트

`Assets/Scripts/`

- `JarvisCommandInput.cs`
- `MockIntentRouter.cs`
- `AppNode.cs`
- `AppNodeManager.cs`
- `TaskCard.cs`
- `TaskTimelineManager.cs`
- `ApprovalGateController.cs`
- `MockJarvisEvent.cs`
- `JarvisEventBus.cs`
- `CameraController.cs`
- `CommandCenterBootstrap.cs` (씬/오브젝트/UI 자동 구성)
- `AppCatalog.cs`
- `IJarvisCommandRouter.cs` (향후 실연동용 라우터 인터페이스)

`Assets/Editor/`

- `CreateCommandCenterScene.cs` (씬 생성/오픈 메뉴)
- `WebGLBuildTools.cs` (WebGL 빌드 메뉴 + CI 배치 진입점)

## 실행 방법

1. Unity Hub에서 이 폴더를 프로젝트로 추가/오픈
2. 메뉴 실행
   - `Jarvis3D/Create Mock Command Center Scene`
3. 생성된 씬 열기
   - `Jarvis3D/Open Mock Command Center Scene`
4. Play 실행

## WebGL 빌드/실행 방법

### 1) Unity Editor에서 WebGL 빌드

1. `Jarvis3D/Create Mock Command Center Scene`
2. `Jarvis3D/WebGL/Build (Development)` 또는 `Jarvis3D/WebGL/Build (Release)`
3. 빌드 산출물 확인: `Builds/WebGL/index.html`

### 2) 로컬 브라우저 실행

프로젝트 루트에서:

```bash
bash ./run_webgl_local.sh 8080
```

브라우저에서:

```text
http://localhost:8080
```

### 3) CI/배치 빌드(Unity CLI)

릴리즈 빌드:

```bash
Unity -batchmode -quit \
  -projectPath "/Users/o/DEV/UNITY/jarvis-3d-command-center/Jarvis3DCommandCenter" \
  -executeMethod WebGLBuildTools.BuildReleaseFromCommandLine
```

개발 빌드:

```bash
Unity -batchmode -quit \
  -projectPath "/Users/o/DEV/UNITY/jarvis-3d-command-center/Jarvis3DCommandCenter" \
  -executeMethod WebGLBuildTools.BuildDevelopmentFromCommandLine
```

## GitHub Pages 배포 (로컬 Unity 없이 가능)

워크플로 파일:

- `.github/workflows/webgl-pages-build-deploy.yml`
- `.github/workflows/webgl-pages-deploy-prebuilt.yml`

### A) 클라우드에서 빌드+배포(권장)

로컬 Unity를 설치하지 않고도, GitHub Actions에서 Unity 빌드 후 Pages 배포할 수 있습니다.

1. GitHub 저장소 Settings -> Pages -> Source를 `GitHub Actions`로 설정
2. GitHub 저장소 Settings -> Secrets and variables -> Actions에 Unity 라이선스 정보 등록
   - 권장: `UNITY_LICENSE`
   - 대안: `UNITY_EMAIL`, `UNITY_PASSWORD`, `UNITY_SERIAL`
3. Actions에서 `WebGL Build and Deploy (GitHub Pages)` 실행
4. 완료 후 Pages URL에서 실행

### B) 사전 빌드 산출물만 배포(Unity 빌드 없이)

이미 만들어진 WebGL 파일만 배포할 때:

1. `Builds/WebGL`이 있으면 아래로 `webgl-dist` 내보내기

```bash
bash ./export_webgl_dist.sh
```

2. `webgl-dist`를 커밋/푸시
3. Actions에서 `WebGL Deploy Prebuilt (No Unity Build)` 실행

## Unity 설치 없이 라이선스 준비(Manual Activation)

`UNITY_LICENSE`가 없으면 WebGL 빌드가 실패합니다. 로컬 설치 없이도 아래 순서로 준비할 수 있습니다.

1. GitHub Actions에서 `Unity License Request (No Local Install)` 실행
2. 실행 결과 Artifact(`unity-activation-file`)에서 `.alf` 파일 다운로드
3. Unity Manual Activation 페이지에서 `.alf` 업로드 -> `.ulf` 파일 발급
4. 발급한 `.ulf`를 저장소 시크릿으로 등록:

```bash
gh secret set UNITY_LICENSE -R youthofdnation/jarvis-3d-command-center < "/absolute/path/to/Unity_lic.ulf"
```

5. 필요 시 계정 시크릿도 실제 값으로 설정:

```bash
gh secret set UNITY_EMAIL -R youthofdnation/jarvis-3d-command-center -b "real-email@example.com"
gh secret set UNITY_PASSWORD -R youthofdnation/jarvis-3d-command-center -b "real-password"
```

6. 이후 `WebGL Build and Deploy (GitHub Pages)` 워크플로 재실행

## 정적 서버 배포 (Nginx/S3/사내 서버 등)

### 1) prebuilt dist 패키징

```bash
bash ./package_webgl_dist.sh
```

산출물:

```text
dist/jarvis3d-webgl-dist.tar.gz
```

이 파일을 정적 서버의 웹 루트에 압축 해제하면 됩니다.

### 2) 로컬에서 prebuilt dist 실행(Unity 불필요)

```bash
bash ./run_webgl_dist_local.sh 8080
```

## PoC 동작 플로우

```text
User Command
-> Received
-> Analyzing (MockIntentRouter)
-> Selected App Highlight + Core Link
-> Risk Check
-> Approval Gate (if required)
-> Running
-> Completed / Rejected / Failed
```

Task Card에는 다음 필드가 표시됩니다.

- Task ID
- Command
- Selected App
- Risk Level
- Approval Required
- Status
- Started At
- Completed At
- Result Summary
- Next Step
- Failure Reason

## Mock Intent Routing 규칙

- `"StoreBot", "매장", "고객응대"` -> `StoreBot`
- `"Workflow", "문서", "PPT", "소개자료", "사업계획서", "제출자료", "초안"` -> `Workflow Studio`
- `"Gmail", "메일"` -> `Gmail`
- `"Calendar", "일정", "마감"` -> `Calendar`
- `"CCTV", "영상", "운영 리포트", "VLM"` -> `CCTV / Operation Report`
- `"모델", "RAG", "자체모델", "LLM"` -> `Local Model + Cloud Model + RAG Memory`

Risk/Approval mock:

- `LOW` -> 조회/요약/분석
- `MEDIUM` -> 문서/메일/일정 초안 등
- `HIGH` -> 발송/생성/DB 변경/운영 변경/결제/외부 전달 키워드
- `MEDIUM/HIGH` 일부는 Approval Gate 활성화

## 데모 명령

아래 5개는 하단 Demo 버튼으로 바로 실행 가능:

1. `StoreBot 고객응대 품질을 평가해줘`
2. `Workflow Studio로 공공시장 제출자료 초안을 만들어줘`
3. `오늘 중요한 메일을 요약해줘`
4. `영상 기반 운영 리포트 실험 결과를 정리해줘`
5. `자체모델과 클라우드 모델 라우팅 성능을 비교해줘`

## 보안/분리 원칙

- 기존 `jarvis-router` 로직/DB/.env/키/토큰 직접 접근 없음
- 운영 연동 없음
- mock only

## Phase 2 전 남은 작업

1. Jarvis Event API/WebSocket 인터페이스 추가
2. route/event schema 정합성 매핑
3. Approval Gate 실연동(백엔드 승인 엔드포인트)
4. Task 실행 결과/로그 스트림 연동
5. 앱 노드 상태 실시간 동기화

## 참고

- Unity 설치 없이도 **이미 빌드된 WebGL 산출물**은 브라우저에서 실행 가능합니다.
- 단, **새 WebGL 빌드 생성**은 Unity가 반드시 필요합니다(로컬 설치 또는 GitHub Actions 클라우드 빌드 중 하나).
- 즉, "실행만"은 Unity 없이 가능, "수정 후 재빌드"는 Unity가 필요합니다.

