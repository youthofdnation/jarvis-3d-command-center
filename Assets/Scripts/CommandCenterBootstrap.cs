using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jarvis3DCommandCenter
{
    [DisallowMultipleComponent]
    public sealed class CommandCenterBootstrap : MonoBehaviour
    {
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool clearChildrenBeforeBuild = true;

        private readonly List<string> demoCommands = new List<string>
        {
            "StoreBot 고객응대 품질을 평가해줘",
            "Workflow Studio로 공공시장 제출자료 초안을 만들어줘",
            "오늘 중요한 메일을 요약해줘",
            "영상 기반 운영 리포트 실험 결과를 정리해줘",
            "자체모델과 클라우드 모델 라우팅 성능을 비교해줘",
        };

        private JarvisEventBus eventBus;
        private MockIntentRouter router;
        private AppNodeManager nodeManager;
        private TaskTimelineManager timelineManager;
        private ApprovalGateController approvalGateController;
        private JarvisCommandInput commandInputController;

        private Text appDetailText;
        private Text logText;
        private Text projectedCommandTextRef;

        private void Start()
        {
            if (!buildOnStart)
            {
                return;
            }

            Build();
        }

        [ContextMenu("Build Command Center")]
        public void Build()
        {
            if (clearChildrenBeforeBuild)
            {
                ClearChildren();
            }

            EnsureCoreComponents();

            var spatialRoot = CreateSpatialRoot("SpatialRoot", transform);
            var nodesRoot = CreateSpatialRoot("NodesRoot", spatialRoot);
            var linksRoot = CreateSpatialRoot("LinksRoot", spatialRoot);

            var core = CreateCoreObject(nodesRoot);
            nodeManager.Configure(core.transform, linksRoot);

            CreateEnvironment(spatialRoot);
            CreateAppNodes(nodesRoot);
            SetupCamera(core.transform);
            SetupLights(core.transform, transform);
            BuildUi();

            eventBus.PublishLog("Jarvis 3D Command Center mock PoC initialized.");
        }

        private void EnsureCoreComponents()
        {
            eventBus = GetOrAdd<JarvisEventBus>(gameObject);
            router = GetOrAdd<MockIntentRouter>(gameObject);
            nodeManager = GetOrAdd<AppNodeManager>(gameObject);
            timelineManager = GetOrAdd<TaskTimelineManager>(gameObject);
            approvalGateController = GetOrAdd<ApprovalGateController>(gameObject);
            commandInputController = GetOrAdd<JarvisCommandInput>(gameObject);
        }

        private void BuildUi()
        {
            var canvas = CreateCanvas("CommandCenterCanvas");
            var root = canvas.GetComponent<RectTransform>();

            var leftPanel = CreatePanel(root, "LeftPanel", new Vector2(0.02f, 0.14f), new Vector2(0.32f, 0.96f), new Color(0.05f, 0.09f, 0.16f, 0.9f));
            var centerPanel = CreatePanel(root, "CenterPanel", new Vector2(0.34f, 0.14f), new Vector2(0.66f, 0.30f), new Color(0.06f, 0.11f, 0.18f, 0.92f));
            var rightPanel = CreatePanel(root, "RightPanel", new Vector2(0.68f, 0.14f), new Vector2(0.98f, 0.96f), new Color(0.05f, 0.09f, 0.16f, 0.9f));
            var bottomPanel = CreatePanel(root, "BottomPanel", new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.12f), new Color(0.04f, 0.08f, 0.14f, 0.95f));

            CreateLabel(leftPanel, "앱 상세 / 상태", 18, new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.985f), TextAnchor.MiddleLeft, FontStyle.Bold);
            appDetailText = CreateLabel(leftPanel, "앱 오브젝트를 클릭하면 설명/Mock task가 표시됩니다.", 14, new Vector2(0.04f, 0.40f), new Vector2(0.96f, 0.90f), TextAnchor.UpperLeft);
            CreateLabel(leftPanel, "Event Logs", 16, new Vector2(0.04f, 0.33f), new Vector2(0.96f, 0.39f), TextAnchor.MiddleLeft, FontStyle.Bold);
            logText = CreateLabel(leftPanel, "-", 12, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.32f), TextAnchor.UpperLeft);

            var projected = CreateLabel(centerPanel, "Projected Command: -", 15, new Vector2(0.03f, 0.62f), new Vector2(0.97f, 0.95f), TextAnchor.UpperLeft, FontStyle.Bold);
            projectedCommandTextRef = projected;
            var status = CreateLabel(centerPanel, "Status: Idle", 14, new Vector2(0.03f, 0.34f), new Vector2(0.97f, 0.60f), TextAnchor.UpperLeft);
            var selectedApp = CreateLabel(centerPanel, "Selected App: -", 14, new Vector2(0.03f, 0.18f), new Vector2(0.97f, 0.34f), TextAnchor.UpperLeft);
            var routeSummary = CreateLabel(centerPanel, "Risk: -\nApproval Required: -\nRoute: -", 13, new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.18f), TextAnchor.UpperLeft);

            CreateLabel(rightPanel, "Task Timeline / Result Cards", 18, new Vector2(0.04f, 0.92f), new Vector2(0.96f, 0.985f), TextAnchor.MiddleLeft, FontStyle.Bold);
            var timelineContainer = CreateScrollContent(rightPanel, "TimelineCards", new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.90f));
            var cardPrefab = CreateTaskCardTemplate(timelineContainer);
            timelineManager.Configure(timelineContainer, cardPrefab);

            var input = CreateInputField(bottomPanel, "명령 입력 (예: StoreBot 테스트 매장 응답 품질을 평가해줘)",
                new Vector2(0.01f, 0.30f), new Vector2(0.63f, 0.86f));
            var executeBtn = CreateButton(bottomPanel, "Execute", new Vector2(0.64f, 0.30f), new Vector2(0.73f, 0.86f), new Color(0.16f, 0.45f, 0.73f, 1f));

            var demoWrap = CreateRect("DemoButtons", bottomPanel, new Vector2(0.74f, 0.20f), new Vector2(0.99f, 0.92f));
            var demoLayout = demoWrap.gameObject.AddComponent<HorizontalLayoutGroup>();
            demoLayout.childControlWidth = true;
            demoLayout.childForceExpandWidth = false;
            demoLayout.spacing = 6f;
            demoLayout.padding = new RectOffset(0, 0, 0, 0);

            foreach (var command in demoCommands)
            {
                var btn = CreateButton(demoWrap, $"Demo", new Vector2(0, 0), new Vector2(1, 1), new Color(0.12f, 0.28f, 0.48f, 1f));
                var local = command;
                btn.GetComponentInChildren<Text>().text = BuildDemoButtonText(command);
                btn.onClick.AddListener(() => commandInputController.SubmitDemoCommand(local));
                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = 180f;
                le.preferredHeight = 40f;
            }

            var approvalPanel = CreatePanel(root, "ApprovalGatePanel", new Vector2(0.33f, 0.34f), new Vector2(0.67f, 0.66f), new Color(0.10f, 0.10f, 0.14f, 0.97f));
            var approvalStatus = CreateLabel(approvalPanel, "Status: -", 14, new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.74f), TextAnchor.UpperLeft);
            var approvalReason = CreateLabel(approvalPanel, "Reason: -", 13, new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.46f), TextAnchor.UpperLeft);
            var approveBtn = CreateButton(approvalPanel, "Approve", new Vector2(0.06f, 0.04f), new Vector2(0.30f, 0.18f), new Color(0.18f, 0.52f, 0.28f, 1f));
            var rejectBtn = CreateButton(approvalPanel, "Reject", new Vector2(0.34f, 0.04f), new Vector2(0.58f, 0.18f), new Color(0.56f, 0.20f, 0.20f, 1f));
            var reasonBtn = CreateButton(approvalPanel, "Show Reason", new Vector2(0.62f, 0.04f), new Vector2(0.94f, 0.18f), new Color(0.24f, 0.35f, 0.53f, 1f));
            approvalPanel.gameObject.SetActive(false);

            approvalGateController.Configure(
                approvalPanel.gameObject,
                CreateLabel(approvalPanel, "Approval Gate · -", 18, new Vector2(0.06f, 0.76f), new Vector2(0.94f, 0.95f), TextAnchor.MiddleLeft),
                approvalStatus,
                approvalReason,
                approveBtn,
                rejectBtn,
                reasonBtn
            );

            commandInputController.Configure(
                input,
                executeBtn,
                projected,
                status,
                selectedApp,
                routeSummary,
                appDetailText,
                approvalGateController,
                router,
                nodeManager,
                timelineManager,
                eventBus
            );

            if (nodeManager != null)
            {
                nodeManager.NodeSelected += node =>
                {
                    if (node == null || appDetailText == null)
                    {
                        return;
                    }
                    appDetailText.text = nodeManager.BuildNodeDetailText(node.AppId);
                };
            }

            if (eventBus != null)
            {
                eventBus.OnLog -= AppendLog;
                eventBus.OnLog += AppendLog;
                eventBus.OnCommandProjected -= HandleProjectedCommand;
                eventBus.OnCommandProjected += HandleProjectedCommand;
            }

            AppendLog("UI initialized. Mock mode only. No real Jarvis API execution.");
        }

        private void CreateEnvironment(Transform parent)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(parent, false);
            floor.transform.localScale = new Vector3(4.2f, 1f, 4.2f);
            floor.transform.position = new Vector3(0f, 0f, 0f);
            var r = floor.GetComponent<Renderer>();
            if (r != null)
            {
                r.material = new Material(Shader.Find("Standard"));
                r.material.color = new Color(0.03f, 0.05f, 0.08f);
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", new Color(0.02f, 0.07f, 0.14f) * 0.4f);
            }

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "CoreRing";
            ring.transform.SetParent(parent, false);
            ring.transform.position = new Vector3(0f, 0.02f, 0f);
            ring.transform.localScale = new Vector3(5.8f, 0.01f, 5.8f);
            var rr = ring.GetComponent<Renderer>();
            if (rr != null)
            {
                rr.material = new Material(Shader.Find("Standard"));
                rr.material.color = new Color(0.08f, 0.17f, 0.27f);
                rr.material.EnableKeyword("_EMISSION");
                rr.material.SetColor("_EmissionColor", new Color(0.09f, 0.44f, 0.66f) * 0.35f);
            }
        }

        private GameObject CreateCoreObject(Transform parent)
        {
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = AppCatalog.JarvisCore;
            core.transform.SetParent(parent, false);
            core.transform.position = new Vector3(0f, 1.2f, 0f);
            core.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
            var renderer = core.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = new Color(0.19f, 0.43f, 0.72f);
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", new Color(0.28f, 0.66f, 1f) * 0.48f);
            }

            CreateWorldLabel(core.transform, AppCatalog.JarvisCore, new Vector3(0f, 1.6f, 0f), 0.28f);
            return core;
        }

        private void CreateAppNodes(Transform parent)
        {
            var defs = new[]
            {
                new NodeDef(AppCatalog.StoreBot, 0f),
                new NodeDef(AppCatalog.WorkflowStudio, 32f),
                new NodeDef(AppCatalog.Gmail, 66f),
                new NodeDef(AppCatalog.Calendar, 100f),
                new NodeDef(AppCatalog.CctvOpsReport, 136f),
                new NodeDef(AppCatalog.RagMemory, 172f),
                new NodeDef(AppCatalog.LocalModel, 208f),
                new NodeDef(AppCatalog.CloudModel, 244f),
                new NodeDef(AppCatalog.Tasks, 280f),
                new NodeDef(AppCatalog.ApprovalGate, 314f),
                new NodeDef(AppCatalog.LogsAudit, 348f),
            };

            foreach (var def in defs)
            {
                var node = CreateSingleNode(parent, def);
                nodeManager.RegisterNode(node);
            }
        }

        private AppNode CreateSingleNode(Transform parent, NodeDef def)
        {
            var radius = 8.6f;
            var rad = def.Degrees * Mathf.Deg2Rad;
            var position = new Vector3(Mathf.Cos(rad) * radius, 1.1f, Mathf.Sin(rad) * radius);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = $"Node_{def.AppId}";
            body.transform.SetParent(parent, false);
            body.transform.position = position;
            body.transform.localScale = new Vector3(0.9f, 0.65f, 0.9f);

            var rend = body.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = new Material(Shader.Find("Standard"));
                rend.material.color = new Color(0.13f, 0.31f, 0.52f);
                rend.material.EnableKeyword("_EMISSION");
                rend.material.SetColor("_EmissionColor", new Color(0.10f, 0.30f, 0.48f) * 0.22f);
            }

            var label = CreateWorldLabel(body.transform, def.AppId, new Vector3(0f, 1.05f, 0f), 0.16f);
            var node = body.AddComponent<AppNode>();
            node.Configure(
                def.AppId,
                def.AppId,
                AppCatalog.GetDescription(def.AppId),
                new Color(0.13f, 0.31f, 0.52f),
                new Color(0.28f, 0.82f, 1f),
                rend,
                label);
            return node;
        }

        private static TextMesh CreateWorldLabel(Transform parent, string text, Vector3 localPos, float scale)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = Vector3.one * scale;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text ?? "";
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.characterSize = 0.22f;
            tm.fontSize = 54;
            tm.color = new Color(0.74f, 0.85f, 0.95f);
            return tm;
        }

        private void SetupCamera(Transform target)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                cam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.02f, 0.03f, 0.06f);
            cam.fieldOfView = 52f;
            var controller = GetOrAdd<CameraController>(cam.gameObject);
            controller.SetTarget(target);
        }

        private static void SetupLights(Transform core, Transform parent)
        {
            var key = new GameObject("KeyLight");
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(0.72f, 0.84f, 1f);
            keyLight.intensity = 0.8f;
            key.transform.SetParent(parent, false);
            key.transform.rotation = Quaternion.Euler(44f, -34f, 0f);

            var fill = new GameObject("FillLight");
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.range = 40f;
            fillLight.intensity = 1.25f;
            fillLight.color = new Color(0.22f, 0.64f, 0.98f);
            fill.transform.SetParent(parent, false);
            fill.transform.position = core.position + new Vector3(0f, 6f, 0f);
        }

        private Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<GraphicRaycaster>();
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color bg)
        {
            var rect = CreateRect(name, parent, anchorMin, anchorMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = bg;
            return rect;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static Text CreateLabel(
            Transform parent,
            string text,
            int fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax,
            TextAnchor align,
            FontStyle style = FontStyle.Normal)
        {
            var rect = CreateRect("Label", parent, anchorMin, anchorMax);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.text = text ?? "";
            label.fontSize = fontSize;
            label.alignment = align;
            label.fontStyle = style;
            label.color = new Color(0.86f, 0.92f, 0.98f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private static Button CreateButton(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, Color bg)
        {
            var rect = CreateRect("Button", parent, anchorMin, anchorMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = bg;
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = bg;
            colors.highlightedColor = bg * 1.08f;
            colors.pressedColor = bg * 0.9f;
            button.colors = colors;

            CreateLabel(rect, text, 14, new Vector2(0f, 0f), new Vector2(1f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold);
            return button;
        }

        private static InputField CreateInputField(Transform parent, string placeholder, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rect = CreateRect("InputField", parent, anchorMin, anchorMax);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.08f, 0.14f, 0.22f, 1f);

            var input = rect.gameObject.AddComponent<InputField>();

            var textRect = CreateRect("Text", rect, new Vector2(0.02f, 0.10f), new Vector2(0.98f, 0.90f));
            var text = textRect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 15;
            text.color = new Color(0.90f, 0.95f, 0.99f);
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;

            var placeholderRect = CreateRect("Placeholder", rect, new Vector2(0.02f, 0.10f), new Vector2(0.98f, 0.90f));
            var ph = placeholderRect.gameObject.AddComponent<Text>();
            ph.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            ph.fontSize = 14;
            ph.color = new Color(0.62f, 0.73f, 0.85f, 0.84f);
            ph.alignment = TextAnchor.MiddleLeft;
            ph.text = placeholder ?? "";

            input.textComponent = text;
            input.placeholder = ph;
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = 280;
            return input;
        }

        private static RectTransform CreateScrollContent(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var viewport = CreateRect($"{name}_Viewport", parent, anchorMin, anchorMax);
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(0.05f, 0.10f, 0.17f, 0.64f);
            var mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = true;

            var content = CreateRect($"{name}_Content", viewport, new Vector2(0f, 0f), new Vector2(1f, 1f));
            content.offsetMin = new Vector2(8f, 8f);
            content.offsetMax = new Vector2(-8f, -8f);

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childForceExpandHeight = false;
            layout.childControlHeight = true;
            layout.childControlWidth = true;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            return content;
        }

        private static TaskCard CreateTaskCardTemplate(Transform parent)
        {
            var cardRect = CreateRect("TaskCardTemplate", parent, new Vector2(0f, 0f), new Vector2(1f, 0f));
            cardRect.sizeDelta = new Vector2(0f, 210f);

            var image = cardRect.gameObject.AddComponent<Image>();
            image.color = new Color(0.10f, 0.16f, 0.25f, 0.92f);
            cardRect.gameObject.AddComponent<LayoutElement>().preferredHeight = 210f;

            var title = CreateLabel(cardRect, "task-id", 14, new Vector2(0.03f, 0.86f), new Vector2(0.97f, 0.98f), TextAnchor.MiddleLeft, FontStyle.Bold);
            var body = CreateLabel(cardRect, "-", 12, new Vector2(0.03f, 0.04f), new Vector2(0.97f, 0.85f), TextAnchor.UpperLeft);
            body.color = new Color(0.82f, 0.90f, 0.97f);

            var card = cardRect.gameObject.AddComponent<TaskCard>();
            card.Configure(title, body, image);
            card.gameObject.SetActive(false);
            return card;
        }

        private void AppendLog(string line)
        {
            if (logText == null)
            {
                return;
            }

            var now = DateTime.Now.ToString("HH:mm:ss");
            var current = string.IsNullOrWhiteSpace(logText.text) || logText.text == "-"
                ? ""
                : logText.text + "\n";
            var next = $"{current}[{now}] {line}";
            var lines = next.Split(new[] { '\n' }, StringSplitOptions.None);
            var keep = Mathf.Min(lines.Length, 18);
            logText.text = string.Join("\n", lines, lines.Length - keep, keep);
        }

        private void HandleProjectedCommand(string command)
        {
            if (projectedCommandTextRef != null)
            {
                projectedCommandTextRef.text = $"Projected Command: {command}";
            }
        }

        private void ClearChildren()
        {
            var toDelete = new List<GameObject>();
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                toDelete.Add(child.gameObject);
            }

            foreach (var go in toDelete)
            {
                Destroy(go);
            }
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var existing = go.GetComponent<T>();
            return existing != null ? existing : go.AddComponent<T>();
        }

        private static Transform CreateSpatialRoot(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }
            return go.transform;
        }

        private static string BuildDemoButtonText(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return "Demo";
            }
            return command.Length <= 16 ? command : command.Substring(0, 16) + "...";
        }

        private readonly struct NodeDef
        {
            public NodeDef(string appId, float degrees)
            {
                AppId = appId;
                Degrees = degrees;
            }

            public string AppId { get; }
            public float Degrees { get; }
        }
    }
}

