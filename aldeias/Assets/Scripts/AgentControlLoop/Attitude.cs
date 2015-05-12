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
    public HelpAttack HelpAttack;
    public HelpDefense HelpDefense;

    public Attitudes(Habitant h) {
        Explore = new Explore(h);
        ExpandTribe = new ExpandTribe(h);
        ConquerTribe = new ConquerTribe(h);
        MaintainEnergy = new MaintainEnergy(h);
        IncreaseFoodStock = new IncreaseFoodStock(h);
        IncreaseWoodStock = new IncreaseWoodStock(h);
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
        createPlan(beliefs);
        return plan;
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
        IList<Vector2I> targets = beliefs.UnclaimedTerritoryIsNear.RelevantCells;
        foreach (Vector2I target in targets) {
            if (target == plan.LastAction.target
                && !beliefs.WorldInfo.worldTiles.WorldTileInfoAtCoord(target).tribeTerritory.IsClaimed)
                return true;
        }
        return false;
    }

    public override Plan createPlan(Beliefs beliefs) {
        IEnumerable<Vector2I> targets = beliefs
            .UnclaimedTerritoryIsNear.RelevantCells
            .Concat(beliefs.TribeTerritories.UnclaimedTerritories);
        //Vector2I target = habitant.closestCell(targets);
        Vector2I target = targets.First();//FIXME: what if there is no cell left?
        //FIXME: select a free or unknown cell adjacent to target as the cell we want to go to.

        Path pathToTarget = Pathfinder.PathInMapFromTo(beliefs.KnownObstacles.ObstacleMap, 
                                                              CoordConvertions.AgentPosToTile(habitant.pos), 
                                                              target);
        plan.addFollowPath(habitant, pathToTarget);
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
        return beliefs.TribeHasFewFlags.IsActive;
    }
    
    public override bool isSound(Beliefs beliefs) {
        return true;
    }

    public override Plan createPlan(Beliefs beliefs) {
        return plan;
    }

    public IncreaseWoodStock(Habitant habitant) : base(habitant) {}
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