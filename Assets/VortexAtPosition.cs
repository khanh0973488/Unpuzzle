using UnityEngine;

namespace Eldvmo.Ripples
{
    /// <summary>
    /// Gán script này vào GameObject bất kỳ để tạo xoáy nước tại vị trí đó
    /// Xoáy sẽ luôn xuất hiện tại vị trí của GameObject này
    /// </summary>
    public class VortexAtPosition : MonoBehaviour
    {
        [Header("Water Plane Reference")]
        [SerializeField] private MeshRenderer ripplePlane;
        private Collider ripplePlaneCollider;
        private int waterLayerMask;

        [Header("Vortex Settings")]
        [SerializeField] private int vortexSlotIndex = 0; // Slot này sẽ dùng (0-4)
        [SerializeField] private bool activeVortex = true; // Bật/tắt xoáy
        [SerializeField] private float updateInterval = 0.1f; // Cập nhật vị trí mỗi X giây

        [Header("Offset Settings")]
        [SerializeField] private Vector3 vortexOffset = Vector3.zero; // Offset từ vị trí GameObject
        [SerializeField] private bool followYPosition = false; // Xoáy có theo trục Y không

        private Vector4[] vortexPoints = new Vector4[5];
        private float lastUpdateTime;
        private Vector2 currentUV;
        private bool isInitialized = false;

        void Start()
        {
            // Khởi tạo array
            for (int i = 0; i < 5; i++)
            {
                vortexPoints[i] = Vector4.zero;
            }

            if (ripplePlane != null)
            {
                ripplePlaneCollider = ripplePlane.GetComponent<Collider>();
                waterLayerMask = LayerMask.GetMask("Water");

                // Lấy array hiện tại từ shader nếu có
                if (ripplePlane.material.HasProperty("_VortexCentre"))
                {
                    // Nếu có vortex khác đang chạy, lấy dữ liệu của chúng
                    vortexPoints = ripplePlane.sharedMaterial.GetVectorArray("_VortexCentre");
                    if (vortexPoints == null || vortexPoints.Length != 5)
                    {
                        vortexPoints = new Vector4[5];
                    }
                }
            }
            else
            {
                Debug.LogError("Ripple Plane not assigned!");
                enabled = false;
                return;
            }

            lastUpdateTime = Time.time;

            // Khởi tạo xoáy ngay lập tức
            UpdateVortexPosition();
            isInitialized = true;
        }

        void Update()
        {
            if (!activeVortex || !isInitialized) return;

            // Cập nhật vị trí theo interval
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateVortexPosition();
                lastUpdateTime = Time.time;
            }
        }

        void OnEnable()
        {
            if (isInitialized)
            {
                // Kích hoạt lại xoáy khi enable
                UpdateVortexPosition();
            }
        }

        void OnDisable()
        {
            // Tắt xoáy khi disable
            if (isInitialized)
            {
                DeactivateVortex();
            }
        }

        /// <summary>
        /// Cập nhật vị trí xoáy dựa trên vị trí GameObject
        /// </summary>
        private void UpdateVortexPosition()
        {
            if (ripplePlaneCollider == null) return;

            // Tính vị trí thực tế với offset
            Vector3 targetPosition = transform.position + vortexOffset;

            // Raycast xuống mặt nước để lấy UV
            Vector3 rayOrigin = targetPosition + Vector3.up * 10f;
            Ray ray = new Ray(rayOrigin, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 20f, waterLayerMask))
            {
                currentUV = hit.textureCoord;

                // Cập nhật vortex point
                // z = Time.time để shader biết xoáy đang active
                vortexPoints[vortexSlotIndex] = new Vector4(currentUV.x, currentUV.y, Time.time, 0);

                // Gửi lên shader
                if (ripplePlane != null && ripplePlane.material != null)
                {
                    ripplePlane.material.SetVectorArray("_VortexCentre", vortexPoints);
                }
            }
            else
            {
                Debug.LogWarning($"Vortex {gameObject.name}: Cannot find water surface!");
            }
        }

        /// <summary>
        /// Tắt xoáy (set time = 0)
        /// </summary>
        private void DeactivateVortex()
        {
            vortexPoints[vortexSlotIndex] = Vector4.zero;

            if (ripplePlane != null && ripplePlane.material != null)
            {
                ripplePlane.material.SetVectorArray("_VortexCentre", vortexPoints);
            }
        }

        /// <summary>
        /// Bật/tắt xoáy từ code
        /// </summary>
        public void SetActive(bool active)
        {
            activeVortex = active;
            if (!active)
            {
                DeactivateVortex();
            }
            else
            {
                UpdateVortexPosition();
            }
        }

        /// <summary>
        /// Thay đổi slot index (nếu cần nhiều xoáy từ cùng 1 object)
        /// </summary>
        public void SetSlotIndex(int index)
        {
            if (index >= 0 && index < 5)
            {
                // Tắt slot cũ
                DeactivateVortex();

                // Chuyển sang slot mới
                vortexSlotIndex = index;

                // Bật slot mới
                if (activeVortex)
                {
                    UpdateVortexPosition();
                }
            }
        }

        // Vẽ gizmos để dễ debug
        void OnDrawGizmos()
        {
            if (!activeVortex) return;

            Vector3 vortexPos = transform.position + vortexOffset;

            // Vẽ vị trí xoáy
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(vortexPos, 0.5f);

            // Vẽ đường từ GameObject đến vị trí xoáy
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, vortexPos);

            // Vẽ ray xuống
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vortexPos + Vector3.up * 10f, Vector3.down * 20f);
        }

        void OnDrawGizmosSelected()
        {
            // Vẽ text hiển thị slot index
#if UNITY_EDITOR
            Vector3 textPos = transform.position + Vector3.up * 1.5f;
            UnityEditor.Handles.Label(textPos, $"Vortex Slot: {vortexSlotIndex}");
#endif
        }
    }
}