using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Game Data Editor")]
    [Tooltip("Gets a String from a GDE Item")]
    public class GDEGetString : GDEActionBase
    {
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
                    string val;
                    data.TryGetString(FieldName.Value, out val);
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