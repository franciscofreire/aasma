using UnityEngine;
using System.Collections;

public class TreeLayer : MonoBehaviour {

	public WorldInfo worldInfo;
	public GameObject treePrefab;
	public float tileSize=1.0f;
	
	private GameObject[,] trees;
	private bool worldHasChanged=false;

	void Start() {
		CreateTreeObjects();
		worldInfo.AddChangeListener(()=>{worldHasChanged=true;});
	}

	void Update() {
		if(worldHasChanged) {
			ApplyWorldInfo();
		}
		worldHasChanged=false;
	}

	Vector3 worldXZToVec3(int x, int z) {
		float halfTileSize = tileSize * 0.5f;
		return new Vector3(x+halfTileSize, 0, z+halfTileSize);
	}

	void CreateTreeObjects() {
		trees = new GameObject[worldInfo.xSize,worldInfo.zSize];
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				trees[x, z] = (GameObject) Instantiate(treePrefab, worldXZToVec3(x,z), Quaternion.identity);
				trees[x, z].transform.parent = this.transform;
				trees[x, z].SetActive(false);
			}
		}
	}

	void ApplyWorldInfo() {
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				trees[x, z].SetActive(worldInfo.worldTileInfo[x,z].hasTree);
			}
		}
	}
}
