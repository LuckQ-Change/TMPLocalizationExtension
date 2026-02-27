using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TMPro.Localization.Editor
{
    /// <summary>
    /// TMP 编辑器扩展基类
    /// </summary>
    public abstract class BaseTMPEditorExtension : UnityEditor.Editor
    {
        private UnityEditor.Editor defaultEditor;
        private static bool m_Initialized = false;
        private static System.Type m_TMP_TextEditorType = null;
        private static System.Type m_TMP_UIPanelEditorType = null;
        private static System.Type m_TMP_PanelEditorType = null;

        protected virtual void OnEnable()
        {
            if (target == null) return;

            if (!m_Initialized)
            {
                InitializeTMPEditorTypes();
            }

        }

        protected virtual void OnDisable()
        {
            if (defaultEditor != null)
            {
                // 尝试调用 OnDisable
                var type = defaultEditor.GetType();
                var onDisableMethod = type.GetMethod("OnDisable", 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (onDisableMethod != null)
                {
                    try
                    {
                        onDisableMethod.Invoke(defaultEditor, null);
                    }
                    catch { }
                }

                DestroyImmediate(defaultEditor);
                defaultEditor = null;
            }
        }

        /// <summary>
        /// 初始化 TMP 编辑器类型（只需要执行一次）
        /// </summary>
        private static void InitializeTMPEditorTypes()
        {
            // 方法1：通过已知的程序集名称查找
            string[] assemblyNames = new string[]
            {
                "Unity.TextMeshPro.Editor",
                "Unity.TextMeshPro",
                "Unity.TextMeshPro.Editor.Legacy",
                "TextMeshPro-Editor",
                "Assembly-CSharp-Editor" // 某些版本可能在这里
            };

            // 要查找的编辑器类名（不同版本可能不同）
            string[] typeNames = new string[]
            {
                "TMPro.EditorUtilities.TMP_BaseEditorPanel",
                "TMPro.EditorUtilities.TMP_EditorPanel",
                "TMPro.EditorUtilities.TMP_EditorPanelUI",
                "TMPro.EditorUtilities.TextMeshProUGUIEditor",
                "TMPro.EditorUtilities.TextMeshProEditor",
                "TMPro.EditorUtilities.TMP_TextEditor"
            };

            // 遍历所有程序集
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                // 检查是否可能是 TMP 相关的程序集
                bool isTMPAssembly = false;
                foreach (var name in assemblyNames)
                {
                    if (assembly.FullName.Contains(name))
                    {
                        isTMPAssembly = true;
                        break;
                    }
                }

                if (!isTMPAssembly && !assembly.FullName.Contains("TextMeshPro"))
                    continue;

                // 在程序集中查找类型
                foreach (var typeName in typeNames)
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        if (typeName.Contains("PanelUI") || typeName.Contains("UGUI"))
                            m_TMP_UIPanelEditorType = type;
                        else if (typeName.Contains("Panel") && !typeName.Contains("UI"))
                            m_TMP_PanelEditorType = type;
                        else if (typeName.Contains("Base"))
                            m_TMP_TextEditorType = type;
                    }
                }
            }

            // 方法2：如果没找到，通过 CustomEditor 特性查找
            if (m_TMP_TextEditorType == null)
            {
                FindTMPEditorByCustomAttribute();
            }

            m_Initialized = true;
        }

        /// <summary>
        /// 通过 CustomEditor 特性查找 TMP 编辑器
        /// </summary>
        private static void FindTMPEditorByCustomAttribute()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.Contains("TextMeshPro") &&
                    !assembly.FullName.Contains("Unity.TextMeshPro"))
                    continue;

                foreach (var type in assembly.GetTypes())
                {
                    // 检查是否是 Editor 类型
                    if (!typeof(UnityEditor.Editor).IsAssignableFrom(type))
                        continue;

                    // 获取 CustomEditor 特性
                    var attributes = type.GetCustomAttributes(typeof(CustomEditor), true);
                    foreach (CustomEditor attr in attributes)
                    {
                        if (attr.GetType() == typeof(TextMeshProUGUI))
                        {
                            m_TMP_UIPanelEditorType = type;
                        }
                        else if (attr.GetType() == typeof(TextMeshPro))
                        {
                            m_TMP_PanelEditorType = type;
                        }
                        else if (attr.GetType() == typeof(TMP_Text))
                        {
                            m_TMP_TextEditorType = type;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 创建默认编辑器实例
        /// </summary>
        private void CreateDefaultEditor()
        {
            if (defaultEditor != null) return;
            if (target == null) return;

            try
            {
                System.Type targetType = target.GetType();
                System.Type editorType = GetEditorTypeForTarget(targetType);

                if (editorType != null)
                {
                    // 过滤掉无效的目标对象
                    var validTargets = new System.Collections.Generic.List<Object>();
                    foreach (var t in targets)
                    {
                        if (t != null) validTargets.Add(t);
                    }

                    if (validTargets.Count > 0)
                    {
                        defaultEditor = CreateEditor(validTargets.ToArray(), editorType);
                        
                        // 仅在必要时进行额外初始化检查
                        InitializeTMPBaseEditor(defaultEditor);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"创建默认 TMP Editor 失败: {e.Message}");
                defaultEditor = null;
            }
        }

        /// <summary>
        /// 专门处理 TMP_BaseEditorPanel 的初始化
        /// </summary>
        private void InitializeTMPBaseEditor(UnityEditor.Editor editor)
        {
            var type = editor.GetType();

            // 查找并调用 Init 或类似的方法
            var initMethod = type.GetMethod("Init",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (initMethod != null)
            {
                try
                {
                    initMethod.Invoke(editor, null);
                }
                catch
                {
                }
            }

            // 确保 m_SerializedObject 被初始化
            var serializedObjectField = type.GetField("m_SerializedObject",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (serializedObjectField != null)
            {
                try
                {
                    if (serializedObjectField.GetValue(editor) == null)
                    {
                        // 尝试创建新的 SerializedObject
                        var so = new SerializedObject(targets);
                        serializedObjectField.SetValue(editor, so);
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// 根据目标类型获取对应的编辑器类型
        /// </summary>
        private System.Type GetEditorTypeForTarget(System.Type targetType)
        {
            if (targetType == typeof(TextMeshProUGUI))
                return m_TMP_UIPanelEditorType ?? m_TMP_TextEditorType;
            else if (targetType == typeof(TextMeshPro))
                return m_TMP_PanelEditorType ?? m_TMP_TextEditorType;
            else if (typeof(TMP_Text).IsAssignableFrom(targetType))
                return m_TMP_TextEditorType;

            return null;
        }

        protected virtual void OnDestroy()
        {
            if (defaultEditor != null)
            {
                DestroyImmediate(defaultEditor);
                defaultEditor = null;
            }
        }

        /// <summary>
        /// 自定义绘制区域，子类实现
        /// </summary>
        protected virtual void DrawCustomInspector() { }

        public override void OnInspectorGUI()
        {
            if (defaultEditor == null)
            {
                CreateDefaultEditor();
            }
            
            if (target == null) return;

            serializedObject.Update();

            if (defaultEditor != null)
                DrawTMPInspector();
            else
                DrawCustomInspector();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnAfterDrawTextInput() { }

        protected virtual void OnAfterDrawMainSettings() { }

        protected virtual void OnAfterDrawExtraSettings() { }

        protected virtual void DrawTMPInspector()
        {
            var type = defaultEditor.GetType();
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            var drawTextInputMethod = type.GetMethod("DrawTextInput", flags);
            var drawMainSettingsMethod = type.GetMethod("DrawMainSettings", flags);
            var drawExtraSettingsMethod = type.GetMethod("DrawExtraSettings", flags);

            if (drawTextInputMethod == null || drawMainSettingsMethod == null || drawExtraSettingsMethod == null)
            {
                DrawCustomInspector();
                defaultEditor.OnInspectorGUI();
                return;
            }

            var isMixSelectionTypesMethod = type.GetMethod("IsMixSelectionTypes", flags);
            if (isMixSelectionTypesMethod != null)
            {
                try
                {
                    if ((bool)isMixSelectionTypesMethod.Invoke(defaultEditor, null))
                        return;
                }
                catch
                {
                }
            }

            defaultEditor.serializedObject.Update();

            drawTextInputMethod.Invoke(defaultEditor, null);
            OnAfterDrawTextInput();

            drawMainSettingsMethod.Invoke(defaultEditor, null);
            OnAfterDrawMainSettings();

            drawExtraSettingsMethod.Invoke(defaultEditor, null);
            OnAfterDrawExtraSettings();

            EditorGUILayout.Space();

            if (defaultEditor.serializedObject.ApplyModifiedProperties() || IsHavePropertiesChanged(type))
            {
                try
                {
                    var textComponentField = type.GetField("m_TextComponent", flags);
                    if (textComponentField != null)
                    {
                        var textComponent = textComponentField.GetValue(defaultEditor);
                        if (textComponent != null)
                        {
                            var havePropertiesChangedProp = textComponent.GetType().GetProperty("havePropertiesChanged", flags);
                            if (havePropertiesChangedProp != null)
                                havePropertiesChangedProp.SetValue(textComponent, true);
                        }
                    }

                    var havePropertiesChangedField = type.GetField("m_HavePropertiesChanged", flags);
                    if (havePropertiesChangedField != null)
                        havePropertiesChangedField.SetValue(defaultEditor, false);

                    EditorUtility.SetDirty(target);
                }
                catch
                {
                }
            }
        }

        private bool IsHavePropertiesChanged(System.Type type)
        {
            try
            {
                var field = type.GetField("m_HavePropertiesChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    return (bool)field.GetValue(defaultEditor);
            }
            catch
            {
            }

            return false;
        }

        /// <summary>
        /// 转发 Scene 视图绘制事件，确保 TMP 的 handle 能正常工作
        /// </summary>
        protected virtual void OnSceneGUI()
        {
            if (defaultEditor != null)
            {
                // OnSceneGUI 是通过消息机制调用的，Editor 类中没有定义该虚方法，需要反射调用
                var method = defaultEditor.GetType().GetMethod("OnSceneGUI", 
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (method != null)
                {
                    method.Invoke(defaultEditor, null);
                }
            }
        }
    }
}
