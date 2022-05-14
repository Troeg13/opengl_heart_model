using OpenTK.Mathematics;
using System;

namespace Heart
{
    // ������������ ������ ������������� ���������� ������, ������� � ����� ������ ������ ��� ������ �����:
    // ������� ���� ����������� ��� ������� ���������� � ���������� ����, ������� ������������ ��������� � ����������� ������.
    // ����� ���������� ������, ��� ����� �� ��������� � ������� ������������, �����������, �� ������� ��� �������, ������, ����������� ������, � ������, ����������� ����� �� ������.
    public class Camera
    {
        //�������, �������� ����������� ���� ��������� ��� ������
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        //���� �������� ������ ������ ��� X (� ��������)
        private float _pitch;
        //���� �������� ������ ������ ��� Y (� ��������)
        //(��� -2pi/2 ������ ��� ������� ���� �� �������� ������ �� 90 ��������)
        private float _yaw = -MathHelper.PiOver2;

        //���� ������ ������ (� ��������)
        private float _fov = MathHelper.PiOver2;

        //������� (x, y, z) ������ � ������� ������������
        public Vector3 position { get; set; }

        //����������� ������ ����, ������������ ��� ������� projection-�������
        public float aspect_ratio { private get; set; }

        public Camera(Vector3 _position, float _aspect_ratio)
        {
            this.position = _position;
            aspect_ratio = _aspect_ratio;
        }

        
        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;


        // ������� �������� � ��������:

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                //���������� ���� ������� ������ � ��������
                //� ��� ������������ ���������� _pitch
                //�������� ��������� � �������� �� -89 �� 89 ��� �������������� �������� �����.
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

        //���� ������ - (������������) ���� ������ ������.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        //��������� ������� ����������� � ������� ������� LookAt
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(position, position + _front, _up);
        }

        //��������� ������� ��������
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, aspect_ratio, 0.01f, 100f);
        }

        //��������� ������� ����������� ������
        private void UpdateVectors()
        {
            //�������� ������ "�����" ��� ������
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
            _front = Vector3.Normalize(_front);

            //����� �������� ������� "������" � "�����" ����� ��������� ������������
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}