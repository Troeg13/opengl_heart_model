using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using Heart.ImportModel;
using Heart.Shaders;

namespace Heart
{
    public class Window : GameWindow
    {
        private float[] vertices_normals;
        private int[] triangle;

        private int vertex_shader;
        private int fragment_shader;
        private int shader_program;
        private Dictionary<string, int> _uniformLocations;

        private int ver_array_obj;
        private int ver_buff_obj;
        private int triangle_buff_obj;

        //Состояние мыши
        private bool first_move = true;
        private Vector2 last_mouse_pos;

        //Состояние камеры
        private Camera camera;

        /* Переменные состояния: */

        //Для масштабирования сердца
        private float scale = 1.0f;
        private float scale_speed = 0.3f; // скорость биения сердца
        private bool is_upscaling = false;
        //для поворота
        private float rotation_degrees = 0.0f;

        //Отвязанность прожектора от камеры
        private bool _spotlightFixed = false;

        //Начальное положение камеры в мировом пространстве
        private Vector3 start_pos_camera = new Vector3(0.0f, 0.3f, 1.2f);
        //Текущее положение прожектора в мировом пространстве
        private Vector3 current_pos_spotlight = new Vector3(0.0f, 0.0f, 1.2f);
        //Направление прожектора в данный момент
        private Vector3 _currentSpotlightDir;
        // Создаем материал для сердца
        private Material red_mat = new Material(new Vector3(1.0f, 0.2f, 0.4f), new Vector3(1.0f, 0.2f, 0.4f), new Vector3(0.8f), 48.0f);


        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            //Импорт 3D-модели сердца из файла
            ImportModel.ImportModel r = new ImportModel.ImportModel();
            GL3DModel objRes = r.ReadObj("C:/Users/troeg/source/repos/CG/heart_opengl/Heart 3D Model/heart.obj");

            //Получение массива вершин/нормалей
            //и массива номеров вершин треугольников
            vertices_normals = objRes.GetVerticesWithNormals();
            triangle = objRes.triangles;

            //Сборка шейдерной программы (компиляция и линковка двух шейдеров)
            //shader_program = new ShaderProgram( VertexShader.text, FragmentShader.text);

            // Загрузка исходного кода и компиляция вершинного шейдера
            vertex_shader = GL.CreateShader(ShaderType.VertexShader); // создаём шейдерный объект, на выходе получаем индитификатор (число)
            GL.ShaderSource(vertex_shader, VertexShader.text); // подгрузили строку (исходный код) вершинного шейдера
            GL.CompileShader(vertex_shader); // скомпилировали шейдер

            // Загрузка исходного кода и компиляция фрагментного шейдера
            fragment_shader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment_shader, FragmentShader.text);
            GL.CompileShader(fragment_shader);
          
            // Создание шейдерной программы
            shader_program = GL.CreateProgram(); // создание объекта шейдерной программы
            GL.AttachShader(shader_program, vertex_shader); // записываем в программу вершинный шейдер
            GL.AttachShader(shader_program, fragment_shader); // записываем в программу фрагментный шейдер
            GL.LinkProgram(shader_program); // линкуем программу


            //Запись всех uniform-переменных из шейдеров в словарь
            //(словарь содержит имена переменных и хэндлы на них)
            GL.GetProgram(shader_program, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
            _uniformLocations = new Dictionary<string, int>();
            for (int i = 0; i < numberOfUniforms; i++)
            {
                string key = GL.GetActiveUniform(shader_program, i, out _, out _);
                int location = GL.GetUniformLocation(shader_program, key);
                _uniformLocations.Add(key, location);
            }

         
            //Создание объекта буфера вершин/нормалей, его привязка и заполнение
            ver_buff_obj = GL.GenBuffer(); // индификатор, по которому можно положить массив вершин/нормалей 
            GL.BindBuffer(BufferTarget.ArrayBuffer, ver_buff_obj); // делаем текущим
            GL.BufferData(BufferTarget.ArrayBuffer, vertices_normals.Length * sizeof(float), vertices_normals, BufferUsageHint.StaticDraw);

            //Указание OpenGL, где искать вершины в буфере вершин/нормалей
            var positionLocation = GL.GetAttribLocation(shader_program, "vPos"); // возвращает индитификатор по которому шейдер будет брать параметр с именем position

            //Создание Vertex Array Object (он необходим для того, чтоб сообщить программе, что делать с данными в  VBO) и его привязка
            ver_array_obj = GL.GenVertexArray();
            // Привяжим VAO и настроим атрибут позиции
            GL.BindVertexArray(ver_array_obj); // делаем текущим
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(positionLocation);
           
            //Указание OpenGL, где искать нормали в буфере вершин/нормалей
            var normalLocation = GL.GetAttribLocation(shader_program,"vNormal");
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(normalLocation);
            
            //Создание, привязка и заполнение объекта-буфера элементов для треугольников
            triangle_buff_obj = GL.GenBuffer(); // индификатор, по которому можно положить массив индексов треугольников
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, triangle_buff_obj);
            GL.BufferData(BufferTarget.ElementArrayBuffer, triangle.Length * sizeof(int), triangle, BufferUsageHint.StaticDraw);

