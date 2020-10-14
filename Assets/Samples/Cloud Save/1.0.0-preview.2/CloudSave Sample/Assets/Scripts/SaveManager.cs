using UnityEngine;
using UnityEngine.CloudSave;
using UnityEngine.PlayerIdentity;
using UnityEngine.SceneManagement;
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance { get; private set; }

    public IDataset characterInfo  { get; private set; }
    
    private CloudSave _cloudSave;

    public CloudSave CloudSave
    {
        get
        {
            if (_cloudSave == null)
            {
                _cloudSave = new CloudSave(PlayerIdentityManager.Current);
            }
            return _cloudSave;
        }
    }
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            CloudSaveInitializer.AttachToGameObject(gameObject);
            characterInfo = CloudSave.OpenOrCreateDataset("CharacterInfo");
        }
        else
        {
            if (this != instance)
            {
                Destroy(this);
            }
        }
    }
    
}
