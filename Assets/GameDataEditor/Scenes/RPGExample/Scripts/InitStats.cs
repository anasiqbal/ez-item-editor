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

    //
    // Initialize Game Data 
    //
    // Game Data can be initialized in the Start method. 
    // Start is called on the frame when the script is enabled.
    // 
    // Here we will grab the data with the
    // GDEDataManager.Instance.Get() method and pass that data to the
    // Character classes constructor where the individual fields will
    // be retrieved.
    //
    void Start () 
    {
        // Initialize with the file path which is a resource in the
        // Unity Scene hence the no json file extension.
        if (GDEDataManager.Instance.Init("rpg_example_data"))
        {
            // Get the "warrior" Character data 
            Dictionary<string, object> charData;
            GDEDataManager.Instance.Get("warrior", out charData);

            // Pass the data to the Character constructor to pull out
            // the individual fields of data.
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
