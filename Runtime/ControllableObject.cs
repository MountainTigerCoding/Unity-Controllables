using UnityEngine;
using Runtime.Shared;
using Runtime.IO.Input;

namespace Runtime.Controllables
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Controllables/Controllable Object")]
    public class ControllableObject<TInput> : MonoBehaviour where TInput : InputCollection 
    {
    #region Fields
        [SerializeField, Tooltip("This is controlled by Controllables Manager")] private bool _IsControlling = false;

        private GlobalInput _globalInput;
        public TInput Input {private set; get; }
    #endregion

    #region Properties
        public bool IsControlling { get => _IsControlling; }
    #endregion

#if UNITY_EDITOR
        protected virtual void OnValidate () {}
#endif

        protected virtual void Start ()
        {
            ControllablesManager.Instance.Register(this);
        }

        public virtual void BeginControl ()
        {
            _IsControlling = true;
            _globalInput = GlobalInput.Instance;
            UnityAPICallDistributer.Instance.OnUpdate.Register(OnUpdate);
            UnityAPICallDistributer.Instance.OnFixedUpdate.Register(OnFixedUpdate);
        }

        public virtual void EndControl ()
        {
            _IsControlling = false;
            UnityAPICallDistributer.Instance.OnUpdate.Unregister(OnUpdate);
            UnityAPICallDistributer.Instance.OnFixedUpdate.Unregister(OnFixedUpdate);
        }

        public delegate IInputCollection ExternalInput();
        public virtual void BeginExternalControl (ExternalInput input)
        {

        }

        public virtual void EndExternalControl ()
        {
            
        }

        protected virtual void OnUpdateInControl () {}
        private void OnUpdate ()
        {
            if (_IsControlling) {
                Input = _globalInput.ReadDynamic<TInput>();
                OnUpdateInControl();
            }
        }

        protected virtual void OnFixedUpdateInControl () {}
        private void OnFixedUpdate ()
        {
            if (_IsControlling) OnFixedUpdateInControl();
        }
    }
}