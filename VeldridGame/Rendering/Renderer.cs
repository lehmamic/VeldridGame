using Silk.NET.Maths;
using Veldrid;
using VeldridGame.Assets;
using VeldridGame.Camera;
using VeldridGame.Maths;
using VeldridGame.Rendering.Shader;

namespace VeldridGame.Rendering;

public class Renderer : IDisposable
{
    private readonly Game _game;
    private readonly IScreen _screen;
    private readonly IGraphics _graphics;
    private readonly IAssetProvider _assetProvider;

    private readonly List<SpriteComponent> _sprites = new();
    private readonly List<MeshComponent> _meshComps = new();

    private readonly ShaderBase _spriteShader;
    private readonly ShaderBase _meshShader;

    private readonly ShaderPipeline _meshShaderPipeline;
    private readonly CommandBuffer _meshCommandBuffer;

    private VertexArrayObject _spriteVertices;

    private readonly CommandList _commandList;


    // Lighting data
    public AmbientLightInfo AmbientLight { get; set; }
    public DirectionalLightInfo DirectionalLightInfo { get; set; }

    public Renderer(Game game, IScreen screen, IGraphics graphics, IAssetProvider assetProvider)
    {
        _game = game;
        _screen = screen;
        _graphics = graphics;
        _assetProvider = assetProvider;
        
        var factory = _graphics.Factory;
        _commandList = factory.CreateCommandList();
        _commandList.Begin();

        // Make sure we can load and compile shaders
        _spriteShader = new SpriteShader(_graphics, "Shaders/Sprite.vert", "Shaders/Sprite.frag");
        _spriteShader.SetActive(_commandList);
        
        // Set the view-projection matrix
        var spriteViewProj = GameMath.CreateSimpleViewProj(_screen.Width, _screen.Height);
        _spriteShader.SetUniform(_commandList, ShaderUniforms.ViewBuffer, spriteViewProj);
        
        _meshShader = new MeshShader(_graphics, "Shaders/Pong.vert", "Shaders/Pong.frag");
        _meshShader.SetActive(_commandList);

        // Set the view-projection matrix
        ViewMatrix = GameMath.CreateLookAt(Vector3D<float>.Zero, Vector3D<float>.UnitX, Vector3D<float>.UnitZ);
        _meshShader.SetUniform(_commandList, ShaderUniforms.ViewBuffer, ViewMatrix);

        ProjectionMatrix = GameMath.CreatePerspectiveFieldOfView(
            Scalar.DegreesToRadians(70.0f),
            _screen.Width, _screen.Height,
            25.0f,                  // Near plane
            10000.0f);
        _meshShader.SetUniform(_commandList, ShaderUniforms.ProjectionBuffer, ProjectionMatrix);
        
        // Create quad for drawing sprites
        CreateSpriteVertices();

        var shaderPipelineDescription = new ShaderPipelineDescription()
        {
            Shader = new ShaderVariant
            {
                VertexInputs =
                [
                    new VertexInput("Position", VertexElementFormat.Float3),
                    new VertexInput("Normal", VertexElementFormat.Float3),
                    new VertexInput("TexCoords", VertexElementFormat.Float2),
                ],
                Uniforms =
                [
                    new Uniform(ShaderUniforms.ProjectionBuffer, ResourceKind.UniformBuffer, 0, 64),
                    new Uniform(ShaderUniforms.ViewBuffer, ResourceKind.UniformBuffer, 1, 64),
                    new Uniform(ShaderUniforms.WorldBuffer, ResourceKind.UniformBuffer, 2, 64),
                    new Uniform(ShaderUniforms.CameraBuffer, ResourceKind.UniformBuffer, 3, CameraInfo.SizeInBytes),
                    new Uniform(ShaderUniforms.AmbientLightBuffer, ResourceKind.UniformBuffer, 4, AmbientLightInfo.SizeInBytes),
                    new Uniform(ShaderUniforms.DirectionalLightBuffer, ResourceKind.UniformBuffer, 5, DirectionalLightInfo.SizeInBytes),
                    new Uniform(ShaderUniforms.MaterialBuffer, ResourceKind.UniformBuffer, 6, 16),
                    new Uniform("SurfaceTexture", ResourceKind.TextureReadOnly, 7),
                    new Uniform("SurfaceSampler", ResourceKind.Sampler, 8),
                ],
                UniformStages =
                [
                    ShaderStages.Vertex,
                    ShaderStages.Vertex,
                    ShaderStages.Vertex,
                    ShaderStages.Fragment,
                    ShaderStages.Fragment,
                    ShaderStages.Fragment,
                    ShaderStages.Fragment,
                    ShaderStages.Fragment,
                    ShaderStages.Fragment,
                ],
                GlSlShaders =
                [
                    new ShaderDescription(ShaderStages.Vertex, File.ReadAllBytes("Shaders/PongNew.vert"), "main"),
                    new ShaderDescription(ShaderStages.Fragment, File.ReadAllBytes("Shaders/PongNew.frag"), "main"),
                ],
            },
        };
        _commandList.End();
        _graphics.Device.SubmitCommands(_commandList);
        
        _meshShaderPipeline = new ShaderPipeline(_graphics, _assetProvider, shaderPipelineDescription);
        _meshCommandBuffer = new CommandBuffer(_graphics, _meshShaderPipeline);

        _meshCommandBuffer.BeginRecording();
        _meshCommandBuffer.ActivatePipeline();
        _meshCommandBuffer.BindResources();
        _meshCommandBuffer.SetUniform(ShaderUniforms.ViewBuffer, ViewMatrix);
        _meshCommandBuffer.SetUniform(ShaderUniforms.ProjectionBuffer, ProjectionMatrix);
        _meshCommandBuffer.SubmitCommands();
        
        _graphics.Device.WaitForIdle();
    }
    
