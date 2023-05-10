using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Структура, хранящая информацию о вершинах, такую как координаты, цвета, нормали и координаты текстур.
/// </summary>
public class VerticesData
{
    /// <summary>
    /// Число вершин.
    /// </summary>
    public int Count { get => vertices.Count; }
    /// <summary>
    /// Координаты вершин.
    /// </summary>
    public List<Vector3> vertices { get; set; }
    /// <summary>
    /// Цвета вершин.
    /// </summary>
    public List<Color> colors { get; set; }
    /// <summary>
    /// Нормали вершин.
    /// </summary>
    public List<Vector3> normals { get; set; }
    /// <summary>
    /// Координаты текстуры вершин.
    /// </summary>
    public List<Vector2> uvs { get; set; }

    /// <summary>
    /// Создает пустую структуру.
    /// </summary>
    public VerticesData()
    {
        vertices = new List<Vector3>();
        colors = new List<Color>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();
    }

    /// <summary>
    /// Создает пустую структуру с заранее выделенной памятью.
    /// </summary>
    /// <param name="count">Число вершин, под которое необходимо выделить память.</param>
    public VerticesData(int count)
    {
        vertices = new List<Vector3>(count);
        colors = new List<Color>(count);
        normals = new List<Vector3>(count);
        uvs = new List<Vector2>(count);
    }

    /// <summary>
    /// Создает структуру с переданной информацией о вершинах.
    /// </summary>
    /// <param name="initVertices">Инициализирующие координаты вершин.</param>
    /// <param name="initColors">Инициализирующие цвета вершин.</param>
    /// <param name="initNormals">Инициализирующие нормали вершин.</param>
    /// <param name="initUvs">Инициализирующие координаты текстуры вершин.</param>
    public VerticesData(List<Vector3> initVertices, List<Color> initColors, List<Vector3> initNormals, List<Vector2> initUvs)
    {
        vertices = initVertices;
        colors = initColors;
        normals = initNormals;
        uvs = initUvs;
    }

    /// <summary>
    /// Добавляет информацию о вершинах к текущей.
    /// </summary>
    /// <param name="newVerticesData">Добавляемая информация о вершинах.</param>
    public void Add(VerticesData newVerticesData)
    {
        vertices.AddRange(newVerticesData.vertices);
        colors.AddRange(newVerticesData.colors);
        normals.AddRange(newVerticesData.normals);
        uvs.AddRange(newVerticesData.uvs);
    }

    /// <summary>
    /// Добавляет информацию об одной вершине к текущей.
    /// </summary>
    /// <param name="vertix">Координаты добавляемой вершины.</param>
    /// <param name="normal">Нормаль добавляемой вершин.</param>
    /// <param name="color">Цвет добавляемой вершины.</param>
    /// <param name="uv">Координаты текстуры добавляемой вершины.</param>
    public void Add(Vector3 vertix, Vector3 normal, Color color, Vector2 uv)
    {
        vertices.Add(vertix);
        normals.Add(normal);
        colors.Add(color);
        uvs.Add(uv);
    }

    /// <summary>
    /// Добавляет информацию об одной вершине из переданной структуры к текущей.
    /// </summary>
    /// <param name="verticesData">Информация о вершинах.</param>
    /// <param name="index">Индекс вершины, добавляемая из verticesData.</param>
    public void AddFrom(VerticesData verticesData, int index)
    {
        //проверка
        vertices.Add(verticesData.vertices[index]);
        colors.Add(verticesData.colors[index]);
        normals.Add(verticesData.normals[index]);
        uvs.Add(verticesData.uvs[index]);
    }

    /// <summary>
    /// Присваивает ссылки на информацию о вершинах из другой текстуры.
    /// </summary>
    /// <param name="verticesData">Информация о вершинах.</param>
    public void Set(VerticesData verticesData)
    {
        vertices = verticesData.vertices;
        colors = verticesData.colors;
        normals = verticesData.normals;
        uvs = verticesData.uvs;
    }

    /// <summary>
    /// Копирует информацию о нормалях из переданной структуры.
    /// </summary>
    /// <param name="anotherVerticesData">Информация о вершинах, из которой копируются нормали.</param>
    public void CopyNormals(VerticesData anotherVerticesData)
    {
        var newNormals = new Vector3[anotherVerticesData.Count];
        anotherVerticesData.normals.CopyTo(newNormals);
        normals = new List<Vector3>(newNormals);
    }

    /// <summary>
    /// Копирует текущую информацию о вершинах.
    /// </summary>
    /// <returns>Скопированная структура с текущей информацией.</returns>
    public VerticesData Copy()
    {
        var newVertices = new Vector3[Count];
        var newColors = new Color[Count];
        var newNormals = new Vector3[Count];
        var newUvs = new Vector2[Count];
        vertices.CopyTo(newVertices);
        colors.CopyTo(newColors);
        normals.CopyTo(newNormals);
        uvs.CopyTo(newUvs);
        return new VerticesData(new List<Vector3>(newVertices), new List<Color>(newColors), new List<Vector3>(newNormals), new List<Vector2>(newUvs));
    }

    /// <summary>
    /// Удаляет информацию о вершинах.
    /// </summary>
    public void Clear()
    {
        vertices.Clear();
        colors.Clear();
        normals.Clear();
        uvs.Clear();
    }

    /// <summary>
    /// Удаляет часть информации о вершинах
    /// </summary>
    /// <param name="firstIndex">Индекс, после которого удаляется информация о вершинах.</param>
    public void RemoveAfter(int firstIndex)
    {
        vertices.RemoveRange(firstIndex, vertices.Count - firstIndex);
        colors.RemoveRange(firstIndex, colors.Count - firstIndex);
        normals.RemoveRange(firstIndex, normals.Count - firstIndex);
        uvs.RemoveRange(firstIndex, uvs.Count - firstIndex);
    }

    /// <summary>
    /// Меняет местами отдельную информацию овершине с другой.
    /// </summary>
    /// <param name="i">Индекс первой вершины.</param>
    /// <param name="j">Индкс второй вершины.</param>
    public void Swap(int i, int j)
    {
        Swap(vertices, i, j);
        Swap(colors, i, j);
        Swap(normals, i, j);
        Swap(uvs, i, j);
    }

    /// <summary>
    /// Меняет местами элементы списка.
    /// </summary>
    /// <param name="list">Список, чьи жлементы меняются.</param>
    /// <param name="i">Индкс первого элемента.</param>
    /// <param name="j">Индкс второго элемента.</param>
    private void Swap<T>(List<T> list, int i, int j)
    {
        T tmp = list[i];
        list[i] = list[j];
        list[j] = tmp;
    }
}
