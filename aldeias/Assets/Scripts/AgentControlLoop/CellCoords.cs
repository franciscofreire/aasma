using System.Collections.Generic;
using System.Linq;

public class CellCoordsAround {
    Vector2I center;
    WorldInfo world;
    public CellCoordsAround (Vector2I center, WorldInfo world) {
        this.center = center;
        this.world = world;
    }
    public Vector2I Center {
        get {
            return center;
        }
    }
    public IEnumerable<Vector2I> CoordsAtDistance(int mannhatanDist) {
        /*
             * 4 3 2 3 4
             * 3 2 1 2 3
             * 2 1 0 1 2
             * 3 2 1 2 3
             * 4 3 2 3 4
             */
        
        Vector2I center = Center;
        
        Vector2I[][] diagonalTemplates = new Vector2I[][]{
            /* [ startingPointStep, diagStep ] */
            new Vector2I[]{new Vector2I(0,1),new Vector2I(1,-1)},
            new Vector2I[]{new Vector2I(1,0),new Vector2I(-1,-1)},
            new Vector2I[]{new Vector2I(0,-1),new Vector2I(-1,1)},
            new Vector2I[]{new Vector2I(-1,0),new Vector2I(1,1)}
        };
        
        
        int diagonalSize = mannhatanDist;
        var diagonals = diagonalTemplates
            .Select(templ=>new Diagonal(center+templ[0].TimesScalar(mannhatanDist),
                                        templ[1]));
        var diagonalPoints = diagonals
            .Select(d=>Enumerable.Range(0,diagonalSize)
                    .Select(n=>d.NthPoint(n)))
                .Aggregate((acc,diagPts)=>acc.Concat(diagPts));
        return diagonalPoints.Where(pt=>world.isInsideWorld(pt));
        
    }
    
    public IEnumerable<Vector2I> CoordUntilDistance(int mannhatanDist) {
        if(mannhatanDist<1)
            return Enumerable.Empty<Vector2I>();
        return Enumerable.Range(0,mannhatanDist)
            .Select(i=>CoordsAtDistance(i+1))
                .Aggregate((e1,e2)=>e1.Concat(e2));
    }
    
    public IEnumerable<Vector2I> CloserFirst {
        get {
            for(int radius = 1; true; radius++) {
                if(CoordsAtDistance(radius).Count() == 0)
                    yield break;
                foreach(var pt in CoordsAtDistance(radius)) {
                    yield return pt;
                }
            }
        }
    }
    class Diagonal {
        Vector2I start;
        Vector2I step;
        public Vector2I NthPoint(int n) {
            return start + step.TimesScalar(n);
        }
        public Diagonal(Vector2I start, Vector2I step) {
            this.start = start;
            this.step = step;
        }
    }
}