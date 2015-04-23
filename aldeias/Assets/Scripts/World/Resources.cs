
// A WoodQuantity a non-negative quantity.
//    Represents wood resource amounts.
public struct WoodQuantity {
	public int Count;
	public WoodQuantity(int c) {
		Count = c;
	}
	public static readonly Weight WeightPerUnit = new Weight(1);
	public Weight Weight {
		get {
			return WeightPerUnit.MultiplyByInt(Count);
		}
	}
	public static WoodQuantity operator -(WoodQuantity w1, WoodQuantity w2) {
		return new WoodQuantity(w1.Count - w2.Count);
	}
	public static WoodQuantity operator +(WoodQuantity w1, WoodQuantity w2) {
		return new WoodQuantity(w1.Count + w2.Count);
	}
	public static bool operator >(WoodQuantity w1, WoodQuantity w2) {
		return w1.Count > w2.Count;
	}
	public static bool operator <(WoodQuantity w1, WoodQuantity w2) {
		return w1.Count < w2.Count;
	}
	public static bool operator >=(WoodQuantity w1, WoodQuantity w2) {
		return w1.Count >= w2.Count;
	}
	public static bool operator <=(WoodQuantity w1, WoodQuantity w2) {
		return w1.Count <= w2.Count;
	}
	public static bool operator ==(WoodQuantity w1, WoodQuantity w2) {
		return w1.Count == w2.Count;
	}
	public static bool operator !=(WoodQuantity w1, WoodQuantity w2) {
		return w1.Count != w2.Count;
	}
	public static WoodQuantity Zero {
		get { return new WoodQuantity(0); }
	}
}

public struct FoodQuantity {
	public int Count;
	public FoodQuantity(int c) {
		Count = c;
	}
	public static readonly Weight WeightPerUnit = new Weight(1);
	public Weight Weight {
		get {
			return WeightPerUnit.MultiplyByInt(Count);
		}
	}
	public static FoodQuantity operator +(FoodQuantity w1, FoodQuantity w2) {
		return new FoodQuantity(w1.Count + w2.Count);
	}
	public static FoodQuantity operator -(FoodQuantity w1, FoodQuantity w2) {
		return new FoodQuantity(w1.Count - w2.Count);
	}
	public static bool operator >(FoodQuantity w1, FoodQuantity w2) {
		return w1.Count > w2.Count;
	}
	public static bool operator <(FoodQuantity w1, FoodQuantity w2) {
		return w1.Count < w2.Count;
	}
	public static bool operator >=(FoodQuantity w1, FoodQuantity w2) {
		return w1.Count >= w2.Count;
	}
	public static bool operator <=(FoodQuantity w1, FoodQuantity w2) {
		return w1.Count <= w2.Count;
	}
	public static bool operator ==(FoodQuantity w1, FoodQuantity w2) {
		return w1.Count == w2.Count;
	}
	public static bool operator !=(FoodQuantity w1, FoodQuantity w2) {
		return w1.Count != w2.Count;
	}
	public static FoodQuantity Zero {
		get { return new FoodQuantity(0); }
	}
} 

public struct Weight {
	public int Count;
	public Weight(int c) {
		Count = c;
	}
	public static Weight operator +(Weight w1, Weight w2) {
		return new Weight(w1.Count + w2.Count);
	}
	public Weight MultiplyByInt(int posInt) {
		return new Weight(Count * posInt);
	}
	public static bool operator >(Weight w1, Weight w2) {
		return w1.Count > w2.Count;
	}
	public static bool operator <(Weight w1, Weight w2) {
		return w1.Count < w2.Count;
	}
	public static bool operator >=(Weight w1, Weight w2) {
		return w1.Count >= w2.Count;
	}
	public static bool operator <=(Weight w1, Weight w2) {
		return w1.Count <= w2.Count;
	}
	public static bool operator ==(Weight w1, Weight w2) {
		return w1.Count == w2.Count;
	}
	public static bool operator !=(Weight w1, Weight w2) {
		return w1.Count != w2.Count;
	}
	public static Weight Zero {
		get { return new Weight(0); }
	}
}