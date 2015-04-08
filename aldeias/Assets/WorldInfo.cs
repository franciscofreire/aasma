using UnityEngine;
using System.Collections;

public class WorldInfo : MonoBehaviour {
	
	//Classes and values that have to be declared and defined before the actual WorldTileInfo declaration
	public partial class WorldTileInfo {
		public static Tribe nullTribe=new Tribe();
		public static TribeTerritory defaultTribeTerritory=new TribeTerritory();
		public class TribeTerritory {
			public bool hasFlag=false;
			public Tribe ownerTribe=nullTribe;
		}
		public class Tribe {
			//Insert tribe identification here
		}
	}

	//Information being holded in every tile
	//The information contained in every tile is readonly.
	public partial class WorldTileInfo {
		public bool hasTree=false;
		public bool isHabitat=false;
		public TribeTerritory tribeTerritory=defaultTribeTerritory;
	}


	public int xSize = 50;
	public int zSize = 50;
	public WorldTileInfo[,] worldTileInfo;

	void Start () {
		GenerateWorldTileInfo ();
		SetDebugWorldTileInfo ();
	}

	void GenerateWorldTileInfo () {
		worldTileInfo = new WorldTileInfo[xSize,zSize];
		FillWithDefaultWorldTileInfo ();
	}

	void FillWithDefaultWorldTileInfo () {
		for(int x=0; x<xSize; x++) {
			for(int z=0; z<zSize; z++) {
				worldTileInfo[x,z] = new WorldTileInfo();
			}
		}
	}

	//FIXME: Currently setting WorldTileInfo fields directly. Might need to have a better way to change their values.
	void SetDebugWorldTileInfo () {
		//Fill (0,0) to (xsize/2 - 1,zSize/2 - 1) with animal habitat
		for(int x=0; x<xSize/2; x++) {
			for(int z=0; z<zSize/2; z++) {
				worldTileInfo[x,z].isHabitat = true;
			}
		}
		//Fill (xSize/2,0) to (xsize-1,zSize/2 - 1) corner with trees
		for(int x=xSize/2; x<xSize; x++) {
			for(int z=0; z<zSize/2; z++) {
				worldTileInfo[x,z].hasTree = true;
			}
		}
		//Fill (0,zSize/2) to (xSize/2 - 1,zSize-1) with null tribe flags
		for(int x=0; x<xSize/2; x++) {
			for(int z=zSize/2; z<zSize; z++) {
				worldTileInfo[x,z].tribeTerritory.hasFlag = true;
			}
		}
	}
}
