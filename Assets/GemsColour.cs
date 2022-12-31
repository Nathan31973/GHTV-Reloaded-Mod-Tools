using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

public class GemsColour : MonoBehaviour
{
    public List<SpriteRenderer> GemBothSmash;
    public List<SpriteRenderer> GemDownSmash;
    public List<SpriteRenderer> GemUpSmash;
    public List<SpriteRenderer> OpenGemSmash;
    public List<SpriteRenderer> Lighting;
    public SpriteRenderer Fire;
    public SpriteRenderer UpSmashBeam;
    public SpriteRenderer Flare;
    public SpriteRenderer AlhpaStrip;

    private Color32 GemBothSmashColour;
    private Color32 GemDownSmashColour;
    private Color32 GemUpSmashColour;
    private Color32 OpenGemSmashColour;
    private Color32 fireColour;
    private Color32 UpSmashBeamColour;
    private Color32 FlareColour;
    private Color32 LightingColour;
    private Color32 AlhpaStripColour;

    public FlexibleColorPicker GemBothSmashFlex;
    public FlexibleColorPicker GemDownSmashFlex;
    public FlexibleColorPicker GemUpSmashFlex;
    public FlexibleColorPicker OpenGemSmashFlex;
    public FlexibleColorPicker FireFlex;
    public FlexibleColorPicker UpSmashBeamFlex;
    public FlexibleColorPicker FlareFlex;
    public FlexibleColorPicker AlphaStripFlex;
    public FlexibleColorPicker LightingStripFlex;

    public GameObject MessageBox;
    public GameObject fadeToBlack;

