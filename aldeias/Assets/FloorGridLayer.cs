using UnityEngine;
using System.Collections;

public class FloorGridLayer : MonoBehaviour {

	public WorldInfo worldInfo;
	private FloorGrid floorGrid;

	private bool worldHasChanged=false;

	// Use this for initialization
	void Start () {
		floorGrid = GetComponent<FloorGrid>();
		worldInfo.AddChangeListener(()=>{worldHasChanged=true;});
	}
	
	// Update is called once per frame
	void Update () {
		if(worldHasChanged){
			floorGrid.SetTiles(WorldInfoTileToAtlasIndexFunc);
			worldHasChanged=false;
		}
	}

	int WorldInfoTileToAtlasIndexFunc(int x, int z) {
		const int ATLAS_GRASS = 0;
		WorldInfo.WorldTileInfo.TribeTerritory tt = worldInfo.worldTileInfo[x,z].tribeTerritory;

		if (tt.hasFlag == true) {
			if (tt.ownerTribe.id == "A")
				return 1;
			if (tt.ownerTribe.id == "B")
				return 2;
		}

		return ATLAS_GRASS;
	}
}
