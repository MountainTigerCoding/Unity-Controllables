using UnityEngine;
using Runtime.Shared;
using Runtime.IO.Input;

namespace Runtime.Controllables.Characters
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Controllables/Characters/Character Controller Non Physics")]
    public class CharacterControllerNonPhysics : ControllableObject<CharacterInput>
    {
    #region Fields
        [Space(10f)]
        [SerializeField] private CharacterCamera _camera;
        [SerializeField] private Transform _orientation;
        [SerializeField] private float _mass = 20f;

        [Header("Movement")]
        [SerializeField] private float _walkSpeed;
        [SerializeField] private float _sprintSpeed = 1.5f;
        [SerializeField] private float _groundFriction = 1f;

        [Header("Jumping")]
        [SerializeField] private float _jumpHeight;
        [SerializeField] private float _jumpCooldownTime;
        [SerializeField, ReadOnlyInInspector] private bool _readyToJump = true;

        [Header("Ground Check")]
        [SerializeField, ReadOnlyInInspector] private bool _isGrounded = false;
        [SerializeField] private Transform _groundedCheckOrigin;
        [SerializeField] private LayerMask _whatIsGroundMask;

/*
        [Header("Optional")]
        [SerializeField] private CharacterInventory _inventory;
*/

        [Header("Stats")]
        [SerializeField, ReadOnlyInInspector] Vector3 _velocity;

        private CharacterController _characterController;
        private Vector3 _gravity;
    #endregion Fields

#if UNITY_EDITOR
        protected override void OnValidate ()
        {
            base.OnValidate();
            Init();
        }
#endif

        protected override void Start ()
        {
            base.Start();
            Init();
        }

        private void Init ()
        {
            if (_characterController == null) _characterController = transform.GetComponent<CharacterController>();
            _gravity = Physics.gravity;
        }

/*
        protected override void OnUpdateInControl()
        {
            _inventory?.OnUpdate();
        }
*/

    #region Movement
        protected override void OnFixedUpdateInControl ()
        {
            Movement();
            if (_isGrounded && _readyToJump) Jumping();

            if (_isGrounded && _velocity.y < 0f) {
                _velocity.y = -2f;
            }

            GroundChecks();
            TickVelocity(Time.fixedDeltaTime);

            _camera.Move(Input.Look);
        }

        private void Movement ()
        {
            Vector3 moveDirection = _orientation.right * Input.Move.x + _orientation.forward * Input.Move.y;

            float speed;
            if (Input.Sprint) speed = _sprintSpeed;
            else speed = _walkSpeed;

            AddForce(speed * moveDirection);
        }

        private void Jumping ()
        {
            if (Input.Jump) {
                AddForce( new Vector3(0f, Mathf.Sqrt(_jumpHeight * -2f * _gravity.y * _mass), 0f) );

                _readyToJump = false;
                _isGrounded = false;
                Invoke(nameof(ResetJump), _jumpCooldownTime);
            }
        }

        private void ResetJump ()
        {
            _readyToJump = true;
        }

        private void GroundChecks ()
        {
            _isGrounded = Physics.CheckSphere(_groundedCheckOrigin.position, 0.3f, _whatIsGroundMask);
        }
    #endregion Movement


    #region Force Management
        private void AddForce (Vector3 force)
        {
            _velocity += force;
        }

        private void TickVelocity (float timeStep)
        {
            // Gravity Movement
            _velocity.y += _gravity.y * _mass * timeStep;

            // Ground Movement
            float friction = Mathf.Clamp(_groundFriction * timeStep, 0, 0.99f);
            _velocity.x *= friction;
            _velocity.z *= friction;

            _characterController.Move(_velocity * timeStep);
        }
    #endregion Force Management
    }
} 