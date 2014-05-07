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
    public string Name;

    private int baseHP = 0;
    private int baseMana = 0;
    private int baseDamage = 0;

    private int bonusHP = 0;
    private int bonusMana = 0;
    private int bonusDamage = 0;

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

    public Character(Dictionary<string, object> data)
    {
        if (data != null)
        {
            // Load our stats from the data
            data.TryGetString("name", out Name);
            data.TryGetInt("hp", out baseHP);
            data.TryGetInt("mana", out baseMana);
            data.TryGetInt("damage", out baseDamage);

            List<string> buffKeyList;
            if (data.TryGetStringList("buffs", out buffKeyList))
            {
                Buffs = new List<Buff>();
                foreach(string buffKey in buffKeyList)
                {
                    Buff curBuff;
                    Dictionary<string, object> curBuffData;
                    GDEDataManager.Instance.Get(buffKey, out curBuffData);

                    curBuff = new Buff(curBuffData);
                    Buffs.Add(curBuff);

                    // Add the bonus' from the buff
                    bonusHP += curBuff.HPDelta;
                    bonusMana += curBuff.ManaDelta;
                    bonusDamage += curBuff.DamageDelta;
                }
            }
        }
    }

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
