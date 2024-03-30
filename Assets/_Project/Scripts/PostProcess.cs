using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Coraline
{
    public class PostProcess : MonoBehaviour
    {
        private Material _material;
        [FormerlySerializedAs("_shader")] 
        [SerializeField] private Shader shader;

        public string[] excludedObjects = { "Player", "HiddenDoor" };

        private void Start()
        {
            _material = new Material(shader);
        }
        private void Update()
        {
            foreach (var obj in FindObjectsOfType<GameObject>())
            {
                if (Array.IndexOf(excludedObjects, obj.name) != -1) continue;
                var localRenderer = obj.GetComponent<Renderer>();
                if (!localRenderer) continue;
                var material = new Material(shader);
                localRenderer.material = material;
            }
        }
    }
}