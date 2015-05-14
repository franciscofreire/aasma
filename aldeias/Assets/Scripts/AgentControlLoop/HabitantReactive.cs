using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HabitantReactive : AgentImplementation {
    private Habitant habitant;

    private Action RunAwayOrWalkRandomly() {

        Vector2 oppositePos = habitant.pos + 
            habitant.orientation.LeftOrientation().LeftOrientation().ToVector2();
        Vector2I tileCoordOppositePos = CoordConvertions.AgentPosToTile(oppositePos);
        if(habitant.worldInfo.isInsideWorld(tileCoordOppositePos)) {
            return new Walk(habitant, tileCoordOppositePos);
        } else {
            return Action.WalkRandomly(habitant);
        }
    }

    private Action RunAwayOrWalkRandomlyOrEat() {
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
            return Action.WalkRandomly(habitant);
        }
    }

    public void doAction() {
        createAction().apply();
    }

    public Action createAction() {
        Vector2I target;
        if ((EnemyInAdjacentPos(out target) || AnimalInAdjacentPos(out target)) && LowEnergy()) {
            return RunAwayOrWalkRandomly();
        }
        else if (EnemyInAdjacentPos(out target) || AnimalInAdjacentPos(out target)) {
            Logger.Log("Attacker pos: " + habitant.pos.x + "," + habitant.pos.y, Logger.VERBOSITY.AGENTS);
            return new Attack(habitant, target);
        }
        else if (CanCarryWeight(Animal.FoodTearQuantity.Weight) && FoodInAdjacentPos(out target)) {
            return new PickupFood(habitant, target);
        }
        else if (CanCarryWeight(Tree.WoodChopQuantity.Weight) && CutDownTreeWithWoodInFront()) {
            return new ChopTree(habitant, sensorData.FrontCell);
        }
        else if (CanCarryWeight(Tree.WoodChopQuantity.Weight) && AliveTreeInFront()) {
            return new CutTree(habitant, sensorData.FrontCell);
        }
        else if (CarryingFood && MeetingPointInFront()) {
            return new DropFood(habitant, sensorData.FrontCell);
        }
        else if (CarryingWood && MeetingPointInFront()) {
            return new DropTree(habitant, sensorData.FrontCell);
        }
        else if (UnclaimedTerritoryInAdjacentPos(out target)) {
            return new PlaceFlag(habitant, target);
        }
        else if (EnemyTerritoryInAdjacentPos(out target)) {
            return new PlaceFlag(habitant, target);
        }
        else if (LowEnergy() &&
                 EatCarriedFood.IsEnoughFood(carriedFood)) {
            return new EatCarriedFood(habitant);
        }
        else if(LowEnergy() && 
                IsInTribeTerritory() && 
                EatInTribe.IsEnoughFood(tribe.FoodStock)) {
            return new EatInTribe(habitant,CoordConvertions.AgentPosToTile(pos));
        }
        else if((AnimalsInFrontPositions() || FoodInFrontPositions() ||
                EnemiesInFrontPositions() || TreesInFrontPositions()) &&
                (!AliveTreeInFront() && !DeadTreeInFront())){
            return Action.WalkFront(habitant);
        }
        return Action.WalkRandomly(habitant);
    }
    
    public HabitantReactive(Habitant habitant) {
        this.habitant = habitant;
    }


    //#################################################################################################

    private SensorData SensorData {
        get {
            return habitant.sensorData;
        }
    }

    private SensorData.TreeInfo? TreeInfoAtCoord(Vector2I coord) {
        return SensorData.CurrentVisionInfo.VisibleTrees.Where (ti=>ti.Coord==coord).FirstOrDefault(null);
    }

    private IEnumerable<SensorData.HabitantInfo> VisibleEnemies {
        get {
            var myTribe = SensorData.CurrentSelfInfo.Tribe;
            return SensorData.CurrentVisionInfo.VisibleHabitants.Where(hi=>hi.Tribe!=myTribe);
        }
    }

    //#################################################################################################

    private Vector2I FrontCell {
        get {
            var selfInfo = SensorData.CurrentSelfInfo;
            return selfInfo.Coord + selfInfo.Orientation.ToVector2I();
        }
    }

    private Vector2I RightCell {
        get {
            var selfInfo = SensorData.CurrentSelfInfo;
            return selfInfo.Coord + selfInfo.Orientation.RightOrientation().ToVector2I();
        }
    }

    private Vector2I LeftCell {
        get {
            var selfInfo = SensorData.CurrentSelfInfo;
            return selfInfo.Coord + selfInfo.Orientation.LeftOrientation().ToVector2I();
        }
    }

    //#################################################################################################
    /*
    public bool closeToTribe() {
        IList<Vector2I> result = new List<Vector2I>();
        IList<Vector2I> cellsInRadius = worldInfo.nearbyFreeCellsInRadius(CoordConvertions.AgentPosToTile(pos), 3);
        foreach (Vector2I candidate in cellsInRadius) {
            if (worldInfo.worldTiles.WorldTileInfoAtCoord(candidate)
                .tribeTerritory.Flag.Value.Tribe.id.Equals(this.tribe.id))
                return true;
        }
        return false;
    }
    */
    public bool AliveTree(Vector2I cell) {
        var treeInfo = TreeInfoAtCoord(cell);
        return treeInfo.HasValue && treeInfo.Value.IsAlive;
    }
    
    public bool DeadTree(Vector2I cell) {
        var treeInfo = TreeInfoAtCoord(cell);
        return treeInfo.HasValue && !treeInfo.Value.IsAlive;
    }
    
    public bool CutDownTreeWithWood(Vector2I cell) {
        var treeInfo = TreeInfoAtCoord(cell);
        return treeInfo.HasValue && !treeInfo.Value.IsAlive && treeInfo.Value.Wood!=WoodQuantity.Zero;
    }
    
    public bool DepletedTree(Vector2I cell) {
        var treeInfo = TreeInfoAtCoord(cell);
        return treeInfo.HasValue && !treeInfo.Value.IsAlive && treeInfo.Value.Wood==WoodQuantity.Zero;
    }

    public bool EnemyInAdjacentPos(out Vector2I target) {
        target = new Vector2I(-1,-1);
        Vector2I frontCell = FrontCell;
        Vector2I rightCell = RightCell;
        Vector2I leftCell = LeftCell;
        foreach(SensorData.HabitantInfo enemy in VisibleEnemies) {
            Vector2I enemyPos = enemy.Coord;
            if(enemyPos == frontCell || enemyPos == rightCell || enemyPos == leftCell) {
                target = enemyPos;
                return true;
            } 
        }
        return false;
    }
    
    public bool AnimalInAdjacentPos(out Vector2I target) {
        target = new Vector2I(-1,-1);
        Vector2I frontCell = FrontCell;
        Vector2I rightCell = RightCell;
        Vector2I leftCell = LeftCell;
        foreach(SensorData.AnimalInfo animal in SensorData.CurrentVisionInfo.VisibleAnimals) {
            Vector2I animalPos = animal.Coord;
            if(animalPos == frontCell || animalPos == rightCell || animalPos == leftCell) {
                target = animalPos;
                return true;
            } 
        }
        return false;
    }
    
    public bool UnclaimedTerritoryInAdjacentPos(out Vector2I target) {
        if(UnclaimedTerritoryInPos(FrontCell)) {
            target = FrontCell;
            return true;
        }
        if(UnclaimedTerritoryInPos(LeftCell)) {
            target = LeftCell;
            return true;
        }
        if(UnclaimedTerritoryInPos(RightCell)) {
            target = RightCell;
            return true;
        }
        target = Vector2I.INVALID;
        return false;
    }
    
    public bool EnemyTerritoryInAdjacentPos(out Vector2I target) {
        if(EnemyTerritoryInPos(FrontCell)) {
            target = FrontCell;
            return true;
        }
        if(EnemyTerritoryInPos(LeftCell)) {
            target = LeftCell;
            return true;
        }
        if(EnemyTerritoryInPos(RightCell)) {
            target = RightCell;
            return true;
        }
        target = new Vector2I(-1,-1);
        return false;
    }
    
    public bool EnemyTerritoryInPos(Vector2I pos) {
        return worldInfo.isInsideWorld(pos) // Valid cell
            && worldInfo.worldTiles.WorldTileInfoAtCoord(pos)
                .tribeTerritory.IsClaimed // Occupied cell
                && !worldInfo.worldTiles.WorldTileInfoAtCoord(pos)
                .tribeTerritory.Flag.Value.Tribe.id.Equals(tribe.id) // Cell has an enemy flag
                && tribe.FlagMachine.CanMakeFlag(); // At least one flag available in tribe
    }
    
    public bool UnclaimedTerritoryInPos(Vector2I pos) {
        return worldInfo.isInsideWorld(pos) // Valid cell
            && !worldInfo.worldTiles.WorldTileInfoAtCoord(pos)
                .tribeTerritory.IsClaimed // Unoccupied cell
                && tribe.FlagMachine.CanMakeFlag(); // At least one flag available in tribe
    }
    
    public bool UnclaimedTerritoryAtLeft() {
        return worldInfo.isInsideWorld(SensorData.LeftCell) // Valid cell
            && !worldInfo.worldTiles.WorldTileInfoAtCoord(SensorData.LeftCell)
                .tribeTerritory.IsClaimed // Unoccupied cell
                && tribe.FlagMachine.CanMakeFlag(); // At least one flag available in tribe
    }
    public bool UnclaimedTerritoryAtRight() {
        return worldInfo.isInsideWorld(SensorData.RightCell) // Valid cell
            && !worldInfo.worldTiles.WorldTileInfoAtCoord(SensorData.RightCell)
                .tribeTerritory.IsClaimed // Unoccupied cell
                && tribe.FlagMachine.CanMakeFlag(); // At least one flag available in tribe
    }
    
    public bool AnimalsInFrontPositions() {
        foreach(Animal a in SensorData.Animals) {
            Vector2I animalPos = CoordConvertions.AgentPosToTile(a.pos);
            foreach(Vector2I sensorPos in SensorData.FarAwayCells) {
                if(animalPos == sensorPos) {
                    return true;
                }
            }
        }
        return false;
    }
    
    public bool EnemiesInFrontPositions() {
        foreach(Habitant h in SensorData.Enemies) {
            Vector2I enemyPos = CoordConvertions.AgentPosToTile(h.pos);
            foreach(Vector2I sensorPos in SensorData.FarAwayCells) {
                if(enemyPos == sensorPos) {
                    return true;
                }
            }
        }      
        return false;
    }
    
    public bool TreesInFrontPositions() {
        foreach(Tree t in SensorData.Trees) {
            foreach(Vector2I sensorPos in SensorData.FarAwayCells) {
                if(t.Pos == sensorPos) {
                    return true;
                }
            }
        }
        return false;
    }
    
    public bool FoodInFrontPositions() {
        foreach(Animal a in SensorData.Food) {
            Vector2I animalPos = CoordConvertions.AgentPosToTile(a.pos);
            foreach(Vector2I sensorPos in SensorData.FarAwayCells) {
                if(animalPos == sensorPos) {
                    return true;
                }
            }
        }
        return false;
    }
    
    public bool IsInTribeTerritory() {
        Flag? flag = 
            worldInfo.worldTiles.WorldTileInfoAtCoord(
                CoordConvertions.AgentPosToTile(this.pos)).tribeTerritory.Flag;
        return flag.HasValue && flag.Value.Tribe.Equals(this.tribe);
    }

    private static Energy CRITICAL_ENERGY_LEVEL = new Energy(50);
    public bool LowEnergy() {
        return this.energy <= CRITICAL_ENERGY_LEVEL;
    }
    
    public bool FoodInAdjacentPos(out Vector2I target) {
        target = new Vector2I(-1,-1);
        Vector2I frontCell = SensorData.FrontCell;
        Vector2I rightCell = SensorData.RightCell;
        Vector2I leftCell = SensorData.LeftCell;
        foreach(Animal animal in SensorData.Food) {
            Vector2I animalPos = CoordConvertions.AgentPosToTile(animal.pos);
            if(animalPos == frontCell || animalPos == rightCell || animalPos == leftCell) {
                target = animalPos;
                return true;
            } 
        }
        return false;
    }
    
    public bool MeetingPointInFront() {
        return tribe.meetingPoint.IsInMeetingPoint(SensorData.FrontCell);
    }
}