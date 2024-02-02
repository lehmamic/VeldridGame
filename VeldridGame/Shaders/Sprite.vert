// Request GLSL 4.5
#version 450

// Uniforms for world transform and view-proj
layout(set = 0, binding = 0) uniform ViewBuffer
{
    mat4 View;
};

layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
};

// Attribute 0 is position, 1 is normal, 2 is tex coords
layout(location=0) in vec3 Position;
layout(location=1) in vec3 Normal;
layout(location=2) in vec2 TextCoord;

// Add texture coordinate as output
layout(location = 0) out vec2 fragTextCoord;

void main()
{
    // Convert position to homogeneous coordinates
    vec4 pos = vec4(Position, 1.0);

    // Transform position to world space, then clip space
    gl_Position =  View * World * pos;

    // Pass along the texture coordinate to frag shader
    fragTextCoord = TextCoord;
}