using System.Collections.Generic;
using UnityEngine;

namespace Jarvis3DCommandCenter
{
    [DisallowMultipleComponent]
    public sealed class TaskTimelineManager : MonoBehaviour
    {
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private TaskCard cardPrefab;

        private readonly Dictionary<string, TaskCard> cardsByTaskId = new Dictionary<string, TaskCard>();
        private readonly Dictionary<string, MockTaskState> tasksById = new Dictionary<string, MockTaskState>();

        public void Configure(RectTransform container, TaskCard prefab)
        {
            cardContainer = container;
            cardPrefab = prefab;
        }

        public void UpsertTask(MockTaskState state)
        {
            if (state == null || string.IsNullOrWhiteSpace(state.TaskId) || cardContainer == null || cardPrefab == null)
            {
                return;
            }

            tasksById[state.TaskId] = state;

            if (!cardsByTaskId.TryGetValue(state.TaskId, out var card) || card == null)
            {
                card = Instantiate(cardPrefab, cardContainer);
                card.gameObject.name = $"TaskCard_{state.TaskId}";
                card.gameObject.SetActive(true);
                cardsByTaskId[state.TaskId] = card;
                card.transform.SetAsFirstSibling();
            }

            card.Bind(state);
        }

        public MockTaskState GetTask(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                return null;
            }

            return tasksById.TryGetValue(taskId, out var state) ? state : null;
        }

        public MockTaskState GetLatestTask()
        {
            MockTaskState latest = null;
            foreach (var pair in tasksById)
            {
                if (latest == null)
                {
                    latest = pair.Value;
                    continue;
                }

                if (string.CompareOrdinal(pair.Value.StartedAt, latest.StartedAt) > 0)
                {
                    latest = pair.Value;
                }
            }

            return latest;
        }
    }
}

