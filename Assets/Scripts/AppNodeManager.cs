using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jarvis3DCommandCenter
{
    [DisallowMultipleComponent]
    public sealed class AppNodeManager : MonoBehaviour
    {
        [SerializeField] private Transform coreTransform;
        [SerializeField] private Transform linksRoot;

        private readonly Dictionary<string, AppNode> nodesById = new Dictionary<string, AppNode>();
        private readonly List<LineRenderer> activeLinks = new List<LineRenderer>();
        private Material lineMaterial;
        private string selectedNodeId = "";

        public event Action<AppNode> NodeSelected;

        public void Configure(Transform core, Transform linksContainer)
        {
            coreTransform = core;
            linksRoot = linksContainer;
        }

        public void RegisterNode(AppNode node)
        {
            if (node == null || string.IsNullOrWhiteSpace(node.AppId))
            {
                return;
            }

            nodesById[node.AppId] = node;
            node.Clicked -= HandleNodeClicked;
            node.Clicked += HandleNodeClicked;
        }

        public void HighlightApps(IReadOnlyList<string> appIds)
        {
            var selected = new HashSet<string>((appIds ?? Array.Empty<string>()).Where(x => !string.IsNullOrWhiteSpace(x)));
            foreach (var pair in nodesById)
            {
                pair.Value.SetHighlighted(selected.Contains(pair.Key));
            }

            if (selected.Count > 0)
            {
                selectedNodeId = selected.First();
            }

            RebuildLinks(selected);
        }

        public void ClearHighlights()
        {
            foreach (var pair in nodesById)
            {
                pair.Value.SetHighlighted(false);
            }

            selectedNodeId = "";
            RebuildLinks(new HashSet<string>());
        }

        public AppNode GetNode(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return null;
            }

            return nodesById.TryGetValue(appId, out var node) ? node : null;
        }

        public string BuildNodeDetailText(string appId)
        {
            var node = GetNode(appId);
            if (node == null)
            {
                return "선택된 앱이 없습니다.";
            }

            var tasks = AppCatalog.GetMockTasks(node.AppId);
            return
                $"앱: {node.DisplayName}\n" +
                $"설명: {node.Description}\n\n" +
                "[Mock Task 목록]\n" +
                string.Join("\n", tasks.Select((x, idx) => $"{idx + 1}. {x}"));
        }

        public string BuildSelectedNodeDetailText()
        {
            if (string.IsNullOrWhiteSpace(selectedNodeId))
            {
                return "선택된 앱이 없습니다.";
            }

            return BuildNodeDetailText(selectedNodeId);
        }

        public void ForceSelect(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return;
            }

            if (!nodesById.TryGetValue(appId, out var node))
            {
                return;
            }

            selectedNodeId = appId;
            NodeSelected?.Invoke(node);
        }

        private void HandleNodeClicked(AppNode node)
        {
            if (node == null)
            {
                return;
            }

            selectedNodeId = node.AppId;
            NodeSelected?.Invoke(node);
        }

        private void RebuildLinks(HashSet<string> selected)
        {
            foreach (var lr in activeLinks)
            {
                if (lr != null)
                {
                    Destroy(lr.gameObject);
                }
            }
            activeLinks.Clear();

            if (coreTransform == null || linksRoot == null || selected == null || selected.Count == 0)
            {
                return;
            }

            if (lineMaterial == null)
            {
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            foreach (var appId in selected)
            {
                if (!nodesById.TryGetValue(appId, out var node) || node == null)
                {
                    continue;
                }

                var go = new GameObject($"Link_{appId}");
                go.transform.SetParent(linksRoot, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.material = lineMaterial;
                lr.positionCount = 2;
                lr.startWidth = 0.04f;
                lr.endWidth = 0.02f;
                lr.useWorldSpace = true;
                lr.numCapVertices = 6;
                lr.alignment = LineAlignment.View;
                lr.startColor = new Color(0.30f, 0.77f, 1f, 0.88f);
                lr.endColor = new Color(0.17f, 0.54f, 0.92f, 0.62f);

                var from = coreTransform.position;
                var to = node.transform.position;
                lr.SetPosition(0, from);
                lr.SetPosition(1, to);
                activeLinks.Add(lr);
            }
        }
    }
}

