#version 460 core 

// The fragment shader only requires one output variable and that is a vector of size 4
out vec4 FragColor; 

in vec2 uvCoord;
in float viewDist; 
uniform sampler2D texture0;
uniform sampler2D texture1;

uniform vec3 fogColor;
uniform float fogStart;
uniform float fogEnd;

void main()
{
    vec4 color = mix(texture(texture1, uvCoord), texture(texture0, uvCoord), 0.8);

    float fogFactor = clamp((fogEnd - viewDist) / (fogEnd - fogStart), 0.0, 1.0);
    FragColor = vec4(mix(fogColor, color.rgb, fogFactor), color.a);
}