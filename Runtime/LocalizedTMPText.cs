using TMPro;
using UnityEngine;

namespace TMPro.Localization
{
    [DisallowMultipleComponent]
    public sealed class LocalizedTMPText : MonoBehaviour
    {
        [SerializeField] private TMP_Text target;
        [SerializeField] private long textId;
        private object[] _lastArgs;

        public TMP_Text Target
        {
            get => target;
            set => target = value;
        }

        public long TextId
        {
            get => textId;
            set => textId = value;
        }

        private void Reset()
        {
            target = GetComponent<TMP_Text>();
        }

        private void Awake()
        {
            if (target == null)
                target = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh(params object[] args)
        {
            if (target == null) return;
            if (textId <= 0) return;

            if (args != null && args.Length > 0)
            {
                _lastArgs = args;
            }

            Localization.SetTextWithId(target, textId, _lastArgs);
        }
    }
}
