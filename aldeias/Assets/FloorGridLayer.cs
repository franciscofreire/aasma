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
			//floorGrid.SetTiles(WorldInfoTileToAtlasIndexFunc);
			worldHasChanged=false;
		}
	}

	int WorldInfoTileToAtlasIndexFunc(int x, int z) {
		const int ATLAS_TREE = 1;
		const int ATLAS_GRASS = 0;
		if (worldInfo.worldTileInfo[x,z].hasTree) {
			return ATLAS_TREE;
		} else {
			return ATLAS_GRASS;
		}
	}
}
