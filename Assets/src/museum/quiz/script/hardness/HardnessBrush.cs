using src.museum.quiz.model.item;
using UnityEngine;

namespace src.museum.quiz.script.hardness
{
    [RequireComponent( typeof( MeshCollider ) )]
    public class HardnessBrush : MonoBehaviour
    {
        public CustomRenderTexture hardnessHeightMap;
        public Material heightMapUpdate;

        private GameObject _gameObject;
        private Camera _mainCamera;

        private static readonly int ContactPosition = Shader.PropertyToID("_ContactPosition");
        private static readonly int SmoothMultiplier = Shader.PropertyToID("_smoothMultiplier");

        private GameObject _lastContactedMineral = null;
        
        public void Start()
        {
            heightMapUpdate.SetVector(ContactPosition, new Vector2(-2f, -2f));
            hardnessHeightMap.Initialize();
            _mainCamera = Camera.main;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out QuizItem quizItem))
            {
                var contact = collision.GetContact(0);
                var ray = new Ray(contact.point - contact.normal * 0.5f * 0.1f, contact.normal);
                if (Physics.Raycast(ray, out var hit, 0.1f))
                {
                    // if (_lastContactedMineral != null)
                    // {
                        // if (collision.gameObject != _lastContactedMineral)
                        // {
                            // heightMapUpdate.SetVector(ContactPosition, new Vector2(-2f, -2f));
                            // hardnessHeightMap.Initialize();
                            // _lastContactedMineral = collision.gameObject;
                        // }
                    // }
                    // else
                    // {
                        // _lastContactedMineral = collision.gameObject;
                    // }

                    var hitCoordinate = hit.textureCoord;
                    heightMapUpdate.SetFloat(SmoothMultiplier, quizItem.hardness / 5f);
                    heightMapUpdate.SetVector(ContactPosition, hitCoordinate);
                }
            }
        }

        // private void Update()
        // {
            // if (Input.GetMouseButton(0))
            // {
                // var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

                // if (Physics.Raycast(ray, out var hit))
                // {
                    // var hitCoordinate = hit.textureCoord;
                    // heightMapUpdate.SetVector(ContactPosition, hitCoordinate);
                // }
            // }
        // }
    }
}