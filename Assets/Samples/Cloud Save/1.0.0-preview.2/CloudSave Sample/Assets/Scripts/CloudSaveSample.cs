using System.Collections.Generic;
using UnityEngine.UI;

namespace UnityEngine.CloudSave.Examples
{

    public class CloudSaveSample : MonoBehaviour, ISyncCallback
    {

        public Text statusMessage;
        public InputField alias;
        public InputField level;
        public GameObject mainPanel;
        public Canvas canvas;
        
        public bool OnConflict(IDataset dataset, IList<SyncConflict> conflicts)
        {
            statusMessage.text = "Handling Sync Conflicts.";
            Debug.Log("OnSyncConflict");
            List<Record> resolvedRecords = new List<Record>();

            foreach (SyncConflict conflictRecord in conflicts)
            {
                // This example resolves all the conflicts using ResolveWithRemoteRecord 
                // Cloudsave provides the following default conflict resolution methods:
                //      ResolveWithRemoteRecord - overwrites the local with remote records
                //      ResolveWithLocalRecord - overwrites the remote with local records
                //      ResolveWithValue - for developer logic  
                resolvedRecords.Add(conflictRecord.ResolveWithRemoteRecord());
            }

            // resolves the conflicts in local storage
            dataset.ResolveConflicts(resolvedRecords);

            // on return true the synchronize operation continues where it left,
            //      returning false cancels the synchronize operation
            return true;
        }

        public void OnError(IDataset dataset, DatasetSyncException syncEx)
        {
            Debug.Log("Sync failed for dataset : " + dataset.Name);
            Debug.LogException(syncEx);

            statusMessage.text = "Syncing to CloudSave failed:" + syncEx.Message;
        }

        public void OnSuccess(IDataset dataset)
        {
            var characterInfo = SaveManager.instance.characterInfo;
            Debug.Log("Successfully synced for dataset: " + dataset.Name);

            if (dataset == characterInfo)
            {
                alias.text = string.IsNullOrEmpty(characterInfo.Get("alias")) ? "Enter your alias" : dataset.Get("alias");
                level.text = string.IsNullOrEmpty(characterInfo.Get("level")) ? "1" : dataset.Get("level");
            }
            statusMessage.text = "Syncing to cloudsave succeeded";
        }
        
        public void OnSaveOfflineClick()
        {
            var characterInfo = SaveManager.instance.characterInfo;
            statusMessage.text = "Saving offline";

            characterInfo.Put("alias", alias.text);
            characterInfo.Put("level", level.text);

            alias.text = string.IsNullOrEmpty(characterInfo.Get("alias")) ? "Enter your alias" : characterInfo.Get("alias");
            level.text = string.IsNullOrEmpty(characterInfo.Get("level")) ? "1" : characterInfo.Get("level");

            statusMessage.text = "Saved offline";
        }
        
        public void OnSyncClick()
        {
            var characterInfo = SaveManager.instance.characterInfo;
            statusMessage.text = "Saving to Cloudsave";
            characterInfo.Put("alias", alias.text);
            characterInfo.Put("level", level.text);
            characterInfo.SynchronizeAsync(this);
        }
        
        public void OnDeleteClick()
        {
            statusMessage.text = "Deleting all local data";
            SaveManager.instance.CloudSave.WipeOut();
            level.text = "1";
            alias.text = "Enter your alias";
            statusMessage.text = "Deleting all local data complete. ";
        }
        
        public void OnCloseClick()
        {
            mainPanel.SetActive(false);
            canvas.sortingOrder = -1;
        }
        
        public void OnStartClick()
        {
            statusMessage.text = "";
            var characterInfo = SaveManager.instance.characterInfo;
            alias.text = string.IsNullOrEmpty(characterInfo.Get("alias")) ? "Enter characterInfo alias" : characterInfo.Get("alias");
            level.text= string.IsNullOrEmpty(characterInfo.Get("level")) ? "1" : characterInfo.Get("level");
            
            mainPanel.SetActive(true);
            canvas.sortingOrder = 1;
        }

    }
}
