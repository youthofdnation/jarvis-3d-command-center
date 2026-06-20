using UnityEngine;

namespace Jarvis3DCommandCenter
{
    [DisallowMultipleComponent]
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 18f;
        [SerializeField] private float minDistance = 10f;
        [SerializeField] private float maxDistance = 30f;
        [SerializeField] private float yaw = 25f;
        [SerializeField] private float pitch = 24f;
        [SerializeField] private float orbitSpeed = 120f;
        [SerializeField] private float zoomSpeed = 8f;
        [SerializeField] private float panSpeed = 0.03f;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * orbitSpeed * Time.deltaTime;
                pitch -= Input.GetAxis("Mouse Y") * orbitSpeed * 0.6f * Time.deltaTime;
                pitch = Mathf.Clamp(pitch, 8f, 72f);
            }

            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                distance -= scroll * zoomSpeed;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            if (Input.GetMouseButton(2))
            {
                var pan = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"), 0f) * panSpeed;
                target.position += transform.TransformDirection(pan);
            }

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var offset = rotation * new Vector3(0f, 0f, -distance);
            transform.position = target.position + offset;
            transform.rotation = rotation;
        }

        public void SetTarget(Transform value)
        {
            target = value;
        }
    }
}

