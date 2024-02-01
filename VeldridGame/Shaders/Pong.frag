// Request GLSL 4.5
#version 450

// Create a structure for directional light
struct DirectionalLightInfo
{
    // Direction of light
    vec3 Direction;

    // Diffuse color
    vec3 DiffuseColor;

    // Specular color
    vec3 SpecColor;
};

// Create a structure for material
struct MaterialInfo
{
    // Specular power for this surface
    float SpecPower;
};

// Text coord input from vertex shader
layout(location = 0) in vec2 fragTextCoord;

// Normal (in world space)
layout(location = 1) in vec3 fragNormal;

// Position
layout(location = 2) in vec3 fragWorldPos;

// Uniforms for lighting
// Camera position (in world space)
layout(set = 2, binding = 0) uniform CameraPositionBuffer
{
    vec3 CameraPosition;
};

// Ambient light level
layout(set = 2, binding = 1) uniform AmbientLightBuffer
{
    vec3 AmbientLight;
};

// Directional Light (only one for now)
layout(set = 2, binding = 2) uniform DirectionalLightBuffer
{
    DirectionalLightInfo DirLight;
};

// Uniforms for material
// Specular power for this surface
layout(set = 3, binding = 0) uniform MaterialBuffer
{
    MaterialInfo Material;
};

// For texture sampling
layout(set = 4, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 4, binding = 1) uniform sampler SurfaceSampler;

// This corresponds to the output color to the color buffer
layout(location = 0) out vec4 outColor;

void main()
{
    // Surface normal
    vec3 N = normalize(fragNormal);

    // Vector from surface to light
    vec3 L = normalize(-DirLight.Direction);

    // Vector from surface to camera
    vec3 V = normalize(CameraPosition - fragWorldPos);

    // Reflection of -L about N
    vec3 R = normalize(reflect(-L, N));

    // Compute phong reflection
    vec3 Phong = AmbientLight;
    float NdotL = dot(N, L);
    if (NdotL > 0)
    {
        vec3 Diffuse = DirLight.DiffuseColor * NdotL;
        vec3 Specular = DirLight.SpecColor * pow(max(0.0, dot(R, V)), Material.SpecPower);

        Phong += Diffuse + Specular;
    }

    // Final color is texture color times phong light (alpha 1)
    outColor = texture(sampler2D(SurfaceTexture, SurfaceSampler), fragTextCoord) * vec4(Phong, 1.0f);
}