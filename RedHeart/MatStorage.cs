using OpenTK.Mathematics;

namespace Heart
{
    public struct Material
    {
        public readonly Vector3 ambient; // фоновое освещение
        public readonly Vector3 diffuse; // цвет под рассеянным освещением
        public readonly Vector3 specular; // цвет блика
        public readonly float shininess; // радиус (интенсивность) блика

        public Material(Vector3 _ambient, Vector3 _diffuse, Vector3 _specular, float _shininess) 
        {
            ambient = _ambient;
            diffuse = _diffuse;
            specular = _specular;
            shininess = _shininess;
        }

    }
}