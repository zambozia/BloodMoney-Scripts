using FS_Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FS_Core
{

    public class ObjectStateSaver : MonoBehaviour, ISavable
    {
        [SerializeField] List<GameObject> objectsToSave = new List<GameObject>();

        public object CaptureState()
        {
            var saveData = new ObjectStateSaveData();
            saveData.objectActiveStates = new List<bool>();

            foreach (var item in objectsToSave)
                saveData.objectActiveStates.Add(item.activeSelf);

            return saveData;
        }

        public Type GetSavaDataType()
        {
            return typeof(ObjectStateSaveData);
        }

        public void RestoreState(object state)
        {
            var saveData = state as ObjectStateSaveData;
            for (int i = 0; i < saveData.objectActiveStates.Count; i++)
            {
                if (i < objectsToSave.Count)
                    objectsToSave[i].SetActive(saveData.objectActiveStates[i]);
            }
        }

        [System.Serializable]
        public class ObjectStateSaveData
        {
            public List<bool> objectActiveStates;
        }
    }
}
