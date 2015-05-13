using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Attitudes {
    public Explore Explore;
    public ExpandTribe ExpandTribe;
    public ConquerTribe ConquerTribe;
    public MaintainEnergy MaintainEnergy;
    public IncreaseFoodStock IncreaseFoodStock;
    public IncreaseWoodStock IncreaseWoodStock;
    public DropResources DropResources;
    public StartAttack StartAttack;
    // TODO: StartDefense
    public HelpAttack HelpAttack;
    public HelpDefense HelpDefense;

    public Attitudes(Habitant h) {
        Explore = new Explore(h);
        ExpandTribe = new ExpandTribe(h);
        ConquerTribe = new ConquerTribe(h);
        MaintainEnergy = new MaintainEnergy(h);
        IncreaseFoodStock = new IncreaseFoodStock(h);
        IncreaseWoodStock = new IncreaseWoodStock(h);
        DropResources = new DropResources(h);
        StartAttack = new StartAttack(h);
        HelpAttack = new HelpAttack(h);
        HelpDefense = new HelpDefense(h);
    }
    
    public IEnumerable<Attitude> AllAttitudes {
        get {
            yield return Explore;
            yield return ExpandTribe;
            yield return ConquerTribe;
            yield return MaintainEnergy;
            yield return IncreaseFoodStock;
            yield return IncreaseWoodStock;
            yield return DropResources;
            yield return StartAttack;
            yield return HelpAttack;
            yield return HelpDefense;
        }
    }
}

public abstract class Attitude {
    protected Habitant habitant;
    protected Plan plan;

    // Used by the Filter method
    // The bigger the value, the bigger the chance of being an Intention
    public float Importance {
        get; set;
    }

    public Attitude(Habitant habitant) {
        this.habitant = habitant;
        this.plan = new Plan(this);
    }

    public abstract bool isDesirable(Beliefs beliefs);
    
    public abstract bool isSound(Beliefs beliefs);
    public Plan updatePlan(Beliefs beliefs) {
        this.plan.clear();
        this.plan = createPlan(beliefs);
        return this.plan;
    }
    public abstract Plan createPlan(Beliefs beliefs);
}

/*
Lista candidata de Desires/Intentions:
 -> Explorar
 -> Expandir tribo
    - Verificar se há flags
    - Procurar território neutro
    - Colocar flags
 -> Conquistar tribo adversária
    - Matar os outros
    - "Roubar" Território
 -> Manter energia em níveis aceitáveis
    - Evitar animais e inimigos
    - Procurar animais ou comida
    - Matar animais
    - Recolher comida
 -> Aumentar reservas comida
    Procurar animais ou comida
    - Matar animais
    - Recolher comida
    - Largar comida no meeting point
 -> Aumentar as reservas de madeira
    - Procurar árvores e troncos
    - Cortar árvores
    - Recolher madeira
    - Largar madeira no meeting point
 -> Ajudar os compatriotas - modo defesa
 -> Ajudar os compatriotas - modo ataque
 */

public class Explore : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return true;
    }
    
    public override bool isSound(Beliefs beliefs) {
        return true;
    }

    public override Plan createPlan(Beliefs beliefs) {
        // Try to move away from one's tribe territory
        /*
        if (habitant.closeToTribe()) {
            IEnumerable<Vector2I> targets = beliefs.TribeTerritories.UnclaimedTerritories
                .Where(t=>beliefs.KnownObstacles.ObstacleMap[t.x,t.y]!=KnownObstacles.ObstacleMapEntry.Obstacle);
            Vector2I target = habitant.closestCell(targets);
            
            plan.addFollowPath(habitant, beliefs, target);
        } else
        */

        //TODO: - Go somewhere we haven't been before.
        //TODO: - Prefer to go to places that are least known.
        //TODO: --- Go to a place we haven't visited for a long time.
        //TODO: - Try not to travel nor too little nor too much.

        /*
        //Select the closest cell that is not an obstacle and that has the minimum
        HabitantCellCoords habitantCoords = new CellCoordsAround(habitant.pos, habitant.worldInfo);
        IEnumerable<Vector2I> nearbycellsNotObstacles = habitantCoords.CoordUntilDistance(5)
            .Where(c=>beliefs.KnownObstacles.ObstacleMap[c.x,c.y]!=KnownObstacles.ObstacleMapEntry.Obstacle);
        
        Vector2I target = nearbycellsNotObstacles//.Take(1)//FIXME: The number of cells to consider here is hardcoded.
            .OrderBy(c=>beliefs.CellSeenOrders.LastSeenOrders[c])
                .First();
        
        Plan p = new Plan(this);
        p.addFollowPath(habitant, Pathfinder.PathInMapFromTo(beliefs.KnownObstacles.ObstacleMap, habitantCoords.Center, target));
        
        return p;*/

        plan.add(Action.WalkRandomly(habitant));
        return plan;
    }
    
    public Explore(Habitant habitant) : base(habitant) {
        Importance = 0; // Only explore when you have no other desires
    }
}

