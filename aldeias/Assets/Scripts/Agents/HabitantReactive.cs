public class HabitantReactive : AgentImplementation {
    private Habitant habitant;

    public Action doAction() {
        if ((habitant.EnemyInFront() || habitant.AnimalInFront()) && habitant.LowEnergy()) {
            // Reactive agent: Flee randomly
            int fleeIndex = WorldRandom.Next(habitant.sensorData.Cells.Count);
            Vector2I fleeTarget = habitant.sensorData.Cells[fleeIndex];
            return new Walk(habitant, fleeTarget);
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
        
        // Reactive agent: Walk randomly
        int index = WorldRandom.Next(habitant.sensorData.AdjacentCells.Count);
        Vector2I target = habitant.sensorData.AdjacentCells[index];
        return new Walk(habitant, target);
    }
    
    public HabitantReactive(Habitant habitant) {
        this.habitant = habitant;
    }
}
