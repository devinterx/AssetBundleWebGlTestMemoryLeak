using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    [FormerlySerializedAs("AssetBundleUrl")]
    public string assetBundleUrl = "https://127.0.0.1:8080/StreamingAssets/mainbundle";

    [FormerlySerializedAs("LoadScene")]
    public Button loadScene;

    [FormerlySerializedAs("UnLoadScene")]
    public Button unLoadScene;

    [FormerlySerializedAs("LoadBundle")]
    public Button loadBundle;

    [FormerlySerializedAs("UnLoadBundle")]
    public Button unLoadBundle;

    private bool _isBundledSceneLoaded;
    private bool _isBundledSceneStartLoaded;

    private bool _isBundleLoaded;
    private bool _isBundleStartLoaded;

    private GameObject _container;

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void Awake()
    {
        if (loadScene != null) loadScene.onClick.AddListener(OnButtonLoadSceneClicked);
        if (unLoadScene != null) unLoadScene.onClick.AddListener(OnButtonUnLoadSceneClicked);
        if (loadBundle != null) loadBundle.onClick.AddListener(OnButtonLoadBundleClicked);
        if (unLoadBundle != null) unLoadBundle.onClick.AddListener(OnButtonUnLoadBundleClicked);
    }

    private void Update()
    {
        if (_isBundledSceneLoaded && loadScene.interactable) loadScene.interactable = false;
        if (_isBundledSceneLoaded && !unLoadScene.interactable) unLoadScene.interactable = true;

        if (!_isBundledSceneLoaded && !loadScene.interactable) loadScene.interactable = true;
        if (!_isBundledSceneLoaded && unLoadScene.interactable) unLoadScene.interactable = false;

        if (!_isBundledSceneLoaded && loadBundle.interactable) loadBundle.interactable = false;
        if (!_isBundledSceneLoaded && unLoadBundle.interactable) unLoadBundle.interactable = false;

        if (_isBundledSceneLoaded && !_isBundleLoaded && !loadBundle.interactable) loadBundle.interactable = true;
        if (_isBundledSceneLoaded && _isBundleLoaded && !unLoadBundle.interactable) unLoadBundle.interactable = true;

        if (_isBundledSceneLoaded && _isBundleLoaded && loadBundle.interactable) loadBundle.interactable = false;
        if (_isBundledSceneLoaded && !_isBundleLoaded && unLoadBundle.interactable) unLoadBundle.interactable = false;
    }

    private void OnButtonLoadSceneClicked()
    {
        if (_isBundledSceneLoaded || _isBundledSceneStartLoaded) return;

        _isBundledSceneStartLoaded = true;
        Debug.Log("Load BundleScene");
        StartCoroutine(LoadBundledSceneAsync());
    }

    private IEnumerator LoadBundledSceneAsync()
    {
        var asyncLoad = SceneManager.LoadSceneAsync("Scenes/BundleScene", LoadSceneMode.Additive);

        while (!asyncLoad.isDone) yield return null;

        _container = GameObject.FindWithTag("Container");

        _isBundledSceneLoaded = true;
        _isBundledSceneStartLoaded = false;
    }

    private void OnButtonUnLoadSceneClicked()
    {
        if (!_isBundledSceneLoaded) return;
        Debug.Log("Unload BundleScene");
        StartCoroutine(UnLoadBundledSceneAsync());
    }

    private IEnumerator UnLoadBundledSceneAsync()
    {
        var asyncUnload = SceneManager.UnloadSceneAsync("Scenes/BundleScene");

        while (!asyncUnload.isDone) yield return null;
        _isBundledSceneLoaded = false;
        _isBundledSceneStartLoaded = false;
        _isBundleStartLoaded = false;
        _isBundleLoaded = false;

        AssetBundle.UnloadAllAssetBundles(true);
        Resources.UnloadUnusedAssets();
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        _container = null;
    }

    private void OnButtonLoadBundleClicked()
    {
        if (!_isBundledSceneLoaded || _isBundleLoaded || _isBundleStartLoaded || _container == null) return;

        _isBundleStartLoaded = true;
        Debug.Log($"Load AssetBundle: {assetBundleUrl}");
        StartCoroutine(LoadBundleAsync());
    }

    private IEnumerator LoadBundleAsync()
    {
        var bundleRequest = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl);
        yield return bundleRequest.SendWebRequest();

        if (bundleRequest.isNetworkError || bundleRequest.isHttpError)
        {
            _isBundleStartLoaded = false;
            _isBundleLoaded = false;
            Debug.Log(bundleRequest.error);
        }
        else
        {
            var bundle = DownloadHandlerAssetBundle.GetContent(bundleRequest);

            if (bundle == null)
            {
                Debug.Log("Failed to load AssetBundle");
                _isBundleStartLoaded = false;
                _isBundleLoaded = false;
                yield break;
            }

            if (_container == null)
            {
                _isBundleStartLoaded = false;
                _isBundleLoaded = false;
                yield break;
            }

            var assetLoaded = bundle.LoadAssetAsync<GameObject>("assets/bundle/prefab/purpledrumset.prefab");
            yield return assetLoaded;

            var prefab = assetLoaded.asset as GameObject;

            if (prefab == null)
            {
                _isBundleStartLoaded = false;
                _isBundleLoaded = false;
                yield break;
            }

            Instantiate(prefab, Vector3.zero, Quaternion.identity).transform.parent = _container.transform;

            var _object = _container.transform.Find("PurpleDrumSet(Clone)");
            _object.localPosition = new Vector3(0, 0, 0);
            _object.localRotation = new Quaternion(0, 0, 0, 0);

            _isBundleStartLoaded = false;
            _isBundleLoaded = true;

            bundle.Unload(false);
        }

        bundleRequest.Dispose();
    }

    private void OnButtonUnLoadBundleClicked()
    {
        if (!_isBundledSceneLoaded || !_isBundleLoaded || _isBundleStartLoaded || _container == null) return;

        Debug.Log("UnLoad AssetBundles");

        var _object = _container.transform.Find("PurpleDrumSet(Clone)").gameObject;
        if (_object) Destroy(_object);
        AssetBundle.UnloadAllAssetBundles(true);

        _isBundleStartLoaded = false;
        _isBundleLoaded = false;
    }
}
