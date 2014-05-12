using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using GameDataEditor.MiniJSON;
using GameDataEditor.GDEExtensionMethods;

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
        private Dictionary<string, List<string>> dataKeysBySchema = null;

        public Dictionary<string, object> DataDictionary
        {
            get
            {
                return dataDictionary;
            }
        }
        #endregion

        #region Properties
        private string _dataFilePath;
        public string DataFilePath
        {
            private set { _dataFilePath = value; }
            get { return _dataFilePath; }
        }
        #endregion

        #region Init Methods
        /// <summary>
        /// Loads the specified data file
        /// </summary>
        /// <param name="filePath">Data file path.</param>
        public bool Init(string filePath)
        {
            bool result = true;

            if (isInitialized)
                return result;

            try
            {
                DataFilePath = filePath;
               
                TextAsset dataAsset = Resources.Load(DataFilePath) as TextAsset;
                string json = dataAsset.text;
                dataDictionary = Json.Deserialize(json) as Dictionary<string, object>;

                BuildDataKeysBySchemaList();

                isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Builds the data keys by schema list for lookups by schema.
        /// </summary>
        private void BuildDataKeysBySchemaList()
        {
            dataKeysBySchema = new Dictionary<string, List<string>>();
            foreach(KeyValuePair<string, object> pair in dataDictionary)
            {
                if (pair.Key.StartsWith(GDEConstants.SchemaPrefix))
                    continue;

                // Get the schema for the current data set
                string schema;
                Dictionary<string, object> currentDataSet = pair.Value as Dictionary<string, object>;
                currentDataSet.TryGetString(GDEConstants.SchemaKey, out schema);

                // Add it to the list of data keys by type
                List<string> dataKeyList;
                if (dataKeysBySchema.TryGetValue(schema, out dataKeyList))                
                {
                    dataKeyList.Add(pair.Key);
                }
                else
                {
                    dataKeyList = new List<string>();
                    dataKeyList.Add(pair.Key);
                    dataKeysBySchema.Add(schema, dataKeyList);
                }
            }
        }
        #endregion

        #region Data Access Methods
        /// <summary>
        /// Get the data associated with the specified key in a Dictionar<string, object>
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="data">Data</param>
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

        /// <summary>
        /// Returns a subset of the data containing only data sets by the given schema
        /// </summary>
        /// <returns><c>true</c>, if the given schema exists <c>false</c> otherwise.</returns>
        /// <param name="type">Schema.</param>
        /// <param name="data">Subset of the Data Set list containing entries with the specified schema.</param>
        public bool GetAllDataBySchema(string schema, out Dictionary<string, object> data)
        {
            if (dataDictionary == null)
            {
                data = null;
                return false;
            }

            List<string> dataKeys;
            bool result = true;
            data = new Dictionary<string, object>();

            if (dataKeysBySchema.TryGetValue(schema, out dataKeys))
            {
                foreach(string dataKey in dataKeys)
                {
                    Dictionary<string, object> currentDataSet;
                    if (Get(dataKey, out currentDataSet))
                        data.Add(dataKey.Clone().ToString(), currentDataSet.DeepCopy());
                }
            }
            else
               result = false;

            return result;
        }

        /// <summary>
        /// Gets all data keys by schema.
        /// </summary>
        /// <returns><c>true</c>, if the given schema exists <c>false</c> otherwise.</returns>
        /// <param name="schema">Schema.</param>
        /// <param name="dataKeys">Data Key List.</param>
        public bool GetAllDataKeysBySchema(string schema, out List<string> dataKeys)
        {
            if (dataDictionary == null)
            {
                dataKeys = null;
                return false;
            }

            return dataKeysBySchema.TryGetValue(schema, out dataKeys);
        }
        #endregion
    }
}
