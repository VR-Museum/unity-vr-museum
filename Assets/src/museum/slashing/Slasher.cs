using UnityEngine;

/// <summary>
/// Объекта, разрезающий объекты с компонентом ChanginMesh. Для работы нужны компоненты Rigidbody и Collider.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Slasher : MonoBehaviour
{
    /// <summary>
    /// Вектор вдоль объекта, нужный для расчета нормали режущей плоскости.
    /// </summary>
    [SerializeField] private Vector3 MainAxis = Vector3.up;
    /// <summary>
    /// Скорость, которую должен иметь объект для разрезания. Стоит заменить на другой параметр.
    /// </summary>
    [SerializeField] private float minVelocityToSlash = 1;
    /// <summary>
    /// Компонент Rigidbody.
    /// </summary>
    private Rigidbody _rigidbody;
    
    /// <summary>
    /// Присваивает компонент Rigidbody при инициализации.
    /// </summary>
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// При соприкосновении с объектом, если у него компонент ChangingMesh, расчитывает параметры разрезающей плоскости
    /// через векторное произведение скорости самого объекта и MainAxis (параметр D получается из уравнения Ax+By+Cz+D=0)
    /// и вызывает разрезание.
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if(_rigidbody.velocity.magnitude < minVelocityToSlash)
        {
            return;
        }
        var changingMesh = collision.gameObject.GetComponent<ChangingMesh>();
        if(changingMesh != null)
        {
            var otherTransform = collision.transform;
            var point = otherTransform.InverseTransformPoint(collision.contacts[0].point);
            var velocityAxis = transform.InverseTransformDirection(_rigidbody.velocity);
            var normal = otherTransform.InverseTransformDirection(transform.TransformDirection(Vector3.Cross(MainAxis, velocityAxis))).normalized;
            float D = -(normal.x * point.x) - (normal.y * point.y) - (normal.z * point.z);
            changingMesh.SliceByPlane(normal.x, normal.y, normal.z, D);
        }
    }
}
