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

    private Action RunAwayOrWalkRandomly() {
        Vector2 oppositePos = habitant.pos + 
            habitant.orientation.LeftOrientation().LeftOrientation().ToVector2();
        Vector2I tileCoordOppositePos = CoordConvertions.AgentPosToTile(oppositePos);
        if(habitant.worldInfo.isInsideWorld(tileCoordOppositePos)) {
            return new Walk(habitant, tileCoordOppositePos);
        } else {
            return walkRandomly();
        }
    }

    public Action doAction() {
        Vector2I target;
        if ((habitant.EnemyInAdjacentPos(out target) || habitant.AnimalInAdjacentPos(out target)) && habitant.LowEnergy()) {
            return RunAwayOrWalkRandomly();
        }
        else if (habitant.EnemyInAdjacentPos(out target) || habitant.AnimalInAdjacentPos(out target)) {
            Logger.Log("Attacker pos: " + habitant.pos.x + "," + habitant.pos.y, Logger.VERBOSITY.AGENTS);
            return new Attack(habitant, target);
        }
        else if (habitant.FoodInAdjacentPos(out target) && !habitant.CarryingResources()) {
            return new PickupFood(habitant, target);
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
        else if (habitant.UnclaimedTerritoryInAdjacentPos(out target)) {
            return new PlaceFlag(habitant, target);
        }
        else if (habitant.EnemyTerritoryInAdjacentPos(out target)) {
            return new PlaceFlag(habitant, target);
        }
        return walkRandomly();
    }
    
    public HabitantReactive(Habitant habitant) {
        this.habitant = habitant;
    }
}