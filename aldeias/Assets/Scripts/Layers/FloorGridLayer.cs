using UnityEngine;
using System.Collections;

public class FloorGridLayer : Layer {
	enum ATLAS {GRASS, TRIBE_A, TRIBE_B};

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
			if (tt.Flag.Value.Tribe.id == "A")
				return (int) ATLAS.TRIBE_A;
			else if (tt.Flag.Value.Tribe.id == "B")
				return (int) ATLAS.TRIBE_B;
		}
        
		return (int) ATLAS.GRASS;
	}
}