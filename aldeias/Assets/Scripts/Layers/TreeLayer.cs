using UnityEngine;
using System.Collections;

public class TreeLayer : Layer {
	public GameObject treePrefab;
	
	private GameObject[,] trees;

	public override void CreateObjects() {
		trees = new GameObject[worldInfo.xSize,worldInfo.zSize];
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				trees[x, z] = (GameObject) Instantiate(treePrefab, worldXZToVec3(x,z), Quaternion.identity);
				trees[x, z].transform.parent = this.transform;
				trees[x, z].SetActive(false);
			}
		}
	}

	public override void ApplyWorldInfo() {
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				trees[x, z].SetActive(worldInfo.worldTileInfo[x,z].hasTree);
			}
		}
	}
}
