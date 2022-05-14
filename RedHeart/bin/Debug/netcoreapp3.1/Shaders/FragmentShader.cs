namespace RedHeart.Shaders
{
    public static class FragmentShader
    {
        //1. Ambient - фоновое освещение

        //2. Diffuse - цвет под рассеянным освещением
        //dot - скалярное произведение - для единичных (нормализованных векторов
        //даёт косинус угла между ними;
        //угол между направлением света и нормалью фрагмента маленький -> косинус большой -> света много.

        //3. Specular - цвет блика на сердце

        //4. Attenuation - затухание света в зависимости от расстояния от источника до объекта

        //shininess - радиус блика (сила блеска)
        //spotlight - прожектор - точка, свет из которой
        //отображается только под определённым углом
        //cutOff - этот угол
        //outerCutOff позволяет сделать переход от света к темноте плавнее

        //замечание: *= для векторов (одного размера) работает покомпонентно

        public static readonly string text = @"
            #version 330
            
            struct HeartMaterial {
                vec3 ambient;
                vec3 diffuse;
                vec3 specular;
                float shininess;
            };

            struct Light {
                vec3 position;
                vec3 direction;
                float cutOff;
                float outerCutOff;
            
                vec3 ambient;
                vec3 diffuse;
                vec3 specular;
            
                float constant;
                float linear;
                float quadratic;
            };
            
            uniform Light light;
            uniform HeartMaterial material;
            uniform vec3 viewPos;
            
            in vec3 FragPos;
            in vec3 Normal;

            out vec4 FragColor;
            
            void main()
            {
                //TODO remove
                vec3 heartColor = vec3(1.0, 0.2, 0.4);

                /*Light calculations:*/

                //1. Ambient component
                vec3 ambient = light.ambient * material.ambient;

            
                //2. Diffuse component
                vec3 norm = normalize(Normal);
                //(Direction of the light - a difference vector between the light's position and the fragment's position)
                vec3 lightDir = normalize(light.position - FragPos);
                float diff = max(dot(norm, lightDir), 0.0);
                vec3 diffuse = light.diffuse * (diff * material.diffuse);
            
                //3. Specular component
                vec3 viewDir = normalize(viewPos - FragPos);
                vec3 reflectDir = reflect(-lightDir, norm);
                float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
                vec3 specular = light.specular * (spec * material.specular);
            
                //4. Attenuation
                float distance    = length(light.position - FragPos);
                float attenuation = 1.0 / (light.constant + light.linear * distance +
                                           light.quadratic * (distance * distance));
            
                //Spotlight intensity calculations
                //This is how we calculate the spotlight
                float theta     = dot(lightDir, normalize(-light.direction));
                float epsilon   = light.cutOff - light.outerCutOff;
                float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0); //The intensity, is the lights intensity on a given fragment,
                                                                                            //this is used to make the smooth border.    
                //Applying the spotlight intensity by multiplying our components by it.
                //Ambient is where the light dosen't hit, this means the spotlight shouldn't be applied
                ambient  *= attenuation;
                diffuse  *= attenuation * intensity;
                specular *= attenuation * intensity;
            
                //Returning our final color of this fragment
                FragColor = vec4((ambient + diffuse + specular), 1.0);
            }
        ";
    }
}