    // Start is called before the first frame update
    void Start()
    {
        DiscordController.instance.UpdateDiscordActivity("Highway FX Changer", "Changing the colour of the highway FX", "highway_fx", "Highway FX Changer");
        GemBothSmashFlex.gameObject.SetActive(true);
        GemDownSmashFlex.gameObject.SetActive(false);
        GemUpSmashFlex.gameObject.SetActive(false);
        OpenGemSmashFlex.gameObject.SetActive(false);
        FireFlex.gameObject.SetActive(false);
        UpSmashBeamFlex.gameObject.SetActive (false);
        FlareFlex.gameObject.SetActive(false);
        AlphaStripFlex.gameObject.SetActive(false);
        LightingStripFlex.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameObject a = Instantiate(fadeToBlack);
                a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
                a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
                a.GetComponent<FadeToBlack>().anim.Play();
            }
        }
        GemBothSmashColour = GemBothSmashFlex.color;
        GemDownSmashColour = GemDownSmashFlex.color;
        GemUpSmashColour = GemUpSmashFlex.color;
        OpenGemSmashColour = OpenGemSmashFlex.color;
        UpSmashBeamColour = UpSmashBeamFlex.color;
        FlareColour = FlareFlex.color;
        AlhpaStripColour = AlphaStripFlex.color;
        LightingColour = LightingStripFlex.color;
        fireColour = FireFlex.color;
        foreach(SpriteRenderer gem in GemBothSmash)
        {
            gem.color = GemBothSmashColour;
        }
        foreach (SpriteRenderer gem in GemDownSmash)
        {
            gem.color = GemDownSmashColour;
        }
        foreach (SpriteRenderer gem in GemUpSmash)
        {
            gem.color = GemUpSmashColour;
        }
        foreach (SpriteRenderer gem in OpenGemSmash)
        {
            gem.color = OpenGemSmashColour;
        }
        foreach(SpriteRenderer gem in Lighting)
        {
            gem.color = LightingColour;
        }
        Fire.color = fireColour;
        UpSmashBeam.color = UpSmashBeamColour;
        Flare.color = FlareColour;
        AlhpaStrip.color = AlhpaStripColour;
    }

    public void GEMBOTH()
    {
        GemBothSmashFlex.gameObject.SetActive(true);
        GemDownSmashFlex.gameObject.SetActive(false);
        GemUpSmashFlex.gameObject.SetActive(false);
        OpenGemSmashFlex.gameObject.SetActive(false);
        FireFlex.gameObject.SetActive(false);
        UpSmashBeamFlex.gameObject.SetActive(false);
        FlareFlex.gameObject.SetActive(false);
        AlphaStripFlex.gameObject.SetActive(false);
        LightingStripFlex.gameObject.SetActive(false);
    }
    public void GEMDOWN()
    {
        GemBothSmashFlex.gameObject.SetActive(false);
        GemDownSmashFlex.gameObject.SetActive(true);
        GemUpSmashFlex.gameObject.SetActive(false);
        OpenGemSmashFlex.gameObject.SetActive(false);
        FireFlex.gameObject.SetActive(false);
        UpSmashBeamFlex.gameObject.SetActive(false);
        FlareFlex.gameObject.SetActive(false);
        AlphaStripFlex.gameObject.SetActive(false);
        LightingStripFlex.gameObject.SetActive(false);
    }
    public void GEMUP()
    {
        GemBothSmashFlex.gameObject.SetActive(false);
        GemDownSmashFlex.gameObject.SetActive(false);
        GemUpSmashFlex.gameObject.SetActive(true);
        OpenGemSmashFlex.gameObject.SetActive(false);
        FireFlex.gameObject.SetActive(false);
        UpSmashBeamFlex.gameObject.SetActive(false);
        FlareFlex.gameObject.SetActive(false);
        AlphaStripFlex.gameObject.SetActive(false);
        LightingStripFlex.gameObject.SetActive(false);
    }
    public void GEMOPEN()
    {
        GemBothSmashFlex.gameObject.SetActive(false);
        GemDownSmashFlex.gameObject.SetActive(false);
        GemUpSmashFlex.gameObject.SetActive(false);
        OpenGemSmashFlex.gameObject.SetActive(true);
        FireFlex.gameObject.SetActive(false);
        UpSmashBeamFlex.gameObject.SetActive(false);
        FlareFlex.gameObject.SetActive(false);
        AlphaStripFlex.gameObject.SetActive(false);
        LightingStripFlex.gameObject.SetActive(false);
    }
    public void LIGHTING()
    {
        GemBothSmashFlex.gameObject.SetActive(false);
        GemDownSmashFlex.gameObject.SetActive(false);
        GemUpSmashFlex.gameObject.SetActive(false);
        OpenGemSmashFlex.gameObject.SetActive(false);
        FireFlex.gameObject.SetActive(false);
        UpSmashBeamFlex.gameObject.SetActive(false);
        FlareFlex.gameObject.SetActive(false);
        AlphaStripFlex.gameObject.SetActive(false);
        LightingStripFlex.gameObject.SetActive(true);
    }
    public void FIRE()
    {
        GemBothSmashFlex.gameObject.SetActive(false);
        GemDownSmashFlex.gameObject.SetActive(false);
        GemUpSmashFlex.gameObject.SetActive(false);
        OpenGemSmashFlex.gameObject.SetActive(false);
        FireFlex.gameObject.SetActive(true);
        UpSmashBeamFlex.gameObject.SetActive(false);
        FlareFlex.gameObject.SetActive(false);
        AlphaStripFlex.gameObject.SetActive(false);
        LightingStripFlex.gameObject.SetActive(false);
    }
    public void UPSMASH()
    {
        GemBothSmashFlex.gameObject.SetActive(false);
        GemDownSmashFlex.gameObject.SetActive(false);
        GemUpSmashFlex.gameObject.SetActive(false);
        OpenGemSmashFlex.gameObject.SetActive(false);
        FireFlex.gameObject.SetActive(false);
        UpSmashBeamFlex.gameObject.SetActive(true);
        FlareFlex.gameObject.SetActive(false);
        AlphaStripFlex.gameObject.SetActive(false);
        LightingStripFlex.gameObject.SetActive(false);
    }
    public void FLARE()
    {
        GemBothSmashFlex.gameObject.SetActive(false);
        GemDownSmashFlex.gameObject.SetActive(false);
        GemUpSmashFlex.gameObject.SetActive(false);
        OpenGemSmashFlex.gameObject.SetActive(false);
        FireFlex.gameObject.SetActive(false);
        UpSmashBeamFlex.gameObject.SetActive(false);
        FlareFlex.gameObject.SetActive(true);
        AlphaStripFlex.gameObject.SetActive(false);
        LightingStripFlex.gameObject.SetActive(false);
    }
    public void BARSTRIP()
    {
        GemBothSmashFlex.gameObject.SetActive(false);
        GemDownSmashFlex.gameObject.SetActive(false);
        GemUpSmashFlex.gameObject.SetActive(false);
        OpenGemSmashFlex.gameObject.SetActive(false);
        FireFlex.gameObject.SetActive(false);
        UpSmashBeamFlex.gameObject.SetActive(false);
        FlareFlex.gameObject.SetActive(false);
        AlphaStripFlex.gameObject.SetActive(true);
        LightingStripFlex.gameObject.SetActive(false);
    }
    public void Done()
    {
        try
        {
            if (userData.instance.LocalFilePath == "TEST")
            {
                var paths = StandaloneFileBrowser.OpenFolderPanel("SELECT YOUR RPCS3 FOLDER", "RPSC3 FOLDER", false);
                foreach (var path in paths)
                {
                    if (Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
                    {
                        Debug.Log($"[Highway FX] {path} Is valid");
                        userData.instance.LocalFilePath = path;
                    }
                    else
                    {
                        GameObject t = Instantiate(MessageBox);
                        t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
                        t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
                    }
                }
            }
            //checking if path is vaild
            if (Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLES02180/USRDIR/UPDATE") | Directory.Exists($"{userData.instance.LocalFilePath}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE"))
            {
                Debug.Log($"[Highway FX] {userData.instance.LocalFilePath} Is valid");
                UnityToXML(userData.instance.LocalFilePath, GemBothSmashColour, "GEMBOTH_SMASH_EFFECT.XML");
                UnityToXML(userData.instance.LocalFilePath, fireColour, "CATCHER_BLUE_FLAME_EFFECT.XML");
                UnityToXML(userData.instance.LocalFilePath, GemDownSmashColour, "GEMDOWN_SMASH_EFFECT.XML");
                UnityToXML(userData.instance.LocalFilePath, GemUpSmashColour, "GEMUP_SMASH_EFFECT.XML");
                UnityToXML(userData.instance.LocalFilePath, OpenGemSmashColour, "OPEN_GEM_SMASH_EFFECT.XML");
                GameObject a = Instantiate(MessageBox);
                a.GetComponent<GUI_MessageBox>().title = "Colours Saved!!";
                a.GetComponent<GUI_MessageBox>().message = $"Your custom colour have been save to your Guitar Hero Live game. \n\nColour will be change the next time you launch GHL/GHTV from the main menu";
                a.GetComponent<GUI_MessageBox>().button.onClick.AddListener(ReturnToMainMenu);
            }
            else
            {
                userData.instance.LocalFilePath = "TEST";
                GameObject t = Instantiate(MessageBox);
                t.GetComponent<GUI_MessageBox>().title = "Invalided Path";
                t.GetComponent<GUI_MessageBox>().message = $"Please make sure you have selected your RPCS3 Root folder!";
            }


        }
        catch(Exception e)
        {
            Debug.LogError("[Highway FX] " + e.Message);
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = "Failed To Save Colours";
            t.GetComponent<GUI_MessageBox>().message = $"An Error has occured while saving\nPlease try again\n\nERROR CODE: FXsysFailed";
            
        }
    }
    public void UnityToXML(string path, Color32 c, string filename)
    {

        if (path == null || path == "")
        {
            GameObject t = Instantiate(MessageBox);
            t.GetComponent<GUI_MessageBox>().title = "Failed To Save Hyper Speed";
            t.GetComponent<GUI_MessageBox>().message = $"An Error has occured while saving\nPlease try again\n\nERROR CODE: HSsysNoneFailed";
            return;
        }

        Debug.LogWarning("[Highway FX] " + filename +" "+"RGB COLOUR: " + ColourConverter(c));
        var doc = XDocument.Load(Application.streamingAssetsPath + "/PARTICLES/"+ filename);


        //LIGHTBOI
        var lighting = doc.Descendants()
                  .Where(o => o.Value == "LIGHTBOI");
        foreach (XElement el in lighting)
        {
            el.Value = ColourConverter(LightingColour);
        }


        //FLEARY
        var flare = doc.Descendants()
                          .Where(o => o.Value == "FLEARY");
        foreach (XElement el in flare)
        {
            el.Value = ColourConverter(FlareColour);
        }


        var Upsmash_beam = doc.Descendants()
                                  .Where(o => o.Value == "UpSmash_BeamCOLOUR");
        foreach(XElement el in Upsmash_beam)
        {
            el.Value = ColourConverter(UpSmashBeamColour);
        }

        //select all leaf elements having value equals "john"
        var elementsToUpdate = doc.Descendants()
                                  .Where(o => o.Value == "0.01,0.1,0.1,1");
        //update elements value
        foreach (XElement element in elementsToUpdate)
        {
            element.Value = ColourConverter(c);
        }

        //save the XML back as file
        //checking if directory exist
        if (!Directory.Exists($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/WIZARDRY/PARTICLES")) ;
        {
            Directory.CreateDirectory($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/WIZARDRY/PARTICLES");
        }
        if (!Directory.Exists($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/WIZARDRY/PARTICLES")) ;
        {
            Directory.CreateDirectory($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/WIZARDRY/PARTICLES");
        }
        doc.Save($"{path}/dev_hdd0/game/BLUS31556/USRDIR/UPDATE/OVERRIDE/WIZARDRY/PARTICLES/{filename}");
        doc.Save($"{path}/dev_hdd0/game/BLES02180/USRDIR/UPDATE/OVERRIDE/WIZARDRY/PARTICLES/{filename}");

    }
    string ColourConverter(Color32 c)
    {
        return $"{Math.Round(c.r/255.0,1)},{Math.Round(c.g/255.0,1)},{Math.Round(c.b/255.0,1)},{Math.Round(c.a/255.0,1)}";
    }
    public void ReturnToMainMenu()
    {
        GameObject a = Instantiate(fadeToBlack);
        a.gameObject.GetComponent<FadeToBlack>().levelToChangeScene = "Main Menu";
        a.GetComponent<FadeToBlack>().anim.clip = a.GetComponent<FadeToBlack>().animClip[1];
        a.GetComponent<FadeToBlack>().anim.Play();
    }
}
