using Veldrid;
using VeldridGame.Abstractions;
using VeldridGame.Rendering;
using Shader = VeldridGame.Rendering.Shader;

namespace VeldridGame.Terrains;

public class TerrainComponent : Component
{
    public TerrainComponent(Actor owner, int updateOrder = 100)
        : base(owner, updateOrder)
    {
        Owner.Game.Renderer.AddTerrain(this);
    }

    public bool Visible { get; set; } = true;
    
    public Terrain Terrain { get; set; }

    /// <summary>
    /// Draw the mesh component with the provided shader.
    /// </summary>
    /// <param name="commandList"></param>
    /// <param name="shader">The shader to use for drawing the mesh.</param>
    public virtual void Draw(CommandList commandList, Shader shader)
    {
        // Set the world transform
        shader.SetUniform(commandList, ShaderUniforms.WorldBuffer, Owner.Transform.WorldTransform);
        //
        // // Set specular power
        // shader.SetUniform(commandList, ShaderUniforms.MaterialBuffer, Mesh.MaterialInfo);

        // Set the active texture
        // var texture = Mesh.GetTexture(TextureIndex);
        // if (texture is not null)
        // {
        //     texture.SetActive(commandList, 4);
        // }

        // Set mesh's vertex array as active
        var vao = Terrain.VertexArrayObject;
        vao.SetActive(commandList);

        // Draw
        commandList.DrawIndexed((uint) vao.NumberOfIndices, 1, 0, 0, 0);
    }
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Owner.Game.Renderer.RemoveTerrain(this);
        }

        base.Dispose(disposing);
    }
}