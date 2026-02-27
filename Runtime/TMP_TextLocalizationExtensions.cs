using TMPro;

namespace TMPro.Localization
{
    public static class TMP_TextLocalizationExtensions
    {
        public static void SetText(this TMP_Text component, string text)
        {
            if (component == null) return;
            
            // 检查是否是 ID 语法
            if (!string.IsNullOrEmpty(text) && text.StartsWith("|"))
            {
                if (long.TryParse(text.Substring(1), out long id))
                {
                    Localization.SetTextWithId(component, id);
                    return;
                }
            }
            
            // 如果设置普通文本，解除之前的本地化绑定，防止语言切换时被回滚
            Localization.Unbind(component);
            component.text = text;
        }

        public static void SetTextLocalization(this TMP_Text component, long textId, params object[] args)
        {
            Localization.SetTextWithId(component, textId, args);
        }
    }
}