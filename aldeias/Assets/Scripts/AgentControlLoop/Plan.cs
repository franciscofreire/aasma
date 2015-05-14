using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Priority_Queue;

public class Plan {
    private Queue<Action> plan = new Queue<Action>();
    public Action LastAction { get; set; }
    public Vector2I LastActionTarget {
        get {
            if (LastAction == null)
                return plan.Peek().target;
            else
                return LastAction.target;
        }
    }

    private Attitude intention;

    public Plan(Attitude intention) {
        this.intention = intention;
    }

    public bool isSound(Beliefs beliefs) {
        return intention.isSound(beliefs);
    }

    public bool isEmpty() {
        return plan.Count == 0;
    }
    
    public int actionsRemaining() {
        return plan.Count;
    }
    
    public void clear() {
        plan.Clear();
        LastAction = null;
    }

    public Action peek() {
        return plan.Peek();
    }

    public Action head() {
        return plan.Dequeue();
    }

    public void add(Action a) {
        plan.Enqueue(a);
    }
    
    public void addLastAction(Action a) {
        add(a);
        LastAction = a;
    }

    public void addFollowPath(Habitant h, Beliefs beliefs, Vector2I target) {
        Path path = Pathfinder.PathInMapFromTo(beliefs.KnownObstacles.ObstacleMap, 
                                               CoordConvertions.AgentPosToTile(h.pos), 
                                               target);
        try {
            for(int i=1; i<path.PathPoints.Count; i++) {
                add(new Walk(h, path.PathPoints[i]));
            }
        }
        catch (System.Exception) {
            Debug.Log("#### NO PATHPOINT FROM AGENT");
        }
    }
    
    public void addFollowPath(Habitant h, Beliefs beliefs, Vector2I from, Vector2I target) {
        Path path = Pathfinder.PathInMapFromTo(beliefs.KnownObstacles.ObstacleMap, 
                                               from, 
                                               target);
        try {
            for(int i=1; i<path.PathPoints.Count; i++) {
                add(new Walk(h, path.PathPoints[i]));
            }
        }
        catch (System.Exception) {
            Debug.Log("#### NO PATHPOINT FROM CELL");
        }
    }

    // Society rule: When you encounter a friendly agent,
    // move to a cell near you and recalculate pathfinding
    public void updatePath(Habitant habitant, Beliefs beliefs, Vector2I nextMove) {
            // Find a free near cell that isn't nextMove
            CellCoordsAround cca = new CellCoordsAround(nextMove, habitant.worldInfo);
            Vector2I neighbor = cca.CoordsAtDistance(1).Where(
                c => {
                    return beliefs.KnownObstacles.ObstacleMap[c.x, c.y] != KnownObstacles.ObstacleMapEntry.Obstacle;
                }
            ).First();

            head(); // Trash the invalid move
            add(new Walk(habitant, neighbor));
            addFollowPath(habitant, beliefs, nextMove);

            /*
            bool candidateAvailable = false;
            IList<Vector2I> candidates = habitant.sensorData.AdjacentCells;
            foreach (Vector2I candidate in candidates) {
                if (candidate != nextMove) {
                    head(); // Trash the invalid move
                    addFollowPath(habitant, beliefs, nextMove);
                    add(new Walk(habitant, candidate));
                    
                    candidateAvailable = true;
                    break;
                }
            }
            
            if (!candidateAvailable)
                return false; // Couldn't apply society rule
            */
    }
}

public class Path {
    public List<Vector2I> PathPoints;
    public Path(List<Vector2I> pts) {
        PathPoints = pts;
    }
}

public class Pathfinder {

