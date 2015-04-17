using UnityEngine;
using System.Collections;

public class StumpLayer : Layer {
	public GameObject stumpPrefab;
	
	private GameObject[,] stumps;

	public override void CreateObjects() {
		stumps = new GameObject[worldInfo.xSize,worldInfo.zSize];
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				stumps[x, z] = (GameObject) Instantiate(stumpPrefab, worldXZToVec3(x,z), Quaternion.identity);
				stumps[x, z].transform.parent = this.transform;
				stumps[x, z].SetActive(false);
			}
		}
	}
	
	public override void ApplyWorldInfo() {
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				stumps[x, z].SetActive(worldInfo.worldTileInfo[x,z].hasStump);
			}
		}
	}
}