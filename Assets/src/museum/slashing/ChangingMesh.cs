using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Компонент, позволяющий разрезать объекты, которым он принадлежит.
/// Для корректной работы объект должен иметь компоненты: MeshCollider, MeshRenderer, MeshFilter.
/// Желательно, чтобы у MeshRenderer было 2 материала: внешней и внутренней сторон.
/// Здесь используется алгоритм из презентации: https://www.gdcvault.com/play/1026882/How-to-Dissect-an-Exploding.
/// </summary>
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class ChangingMesh : MonoBehaviour
{
    /// <summary>
    /// Позиция для дочернего объекта, который подгружается до разрезания.
    /// </summary>
    [SerializeField] private Vector3 ChildPosition = new Vector3(4f, 0f, 4f);
    /// <summary>
    /// Cooldown после создания или разрезания объекта перед очредным разрезанием.
    /// </summary>
    [SerializeField] private int TimeForNoSlashing = 1;
    /// <summary>
    /// Число разрезов, которое можно совершить над объект. Не общий для всех объектов.
    /// </summary>
    [SerializeField] private int MaxSlicesCount = 2;
    /// <summary>
    /// Цвет вершин, относящихся к срезам.
    /// </summary>
    private readonly Color ColorForSection = Color.white;
    /// <summary>
    /// Цвет вершин, относящихся к внешней стороне.
    /// </summary>
    private readonly Color ColorForOuter = Color.black;
    /// <summary>
    /// Имя нового меша, которое получит дочерний объект.
    /// </summary>
    private readonly string ChildMeshName = "Child";
    /// <summary>
    /// Имя нового меша, которое получит основной объект.
    /// </summary>
    private readonly string BaseMeshName = "Base";
    /// <summary>
    /// Константа, показатель того, что не выбран индекс ведущей вершины при веерной триангуляции.
    /// </summary>
    private readonly int NoIndex = -1;
    /// <summary>
    /// Число задач, на которые распараллеливается процесс разрезания.
    /// </summary>
    private readonly int TasksCount = 4;
    /// <summary>
    /// Число индексов для треугольной грани.
    /// </summary>
    private const int TriangleIndecesCount = 3;
    /// <summary>
    /// Дочерний объект, который после разрезания будет второй половиной.
    /// </summary>
    private GameObject _childObject = null;
    /// <summary>
    /// Меш дочернего объекта. Нужно, чтобы при создании дочернего объекта меш заранее был подгружен.
    /// </summary>
    private Mesh _childMesh;
    /// <summary>
    /// Вершины объекта, относящиеся к мешу для отрисовки.
    /// </summary>
    private VerticesData _renderingVerticesData = new VerticesData();
    /// <summary>
    /// Вершины объекта, относящиеся к мешу для коллайдера.
    /// </summary>
    private VerticesData _colliderVerticesData = new VerticesData();
    /// <summary>
    /// Задача, которая совершает процесс разрезания объекта.
    /// </summary>
    private Task<(TaskData, TaskData)> _slashingTask;
    /// <summary>
    /// Индексы объекта, относящиеся к мешу для отрисовки.
    /// </summary>
    private List<int> _renderingTriangles;
    /// <summary>
    /// Индексы объекта, относящиеся к мешу для коллайдера.
    /// </summary>
    private List<int> _colliderTriangles;
    /// <summary>
    /// Пустой список.
    /// </summary>
    private List<int> _emptyList = new List<int>();
    /// <summary>
    /// Число совершенных разрезов до образования текущего объекта.
    /// </summary>
    private int _slicesCount;
    /// <summary>
    /// Флаг, позволяющий совершать разрезы после cooldown.
    /// </summary>
    private bool _canSlash = false;
    /// <summary>
    /// Флаг, информирующий о запустившемся процессе разрезания объекта.
    /// </summary>
    private bool _isSlashing = false;
    /// <summary>
    /// Флаг, иформирующий о том, что объект базовый, а значит следует инициализировать его данные.
    /// </summary>
    private bool _isBase = true;

    /// <summary>
    /// Инициализирует базовый объект.
    /// _slicesCount = 0, Мешам назначается 2 подмеша, запускается корутина,
    /// которая подготовит всё для начала разрезания.
    /// </summary>
    void Start()
    {
        if(_isBase)
        {
            _slicesCount = 0;
            _renderingTriangles = new List<int>();
            _colliderTriangles = new List<int>();
            Mesh mesh = GetComponent<MeshFilter>().mesh;
            mesh.subMeshCount = 2;
            mesh = GetComponent<MeshCollider>().sharedMesh;
            mesh.subMeshCount = 2;
            GetComponent<MeshCollider>().sharedMesh = mesh;

            StartCoroutine(PrepareForSlashing());
        }
    }

    /// <summary>
    /// На каждом фрэйме проверяется, запущен ли процесс разрезания и завершен ли он.
    /// Если да, то меши дочернего и основного объектов получают новую информацию о вершинах и индексах,
    /// дочерний объект становится активным.
    /// Если совершено максимальное количество разрезов, у объектов удаляется компонент ChangingMesh.
    /// </summary>
    void Update()
    {
        if(_isSlashing && _slashingTask.IsCompleted)
        {
            (TaskData renderingData, TaskData colliderData) = _slashingTask.Result;
            var childChangingMesh = _childObject.GetComponent<ChangingMesh>();
            SetDataToObject(_childObject, childChangingMesh._renderingVerticesData, renderingData.RightTriangles, renderingData.RightSection, false, ChildMeshName, this.transform);
            SetDataToObject(this.gameObject, _renderingVerticesData, renderingData.LeftTriangles, renderingData.LeftSection, false, BaseMeshName, this.transform);
            SetDataToObject(_childObject, childChangingMesh._colliderVerticesData, colliderData.RightTriangles, colliderData.RightSection, true, ChildMeshName, this.transform);
            SetDataToObject(this.gameObject, _colliderVerticesData, colliderData.LeftTriangles, colliderData.LeftSection, true, BaseMeshName, this.transform);
            _childObject.SetActive(true);
            _childObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            if(_slicesCount >= MaxSlicesCount)
            {
                Destroy(childChangingMesh);
                Destroy(this);
            }
            else
            {
                childChangingMesh._slicesCount = _slicesCount;
            }
            _isSlashing = false;
        }
    }

    /// <summary>
    /// Запускает процесс разрезания объекта, увеличивает число разрезаний.
    /// Предварительно инициализирует данные о вершинах и индексах.
    /// В качестве параметров принимает параметры плоскости в уравнении Ax+By+Cz+D=0.
    /// </summary>
    /// <param name="A">Параметр A в уравнении плоскости Ax+By+Cz+D=0.</param>
    /// <param name="B">Параметр B в уравнении плоскости Ax+By+Cz+D=0.</param>
    /// <param name="C">Параметр C в уравнении плоскости Ax+By+Cz+D=0.</param>
    /// <param name="D">Параметр D в уравнении плоскости Ax+By+Cz+D=0.</param>
    public void SliceByPlane(float A, float B, float C, float D)
    {
        if(!_canSlash)
        {
            return;
        }
        ++_slicesCount;
        _canSlash = false;
        var childChangingMesh = _childObject.GetComponent<ChangingMesh>();
        Mesh renderingMesh = GetComponent<MeshFilter>().mesh;
        Mesh colliderMesh = GetComponent<MeshCollider>().sharedMesh;
        InitializeData(renderingMesh, _renderingTriangles, _renderingVerticesData);
        InitializeData(colliderMesh, _colliderTriangles, _colliderVerticesData);
        childChangingMesh._renderingVerticesData = _renderingVerticesData.Copy();
        childChangingMesh._colliderVerticesData = _colliderVerticesData.Copy();
        _slashingTask = new Task<(TaskData, TaskData)>(() => 
        {
            var plane = new Plane { A=A, B=B, C=C, D=D };
            var time = System.DateTime.Now;
            var taskDataRendering = SliceByPlane(plane, _renderingTriangles, _renderingVerticesData, childChangingMesh._renderingVerticesData);
            var taskDataCollider = SliceByPlane(plane, _colliderTriangles, _colliderVerticesData, childChangingMesh._colliderVerticesData);
            return (taskDataRendering, taskDataCollider);
        });
        _slashingTask.Start();
        _isSlashing = true;
    }

    /// <summary>
    /// Запускает перебор вершин в нескольких задачах и собирает полученную информацию.
    /// Формирует грани для разрезов и расчитывает информацию их вершин.
    /// </summary>
    /// <param name="plane">Структура, хранящая параметры плоскости для разрезания.</param>
    /// <param name="triangles">Индексы меша, который разрезается.</param>
    /// <param name="verticesData">Информация о вершинах меша основного объекта.</param>
    /// <param name="childVerticesData">Информация о вершинах дочернего объекта.</param>
    /// <returns>Структура, содержащая индексы внешних и внетренних граней основного и дочернего объектов.</returns>
    private TaskData SliceByPlane(Plane plane, List<int> triangles, VerticesData verticesData, VerticesData childVerticesData)
    {
        var sectionIndeces = new Dictionary<int, int>();
        var leftTriangles = new List<int>();
        var rightTriangles = new List<int>();
        var leftSection = new List<int>();
        var rightSection = new List<int>();

        List<Task<TaskData>> tasks = new List<Task<TaskData>>();
        int indecesCountForOneTask = (triangles.Count / TasksCount) + 
        (TriangleIndecesCount - triangles.Count / TasksCount % TriangleIndecesCount);
        for(int t = 0; t < TasksCount; ++t)
        {
            int firstIndex = t * indecesCountForOneTask;
            int lastIndex = (t == TasksCount - 1) ? triangles.Count : indecesCountForOneTask + firstIndex;
            tasks.Add(GetTaskToEvaluateDataByPlane(firstIndex, lastIndex, triangles, verticesData, plane));
            tasks[tasks.Count - 1].Start();
        }
        Task.WaitAll(tasks.ToArray());

        int offset = verticesData.Count;
        JoinFormedData(verticesData, childVerticesData, sectionIndeces, tasks, leftTriangles, rightTriangles, leftSection, rightSection);
        var mainSectionIndex = GetMainSectionIndex(sectionIndeces);
        var maxMinCoordinates = FindMaxMinCoordinates(verticesData, offset);
        SetIndecesForSections(sectionIndeces, mainSectionIndex, leftSection, rightSection);

        SetVerticesDataForSection(sectionIndeces, childVerticesData, new Vector3(-plane.A, -plane.B, -plane.C), maxMinCoordinates);
        SetVerticesDataForSection(sectionIndeces, verticesData, new Vector3(plane.A, plane.B, plane.C), maxMinCoordinates);

        return new TaskData{ LeftTriangles=leftTriangles, RightTriangles=rightTriangles, LeftSection=leftSection, RightSection=rightSection };
    }

    /// <summary>
    /// Переносит вершины, чьи индексы не используются в образовании граней, в одну точку.
    /// Костыль. Желательно, чтобы эти вершины просто удалялись, а индексы сдвигались.
    /// </summary>
    /// <param name="mesh">Меш, чьи вершины переносятся.</param>
    /// <param name="triangles">Новые индексы.</param>
    private void FixUnusedVertices(Mesh mesh, List<int> triangles)
    {
        var vertices = new List<Vector3>();
        mesh.GetVertices(vertices);
        var mainSectionIndex = triangles[0];
        for(int i = 0; i < vertices.Count; ++i)
        {
            if(!triangles.Contains(i))
            {
                vertices[i] = vertices[mainSectionIndex];
            }
        }
        mesh.SetVertices(vertices);
    }

    /// <summary>
    /// Инициализирует данные о вершинах и индексах меша. Если у меша нет информации о цвете и UV-координатах вершин?
    /// то заносит значения по умолчанию и присваивает цвета самому мешу.
    /// </summary>
    /// <param name="mesh">Меш, из которого берутся данные.</param>
    /// <param name="triangles">Список, в котором будет расположены данные о индексах меша (первого и второго подмешей).</param>
    /// <param name="verticesData">Структура данных, в котором будет расположена информация о вершинах меша.</param>
    private void InitializeData(Mesh mesh, List<int> triangles, VerticesData verticesData)
    {
        mesh.GetVertices(verticesData.vertices);
        mesh.GetNormals(verticesData.normals);
        mesh.GetUVs(0, verticesData.uvs);
        mesh.GetTriangles(triangles, 0);
        var sectionTriangles = new List<int>();
        mesh.GetTriangles(sectionTriangles, 1);
        triangles.AddRange(sectionTriangles);
        mesh.GetColors(verticesData.colors);
        for(int i = 0; i < verticesData.Count; ++i)
        {
            if(verticesData.colors.Count == i)
            {
                verticesData.colors.Add(ColorForOuter);
            }
        }
        for(int i = 0; i < verticesData.Count; ++i)
        {
            if(verticesData.uvs.Count == i)
            {
                verticesData.uvs.Add(Vector2.zero);
            }
        }
        mesh.SetColors(verticesData.colors);
    }

    /// <summary>
    /// Заносит новую информацию о вершинах и индексах в меш.
    /// </summary>
    /// <param name="mesh">Меш, в которую нужно занести новую информацию.</param>
    /// <param name="verticesData">Новый набор вершин и данных о них.</param>
    /// <param name="triangles">Новый набор индексов для для первого подмеша.</param>
    /// <param name="sectionTriangles">Новый набор индексов для второго подмеша.</param>
    private void SetVerticesData(Mesh mesh, VerticesData verticesData, List<int> triangles, List<int> sectionTriangles)
    {
        mesh.subMeshCount = 2;
        mesh.SetTriangles(_emptyList, 0, true, 0);
        mesh.SetTriangles(_emptyList, 1, true, 0);
        mesh.SetVertices(verticesData.vertices);
        mesh.SetTriangles(triangles, 0, true, 0);
        mesh.SetTriangles(sectionTriangles, 1, true, 0);
        triangles.AddRange(sectionTriangles);
        mesh.SetNormals(verticesData.normals);
        mesh.SetColors(verticesData.colors);
        mesh.SetUVs(0, verticesData.uvs);
    }

    /// <summary>
    /// Создает задачу, которая перебирает индексы в определенном диапазоне и распределяет грани между
    /// основным и дочерним объектами. Если грань пересекается, преобразует ее в 3 новых и соответственно распределяет
    /// между объектами.
    /// </summary>
    /// <param name="beginningIndex">Номер индекса, с которого начинается перебор (включительно).</param>
    /// <param name="lastIndex">Номер индекса, на котором перебор оканчивается (без перебора самого индекса).</param>
    /// <param name="triangles">Список индексов, которые необходимо перебрать.</param>
    /// <param name="verticesData">Информация о вершинах меша до разрезания.</param>
    /// <param name="plane">Структура, хранящая параметры плоскости, по отношению к которой происходит перебор вершин.</param>
    /// <returns>Задача, результатом которой является структура с новой информацией об индексах и вершинах</returns>
    private Task<TaskData> GetTaskToEvaluateDataByPlane(int beginningIndex, int lastIndex, 
                                            List<int> triangles, VerticesData verticesData, Plane plane)
    {
        return new Task<TaskData>(() => {
            var taskData = new TaskData();
            var localLeftIndeces = new List<int>();
            var localRightIndeces = new List<int>();
            //Основной цикл
            for(int i = beginningIndex; i < lastIndex; i += TriangleIndecesCount)
            {
                var firstIndex = triangles[i];
                var secondIndex = triangles[i + 1];
                var thirdIndex = triangles[i + 2];
                var firstVertix = verticesData.vertices[firstIndex];
                var secondVertix = verticesData.vertices[secondIndex];
                var thirdVertix = verticesData.vertices[thirdIndex];
                if(VertixIsToLeftOfPlane(firstVertix, plane) &&
                    VertixIsToLeftOfPlane(secondVertix, plane) &&
                    VertixIsToLeftOfPlane(thirdVertix, plane))
                {
                    taskData.AddToLeftHalf(verticesData.colors[firstIndex].Equals(ColorForSection),
                        firstIndex, secondIndex, thirdIndex);
                }
                else if(IsToRightOfPlane(firstVertix, plane) &&
                    IsToRightOfPlane(secondVertix, plane) &&
                    IsToRightOfPlane(thirdVertix, plane))
                {
                    taskData.AddToRightHalf(verticesData.colors[firstIndex].Equals(ColorForSection),
                        firstIndex, secondIndex, thirdIndex);
                }
                else
                {
                    //Обработка разреза
                    SeparateVertices(localLeftIndeces, localRightIndeces, triangles, i, verticesData, plane);
                    var colorForNewVertices = verticesData.colors[localLeftIndeces[0]];
                    (Vector3, Vector3) newVertices = GetNewVertices(verticesData, localLeftIndeces, localRightIndeces, plane);
                    AddFormedTriangles(newVertices, localLeftIndeces, localRightIndeces,
                        verticesData, colorForNewVertices, taskData);
                    localLeftIndeces.Clear();
                    localRightIndeces.Clear();
                }
            }
            return taskData;
        });
    }

    /// <summary>
    /// Разносит вершины разрезаемой грани в зависимости от того, с какой стороны от разрезающей грани они находятся.
    /// При этом для двух вершин с одной стороны первая вершина при исходном обходе вершин не должна переходить
    /// во вторую (это необходимо для корректного образования новых граней далее).
    /// </summary>
    /// <param name="localLeftIndeces">Список, в которые попадут индексы вершин, находящиеся "слева" от плоскости. Должен быть пустым.</param>
    /// <param name="localRightIndeces">Список, в которые попадут индексы вершин, находящиеся "справа" от плоскости. Должен быть пустым.</param>
    /// <param name="triangles">Индексы меша.</param>
    /// <param name="firstIndexOfTriangle">Первый индекс в обходе вершин грани.</param>
    /// <param name="verticesData">Информация о вершинах меша до разрезания.</param>
    /// <param name="plane">Структура, хранящая параметры плоскости, по которой происходит разрез.</param>
    private void SeparateVertices(List<int> localLeftIndeces, List<int> localRightIndeces, List<int> triangles,
                                int firstIndexOfTriangle, VerticesData verticesData, Plane plane)
    {
        var indeces = new int[TriangleIndecesCount] {
            triangles[firstIndexOfTriangle],
            triangles[firstIndexOfTriangle + 1],
            triangles[firstIndexOfTriangle + 2]
        };
        var vertices = new Vector3[TriangleIndecesCount] {
            verticesData.vertices[indeces[0]],
            verticesData.vertices[indeces[1]],
            verticesData.vertices[indeces[2]]
        };
        var verticesAreToLeftOfPlane = new bool[TriangleIndecesCount] {
            VertixIsToLeftOfPlane(vertices[0], plane),
            VertixIsToLeftOfPlane(vertices[1], plane),
            VertixIsToLeftOfPlane(vertices[2], plane),
        };
        while(!((verticesAreToLeftOfPlane[0] ^ verticesAreToLeftOfPlane[1]) &&
            (verticesAreToLeftOfPlane[1] ^ verticesAreToLeftOfPlane[2])))
        {
            ShiftArray(indeces);
            ShiftArray(vertices);
            ShiftArray(verticesAreToLeftOfPlane);
        }
        for(int i = 0; i < TriangleIndecesCount; ++i)
        {
            if(verticesAreToLeftOfPlane[i])
            {
                localLeftIndeces.Add(indeces[i]);
            }
            else
            {
                localRightIndeces.Add(indeces[i]);
            }
        }
    }

    /// <summary>
    /// Расчитывает координаты новых вершин, образующиеся в результате разрезания грани по плоскости.
    /// Используется система уравнений из параметрического уравнения прямой между вершинами с двух сторон от плоскости
    /// и уравнение плоскости. Для двух вершин с одной стороны первая вершина при исходном обходе вершин не должна переходить
    /// во вторую (это необходимо для корректного образования новых граней далее).
    /// </summary>
    /// <param name="verticesData">Информация о всех вершинах меша.</param>
    /// <param name="localLeftIndeces">Индексы вершин грани, находящиеся "слева" от плоскости.</param>
    /// <param name="localRightIndeces">Индексы вершин грани, находящиеся "справа" от плоскости.</param>
    /// <param name="plane">Структура, хранящая параметры плоскости, по которой происходит разрез.</param>
    /// <returns>Кортеж из координат двух новых вершин.</returns>
    private (Vector3, Vector3) GetNewVertices(VerticesData verticesData, List<int> localLeftIndeces,
                                            List<int> localRightIndeces, Plane plane)
    {
        /*
            x = mt + x0
            y = nt + y0
            z = pt + z0

            t = -(Ax0 + By0 + Cz0 + D) / (Am + Bn + Cp)
        */
        (int firstIndex, int secondIndex, int aloneIndex) = (localLeftIndeces.Count > localRightIndeces.Count) ?
            (localLeftIndeces[0], localLeftIndeces[1], localRightIndeces[0]) :
            (localRightIndeces[0], localRightIndeces[1], localLeftIndeces[0]);
        var firstVertix = verticesData.vertices[firstIndex];
        var secondVertix = verticesData.vertices[secondIndex];
        var aloneVertix = verticesData.vertices[aloneIndex];

        var firstNewVertix = new Vector3();
        var secondNewVertix = new Vector3();
        float m = firstVertix.x - aloneVertix.x;
        float n = firstVertix.y - aloneVertix.y;
        float p = firstVertix.z - aloneVertix.z;
        float t = -(plane.A * firstVertix.x + plane.B * firstVertix.y + plane.C * firstVertix.z + plane.D) /
            (plane.A * m + plane.B * n + plane.C * p);
        firstNewVertix.x = m * t + firstVertix.x;
        firstNewVertix.y = n * t + firstVertix.y;
        firstNewVertix.z = p * t + firstVertix.z;

        m = secondVertix.x - aloneVertix.x;
        n = secondVertix.y - aloneVertix.y;
        p = secondVertix.z - aloneVertix.z;
        t = -(plane.A * secondVertix.x + plane.B * secondVertix.y + plane.C * secondVertix.z + plane.D) /
            (plane.A * m + plane.B * n + plane.C * p);
        secondNewVertix.x = m * t + secondVertix.x;
        secondNewVertix.y = n * t + secondVertix.y;
        secondNewVertix.z = p * t + secondVertix.z;
        return (firstNewVertix, secondNewVertix);
    }

    /// <summary>
    /// Определяет координаты текстур для новых вершин, образовавшихся в результате разрезнаия грани.
    /// Координаты получаются из отношений расстояний вершин друг к другу.
    /// </summary>
    /// <param name="twoIndeces">Индексы двух вершин грани, оказавшиеся с одной стороны от плоскости при разрезании.</param>
    /// <param name="aloneVertixIndex">Индекс вершины грани, оказавшаяся с другой стороны от плоскости при разрезании.</param>
    /// <param name="firstNewIndex">Индекс первой новой вершины в списке новых вершин.</param>
    /// <param name="secondNewIndex">Индекс второй новой вершины в списке новых вершин.</param>
    /// <param name="commonVerticesData">Информация о вершинах меша до разрезания.</param>
    /// <param name="newVerticesData">Информация о новых вершинах.</param>
    private void SetNewUVs(List<int> twoIndeces, int aloneVertixIndex,
                            int firstNewIndex, int secondNewIndex,
                            VerticesData commonVerticesData, VerticesData newVerticesData)
    {
        var firstIndex = twoIndeces[0];
        var secondIndex = twoIndeces[1];
        var firstUV = commonVerticesData.uvs[firstIndex];
        var secondUV = commonVerticesData.uvs[secondIndex];
        var aloneUV = commonVerticesData.uvs[aloneVertixIndex];
        var firstVertix = commonVerticesData.vertices[firstIndex];
        var secondVertix = commonVerticesData.vertices[secondIndex];
        var aloneVertix = commonVerticesData.vertices[aloneVertixIndex];
        var firstNewVertix = newVerticesData.vertices[firstNewIndex];
        var secondNewVertix = newVerticesData.vertices[secondNewIndex];

        var magnitude = (firstNewVertix - firstVertix).magnitude / (aloneVertix - firstVertix).magnitude;
        var offset = (aloneUV - firstUV) * magnitude;
        newVerticesData.uvs[firstNewIndex] = firstUV + offset;

        magnitude = (secondNewVertix - secondVertix).magnitude / (aloneVertix - secondVertix).magnitude;
        offset = (aloneUV - secondUV) * magnitude;
        newVerticesData.uvs[secondNewIndex] = secondUV + offset;
    }

    /// <summary>
    /// Создает новые вершины в результате разрезнаия грани. Добавляет новую информацию в списки индексов (двух подмешей).
    /// Также создает копию этих вершин для формаирования граней разреза.
    /// </summary>
    /// <param name="newVertices">Кортеж с координатами новых вершин (2 координаты).</param>
    /// <param name="localLeftIndeces">Индексы вершин грани, находящиеся "слева" от плоскости.</param>
    /// <param name="localRightIndeces">Индексы вершин грани, находящиеся "справа" от плоскости.</param>
    /// <param name="commonVerticesData">Информация о вершинах меша до разрезания.</param>
    /// <param name="colorForNewVertices">Цвет для новых вершин.</param>
    /// <param name="taskData">Структура с новой информацией о вершинах и индексах.</param>
    private void AddFormedTriangles((Vector3, Vector3) newVertices,
                                    List<int> localLeftIndeces, List<int> localRightIndeces,
                                    VerticesData commonVerticesData, Color colorForNewVertices,
                                    TaskData taskData)
    {
        var newVerticesData = taskData.NewVerticesData;
        var localTwoIndeces = (localLeftIndeces.Count > localRightIndeces.Count) ? localLeftIndeces : localRightIndeces;
        var localAloneIndex = (localLeftIndeces.Count < localRightIndeces.Count) ? localLeftIndeces[0] : localRightIndeces[0];
        var leftHalfHasMoreVertices = localLeftIndeces.Count > localRightIndeces.Count;
        List<int> listForNewTwoTriangles, listForNewOneTriangle, listToModifyListForTwoNewTriangles, listToModifyListForNewOneTriangles;
        SetCorrectLists(taskData, out listForNewTwoTriangles, out listForNewOneTriangle,
            out listToModifyListForTwoNewTriangles, out listToModifyListForNewOneTriangles,
            leftHalfHasMoreVertices, colorForNewVertices.Equals(ColorForSection));

        newVerticesData.Add(newVertices.Item1, commonVerticesData.normals[localAloneIndex],
            colorForNewVertices, Vector2.zero);
        newVerticesData.Add(newVertices.Item2, commonVerticesData.normals[localAloneIndex],
            colorForNewVertices, Vector2.zero);
        
        SetNewUVs(localTwoIndeces, localAloneIndex, newVerticesData.Count - 2, newVerticesData.Count - 1,
            commonVerticesData, newVerticesData);

        AddFromedTriangles(listForNewTwoTriangles, listForNewOneTriangle, listToModifyListForTwoNewTriangles, listToModifyListForNewOneTriangles,
            localTwoIndeces, localAloneIndex, newVerticesData.Count - 2, newVerticesData.Count - 1);

        newVerticesData.Add(newVertices.Item1, Vector3.zero, ColorForSection, Vector2.zero);
        newVerticesData.Add(newVertices.Item2, Vector3.zero, ColorForSection, Vector2.zero);
        
        taskData.AddSectionsIndeces(taskData.NewVerticesData.Count - 1, taskData.NewVerticesData.Count - 2, leftHalfHasMoreVertices);
    }

    /// <summary>
    /// Добавляет индексы новых граней в соответствующие списки индексов. Также добавляет индексы, которые в
    /// дальнейшем необходимо будет сдвинуть.
    /// </summary>
    /// <param name="listForNewTwoTriangles">Список индексов, в который добавляются индексы двух новых граней, которые находится по одну сторону от разрезающей плоскости.</param>
    /// <param name="listForNewOneTriangle">Список индексов, в который добавляются индексы одной новой грани, которая находится по другую сторону от разрезающей плоскости.</param>
    /// <param name="listToModifyListForTwoNewTriangles">Список индексов списка индексов с новыми двумя гранями, которые необходимо будет сдвинуть в дальнейшем.</param>
    /// <param name="listToModifyListForNewOneTriangles">Список индексов списка индексов с одной новой гранью, которые необходимо будет сдвинуть в дальнейшем.</param>
    /// <param name="localTwoIndeces">Индексы двух вершин разрезаемой грани, находящиеся по одну сторону от плоскости.</param>
    /// <param name="localAloneIndex">Индекс вершины грани, находящаяся по другую сторону от плоскости.</param>
    /// <param name="firstNewIndex">Индекс первой новой вершины.</param>
    /// <param name="secondNewIndex">Индекс второй новой вершины.</param>
    private void AddFromedTriangles(List<int> listForNewTwoTriangles, List<int> listForNewOneTriangle,
                                    List<int> listToModifyListForTwoNewTriangles, List<int> listToModifyListForNewOneTriangles,
                                    List<int> localTwoIndeces, int localAloneIndex,
                                    int firstNewIndex, int secondNewIndex)
    {
        listForNewTwoTriangles.Add(localTwoIndeces[0]);
        listForNewTwoTriangles.Add(firstNewIndex);
        listToModifyListForTwoNewTriangles.Add(listForNewTwoTriangles.Count - 1);
        listForNewTwoTriangles.Add(localTwoIndeces[1]);

        listForNewTwoTriangles.Add(localTwoIndeces[1]);
        listForNewTwoTriangles.Add(firstNewIndex);
        listToModifyListForTwoNewTriangles.Add(listForNewTwoTriangles.Count - 1);
        listForNewTwoTriangles.Add(secondNewIndex);
        listToModifyListForTwoNewTriangles.Add(listForNewTwoTriangles.Count - 1);

        listForNewOneTriangle.Add(firstNewIndex);
        listToModifyListForNewOneTriangles.Add(listForNewOneTriangle.Count - 1);
        listForNewOneTriangle.Add(localAloneIndex);
        listForNewOneTriangle.Add(secondNewIndex);
        listToModifyListForNewOneTriangles.Add(listForNewOneTriangle.Count - 1);
    }

    /// <summary>
    /// Собирает результат работы задач по перебору вершин в соответствующие списки и структуры.
    /// Также производит необходимый сдвиг индексов новых вершин.
    /// </summary>
    /// <param name="verticesData">Информация о вершинах основного объекта.</param>
    /// <param name="childVerticesData">Информация о вершинах дочернего объекта.</param>
    /// <param name="sectionIndeces">Пары новых вершин (харнящиеся как ключ-значение), которые будут участвовать в формировании граней разреза.</param>
    /// <param name="tasks">Список задач, переберавшие вершины в определённых диапазонах.</param>
    /// <param name="leftTriangles">Индексы для основного объекта (первый подмеш).</param>
    /// <param name="rightTriangles">Индексы для дочернего объекта(первый подмеш).</param>
    /// <param name="leftSection">Индексы для основного объекта (второй подмеш).</param>
    /// <param name="rightSection">Индексы для дочернего объекта (второй подмеш).</param>
    private void JoinFormedData(VerticesData verticesData, VerticesData childVerticesData,
                                Dictionary<int, int> sectionIndeces, List<Task<TaskData>> tasks,
                                List<int> leftTriangles, List<int> rightTriangles, List<int> leftSection, List<int> rightSection)
    {
        int offset = verticesData.Count;
        foreach(var task in tasks)
        {
            var taskData = task.Result;
            taskData.ModifyLists(offset);
            leftTriangles.AddRange(taskData.LeftTriangles);
            rightTriangles.AddRange(taskData.RightTriangles);
            leftSection.AddRange(taskData.LeftSection);
            rightSection.AddRange(taskData.RightSection);
            foreach(var indeces in taskData.SectionIndeces)
            {
                sectionIndeces.Add(indeces.Key + offset, indeces.Value + offset);
            }
            verticesData.Add(taskData.NewVerticesData);
            childVerticesData.Add(taskData.NewVerticesData);
            offset += taskData.NewVerticesData.Count;
        }
    }

    /// <summary>
    /// Берёт первый попавшийся индекс для веерной триангуляции. Реализацию стоит изменить.
    /// </summary>
    /// <param name="sectionIndeces">Индексы пар вершин (ключ-значение), смежные в многоугольнике, над которым производится триангуляция.</param>
    /// <returns>Индекс вершины для веерной триангуляции.</returns>
    private int GetMainSectionIndex(Dictionary<int, int> sectionIndeces)
    {
        var mainSectionIndex = NoIndex;
        foreach(var indeces in sectionIndeces)
        {
            if(mainSectionIndex == NoIndex)
            {
                mainSectionIndex = indeces.Key;
            }
        }
        return mainSectionIndex;
    }

    /// <summary>
    /// Определяет максимальные и минимальные xyz координаты вершин меша.
    /// </summary>
    /// <param name="verticesData">Информация о вершинах меша.</param>
    /// <param name="offset">Индекс, с которого производится перебор вершин.</param>
    /// <returns>Структура с максимальными и минимальными xyz координатами. Могут быть null.</returns>
    private MaxMinCoordinates FindMaxMinCoordinates(VerticesData verticesData, int offset)
    {
        float? nullableMaxX = null;
        float? nullableMaxY = null;
        float? nullableMaxZ = null;
        float? nullableMinX = null;
        float? nullableMinY = null;
        float? nullableMinZ = null;
        for(int i = offset; i < verticesData.Count; ++i)
        {
            var vertix = verticesData.vertices[i];
            nullableMaxX = Max(nullableMaxX, vertix.x);
            nullableMaxY = Max(nullableMaxY, vertix.y);
            nullableMaxZ = Max(nullableMaxZ, vertix.z);
            nullableMinX = Min(nullableMinX, vertix.x);
            nullableMinY = Min(nullableMinY, vertix.y);
            nullableMinZ = Min(nullableMinZ, vertix.z);
        }
        return new MaxMinCoordinates {
            MaxX = (float)nullableMaxX,
            MaxY = (float)nullableMaxY,
            MaxZ = (float)nullableMaxZ,
            MinX = (float)nullableMinX,
            MinY = (float)nullableMinY,
            MinZ = (float)nullableMinZ
        };
    }

    /// <summary>
    /// Производит веерную триангуляцию, путём добавления индексов вершин основному и дочернему объектам.
    /// </summary>
    /// <param name="sectionIndeces">Индексы пар вершин (ключ-значение), смежные в многоугольнике, над которым производится триангуляция.</param>
    /// <param name="mainSectionIndex">Индекс вершины, с которой происходит соедение остальных вершин.</param>
    /// <param name="leftSection">Индексы вершин граней разреза основного объекта.</param>
    /// <param name="rightSection">Индексы вершин граней разреза дочернего объекта.</param>
    private void SetIndecesForSections(Dictionary<int, int> sectionIndeces, int mainSectionIndex,
                                        List<int> leftSection, List<int> rightSection)
    {
        foreach(int vertixIndex in sectionIndeces.Keys)
        {
            if(mainSectionIndex == vertixIndex)
            {
                continue;
            }
            leftSection.Add(mainSectionIndex);
            leftSection.Add(vertixIndex);
            leftSection.Add(sectionIndeces[vertixIndex]);

            rightSection.Add(mainSectionIndex);
            rightSection.Add(sectionIndeces[vertixIndex]);
            rightSection.Add(vertixIndex);
        }
    }

    /// <summary>
    /// Устанавливает нормали и координаты текстур для вершин разреза.
    /// </summary>
    /// <param name="sectionIndeces">Индексы пар вершин (ключ-значение) граней разреза.</param>
    /// <param name="verticesData">Информация о вершинах меша.</param>
    /// <param name="commonNormal">Нормаль, которая будет присвоена вершинам граней разреза.</param>
    /// <param name="coordinates">Максимальные и минимальные xyz координаты вершин граней разреза.</param>
    private void SetVerticesDataForSection(Dictionary<int, int> sectionIndeces, VerticesData verticesData, Vector3 commonNormal,
                                            MaxMinCoordinates coordinates)
    {
        foreach(var indeces in sectionIndeces)
        {
            verticesData.normals[indeces.Key] = commonNormal;
            verticesData.normals[indeces.Value] = commonNormal;
            verticesData.uvs[indeces.Key] = GetUvsForVertix(verticesData.vertices[indeces.Key], coordinates);
            verticesData.uvs[indeces.Value] = GetUvsForVertix(verticesData.vertices[indeces.Value], coordinates);
        }
    }

    /// <summary>
    /// Заносит новую информацию о вершинах и индексах в меш объекта.
    /// </summary>
    /// <param name="obj">Объект, чей меш необходимо изменить.</param>
    /// <param name="verticesData">Новый набор вершин и данных о них.</param>
    /// <param name="triangles">Новый набор индексов для внешней стороны объекта (для первого подмеша).</param>
    /// <param name="section">Новый набор индексов для внутренней стороны объекта (для второго подмеша).</param>
    /// <param name="isCollider">Флаг, оспределяющий, какой меш измениется. True означает, что изменяется
    /// меш коллайдера. Иначе - меш для отрисовки.</param>
    /// <param name="MeshName">Новое имя для изменяющегося меша.</param>
    /// <param name="correctTransform">Позиция, в которой должен оказаться объект.</param>
    private void SetDataToObject(GameObject obj, VerticesData verticesData, List<int> triangles, List<int> section,
                                bool isCollider, string MeshName, Transform correctTransform)
    {
        Mesh mesh;
        var changingMesh = obj.GetComponent<ChangingMesh>();
        var objectTransform = obj.transform;
        if(isCollider)
        {
            mesh = new Mesh();
            mesh.name = MeshName;
            var collider = obj.GetComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            SetVerticesData(mesh, verticesData, triangles, section);
            FixUnusedVertices(mesh, triangles);
            collider.convex = true;
            changingMesh._colliderTriangles = triangles;
            objectTransform.position = correctTransform.position;
            objectTransform.rotation = correctTransform.rotation;
            if(_slicesCount < MaxSlicesCount)
            {
                StartCoroutine(changingMesh.PrepareForSlashing());
            }
        }
        else
        {
            mesh = obj.GetComponent<MeshFilter>().mesh;
            mesh.name = MeshName;
            SetVerticesData(mesh, verticesData, triangles, section);
            changingMesh._renderingTriangles = triangles;
        }
    }

    /// <summary>
    /// Присваивает ссылки на списки индексов из структуры с новой информацией для меша.
    /// </summary>
    /// <param name="taskData">Структура с новой информацией о вершинах и индексах.</param>
    /// <param name="listForNewTwoTriangles">Список, в который добавятся индексы двух новых граней в результате разрезания, которые находятся по одну сторону от разрезающей плоскости. Должен быть пустым.</param>
    /// <param name="listForNewOneTriangle">Список, в который добавятся индексы одной новой грани в результате разрезания, которая находится по другую сторону от разрезающей плоскости. Должен быть пустым.</param>
    /// <param name="listToModifyListForTwoNewTriangles">Список индексов списка индексов с новыми двумя гранями, которые необходимо будет сдвинуть в дальнейшем. Должен быть пустым.</param>
    /// <param name="listToModifyListForNewOneTriangles">Список индексов списка индексов с одной новой гранью, которые необходимо будет сдвинуть в дальнейшем. Должен быть пустым.</param>
    /// <param name="leftHalfHasMoreVertices">Флаг, говорящий о том, что "слева" от плоскости оказалось больше вершин, чем "справа".</param>
    /// <param name="isSection">Флаг, говорящий о том, что разрезаемая грань является гранью разреза, произошедшего ранее.</param>
    private void SetCorrectLists(TaskData taskData, out List<int> listForNewTwoTriangles, out List<int> listForNewOneTriangle,
                                out List<int> listToModifyListForTwoNewTriangles, out List<int> listToModifyListForNewOneTriangles,
                                bool leftHalfHasMoreVertices, bool isSection)
    {
        if(leftHalfHasMoreVertices)
        {
            if(isSection)
            {
                listForNewTwoTriangles = taskData.LeftSection;
                listForNewOneTriangle = taskData.RightSection;
                listToModifyListForTwoNewTriangles = taskData.LeftSectionToModify;
                listToModifyListForNewOneTriangles = taskData.RightSectionToModify;
            }
            else
            {
                listForNewTwoTriangles = taskData.LeftTriangles;
                listForNewOneTriangle = taskData.RightTriangles;
                listToModifyListForTwoNewTriangles = taskData.LeftTrianglesToModify;
                listToModifyListForNewOneTriangles = taskData.RightTrianglesToModify;
            }
        }
        else
        {
            if(isSection)
            {
                listForNewTwoTriangles = taskData.RightSection;
                listForNewOneTriangle = taskData.LeftSection;
                listToModifyListForTwoNewTriangles = taskData.RightSectionToModify;
                listToModifyListForNewOneTriangles = taskData.LeftSectionToModify;
            }
            else
            {
                listForNewTwoTriangles = taskData.RightTriangles;
                listForNewOneTriangle = taskData.LeftTriangles;
                listToModifyListForTwoNewTriangles = taskData.RightTrianglesToModify;
                listToModifyListForNewOneTriangles = taskData.LeftTrianglesToModify;
            }
        }
    }

    /// <summary>
    /// Проверяет, с какой стороны от плоскости находится вершина. Проверка происходит путем подстановки координат
    /// вершины в уравнение плоскости Ax+By+Cz+D и оценки знака результата.
    /// </summary>
    /// <param name="point">Координаты вершины</param>
    /// <param name="plane">Структура с параметрами плоскости.</param>
    /// <returns>True, если вершниа "слева" от плоскости, или лежит на ней. False - иначе.</returns>
    private bool VertixIsToLeftOfPlane(Vector3 point, Plane plane)
    {
        return plane.A * point.x + plane.B * point.y + plane.C * point.z + plane.D < 0 || IsOnPlane(point, plane);
    }

    /// <summary>
    /// Проверяет, с какой стороны от плоскости находится вершина. Проверка происходит путем подстановки координат
    /// вершины в уравнение плоскости Ax+By+Cz+D и оценки знака результата.
    /// </summary>
    /// <param name="point">Координаты вершины</param>
    /// <param name="plane">Структура с параметрами плоскости.</param>
    /// <returns>True, если вершниа "справа" от плоскости. False - иначе.</returns>
    private bool IsToRightOfPlane(Vector3 point, Plane plane)
    {
        return plane.A * point.x + plane.B * point.y + plane.C * point.z + plane.D > 0;
    }

    /// <summary>
    /// Проверяет, лежит ли вершина на плоскости. Проверка происходит путем подстановки координат
    /// вершины в уравнение плоскости Ax+By+Cz+D и оценки результата.
    /// </summary>
    /// <param name="point">Координаты вершины</param>
    /// <param name="plane">Структура с параметрами плоскости.</param>
    /// <returns>True, если вершниа принадлежит плоскости. False - иначе.</returns>
    private bool IsOnPlane(Vector3 point, Plane plane)
    {
        return plane.A * point.x + plane.B * point.y + plane.C * point.z + plane.D == 0;
    }

    /// <summary>
    /// Определяет минимальное значение из двух действительных чисел.
    /// </summary>
    /// <param name="min">Первое число. Может буть null.</param>
    /// <param name="newNumb">Второе число.</param>
    /// <returns>Минимальное значение из двух чисел. Если первое null, то вернёт второе.</returns>
    private float Min(float? min, float newNumb)
    {
        if(min == null)
        {
            return newNumb;
        }
        else
        {
            return (min <= newNumb) ? (float)min : newNumb;
        }
    }

    /// <summary>
    /// Определяет максимальное значение из двух действительных чисел.
    /// </summary>
    /// <param name="max">Первое число. Может буть null.</param>
    /// <param name="newNumb">Второе число.</param>
    /// <returns>Максимальное значение из двух чисел. Если первое null, то вернёт второе.</returns>
    private float Max(float? max, float newNumb)
    {
        if(max == null)
        {
            return newNumb;
        }
        else
        {
            return (max >= newNumb) ? (float)max : newNumb;
        }
    }

    /// <summary>
    /// Сдвигает 3 первых элемента массива по кольцу влево.
    /// </summary>
    /// <param name="array">Массив, чьи элементы сдвигаются. Должен иметь как минимум 3 элемента.</param>
    private void ShiftArray<T>(T[] array)
    {
        var tmp = array[0];
        array[0] = array[1];
        array[1] = array[2];
        array[2] = tmp;
    }

    /// <summary>
    /// Определяет координаты текстуры для вершины. Производится проекция на определенную плоскость, в зависимости
    /// от разностей максимальных и минимальных xyz координат. Если разность минимальная, то соответсвтующая координатная
    /// ось является нормалью к плоскости, на которую проецируются вершины.
    /// </summary>
    /// <param name="vertix">Координаты вершины</param>
    /// <param name="coordinates">Максимальные и минимальные xyz всех вершин.</param>
    /// <returns>Координаты текстуры для данной вершины.</returns>
    private Vector2 GetUvsForVertix(Vector3 vertix, MaxMinCoordinates coordinates)
    {
        float magnitudeX = coordinates.MaxX - coordinates.MinX;
        float magnitudeY = coordinates.MaxY - coordinates.MinY;
        float magnitudeZ = coordinates.MaxZ - coordinates.MinZ;
        float firstMagnitude, secondMagnitude, firstCoordinate, secondCoordinate, minMagnitude, minCoordinate;
        (firstMagnitude, firstCoordinate, minMagnitude, minCoordinate) = (magnitudeX > magnitudeY) ? 
            (magnitudeX, (vertix.x - coordinates.MinX) / magnitudeX, magnitudeY, (vertix.y - coordinates.MinY) / magnitudeY) :
            (magnitudeY, (vertix.y - coordinates.MinY) / magnitudeY, magnitudeX, (vertix.x - coordinates.MinX) / magnitudeX);
        (secondMagnitude, secondCoordinate) = (magnitudeZ > minMagnitude) ? 
            (magnitudeZ, (vertix.z - coordinates.MinZ) / magnitudeZ) : (minMagnitude, minCoordinate);
        return new Vector2(firstCoordinate, secondCoordinate);
    }

    /// <summary>
    /// Корутина, запускающаяся после cooldown. Создает дочерний объект и делает его неактивным.
    /// Также очищает данные о вершинах и индексах и поднимает флаг, разрешающий процесс разрезания.
    /// </summary>
    private IEnumerator PrepareForSlashing()
    {
        yield return new WaitForSeconds(TimeForNoSlashing);

        _childObject = Instantiate(this.gameObject);
        _childObject.SetActive(false);
        var childChangingMesh = _childObject.GetComponent<ChangingMesh>();
        childChangingMesh._isBase = false;
        _childObject.transform.position = ChildPosition;
        _childObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        _childMesh = _childObject.GetComponent<MeshFilter>().mesh;

        _renderingTriangles.Clear();
        _colliderTriangles.Clear();
        _renderingVerticesData.Clear();
        _colliderVerticesData.Clear();
        _canSlash = true;
    }
}
