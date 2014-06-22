using UnityEngine;
using GameDataEditor;

#if GDE_PLAYMAKER_SUPPORT

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Game Data Editor")]
    [Tooltip("Initializes the Game Data Manager")]
    public class GDEManagerInit : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("GDE Data File Name")]
        public FsmString GDEDataFileName;

        public override void Reset()
        {
            GDEDataFileName = null;
        }
        
        public override void OnEnter()
        {
            try
            {
                if (!GDEDataManager.Instance.Init(GDEDataFileName.Value))
                    LogError("GDE Data Manager not initialized! " + GDEDataFileName.Value);
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
