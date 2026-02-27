using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;
using TMPro;

namespace TMPro.Localization
{
    public static class Localization
    {
        private struct BoundData
        {
            public long TextId;
            public object[] Args;
        }

        private static bool _initialized;
        public static bool Initialized => _initialized;
        private static Dictionary<long, LocalizedTextEntry> _textCache = new Dictionary<long, LocalizedTextEntry>();
        private static Dictionary<int, WeakReference<TMP_Text>> _registeredComponents = new Dictionary<int, WeakReference<TMP_Text>>();
        private static Dictionary<int, BoundData> _componentIdMap = new Dictionary<int, BoundData>();
        private static ILocalizationLoader _loader;
        private static CultureInfo _currentCulture = CultureInfo.CurrentCulture;
        private static SynchronizationContext _unityContext;
        private static int _mainThreadId;

        public static void Initialize(ILocalizationLoader loader)
        {
            if (_unityContext == null)
            {
                _unityContext = SynchronizationContext.Current;
                _mainThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            _loader = loader;
            RefreshCache();
            _initialized =  true;
        }

        public static void SetLanguage(SystemLanguage language)
        {
            string cultureCode = GetCultureCode(language);
            SetLanguage(new CultureInfo(cultureCode));
        }

        public static void SetLanguage(CultureInfo culture)
        {
            if (_unityContext != null && Thread.CurrentThread.ManagedThreadId != _mainThreadId)
            {
                _unityContext.Post(_ => SetLanguage(culture), null);
                return;
            }

            _currentCulture = culture;
            
            if (_loader != null)
            {
                var newCache = _loader.Load(_currentCulture);
                _textCache = newCache;
            }

            RefreshAllComponents();
        }

        public static void SetTextWithId(TMP_Text component, long textId, params object[] args)
        {
            if (component == null) return;

            // 如果有本地化组件，同步 ID 并由组件负责（为了保留其参数缓存逻辑）
            if (component.TryGetComponent<LocalizedTMPText>(out var localizedText))
            {
                bool idChanged = localizedText.TextId != textId;
                bool argsProvided = args != null && args.Length > 0;
                
                if (idChanged || argsProvided)
                {
                    localizedText.TextId = textId;
                    localizedText.Refresh(args);
                    return;
                }
            }

            Bind(component, textId, args);
            RegisterComponent(component);

            if (_textCache.TryGetValue(textId, out var entry))
            {
                string formattedText = args != null && args.Length > 0 
                    ? string.Format(entry.Text, args) 
                    : entry.Text;
                
                // 这里还可以调用 SpriteTagParser 处理图文混排，后面实现
                component.text = formattedText;
            }
            else
            {
                component.text = $"[Missing ID: {textId}]";
            }
        }

        public static string GetText(long textId)
        {
            if (_textCache.TryGetValue(textId, out var entry))
            {
                return entry.Text;
            }
            return null;
        }

        public static IEnumerable<LocalizedTextEntry> GetAllEntries()
        {
            return _textCache.Values;
        }

        private static void RefreshCache()
        {
            if (_loader != null)
            {
                _textCache = _loader.Load(_currentCulture);
            }
        }

        private static void RegisterComponent(TMP_Text component)
        {
            if (component == null) return;
            int instanceId = component.GetInstanceID();
            
            if (_registeredComponents.ContainsKey(instanceId))
            {
                if (_registeredComponents[instanceId].TryGetTarget(out _))
                {
                    return;
                }
            }
            
            _registeredComponents[instanceId] = new WeakReference<TMP_Text>(component);
        }

        private static void RefreshAllComponents()
        {
            List<int> toRemove = new List<int>();
            
            foreach (var kvp in _registeredComponents)
            {
                if (kvp.Value.TryGetTarget(out var component))
                {
                    TryRefreshComponent(component);
                }
                else
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (var id in toRemove)
            {
                _registeredComponents.Remove(id);
                _componentIdMap.Remove(id);
            }
        }

        public static void Bind(TMP_Text component, long textId, params object[] args)
        {
            if (component == null) return;
            if (textId <= 0) return;
            _componentIdMap[component.GetInstanceID()] = new BoundData { TextId = textId, Args = args };
        }

        public static void Unbind(TMP_Text component)
        {
            if (component == null) return;
            int instanceId = component.GetInstanceID();
            _componentIdMap.Remove(instanceId);
            _registeredComponents.Remove(instanceId);
        }

        public static void TryRefreshComponent(TMP_Text component)
        {
            if (component == null) return;
            
            // 优先检查是否有挂载 LocalizedTMPText 组件，如果有，由它负责刷新（保留参数）
            if (component.TryGetComponent<LocalizedTMPText>(out var localizedText))
            {
                localizedText.Refresh();
                return;
            }

            int instanceId = component.GetInstanceID();
            if (_componentIdMap.TryGetValue(instanceId, out var boundData) && boundData.TextId > 0)
            {
                SetTextWithId(component, boundData.TextId, boundData.Args);
                return;
            }

            string currentText = component.text;
            if (!string.IsNullOrEmpty(currentText) && currentText.Length > 1 && currentText[0] == '|')
            {
                if (long.TryParse(currentText.Substring(1), out long id) && id > 0)
                {
                    Bind(component, id);
                    SetTextWithId(component, id);
                }
            }
        }

        private static string GetCultureCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Chinese: return "zh-CN";
                case SystemLanguage.English: return "en-US";
                default: return "en-US";
            }
        }

        // 用于编辑器下强制刷新缓存
        public static void ClearCache()
        {
            _textCache.Clear();
            RefreshCache();
            RefreshAllComponents();
        }
    }
}
