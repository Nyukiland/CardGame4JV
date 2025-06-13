using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CardGame.Useful
{
    public static class AddressableErrorHandler
    {
        /// <summary>
        /// Method used to handle Addressable errors as Unity does not do that well
        /// </summary>
        public static void HandleAddressableException(AsyncOperationHandle handle, Exception exception)
        {
            if (exception == null) return;

            if (exception.GetType() == typeof(InvalidKeyException))
            {
                Debug.LogError($"Addressable Error caught : {exception.Message}");
                return;
            }

            Debug.LogError($"Unhandled Addressable exception of type '{exception.GetType()}'\n\n{exception.Message}");
        }
    }
}