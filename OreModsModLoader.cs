using Engine;

namespace Game
{
    public class OreModsModLoader : ModLoader
    {
        public const string PackageName = "Pirachpol.OreMods";

        // -----------------------------
        // Titanium placer settings
        // -----------------------------

        private const int PlacerWaterDepthLimit = 8;
        private const int PlacerShoreMinDistance = -8;
        private const int PlacerShoreMaxDistance = 12;

        // -----------------------------
        // Titanium hard-rock host rocks
        // Verified from Survivalcraft API 1.9.5.1
        // -----------------------------

        private const int GraniteContents = 3;
        private const int LimestoneContents = 66;
        private const int BasaltContents = 67;
        private const int AirContents = 0;

        private static int m_titaniumOreIndex = -1;

        public override void __ModInitialize()
        {
            ModsManager.RegisterHook("BlocksInitalized", this);
            ModsManager.RegisterHook(
                "TerrainContentsGenerator24Initialize",
                this
            );
        }

        public override void BlocksInitalized()
        {
            m_titaniumOreIndex =
                BlocksManager.GetBlockIndex<TitaniumOreBlock>(true);

            Log.Information(
                $"Ore Mods: Titanium Ore registered for geological world generation (Contents: {m_titaniumOreIndex})."
            );
        }

        public override void TerrainContentsGenerator24Initialize(
            ITerrainContentsGenerator terrainContentsGenerator,
            SubsystemTerrain subsystemTerrain)
        {
            if (m_titaniumOreIndex < 0)
            {
                return;
            }

            if (terrainContentsGenerator is not TerrainContentsGenerator24 generator)
            {
                return;
            }

            bool addedStep = false;

            // -----------------------------------------
            // 1. Hard-rock Titanium
            // Runs after vanilla caves/pockets/minerals.
            // -----------------------------------------

            if (!generator.ChunkGenerationStep3.Exists(
                step => step.Name == "OreMods_Titanium_HardRock"))
            {
                generator.ChunkGenerationStep3.Add(
                    new ChunkGenerationStep(
                        350,
                        chunk => GenerateTitaniumHardRock(
                            chunk,
                            generator
                        ),
                        "OreMods_Titanium_HardRock"
                    )
                );

                addedStep = true;
            }

            // -----------------------------------------
            // 2. Cave-exposed Titanium
            // Runs after hard-rock generation and after caves.
            // -----------------------------------------

            if (!generator.ChunkGenerationStep3.Exists(
                step => step.Name == "OreMods_Titanium_CaveExposure"))
            {
                generator.ChunkGenerationStep3.Add(
                    new ChunkGenerationStep(
                        360,
                        chunk => GenerateTitaniumCaveExposure(
                            chunk,
                            generator
                        ),
                        "OreMods_Titanium_CaveExposure"
                    )
                );

                addedStep = true;
            }

            // -----------------------------------------
            // 3. Coastal / sedimentary placer Titanium
            // -----------------------------------------

            if (!generator.ChunkGenerationStep3.Exists(
                step => step.Name == "OreMods_Titanium_Placer"))
            {
                generator.ChunkGenerationStep3.Add(
                    new ChunkGenerationStep(
                        600,
                        chunk => GenerateTitaniumPlacers(
                            chunk,
                            generator
                        ),
                        "OreMods_Titanium_Placer"
                    )
                );

                addedStep = true;
            }

            if (addedStep)
            {
                generator.ChunkGenerationStep3.Sort(
                    (a, b) => a.GenerateOrder.CompareTo(b.GenerateOrder)
                );

                Log.Information(
                    "Ore Mods: Titanium geological generation registered (hard-rock, cave-exposed, and sedimentary placer deposits)."
                );
            }
        }

        // ============================================================
        // HARD-ROCK TITANIUM
        // ============================================================

