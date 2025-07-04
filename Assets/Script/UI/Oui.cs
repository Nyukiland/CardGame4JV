using CardGame.StateMachine;
using CardGame.Turns;
using CardGame.Utility;
using UnityEngine;

namespace CardGame
{
    public class Oui : MonoBehaviour
    {
        [ContextMenu("OpenWin")]
        public void OpenWin()
        {
            Storage.Instance.GetElement<Controller>().GetStateComponent<HUDResource>().OpenWin();
        }

        [ContextMenu("AddScore")]
        public void AddScore()
        {
            GameManager.Instance.AddScore(50);
        }
        
        [ContextMenu("ShowHUD")]
        public void ShowHUD()
        {
            Storage.Instance.GetElement<Controller>().GetStateComponent<HUDResource>().OpenHud();
        }
        
        [ContextMenu("HideHUD")]
        public void HideHUD()
        {
            Storage.Instance.GetElement<Controller>().GetStateComponent<HUDResource>().CloseHud();
        }
    }
}