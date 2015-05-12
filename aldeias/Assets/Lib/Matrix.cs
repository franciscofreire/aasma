using System.Collections.Generic;
using System.Linq;

public class Matrix<T> {
    T[,] elems;
    public Matrix(Vector2I size) {
        elems = new T[size.x,size.y];
    }
    public Vector2I Size {
        get {
            return new Vector2I(elems.GetLength(0), elems.GetLength(1));
        }
    }
    public T this[Vector2I coord] {
        get {
            return elems[coord.x, coord.y];
        }
        set {
            elems[coord.x, coord.y] = value;
        }
    }
    public IEnumerable<Vector2I> AllCoords {
        get {
            foreach(var x in Enumerable.Range(0,Size.x)) {
                foreach(var y in Enumerable.Range(0, Size.y)) {
                    yield return new Vector2I(x,y);
                }
            }
        }
    }
    public IEnumerable<T> Elems {
        get {
            return AllCoords.Select(c=>this[c]);
        }
    }
}