using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class Belief {
    private const int MAX_SIZE_SENSOR_DATA = 10;

    private bool isActive;
    private IList<Vector2I> relevantCells;
    private List<SensorData> previousSensorData;
    protected int timesToBeActive;

    public Belief() {
        this.relevantCells = new List<Vector2I>();
        this.previousSensorData = new List<SensorData>(MAX_SIZE_SENSOR_DATA);
        this.DisableBelief();
        this.timesToBeActive = 0;
    }

    public Belief(IList<Vector2I> relevantCells) {
        this.relevantCells = relevantCells;
    }
    
    public IList<Vector2I> RelevantCells {
        get { return this.relevantCells; }
        set { this.relevantCells = value; }
    }

    // It only adds if it does not exists
    public void addRelevantCell(Vector2I cell) {
        bool isPresent = false;
        foreach(var pos in RelevantCells) {
            if(pos == cell) {
                isPresent = true;
                break;
            }
        }
        if(!isPresent) {
            RelevantCells.Add (cell);
        }
    }

    public bool IsActive {
        get { return this.isActive; }
        set { this.isActive = value; }
    }

    public void AddSensorData(SensorData sensorData) {
        if(previousSensorData.Count == MAX_SIZE_SENSOR_DATA) {
            previousSensorData.RemoveAt(previousSensorData.Count - 1);
        }
        previousSensorData.Insert(0, sensorData);
    }

    /// <exception cref="SensorDataDoesNotExists">index given does not exists</exception>
    public SensorData GetSensorData(int index) {
        try {
            return previousSensorData[index];
        } catch (ArgumentOutOfRangeException) {
            throw new SensorDataDoesNotExists();
        }
    }

    public int PreviousSensorDataCount {
        get { return previousSensorData.Count; }
    }

    public void EnableBelief() {
        this.isActive = true;
    }
    public void EnableBelief(int count) {
        this.isActive = true;
        this.timesToBeActive = count;
    }
    // default disable
    public void DisableBelief() {
        this.isActive = false;
        this.previousSensorData.Clear();
        this.relevantCells.Clear();
        this.timesToBeActive = 0;
    }

    public void DisableBeliefKeepingRelevantCells() {
        this.isActive = false;
        this.previousSensorData.Clear();
        this.timesToBeActive = 0;
    }

    public virtual void UpdateBelief(Percept p) {
        //FIXME: I don't know the purpose of saving SensorDatas in the base class.
        // append sensorData to the beginning
        AddSensorData(p.SensorData);
    }

    public static void brf(Beliefs beliefs, Percept p) {
        foreach(var b in beliefs.AllBeliefs) {
            b.UpdateBelief (p);
        }
        p.Habitant.PendingMessages.Clear();//FIXME: Make sure we want to clear the message queue after each belief review.
    }
}

public class Beliefs {
    public WorldInfo WorldInfo { get; set; }

    public NearMeetingPoint NearMeetingPoint;
    public TribeIsBeingAttacked TribeIsBeingAttacked;
    public TribeHasLowFoodLevel TribeHasLowFoodLevel;
    public TribeHasFewFlags TribeHasFewFlags;
    public AnimalsAreNear AnimalsAreNear;
    public EnemiesAreNear EnemiesAreNear;
    public NearEnemyTribe NearEnemyTribe;
    public KnownWood KnownWood;
    public PickableFood DroppedFood;
    public PickableWood DroppedWood;
    public HabitantHasLowEnergy HabitantHasLowEnergy;
    public UnclaimedTerritoryIsNear UnclaimedTerritoryIsNear;
    public KnownObstacles KnownObstacles;
    public TribeTerritories TribeTerritories;
    public CellSeenOrders CellSeenOrders;

