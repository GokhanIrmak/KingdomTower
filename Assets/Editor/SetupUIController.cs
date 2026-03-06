using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using KingdomTower;

public class SetupUIController
{
    [MenuItem("Tools/Setup GameUIController")]
    public static void Setup()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/SampleScene.unity");
        
        GameObject gmObj = GameObject.Find("_GameManager");
        if (gmObj != null)
        {
            if (gmObj.GetComponent<GameUIController>() == null)
            {
                gmObj.AddComponent<GameUIController>();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Successfully added GameUIController to _GameManager and saved the scene.");
            }
            else
            {
                Debug.Log("GameUIController already exists on _GameManager");
            }
        }
        else
        {
            GameObject newGm = new GameObject("_GameManager");
            newGm.AddComponent<GameManager>();
            newGm.AddComponent<GameUIController>();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Created _GameManager and added components.");
        }
    }
}
