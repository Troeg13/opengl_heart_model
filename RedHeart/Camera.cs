using OpenTK.Mathematics;
using System;

namespace Heart
{
    // ѕространстве камеры подразумевает координаты вершин, видимых с точки зрени€ камеры как начало сцены:
    // матрица вида преобразует все мировые координаты в координаты вида, которые относительны положени€ и направлени€ камеры.
    // „тобы определить камеру, нам нужно ее положение в мировом пространстве, направление, на которое она смотрит, вектор, указывающий вправо, и вектор, указывающий вверх от камеры.
    public class Camera
    {
        //¬екторы, задающие направлени€ осей координат дл€ камеры
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        //”гол поворота камеры вокруг оси X (в радианах)
        private float _pitch;
        //”гол поворота камеры вокруг оси Y (в радианах)
        //(без -2pi/2 камера при запуске была бы повЄрнута вправо на 90 градусов)
        private float _yaw = -MathHelper.PiOver2;

        //”гол обзора камеры (в радианах)
        private float _fov = MathHelper.PiOver2;

        //ѕозици€ (x, y, z) камеры в мировом пространстве
        public Vector3 position { get; set; }

        //—оотношение сторон окна, используемое дл€ расчЄта projection-матрицы
        public float aspect_ratio { private get; set; }

        public Camera(Vector3 _position, float _aspect_ratio)
        {
            this.position = _position;
            aspect_ratio = _aspect_ratio;
        }

        
        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;


        // ”добные свойства в градусах:

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                //¬ычисление угла наклона камеры в радианах
                //и его присваивание переменной _pitch
                //«начение ужимаетс€ в диапазон от -89 до 89 дл€ предотвращени€ странных багов.
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        //”гол обзора - (вертикальный) угол обзора камеры.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        //ѕолучение матрицы отображени€ с помощью функции LookAt
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(position, position + _front, _up);
        }

        //ѕолучение матрицы проекции
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, aspect_ratio, 0.01f, 100f);
        }

        //ќбновл€ет вектора направлени€ камеры
        private void UpdateVectors()
        {
            //¬ычислим вектор "вперЄд" дл€ камеры
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
            _front = Vector3.Normalize(_front);

            //“акже вычислим векторы "вправо" и "вверх" через векторное произведение
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}