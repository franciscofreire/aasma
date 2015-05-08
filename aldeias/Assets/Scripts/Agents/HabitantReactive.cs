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
            // We don't have nearby free cells, so we do nothing
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
            Logger.Log("Attacker pos: " + habitant.pos.x + "," + habitant.pos.y, Logger.VERBOSITY.AGENTS);
            return new Attack(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.FoodInFront() && !habitant.CarryingResources()) {
            return new PickupFood(habitant, habitant.sensorData.FrontCell);
        }
        else if (!habitant.CarryingResources() && habitant.CutDownTreeWithWoodInFront()) {
            return new PickupTree(habitant, habitant.sensorData.FrontCell);
        }
        else if (!habitant.CarryingResources() && habitant.AliveTreeInFront()) {
            return new CutTree(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.CarryingFood && habitant.MeetingPointInFront()) {
            return new DropFood(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.CarryingWood && habitant.MeetingPointInFront()) {
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