using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace Coraline
{
    public class TransformFollower : MonoBehaviour
    {
        [FormerlySerializedAs("References")] [SerializeField]
        private Transform playerTransform;

        [SerializeField] private Vector3 offsetPosition;
        [SerializeField] private bool debug;

        private void Update()
        {
            Refresh();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void Refresh()
        {
            if(!playerTransform)
            {
                Debug.LogWarning("Missing target ref !", this);

                return;
            }

            if (debug)
            {
                Debug.LogWarning("Debugging !", this);
            }
            // var offset = Vector3.Normalize(playerTransform.forward);
            // offset.Scale(offsetPosition);
            // transform.Rotate(playerTransform.forward, 0.0f, Space.World);
            // transform.position = playerTransform.position - offset;
        }
    }
}
