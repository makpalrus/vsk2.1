#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class SeparateMenuScenes
{
    const string MenuPath="Assets/Scenes/MainMenu.unity";
    const string LevelPath="Assets/Scenes/SampleScene.unity";
    static SeparateMenuScenes() { EditorApplication.delayCall += AutoSetup; EditorApplication.playModeStateChanged += state => { if(state==PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall += AutoSetup; }; }
    static void AutoSetup()
    {
        if(!File.Exists("Temp/separate-menu.request") || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete("Temp/separate-menu.request");
        try { Setup(); File.WriteAllText("Temp/separate-menu.result","PASS: MainMenu created; level saved; both scenes enabled; editor starts at MainMenu."); }
        catch(Exception e) { File.WriteAllText("Temp/separate-menu.result",e.ToString()); Debug.LogWarning(e); }
    }
    [MenuItem("Game/Mobile UI/Create separate main menu scene")]
    public static void Setup()
    {
        if(File.Exists(MenuPath)) throw new InvalidOperationException("MainMenu already exists; existing scenes will not be overwritten.");
        var level=SceneManager.GetSceneByPath(LevelPath);
        if(!level.IsValid() || !level.isLoaded) level=EditorSceneManager.OpenScene(LevelPath);
        var ui=level.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MobileGameUI>(true)).Single();
        Directory.CreateDirectory("Documentation/Backups");
        File.Copy(LevelPath,"Documentation/Backups/SampleScene.before-separate-menu.unity.txt",true);
        var menuScene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        var canvas=UnityEngine.Object.Instantiate(ui.gameObject);
        canvas.name="Main Menu Canvas";
        SceneManager.MoveGameObjectToScene(canvas,menuScene);
        var copiedUI=canvas.GetComponent<MobileGameUI>();
        UnityEngine.Object.DestroyImmediate(copiedUI);
        foreach(Transform child in canvas.transform.Cast<Transform>().ToArray())
            if(child.name!="Main Menu") UnityEngine.Object.DestroyImmediate(child.gameObject);
        var menu=canvas.transform.Find("Main Menu").gameObject;
        menu.SetActive(true);
        menu.GetComponent<Image>().color=new Color32(18,27,45,255);
        var controller=canvas.AddComponent<MainMenuController>();
        var buttons=canvas.GetComponentsInChildren<Button>(true);
        controller.playButton=buttons.Single(b=>b.name=="Play");
        controller.quitButton=buttons.Single(b=>b.name=="Quit");
        controller.levelScene="SampleScene";
        var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
        SceneManager.MoveGameObjectToScene(camera,menuScene);
        camera.tag="MainCamera"; camera.transform.position=new Vector3(0,0,-10);
        camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;
        camera.GetComponent<Camera>().backgroundColor=new Color32(18,27,45,255);
        var es=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(es,menuScene);
        es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        UnityEngine.Object.DestroyImmediate(ui.mainMenu);
        ui.mainMenu=null; ui.playButton=null; ui.quitButton=null;
        ui.hud.SetActive(true); ui.pauseMenu.SetActive(false); ui.gameOver.SetActive(false);
        EditorSceneManager.MarkSceneDirty(level);
        EditorSceneManager.SaveScene(level);
        EditorSceneManager.SaveScene(menuScene,MenuPath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(MenuPath,true),new EditorBuildSettingsScene(LevelPath,true)};
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuPath);
        AssetDatabase.SaveAssets();
        EditorSceneManager.CloseScene(level,true);
        SceneManager.SetActiveScene(menuScene);
    }
}
#endif

