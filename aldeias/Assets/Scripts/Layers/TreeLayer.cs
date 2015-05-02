using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: Reflect the new Tree design. Trees can be Alive, Cutdown or Depleted.
//IDEA: The Tree's WoodQuantity can be used to change it's size.
public class TreeLayer : Layer {
	public GameObject treeModel, stumpModel;
	
	private IDictionary<Tree, GameObject> treesGameObjects=new Dictionary<Tree, GameObject>();

	public override void CreateObjects() {
		foreach (Tree t in worldInfo.AllTrees) {
			GameObject g = (GameObject) Instantiate(treeModel, WorldXZToVec3(t.Pos), Quaternion.identity);
			g.transform.parent = this.transform;
			treesGameObjects.Add(t, g);
		}
	}

	public override void ApplyWorldInfo() {
		foreach (Tree t in worldInfo.AllTrees) {
		
			// Remove depleted tree
			if(!t.HasWood) {
				Destroy(treesGameObjects[t]);
			}

			// Change to stump model when an agent starts to collect wood
			if(!t.Alive) {
				Destroy(treesGameObjects[t]);
				treesGameObjects[t] = (GameObject) Instantiate(stumpModel, WorldXZToVec3(t.Pos), Quaternion.identity);
				treesGameObjects[t].transform.parent = this.transform;
			}
		}
	}
}
