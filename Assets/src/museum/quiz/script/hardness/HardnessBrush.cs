using src.museum.quiz.model.item;
using UnityEngine;

namespace src.museum.quiz.script.hardness
{
    [RequireComponent( typeof( MeshCollider ) )]
    public class HardnessBrush : MonoBehaviour
    {
        public CustomRenderTexture hardnessHeightMap;
        public Material heightMapUpdate;
        [Range(0.001f, 20f)]
        public float hardnessMultiplier = 5;
        
        private GameObject _gameObject;

        private static readonly int ContactPosition = Shader.PropertyToID("_ContactPosition");
        private static readonly int SmoothMultiplier = Shader.PropertyToID("_smoothMultiplier");
        
        public void Start()
        {
            heightMapUpdate.SetVector(ContactPosition, new Vector2(-2f, -2f));
            hardnessHeightMap.Initialize();
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out QuizItem quizItem))
            {
                var contact = collision.GetContact(0);
                var ray = new Ray(contact.point - contact.normal * 0.5f * 0.1f, contact.normal);
                if (Physics.Raycast(ray, out var hit, 0.1f))
                {
                    var hitCoordinate = hit.textureCoord;
                    heightMapUpdate.SetFloat(SmoothMultiplier, quizItem.hardness / hardnessMultiplier);
                    heightMapUpdate.SetVector(ContactPosition, hitCoordinate);
                }
            }
        }
    }
}