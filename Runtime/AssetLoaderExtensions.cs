using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.ktgame.assets.loader.core
{
    public static class AssetLoaderExtensions
    {
        /// <summary>
        /// Loads an asset asynchronously and ties its lifecycle to the provided CancellationToken.
        /// If the token is cancelled, the asset is automatically released to prevent memory leaks.
        /// </summary>
        public static AssetRequest<TAsset> LoadAsync<TAsset>(this IAssetLoader loader, string address, CancellationToken cancellationToken) where TAsset : Object
        {
            var request = loader.LoadAsync<TAsset>(address);
            
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => 
                {
                    try 
                    { 
                        loader.Release(request); 
                    } 
                    catch (InvalidOperationException) 
                    { 
                        // Already released, safe to ignore
                    }
                });
            }

            return request;
        }

        /// <summary>
        /// Loads an asset asynchronously and automatically releases it when the provided GameObject is destroyed.
        /// This completely eliminates manual release tracking and memory leaks.
        /// </summary>
        public static AssetRequest<TAsset> LoadAsync<TAsset>(this IAssetLoader loader, string address, GameObject bindTo) where TAsset : Object
        {
            if (bindTo == null) return loader.LoadAsync<TAsset>(address);
            return loader.LoadAsync<TAsset>(address, bindTo.GetCancellationTokenOnDestroy());
        }
        
        /// <summary>
        /// Binds an already created AssetRequest to a GameObject's lifecycle so it automatically releases when the GameObject is destroyed.
        /// </summary>
        public static AssetRequest<TAsset> BindTo<TAsset>(this AssetRequest<TAsset> request, IAssetLoader loader, GameObject gameObject) where TAsset : Object
        {
            if (gameObject == null) return request;
            
            var token = gameObject.GetCancellationTokenOnDestroy();
            token.Register(() => 
            {
                try 
                { 
                    loader.Release(request); 
                } 
                catch (InvalidOperationException) 
                { 
                    // Already released, safe to ignore
                }
            });
            return request;
        }
    }
}
