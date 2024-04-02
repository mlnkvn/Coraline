using System;
using System.Linq;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;

namespace Coraline
{
    public class InteractController : MonoBehaviour
    {
        private bool interacted;
        public enum InteractionType
        {
            Pickable,
            Moveable,
            Openable,
            None
        }
        
        [Header("Settings")]
        [SerializeField] internal InteractionType interactionType = InteractionType.None;
        
        [Header("References")]
        [SerializeField, Anywhere] private PlayerController playerController;


        private void OnEnable()
        {
            shouldMove = false;
            shouldRotate = false;
            interacted = false;
        }

        private void OnPick()
        {
            if (playerController.hasItemLeft && playerController.hasItemRight) return;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<BoxCollider>().enabled = false;
            Transform hand;
            if (playerController.hasItemRight)
            {
                hand = playerController.myLeftHand.transform;
                playerController.hasItemLeft = true;
            }
            else
            {
                hand = playerController.myRightHand.transform;
                playerController.hasItemRight = true;
            }
            transform.position = hand.position;
            transform.rotation = hand.rotation;
            transform.parent = hand;
            interacted = true;
        }
        
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private bool shouldMove;
        private bool shouldRotate;

        private const float PicDelta = 0.3f;
        private void OnMove()
        {
            targetPosition = transform.localPosition + new Vector3(0, 0, PicDelta);
            shouldMove = true;
        }

        private void OnOpen()
        {
            if (interacted || !playerController.HoldsObject("Needed Key"))
            {
                return;
            }
            
            if (!Physics.Raycast(playerController.transform.position,
                    transform.position - playerController.transform.position, out var hit)) return;
            // check if hit.collider.gameObject is in a gameObject hierarchy
            var seeDoor = gameObject.GetComponentsInChildren<Transform>().Any(obj => hit.collider.gameObject == obj.gameObject);
            if (!seeDoor) return;
            shouldRotate = true;
            targetRotation = transform.localRotation * Quaternion.Euler(0, 105, 0);
        }
        
        internal void Interact()
        {
            if (interacted) return;
            switch (interactionType)
            {
                case InteractionType.Pickable:
                    OnPick();
                    break;
                case InteractionType.Moveable:
                    OnMove();
                    break;
                case InteractionType.Openable:
                    OnOpen();
                    break;
                case InteractionType.None:
                    break;
                default:
                    return;
            }
        }
        
        private void Update()
        {
            if (shouldMove)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime);

                if (!(Vector3.Distance(transform.localPosition, targetPosition) < 0.01f)) return;
                shouldMove = false;
                interacted = true;
            } else if (shouldRotate)
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime);
                if (!(Quaternion.Angle(transform.localRotation, targetRotation) < 0.01f)) return;
                shouldRotate = false;
                interacted = true;
            }
        }
        

    }
}