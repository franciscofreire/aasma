using UnityEngine;

public class HabitantReactive : AgentImplementation {
    private Habitant habitant;

    public void doAction() {
        createAction().apply();
    }

    public Action createAction() {
        Vector2I target;
        if ((habitant.EnemyInAdjacentPos(out target) || habitant.AnimalInAdjacentPos(out target)) && habitant.LowEnergy()) {
            return Action.RunAwayOrWalkRandomly(habitant);
        }
        else if (habitant.EnemyInAdjacentPos(out target) || habitant.AnimalInAdjacentPos(out target)) {
            Logger.Log("Attacker pos: " + habitant.pos.x + "," + habitant.pos.y, Logger.VERBOSITY.AGENTS);
            return new Attack(habitant, target);
        }
        else if (habitant.CanCarryWeight(Animal.FoodTearQuantity.Weight) && habitant.FoodInAdjacentPos(out target)) {
            return new PickupFood(habitant, target);
        }
        else if (habitant.CanCarryWeight(Tree.WoodChopQuantity.Weight) && habitant.StumpWithWoodInFront()) {
            return new ChopTree(habitant, habitant.sensorData.FrontCell);
        }
        else if (habitant.CanCarryWeight(Tree.WoodChopQuantity.Weight) && habitant.AliveTreeInFront()) {
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
        else if (habitant.LowEnergy() &&
                 EatCarriedFood.IsEnoughFood(habitant.carriedFood)) {
            return new EatCarriedFood(habitant);
        }
        else if(habitant.LowEnergy() && 
                habitant.IsInTribeTerritory() && 
                EatInTribe.IsEnoughFood(habitant.tribe.FoodStock)) {
            return new EatInTribe(habitant,CoordConvertions.AgentPosToTile(habitant.pos));
        }
        else if((habitant.AnimalsInFrontPositions() || habitant.FoodInFrontPositions() ||
                habitant.EnemiesInFrontPositions() || habitant.TreesInFrontPositions()) &&
                (!habitant.AliveTreeInFront() && !habitant.DeadTreeInFront())) {
            return Action.WalkFront(habitant);
        }
        return Action.WalkRandomly(habitant);
    }
    
    public HabitantReactive(Habitant habitant) {
        this.habitant = habitant;
    }
}