    public IEnumerable<Belief> AllBeliefs {
        get {
            yield return NearMeetingPoint;
            yield return TribeIsBeingAttacked;
            yield return TribeHasLowFoodLevel;
            yield return TribeHasFewFlags;
            yield return EnemiesAreNear;
            yield return AnimalsAreNear;
            yield return NearEnemyTribe;
            yield return KnownWood;
            yield return DroppedFood;
            yield return DroppedWood;
            yield return HabitantHasLowEnergy;
            yield return UnclaimedTerritoryIsNear;
            yield return KnownObstacles;
            yield return TribeTerritories;
            yield return CellSeenOrders;
        }
    }

    public Beliefs(Habitant h) {
        WorldInfo = h.worldInfo;

        NearMeetingPoint=new NearMeetingPoint();
        TribeIsBeingAttacked=new TribeIsBeingAttacked(h);
        TribeHasLowFoodLevel=new TribeHasLowFoodLevel();
        TribeHasFewFlags=new TribeHasFewFlags();
        AnimalsAreNear=new AnimalsAreNear();
        EnemiesAreNear=new EnemiesAreNear();
        NearEnemyTribe=new NearEnemyTribe();
        KnownWood=new KnownWood(h);
        DroppedFood=new PickableFood();
        DroppedWood=new PickableWood();
        HabitantHasLowEnergy=new HabitantHasLowEnergy();
        UnclaimedTerritoryIsNear=new UnclaimedTerritoryIsNear();
        KnownObstacles=new KnownObstacles(h);
        TribeTerritories=new TribeTerritories(h);
        CellSeenOrders=new CellSeenOrders(h);
    }
}

/* Conditions: 
 *  - MeetingPoint is on Agents' vision
 */  
public class NearMeetingPoint : Belief {
    // Belief remains active if the last sensor
    // satisfies the condition (Agent saw meeting point cells)
    // Found meeting point cells are saved in Relevant Cells
    // even when belief is inactive
    public override void UpdateBelief (Percept p) {
        foreach(var cell in p.SensorData.MeetingPointCells) {
            addRelevantCell(cell);
        }
    }

    public NearMeetingPoint() {
        EnableBelief ();
    }
}

public class TribeIsBeingAttacked : Belief {
    Habitant habitant;
    /* Conditions:
     *  - Habitant is near enemy and it is insideTribe
     *  - Tribe territory is decreasing
     */
    private bool ArePreconditionsSatisfied(SensorData sensorData) {
        return sensorData.AgentIsInsideTribe &&
            sensorData.Enemies.Count > 0;
    }
   
    private void InformOthers() {
        var message = new HabitantMessages.Messages.HabitantBeingAttacked(habitant, RelevantCells);
        habitant.SendMessageToAllies(message);
    }

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(!IsActive && ArePreconditionsSatisfied(p.SensorData)) {
            foreach (Habitant h in p.SensorData.Enemies) {
                RelevantCells.Add (CoordConvertions.AgentPosToTile(h.pos));
            }
            InformOthers();
            EnableBelief();
        } else if(p.Habitant.PendingMessages.Count != 0) { 
            foreach(HabitantMessages.Message m in p.Habitant.PendingMessages) {
                var messageVisitor = new TribeAttackedRecognizer();
                m.AcceptMessageVisitor(messageVisitor);
                if(messageVisitor.EnemyPositions.Count != 0) {
                    foreach(var pos in messageVisitor.EnemyPositions) {
                        RelevantCells.Add(pos);
                    }
                    EnableBelief();
                }
            }
        }
        else if(IsActive) {
            // TODO: consider communication between habitants
            if(p.SensorData.Enemies.Count == 0) {
                DisableBelief();
            }
        }
    }
    public TribeIsBeingAttacked(Habitant habitant) {
        this.habitant = habitant;
    }
    private class TribeAttackedRecognizer : HabitantMessages.IMessageVisitor {
        public List<Vector2I> EnemyPositions = new List<Vector2I>();
        public void VisitHabitantBeingAttacked(HabitantMessages.Messages.HabitantBeingAttacked message) {
            EnemyPositions.AddRange(message.EnemyPositions);

        }
    }
}

