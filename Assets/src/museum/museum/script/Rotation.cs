using UnityEngine;

namespace src.museum.museum.script
{
    public class Rotation : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate(new Vector3(0, 5.625F, 0) * Time.deltaTime);
        }
    }
}
