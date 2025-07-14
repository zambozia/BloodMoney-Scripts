using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SearchUtility
{
    public static GameObject[] FindAllObjectsByName(string targetName, bool atCityOnly = true)
    {

        Transform searchRoot = null;

        if (atCityOnly && GameObject.Find("City-Maker") != null)
            searchRoot = GameObject.Find("City-Maker").transform;


        List<GameObject> foundObjects = new List<GameObject>();

        if (searchRoot != null)
        {
            SearchRecursively(searchRoot, targetName, foundObjects);
        }
        else
        {
             foreach (GameObject rootObj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                SearchRecursively(rootObj.transform, targetName, foundObjects);
            }
        }

        return foundObjects.ToArray();
    }

    private static void SearchRecursively(Transform current, string targetName, List<GameObject> results)
    {
        if (current.name == targetName)
        {
            results.Add(current.gameObject);
        }

        foreach (Transform child in current)
        {
            SearchRecursively(child, targetName, results);
        }
    }
}
