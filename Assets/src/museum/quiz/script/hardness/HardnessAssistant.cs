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
        private static readonly int DrawingPoint = Shader.PropertyToID("_DrawingPoint");
        private static readonly int DrawingColor = Shader.PropertyToID("_DrawingColor");
        private static readonly int TestPoint1 = Shader.PropertyToID("_TestPoint1");
        private static readonly int TestPoint2 = Shader.PropertyToID("_TestPoint2");
        private readonly int HardnessBorder = 5;
        private readonly Vector3 UndrawingPoint = new Vector3(-2f, -2f, 0);

        private Vector3 _previousPoint;
        
        public void Start()
        {
            /*Vector3 _TestPoint2 = new Vector3(0.6f, 0.2f, 0);
            Vector3 _TestPoint1 = new Vector3(0.5f, 0.5f, 0);
                float A = 1 / (_TestPoint2.x - _TestPoint1.x);
                float B = -1 / (_TestPoint2.y - _TestPoint1.y);
                float C = _TestPoint1.y / (_TestPoint2.y - _TestPoint1.y) - _TestPoint1.x / (_TestPoint2.x - _TestPoint1.x);
                float Z = C + A * 0.5f - B * 0.5f;
                float y = (-B * C + A * (Z - C)) / (A * A + B * B);
                float x = (A * y - Z + C) / B;
                Debug.Log(x + " " + y);*/
            heightMapUpdate.SetVector(ContactPosition, new Vector2(-2f, -2f));
            colorTextureMaterial.SetVector(DrawingPoint, new Vector2(-2f, -2f));
            hardnessHeightMap.Initialize();
            colorTexture.Initialize();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out QuizItem quizItem))
            {
                var contact = collision.GetContact(0);
                var ray = new Ray(contact.point - contact.normal * 0.5f * 0.1f, contact.normal);
                if (Physics.Raycast(ray, out var hit, 0.1f))
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
                var ray = new Ray(contact.point - contact.normal * 0.5f * 0.1f, contact.normal);
                if (Physics.Raycast(ray, out var hit, 0.1f))
                {
                    var hitCoordinate = hit.textureCoord;
                    if(quizItem.hardness < HardnessBorder)
                    {
                        colorTextureMaterial.SetVector(TestPoint1, _previousPoint);
                        colorTextureMaterial.SetVector(TestPoint2, hitCoordinate);
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