using Engine;
using Engine.Graphics;

namespace Game
{
    public class TitaniumOreBlock : CubeBlock
    {
        public Texture2D m_texture;

        public override void Initialize()
        {
            base.Initialize();

            CanBeBuiltIntoFurniture = true;

            m_texture = ContentManager.Get<Texture2D>(
                "Textures/TitaniumOre"
            );

            int contents = BlocksManager.GetBlockIndex<TitaniumOreBlock>();

            Log.Information(
                $"Ore Mods: Titanium Ore loaded (Contents: {contents})."
            );
        }

        public override Texture2D GetDefaultTexture(int value)
        {
            return m_texture;
        }
    }
}
