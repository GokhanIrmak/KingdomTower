using UnityEngine;

namespace KingdomTower
{
    /// <summary>
    /// Makes the object always face the camera (billboard effect).
    /// Used for HP text display above towers.
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return;

            // Face the camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                            mainCamera.transform.rotation * Vector3.up);
        }
    }
}
