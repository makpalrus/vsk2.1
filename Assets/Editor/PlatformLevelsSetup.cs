#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[InitializeOnLoad]
public static class PlatformLevelsSetup
{
    const string First="Assets/Scenes/SampleScene.unity", Second="Assets/Scenes/Level2.unity";
    static PlatformLevelsSetup()
    {
        EditorApplication.delayCall+=Auto;
        EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredEditMode) EditorApplication.delayCall+=Auto;};
    }
    static void Auto()
    {
        if(!File.Exists("Temp/platform-levels.request") || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete("Temp/platform-levels.request");
        try { Build(); File.WriteAllText("Temp/platform-levels.result","PASS: 3 platforms with coins; locked finish; separate Level2 with purple player and night theme; build scenes updated."); }
        catch(Exception e) { File.WriteAllText("Temp/platform-levels.result",e.ToString()); Debug.LogWarning(e); }
    }
    [MenuItem("Game/Levels/Create platform levels")]
    public static void Build()
    {
        if(File.Exists(Second)) throw new InvalidOperationException("Level2 already exists; existing work will not be overwritten.");
        Directory.CreateDirectory("Documentation/Backups");
        File.Copy(First,"Documentation/Backups/SampleScene.before-platforms.unity.txt",true);
        var loaded=SceneManager.GetSceneByPath(First);
        var scene=loaded.IsValid() && loaded.isLoaded ? loaded : EditorSceneManager.OpenScene(First);
        SceneManager.SetActiveScene(scene);
        var ui=UnityEngine.Object.FindFirstObjectByType<MobileGameUI>();
        var player=ui.player;
        // Spawn on the ground so the new central platform never intersects the hero.
        player.transform.position=new Vector3(-.03f,-3.3f,0);
        var sheet=AssetDatabase.LoadAllAssetsAtPath("Assets/Simple 2D Platformer BE2/Sprites/Platforms.png").OfType<Sprite>().ToArray();
        var coins=AssetDatabase.LoadAllAssetsAtPath("Assets/Simple 2D Platformer BE2/Sprites/Coins.png").OfType<Sprite>().Where(s=>s.name.StartsWith("Gold")).OrderBy(s=>s.name).ToArray();
        var root=new GameObject("Raised Platforms");
        Vector2[] places={new Vector2(-3,-2.1f),new Vector2(0,-.4f),new Vector2(3,1.3f)};
        for(int i=0;i<places.Length;i++) AddPlatform(root.transform,places[i],i,sheet,coins);
        var gate=new GameObject("Finish Flag",typeof(SpriteRenderer),typeof(BoxCollider2D),typeof(FinishGate));
        gate.transform.position=new Vector3(5.3f,-3.05f,0);
        gate.GetComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAllAssetsAtPath("Assets/Simple 2D Platformer BE2/Sprites/Objects.png").OfType<Sprite>().Single(s=>s.name=="Finish");
        gate.GetComponent<SpriteRenderer>().sortingOrder=8;
        gate.transform.localScale=Vector3.one*1.5f;
        var trigger=gate.GetComponent<BoxCollider2D>(); trigger.isTrigger=true; trigger.size=new Vector2(.85f,1.25f);
        var label=new GameObject("Finish Hint",typeof(TextMesh)); label.transform.SetParent(gate.transform,false); label.transform.localPosition=new Vector3(-.1f,.8f,0);
        var text=label.GetComponent<TextMesh>(); text.text="COLLECT ALL COINS"; text.anchor=TextAnchor.MiddleCenter; text.alignment=TextAlignment.Center; text.characterSize=.055f; text.fontSize=48; text.color=new Color32(18,27,45,255);
        text.GetComponent<MeshRenderer>().sortingOrder=12;
        gate.GetComponent<FinishGate>().message=text;
        gate.GetComponent<FinishGate>().nextLevel="Level2";
        var counterPanel=ui.counter.transform.parent.GetComponent<RectTransform>(); counterPanel.sizeDelta=new Vector2(330,110);
        ui.counter.rectTransform.sizeDelta=new Vector2(210,80); ui.counter.rectTransform.anchoredPosition=new Vector2(35,0); ui.counter.fontSize=40;
        counterPanel.Find("Coin").GetComponent<RectTransform>().anchoredPosition=new Vector2(-112,0);
        ui.counter.text="0 / 10";
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        EditorSceneManager.SaveScene(scene,Second,true);
        var second=EditorSceneManager.OpenScene(Second);
        ui=UnityEngine.Object.FindFirstObjectByType<MobileGameUI>();
        ui.player.GetComponent<SpriteRenderer>().color=new Color32(180,130,255,255);
        Camera.main.backgroundColor=new Color32(29,30,67,255);
        foreach(var tiles in UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            if(tiles.name=="Ground") tiles.color=new Color32(153,122,215,255);
        var platforms=GameObject.Find("Raised Platforms");
        Vector2[] secondPlaces={new Vector2(-3.7f,-1.9f),new Vector2(-.3f,-.1f),new Vector2(3.2f,1.7f)};
        for(int i=0;i<3;i++)
        {
            var platform=platforms.transform.GetChild(i);
            platform.position=secondPlaces[i];
            foreach(var sr in platform.GetComponentsInChildren<SpriteRenderer>()) if(!sr.GetComponent<CollectibleCoin>()) sr.color=new Color32(175,140,235,255);
        }
        var secondGate=UnityEngine.Object.FindFirstObjectByType<FinishGate>(); secondGate.nextLevel=""; secondGate.message.color=Color.white;
        EditorSceneManager.MarkSceneDirty(second); EditorSceneManager.SaveScene(second);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",true),new EditorBuildSettingsScene(First,true),new EditorBuildSettingsScene(Second,true)};
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenu.unity");
        AssetDatabase.SaveAssets(); EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
    }
    static void AddPlatform(Transform root,Vector2 pos,int index,Sprite[] sheet,Sprite[] coins)
    {
        var go=new GameObject("Platform "+(index+1),typeof(BoxCollider2D)); go.layer=6; go.transform.SetParent(root); go.transform.position=pos;
        go.GetComponent<BoxCollider2D>().size=new Vector2(3,.5f); go.GetComponent<BoxCollider2D>().offset=new Vector2(0,-.25f);
        string[] names={"Single Left","Single","Single Right"};
        for(int i=0;i<3;i++) { var tile=new GameObject("Tile "+i,typeof(SpriteRenderer)); tile.layer=6; tile.transform.SetParent(go.transform,false); tile.transform.localPosition=new Vector3(i-1,-.5f,0); tile.GetComponent<SpriteRenderer>().sprite=sheet.Single(s=>s.name==names[i]); tile.GetComponent<SpriteRenderer>().sortingOrder=2; }
        var coin=new GameObject("Platform Coin",typeof(SpriteRenderer),typeof(CircleCollider2D),typeof(CollectibleCoin));
        coin.transform.SetParent(go.transform,false); coin.transform.localPosition=new Vector3(0,.9f,0); coin.transform.localScale=Vector3.one*.65f;
        coin.GetComponent<SpriteRenderer>().sprite=coins[0]; coin.GetComponent<SpriteRenderer>().sortingOrder=5;
        coin.GetComponent<CircleCollider2D>().isTrigger=true; coin.GetComponent<CircleCollider2D>().radius=.45f; coin.GetComponent<CollectibleCoin>().frames=coins;
    }
}
#endif
