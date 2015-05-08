using UnityEngine;

public class HabitantResourceRepresentation : MonoBehaviour {
    //This MonoBehaviour is attached to the Habitant prefab of the AgentSpawner.
    //These GameObjects are children of the GameObject this Component is attached to.
    public GameObject FoodBarModel;
    public GameObject WoodBarModel;

    public Habitant Habitant;

    public void SetHabitantWithMaterial(Habitant h) {
        Habitant = h;
    }

    public void UpdateModels() {
        var relativeFood = Habitant.carriedFood.Count / 100f;
        FoodBarModel.transform.localScale = new Vector3(1,relativeFood,1);
        var relativeWood = Habitant.carriedWood.Count / 100f;
        WoodBarModel.transform.localScale = new Vector3(1,relativeWood,1);
    }
}