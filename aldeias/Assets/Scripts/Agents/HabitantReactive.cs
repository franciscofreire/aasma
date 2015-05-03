using UnityEngine;

public class HabitantReactive : AgentImplementation {
    private Habitant habitant;

    private Action walkRandomly() {
        int index = WorldRandom.Next(habitant.sensorData.AdjacentCells.Count);
        Vector2I target;
        try {
            target = habitant.sensorData.AdjacentCells[index];
        }
        catch (System.Exception) {
            //Debug.Log("[ERROR] @AdjacentCells: " + e.ToString());
            // we don't have nearby free cells, so we do nothing
            // and stay at the same position
            target = new Vector2I(habitant.pos);
        }
        return new Walk(habitant, target);
    }

    public Action doAction() {
        if ((habitant.EnemyInFront() || habitant.AnimalInFront()) && habitant.LowEnergy()) {
            return walkRandomly();
        }
        else if (habitant.EnemyInFront() || habitant.AnimalInFront()) {
            return new Attack(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.FoodInFront() && !habitant.CarryingResources()) {
            return new PickupFood(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.CutDownTreeWithWoodInFront() && !habitant.CarryingResources()) {
            return new PickupTree(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.AliveTreeInFront() && !habitant.CarryingResources()) {
            return new CutTree(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.MeetingPointInFront() && habitant.CarryingFood) {
            return new DropFood(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.MeetingPointInFront() && habitant.CarryingWood) {
            return new DropTree(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.UnclaimedTerritoryInFront()) {
            return new PlaceFlag(habitant, habitant.sensorData.FrontCell);
        }

        return walkRandomly();
    }
    
    public HabitantReactive(Habitant habitant) {
        this.habitant = habitant;
    }
}