public class ExpandTribe : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.UnclaimedTerritoryIsNear.IsActive
            && !beliefs.TribeHasFewFlags.IsActive;
    }
    
    public override bool isSound(Beliefs beliefs) {
        // If the target was claimed in the meantime, you're screwed
        Vector2I target = plan.LastAction.target;
        return !beliefs.WorldInfo.worldTiles.WorldTileInfoAtCoord(plan.LastAction.target)
                       .tribeTerritory.IsClaimed
               ;
               //&& plan.ensureFreeCell(habitant, beliefs, plan.peek().target);
    }

    public override Plan createPlan(Beliefs beliefs) {
        IEnumerable<Vector2I> targets = beliefs.TribeTerritories.UnclaimedTerritories
            .Where(t=>beliefs.KnownObstacles.ObstacleMap[t.x,t.y]!=KnownObstacles.ObstacleMapEntry.Obstacle);
        Vector2I target = habitant.closestCell(targets);

        plan.addFollowPath(habitant, beliefs, target);
        plan.addLastAction(new PlaceFlag(habitant, target));

        return plan;
    }

    public ExpandTribe(Habitant habitant) : base(habitant) {
        Importance = 10;
    }
}

public class ConquerTribe : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.NearEnemyTribe.IsActive;
    }

    public override bool isSound(Beliefs beliefs) {
        return true;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return plan;
    }

    public ConquerTribe(Habitant habitant) : base(habitant) {}
}

public class MaintainEnergy : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.HabitantHasLowEnergy.IsActive;
    }
    
    public override bool isSound(Beliefs beliefs) {
        return true;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return plan;
    }

    public MaintainEnergy(Habitant habitant) : base(habitant) {}
}

public class IncreaseFoodStock : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.TribeHasLowFoodLevel.IsActive;
    }
    
    public override bool isSound(Beliefs beliefs) {
        return true;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return plan;
    }

    public IncreaseFoodStock(Habitant habitant) : base(habitant) {}
}

public class IncreaseWoodStock : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        bool condition = beliefs.TribeHasFewFlags.IsActive
            && habitant.CanCarryWeight(Tree.WoodChopQuantity.Weight);
        return condition;
        //return false;
    }
    
    public override bool isSound(Beliefs beliefs) {
        return habitant.DepletedTree(plan.LastAction.target);
    }
    
    public override Plan createPlan(Beliefs beliefs) {
        IEnumerable<Vector2I> targets = beliefs.ForestNear.AvailableTrees
            ;
            //.Where(t=>beliefs.KnownObstacles.ObstacleMap[t.x,t.y]!=KnownObstacles.ObstacleMapEntry.Obstacle);

        // Do we know about any trees?
        if (targets.Count() > 0) {
            Vector2I target = Vector2I.INVALID;
            foreach(Vector2I t in targets) {
                if (!habitant.DepletedTree(t)) {
                    target = t;
                    break;
                }
            }
            //Vector2I target = habitant.closestCell(targets);
            CellCoordsAround cca = new CellCoordsAround(target, habitant.worldInfo);
            Vector2I neighbor = Vector2I.INVALID;
            try {
                neighbor = cca.CoordsAtDistance(1).Where(
                    c => {
                        return beliefs.KnownObstacles.ObstacleMap[c.x, c.y] != KnownObstacles.ObstacleMapEntry.Obstacle;
                    }
                ).First();
            
            }
            catch (System.Exception) {
                Debug.Log("bBBBBBBBBBBBBB");
                return plan;
            }
        
        plan.addFollowPath(habitant, beliefs, neighbor);

            if (habitant.AliveTree(target))
                plan.add(new CutTree(habitant, target));

            plan.addLastAction(new ChopTree(habitant, target));
        }
        // Search for trees
        else {

        }
        return plan;
    }

    public IncreaseWoodStock(Habitant habitant) : base(habitant) {
        Importance = 20;
    }
}

public class DropResources : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return !habitant.CanCarryWeight(Tree.WoodChopQuantity.Weight)
            && !habitant.CanCarryWeight(Animal.FOOD_WEIGHT);
    }
    
    public override bool isSound(Beliefs beliefs) {
        // TODO: ensureFreeCell check
        return true;
    }
    
    public override Plan createPlan(Beliefs beliefs) {
        Vector2I target = habitant.tribe.meetingPoint.center;
        plan.addFollowPath(habitant, beliefs, target);

        if (habitant.CarryingFood)
            plan.add(new DropFood(habitant, target));
        if (habitant.CarryingWood)
            plan.add(new DropTree(habitant, target));

        return plan;
    }
    
    public DropResources(Habitant habitant) : base(habitant) {
        Importance = 50;
    }
}

public class StartAttack : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.EnemiesAreNear.IsActive; // FIXME: Maybe another belief
    }
    
    public override bool isSound(Beliefs beliefs) {
        return true;
    }
    
    public override Plan createPlan(Beliefs beliefs) {
        return plan;
    }
    
    public StartAttack(Habitant habitant) : base(habitant) {}
}

public class HelpDefense : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.TribeIsBeingAttacked.IsActive;
    }
    
    public override bool isSound(Beliefs beliefs) {
        return true;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return plan;
    }

    public HelpDefense(Habitant habitant) : base(habitant) {}
}

public class HelpAttack : Attitude {
    public override bool isDesirable(Beliefs beliefs) {
        return beliefs.TribeIsBeingAttacked.IsActive; // FIXME: Maybe another belief
    }
    
    public override bool isSound(Beliefs beliefs) {
        return true;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return plan;
    }

    public HelpAttack(Habitant habitant) : base(habitant) {}
}