    class State {
        public Vector2I Target;
        public List<Vector2I> Path;
        public KnownObstacles.ObstacleMapEntry[,] Map;
        public Vector2I MapSize { 
            get {
                return new Vector2I(Map.GetLength(0),Map.GetLength(1));
            }
        }
        public State(Vector2I from, KnownObstacles.ObstacleMapEntry[,] map, Vector2I target) {
            Target=target;
            Path=new List<Vector2I>();
            Path.Add(from);
            Map=map;
        }
        public State(List<Vector2I> path, KnownObstacles.ObstacleMapEntry[,] map, Vector2I nextPos, Vector2I target) {
            Target=target;
            Path=new List<Vector2I>(path);
            Path.Add(nextPos);
            Map=map;
        }
        public List<State> Neighbors() {
            Vector2I lastPos = Path[Path.Count-1];
            Vector2I[] adjacents = new Vector2I[]{
                lastPos+new Vector2I(1,0),
                lastPos+new Vector2I(-1,0),
                lastPos+new Vector2I(0,1),
                lastPos+new Vector2I(0,-1)
            };
            List<State> neighbors = new List<State>();
            foreach(var coord in adjacents) {
                if(coordInsideMap(coord) && coordIsFree(coord)) {
                    State s = new State(Path, Map, coord, Target);
                    neighbors.Add(s);
                }
            }
            return neighbors;
        }
        private bool coordInsideMap(Vector2I coord) {
            return coord.x >= 0 && coord.x < MapSize.x &&
                coord.y >= 0 && coord.y < MapSize.y;
        }
        private bool coordIsFree(Vector2I coord) {
            return Map[coord.x,coord.y] != KnownObstacles.ObstacleMapEntry.Obstacle;
        }
        public bool IsFinal() {
            return Path[Path.Count-1] == Target;
        }
        public int Cost {
            get {
                //All states include the initial position.
                return Path.Count-1;
            }
        }
        public int RemainingCostEstimate {
            get {   
                return Path[Path.Count-1].DistanceTo(Target);
            }
        }
        public static State FirstState(Vector2I from, KnownObstacles.ObstacleMapEntry[,] map, Vector2I to) {
            return new State(from, map, to);
        }
    }
    class PQueueNode : PriorityQueueNode {
        public State S;
        public PQueueNode(State s) { S = s; }
    }

    public static Path PathInMapFromTo(KnownObstacles.ObstacleMapEntry[,] map, Vector2I from, Vector2I to) {
        State initialState = State.FirstState(from, map, to);
        IPriorityQueue<PQueueNode> openStates = new HeapPriorityQueue<PQueueNode>(map.GetLength(0)*map.GetLength(1)*2);
        PQueueNode init = new PQueueNode(initialState);
        openStates.Enqueue(init, initialState.Cost+initialState.RemainingCostEstimate);
        HashSet<Vector2I> closed = new HashSet<Vector2I>(); 
        HashSet<Vector2I> generated = new HashSet<Vector2I>();
        while(openStates.Count != 0) {
            State currentState = openStates.Dequeue().S;
            if(currentState.IsFinal()) {
                return new Path(currentState.Path);
            }
            closed.Add(currentState.Path[currentState.Path.Count-1]);
            List<State> neighbors = currentState.Neighbors();
            foreach(var neighbor in neighbors) {
                Vector2I nPos = neighbor.Path[neighbor.Path.Count-1];
                if(closed.Contains(nPos)||generated.Contains(nPos)) {
                    continue;//If the neigbour's last coord was already visited the neighbour is not good enough.
                }
                generated.Add(nPos);
                PQueueNode nei = new PQueueNode(neighbor);
                try {
                openStates.Enqueue(nei, neighbor.Cost + neighbor.RemainingCostEstimate);
                } catch (System.IndexOutOfRangeException) {
                    Debug.Log("array out of bounds");
                    throw;
                }
            }
        }
        return null;
    }
}

public class AmazingPathfinder {
    public static Path PathInMapFromTo(KnownObstacles.ObstacleMapEntry[,] map, Vector2I from, Vector2I to) {
        return OrthogonalPathFromTo(from, to);
    }
    // Amazing pathfinding: 2 straight paths ignoring collisions
    public static Path OrthogonalPathFromTo(Vector2I from, Vector2I target) {
        Vector2I pos = from;
        Vector2I step = target - pos;
        step.x = (int) Mathf.Sign(step.x);
        step.y = (int) Mathf.Sign(step.y);
        List<Vector2I> points = new List<Vector2I>();
        points.Add(pos);
        if (pos.x != target.x)
        for (int i = pos.x + step.x; i != target.x; i += step.x) {
            points.Add(new Vector2I(i, pos.y));
        }
        if (pos.y != target.y)
        for (int i = pos.y + step.y; i != target.y; i += step.y) {
            points.Add(new Vector2I(target.x, i));
        }
            // Diagonal case: none of the previous adds where made, so force one of them
            // Of course it doesn't work, plz replace with A*
            //if ((int) Mathf.Abs(step.x) == 1 && (int) Mathf.Abs(step.y) == 1) {
            //    add(new Walk(agent, new Vector2I(target.x, pos.y + step.y)));
            //}
        return new Path(points);
    }
}