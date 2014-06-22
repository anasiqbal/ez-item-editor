using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Game Data Editor")]
    [Tooltip("Gets a String from a GDE Custom Item")]
    public class GDEGetCustomString : GDEActionBase
    {   
        [UIHint(UIHint.FsmString)]
        [Tooltip("The field name of the string inside the custom item.")]
        public FsmString CustomField;

        [UIHint(UIHint.FsmString)]
        public FsmString StoreResult;
        
        public override void Reset()
        {
            base.Reset();
            StoreResult = null;
        }
        
        public override void OnEnter()
        {
            try
            {
                Dictionary<string, object> data;
                if (GameDataEditor.GDEDataManager.Instance.Get(ItemName.Value, out data))
                {
                    string customKey;
                    data.TryGetString(FieldName.Value, out customKey);

                    Dictionary<string, object> customData;
                    GDEDataManager.Instance.Get(customKey, out customData);

                    string val;
                    customData.TryGetString(CustomField.Value, out val);
                    StoreResult.Value = val;
                }
                else
                {
                    LogError(string.Format(GDEConstants.ErrorLoadingValue, "string", ItemName.Value, FieldName.Value));
                }
            }
            catch(UnityException ex)
            {
                LogError(ex.ToString());
            }
            finally
            {
                Finish();
            }
        }
    }
}

#endif
