using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AgentSpawner : Layer {
	public GameObject habitantModel, warriorModel, animalModel, foodModel;

	public IDictionary<Habitant, GameObject> list_habitants = 
		new Dictionary<Habitant, GameObject>();
	public IDictionary<Animal, GameObject> list_animals = 
		new Dictionary<Animal, GameObject>();

    public IDictionary<string, Material> list_agent_materials =
        new Dictionary<string, Material>();

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
        // An animal is identified by it's position in the world (not ideal, but...)
        // AgentSpawner will use this information to know which animal will be turned to food
        worldInfo.AddAgentDiedListener((Vector2I pos)=>{
            TurnToFood(pos);
        });

        // Assign tribe colors to materials
        Material mat_habitant = habitantModel.transform.Find("Body").renderer.material;
        Material mat_tribe_A  = new Material(mat_habitant);
        Material mat_tribe_B  = new Material(mat_habitant);
        mat_tribe_A.color = Color.blue;
        mat_tribe_B.color = Color.red;
        list_agent_materials.Add("A", mat_tribe_A);
        list_agent_materials.Add("B", mat_tribe_B);

		foreach (Habitant h in worldInfo.AllHabitants) {
			GameObject agentModel = (GameObject) Instantiate(
				habitantModel,
				AgentPosToVec3(h.pos),
				Quaternion.identity);
			agentModel.transform.parent = this.transform;
			agentModel.SetActive(true);

            // Assign materials to habitants
            agentModel.transform.Find("Body").renderer.sharedMaterial =
                list_agent_materials[h.tribe.id];
            agentModel.transform.Find("Orientation").renderer.sharedMaterial =
                list_agent_materials[h.tribe.id];

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
        //TODO: add animals and habitants that appeared
        
        // Remove depleted food
        List<Animal> keys_animals = new List<Animal>(list_animals.Keys);
        foreach (Animal a in keys_animals) {
            GameObject g = list_animals[a];
            g.transform.localPosition = AgentPosToVec3(a.pos);
            if (a.Alive) {
                g.transform.localRotation = a.orientation.ToQuaternion();
            } 
            else if (!a.HasFood) {
                Destroy(list_animals[a]);
                list_animals.Remove(a);
            }
        }

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
	}
    
    public void TurnToFood(Vector2I pos) {
        Animal a = (Animal) worldInfo.worldTiles.WorldTileInfoAtCoord(pos).Agent;

        // Change to food model when an agent starts to collect food
        //GameObject g = list_animals[a];
        Destroy(list_animals[a]);
        list_animals[a] = (GameObject) Instantiate(foodModel, WorldXZToVec3(pos), Quaternion.identity);
        list_animals[a].transform.parent = this.transform;
    }
}