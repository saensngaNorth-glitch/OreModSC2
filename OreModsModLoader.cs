using Engine;

namespace Game {
    public class OreModsModLoader : ModLoader {
        public const string PackageName = "Pirachpol.OreMods";

        private static int m_titaniumOreIndex = -1;
        private static TerrainBrush m_titaniumBrush;

        public override void __ModInitialize() {
            ModsManager.RegisterHook("BlocksInitalized", this);
            ModsManager.RegisterHook("TerrainContentsGenerator24Initialize", this);
        }

        public override void BlocksInitalized() {
            m_titaniumOreIndex = BlocksManager.GetBlockIndex<TitaniumOreBlock>(true);

            TerrainBrush brush = new TerrainBrush();
            brush.AddCell(0, 0, 0, m_titaniumOreIndex);
            brush.AddCell(1, 0, 0, m_titaniumOreIndex);
            brush.AddCell(0, 1, 0, m_titaniumOreIndex);
            brush.AddCell(1, 1, 0, m_titaniumOreIndex);
            brush.AddCell(0, 0, 1, m_titaniumOreIndex);
            brush.AddCell(1, 0, 1, m_titaniumOreIndex);
            brush.AddCell(0, 1, 1, m_titaniumOreIndex);
            brush.AddCell(1, 1, 1, m_titaniumOreIndex);
            brush.Compile();
            m_titaniumBrush = brush;

            Log.Information($"Ore Mods: Titanium Ore index = {m_titaniumOreIndex}.");
        }

        public override void TerrainContentsGenerator24Initialize(
            ITerrainContentsGenerator terrainContentsGenerator,
            SubsystemTerrain subsystemTerrain) {
            if (m_titaniumBrush == null || m_titaniumOreIndex < 0) {
                return;
            }

            if (terrainContentsGenerator is not TerrainContentsGenerator24 generator) {
                return;
            }

            if (generator.ChunkGenerationStep3.Exists(step => step.Name == "OreMods_Titanium")) {
                return;
            }

            generator.ChunkGenerationStep3.Add(
                new ChunkGenerationStep(350, GenerateTitanium, "OreMods_Titanium")
            );
            generator.ChunkGenerationStep3.Sort((a, b) => a.GenerateOrder.CompareTo(b.GenerateOrder));
        }

        private static void GenerateTitanium(TerrainChunk chunk) {
            int x = chunk.Coords.X;
            int z = chunk.Coords.Y;
            Random random = new Random(x * 73428767 + z * 912931 + 137);

            int veinCount = random.Int(2, 4);
            for (int i = 0; i < veinCount; i++) {
                int oreX = x * 16 + random.Int(0, 15);
                int oreY = random.Int(8, 45);
                int oreZ = z * 16 + random.Int(0, 15);
                m_titaniumBrush.PaintFastSelective(chunk, oreX, oreY, oreZ, 3);
            }
        }
    }
}