            //Установка серого фона
            GL.ClearColor(0.51f, 0.51f, 0.51f, 0.0f);
            //Включение отрисовки только видимого...
            GL.Enable(EnableCap.CullFace);
            //и теста глубины во избежание наложений
            GL.Enable(EnableCap.DepthTest);

            //Установка стартового положения камеры
            camera = new Camera(start_pos_camera, Size.X / (float)Size.Y);

            //Захват курсора
            CursorGrabbed = true;
        }

        protected override void OnUnload()
        {
            //Отвязка всех ресурсов - установка в 0/null
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.UseProgram(0);
            //Очистка всех ресурсов
            GL.DeleteVertexArray(ver_array_obj);
            GL.DeleteBuffer(ver_buff_obj);
            GL.DeleteBuffer(triangle_buff_obj);
            GL.DetachShader(shader_program, vertex_shader);
            GL.DetachShader(shader_program, fragment_shader);
            GL.DeleteShader(fragment_shader);
            GL.DeleteShader(vertex_shader);

        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(shader_program);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(shader_program);
            GL.Uniform1(_uniformLocations[name], data); // устанавливает значение для постоянных переменных uniform в шейдере
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(shader_program);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(shader_program);
            GL.Uniform3(_uniformLocations[name], data);
        }

        
        protected override void OnRenderFrame(FrameEventArgs e) // отрисовка
        {
            base.OnRenderFrame(e);

            //Очистка буферов цвета и глубины
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Привязка буфера вершин/нормалей
            GL.BindVertexArray(ver_array_obj);
            //Указание использовать данную шейдерную программу/ делаем текущей
            GL.UseProgram(shader_program);
            //Привязка входных данных через uniform-переменные
            //матрица преобразования с учётом матрицы маштабирования и матрицы поворота
            // Matrix4.CreateScale(scale)  - Создает матрицу равномерного масштабирования, выполняющую равномерное масштабирование по каждой оси на величину scale
            // Matrix4.CreateRotationY(d) - Создает матрицу для вращения точек вокруг оси Y, d - угол в радианах, на который нужно вращаться вокруг оси Y
            Matrix4 model = Matrix4.CreateScale(scale) * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation_degrees));


            SetMatrix4("model", model);
            //(матрица переходв в пространство вида - "eye space")
            SetMatrix4("view", camera.GetViewMatrix());
            //(матрица проекции на систему координат от -1 до 1 по x и y)
            SetMatrix4("projection", camera.GetProjectionMatrix());
            //(позиция наблюдателя)
            SetVector3("viewPos", camera.position);

            //(параметры света)
            if (_spotlightFixed)
            {
                SetVector3("light.position", current_pos_spotlight);
                SetVector3("light.direction", _currentSpotlightDir);
            }
            else
            {
                SetVector3("light.position", camera.position);
                SetVector3("light.direction", camera.Front);
            }
            SetFloat("light.cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            SetFloat("light.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(32.5f)));
            SetVector3("light.ambient", new Vector3(0.2f));
            SetVector3("light.diffuse", new Vector3(0.7f));
            SetVector3("light.specular", new Vector3(1.0f));
            SetFloat("light.constant", 1.0f);
            SetFloat("light.linear", 0.09f);
            SetFloat("light.quadratic", 0.032f);

            //(свойства материала)
            //Цвет сам по себе
            SetVector3("material.ambient", red_mat.ambient);
            //Цвет под рассеянным освещением
            SetVector3("material.diffuse", red_mat.diffuse);
            //Цвет блика
            SetVector3("material.specular", red_mat.specular);
            //Сила блеска
            SetFloat("material.shininess", red_mat.shininess);

            GL.DrawElements(
                PrimitiveType.Triangles,
                triangle.Length,
                DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (!IsFocused)
                return;

            var input = KeyboardState;

            //Закрытие окна на Esc
            if (input.IsKeyDown(Keys.Escape))
                Close();

            //Обновление значений масштаба...
            if (scale <= 0.8f)
                is_upscaling = true;
            if (scale >= 0.999999f)
                is_upscaling = false;

            if (is_upscaling)
                scale += scale_speed * 0.5f * (float)e.Time;
            else
                scale -= scale_speed * 0.5f * (float)e.Time;

            //и поворота
            rotation_degrees += 30f * (float)e.Time;
            if (rotation_degrees >= 359.999) 
                rotation_degrees = 0.0f;

            //Обработка нажатий клавиш
            //(в том числе вычисление нового положения камеры перед следующим кадром)
            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.1f;

            float speedMultiplier = 1.0f;

            
            var mouse = MouseState;

            if (first_move)
            {
                last_mouse_pos = new Vector2(mouse.X, mouse.Y);
                first_move = false;
            }
            else
            {
                //Обновление камеры исходя из передвижений мыши
                var deltaX = mouse.X - last_mouse_pos.X;
                var deltaY = mouse.Y - last_mouse_pos.Y;
                last_mouse_pos = new Vector2(mouse.X, mouse.Y);

                camera.Yaw += deltaX * sensitivity;
                camera.Pitch -= deltaY * sensitivity;
            }
        }

        //Изменение угла обзора камеры по колесу мыши
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            camera.Fov -= 2 * e.OffsetY;
        }
    }
}

        
 