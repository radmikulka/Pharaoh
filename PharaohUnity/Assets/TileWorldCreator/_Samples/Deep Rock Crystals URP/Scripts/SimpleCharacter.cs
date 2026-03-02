/*
 *	DATABRAIN
 *	(c) 2023 Giant Grey
 *	www.databrain.cc
 *	
 */
using UnityEngine;

namespace GiantGrey.TileWorldCreator.Samples
{

    public class SimpleCharacter : MonoBehaviour
    {
        private CharacterController controller;
        private Vector3 playerVelocity;
        private bool groundedPlayer;
        public float playerSpeed = 4.0f;
        public float jumpHeight = 1.0f;
        public float gravityValue = -9.81f;

        private bool isActive = true;

        private Vector3 movement;

#if ENABLE_INPUT_SYSTEM
        private DemoCharacterMovement inputActions;
#endif

#if ENABLE_INPUT_SYSTEM
        private void Awake()
        {
            inputActions = new DemoCharacterMovement();
            inputActions.CharacterMovement.Movement.performed += ctx =>
            {
                var v2 = ctx.ReadValue<Vector2>();
                movement = new Vector3(v2.x, 0,v2.y);
            };
        }
#endif


        private void Start()
        {
            controller = gameObject.GetComponent<CharacterController>();
        }

#if ENABLE_INPUT_SYSTEM
        private void OnEnable()
        {
            inputActions.Enable();
        }

        private void OnDisable()
        {
            inputActions.Disable();
        }
#endif

        void Update()
        {
            if (!isActive)
                return;

            groundedPlayer = controller.isGrounded;
            if (groundedPlayer && playerVelocity.y < 0)
            {
                playerVelocity.y = -0.1f;
            }

#if ENABLE_INPUT_SYSTEM
            controller.Move(movement * Time.deltaTime * playerSpeed);
#else
            movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            controller.Move(movement * Time.deltaTime * playerSpeed);
#endif

            if (movement != Vector3.zero)
            {
                gameObject.transform.forward = movement;
            }

            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }


        public void DeactivatePlayer()
        {
            isActive = false;
        }

        public void ActivatePlayer()
        {
            isActive = true;
        }
    }

}