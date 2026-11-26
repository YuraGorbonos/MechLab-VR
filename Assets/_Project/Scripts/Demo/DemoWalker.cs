using UnityEngine;

namespace SkillForge.Demo
{
    public class DemoWalker : MonoBehaviour
    {
        [SerializeField] private float _speed = 2.4f;
        [SerializeField] private float _lookSpeed = 2.2f;

        private float _yaw;
        private float _pitch;
        private CharacterController _body;
        private Transform _eyes;

        private void Awake()
        {
            _body = gameObject.AddComponent<CharacterController>();
            _body.height = 1.7f;
            _body.radius = 0.28f;
            _body.center = new Vector3(0f, 0.85f, 0f);

            var eyeObject = new GameObject("Eyes");
            eyeObject.transform.SetParent(transform, false);
            eyeObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var camera = eyeObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;
            camera.tag = "MainCamera";
            eyeObject.AddComponent<AudioListener>();
            _eyes = eyeObject.transform;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var locked = Cursor.lockState != CursorLockMode.Locked;
                Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !locked;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                _yaw += Input.GetAxis("Mouse X") * _lookSpeed;
                _pitch = Mathf.Clamp(_pitch - Input.GetAxis("Mouse Y") * _lookSpeed, -75f, 75f);
                transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
                _eyes.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }

            var move = transform.right * Input.GetAxis("Horizontal") + transform.forward * Input.GetAxis("Vertical");
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            var velocity = move * _speed;
            velocity.y = _body.isGrounded ? -1f : -6f;
            _body.Move(velocity * Time.deltaTime);
        }
    }
}
