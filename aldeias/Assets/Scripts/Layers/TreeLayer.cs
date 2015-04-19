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
				// Add tree to WorldInfo
				Tree t = worldInfo.WorldTileInfoAtCoord(x, z).tree;

				// Create tree model and save tree
				trees[x, z] = new KeyValuePair<Tree,GameObject>(
					t,
					(GameObject) Instantiate(treeModel, worldXZToVec3(x, z), Quaternion.identity));
				trees[x, z].Value.transform.parent = this.transform;
				trees[x, z].Value.SetActive(false);
			}
		}
	}

	public override void ApplyWorldInfo() {
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				Tree t = worldInfo.WorldTileInfoAtCoord(x, z).tree;

				// Remove depleted tree
				if(t.wood == 0) {
					Destroy(trees[x, z].Value);
					t.isStump = false;
					t.hasTree = false;
				}

				// Change to stump model when an agent starts to collect wood
				if(t.turnToStump) {
					Destroy(trees[x, z].Value);
					trees[x, z] = new KeyValuePair<Tree,GameObject>(
						t,
						(GameObject) Instantiate(stumpModel, worldXZToVec3(x, z), Quaternion.identity));
					trees[x, z].Value.transform.parent = this.transform;

					t.turnToStump = false;
				}
				
				// Show tree if it exists
				trees[x, z].Value.SetActive(t.hasTree);
			}
		}
	}
}
