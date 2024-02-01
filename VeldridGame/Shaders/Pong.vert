// Request GLSL 4.5
#version 450

// Uniforms for world transform and view-proj
layout(set = 0, binding = 0) uniform ProjectionBuffer
{
    mat4 Projection;
};

layout(set = 0, binding = 1) uniform ViewBuffer
{
    mat4 View;
};

layout(set = 1, binding = 0) uniform WorldBuffer
{
    mat4 World;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoords;

// Add texture coordinate as output
layout(location = 0) out vec2 fragTextCoord;

// Normal (in world space)
layout(location = 1) out vec3 fragNormal;

// Position
layout(location = 2) out vec3 fragWorldPos;

void main()
{
    // Convert position to homogeneous coordinates
    vec4 pos = vec4(Position, 1.0);

    // Transform position to world space
    pos = World * pos;

    // Save world position
    fragWorldPos = pos.xyz;

    // Transform position to world space, then clip space
    gl_Position = Projection * View * pos;

    // Pass along the texture coordinate to frag shader
    fragTextCoord = TexCoords;

    // Transform normal into world space
    fragNormal = (World * vec4(Normal, 0.0f)).xyz;
}