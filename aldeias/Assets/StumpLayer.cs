using UnityEngine;
using System.Collections;

public class StumpLayer : MonoBehaviour {
	
	public WorldInfo worldInfo;
	public GameObject stumpPrefab;
	public float tileSize=1.0f;
	
	private GameObject[,] stumps;
	private bool worldHasChanged=false;
	
	void Start() {
		CreateStumpsObjects();
		worldInfo.AddChangeListener(()=>{worldHasChanged=true;});
	}
	
	void Update() {
		if(worldHasChanged) {
			ApplyWorldInfo();
			worldHasChanged=false;
		}
	}
	
	Vector3 worldXZToVec3(int x, int z) {
		float halfTileSize = tileSize * 0.5f;
		return new Vector3(x+halfTileSize, 0, z+halfTileSize);
	}
	
	void CreateStumpsObjects() {
		stumps = new GameObject[worldInfo.xSize,worldInfo.zSize];
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				stumps[x, z] = (GameObject) Instantiate(stumpPrefab, worldXZToVec3(x,z), Quaternion.identity);
				stumps[x, z].transform.parent = this.transform;
				stumps[x, z].SetActive(false);
			}
		}
	}
	
	void ApplyWorldInfo() {
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				stumps[x, z].SetActive(worldInfo.worldTileInfo[x,z].hasStump);
			}
		}
	}
}