public class TribeHasLowFoodLevel : Belief {
    public FoodQuantity foodQuantity;

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        this.foodQuantity = p.SensorData.FoodTribe;
        if(p.SensorData.FoodTribe < new FoodQuantity(Tribe.CRITICAL_FOOD_LEVEL) &&
           !IsActive) {
            EnableBelief();
        } else if(IsActive) {
            DisableBelief();
        }
    }
}

public class TribeHasFewFlags : Belief {
    public int flagsCount;

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        this.flagsCount = p.SensorData.TribeFlags;
        if(p.SensorData.TribeFlags < Tribe.CRITICAL_FLAG_QUANTITY) {
            EnableBelief(2);
        } else if(IsActive && timesToBeActive-- == 0){
            DisableBelief();
        }
    }
}

public class AnimalsAreNear : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        RelevantCells = new List<Vector2I>();
        if(p.SensorData.Animals.Count > 0) {
            foreach(Animal a in p.SensorData.Animals) {
                RelevantCells.Add(CoordConvertions.AgentPosToTile(a.pos));
            }
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class EnemiesAreNear : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        RelevantCells = new List<Vector2I>();
        if(p.SensorData.Enemies.Count > 0) {
            foreach(Habitant h in p.SensorData.Enemies) {
                RelevantCells.Add(CoordConvertions.AgentPosToTile(h.pos));
            }
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class NearEnemyTribe : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        if(p.SensorData.EnemyTribeCells.Count > 0) {
            RelevantCells = p.SensorData.EnemyTribeCells;
            EnableBelief();
        } else {
            DisableBelief();
        }
    }   
}

public class KnownWood : Belief {
    public struct KnownWoodEntry {
        WoodQuantity Wood;
        bool Alive;
        public KnownWoodEntry(WoodQuantity wood, bool alive) {
            this.Wood = wood;
            this.Alive = alive;
        }
        public bool HasWood() {
            return Wood > new WoodQuantity(0);
        }
    }
    public Matrix<KnownWoodEntry?> Map;
    public IEnumerable<Vector2I> CoordsWithWood {
        get {
            return Map.AllCoords.Where(c=>Map[c]!=null && Map[c].Value.HasWood());
        }
    }

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);

            /*
        if(p.SensorData.Trees.Count > 0) {
            RelevantCells = new List<Vector2I>();
            foreach(Tree t in p.SensorData.Trees) {
                if(t.HasWood) {
                    Vector2I c = t.Pos;
                    Forest[c] = t;
                    RelevantCells.Add(t.Pos);
                }
            }
            EnableBelief();
        } else {
            DisableBelief();
        }
        
        // Update with depleted trees
        foreach(var c in p.SensorData.NearbyCells) {
            if (!p.Habitant.worldInfo.worldTiles.WorldTileInfoAtCoord(c).HasTree)
                Forest[c] = null;
                */

        foreach(Tree t in p.SensorData.Trees.Concat(p.SensorData.Stumps))
            Map[t.Pos] = t.HasWood ? new KnownWoodEntry(t.Wood, t.Alive) : (KnownWoodEntry?)null;
            
        // Update with depleted trees
        foreach(var c in p.SensorData.NearbyCells) {
            if (!p.Habitant.worldInfo.worldTiles.WorldTileInfoAtCoord(c).HasTree)
                Map[c] = null;
        }
    }
    
    public KnownWood(Habitant h) {
        Map = new Matrix<KnownWoodEntry?>(h.worldInfo.Size);
    }
}

public class PickableFood : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        RelevantCells = new List<Vector2I>();
        if(p.SensorData.Food.Count > 0) {
            foreach(Animal a in p.SensorData.Food) {
                RelevantCells.Add(CoordConvertions.AgentPosToTile(a.pos));
            }
            EnableBelief();
        } else {
            
            DisableBelief();
        }
    }
}

