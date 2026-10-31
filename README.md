# Ore Mods — Geological Titanium Generation

Target: Survivalcraft 2 API 1.9.5.1

## Titanium Ore

Titanium Ore is represented using two geological deposit settings:

1. **Hard-rock deposits** — irregular, narrow ore lenses replace basalt. Basalt is used as the closest available in-game proxy for mafic host rocks; the chance is increased in mountain belts. Generation is deterministic per world seed and supports deposits crossing chunk boundaries.
2. **Placer deposits** — small, buried patches replace sand or gravel near ocean shorelines and shallow river/lake margins. The patch is generated after surface materials and downward fluid propagation, so the generator can inspect the finished sediment layer. Deep-water seabeds and thin sediment layers are skipped.

The in-game Titanium Ore block is an abstraction for concentrated titanium-bearing minerals such as ilmenite and rutile; real deposits are not native titanium metal. This is a gameplay-scale geological approximation, not a full mineral-processing simulation.

## Important world-generation note

Terrain chunks already generated and saved before this update are not regenerated. Test the new distribution in a **new world**, or explore into previously ungenerated terrain in an existing world.

## Files

- `TitaniumOreBlock.cs` — Titanium Ore block.
- `OreModsModLoader.cs` — block registration and geological generation steps.
- `Assets/TitaniumOreBlocksData.csv` — block registration data.
- `Assets/Textures/TitaniumOre.png` — custom 32×32 block texture.

The custom texture loader in `TitaniumOreBlock.cs`, CSV registration, and PNG asset are preserved. The new world-generation logic is contained in `OreModsModLoader.cs`.

## Build

Requires .NET 10 SDK and `SurvivalcraftAPI.Survivalcraft` version `1.9.5.1`.
The GitHub Actions workflow builds the project and packages the `.scmod` artifact.
