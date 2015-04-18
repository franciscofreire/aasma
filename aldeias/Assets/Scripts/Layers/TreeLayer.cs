using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeLayer : Layer {
	public GameObject treeModel, stumpModel;
	
	private KeyValuePair<Tree,GameObject>[,] trees;

	public override void CreateObjects() {
		trees = new KeyValuePair<Tree,GameObject>[worldInfo.xSize, worldInfo.zSize];
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				// Create tree model
				trees[x, z] = new KeyValuePair<Tree,GameObject>(
					new Tree(x, z),
					(GameObject) Instantiate(treeModel, worldXZToVec3(x, z), Quaternion.identity));
				trees[x, z].Value.transform.parent = this.transform;
				trees[x, z].Value.SetActive(false);
			}
		}
	}

	public override void ApplyWorldInfo() {
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				// Show a tree if it exists and has wood
				trees[x, z].Value.SetActive(worldInfo.worldTileInfo[x, z].hasTree &&
				                            trees[x, z].Key.wood > 0);
				// Change to stump model when an agent starts to collect wood
				if(trees[x, z].Key.turnToStump) {
					trees[x, z] = new KeyValuePair<Tree,GameObject>(
						trees[x, z].Key,
						(GameObject) Instantiate(stumpModel, worldXZToVec3(x, z), Quaternion.identity));
					trees[x, z].Key.turnToStump = false;
				}
			}
		}
	}
}