        private static void GenerateTitaniumHardRock(
            TerrainChunk chunk,
            TerrainContentsGenerator24 generator)
        {
            if (m_titaniumOreIndex < 0 ||
                !generator.TGCavesAndPockets)
            {
                return;
            }

            int chunkX = chunk.Coords.X;
            int chunkZ = chunk.Coords.Y;

            // Match vanilla mineral generation's 3x3 source-chunk
            // sampling so deposits can cross chunk borders naturally.
            for (
                int sourceChunkX = chunkX - 1;
                sourceChunkX <= chunkX + 1;
                sourceChunkX++)
            {
                for (
                    int sourceChunkZ = chunkZ - 1;
                    sourceChunkZ <= chunkZ + 1;
                    sourceChunkZ++)
                {
                    Random random = new Random(
                        CreateChunkSeed(
                            generator.m_seed,
                            sourceChunkX,
                            sourceChunkZ,
                            0x13579B
                        )
                    );

                    float mountainFactor =
                        MathUtils.Saturate(
                            generator.CalculateMountainRangeFactor(
                                sourceChunkX * TerrainChunk.Size
                                    + TerrainChunk.Size / 2,
                                sourceChunkZ * TerrainChunk.Size
                                    + TerrainChunk.Size / 2
                            )
                        );

                    // Stronger Titanium probability in mountain belts.
                    float mountainInfluence =
                        MathUtils.Saturate(
                            (mountainFactor - 0.75f) / 0.25f
                        );

                    float veinChance =
                        0.025f + 0.18f * mountainInfluence;

                    if (!random.Bool(veinChance))
                    {
                        continue;
                    }

                    int veinCount = 1;

                    if (mountainInfluence > 0.75f &&
                        random.Bool(0.10f))
                    {
                        veinCount = 2;
                    }

                    for (int i = 0; i < veinCount; i++)
                    {
                        int oreX =
                            sourceChunkX * TerrainChunk.Size
                            + random.Int(
                                0,
                                TerrainChunk.Size - 1
                            );

                        int oreY = random.Int(18, 110);

                        int oreZ =
                            sourceChunkZ * TerrainChunk.Size
                            + random.Int(
                                0,
                                TerrainChunk.Size - 1
                            );

                        TerrainBrush vein =
                            CreateHardRockVeinBrush(
                                random,
                                m_titaniumOreIndex
                            );

                        // Basalt: strongest geological match.
                        vein.PaintFastSelective(
                            chunk,
                            oreX,
                            oreY,
                            oreZ,
                            BasaltContents
                        );

                        // Granite: allows hard-rock deposits
                        // in mountainous granite terrain.
                        vein.PaintFastSelective(
                            chunk,
                            oreX,
                            oreY,
                            oreZ,
                            GraniteContents
                        );

                        // Limestone: supports deposits in other
                        // natural mountain/rock formations.
                        vein.PaintFastSelective(
                            chunk,
                            oreX,
                            oreY,
                            oreZ,
                            LimestoneContents
                        );
                    }
                }
            }
        }

        // ============================================================
        // CAVE-EXPOSED TITANIUM
        // ============================================================

        private static void GenerateTitaniumCaveExposure(
            TerrainChunk chunk,
            TerrainContentsGenerator24 generator)
        {
            if (m_titaniumOreIndex < 0 ||
                !generator.TGCavesAndPockets)
            {
                return;
            }

            Random random = new Random(
                CreateChunkSeed(
                    generator.m_seed,
                    chunk.Coords.X,
                    chunk.Coords.Y,
                    0x5A17C4
                )
            );

            float mountainFactor =
                MathUtils.Saturate(
                    generator.CalculateMountainRangeFactor(
                        chunk.Origin.X + TerrainChunk.Size / 2,
                        chunk.Origin.Y + TerrainChunk.Size / 2
                    )
                );

            float mountainInfluence =
                MathUtils.Saturate(
                    (mountainFactor - 0.70f) / 0.30f
                );

            // Cave exposure is intentionally uncommon.
            // Mountain regions get a stronger chance.
            float exposureChance =
                0.035f + 0.09f * mountainInfluence;

            if (!random.Bool(exposureChance))
            {
                return;
            }

            int attempts = random.Int(8, 14);

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                int x = random.Int(2, TerrainChunk.Size - 3);
                int y = random.Int(12, 105);
                int z = random.Int(2, TerrainChunk.Size - 3);

                int current =
                    chunk.GetCellContentsFast(x, y, z);

                if (!IsHardRock(current))
                {
                    continue;
                }

                if (!IsExposedToCave(
                    chunk,
                    x,
                    y,
                    z))
                {
                    continue;
                }

                TerrainBrush vein =
                    CreateHardRockVeinBrush(
                        random,
                        m_titaniumOreIndex
                    );

                // Paint only the actual host-rock types.
                vein.PaintFastSelective(
                    chunk,
                    x,
                    y,
                    z,
                    BasaltContents
                );

                vein.PaintFastSelective(
                    chunk,
                    x,
                    y,
                    z,
                    GraniteContents
                );

                vein.PaintFastSelective(
                    chunk,
                    x,
                    y,
                    z,
                    LimestoneContents
                );

                // One cave-exposed deposit per generated chunk
                // is enough for a rare geological occurrence.
                return;
            }
        }

