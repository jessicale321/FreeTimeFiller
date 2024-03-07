using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;

public static class DataManager
{
    ///-///////////////////////////////////////////////////////////
    /// Save the provided object under the provided string name.
    /// 
    public static async Task SaveData(string dataName, object itemToSave)
    {
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { dataName, itemToSave} });
        Debug.Log("Data Manager saved: " + dataName);
    }

    ///-///////////////////////////////////////////////////////////
    /// Loads and returns data that was saved with provided string. Otherwise, will display that the data couldn't be loaded/found.
    /// 
    public static async Task<T> LoadData<T>(string dataName)
    {
        var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>
        {
            dataName
        });

        if (savedData.TryGetValue(dataName, out var dataLoaded))
        {
            Debug.Log($"Data Manager loaded: {dataName}");
            return dataLoaded.Value.GetAs<T>();
        }
        else
        {
            throw new Exception($"Data associated with '{dataName}' not found or retrieval failed.");
        }
    }

    ///-///////////////////////////////////////////////////////////
    /// Clear all of the data under the provided string name.
    /// 
    public static async Task DeleteAllDataByName(string dataName)
    {
        try
        {
            await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object> {
            { dataName, ""} });
        }
        catch (Exception e)
        {
            Debug.Log($"Could not delete {dataName}: " + e);
        }
    }
}
