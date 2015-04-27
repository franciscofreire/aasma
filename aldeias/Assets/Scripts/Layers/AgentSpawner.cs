using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AgentSpawner : Layer {
	public GameObject habitantModel, warriorModel, animalModel;

	public IDictionary<Habitant, GameObject> list_habitants = 
		new Dictionary<Habitant, GameObject>();
	public IDictionary<Animal, GameObject> list_animals = 
		new Dictionary<Animal, GameObject>();

	//WorldInfo events that AgentSpawner would like to listen to:
	//   WorldChanged
	//      NewAgent
	//         NewHabitant - to create and add a representation of the new habitant
	//         NewAnimal   - to create and add a representation of the new animal
	//      KillAgent
	//         KillHabitant- to remove the representation of the dead habitant
	//         KillAnimal  - to remove the representation of the dead animal
	//      AgentMoved - to move the representation of the agent that moved
	//WorldInfo events that might be useful:
	//   WorldCreated - to initialize information that doesn't change

	public override void CreateObjects() {
		foreach (Habitant h in worldInfo.AllHabitants) {
			GameObject agentModel = (GameObject) Instantiate(
				habitantModel,
				AgentPosToVec3(h.pos),
				Quaternion.identity);
			agentModel.transform.parent = this.transform;
			agentModel.SetActive(true);

			Transform wood = agentModel.transform.Find("Wood");
			wood.GetComponent<Renderer>().enabled = false;

			list_habitants.Add(h, agentModel);
		}

		foreach (Animal a in worldInfo.AllAnimals) {
			GameObject agentModel = (GameObject) Instantiate(
				animalModel,
				AgentPosToVec3(a.pos),
				Quaternion.identity);
			agentModel.transform.parent = this.transform;
			agentModel.SetActive(true);

			list_animals.Add(a, agentModel);
		}
	}


	public override void ApplyWorldInfo() {
		//Remove habitants no longer present in the worldInfo
		List<KeyValuePair<Habitant, GameObject>> hsToRemove=new List<KeyValuePair<Habitant, GameObject>>();
		foreach(KeyValuePair<Habitant,GameObject> ourH in list_habitants) {
			bool ourHPresent = false;
			foreach(Tribe t in worldInfo.tribes) {
				foreach(Habitant h in t.habitants) {
					if(h.Equals(ourH.Key)){
						ourHPresent = true;
						break;
					}
				}
			}
			if(!ourHPresent) {
				hsToRemove.Add(ourH);
			}
		}
		foreach(var hGo in hsToRemove) {
			list_habitants.Remove (hGo);
		}

		//Remove animals no longer present in the worldInfo
		List<KeyValuePair<Animal, GameObject>> asToRemove=new List<KeyValuePair<Animal, GameObject>>();
		foreach(var ourA in list_animals) {
			bool ourAPresent = false;
			foreach(WorldInfo.Habitat hh in worldInfo.habitats) {
				foreach(Animal a in hh.animals) {
					if(a.Equals(ourA.Key)){
						ourAPresent = true;
						break;
					} 
				}
			}
			if(!ourAPresent) {
				asToRemove.Add(ourA);
			}
		}
		foreach(var aGo in asToRemove) {
			list_animals.Remove (aGo);
		}

		//TODO: add animals and habitants that appeared

		foreach (KeyValuePair<Habitant,GameObject> kvp in list_habitants) {
			Habitant h = kvp.Key;
			GameObject g = kvp.Value;
			g.transform.localPosition = AgentPosToVec3(h.pos);
			g.transform.localRotation = h.orientation.ToQuaternion();
            if (h.CarryingWood) {
                Transform wood = g.transform.Find("Wood");
                wood.GetComponent<Renderer>().enabled = true;
            }
		}
		foreach (KeyValuePair<Animal,GameObject> kvp in list_animals) {
			Agent a = kvp.Key;
			GameObject g = kvp.Value;
            g.transform.localPosition = AgentPosToVec3(a.pos);
            if (a.Alive) {
                g.transform.localRotation = a.orientation.ToQuaternion();
            } else {
                Vector3 pos = g.transform.localPosition;
                pos.y = 1;
                g.transform.localPosition = pos;
                g.transform.localRotation = a.orientation.ToQuaternionInX();
            }

		}
	}
}