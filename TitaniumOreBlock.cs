namespace Game {
    public class TitaniumOreBlock : CubeBlock {
        public override void Initialize() {
            base.Initialize();
            CanBeBuiltIntoFurniture = true;
            int contents = BlocksManager.GetBlockIndex<TitaniumOreBlock>();
            Engine.Log.Information($"Ore Mods: Titanium Ore loaded (Contents: {contents}).");
        }
    }
}
