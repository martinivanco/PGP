#version 410 core
layout(location = 0) in vec3 vertexPosition;
layout(location = 1) in vec2 uvPosition;
out vec2 pixelPosition;
uniform vec3 cameraPosition;
uniform vec3 tlCoord;
uniform vec3 xStep;
uniform vec3 yStep;

void main(){
    gl_Position.xyz = vertexPosition;
    gl_Position.w = 1.0;
    pixelPosition = uvPosition;
}
