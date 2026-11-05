#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MobileUISetup
{
    static readonly Color Ink = new Color32(18, 27, 45, 255);
    static readonly Color Gold = new Color32(255, 204, 92, 255);
    static Font font;
    static Sprite coin;
    static MobileUISetup() { EditorApplication.delayCall += AutoSetup; }
    static void AutoSetup()
    {
        if (!File.Exists("Temp/mobile-ui-setup.request") || EditorApplication.isPlayingOrWillChangePlaymode) return;
        File.Delete("Temp/mobile-ui-setup.request");
        try { Setup(); File.WriteAllText("Temp/mobile-ui-setup.result", "OK"); }
        catch (Exception e) { Debug.LogException(e); File.WriteAllText("Temp/mobile-ui-setup.result", e.ToString()); }
    }
    [MenuItem("Game/Mobile UI/Build lecture UI")]
    public static void Setup()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        if (GameObject.Find("Mobile Interface")) throw new InvalidOperationException("Mobile Interface already exists. Setup does not overwrite it.");
        Directory.CreateDirectory("Documentation/Backups");
        File.Copy(scene.path, "Documentation/Backups/SampleScene.before-mobile-ui.unity.txt", true);
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Simple 2D Platformer BE2/Sprites/Coins.png").OfType<Sprite>().Where(s => s.name.StartsWith("Gold")).OrderBy(s => s.name).ToArray();
        coin = sprites.First();
        var player = UnityEngine.Object.FindFirstObjectByType<Player>();
        if (!player) throw new InvalidOperationException("No Player in level");
        if (!player.GetComponent<PlayerHealth>()) player.gameObject.AddComponent<PlayerHealth>();
        var root = new GameObject("Mobile Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MobileGameUI));
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        root.GetComponent<Canvas>().sortingOrder = 100;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1;
        var ui = root.GetComponent<MobileGameUI>();
        ui.player = player;
        ui.mainMenu = ScreenRoot(root.transform, "Main Menu", out var menu);
        Label(menu, "Caption", "A LITTLE ADVENTURE", 36, Gold, new Vector2(.5f,.5f), new Vector2(0,225), new Vector2(760,60));
        Label(menu, "Game Title", "SKYBOUND", 108, Color.white, new Vector2(.5f,.5f), new Vector2(0,125), new Vector2(1000,150));
        Label(menu, "Subtitle", "Collect. Jump. Explore.", 36, new Color32(171,192,212,255), new Vector2(.5f,.5f), new Vector2(0,20), new Vector2(800,70));
        ui.playButton = Button(menu, "Play", new Vector2(.5f,.5f), new Vector2(0,-110), new Vector2(460,120), true);
        ui.quitButton = Button(menu, "Quit", new Vector2(.5f,.5f), new Vector2(0,-250), new Vector2(460,120));
        Label(menu, "Hint", "A / D or joystick to move     SPACE or JUMP to jump", 36, new Color32(171,192,212,255), new Vector2(.5f,0), new Vector2(0,65), new Vector2(1120,60));
        ui.hud = Rect(root.transform, "HUD - Safe Area").gameObject;
        Stretch(ui.hud.GetComponent<RectTransform>());
        ui.hud.AddComponent<SafeArea>();
        var health = Box(ui.hud.transform, "Health", new Vector2(0,1), new Vector2(185,-90), new Vector2(290,110), Ink);
        ui.hearts = new HeartGraphic[3];
        for(int i=0;i<3;i++) { var h=Rect(health,"Heart "+(i+1)); Place(h,new Vector2(.5f,.5f),new Vector2((i-1)*82,0),new Vector2(68,68)); ui.hearts[i]=h.gameObject.AddComponent<HeartGraphic>(); ui.hearts[i].raycastTarget=false; }
        var item = Box(ui.hud.transform,"Collected Items",new Vector2(.5f,1),new Vector2(0,-90),new Vector2(230,110),Ink);
        var icon=Rect(item,"Coin"); Place(icon,new Vector2(.5f,.5f),new Vector2(-62,0),new Vector2(60,60)); icon.gameObject.AddComponent<Image>().sprite=coin; icon.GetComponent<Image>().preserveAspect=true; icon.GetComponent<Image>().raycastTarget=false;
        ui.counter=Label(item,"Count","0",48,Color.white,new Vector2(.5f,.5f),new Vector2(40,0),new Vector2(100,80));
        ui.pauseButton=Button(ui.hud.transform,"Pause",new Vector2(1,1),new Vector2(-142,-90),new Vector2(204,120));
        var stick=Box(ui.hud.transform,"Joystick",new Vector2(0,0),new Vector2(210,205),new Vector2(270,270),new Color32(18,27,45,180));
        stick.GetComponent<Image>().sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var knob=Box(stick,"Knob",new Vector2(.5f,.5f),Vector2.zero,new Vector2(118,118),Gold);
        knob.GetComponent<Image>().sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var onStick=knob.gameObject.AddComponent<OnScreenStick>(); onStick.controlPath="<Gamepad>/leftStick"; onStick.movementRange=80;
        Label(ui.hud.transform,"Move hint","MOVE",36,Ink,new Vector2(0,0),new Vector2(210,45),new Vector2(240,50));
        var jump=Button(ui.hud.transform,"Jump",new Vector2(1,0),new Vector2(-200,195),new Vector2(220,220),true);
        jump.GetComponent<Image>().sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        jump.GetComponent<Image>().preserveAspect=true;
        jump.gameObject.AddComponent<OnScreenButton>().controlPath="<Gamepad>/buttonSouth";
        ui.pauseMenu=ScreenRoot(root.transform,"Pause Menu",out var paused);
        Label(paused,"Title","PAUSED",84,Color.white,new Vector2(.5f,.5f),new Vector2(0,250),new Vector2(900,120));
        ui.resumeButton=Button(paused,"Resume",new Vector2(.5f,.5f),new Vector2(0,65),new Vector2(460,120),true);
        ui.restartButton=Button(paused,"Restart",new Vector2(.5f,.5f),new Vector2(0,-75),new Vector2(460,120));
        ui.menuButton=Button(paused,"Menu",new Vector2(.5f,.5f),new Vector2(0,-215),new Vector2(460,120));
        ui.gameOver=ScreenRoot(root.transform,"Game Over",out var over);
        Label(over,"Title","TRY AGAIN",84,Color.white,new Vector2(.5f,.5f),new Vector2(0,185),new Vector2(900,120));
        ui.retryButton=Button(over,"Restart",new Vector2(.5f,.5f),new Vector2(0,-10),new Vector2(460,120),true);
        ui.gameOverMenuButton=Button(over,"Menu",new Vector2(.5f,.5f),new Vector2(0,-150),new Vector2(460,120));
        ui.hud.SetActive(false); ui.pauseMenu.SetActive(false); ui.gameOver.SetActive(false);
        var es=UnityEngine.Object.FindFirstObjectByType<EventSystem>();
        if(!es) es=new GameObject("EventSystem",typeof(EventSystem)).GetComponent<EventSystem>();
        var old=es.GetComponent<StandaloneInputModule>(); if(old) UnityEngine.Object.DestroyImmediate(old);
        if(!es.GetComponent<InputSystemUIInputModule>()) es.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        Physics2D.SyncTransforms();
        var coins=new GameObject("Collectible Coins");
        for(int i=0;i<7;i++)
        {
            float x=-4.5f+i*1.5f;
            if (Mathf.Abs(x-player.transform.position.x)<.85f) x=player.transform.position.x+1f;
            var hit=Physics2D.Raycast(new Vector2(x,4.5f),Vector2.down,12,player.groundLayer);
            if(!hit.collider) continue;
            var go=new GameObject("Gold Coin "+(i+1),typeof(SpriteRenderer),typeof(CircleCollider2D),typeof(CollectibleCoin));
            go.transform.SetParent(coins.transform); go.transform.position=new Vector3(x,hit.point.y+1.15f,0); go.transform.localScale=Vector3.one*.65f;
            go.GetComponent<SpriteRenderer>().sprite=coin; go.GetComponent<SpriteRenderer>().sortingOrder=5;
            go.GetComponent<CircleCollider2D>().isTrigger=true; go.GetComponent<CircleCollider2D>().radius=.45f;
            go.GetComponent<CollectibleCoin>().frames=sprites;
        }
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait=false; PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene.path,true)};
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        SeparateMenuScenes.Setup();
        Debug.Log("Lecture mobile UI installed: Canvas 1920x1080 Match=1, SafeArea, Input System, coins and hearts.");
    }
    static RectTransform Rect(Transform parent,string name) { var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); return r; }
    static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=r.offsetMax=Vector2.zero; }
    static void Place(RectTransform r,Vector2 anchor,Vector2 pos,Vector2 size) { r.anchorMin=r.anchorMax=anchor; r.pivot=new Vector2(.5f,.5f); r.anchoredPosition=pos; r.sizeDelta=size; }
    static RectTransform Box(Transform p,string name,Vector2 a,Vector2 pos,Vector2 size,Color c) { var r=Rect(p,name); Place(r,a,pos,size); var im=r.gameObject.AddComponent<Image>(); im.color=c; return r; }
    static Text Label(Transform p,string name,string value,int size,Color c,Vector2 a,Vector2 pos,Vector2 rect)
    {
        var r=Rect(p,name); Place(r,a,pos,rect); var t=r.gameObject.AddComponent<Text>(); t.text=value; t.font=font; t.fontSize=size; t.color=c; t.alignment=TextAnchor.MiddleCenter; t.raycastTarget=false; return t;
    }
    static Button Button(Transform p,string name,Vector2 a,Vector2 pos,Vector2 size,bool primary=false)
    {
        var r=Box(p,name,a,pos,size,primary?Gold:new Color32(39,54,77,255));
        var b=r.gameObject.AddComponent<Button>(); var colors=b.colors; colors.highlightedColor=new Color(.9f,.94f,1); colors.pressedColor=new Color(.7f,.76f,.85f); b.colors=colors;
        var nav=b.navigation; nav.mode=Navigation.Mode.None; b.navigation=nav;
        Label(r,"Label",name,40,primary?Ink:Color.white,new Vector2(.5f,.5f),Vector2.zero,size);
        return b;
    }
    static GameObject ScreenRoot(Transform parent,string name,out RectTransform safe)
    {
        var r=Rect(parent,name); Stretch(r); var bg=r.gameObject.AddComponent<Image>(); bg.color=new Color32(13,22,38,242);
        safe=Rect(r,"Safe Area"); Stretch(safe); safe.gameObject.AddComponent<SafeArea>(); return r.gameObject;
    }
}
#endif




