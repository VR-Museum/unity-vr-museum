using src.museum.quiz.model.item;
using TMPro;
using UnityEngine;

namespace src.museum.quiz.script.hardness
{
    [RequireComponent( typeof( MeshCollider ) )]
    public class HardnessAssistant : MonoBehaviour
    {
        public CustomRenderTexture hardnessHeightMap;
        public CustomRenderTexture colorTexture;
        public Material heightMapUpdate;
        public Material colorTextureMaterial;
        [Range(0.001f, 20f)]
        public float hardnessMultiplier = 5;
        [SerializeField]
        public GameObject hardnessText;
        
        private GameObject _gameObject;

        private static readonly int ContactPosition = Shader.PropertyToID("_ContactPosition");
        private static readonly int SmoothMultiplier = Shader.PropertyToID("_smoothMultiplier");
        private static readonly int DrawingColor = Shader.PropertyToID("_DrawingColor");
        private static readonly int Point1 = Shader.PropertyToID("_Point1");
        private static readonly int Point2 = Shader.PropertyToID("_Point2");
        [SerializeField] private LayerMask PlaneLayer;
        private readonly int HardnessBorder = 5;
        private readonly Vector3 UndrawingPoint = new Vector3(-2f, -2f, 0);

        private Vector3 _previousPoint;
        
        public void Start()
        {
            heightMapUpdate.SetVector(ContactPosition, new Vector2(-2f, -2f));
            hardnessHeightMap.Initialize();
            colorTexture.Initialize();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out QuizItem quizItem))
            {
                var contact = collision.GetContact(0);
                var ray = new Ray(contact.point + transform.up, -transform.up);
                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, PlaneLayer))
                {
                    _previousPoint = hit.textureCoord;
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out QuizItem quizItem))
            {
                var contact = collision.GetContact(0);
                var ray = new Ray(contact.point + transform.up, -transform.up);
                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, PlaneLayer))
                {
                    var hitCoordinate = hit.textureCoord;
                    if(quizItem.hardness < HardnessBorder)
                    {
                        colorTextureMaterial.SetVector(Point1, _previousPoint);
                        colorTextureMaterial.SetVector(Point2, hitCoordinate);
                        colorTextureMaterial.SetVector(DrawingColor, quizItem.color);
                        colorTexture.Update();
                        _previousPoint = hitCoordinate;
                    }
                    else
                    {
                        heightMapUpdate.SetFloat(SmoothMultiplier, quizItem.hardness / hardnessMultiplier);
                        heightMapUpdate.SetVector(ContactPosition, hitCoordinate);
                    }
                }

                hardnessText.GetComponent<TextMeshPro>().text = "Твёрдость по шкале Мооса : " + quizItem.hardness;
            }
        }
    }
}