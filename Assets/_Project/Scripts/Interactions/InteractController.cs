using System;
using KBCore.Refs;
using Unity.VisualScripting;
using UnityEngine;

namespace Coraline
{
    public class InteractController : MonoBehaviour
    {
        
        public enum InteractionType
        {
            Pickable,
            Moveable,
            None
        }
        
        [Header("Settings")]
        [SerializeField] internal InteractionType interactionType = InteractionType.None;
        
        [Header("References")]
        [SerializeField, Anywhere] private PlayerController playerController;


        private void OnEnable()
        {
            shouldMove = false;
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
        }
        
        private Vector3 targetPosition;
        private bool shouldMove;

        private const float PicDelta = 0.25f;
        private void OnMove()
        {
            targetPosition = transform.localPosition + new Vector3(0, 0, PicDelta);
            shouldMove = true;
        }
        
        internal void Interact()
        {
            switch (interactionType)
            {
                case InteractionType.Pickable:
                    OnPick();
                    break;
                case InteractionType.Moveable:
                    OnMove();
                    break;
                case InteractionType.None:
                    break;
                default:
                    return;
            }
        }
        
        private void Update()
        {
            if (!shouldMove) return;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime);

            if (Vector3.Distance(transform.localPosition, targetPosition) < 0.01f)
            {
                shouldMove = false;
            }
        }
        

    }
}