using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class TribeOverlayLayer : Layer {
    public Text Text;

    public override void CreateObjects () {
        worldInfo.AddGameEndedListener((string s)=>{
            Text.text += s;
        });
    }

    public override void ApplyWorldInfo () {
        var tribeViews = worldInfo.tribes
            .Select((t)=>{
                var tView = string.Format("Tribe {0}\n\t{1}\n\t{2}\n",t.id,t.FoodStock,t.WoodStock);
                return tView;
            });
        var concatedViews = tribeViews.Aggregate("",(acc,next)=>acc+next);
        Text.text = concatedViews;
    }
}
