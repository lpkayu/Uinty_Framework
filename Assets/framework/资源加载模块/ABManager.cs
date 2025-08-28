using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class ABManager : SingletonAuto<ABManager>
{
   private AssetBundle mainAB=null; //主包
   private AssetBundleManifest _manifest; //主包中的固定文件

   //用来存储加载过的AB包，因为AB包不能重复加载
   private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

   private string abPath
   {
      get
      {
         return Application.streamingAssetsPath + "/";
      }
   }

   private string mainABName
   {
      get
      {
#if UNITY_IOS
         return "IOS";
#elif UNITY_ANDROID
         return "Android";
#else
         return "PC";
#endif
      }
   }

/// <summary>
/// 加载主包
/// </summary>
   public void LoadABMain()
   {
      if (mainAB == null)
      {
         mainAB = AssetBundle.LoadFromFile(abPath+mainABName);
         _manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
      }
   }


   /// <summary>
   /// 直接异步加载
   /// </summary>
   /// <param name="abName">AB包名</param>
   /// <param name="resName">资源名</param>
   /// <param name="callBack">回调函数</param>
   /// <param name="isAsync">是否使用异步加载</param>
   public void LoadResourceAsync(string abName, string resName,bool isAsync,UnityAction<Object> callBack)
   {
      StartCoroutine(LoadAbResource(abName, resName, isAsync,callBack));
   }

   public IEnumerator LoadAbResource(string abName, string resName,bool isAsync,UnityAction<Object> callBack)
   {
      LoadABMain();
      //异步获取依赖包
      string[] manifests = _manifest.GetAllDependencies(abName);
      for (int i = 0; i < manifests.Length; i++)
      {
         //如果字典中不存在该包 则添加
         if (!abDic.ContainsKey(manifests[i]))
         {
            if (!isAsync)
            {
               AssetBundle ab = AssetBundle.LoadFromFile(abPath + manifests[i]);
               abDic.Add(manifests[i],ab);
            }
            else
            {
               abDic.Add(manifests[i], null);
               AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(abPath+manifests[i]);
               yield return request;
               abDic[manifests[i]] = request.assetBundle;
            }
         }
         else
         {
            //防止同时加载同一个AB包
            while (abDic[manifests[i]]==null)
            {
               yield return 0;
            }
         }
      }
      
      //加载目标资源包部分
      if (!abDic.ContainsKey(abName))
      {
         if (!isAsync)
         {
            AssetBundle ab = AssetBundle.LoadFromFile(abPath + abName);
            abDic.Add(abName,ab);
         }
         else
         {
            abDic.Add(abName,null);
            AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(abPath + abName);
            yield return ab;
            abDic[abName] = ab.assetBundle;
         }
      }
      else
      {
         while (abDic[abName]==null)
         {
            yield return 0;
         }
      }
      
      //加载包中资源
      if (!isAsync)
      {
         callBack(abDic[abName].LoadAsset(resName));
      }
      else
      {
         AssetBundleRequest abr=abDic[abName].LoadAssetAsync(resName);
         yield return abr;
         callBack(abr.asset);
      }
      
   }
   
   
   /// <summary>
   /// 使用type进行异步加载
   /// </summary>
   /// <param name="abName">AB包名</param>
   /// <param name="resName">资源名</param>
   /// <param name="type">资源类型</param>
   /// <param name="isAsync">是否使用异步加载</param>
   /// <param name="callBack">回调函数</param>
   public void LoadResourceAsync(string abName, string resName,Type type,bool isAsync,UnityAction<Object> callBack)
   {
      StartCoroutine(LoadAbResource(abName, resName,type,isAsync,callBack));
   }

   public IEnumerator LoadAbResource(string abName, string resName, Type type, bool isAsync,
      UnityAction<Object> callBack)
   {
      LoadABMain();
      //异步获取依赖包
      string[] manifests = _manifest.GetAllDependencies(abName);
      for (int i = 0; i < manifests.Length; i++)
      {
         if (!abDic.ContainsKey(manifests[i]))
         {
            if (!isAsync)
            {
               AssetBundle ab = AssetBundle.LoadFromFile(abPath + manifests[i]);
               abDic.Add(manifests[i], ab);
            }
            else
            {
               abDic.Add(manifests[i], null);
               AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(abPath + manifests[i]);
               yield return request;
               abDic[manifests[i]] = request.assetBundle;
            }
         }
         else
         {
            while (abDic[manifests[i]] == null)
            {
               yield return 0;
            }
         }
      }

      //加载目标资源包部分
      if (!abDic.ContainsKey(abName))
      {
         if (!isAsync)
         {
            AssetBundle ab = AssetBundle.LoadFromFile(abPath + abName);
            abDic.Add(abName, ab);
         }
         else
         {
            abDic.Add(abName, null);
            AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(abPath + abName);
            yield return ab;
            abDic[abName] = ab.assetBundle;
         }
      }
      else
      {
         while (abDic[abName] == null)
         {
            yield return 0;
         }
      }

      if (!isAsync)
      {
         callBack(abDic[abName].LoadAsset(resName,type));
      }
      else
      {
         AssetBundleRequest abr = abDic[abName].LoadAssetAsync(resName, type);
         yield return abr;
         callBack(abr.asset);
      }
      
   }

   
   /// <summary>
   /// 使用泛型进行异步加载
   /// </summary>
   /// <param name="abName">AB包名</param>
   /// <param name="resName">资源名</param>
   /// <param name="isAsync">是否使用异步加载</param>
   /// <param name="callBack">回调函数</param>
   /// <typeparam name="T">资源类型</typeparam>
   public void LoadResourceAsync<T>(string abName, string resName,bool isAsync,UnityAction<T> callBack)where T:Object
   {
      StartCoroutine(LoadAbResource<T>(abName, resName,isAsync,callBack));
   }

   public IEnumerator LoadAbResource<T>(string abName, string resName,bool isAsync,UnityAction<T> callBack)where T:Object
   {
      LoadABMain();
      //异步获取依赖包
      string[] manifests = _manifest.GetAllDependencies(abName);
      for (int i = 0; i < manifests.Length; i++)
      {
         if (!abDic.ContainsKey(manifests[i]))
         {
            if (!isAsync)
            {
               AssetBundle ab = AssetBundle.LoadFromFile(abPath+manifests[i]);
               abDic.Add(manifests[i],ab);
            }
            else
            {
               abDic.Add(manifests[i], null);
               AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(abPath+manifests[i]);
               yield return request;
               abDic[manifests[i]] = request.assetBundle;
            }
         }
         else
         {
            while (abDic[manifests[i]]==null)
            {
               yield return 0;
            }
         }
      }
      
      //加载目标资源包部分
      if (!abDic.ContainsKey(abName))
      {
         if (!isAsync)
         {
            AssetBundle ab = AssetBundle.LoadFromFile(abPath+abName);
            abDic.Add(abName,ab);
         }
         else
         {
            abDic.Add(abName,null);
            AssetBundleCreateRequest ab = AssetBundle.LoadFromFileAsync(abPath + abName);
            yield return ab;
            abDic[abName] = ab.assetBundle;
         }
      }
      else
      {
         while (abDic[abName]==null)
         {
            yield return 0;
         }
      }

      if (!isAsync)
      {
         callBack(abDic[abName].LoadAsset<T>(resName));
      }
      else
      {
         AssetBundleRequest abr=abDic[abName].LoadAssetAsync<T>(resName);
         yield return abr;
         callBack(abr.asset as T);
      }
      
   }
   
   //单个包卸载 callBack用于返回是否卸载完成
   public void UnLoadResource(string abName,UnityAction<bool> callBack)
   {
      if (abDic.ContainsKey(abName))
      {
         if (abDic[abName] == null)
         {
            callBack(false);
            return;
         }
         abDic[abName].Unload(false);
         abDic.Remove(abName);
         //表示卸载成功
         callBack(true);
      }
   }

   //所有包卸载
   public void UnloadAllResource()
   {
      StopAllCoroutines();
      AssetBundle.UnloadAllAssetBundles(false);
      abDic.Clear();
      mainAB = null;
      _manifest = null;
   }
   
}
