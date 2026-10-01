#version 460 core 

// The fragment shader only requires one output variable and that is a vector of size 4
out vec4 FragColor; 

in vec2 uvCoord;
uniform sampler2D texture0;
uniform sampler2D texture1;

void main()
{
    FragColor = mix(texture(texture1, uvCoord), texture(texture0, uvCoord), 0.8);
}