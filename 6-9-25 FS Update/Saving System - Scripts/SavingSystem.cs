using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FS_Core
{

    public class SavingSystem : MonoBehaviour
    {
        public static SavingSystem i { get; private set; }
        private void Awake()
        {
            i = this;
        }

        Dictionary<string, object> gameState = new Dictionary<string, object>();

        public void Save(string saveFile)
        {
            CaptureState(gameState);
            SaveFile(saveFile, gameState);
        }

        public void Load(string saveFile)
        {
            gameState = LoadFile(saveFile);
            RestoreState(gameState);
        }

        public void Delete(string saveFile)
        {
            File.Delete(GetPath(saveFile));
        }

        public bool CheckIfSaveExists(string saveFile)
        {
            return File.Exists(GetPath(saveFile));
        }

        // Used to capture states of all savable objects in the game
        private void CaptureState(Dictionary<string, object> state)
        {
            foreach (SavableObject savable in FindObjectsOfType<SavableObject>())
            {
                state[savable.UniqueId] = savable.CaptureState();
            }
        }

        // Used to restore states of all savable objects in the game
        private void RestoreState(Dictionary<string, object> state)
        {
            foreach (SavableObject savable in FindObjectsOfType<SavableObject>())
            {
                string id = savable.UniqueId;
                if (state.ContainsKey(id))
                {
                    savable.RestoreState(state[id]);
                }
            }
        }

        void SaveFile(string saveFile, Dictionary<string, object> state)
        {
            string path = GetPath(saveFile);
            print($"saving to {path}");

            string json = JsonConvert.SerializeObject(state);
            File.WriteAllText(path, json);
        }

        Dictionary<string, object> LoadFile(string saveFile)
        {
            string path = GetPath(saveFile);
            if (!File.Exists(path))
                return new Dictionary<string, object>();

            string json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }

        private string GetPath(string saveFile)
        {
            return Path.Combine(Application.persistentDataPath, saveFile);
        }
    }
}
