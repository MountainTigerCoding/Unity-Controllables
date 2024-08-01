using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Shared;
using Runtime.IO.Input;

namespace Runtime.Controllables
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Controllables/Controllables Manager")]
    public sealed class ControllablesManager : SingletonBehaviour<ControllablesManager>
    {
    #region Fields
        [SerializeField] private ControllableObject<InputCollection> _selected;
        [SerializeField, ReadOnlyInInspector] private List<ControllableObject<InputCollection>> _controllables;
    #endregion
        
    #region Properties
        public ControllableObject<InputCollection> Selected { get => _selected; }
    #endregion

#if UNITY_EDITOR
        private void OnValidate ()
        {
            if (!_controllables.Contains(_selected)) Register(_selected);
            Select(_selected);
        }

        [ContextMenu("Find In Children")]
        private void FindInChildren ()
        {
            _controllables.Clear();
            foreach (Transform child in Utils.GetAllChildren(transform))
            {
                if (child.TryGetComponent(out ControllableObject<InputCollection> controllable)) {
                    _controllables.Add(controllable);
                    continue;
                }
            }
        }
#endif

        protected override void Awake ()
        {
            _controllables ??= new();
            _controllables.Clear();
            if (_selected != null) Select(_selected);
        }

        public void Register <TInput>(ControllableObject<TInput> controllable) where TInput : InputCollection
        {
            if (controllable == null) return;
            ControllableObject<InputCollection> baseControllable = controllable as ControllableObject<InputCollection>;
            _controllables.Add(baseControllable);

            if (Application.isPlaying && _selected == null) {
                Select(baseControllable);
            }
        }

        public void Select (ControllableObject<InputCollection> controllable)
        {
            if (_selected == null) return;
            Select(_controllables.IndexOf(controllable));
        }

        public void Select (int index)
        {
            _selected =  _controllables[Math.Clamp(index, 0, _controllables.Count - 1)];

            // Deselect all controllers
            foreach (ControllableObject<InputCollection> controllable in _controllables)
            {
                if (controllable.IsControlling) controllable.EndControl();
            }

            _selected.BeginControl();
#if UNITY_EDITOR
            Debug.Log("Selected controllable '" + Selected.name + "'", Selected.gameObject);
#endif
        }
    }
}