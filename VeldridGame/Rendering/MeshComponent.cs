using Veldrid;
using VeldridGame.Abstractions;

namespace VeldridGame.Rendering;

public class MeshComponent : Component
{
    public MeshComponent(Actor owner, bool isSkeletal = false)
        : base(owner)
    {
        IsSkeletal = isSkeletal;
        Owner.Game.Renderer.AddMeshComp(this);
    }

    public bool Visible { get; set; } = true;

    public Mesh Mesh { get; set; }

    public int TextureIndex { get; set; }

    public bool IsSkeletal { get; }

    /// <summary>
    /// Draw the mesh component with the provided shader.
    /// </summary>
    /// <param name="commandList"></param>
    /// <param name="shader">The shader to use for drawing the mesh.</param>
    public virtual void Draw(CommandList commandList, Shader shader)
    {
        // Set the world transform
        shader.SetUniform(commandList, ShaderUniforms.WorldBuffer, Owner.Transform.WorldTransform);

        // Set specular power
        shader.SetUniform(commandList, ShaderUniforms.MaterialBuffer, Mesh.MaterialInfo);

        // Set the active texture
        var texture = Mesh.GetTexture(TextureIndex);
        if (texture is not null)
        {
            texture.SetActive(commandList, 4);
        }

        // Set mesh's vertex array as active
        var vao = Mesh.VertexArrayObject;
        vao.SetActive(commandList);

        // Draw
        commandList.DrawIndexed((uint) vao.NumberOfIndices, 1, 0, 0, 0);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Owner.Game.Renderer.RemoveMeshComp(this);
        }

        base.Dispose(disposing);
    }
}