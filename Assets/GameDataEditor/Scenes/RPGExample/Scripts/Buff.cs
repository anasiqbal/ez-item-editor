using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor.GDEExtensionMethods;

public class Buff 
{
    public readonly string Name;
    public readonly int HPDelta;
    public readonly int ManaDelta;
    public readonly int DamageDelta;

    //
    // Constructor that takes the data returned by the
    // GDEDataManager.Get() method and will now pull out the
    // individual fields using the TryGet() methods.
    //
    public Buff(Dictionary<string, object> data)
    {
        if (data != null)
        {
            data.TryGetString("name", out Name);
            data.TryGetInt("hp_delta", out HPDelta);
            data.TryGetInt("mana_delta", out ManaDelta);
            data.TryGetInt("damage_delta", out DamageDelta);
        }
    }

    // 
    // Pretty print the Buff properties
    // 
    public override string ToString()
    {
        bool needsComma = false;
        string buffString = Name + ": ";

        if (HPDelta != 0)
        {
            buffString += string.Format("{0}{1} HP", HPDelta>0?"+":"", HPDelta);
            needsComma = true;
        }

        if (ManaDelta != 0)
        {
            buffString += string.Format("{0}{1}{2} Mana", needsComma?",":"", ManaDelta>0?"+":"", ManaDelta);
            needsComma = true;
        }

        if (DamageDelta != 0)
        {
            buffString += string.Format("{0}{1}{2} Damage", needsComma?",":"", DamageDelta>0?"+":"", DamageDelta);
        }

        return buffString;
    }
}
