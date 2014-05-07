using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

public class InitStats : MonoBehaviour {

    public GUIText CharacterName;
    public GUIText HitPoints;
    public GUIText Mana;
    public GUIText Damage;

    public List<GUIText> Buffs;

    Character character = null;

	void Start () 
    {
        if (GDEDataManager.Instance.Init("rpg_example_data"))
        {
            Dictionary<string, object> charData;
            GDEDataManager.Instance.Get("warrior", out charData);

            character = new Character(charData);

            // Set our GUITexts based on what the character loaded
            CharacterName.text = character.Name;
            HitPoints.text = character.FormatStat(StatType.HP);
            Mana.text = character.FormatStat(StatType.Mana);
            Damage.text = character.FormatStat(StatType.Damage);

            // Set our buff descriptions based on what the character loaded
            for(int index=0;  index<Buffs.Count;  index++)
            {
                if (character.Buffs.IsValidIndex(index))                
                    Buffs[index].text = character.Buffs[index].ToString();
            }
        }
	}
}