public class PickableWood : Belief {
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        RelevantCells = new List<Vector2I>();
        if(p.SensorData.Stumps.Count > 0) {
            foreach(Tree t in p.SensorData.Stumps) {
                RelevantCells.Add(t.Pos);
            }
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class HabitantHasLowEnergy : Belief {
    public Energy habitantEnergy;

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        habitantEnergy = p.Habitant.energy;
        if(p.Habitant.LowEnergy()) {
            EnableBelief();
        } else {
            DisableBelief();
        }
    }
}

public class UnclaimedTerritoryIsNear : Belief {

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        RelevantCells = p.SensorData.UnclaimedCells;
        if(p.SensorData.UnclaimedCells.Count > 0) {
            EnableBelief(2);
        } else if(IsActive && timesToBeActive-- == 0){
            DisableBelief();
        }
    }
}

public class KnownObstacles : Belief {
    public enum ObstacleMapEntry { Obstacle, Free, Unknown };
    public Matrix<ObstacleMapEntry> ObstacleMap;
    public bool CoordIsFree(Vector2I coord) {
        return ObstacleMap[coord]!=ObstacleMapEntry.Obstacle;
    }
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        //Update obstacle positions.
        foreach(var obsCoord in SensorDataObstacles(p.SensorData)) {
            ObstacleMap[obsCoord] = ObstacleMapEntry.Obstacle;
        }
        //Update free positions.
        foreach(var freeCoord in p.SensorData.Cells.Except(SensorDataObstacles(p.SensorData))) {
            ObstacleMap[freeCoord] = ObstacleMapEntry.Free;
        }
    }
    public KnownObstacles(Habitant h) {
        EnableBelief();
        CreateObstacleMapForHabitant(h);
    }
    private void CreateObstacleMapForHabitant(Habitant h) {
        var mapSize = h.worldInfo.Size;
        ObstacleMap = new Matrix<ObstacleMapEntry>(mapSize,ObstacleMapEntry.Unknown);
    }
    private IEnumerable<Vector2I> SensorDataObstacles(SensorData sensorData) {
        return sensorData.Trees
                .Concat(sensorData.Stumps).Where(t=>t.HasWood).Select(t=>t.Pos)
                .Concat(sensorData.Enemies.Select(e=>CoordConvertions.AgentPosToTile(e.pos)))
                .Concat(sensorData.Animals.Select(a=>CoordConvertions.AgentPosToTile(a.pos))); 
    }
}

public class TribeTerritories : Belief {
    public Matrix<Tribe> Territories;
    public IEnumerable<Vector2I> UnclaimedTerritories {
        get {
            return Territories.AllCoords.Where(c=>Territories[c]==null);
        }
    }

    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        foreach(var coordTribe in p.SensorData.Territories) {
            Vector2I c = coordTribe.Key;
            Territories[c] = coordTribe.Value;
        }
        foreach(var c in p.SensorData.UnclaimedCells) {
            Territories[c] = null;
        }
    }

    public TribeTerritories(Habitant h) {
        EnableBelief();
        var size = h.worldInfo.Size;
        Territories = new Matrix<Tribe>(size);

        h.sensorData.Territories = new List<KeyValuePair<Vector2I,Tribe>>();
        for (int i = h.tribe.start_x; i < h.tribe.cell_line; i++)
            for (int j = h.tribe.start_y; j < h.tribe.cell_line; j++)
                Territories[new Vector2I(i, j)] = h.tribe;
    }
}

public class CellSeenOrders : Belief {
    public Matrix<int> LastSeenOrders;
    public int CurrentOrder {
        get; 
        private set;
    }
    private readonly int NotSeenBeforeOrder = int.MinValue;
    public CellSeenOrders(Habitant h) {
        CurrentOrder = 0;
        LastSeenOrders = new Matrix<int>(h.worldInfo.Size, NotSeenBeforeOrder);
    }
    public override void UpdateBelief (Percept p) {
        base.UpdateBelief(p);
        CurrentOrder += 1;
        //Mark each seen cell with CurrentOrder.
        foreach(var seenCoord in p.SensorData.Cells) {//FIXME: Make sure these are all the seen cells.
            LastSeenOrders[seenCoord] = CurrentOrder;
        }
    }
}

public class SensorDataDoesNotExists : SystemException {

}