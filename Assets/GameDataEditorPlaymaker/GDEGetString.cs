using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Game Data Editor")]
    [Tooltip("Gets a String from a GDE Item")]
    public class GDEGetString : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("Item Name")]
        public FsmString itemName;

        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("Field Name")]
        public FsmString fieldName;

        [UIHint(UIHint.FsmString)]
        public FsmString storeResult;

        public override void Reset()
        {
            itemName = null;
            fieldName = null;
            storeResult = null;
        }
        
        public override void OnEnter()
        {
            try
            {
                Dictionary<string, object> data;
                if (GameDataEditor.GDEDataManager.Instance.Get(itemName.Value, out data))
                {
                    string val;
                    data.TryGetString(fieldName.Value, out val);
                    storeResult.Value = val;
                }
            }
            catch(UnityException ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                Finish();       
            }
        }
    }
}