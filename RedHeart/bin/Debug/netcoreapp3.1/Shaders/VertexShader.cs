namespace RedHeart.Shaders
{
    public static class VertexShader
    {
        public static readonly string text = @"
            #version 330

            //Input data of this shader:
            //Position and normal of the vertex
            //And some useful matrices

            layout (location = 0) in vec3 vPos;
            layout (location = 1) in vec3 vNormal;
            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            
            //Output data of this shader:
            //Fragment position and recalculated normal

            out vec3 FragPos;
            out vec3 Normal;
            
            void main()
            {
                vec4 worldCoordinates = vec4(vPos, 1.0) * model;

                //Setting up position of our vertex in a clip space
                gl_Position = worldCoordinates * view * projection;
                
                //And passing into fragment shader our this position in our world's coordinates
                FragPos = vec3(worldCoordinates);

                //Passing recalculated normals into fragment shader
                //(our model matrix includes rotation, and normals
                // in a world space become incorrect after that)
                Normal = vNormal * mat3(transpose(inverse(model)));
            }
        ";
    }
}