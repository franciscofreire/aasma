using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: Reflect the new Tree design. Trees can be Alive, Cutdown or Depleted.
//IDEA: The Tree's WoodQuantity can be used to change it's size.
public class TreeLayer : Layer {
	public GameObject treeModel, stumpModel;
	
	private IDictionary<Tree, GameObject> treesGameObjects = 
        new Dictionary<Tree, GameObject>();

	public override void CreateObjects() {
        // A tree is identified by it's position in the world.
        // TreeLayer will use this information to know which tree will be stump'd
        worldInfo.AddTreeDiedListener((Vector2I pos)=>{
            TurnToStump(pos);
        });

		foreach (Tree t in worldInfo.AllTrees) {
			GameObject g = (GameObject) Instantiate(treeModel, TileToVec3(t.Pos), Quaternion.identity);
			g.transform.parent = this.transform;
			treesGameObjects.Add(t, g);
		}
	}

    public override void ApplyWorldInfo() {
        // Remove depleted trees
		foreach (Tree t in worldInfo.AllTrees) {
			if (!t.HasWood) {
				Destroy(treesGameObjects[t]);
			}
		}
	}

    public void TurnToStump(Vector2I pos) {
        Tree t = worldInfo.worldTiles.WorldTileInfoAtCoord(pos).Tree;

        // Change to stump model when an agent starts to collect wood
        Destroy(treesGameObjects[t]);
        treesGameObjects[t] = (GameObject) Instantiate(stumpModel, TileToVec3(pos), Quaternion.identity);
        treesGameObjects[t].transform.parent = this.transform;
    }
}