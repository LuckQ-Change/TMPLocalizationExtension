using System.Collections;
using System.Collections.Generic;
using Config;
using TMPro;
using TMPro.Localization;
using UnityEngine;

public class LocalizationDemoTest : MonoBehaviour
{
    public TMP_Text Test_Text;
    // Start is called before the first frame update
    void Start()
    {
        Test_Text.SetTextLocalization(LanguageTable.Test10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnChangeChinese()
    {
        Localization.SetLanguage(SystemLanguage.Chinese);
    }

    public void OnChangeEnglish()
    {
        Localization.SetLanguage(SystemLanguage.English);
    }
}
