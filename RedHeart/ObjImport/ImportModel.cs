using System;
using System.Collections.Generic;
using System.IO;

namespace Heart.ImportModel
{
    //Вершина
    public struct Vertex
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Vertex(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

    }

    //Нормаль
    public struct Normal
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Normal(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

    }
    public class GL3DModel
    {
        //Вершины модели в виде списка
        public Vertex[] vertices { get; set; }

        //Нормали этих вершин (здесь - в том же порядке)
        public Normal[] normals { get; set; }

        //Треугольники, представленные списками вершин
        //в виде v11, v12, v13, - треугольник 1
        //       v21, v22, v23, - треугольник 2
        //       ...........
        //       vk1, vk2, vk3  - треугольник k (последний)
        public int[] triangles { get; set; }

        public GL3DModel(Vertex[] vertices, Normal[] normals, int[] triangles)
        {
            this.vertices = vertices;
            this.normals = normals;
            this.triangles = triangles;
        }

        //Получение всех вершин с их нормалями в формате:
        //x1, y1, z1, nx1, ny1, nz1,
        //x2, y2, z2, nx2, ny2, nz2,
        //...........
        //xm, ym, zm, nxn, nyn, nzn
        //Первые 4 числа - описание вершины, следующие 3 - нормали, и т. д.
        public float[] GetVerticesWithNormals()
        {
            float[] result = new float[vertices.Length * 6];
            for (int i = 0, j = 0; i < vertices.Length; i++, j += 6)
            {
                result[j] = vertices[i].x;
                result[j + 1] = vertices[i].y;
                result[j + 2] = vertices[i].z;

                result[j + 3] = normals[i].x;
                result[j + 4] = normals[i].y;
                result[j + 5] = normals[i].z;
            }
            return result;
        }
    }

    public class ImportModel
    {
        public GL3DModel ReadObj(string path)
        {
            List<Vertex> objVertices = new List<Vertex>();
            List<Normal> objNormals = new List<Normal>();

            string fileText = File.ReadAllText(path);
            string[] fileLines = fileText.Split('\n');

            //Проход по каждой строке .obj-файла
            int i = 0;
            for (; i < fileLines.Length; i++)
            {
                string line = fileLines[i];
                string[] lineParts = line.Split(' ');

                //Считывание нормали
                if (line.StartsWith("vn"))
                {
                    // float.Parse - преобразует строковое представление числа в эквивалентное ему число с плавающей запятой одиночной точности.
                    // Возвращает новую строку, в которой все вхождения заданного знака или String в текущей строке заменены другим заданным знаком Юникода или String.
                    // Заменяем в записи чисел "." на ",", чтоб ситать числож 
                    float x = float.Parse(lineParts[1].Replace('.', ','));
                    float y = float.Parse(lineParts[2].Replace('.', ','));
                    float z = float.Parse(lineParts[3].Replace('.', ','));
                    objNormals.Add(new Normal(x, y, z));
                }

                //Считывание вершины
                else if (line.StartsWith("v "))
                {
                    float x = float.Parse(lineParts[1].Replace('.', ','));
                    float y = float.Parse(lineParts[2].Replace('.', ','));
                    float z = float.Parse(lineParts[3].Replace('.', ','));
                    objVertices.Add(new Vertex(x, y, z));
                }
                //Начало треугольников в файле (конец перечисления вершин и нормалей)
                else if (line.StartsWith("f")) break;
            }

            //Массив нормалей к ним (уже отсортированный, каждой вершине нормаль)
            Normal[] normalsSorted = new Normal[objVertices.Count];

            //Список треугольников
            // В файле heart.obj треугольники записанны в виде четырёхугольников, который образует два треугольника
            List<int> objTriangles = new List<int>();

            //Считывание треугольников
            for (; i < fileLines.Length; i++)
            {
                // считываем из строки (четырёхугольники) вида f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3 v3/vt3/vn3 v4/vt4/vn4 
                // компонента vt отвечает индекс коордитаны текстуры вершины, она нам не нужна
                string line = fileLines[i];
                string[] lineParts = line.Split(' ');
                
                if (line.StartsWith("f"))
                {
                    // разделяем строки вида v1/vt1/vn1 на подстроки
                    // StringSplitOptions.None - использование параметра по умолчанию, т.е в подстроках пустые строки не опускать, пробелы не пропускать
                    string[] s1 = lineParts[1]
                        .Split('/', StringSplitOptions.None);
                    string[] s2 = lineParts[2]
                        .Split('/', StringSplitOptions.None);
                    string[] s3 = lineParts[3]
                        .Split('/', StringSplitOptions.None);
                    string[] s4 = lineParts[4]
                        .Split('/', StringSplitOptions.None);

                    //вычитаем 1, так как нумерация в файле obj, ведётся с 1, а не с 0
                    int v1 = int.Parse(s1[0]) - 1;
                    int vn1 = int.Parse(s1[2]) - 1;

                    int v2 = int.Parse(s2[0]) - 1;
                    int vn2 = int.Parse(s2[2]) - 1;

                    int v3 = int.Parse(s3[0]) - 1;
                    int vn3 = int.Parse(s3[2]) - 1;

                    int v4 = int.Parse(s4[0]) - 1;
                    int vn4 = int.Parse(s4[2]) - 1;

                    normalsSorted[v1] = objNormals[vn1];
                    normalsSorted[v2] = objNormals[vn2];
                    normalsSorted[v3] = objNormals[vn3];
                    normalsSorted[v4] = objNormals[vn4];

                    objTriangles.Add(v1);
                    objTriangles.Add(v2);
                    objTriangles.Add(v3);

                    objTriangles.Add(v1);
                    objTriangles.Add(v3);
                    objTriangles.Add(v4);
                }
            }
            //Операция ToArray создает массив типа T из входной последовательности типа T
            return new GL3DModel(
                objVertices.ToArray(), normalsSorted, objTriangles.ToArray());
        }
    }
}