using UnityEngine;

public class HabitantReactive : AgentImplementation {
    private Habitant habitant;

    private Action WalkRandomly() {
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

    private Action WalkFront() {
        return new Walk(habitant, habitant.sensorData.FrontCell);
    }

    private Action RunAwayOrwalkRandomly() {
        Vector2 oppositePos = habitant.pos + 
            habitant.orientation.LeftOrientation().LeftOrientation().ToVector2();
        Vector2I tileCoordOppositePos = CoordConvertions.AgentPosToTile(oppositePos);
        if(habitant.worldInfo.isInsideWorld(tileCoordOppositePos)) {
            return new Walk(habitant, tileCoordOppositePos);
        } else {
            return WalkRandomly();
        }
    }

    private Action RunAwayOrwalkRandomlyOrEat() {
        Vector2 oppositePos = habitant.pos + 
            habitant.orientation.LeftOrientation().LeftOrientation().ToVector2();
        Vector2I tileCoordOppositePos = CoordConvertions.AgentPosToTile(oppositePos);
        CryptoRandom rnd = new CryptoRandom();
        if(rnd.Next (10) <= 5) {
            return new EatInTribe(habitant,CoordConvertions.AgentPosToTile(habitant.pos));
        }
        if(habitant.worldInfo.isInsideWorld(tileCoordOppositePos)) {
            return new Walk(habitant, tileCoordOppositePos);
        } else {
            return WalkRandomly();
        }
    }

    public Action doAction() {
        Vector2I target;
        if ((habitant.EnemyInAdjacentPos(out target) || habitant.AnimalInAdjacentPos(out target)) && habitant.LowEnergy()) {
            return RunAwayOrwalkRandomlyOrEat();
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
        else if(habitant.LowEnergy() && 
                habitant.IsInTribeTerritory() && 
                habitant.TribeHasFood() ) {
            return new EatInTribe(habitant,CoordConvertions.AgentPosToTile(habitant.pos));
        }
        else if((habitant.AnimalsInFrontPositions() || habitant.FoodInFrontPositions() ||
                habitant.EnemiesInFrontPositions() || habitant.TreesInFrontPositions()) &&
                (!habitant.AliveTreeInFront() && !habitant.DeadTreeInFront())){
            return WalkFront();
        }
        return WalkRandomly();
    }
    
    public HabitantReactive(Habitant habitant) {
        this.habitant = habitant;
    }
}