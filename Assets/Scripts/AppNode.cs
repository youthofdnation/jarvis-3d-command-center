using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jarvis3DCommandCenter
{
    [DisallowMultipleComponent]
    public sealed class AppNode : MonoBehaviour
    {
        [SerializeField] private string appId = "";
        [SerializeField] private string displayName = "";
        [SerializeField] private string description = "";
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private TextMesh labelText;
        [SerializeField] private Color baseColor = new Color(0.18f, 0.42f, 0.67f);
        [SerializeField] private Color highlightColor = new Color(0.33f, 0.82f, 1f);

        private bool highlighted;
        private float pulseTimer;

        public event Action<AppNode> Clicked;

        public string AppId => appId;
        public string DisplayName => displayName;
        public string Description => description;

        public void Configure(string id, string name, string desc, Color normal, Color highlightedTint, Renderer renderer, TextMesh label)
        {
            appId = id ?? "";
            displayName = string.IsNullOrWhiteSpace(name) ? appId : name;
            description = desc ?? "";
            baseColor = normal;
            highlightColor = highlightedTint;
            targetRenderer = renderer;
            labelText = label;

            if (labelText != null)
            {
                labelText.text = displayName;
            }

            ApplyVisual(immediate: true);
        }

        public void SetHighlighted(bool value)
        {
            highlighted = value;
            ApplyVisual(immediate: false);
        }

        private void Awake()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }

            if (labelText == null)
            {
                labelText = GetComponentInChildren<TextMesh>();
            }
        }

        private void Update()
        {
            if (!highlighted)
            {
                return;
            }

            pulseTimer += Time.deltaTime * 2.3f;
            var wobble = 1f + Mathf.Sin(pulseTimer) * 0.05f;
            transform.localScale = Vector3.one * wobble;
        }

        private void OnMouseDown()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Clicked?.Invoke(this);
        }

        private void ApplyVisual(bool immediate)
        {
            if (targetRenderer != null)
            {
                if (targetRenderer.material == null)
                {
                    targetRenderer.material = new Material(Shader.Find("Standard"));
                }

                var tint = highlighted ? highlightColor : baseColor;
                targetRenderer.material.color = tint;
                targetRenderer.material.EnableKeyword("_EMISSION");
                targetRenderer.material.SetColor("_EmissionColor", highlighted ? tint * 0.45f : Color.black);
            }

            if (!highlighted || immediate)
            {
                transform.localScale = Vector3.one;
            }

            if (labelText != null)
            {
                labelText.color = highlighted ? new Color(0.84f, 0.96f, 1f) : new Color(0.74f, 0.84f, 0.94f);
            }
        }
    }
}

