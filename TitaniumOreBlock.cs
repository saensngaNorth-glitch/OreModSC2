using Engine;
using Engine.Graphics;

namespace Game {
    public class TitaniumOreBlock : CubeBlock {
        public Texture2D m_texture;

        public override void Initialize() {
            base.Initialize();

            CanBeBuiltIntoFurniture = true;

            m_texture = ContentManager.Get<Texture2D>(
                "Textures/TitaniumOre",
                "png"
            );

            int contents = BlocksManager.GetBlockIndex<TitaniumOreBlock>();

            Log.Information(
                $"Ore Mods: Titanium Ore loaded (Contents: {contents}, Texture: {m_texture != null})."
            );
        }

        public override int GetFaceTextureSlot(int face, int value) {
            return 0;
        }

        public override int GetTextureSlotCount(int value) {
            return 1;
        }

        public override Texture2D GetDefaultTexture(int value) {
            return m_texture;
        }
    }
}