        private static bool IsExposedToCave(
            TerrainChunk chunk,
            int x,
            int y,
            int z)
        {
            return
                chunk.GetCellContentsFast(x - 1, y, z)
                    == AirContents
                ||
                chunk.GetCellContentsFast(x + 1, y, z)
                    == AirContents
                ||
                chunk.GetCellContentsFast(x, y - 1, z)
                    == AirContents
                ||
                chunk.GetCellContentsFast(x, y + 1, z)
                    == AirContents
                ||
                chunk.GetCellContentsFast(x, y, z - 1)
                    == AirContents
                ||
                chunk.GetCellContentsFast(x, y, z + 1)
                    == AirContents;
        }

        private static bool IsHardRock(int contents)
        {
            return
                contents == BasaltContents
                ||
                contents == GraniteContents
                ||
                contents == LimestoneContents;
        }

        // ============================================================
        // PLACER TITANIUM
        // ============================================================

        private static void GenerateTitaniumPlacers(
            TerrainChunk chunk,
            TerrainContentsGenerator24 generator)
        {
            if (m_titaniumOreIndex < 0 ||
                !generator.TGCavesAndPockets)
            {
                return;
            }

            Random random = new Random(
                CreateChunkSeed(
                    generator.m_seed,
                    chunk.Coords.X,
                    chunk.Coords.Y,
                    0x2468AC
                )
            );

            int attempts = random.Int(5, 9);

            for (int attempt = 0; attempt < attempts; attempt++)
            {
                int localX = random.Int(3, 12);
                int localZ = random.Int(3, 12);

                int sedimentTopY;
                int sedimentThickness;
                int waterDepth;

                if (!TryFindSurfaceSediment(
                    chunk,
                    localX,
                    localZ,
                    out sedimentTopY,
                    out sedimentThickness,
                    out waterDepth))
                {
                    continue;
                }

                int worldX =
                    chunk.Origin.X + localX;

                int worldZ =
                    chunk.Origin.Y + localZ;

                float shoreDistance =
                    generator.CalculateOceanShoreDistance(
                        worldX,
                        worldZ
                    );

                bool coastalSite =
                    shoreDistance >= PlacerShoreMinDistance
                    &&
                    shoreDistance <= PlacerShoreMaxDistance;

                bool shallowWaterSite =
                    waterDepth > 0 &&
                    waterDepth <= 6;

                bool fluvialSite =
                    !coastalSite
                    &&
                    !shallowWaterSite
                    &&
                    IsNearSurfaceWater(
                        chunk,
                        localX,
                        localZ,
                        sedimentTopY
                    );

                if (!coastalSite &&
                    !shallowWaterSite &&
                    !fluvialSite)
                {
                    continue;
                }

                // Heavy-mineral concentrations remain patchy.
                if (!random.Bool(0.18f))
                {
                    continue;
                }

                int maxBurialDepth =
                    sedimentThickness - 1;

                if (maxBurialDepth > 3)
                {
                    maxBurialDepth = 3;
                }

                if (maxBurialDepth < 1)
                {
                    continue;
                }

                // Keep at least one sediment layer above the deposit.
                int oreY =
                    sedimentTopY
                    - random.Int(
                        1,
                        maxBurialDepth
                    );

                TerrainBrush patch =
                    CreatePlacerPatch(
                        random,
                        m_titaniumOreIndex
                    );

                patch.PaintFastSelective(
                    chunk,
                    worldX,
                    oreY,
                    worldZ,
                    SandBlock.Index
                );

                patch.PaintFastSelective(
                    chunk,
                    worldX,
                    oreY,
                    worldZ,
                    GravelBlock.Index
                );
            }
        }

        // ============================================================
        // SEDIMENT DETECTION
        // ============================================================

        private static bool TryFindSurfaceSediment(
            TerrainChunk chunk,
            int localX,
            int localZ,
            out int sedimentTopY,
            out int sedimentThickness,
            out int waterDepth)
        {
            sedimentTopY = -1;
            sedimentThickness = 0;
            waterDepth = 0;

            int y =
                chunk.CalculateTopmostCellHeight(
                    localX,
                    localZ
                );

            while (
                y > 0
                &&
                chunk.GetCellContentsFast(
                    localX,
                    y,
                    localZ
                ) == WaterBlock.Index)
            {
                waterDepth++;
                y--;
            }

            if (waterDepth > PlacerWaterDepthLimit ||
                y <= 0)
            {
                return false;
            }

            int contents =
                chunk.GetCellContentsFast(
                    localX,
                    y,
                    localZ
                );

            if (!IsLooseSediment(contents))
            {
                return false;
            }

            sedimentTopY = y;

            while (
                y >= 0
                &&
                IsLooseSediment(
                    chunk.GetCellContentsFast(
                        localX,
                        y,
                        localZ
                    )
                )
            )
            {
                sedimentThickness++;
                y--;
            }

            return sedimentThickness >= 2;
        }

