using Silk.NET.Maths;
using Veldrid;
using VeldridGame.Abstractions;

namespace VeldridGame.Rendering;

public class SpriteComponent : Component
{
    public SpriteComponent(Actor actor, int drawOrder = 100)
        : base(actor)
    {
        DrawOrder = drawOrder;

        Actor.Game.Renderer.AddSprite(this);
    }

    public int DrawOrder { get; }

    public bool Visible { get; set; } = true;

    public Texture2D Texture { get; set; }

    public virtual unsafe void Draw(CommandList commandList, ShaderBase shader)
    {
        // Scale the quad by the width/height of texture
        Matrix4X4<float> scaleMat = Matrix4X4.CreateScale(Texture.Width, Texture.Height, 1.0f);

        Matrix4X4<float> world = scaleMat * Actor.Transform.WorldTransform;

        // Since all sprites use the same shader/vertices,
        // the game first sets them active before any sprite draws

        // Set world transform
        shader.SetUniform(commandList, ShaderUniforms.WorldBuffer, world);

        // Set current texture
        Texture.SetActive(commandList, 2);

        // Draw quad
        commandList.DrawIndexed(6, 1, 0, 0, 0);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Actor.Game.Renderer.RemoveSprite(this);
        }

        base.Dispose(disposing);
    }
}