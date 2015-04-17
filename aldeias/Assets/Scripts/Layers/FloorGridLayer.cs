using UnityEngine;
using System.Collections;

public class FloorGridLayer : Layer {
	enum ATLAS {GRASS, TRIBE_A, TRIBE_B, HABITAT};

	private FloorGrid floorGrid;
	
	public override void CreateObjects() {
		floorGrid = GetComponent<FloorGrid>();
	}
	
	public override void ApplyWorldInfo() {
		floorGrid.SetTiles(WorldInfoTileToAtlasIndexFunc);
	}

	int WorldInfoTileToAtlasIndexFunc(int x, int z) {
		WorldInfo.WorldTileInfo.TribeTerritory tt = worldInfo.worldTileInfo[x,z].tribeTerritory;

		if (tt.hasFlag == true) {
			if (tt.ownerTribe.id == "A")
				return (int) ATLAS.TRIBE_A;
			if (tt.ownerTribe.id == "B")
				return (int) ATLAS.TRIBE_B;
		}
		if (worldInfo.worldTileInfo[x,z].isHabitat == true)
			return (int) ATLAS.HABITAT;

		return (int) ATLAS.GRASS;
	}
}