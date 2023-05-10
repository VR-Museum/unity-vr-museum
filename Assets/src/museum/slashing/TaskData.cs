using System.Collections.Generic;

/// <summary>
/// Структура, необходимая для накопления результата перебора вершин и распределения их между основным и дочерним
/// объектами ChangingMesh при разрезании.
/// </summary>
public record TaskData
{
    /// <summary>
    /// Информация о новых добавленных вершинах.
    /// </summary>
    public VerticesData NewVerticesData { get; } = new VerticesData();
    /// <summary>
    /// Индексы пар новых вершин (ключ-значение), смежные в многоугольнике, над которым будет производится триангуляция.
    /// </summary>
    public Dictionary<int, int> SectionIndeces { get; } = new Dictionary<int, int>();
    /// <summary>
    /// Индексы вершин основного объекта (первый подмеш).
    /// </summary>
    public List<int> LeftTriangles { get; set; } = new List<int>();
    /// <summary>
    /// Индексы вершин дочернего объекта (второй подмеш).
    /// </summary>
    public List<int> RightTriangles { get; set; } = new List<int>();
    /// <summary>
    /// Индексы вершин основного объекта (второй подмеш).
    /// </summary>
    public List<int> LeftSection { get; set; } = new List<int>();
    /// <summary>
    /// Индексы вершин основного объекта (второй подмеш).
    /// </summary>
    public List<int> RightSection { get; set; } = new List<int>();
    /// <summary>
    /// Индексы индексов вершин основного объекта (первый подмеш), которые при сборе результатов перебора вершин
    /// необходимо сдвинуть.
    /// </summary>
    public List<int> LeftTrianglesToModify { get; } = new List<int>();
    /// <summary>
    /// Индексы индексов вершин дочернего объекта (первый подмеш), которые при сборе результатов перебора вершин
    /// необходимо сдвинуть.
    /// </summary>
    public List<int> RightTrianglesToModify { get; } = new List<int>();
    /// <summary>
    /// Индексы индексов вершин основного объекта (второй подмеш), которые при сборе результатов перебора вершин
    /// необходимо сдвинуть.
    /// </summary>
    public List<int> LeftSectionToModify { get; } = new List<int>();
    /// <summary>
    /// Индексы индексов вершин дочернего объекта (второй подмеш), которые при сборе результатов перебора вершин
    /// необходимо сдвинуть.
    /// </summary>
    public List<int> RightSectionToModify { get; } = new List<int>();

    /// <summary>
    /// Добавляет индексы вершин грани в соответствующий список основного объекта.
    /// </summary>
    /// <param name="isSection">Флаг, говорящий о принадлежности грани разрезу.</param>
    /// <param name="firstIndex">Первый индекс.</param>
    /// <param name="secondIndex">Второй индекс.</param>
    /// <param name="thirdIndex">Третий индекс.</param>
    public void AddToLeftHalf(bool isSection, int firstIndex, int secondIndex, int thirdIndex)
    {
        if(isSection)
        {
            LeftSection.Add(firstIndex);
            LeftSection.Add(secondIndex);
            LeftSection.Add(thirdIndex);
        }
        else
        {
            LeftTriangles.Add(firstIndex);
            LeftTriangles.Add(secondIndex);
            LeftTriangles.Add(thirdIndex);
        }
    }

    /// <summary>
    /// Добавляет индексы вершин грани в соответствующий список дочернего объекта.
    /// </summary>
    /// <param name="isSection">Флаг, говорящий о принадлежности грани разрезу.</param>
    /// <param name="firstIndex">Первый индекс.</param>
    /// <param name="secondIndex">Второй индекс.</param>
    /// <param name="thirdIndex">Третий индекс.</param>
    public void AddToRightHalf(bool isSection, int firstIndex, int secondIndex, int thirdIndex)
    {
        if(isSection)
        {
            RightSection.Add(firstIndex);
            RightSection.Add(secondIndex);
            RightSection.Add(thirdIndex);
        }
        else
        {
            RightTriangles.Add(firstIndex);
            RightTriangles.Add(secondIndex);
            RightTriangles.Add(thirdIndex);
        }
    }

    /// <summary>
    /// Добавляет пару индексов новых вершин в SectionIndeces.
    /// </summary>
    /// <param name="firstIndex">Первый индекс.</param>
    /// <param name="secondIndex">Второй индекс.</param>
    /// <param name="firstIndexIsMain">Флаг, говорящий о том, что ключом должен быть первый индекс (по умолачнию true).</param>
    public void AddSectionsIndeces(int firstIndex, int secondIndex, bool firstIndexIsMain = true)
    {
        if(firstIndexIsMain)
        {
            SectionIndeces.Add(firstIndex, secondIndex);
        }
        else
        {
            SectionIndeces.Add(secondIndex, firstIndex);
        }
    }

    /// <summary>
    /// Совершает сдвиг всех индексов, которые необходимо сдвинуть.
    /// </summary>
    /// <param name="offset">Какой необходимо совершить сдвиг.</param>
    public void ModifyLists(int offset)
    {
        ModifyList(offset, LeftTriangles, LeftTrianglesToModify);
        ModifyList(offset, RightTriangles, RightTrianglesToModify);
        ModifyList(offset, LeftSection, LeftSectionToModify);
        ModifyList(offset, RightSection, RightSectionToModify);
    }

    /// <summary>
    /// Совершает сдвиг индексов.
    /// </summary>
    /// <param name="offset">Какой необходимо совершить сдвиг.</param>
    /// <param name="list">Список индексов, к которым относятся сдвигаемые индексы.</param>
    /// <param name="listToModify">Список индексов сдвигаемых индексов.</param>
    private void ModifyList(int offset, List<int> list, List<int> listToModify)
    {
        foreach(int index in listToModify)
        {
            list[index] += offset;
        }
    }
}
