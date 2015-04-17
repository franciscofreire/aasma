using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentSpawner : Layer {
	public const int MAX_AGENTS = 100;

	public GameObject habitantModel, warriorModel, animalModel;

	private GameObject agent;

	public override void CreateObjects() {

		// Create tribe agents
		List<WorldInfo.Tribe> tribes = worldInfo.tribes;
		int num_agents = 4;
		foreach (WorldInfo.Tribe t in tribes) {
			WorldInfo.MeetingPoint mp = t.meetingPoint;
			Vector2 cp = mp.centralPoint;
			for (int i = 0; i < WorldInfo.MEETING_POINT_WIDTH; i++) {
				for (int j = 0; j < WorldInfo.MEETING_POINT_WIDTH; j++) {
					if (num_agents-- > 0) {
						agent = (GameObject) Instantiate(
							habitantModel,
							worldXZToVec3((int)cp[0] + i,(int)cp[1] + j),
							Quaternion.identity);
						agent.transform.parent = this.transform;
						agent.SetActive(true);
						t.agents.Add(agent);
					} else
						break;
				}
			}
			num_agents = 4;
		}

		// Create animals
		List<WorldInfo.Habitat> habitats = worldInfo.habitats;
		int num_animals = 4;
		foreach (WorldInfo.Habitat h in habitats) {
			Vector2 pos = h.corner_pos;
			int posx = (int) pos[0];
			int posz = (int) pos[1]; 
			for(int x = posx; x < posx + WorldInfo.HABITAT_SIZE; x++) {
				for(int z = posz; z > posz - WorldInfo.HABITAT_SIZE; z--) {
					if (num_animals-- > 0) {
						agent = (GameObject) Instantiate(
							animalModel,
							worldXZToVec3(x, z),
							Quaternion.identity);
						agent.transform.parent = this.transform;
						agent.SetActive(true);
					} else
						break;
				}
			}
			num_animals = 4;
		}
	}

	public override void ApplyWorldInfo() {
	}
}