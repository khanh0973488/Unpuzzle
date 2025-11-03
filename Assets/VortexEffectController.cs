using System.Collections;
using UnityEngine;

namespace Eldvmo.Ripples
{
    public class VortexEffectController : MonoBehaviour
    {
        [Header("Water Plane Reference")]
        [SerializeField] private MeshRenderer ripplePlane;
        private Collider ripplePlaneCollider;
        private int waterLayerMask;

        [Header("Vortex Settings")]
        [SerializeField] private int maxVortexCount = 5;
        [SerializeField] private KeyCode spawnVortexKey = KeyCode.V; // Phím để spawn xoáy
        [SerializeField] private bool spawnOnClick = true; // Spawn khi click chuột
        [SerializeField] private bool autoSpawn = false; // Tự động spawn
        [SerializeField] private float autoSpawnInterval = 3f; // Khoảng cách giữa các lần spawn tự động

        [Header("Vortex Spawn Area (Optional)")]
        [SerializeField] private Transform spawnAreaCenter; // Tâm vùng spawn
        [SerializeField] private float spawnRadius = 5f; // Bán kính vùng spawn
        [SerializeField] private bool useRandomSpawn = false; // Spawn random trong vùng

        private Vector4[] vortexPoints;
        private int vortexIndex = 0;
        private float lastAutoSpawnTime;

        void Start()
        {
            // Khởi tạo array
            vortexPoints = new Vector4[maxVortexCount];
            for (int i = 0; i < maxVortexCount; i++)
            {
                vortexPoints[i] = Vector4.zero;
            }

            if (ripplePlane != null)
            {
                ripplePlaneCollider = ripplePlane.GetComponent<Collider>();
                waterLayerMask = LayerMask.GetMask("Water");
            }
            else
            {
                Debug.LogError("Ripple Plane not assigned!");
            }

            lastAutoSpawnTime = Time.time;
        }

        void Update()
        {
            // Spawn bằng phím
            if (Input.GetKeyDown(spawnVortexKey))
            {
                SpawnVortexAtMousePosition();
            }

            // Spawn bằng click chuột
            if (spawnOnClick && Input.GetMouseButtonDown(0))
            {
                SpawnVortexAtMousePosition();
            }

            // Auto spawn
            if (autoSpawn && Time.time - lastAutoSpawnTime >= autoSpawnInterval)
            {
                if (useRandomSpawn)
                {
                    SpawnVortexAtRandomPosition();
                }
                else
                {
                    SpawnVortexAtMousePosition();
                }
                lastAutoSpawnTime = Time.time;
            }
        }

        /// <summary>
        /// Spawn xoáy tại vị trí chuột
        /// </summary>
        public void SpawnVortexAtMousePosition()
        {
            if (ripplePlaneCollider == null) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, waterLayerMask))
            {
                Vector2 uv = hit.textureCoord;
                AddVortex(uv);
            }
        }

        /// <summary>
        /// Spawn xoáy tại vị trí random trong vùng spawn
        /// </summary>
        public void SpawnVortexAtRandomPosition()
        {
            if (ripplePlaneCollider == null) return;

            Vector3 spawnCenter = spawnAreaCenter != null ? spawnAreaCenter.position : Vector3.zero;

            // Random position trong vùng tròn
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = spawnCenter + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Raycast xuống để tìm UV trên mặt nước
            Ray ray = new Ray(randomPos + Vector3.up * 10f, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 20f, waterLayerMask))
            {
                Vector2 uv = hit.textureCoord;
                AddVortex(uv);
            }
        }

        /// <summary>
        /// Spawn xoáy tại vị trí cụ thể (world position)
        /// </summary>
        public void SpawnVortexAtWorldPosition(Vector3 worldPosition)
        {
            if (ripplePlaneCollider == null) return;

            // Raycast xuống để tìm UV
            Ray ray = new Ray(worldPosition + Vector3.up * 5f, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f, waterLayerMask))
            {
                Vector2 uv = hit.textureCoord;
                AddVortex(uv);
            }
        }

        /// <summary>
        /// Spawn xoáy trực tiếp bằng UV coordinates
        /// </summary>
        public void SpawnVortexAtUV(Vector2 uv)
        {
            AddVortex(uv);
        }

        /// <summary>
        /// Thêm xoáy vào array và cập nhật shader
        /// </summary>
        private void AddVortex(Vector2 uv)
        {
            // Thêm vortex point mới
            vortexPoints[vortexIndex] = new Vector4(uv.x, uv.y, Time.time, 0);
            vortexIndex = (vortexIndex + 1) % vortexPoints.Length;

            // Cập nhật shader
            if (ripplePlane != null && ripplePlane.material != null)
            {
                ripplePlane.material.SetVectorArray("_VortexCentre", vortexPoints);
            }
        }

        /// <summary>
        /// Xóa tất cả các xoáy hiện tại
        /// </summary>
        public void ClearAllVortices()
        {
            for (int i = 0; i < maxVortexCount; i++)
            {
                vortexPoints[i] = Vector4.zero;
            }

            if (ripplePlane != null && ripplePlane.material != null)
            {
                ripplePlane.material.SetVectorArray("_VortexCentre", vortexPoints);
            }
        }

        /// <summary>
        /// Spawn nhiều xoáy cùng lúc theo pattern tròn
        /// </summary>
        public void SpawnVortexCirclePattern(Vector3 centerWorldPos, int count, float radius)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                SpawnVortexAtWorldPosition(centerWorldPos + offset);
            }
        }

        // Vẽ gizmos để hiển thị vùng spawn trong Scene view
        void OnDrawGizmosSelected()
        {
            if (useRandomSpawn && spawnAreaCenter != null)
            {
                Gizmos.color = Color.cyan;
                Vector3 center = spawnAreaCenter.position;

                // Vẽ vòng tròn
                int segments = 32;
                float angleStep = 360f / segments;
                Vector3 prevPoint = center + new Vector3(spawnRadius, 0, 0);

                for (int i = 1; i <= segments; i++)
                {
                    float angle = angleStep * i * Mathf.Deg2Rad;
                    Vector3 newPoint = center + new Vector3(
                        Mathf.Cos(angle) * spawnRadius,
                        0,
                        Mathf.Sin(angle) * spawnRadius
                    );
                    Gizmos.DrawLine(prevPoint, newPoint);
                    prevPoint = newPoint;
                }
            }
        }
    }
}