// Request GLSL 4.5
#version 450

// Text coord input from vertex shader
layout(location = 0) in vec2 fragTextCoord;

// This corresponds to the output color to the color buffer
layout(location = 0) out vec4 outColor;

// For texture sampling
//layout(set = 2, binding = 0) uniform texture2D SurfaceTexture;
//layout(set = 2, binding = 1) uniform sampler SurfaceSampler;

void main()
{
    outColor =  vec4(0.5, 0.0, 0.0, 1.0);
}