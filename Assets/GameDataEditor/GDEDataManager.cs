using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor.MiniJSON;

namespace GameDataEditor
{
    public class GDEDataManager {
        #region GDEDataManager Instance
        private bool isInitialized = false;
        private static GDEDataManager _instance = null;
        public static GDEDataManager Instance
        {
            private set { _instance = value; }
            get
            {
                if (_instance == null)
                {
                    _instance = new GDEDataManager();
                }
                return _instance;
            }
        }
        #endregion

        #region Constructor
        private GDEDataManager()
        {

        }
        #endregion

        #region Data Collections
        private Dictionary<string, object> dataDictionary = null;
        #endregion

        #region Init Methods
        // Loads the data file from path in PlayerSettings
        public bool Init()
        {
            return true;
        }

        // Loads the specified data file
        public bool Init(string filePath)
        {
            bool result = true;

            if (isInitialized)
                return result;

            try
            {
                string json = File.ReadAllText(filePath);
                dataDictionary = Json.Deserialize(json) as Dictionary<string, object>;
                isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                result = false;
            }
            return result;
        }
        #endregion

        #region Data Access Methods
        public bool Get(string key, out Dictionary<string, object> data)
        {
            if (dataDictionary == null)
            {
                data = null;
                return false;
            }

            bool result = true;
            object temp;

            result = dataDictionary.TryGetValue(key, out temp);
            data = temp as Dictionary<string, object>;

            return result;
        }
        #endregion
    }
}