    public Matrix4X4<float> ViewMatrix { get; set; }

    public Matrix4X4<float> ProjectionMatrix { get; set; }

    public void Draw()
    {
        _commandList.Begin();
        
        /*
         * Draw 3d scene to the frame buffers
         */

        // Set the current frame buffer
        _commandList.SetFramebuffer(_graphics.ScreenTarget);
        
        // Clear color buffer/depth buffer
        _commandList.ClearColorTarget(0, RgbaFloat.Black);
        _commandList.ClearDepthStencil(1f);

        // Set the basic mesh shader active
        // _meshShader.SetActive(_commandList);
        //
        // // Update view-projection matrix
        // _meshShader.SetUniform(_commandList, ShaderUniforms.ViewBuffer, ViewMatrix);
        // _meshShader.SetUniform(_commandList, ShaderUniforms.ProjectionBuffer, ProjectionMatrix);
        //
        // // Update lighting uniforms
        // SetLightUniforms(_meshShader);
        //
        // // Draw all meshes
        // foreach (var mesh in _meshComps)
        // {
        //     if (mesh.Visible)
        //     {
        //         mesh.Draw(_commandList, _meshShader);
        //     }
        // }
        _commandList.End();
        _graphics.Device.SubmitCommands(_commandList);
        
        /* BEGIN new version */
        // Set the basic mesh shader active
        _meshCommandBuffer.BeginRecording();
        _meshCommandBuffer.SetRenderTarget(_graphics.ScreenTarget);
        _meshCommandBuffer.ActivatePipeline();
        _meshCommandBuffer.BindResources();

        // Update view-projection matrix
        _meshCommandBuffer.SetUniform(ShaderUniforms.ViewBuffer, ViewMatrix);
        _meshCommandBuffer.SetUniform(ShaderUniforms.ProjectionBuffer, ProjectionMatrix);
        
        // Update lighting uniforms
        SetLightUniforms(_meshCommandBuffer);

        // Draw all meshes
        foreach (var mesh in _meshComps)
        {
            if (mesh.Visible)
            {
                mesh.Draw(_meshCommandBuffer);
            }
        }
        /* END new version */
        _meshCommandBuffer.SubmitCommands();
        
        /*
         * Draw all sprite components
         */
        _commandList.Begin();
        _commandList.SetFramebuffer(_graphics.ScreenTarget);
        // Set sprite shader and vertex array objects active
        _spriteShader.SetActive(_commandList);
        _spriteVertices.SetActive(_commandList);

        // Draw all sprites
        foreach (var sprite in _sprites)
        {
            if (sprite.Visible)
            {
                sprite.Draw(_commandList, _spriteShader);
            }
        }
        
        _commandList.End();

        _graphics.Device.SubmitCommands(_commandList);
        _graphics.Device.SwapBuffers(_graphics.Device.MainSwapchain);
        _graphics.Device.WaitForIdle();
    }

