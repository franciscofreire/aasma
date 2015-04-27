using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: Reflect the new Tree design. Trees can be Alive, Cutdown or Depleted.
//IDEA: The Tree's WoodQuantity can be used to change it's size.
public class TreeLayer : Layer {
	public GameObject treeModel, stumpModel;
	
	private KeyValuePair<Tree,GameObject>[,] trees;

	public override void CreateObjects() {
		trees = new KeyValuePair<Tree,GameObject>[worldInfo.xSize, worldInfo.zSize];
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				// Add tree to WorldInfo
				WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(x,z);
				if (t.HasTree) {
					// Create tree model and save tree
					trees[x, z] = new KeyValuePair<Tree,GameObject>(
						t.Tree,
						(GameObject) Instantiate(treeModel, WorldXZToVec3(x, z), Quaternion.identity));
					trees[x, z].Value.transform.parent = this.transform;
				}
			}
		}
	}

	public override void ApplyWorldInfo() {
		for(int x=0; x<worldInfo.xSize; x++) {
			for(int z=0; z<worldInfo.zSize; z++) {
				WorldTileInfo t = worldInfo.worldTiles.WorldTileInfoAtCoord(x, z);
				if(t.HasTree) {
					// Remove depleted tree
					if(!t.Tree.HasWood) {
						Destroy(trees[x, z].Value);
					}

					// Change to stump model when an agent starts to collect wood
					if(!t.Tree.Alive) {
						Destroy(trees[x, z].Value);
						trees[x, z] = new KeyValuePair<Tree,GameObject>(
							t.Tree,
							(GameObject) Instantiate(stumpModel, WorldXZToVec3(x, z), Quaternion.identity));
						trees[x, z].Value.transform.parent = this.transform;
					}
				}
			}
		}
	}
}
