using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Core.Scripts
{
    public class Downloader : StaticReadyChecker<Downloader>
    {
        private class ActiveDownload
        {
            public Coroutine Coroutine;
            public UnityWebRequest Request;
            public event Action<UnityWebRequest> OnResult;

            public void Invoke()
            {
                OnResult?.Invoke(Request);
            }
        }
        
        private readonly Dictionary<string, ActiveDownload> _downloads = new ();
        
        private void Awake()
        {
            DontDestroyOnLoad(this);
            SetReady(this);
        }
        
        public void Download(string url, Action<UnityWebRequest> onDownloadAction)
        {
            if (_downloads.TryGetValue(url, out var downloadProcess))
            {
                if (onDownloadAction != null)
                {
                    if (downloadProcess.Request != null && downloadProcess.Request.isDone)
                    {
                        onDownloadAction.Invoke(downloadProcess.Request);
                    }
                    else
                    {
                        downloadProcess.OnResult += onDownloadAction;
                    }
                }
                
                return;
            }
            
            downloadProcess = new ActiveDownload();
            _downloads.Add(url, downloadProcess);
            if (onDownloadAction != null)
            {
                downloadProcess.OnResult += onDownloadAction;
            }

            downloadProcess.Coroutine = StartCoroutine(DownloadCoroutine(url));
        }

        private IEnumerator DownloadCoroutine(string url)
        {
            if (!_downloads.TryGetValue(url, out var downloadProcess))
            {
                yield break;
            }
            
            downloadProcess.Request = UnityWebRequest.Get(url);
            yield return downloadProcess.Request.SendWebRequest();
            
            switch (downloadProcess.Request.result)
            {
                case UnityWebRequest.Result.Success:
                    break;
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.InProgress:
                default:
                    Debug.LogError($"Download failed url={url}");
                    break;
            }

            downloadProcess.Coroutine = null;
            downloadProcess.Invoke();
        }
    }
}