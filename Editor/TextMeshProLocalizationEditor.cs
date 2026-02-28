using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using TMPro.Localization;
using System;
using System.Linq;
using System.Reflection;

namespace TMPro.Localization.Editor
{
    /// <summary>
    /// 自定义 TextMeshPro Inspector，集成本地化预览功能
    /// </summary>
    [CustomEditor(typeof(TextMeshProUGUI), true)]
    public class TextMeshProLocalizationEditor : BaseTMPEditorExtension
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            TryAutoInitialize();
        }

        private void TryAutoInitialize()
        {
            if (Localization.Initialized) return;

            // 尝试查找项目中实现了 ILocalizationLoader 的无参构造类
            var loaderType = TypeCache.GetTypesDerivedFrom<ILocalizationLoader>()
                .FirstOrDefault(t => !t.IsAbstract && !t.IsInterface && t.GetConstructor(Type.EmptyTypes) != null);

            if (loaderType != null)
            {
                try
                {
                    var loader = (ILocalizationLoader)Activator.CreateInstance(loaderType);
                    Localization.Initialize(loader);
                    // 尝试加载默认语言（当前系统语言）
                    Localization.SetLanguage(System.Globalization.CultureInfo.CurrentCulture);
                }
                catch (Exception e)
                {
                    // 仅在调试时输出，避免骚扰用户
                    // Debug.LogWarning($"[TMP Localization] Auto-init failed: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 在 Text Input 板块（包含 m_text）绘制完成后调用
        /// </summary>
        protected override void OnAfterDrawTextInput()
        {
            // 获取 m_text 属性
            SerializedProperty textProp = serializedObject.FindProperty("m_text");
            if (textProp == null) return;

            // 在这里插入你的自定义绘制逻辑
            DrawLocalizationPreview(textProp);
        }

        private void DrawLocalizationPreview(SerializedProperty textProp)
        {
            if (!Localization.Initialized)
            {
                TryAutoInitialize();
                if (!Localization.Initialized) return;
            }

            string currentInput = textProp.stringValue;
            if (string.IsNullOrEmpty(currentInput)) return;

            string trimmedInput = currentInput.Trim();
            if (trimmedInput.StartsWith("|"))
            {
                string searchKey = trimmedInput.Substring(1).Trim();

                // 获取所有匹配的条目（ID 匹配或内容匹配）
                var matches = new List<LocalizedTextEntry>();
                foreach (var entry in Localization.GetAllEntries())
                {
                    if (entry.Id.ToString().Contains(searchKey) || entry.Text.Contains(searchKey))
                    {
                        matches.Add(entry);
                    }
                }

                if (matches.Count > 0)
                {
                    EditorGUILayout.Space(2);
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUILayout.LabelField($"找到 {matches.Count} 条匹配内容 (点击选择):", EditorStyles.miniBoldLabel);

                        // 限制显示数量，避免面板过长
                        int displayCount = Mathf.Min((int)matches.Count, 5);
                        for (int i = 0; i < displayCount; i++)
                        {
                            var match = matches[i];
                            if (GUILayout.Button($"ID: {match.Id} | {match.Text}", EditorStyles.miniButtonLeft))
                            {
                                var textComponent = target as TMP_Text;
                                if (textComponent != null)
                                {
                                    var binder = textComponent.GetComponent<LocalizedTMPText>();
                                    if (binder == null)
                                        binder = Undo.AddComponent<LocalizedTMPText>(textComponent.gameObject);

                                    Undo.RecordObject(binder, "Set Localization Id");
                                    binder.Target = textComponent;
                                    binder.TextId = match.Id;
                                    EditorUtility.SetDirty(binder);
                                }

                                textProp.stringValue = match.Text;
                                serializedObject.ApplyModifiedProperties();
                                GUI.FocusControl(null); // 失去焦点以应用修改
                                Repaint(); // 强制重绘以显示更新
                            }
                        }

                        if (matches.Count > 5)
                        {
                            EditorGUILayout.LabelField($"... 以及其他 {matches.Count - 5} 条匹配项", EditorStyles.miniLabel);
                        }
                    }

                    EditorGUILayout.Space(2);
                }
                else if (long.TryParse(searchKey, out long id))
                {
                    // 如果没有列表匹配但输入的是完整 ID
                    string localized = Localization.GetText(id);
                    if (localized != null)
                    {
                        EditorGUILayout.HelpBox($"预览: {localized}", MessageType.Info);
                        if (GUILayout.Button("应用此文本", EditorStyles.miniButton))
                        {
                            var textComponent = target as TMP_Text;
                            if (textComponent != null)
                            {
                                var binder = textComponent.GetComponent<LocalizedTMPText>();
                                if (binder == null)
                                    binder = Undo.AddComponent<LocalizedTMPText>(textComponent.gameObject);

                                Undo.RecordObject(binder, "Set Localization Id");
                                binder.Target = textComponent;
                                binder.TextId = id;
                                EditorUtility.SetDirty(binder);
                            }

                            textProp.stringValue = localized;
                            serializedObject.ApplyModifiedProperties();
                            GUI.FocusControl(null);
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"未找到 ID 为 {id} 的本地化文本", MessageType.Warning);
                    }
                }
            }
        }
    }
}
