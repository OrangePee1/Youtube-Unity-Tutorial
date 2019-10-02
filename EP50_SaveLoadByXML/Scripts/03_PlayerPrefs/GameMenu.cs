﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//EP 48
using System.Runtime.Serialization.Formatters.Binary;//If you want to use the BinaryFormatter class, must type this namespace first
using System.IO;//If you want to use the FIle class and FileStream class, you have to use this namespace
//IO: Input / Output

using System.Xml;

public class GameMenu : MonoBehaviour
{
    public Image gameMenuImage;

    public Toggle BGMToggle;
    private AudioSource BGMSource;

    private PlayerMovement player;

    //EP48 
    public Text hintText;
    public Bat batGameObject;//THIS gameobject will be instantiate when we have already kill the enemy but need to load the game later

    private void Start()
    {
        gameMenuImage.gameObject.SetActive(false);

        BGMSource = GetComponent<AudioSource>();

        player = FindObjectOfType<PlayerMovement>();//GET ACCESS OF THE PLAYERMOVEMENT component
        hintText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(GameManager.instance.isPaused)//If we press the escape AND the game is PAUSE
            {
                //We want to resume the game ASAP
                Resume();
            }
            else
            {
                //We want to pause the game
                Pause();
            }
        }

        BGMManager();
    }

    public void Resume()
    {
        //Debug.Log("RESUME");
        gameMenuImage.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        GameManager.instance.isPaused = false;
    }

    private void Pause()
    {
        //Debug.Log("PAUSE");
        gameMenuImage.gameObject.SetActive(true);
        Time.timeScale = 0.0f;
        GameManager.instance.isPaused = true;
    }

    //MARKER THIS FUNCTION WILL BE TRIGGER ON THE TOGGLE EVENT LISTENER
    public void BGMToggleButton()
    {
        if(BGMToggle.isOn)
        {
            //OPEN THE BGM
            PlayerPrefs.SetInt("BGM", 1);//1 means open and 0 means close.(CUSTOMIZED)
            //Debug.Log(PlayerPrefs.GetInt("BGM"));
        }
        else
        {
            //CLOSE THE BGM
            PlayerPrefs.SetInt("BGM", 0);
            //Debug.Log(PlayerPrefs.GetInt("BGM"));
        }
    }

    private void BGMManager()
    {
        if(PlayerPrefs.GetInt("BGM") == 1)
        {
            BGMToggle.isOn = true;
            BGMSource.enabled = true;//PLAY THE SOURCE
        }
        else if(PlayerPrefs.GetInt("BGM") == 0)
        {
            BGMToggle.isOn = false;
            BGMSource.enabled = false;
        }
    }

    //MARKER THIS TWO FUNCTIONS WILL BE TRIGGERED ON THE BUTTON EVENT LISTENER
    public void SaveButton()
    {
        //SaveByPlayerPrefs();
        //SaveBySerialization();
        //SaveByJSON();
        SaveByXML();
    }

    public void LoadButton()
    {
        //LoadByPlayerPrefs();
        //LoadByDeSerialization();
        //LoadByJSON();
        LoadByXML();
        Resume();
    }

    //MARKER SAVE AND LOAD BY SERIALIZATION
    private Save createSaveGameObject()
    {
        Save save = new Save();

        save.coinsNum = GameManager.instance.coins;
        save.diamondsNum = GameManager.instance.diamonds;

        save.playerPositionX = player.transform.position.x;
        save.playerPositionY = player.transform.position.y;

        //MARKER Enemy position
        foreach(Bat bat in GameManager.instance.bats)
        {
            save.isDead.Add(bat.isDead);
            save.enemyPositionX.Add(bat.batPositionX);
            save.enemyPositionY.Add(bat.batPositionY);
        }

        return save;
    }

    private void SaveByXML()
    {
        Save save = createSaveGameObject();
        XmlDocument xmlDocument = new XmlDocument();

        #region Create XMLDocument Elements
        XmlElement root = xmlDocument.CreateElement("Save");
        root.SetAttribute("FileName", "File_01");

        XmlElement coinNumElement = xmlDocument.CreateElement("CoinNum");
        //MARKER Gets or sets the concatenated 串联值 values of the node and all its child nodes.
        coinNumElement.InnerText = save.coinsNum.ToString();//Return string type
        root.AppendChild(coinNumElement);

        XmlElement diamondNumElement = xmlDocument.CreateElement("DiamondNum");
        diamondNumElement.InnerText = save.diamondsNum.ToString();
        root.AppendChild(diamondNumElement);

        XmlElement playerPosXElement = xmlDocument.CreateElement("PlayerPositionX");
        playerPosXElement.InnerText = save.playerPositionX.ToString();
        root.AppendChild(playerPosXElement);

        XmlElement playerPosYElement = xmlDocument.CreateElement("PlayerPositionY");
        playerPosYElement.InnerText = save.playerPositionY.ToString();
        root.AppendChild(playerPosYElement);

        //OPTIONAL ADVANCED
        XmlElement bat, batPositionX, batPositionY, batIsDead;
        for(int i = 0; i < save.enemyPositionX.Count; i++)
        {
            bat = xmlDocument.CreateElement("Bat");
            batPositionX = xmlDocument.CreateElement("BatPositionX");
            batPositionY = xmlDocument.CreateElement("BatPositionY");
            batIsDead = xmlDocument.CreateElement("BatIsDead");

            batPositionX.InnerText = save.enemyPositionX[i].ToString();
            batPositionY.InnerText = save.enemyPositionY[i].ToString();
            batIsDead.InnerText = save.isDead[i].ToString();

            bat.AppendChild(batPositionX);
            bat.AppendChild(batPositionY);
            bat.AppendChild(batIsDead);

            root.AppendChild(bat);
        }
        #endregion

        xmlDocument.AppendChild(root);
        xmlDocument.Save(Application.dataPath + "/Data.text");

        if (File.Exists(Application.dataPath + "/Data.text"))
        {
            Debug.Log("XML FILED SAVED");
        }
    }

    private void LoadByXML()
    {
        string filePath = Application.dataPath + "/Data.text";
        if (File.Exists(filePath))
        {
            Save save = new Save();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

            XmlNodeList bats = xmlDocument.GetElementsByTagName("Bat");

            #region get the SAVE data from the File

            XmlNodeList coinNum = xmlDocument.GetElementsByTagName("CoinNum");
            int coinNumCount = int.Parse(coinNum[0].InnerText);
            save.coinsNum = coinNumCount;

            XmlNodeList diamondNum = xmlDocument.GetElementsByTagName("DiamondNum");
            int diamondNumCount = int.Parse(diamondNum[0].InnerText);
            save.diamondsNum = diamondNumCount;

            XmlNodeList playerPosX = xmlDocument.GetElementsByTagName("PlayerPositionX");
            float playerPosXNum = float.Parse(playerPosX[0].InnerText);
            save.playerPositionX = playerPosXNum;

            XmlNodeList playerPosY = xmlDocument.GetElementsByTagName("PlayerPositionY");
            float playerPosYNum = float.Parse(playerPosY[0].InnerText);
            save.playerPositionY = playerPosYNum;

            //OPTIONAL ADVANCED
 
            if (bats.Count != 0)
            {
                //foreach (XmlNode bat in bats)
                for(int i = 0; i < bats.Count; i++)
                {
                    XmlNodeList batPosX = xmlDocument.GetElementsByTagName("BatPositionX");
                    float batPositionX = float.Parse(batPosX[i].InnerText);
                    save.enemyPositionX.Add(batPositionX);

                    XmlNodeList batPosY = xmlDocument.GetElementsByTagName("BatPositionY");
                    float batPositionY = float.Parse(batPosY[i].InnerText);
                    save.enemyPositionY.Add(batPositionY);

                    XmlNodeList batIsDead = xmlDocument.GetElementsByTagName("BatIsDead");
                    bool enemyIsDead = bool.Parse(batIsDead[i].InnerText);
                    save.isDead.Add(enemyIsDead);
                }
            }
            #endregion


            //MARKER ASSIGN the save data to the game status
            GameManager.instance.coins = save.coinsNum;
            GameManager.instance.diamonds = save.diamondsNum;
            player.transform.position = new Vector2(save.playerPositionX, save.playerPositionY);

            //MARKER Enemy position
            for (int i = 0; i < save.isDead.Count; i++)
            {
                if (GameManager.instance.bats[i] == null)
                {
                    if (!save.isDead[i])//If this enemy is died, but alive before we press the save button
                    {
                        float batPosX = save.enemyPositionX[i];
                        float batPosY = save.enemyPositionY[i];
                        Bat newBat = Instantiate(batGameObject, new Vector2(batPosX, batPosY), Quaternion.identity);
                        GameManager.instance.bats[i] = newBat;//Fill in the position which the Bats elemenet is null
                    }
                }
                else
                {
                    float batPosX = save.enemyPositionX[i];
                    float batPosY = save.enemyPositionY[i];
                    GameManager.instance.bats[i].transform.position = new Vector2(batPosX, batPosY);
                }
            }

        }
        else
        {
            Debug.Log("NOT FOUNDED");
        }
    }

    //MARKER Object(Save Type) --> JSON(String)
    private void SaveByJSON()
    {
        Save save = createSaveGameObject();

        //CORE Returns (String Type) Object's data in JSON format
        string JsonString = JsonUtility.ToJson(save);//Convert SAVE Object into JSON(String Type)

        //CORE Write the "JsonString" to the "JSONData.text" file
        StreamWriter sw = new StreamWriter(Application.dataPath + "/JSONData.text");
        sw.Write(JsonString);//CORE Write a string to a stream

        sw.Close();

        Debug.Log("-=-=-=-SAVED-=-=-=-");
    }

    //MARKER JSON(STring) --> Object(Save Type)
    private void LoadByJSON()
    {
        if(File.Exists(Application.dataPath + "/JSONData.text"))
        {
            //LOAD THE GAME
            StreamReader sr = new StreamReader(Application.dataPath + "/JSONData.text");

            //CORE Read the text directly from the text file
            string JsonString = sr.ReadToEnd();

            sr.Close();

            //Convert JSON to the Object(save)
            Save save = JsonUtility.FromJson<Save>(JsonString);//Into the Save Object

            Debug.Log("-=-=-=-LOADED-=-=-=-=-");

            //MARKER LOAD THE DATA TO THE GAME
            GameManager.instance.coins = save.coinsNum;
            GameManager.instance.diamonds = save.diamondsNum;

            player.transform.position = new Vector2(save.playerPositionX, save.playerPositionY);

            //MARKER Enemy position
            for (int i = 0; i < save.isDead.Count; i++)
            {
                if (GameManager.instance.bats[i] == null)
                {
                    if (!save.isDead[i])//If this enemy is died, but alive before we press the save button
                    {
                        float batPosX = save.enemyPositionX[i];
                        float batPosY = save.enemyPositionY[i];
                        Bat newBat = Instantiate(batGameObject, new Vector2(batPosX, batPosY), Quaternion.identity);
                        GameManager.instance.bats[i] = newBat;//Fill in the position which the Bats elemenet is null
                    }
                }
                else
                {
                    float batPosX = save.enemyPositionX[i];
                    float batPosY = save.enemyPositionY[i];
                    GameManager.instance.bats[i].transform.position = new Vector2(batPosX, batPosY);
                }
            }
        }
        else
        {
            Debug.Log("NOT FOUND FILE");
        }
    }

    //MARKER SAVE --> Stream of Bytes
    private void SaveBySerialization()
    {
        Save save = createSaveGameObject();//MARKER Create a SAVE instance will all the data for current status

        BinaryFormatter bf = new BinaryFormatter();

        //FileStream fileStream = File.Create(Application.persistentDataPath + "/Data.text");
        FileStream fileStream = File.Create(Application.dataPath + "/Data.text");

        bf.Serialize(fileStream, save);//Object to bytes

        fileStream.Close();
    }

    private void LoadByDeSerialization()
    {
        if(File.Exists(Application.persistentDataPath + "/Data.text"))
        {
            //LOAD THE GAME
            BinaryFormatter bf = new BinaryFormatter();

            //FileStream fileStream = File.Open(Application.persistentDataPath + "/Data.text", FileMode.Open);
            FileStream fileStream = File.Open(Application.dataPath + "/Data.text", FileMode.Open);

            Save save = bf.Deserialize(fileStream) as Save;//You have loaded your previous "save" object
            fileStream.Close();

            //MARKER LOAD the data to the game 

            GameManager.instance.coins = save.coinsNum;
            GameManager.instance.diamonds = save.diamondsNum;

            player.transform.position = new Vector2(save.playerPositionX, save.playerPositionY);

            //MARKER Enemy position
            for(int i = 0; i < save.isDead.Count; i++)
            {
                if (GameManager.instance.bats[i] == null)
                {
                    if(!save.isDead[i])//If this enemy is died, but alive before we press the save button
                    {
                        float batPosX = save.enemyPositionX[i];
                        float batPosY = save.enemyPositionY[i];
                        Bat newBat = Instantiate(batGameObject, new Vector2(batPosX, batPosY), Quaternion.identity);
                        GameManager.instance.bats[i] = newBat;//Fill in the position which the Bats elemenet is null
                    }
                }
                else
                {
                    float batPosX = save.enemyPositionX[i];
                    float batPosY = save.enemyPositionY[i];
                    GameManager.instance.bats[i].transform.position = new Vector2(batPosX, batPosY);
                }
            }
        }
        else
        {
            //REPORT THE ERROR
            Debug.Log("NOT FOUND THIS FILE");
        }
    }

    //MARKER SAVE AND LOAD BY PLAYERPREFS
    private void SaveByPlayerPrefs()
    {
        //THE KEY NAME IS CUSTOMIZED AND CHOOSE ONE EASY TO READ FOR OTHERS
        PlayerPrefs.SetInt("Coins", GameManager.instance.coins);//THE KEY is the "Coins"
        PlayerPrefs.SetInt("Diamonds", GameManager.instance.diamonds);//THE VALUE IS ONE VARIABLE

        //SAVE the player gameobject position
        PlayerPrefs.SetFloat("PlayerPosX", player.transform.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", player.transform.position.y);
        Debug.Log("SAVE THE DATA");
        StartCoroutine(DisplayHintCo("SAVED"));
    }

    private void LoadByPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("Coins") && PlayerPrefs.HasKey("Diamonds") && PlayerPrefs.HasKey("PlayerPosX") && PlayerPrefs.HasKey("PlayerPosY"))
        {
            GameManager.instance.coins = PlayerPrefs.GetInt("Coins");//Assign the saving data to our variable
            GameManager.instance.diamonds = PlayerPrefs.GetInt("Diamonds");
            player.playerPosX = PlayerPrefs.GetFloat("PlayerPosX");
            player.playerPosY = PlayerPrefs.GetFloat("PlayerPosY");
            StartCoroutine(DisplayHintCo("LOADED"));
            Debug.Log("LOAD THE DATA");
        }
        else
        {
            StartCoroutine(DisplayHintCo("NOT FOUND"));
        }

        player.transform.position = new Vector2(player.playerPosX, player.playerPosY);
    }

    //MARKER THIS FUNCTION WILL BE TRIGGERED WHEN YOU PRESS THE SAVE OR LOAD BUTTON
    IEnumerator DisplayHintCo(string _message)
    {
        Debug.Log("TTTTT");
        hintText.gameObject.SetActive(true);
        hintText.text = _message;
        yield return new WaitForSeconds(2);
        hintText.gameObject.SetActive(false);
    }

}
