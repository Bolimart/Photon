// declare the OpenGL version used (4.6 core)
#version 460 core 
//set the location of the input variable via layout (location = 0)
//declare all the input vertex attributes in the vertex shader with the in keyword.
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aUvCoord;

out vec2 uvCoord;

uniform mat4 transfrom;

void main()
{
    // built-in variable for vertex shaders that represents the final position of that vertex, gl_Position is a vec4.
    gl_Position = vec4(aPosition, 1.0) * transfrom;
    uvCoord = aUvCoord;
}