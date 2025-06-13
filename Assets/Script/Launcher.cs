using System.Collections.Generic;
using CardGame.Useful;
using UnityEngine;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CardGame
{
    public class Launcher : MonoBehaviour
    {
        // Script used to initalize the game, might be moved later
        void Start()
        {
            ResourceManager.ExceptionHandler = AddressableErrorHandler.HandleAddressableException;
        }
    }
}