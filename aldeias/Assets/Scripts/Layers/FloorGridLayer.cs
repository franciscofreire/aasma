using UnityEngine;
using System.Collections;

public class FloorGridLayer : Layer {
	enum ATLAS {GRASS, TRIBE_BLUE, TRIBE_RED};

	private FloorGrid floorGrid;
	
	public override void CreateObjects() {
		floorGrid = GetComponent<FloorGrid>();
	}
	
	public override void ApplyWorldInfo() {
		floorGrid.SetTiles(WorldInfoTileToAtlasIndexFunc);
	}

	int WorldInfoTileToAtlasIndexFunc(Vector2I tileCoord) {
		WorldTileInfo.TribeTerritory tt = worldInfo.worldTiles.WorldTileInfoAtCoord(tileCoord).tribeTerritory;

		if (tt.IsClaimed) {
			if (tt.Flag.Value.Tribe.id == "Blue")
				return (int) ATLAS.TRIBE_BLUE;
			else if (tt.Flag.Value.Tribe.id == "Red")
				return (int) ATLAS.TRIBE_RED;
		}
        
		return (int) ATLAS.GRASS;
	}
}