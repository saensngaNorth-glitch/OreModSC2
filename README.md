# Ore Mods

Survivalcraft 2 API 1.9.5.1.

## Current content
- Titanium Ore block
- Titanium Ore underground generation
- API 1.9.5.1 block registration via BlocksData CSV
- No hard-coded block ID

## Texture note
Titanium Ore currently uses the built-in block atlas slot 32 as a temporary functional texture. A dedicated Titanium texture can be added later without changing the generation system.

## Build
Requires .NET 10 SDK. The project references SurvivalcraftAPI.Survivalcraft 1.9.5.1 and creates `OreMods.scmod` after a successful Release build.
