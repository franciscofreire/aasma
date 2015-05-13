using UnityEngine;

public class HabitantQuantitiesRepresentation : MonoBehaviour {
    //These GameObjects are children of the GameObject this Component is attached to.
    public GameObject FoodBarModel;
    public GameObject WoodBarModel;
    public GameObject EnergyBarModel;

    public Habitant Habitant;

    public int FoodCount;
    public int WoodCount;
    public int EnergyCount;

    public void SetHabitant(Habitant h) {
        Habitant = h;
    }

    public void UpdateRepresentation() {
        FoodCount = Habitant.carriedFood.Count;
        var relativeFood = Habitant.carriedFood.Count / 100f;
        FoodBarModel.transform.localScale = new Vector3(1,relativeFood,1);

        WoodCount = Habitant.carriedWood.Count;
        var relativeWood = Habitant.carriedWood.Count / 100f;
        WoodBarModel.transform.localScale = new Vector3(1,relativeWood,1);

        EnergyCount = Habitant.energy.Count;
        var relativeEnergy = Habitant.energy.Count / 100f;
        EnergyBarModel.transform.localScale = new Vector3(1,relativeEnergy,1);
    }
}