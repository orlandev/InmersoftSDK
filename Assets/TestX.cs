using InmersoftSDK.Localization;
using UnityEngine;

public class TestX : MonoBehaviour
{
    // Start is called before the first frame update
    public void ButtonCall()
    {
        L18N.Instance.MakeTranslation("EN");
    }
}