        private static bool IsNearSurfaceWater(
            TerrainChunk chunk,
            int localX,
            int localZ,
            int sedimentTopY)
        {
            for (int dx = -3; dx <= 3; dx++)
            {
                for (int dz = -3; dz <= 3; dz++)
                {
                    if (dx == 0 && dz == 0)
                    {
                        continue;
                    }

                    int x = localX + dx;
                    int z = localZ + dz;

                    if (x < 0 ||
                        x >= TerrainChunk.Size ||
                        z < 0 ||
                        z >= TerrainChunk.Size)
                    {
                        continue;
                    }

                    int minY =
                        sedimentTopY - 5;

                    int maxY =
                        sedimentTopY + 5;

                    if (minY < 0)
                    {
                        minY = 0;
                  }

        private static bool IsNearSurfaceWater(
            TerrainChunk chunk,
            int localX,
            int localZ,
            int sedimentTopY) {
            for (int dx = -3; dx <= 3; dx++) {
                for (int dz = -3; dz <= 3; dz++) {
                    if (dx == 0 && dz == 0) {
                        continue;
                    }

                    int x = localX + dx;
                    int z = localZ + dz;
                    if (x < 0 || x >= TerrainChunk.Size || z < 0 || z >= TerrainChunk.Size) {
                        continue;
                    }

                    int minY = sedimentTopY - 5;
                    int maxY = sedimentTopY + 5;
                    if (minY < 0) {
                        minY = 0;
                    }
                    if (maxY >= TerrainChunk.Height) {
                        maxY = TerrainChunk.Height - 1;
                    }

                    // Inspect only a narrow band around the sediment elevation.
                    // This avoids a full 256-block column scan for every candidate point.
                    for (int y = minY; y <= maxY; y++) {
                        if (chunk.GetCellContentsFast(x, y, z) == WaterBlock.Index) {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsLooseSediment(int contents) =>
            contents == SandBlock.Index || contents == GravelBlock.Index;

        private static TerrainBrush CreateHardRockVeinBrush(Random random, int contents) {
            TerrainBrush brush = new TerrainBrush();

            int endX = random.Int(-4, 4);
            int endY = random.Int(-2, 2);
            int endZ = random.Int(-4, 4);
            if (endX == 0 && endY == 0 && endZ == 0) {
                endX = 3;
            }

            // Main narrow, irregular vein.
            brush.AddRay(0, 0, 0, endX, endY, endZ, 1, 1, 1, contents);

            // A short branch gives the deposit a natural lens-like outline.
            if (random.Bool(0.60f)) {
                int branchStartX = endX / 2;
                int branchStartY = endY / 2;
                int branchStartZ = endZ / 2;
                int branchEndX = branchStartX + random.Int(-2, 2);
                int branchEndY = branchStartY + random.Int(-1, 1);
                int branchEndZ = branchStartZ + random.Int(-2, 2);

                if (branchStartX == branchEndX
                    && branchStartY == branchEndY
                    && branchStartZ == branchEndZ) {
                    branchEndZ++;
                }

                brush.AddRay(
                    branchStartX,
                    branchStartY,
                    branchStartZ,
                    branchEndX,
                    branchEndY,
                    branchEndZ,
                    1,
                    1,
                    1,
                    contents
                );
            }

            // Small side pockets break up the straight-line appearance.
            int extraCells = random.Int(1, 4);
            for (int i = 0; i < extraCells; i++) {
                brush.AddCell(
                    random.Int(-2, 2),
                    random.Int(-1, 1),
                    random.Int(-2, 2),
                    contents
                );
            }

            brush.Compile();
            return brush;
        }

        private static TerrainBrush CreatePlacerPatch(Random random, int contents) {
            TerrainBrush brush = new TerrainBrush();
            brush.AddCell(0, 0, 0, contents);

            int radiusX = random.Int(1, 2);
            int radiusZ = random.Int(1, 2);

            for (int dx = -radiusX; dx <= radiusX; dx++) {
                for (int dz = -radiusZ; dz <= radiusZ; dz++) {
                    if (random.Bool(0.52f)) {
                        brush.AddCell(dx, 0, dz, contents);
                    }

                    if (random.Bool(0.14f)) {
                        brush.AddCell(dx, -1, dz, contents);
                    }
                }
            }

            brush.Compile();
            return brush;
        }

        private static int CreateChunkSeed(int worldSeed, int chunkX, int chunkZ, int salt) =>
            unchecked(worldSeed + chunkX * 73428767 + chunkZ * 912931 + salt);
    }
}
