﻿using UnityEngine;
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
		WorldInfo.WorldTileInfo.TribeTerritory tt = worldInfo.WorldTileInfoAtCoord(tileCoord).tribeTerritory;

		if (tt.hasFlag == true) {
			if (tt.ownerTribe.id == "A")
				return (int) ATLAS.TRIBE_A;
			else if (tt.ownerTribe.id == "B")
				return (int) ATLAS.TRIBE_B;
		}

		return (int) ATLAS.GRASS;
	}
}