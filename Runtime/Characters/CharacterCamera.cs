using UnityEngine;

namespace Runtime.Controllables.Characters
{
    public enum CharacterPerspective
    {
        FirstPerson,
        ThirdPerson,
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Controllables/Characters/Character Camera")]
    public sealed class CharacterCamera : MonoBehaviour
    {

    #region Fields
        [SerializeField] private bool _control = true;
        [SerializeField] private CharacterPerspective _perspective;
        [SerializeField] private Transform _orientation;

        [Space(10f)]
        [SerializeField] private Vector3 _targetAngles;
        public Vector3 Sensitivity;
        [SerializeField] private float _smoothRotation;
        [SerializeField] private float _pitchLimit = 80f;
        
    #endregion

    #region Properties
        public CharacterPerspective Perspective { get => _perspective; }
    #endregion

        private void OnValidate ()
        {
            UpdateAngles(true);
        }

        public void SetPerspective (CharacterPerspective perspective)
        {
            _perspective = perspective;
        }

        public void Move (Vector2 curserDelta)
        {
            if (!_control) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            curserDelta *= Sensitivity;
            _targetAngles += new Vector3(curserDelta.y, curserDelta.x, 0f) * Time.deltaTime;
            _targetAngles.x = Mathf.Clamp(_targetAngles.x, -_pitchLimit, _pitchLimit);

            UpdateAngles();
        }

        private void UpdateAngles (bool snap = false)
        {
            Quaternion _targetRotation = Quaternion.Euler(_targetAngles);
            Quaternion localRotation = transform.localRotation;

            if (snap) {
                localRotation = _targetRotation;
            } else {
                localRotation = Quaternion.Slerp(localRotation, _targetRotation, _smoothRotation * Time.deltaTime);
            }

            transform.localRotation = localRotation;
            _orientation.localRotation = Quaternion.Euler(0f, localRotation.eulerAngles.y, 0f);
        }
    }
}