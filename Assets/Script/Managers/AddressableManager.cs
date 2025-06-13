using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CardGame
{
    public static class AddressableManager
    {
        private static List<AsyncOperationHandle> _allAddressable = new();
        
        public static async UniTask<AsyncOperationHandle<T>> LoadAddressable<T>(string key, bool release = false)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            await UniTask.WaitUntil(() => handle.IsDone);

            if (release)
                handle.Release();
            else
                _allAddressable.Add(handle);

            return handle;
        }

        public static void UnloadAddressable<T>(AsyncOperationHandle<T> handle)
        {
            handle.Release();
        }
        
        public static async UniTask<AsyncOperationHandle<IList<T>>> LoadLabel<T>(string label, bool release = false)
        {
            AsyncOperationHandle<IList<T>> handles = Addressables.LoadAssetsAsync<T>(label);
            await UniTask.WaitUntil(() => handles.IsDone);

            if (release)
                handles.Release();
            else
                _allAddressable.Add(handles);

            return handles;
        }
        
        public static void UnloadLabel<T>(AsyncOperationHandle<IList<T>> handles)
        {
            Addressables.Release(handles);
        }
        
        public static void UnloadAllAddressable()
        {
            for (int i = _allAddressable.Count - 1; i >= 0; i--)
            {
                _allAddressable[i].Release();
            }

            _allAddressable = new();
        }
    }
}