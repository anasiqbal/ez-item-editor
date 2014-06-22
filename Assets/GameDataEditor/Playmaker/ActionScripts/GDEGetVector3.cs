using UnityEngine;
using System.Collections.Generic;
using GameDataEditor;
using GameDataEditor.GDEExtensionMethods;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Game Data Editor")]
    [Tooltip("Gets a Vector3 from a GDE Item")]
    public class GDEGetVector3 : GDEActionBase
    {   
        [UIHint(UIHint.FsmVector3)]
        public FsmVector3 StoreResult;
        
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
                    Vector3 val;
                    data.TryGetVector3(FieldName.Value, out val);
                    StoreResult.Value = val;
                }
                else
                {
                    LogError(string.Format(GDEConstants.ErrorLoadingValue, "Vector3", ItemName.Value, FieldName.Value));
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

