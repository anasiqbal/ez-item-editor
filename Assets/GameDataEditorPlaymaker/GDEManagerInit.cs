using UnityEngine;
using GameDataEditor;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("Game Data Editor")]
    [Tooltip("Initializes the Game Data Manager")]
    public class GDEManagerInit : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.FsmString)]
        [Tooltip("GDE Data File Name")]
        public FsmString gdeDataFileName;

        public override void Reset()
        {
            gdeDataFileName = null;
        }
        
        public override void OnEnter()
        {
            try
            {
                GDEDataManager.Instance.Init(gdeDataFileName.Value);
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
