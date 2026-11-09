using UnityEngine;
[RequireComponent(typeof(AzureSTT))]
public class AzureConfigLoader : ConfigLoader
{
    private void Awake()
    {
        onGetConfigRaw.AddListener(
            (configRaw) => {
                var Config = JsonUtility.FromJson<AzureSTT.AzureSettings>(configRaw);
                var azure = GetComponent<AzureSTT>();
                azure.language = Config.Language;
            });
    }
}
