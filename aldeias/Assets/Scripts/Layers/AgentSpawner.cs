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

	public override void CreateObjects() {//TODO: Add habitants and add animals that exist in the world.
		foreach (Habitant h in 
		         worldInfo.tribes //List of tribes
		         	.ConvertAll(t=>t.habitants.AsEnumerable()) //Lists of habitants
		         	.Aggregate((hs1,hs2)=>hs1.Concat(hs2))) { //List of habitants
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

		foreach (Animal a in
		         worldInfo.habitats
		         .ConvertAll(h=>h.animals.AsEnumerable())
		         .Aggregate((as1,as2)=>as1.Concat(as2))) {
			GameObject agentModel = (GameObject) Instantiate(
				animalModel,
				AgentPosToVec3(a.pos),
				Quaternion.identity);
			agentModel.transform.parent = this.transform;
			agentModel.SetActive(true);

			list_animals.Add(a, agentModel);
		}

		/*
		// Create habitants
		// Find the meeting point of a tribe
		List<Tribe> tribes = worldInfo.tribes;
		int num_agents = 4;
		foreach (Tribe t in tribes) {
			MeetingPoint mp = t.meetingPoint;
			Vector2I cp = mp.center;
			int mp_bound = WorldInfo.MEETING_POINT_SIDE / 2; // Limit for agent creation positions
			for (int i = -mp_bound; i <= mp_bound; i++) {
				for (int j = -mp_bound; j <= mp_bound; j++) {
					if (num_agents-- > 0) {
						int x = cp.x + i;
						int z = cp.y + j;
						Vector2I tileCoord = new Vector2I(x,z);

						// Update WordTileInfo
						worldInfo.WorldTileInfoAtCoord(tileCoord).hasAgent = true;

						// Create a model for the new agent
						GameObject agentModel = (GameObject) Instantiate(
							habitantModel,
							worldXZToVec3(x, z),
							Quaternion.identity);
						agentModel.transform.parent = this.transform;
						agentModel.SetActive(true);

                        // Hide objects not collected by habitant
                        Transform wood = agentModel.transform.Find("Wood");
                        wood.GetComponent<Renderer>().enabled = false;

						// Create the habitant and add him to the right tribe
						Habitant habitant = new Habitant(worldInfo,	new Vector2(x, z), t, 1);
						worldInfo.addAgentToTribe(t, habitant);

						// Save this agent
						list_habitants.Add(new KeyValuePair<Habitant, GameObject>(habitant, agentModel));
					} else
						break;
				}
			}
			num_agents = 4;
		}
		*/
		/*
		// Create animals
		// Find the first cell (corner) of a habitat
		List<WorldInfo.Habitat> habitats = worldInfo.habitats;
		int num_animals = 4;
		foreach (WorldInfo.Habitat h in habitats) {
			Vector2 pos = h.corner_pos;
			int posx = (int) pos[0];
			int posz = (int) pos[1]; 
			for(int x = posx; x < posx + WorldInfo.HABITAT_SIDE; x++) {
				for(int z = posz; z > posz - WorldInfo.HABITAT_SIDE; z--) {
					Vector2I tileCoord = new Vector2I(x,z);
					if (num_animals-- > 0) {
						// Update WordTileInfo
						worldInfo.WorldTileInfoAtCoord(tileCoord).hasAgent = true;

						// Create a model for the new agent
						GameObject agentModel = (GameObject) Instantiate(
							animalModel,
							worldXZToVec3(x, z),
							Quaternion.identity);
						agentModel.transform.parent = this.transform;
						agentModel.SetActive(true);

						// Create the animal and add him to the right habitat
						Animal animal = new Animal(worldInfo, new Vector2(x, z));
						worldInfo.addAgentToHabitat(h, animal);
						
						// Save this agent
						list_animals.Add(new KeyValuePair<Animal, GameObject>(animal, agentModel));
					} else
						break;
				}
			}
			num_animals = 4;
		}
		*/
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