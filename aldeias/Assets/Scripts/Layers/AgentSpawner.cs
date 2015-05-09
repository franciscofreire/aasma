using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class AgentSpawner : Layer {
	public GameObject habitantModel, warriorModel, tombstoneModel,
                      animalModel, foodModel;

    public HabitantQuantitiesRepresentation HabitantPrefab;
    public Material HabitantMaterialPrefab;

	public IDictionary<Habitant, GameObject> list_habitants = 
		new Dictionary<Habitant, GameObject>();
    public IDictionary<Habitant, HabitantQuantitiesRepresentation> habResReps = 
        new Dictionary<Habitant, HabitantQuantitiesRepresentation>();
	public IDictionary<Animal, GameObject> list_animals = 
		new Dictionary<Animal, GameObject>();

    public IDictionary<string, Material> list_agent_materials =
        new Dictionary<string, Material>();


	public override void CreateObjects() {
        // An animal is identified by it's position in the world (not ideal, but...)
        // AgentSpawner will use this information to know which animal will be turned to food
        worldInfo.AddAnimalDiedListener((Animal a)=>{
            TurnToFood(a);
        });

        worldInfo.AddHabitantDiedListener((Habitant h)=>{
            TurnToTombstone(h);
            RemoveHabitantResRep(h);
        });
        
        worldInfo.AddHabitantDroppedResourceListener((Habitant h)=>{
        }); // Unused for now (using bars for state)

        // Assign tribe colors to materials
        Material mat_tribe_A  = new Material(HabitantMaterialPrefab);
        Material mat_tribe_B  = new Material(HabitantMaterialPrefab);
        mat_tribe_A.color = Color.blue;
        mat_tribe_B.color = Color.red;
        list_agent_materials.Add("Blue", mat_tribe_A);
        list_agent_materials.Add("Red",  mat_tribe_B);

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

            HabitantQuantitiesRepresentation habGameObj = ((GameObject)Instantiate(
                HabitantPrefab.gameObject,
                AgentPosToVec3(h.pos),
                Quaternion.identity)).GetComponent<HabitantQuantitiesRepresentation>();
            habGameObj.transform.parent = agentModel.transform;
            habGameObj.SetHabitantWithMaterial(h);
            habResReps.Add(h, habGameObj);
            
			list_habitants.Add(h, agentModel);
		}

		foreach (Animal a in worldInfo.AllAnimals) {
            CreateAnimalRepr(a);
		}
	}

    private void CreateAnimalRepr(Animal a) {
        GameObject agentModel = (GameObject) Instantiate(
            animalModel,
            AgentPosToVec3(a.pos),
            Quaternion.identity);
        agentModel.transform.parent = this.transform;
        agentModel.SetActive(true);
        
        list_animals.Add(a, agentModel);
    }

	public override void ApplyWorldInfo() {
        //TODO: add animals and habitants that appeared
        var newAnimals = worldInfo.AliveAnimals.Except(list_animals.Keys);
        foreach(var a in newAnimals) {
            CreateAnimalRepr(a);
        }

        List<Animal> keys_animals = new List<Animal>(list_animals.Keys);
        foreach (Animal a in keys_animals) {
            GameObject g = list_animals[a];
            g.transform.localPosition = AgentPosToVec3(a.pos);
            if (a.Alive) {
                g.transform.localRotation = a.orientation.ToQuaternion();
            } 
            else if (!a.HasFood) { // Remove depleted food
                Destroy(list_animals[a]);
                list_animals.Remove(a);
                a.removeFromWorldInfo();
            }
        }
        
        List<Habitant> keys_habitants = new List<Habitant>(list_habitants.Keys);
		foreach (Habitant h in keys_habitants) {
			GameObject g = list_habitants[h];
			g.transform.localPosition = AgentPosToVec3(h.pos);
			g.transform.localRotation = h.orientation.ToQuaternion();
            if(h.Alive) {
                habResReps[h].UpdateModels();
            }
            else if(h.OldTombstone) { // Remove old tombstone
                Destroy(list_habitants[h]);
                list_habitants.Remove(h);
                h.removeFromWorldInfo();
            }
		}
	}
    
    public void TurnToFood(Animal a) {
        // Change to food model when an agent starts to collect food
        try {
            Destroy(list_animals[a]);
            list_animals[a] = (GameObject) Instantiate(
                foodModel,
                TileToVec3(CoordConvertions.AgentPosToTile(a.pos)),
                Quaternion.identity);
            list_animals[a].transform.parent = this.transform;
        }
        catch (System.ArgumentNullException e) {
            Debug.Log("[ERROR] @TurnToFood: " + e);
        }
    }
    
    public void TurnToTombstone(Habitant h) {
        // Change to tombstone model when habitant is dead
        try {
            Destroy(list_habitants[h]);
            list_habitants[h] = (GameObject) Instantiate(
                tombstoneModel,
                TileToVec3(CoordConvertions.AgentPosToTile(h.pos)),
                Quaternion.identity);
            list_habitants[h].transform.parent = this.transform;
        }
        catch (System.ArgumentNullException e) {
            Debug.Log("[ERROR] @TurnToTombstone: " + e);
        }
    }

    public void RemoveHabitantResRep(Habitant h) {
        Destroy(habResReps[h].gameObject);
        habResReps.Remove(h);
    }
}