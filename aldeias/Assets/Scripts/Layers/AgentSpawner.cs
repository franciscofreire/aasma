using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentSpawner : Layer {
	public GameObject habitantModel, warriorModel, animalModel;

	public IList<KeyValuePair<Habitant,GameObject>> list_habitants =
		new List<KeyValuePair<Habitant,GameObject>>();
	public IList<KeyValuePair<Animal,GameObject>> list_animals =
		new List<KeyValuePair<Animal,GameObject>>();

	public override void CreateObjects() {
		// Create habitants
		// Find the meeting point of a tribe
		List<WorldInfo.Tribe> tribes = worldInfo.tribes;
		int num_agents = 4;
		foreach (WorldInfo.Tribe t in tribes) {
			WorldInfo.MeetingPoint mp = t.meetingPoint;
			Vector2 cp = mp.centralPoint;
			int mp_bound = WorldInfo.MEETING_POINT_WIDTH / 2; // Limit for agent creation positions
			for (int i = -mp_bound; i <= mp_bound; i++) {
				for (int j = -mp_bound; j <= mp_bound; j++) {
					if (num_agents-- > 0) {
						int x = (int)cp[0] + i;
						int z = (int)cp[1] + j;

						// Update WordTileInfo
						worldInfo.worldTileInfo[x, z].hasAgent = true;

						// Create a model for the new agent
						GameObject agentModel = (GameObject) Instantiate(
							habitantModel,
							worldXZToVec3(x, z),
							Quaternion.identity);
						agentModel.transform.parent = this.transform;
						agentModel.SetActive(true);

						// Create the habitant and add him to the right tribe
						Habitant habitant = new Habitant(
							new Vector2(x, z),
							t,
							1);
						worldInfo.addAgentToTribe(t, habitant);

						// Save this agent
						list_habitants.Add(new KeyValuePair<Habitant, GameObject>(habitant, agentModel));
					} else
						break;
				}
			}
			num_agents = 4;
		}

		// Create animals
		// Find the first cell (corner) of a habitat
		List<WorldInfo.Habitat> habitats = worldInfo.habitats;
		int num_animals = 4;
		foreach (WorldInfo.Habitat h in habitats) {
			Vector2 pos = h.corner_pos;
			int posx = (int) pos[0];
			int posz = (int) pos[1]; 
			for(int x = posx; x < posx + WorldInfo.HABITAT_SIZE; x++) {
				for(int z = posz; z > posz - WorldInfo.HABITAT_SIZE; z--) {
					if (num_animals-- > 0) {
						// Update WordTileInfo
						worldInfo.worldTileInfo[x, z].hasAgent = true;

						// Create a model for the new agent
						GameObject agentModel = (GameObject) Instantiate(
							animalModel,
							worldXZToVec3(x, z),
							Quaternion.identity);
						agentModel.transform.parent = this.transform;
						agentModel.SetActive(true);

						// Create the animal and add him to the right habitat
						Animal animal = new Animal(new Vector2(x, z));
						worldInfo.addAgentToHabitat(h, animal);
						
						// Save this agent
						list_animals.Add(new KeyValuePair<Animal, GameObject>(animal, agentModel));
					} else
						break;
				}
			}
			num_animals = 4;
		}
	}
	
	public void move(Agent a, GameObject g, Vector2 target) {
		a.pos[0] = (int) target[0];
		a.pos[1] = (int) target[1];
		g.transform.localPosition = worldXZToVec3((int) target[0], (int) target[1]);
	}

	public override void ApplyWorldInfo() {

		foreach (KeyValuePair<Habitant,GameObject> kvp in list_habitants) {
			Agent a = kvp.Key;
			GameObject g = kvp.Value;
			IList<Vector2> cells = worldInfo.nearbyCells(a);
			System.Random rnd = new System.Random();
			int index = rnd.Next(cells.Count);
			Vector2 cell = cells[index];
			
			worldInfo.nearbyCells(a);
			move(a, g, cell);
		}
	}
}