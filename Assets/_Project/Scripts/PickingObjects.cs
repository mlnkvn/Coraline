using UnityEngine;

namespace Coraline
{
    public class PickingObjects : MonoBehaviour
    {
        public GameObject myHands;
        private bool canPickUp;
        private GameObject objectToPickUp;

        private bool hasItem;

        private void Start()
        {
            canPickUp = false;
            hasItem = false;
        }

        
        private void Update()
        {
            if (canPickUp)
            {
                if (Input.GetKeyDown("e")) 
                {
                    objectToPickUp.GetComponent<Rigidbody>().isKinematic = true;
                    objectToPickUp.transform.position = myHands.transform.position;
                    objectToPickUp.transform.parent = myHands.transform;
                }
            }

            if (!Input.GetButtonDown("q") || !hasItem) return;
            objectToPickUp.GetComponent<Rigidbody>().isKinematic = false;

            objectToPickUp.transform.parent = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Magic Stone")) return;
            canPickUp = true;
            objectToPickUp = other.gameObject;
        }

        private void OnTriggerExit(Collider other)
        {
            canPickUp = false;

        }
    }
}