    public void AddSprite(SpriteComponent sprite)
    {
        // Find the insertion point in the sorted vector
        // (The first element with a order higher than me)
        int index = 0;
        for (; index < _sprites.Count; index++)
        {
            if (sprite.DrawOrder < _sprites[index].DrawOrder)
            {
                break;
            }
        }

        // Inserts element before position of iterator
        _sprites.Insert(index, sprite);
    }

    public void RemoveSprite(SpriteComponent sprite)
    {
        _sprites.Remove(sprite);
    }

    public void AddMeshComp(MeshComponent mesh)
    {
        if (mesh.IsSkeletal)
        {
            // _skeletalMeshes.Add((SkeletalMeshComponent)mesh);
        }
        else
        {
            _meshComps.Add(mesh);
        }
    }

    public void RemoveMeshComp(MeshComponent mesh)
    {
        if (mesh.IsSkeletal)
        {
            // _skeletalMeshes.Remove((SkeletalMeshComponent) mesh);
        }
        else
        {
            _meshComps.Remove(mesh);
        }
    }

    public void Dispose()
    {
        _commandList.Dispose();

        _meshShader.Dispose();
        _spriteShader.Dispose();
        _meshShaderPipeline.Dispose();
        _meshCommandBuffer.Dispose();
    }
    
    private void CreateSpriteVertices()
    {
        var vertices = new[] {
            // vertex(3)/normal(3)/(uv coord)
            new VertexPositionNormalTexture(new(-0.5f, 0.5f, 0.0f), new(0.0f, 0.0f, 0.0f), new(0.0f, 0.0f)), // top left
            new VertexPositionNormalTexture(new(0.5f, 0.5f, 0.0f), new(0.0f, 0.0f, 0.0f), new(1.0f, 0.0f)), // top right
            new VertexPositionNormalTexture(new(0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 0.0f), new(1.0f, 1.0f)), // bottom right
            new VertexPositionNormalTexture(new(-0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, 0.0f), new(0.0f, 1.0f)),  // bottom left
        };

        var indices = new ushort[] {
            0, 1, 2,
            2, 3, 0
        };

        _spriteVertices = new VertexArrayObject(_graphics.Device, vertices, indices);
    }

    private void SetLightUniforms(ShaderBase shader)
    {
        // Camera position is from inverted view
        Matrix4X4.Invert(ViewMatrix, out var invView);
        shader.SetUniform(_commandList, ShaderUniforms.CameraBuffer, new CameraInfo(invView.GetTranslation()));

        // Ambient light
        shader.SetUniform(_commandList, ShaderUniforms.AmbientLightBuffer, AmbientLight);
    
        // Directional light
        shader.SetUniform(_commandList, ShaderUniforms.DirectionalLightBuffer, DirectionalLightInfo);
    }
    
    private void SetLightUniforms(CommandBuffer commands)
    {
        // Camera position is from inverted view
        Matrix4X4.Invert(ViewMatrix, out var invertedView);
        commands.SetUniform(ShaderUniforms.CameraBuffer, new CameraInfo(invertedView.GetTranslation()));

        // Ambient light
        commands.SetUniform(ShaderUniforms.AmbientLightBuffer, AmbientLight);
    
        // Directional light
        commands.SetUniform(ShaderUniforms.DirectionalLightBuffer, DirectionalLightInfo);
    }
}