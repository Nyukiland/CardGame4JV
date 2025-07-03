using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CardGame.Managers
{
    public static class AddressableManager
    {
        private static readonly Dictionary<string, AsyncOperationHandle> _allAddressable = new();
        private static readonly Dictionary<string, AsyncOperationHandle> _allAddressableLabels = new();
        
        public static async UniTask<T> LoadAddressable<T>(string key)
        {
            if (_allAddressable.TryGetValue(key, out AsyncOperationHandle savedHandle))
            {
                return (T)savedHandle.Result;
            }
            
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);

            await UniTask.WaitUntil(() => handle.IsDone);

            _allAddressable.Add(key, handle);

            return handle.Result;
        }

        public static void UnloadAddressable<T>(AsyncOperationHandle<T> handle)
        {
            handle.Release();
        }

        public static async UniTask<IList<T>> LoadLabel<T>(string label)
        {
            if (_allAddressableLabels.TryGetValue(label, out AsyncOperationHandle savedHandle))
            {
                return (IList<T>)savedHandle.Result;
            }
            
            AsyncOperationHandle<IList<T>> handles = Addressables.LoadAssetsAsync<T>(label);

            await UniTask.WaitUntil(() => handles.IsDone);
            
            _allAddressableLabels.Add(label, handles);

            return handles.Result;
        }

        public static void UnloadLabel<T>(AsyncOperationHandle<IList<T>> handles)
        {
            Addressables.Release(handles);
        }
        
        public static void UnloadAllAddressable()
        {
            foreach (AsyncOperationHandle handle in _allAddressable.Values)
            {
                handle.Release();
            }
            
            foreach (AsyncOperationHandle handle in _allAddressableLabels.Values)
            {
                handle.Release();
            }

            _allAddressable.Clear();
            _allAddressableLabels.Clear();
        }
    }
}