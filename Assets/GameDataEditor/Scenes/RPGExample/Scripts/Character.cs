using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public enum StatType
{
    HP,
    Mana,
    Damage
};

public class Character 
{
    private int baseHP = 0;
    private int baseMana = 0;
    private int baseDamage = 0;

    private int bonusHP = 0;
    private int bonusMana = 0;
    private int bonusDamage = 0;

    //
    // Character's public Properties that are defined by the data that
    // gets pulled in from the Game Data Editor.
    //
    public string Name;

    public int HP
    {
        get
        {
            return baseHP + bonusHP;
        }
    }
    public int Mana
    {
        get
        {
            return baseMana + bonusMana;
        }
    }
    public int Damage
    {
        get
        {
            return baseDamage + bonusDamage;
        }
    }

    public List<Buff> Buffs;

    //
    // Constructor that takes the data returned by the
    // GDEDataManager.Get() method and will now pull out the
    // individual fields using the TryGet() methods.
    //
    public Character(Dictionary<string, object> data)
    {
        if (data != null)
        {
            // Pull out the individual fields and load our Character stats.
            data.TryGetString("name", out Name);
            data.TryGetInt("hp", out baseHP);
            data.TryGetInt("mana", out baseMana);
            data.TryGetInt("damage", out baseDamage);

            //
            // Get the Buff list 
            //
            // First get a string list of the buffs using TryGetStringList
            List<string> buffKeyList;
            if (data.TryGetStringList("buffs", out buffKeyList))
            {
                //
                // Spin through each of the Buff names and pull out
                // the data with the GDEDataManager.Get() method.
                //
                Buffs = new List<Buff>();
                foreach(string buffKey in buffKeyList)
                {
                    //
                    // Pull out Buff data with Get() method and pass
                    // it down to the Buff classes constructor where
                    // it will pull out the individual fields and set
                    // it's properties.
                    //
                    Buff curBuff;
                    Dictionary<string, object> curBuffData;
                    GDEDataManager.Instance.Get(buffKey, out curBuffData);

                    //
                    // For each Buff in the string list create a new
                    // object passing it the data from the Get()
                    // method and add it to the Buff's list.
                    //
                    curBuff = new Buff(curBuffData);
                    Buffs.Add(curBuff);
                    
                    //
                    // Now that the Buff's properties have been set
                    // from the data in the Buff's constructor add the
                    // bonuses to the HP, Mana, and Damage. 
                    //
                    bonusHP += curBuff.HPDelta;
                    bonusMana += curBuff.ManaDelta;
                    bonusDamage += curBuff.DamageDelta;
                }
            }
        }
    }


    // 
    // Pretty print the Stats (HP, Mana, Damage) and Bonuses in the Scene
    // 
    public string FormatStat(StatType type)
    {
        string formattedStat;

        switch(type)
        {
            case StatType.HP:
            {
                formattedStat = string.Format("HP: {0}{1}", HP, FormatBonus(type));
                break;
            }

            case StatType.Mana:
            {
                formattedStat = string.Format("Mana: {0}{1}", Mana, FormatBonus(type));
                break;
            }

            case StatType.Damage:
            {
                formattedStat = string.Format("Damage: {0}{1}", Damage, FormatBonus(type));
                break;
            }

            default:
            {
                formattedStat = "Format for "+type.ToString()+" not defined!";
                break;
            }
        }

        return formattedStat;
    }

    private string FormatBonus(StatType type)
    {
        string formattedBonus;

        switch(type)
        {
            case StatType.HP:
            {
                if (bonusHP != 0)
                    formattedBonus = string.Format(" ({0}{1})", bonusHP>0?"+":"", bonusHP);
                else
                    formattedBonus = "";
                break;
            }

            case StatType.Mana:
            {
                if (bonusMana != 0)
                    formattedBonus = string.Format(" ({0}{1})", bonusMana>0?"+":"", bonusMana);
                else
                    formattedBonus = "";
                break;
            }

            case StatType.Damage:
            {
                if (bonusDamage != 0)
                    formattedBonus = string.Format(" ({0}{1})", bonusDamage>0?"+":"", bonusDamage);
                else
                    formattedBonus = "";
                break;
            }

            default:
            {
                formattedBonus = "";
                break;
            }
        }

        return formattedBonus;
    }
}
