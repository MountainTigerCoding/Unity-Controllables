using UnityEngine;
using Runtime.Shared;
using Runtime.IO.Input;

namespace Runtime.Controllables.Characters
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Controllables/Characters/Character Controller Rigibody")]
    public sealed class CharacterControllerRigibody : ControllableObject<CharacterInput>
    {
    #region Field
        [Space(10f)]
        [SerializeField] private CharacterCamera _camera;
        [SerializeField] private Transform _orientation;
        [SerializeField] private bool _ragdoll = false;
        
        [Header("Movement")]
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _moveSpeedMax;
        [SerializeField] private float _sprintMultiplier = 1.5f;
        private bool _readyToJump = true;

        [Header("Jumping")]
        [SerializeField] private float _jumpForce;
        [SerializeField] private float _jumpCooldownTime;

        [Header("Ground Check")]
        [SerializeField, ReadOnlyInInspector] private bool _isGrounded = false;
        [SerializeField] private Transform _groundedCheckOrigin;
        [SerializeField] private LayerMask _whatIsGroundMask;

        [Header("Drag")]
        [SerializeField] private float _airDrag = 1f;
        [SerializeField] private float _groundDrag = 5f;

        private Rigidbody _rigibody;
    #endregion

    #region Properties
        public bool IsGrounded { get => _isGrounded; }
    #endregion

#if UNITY_EDITOR
        protected override void OnValidate ()
        {
            base.OnValidate();
            if (Application.isPlaying) return;
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
            if (_rigibody == null) _rigibody = transform.GetComponent<Rigidbody>();
        }

    #region Movement
        protected override void OnFixedUpdateInControl ()
        {
            if (_ragdoll) {
                _rigibody.constraints = RigidbodyConstraints.FreezeRotation;

                GroundChecks();
                if (_isGrounded) Movement();             
                if (_isGrounded && _readyToJump) Jump();

                LimitGroundSpeed();
                _camera.Move(Input.Look);
            } else {
                _rigibody.constraints = RigidbodyConstraints.None;
                _camera.Move(Input.Look / 10);
            }
        }

        private void GroundChecks ()
        {
            _isGrounded = Physics.CheckSphere(_groundedCheckOrigin.position, 0.3f, _whatIsGroundMask);
            if (_isGrounded) _rigibody.drag = _groundDrag;
            else _rigibody.drag = _airDrag;
        }

        private void Movement ()
        {
            float multiplier;
            if (Input.Sprint) multiplier = _sprintMultiplier;
            else multiplier = 1f;

            Vector3 _moveDirection = _orientation.forward * Input.Move.y + _orientation.right * Input.Move.x;
            _rigibody.AddForce(_moveSpeed * multiplier * 10 * _moveDirection.normalized, ForceMode.Force);
        }

        private void LimitGroundSpeed ()
        {
            Vector3 groundVelocty = new(_rigibody.velocity.x, 0f, _rigibody.velocity.y);
            if (groundVelocty.magnitude > _moveSpeedMax) {
                Vector3 limitedVelocity = groundVelocty.normalized * _moveSpeedMax;
                _rigibody.velocity = new(limitedVelocity.x, _rigibody.velocity.y, limitedVelocity.z);
            }
        }

        private void Jump ()
        {
            if (Input.Jump) {
                _rigibody.AddForce(_orientation.up * _jumpForce, ForceMode.Impulse);

                _readyToJump = false;
                Invoke(nameof(ResetJump), _jumpCooldownTime);
            }
        }

        private void ResetJump ()
        {
            _readyToJump = true;
        }
    #endregion Movement
    }
}