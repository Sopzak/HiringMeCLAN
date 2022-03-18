using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class GameController : MonoBehaviour
{
    public GameObject player;
    public GameObject ballonChatPlayer, ballonChatNpc, ballonMissionPlayer, AnimatedTapFinger, imgDone, objDone;
    public GameObject pnStart, pnButtons, pnMission, pnChat, pnMain, pnInfo, pnMenu, pnGameOver, pnFlash;
    public Button btnStart, btnPlay, btnMenu, btnStage, btnTap;
    public List<GameObject> characters;
    public Scrollbar sbTapMission;
    public StageEnum currentStage;
    public CharacterNameEnum gameCharacterChat;
    public Text txtChatPlayer, txtMissionPlayer, txtChatNpc, txtStage, txtChapter;
    public float timeType = 1, deltaTimeType = 0;


    private bool isAnimate;
    public int countTap, objectiveTap, currentLevel, currentPlayerChat, currentCharacterChat, currentPlayerHistory, currentPlayerMission;
    private Animator animStart, animMain, animHistory, animChat, animMission, animPlayer, animCharacterChat, animTapFinger, animDone;
    private float sizeTapMission = 0.3f;
    public List<GameLevel> levels;
    
    public enum StageEnum
    {
        Start,
        History,
        Mission,
        Chat, 
        GameOver
    }
    public enum CharacterNameEnum
    {
        NOBODY,
        Mary,
        Tony,
        CEO
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //Get Animations from GameObjects
        animStart = pnStart.GetComponent<Animator> ();
        animPlayer = player.GetComponent<Animator> ();
        animTapFinger = AnimatedTapFinger.GetComponent<Animator> ();
        animChat =  pnChat.GetComponent<Animator> ();
        //animDone =  imgDone.GetComponent<Animator> ();

        //Set Stages variables
        txtChatNpc.text = "";
        txtChatPlayer.text ="";
        txtMissionPlayer.text = "";
        levels = new List<GameLevel>();

        //Set Inicial Stage       
        currentStage = StageEnum.Start;
        changeStage(StageEnum.Start);

        //Read JSON
        getLevelJson();

    }

    #region UPDATE
    // Update is called once per frame
    void Update()
    {
        switch(currentStage)
        {
            case StageEnum.History:
            {
                historyUpdate();
                break;
            }
            case StageEnum.Mission:
            {
                missionUpdate();
                break;
            }
            case StageEnum.Chat:
            {
                chatUpdate();
                break;
            }
            default: break;
        }
    }

    private void chatUpdate(){
        if(ballonChatPlayer.activeSelf){
            if(levels[currentLevel].gamechats.isplayertalk[currentPlayerChat] == 1){
                if(txtChatPlayer.text.Length < levels[currentLevel].gamechats.message[currentPlayerChat].Length)
                {
                    setBallonText(levels[currentLevel].gamechats.message[currentPlayerChat], txtChatPlayer);
                } 
            }else{
                if(txtChatNpc.text.Length < levels[currentLevel].gamechats.message[currentPlayerChat].Length)
                {
                    if(!ballonChatNpc.activeSelf){
                        ballonChatNpc.SetActive(true);
                    }
                    setBallonText(levels[currentLevel].gamechats.message[currentPlayerChat], txtChatNpc);
                } 
            }
             
        }
        
    } 

    private void missionUpdate(){
        if(!objDone.activeSelf){
            if(txtMissionPlayer.text.Length < levels[currentLevel].gamemissions[currentPlayerMission].message.Length)
            {
                setBallonText(levels[currentLevel].gamemissions[currentPlayerMission].message, txtMissionPlayer);
            }  
        }
        
    }   
    private void historyUpdate(){
        if(ballonChatPlayer.activeSelf){
            if(txtChatPlayer.text.Length < levels[currentLevel].gamehistory.wordslist[currentPlayerHistory].Length)
            {
                setBallonText(levels[currentLevel].gamehistory.wordslist[currentPlayerHistory], txtChatPlayer);
            }
        }
    }

    #endregion
        
    #region buttons function

    public void openURL(string url){
        Application.OpenURL(url);
    }

    public void btnTapClick(){
        if(!btnPlay.GetComponent<Button>().interactable)
        {
            AnimatedTapFinger.SetActive(false);
            countTap ++;
            sbTapMission.size = countTap * (100/objectiveTap) * 0.01f;
            if(objectiveTap <= countTap){
                imgDone.SetActive(true);
                objDone.SetActive(true);
                txtMissionPlayer.text = "Well Done!";
                btnPlay.GetComponent<Button>().interactable = true;
                countTap =0;
            } 
        }
        
    }

    public void btnStartClick(){
        changeStage(StageEnum.History);
        currentLevel = 1;
        txtChapter.text = "Chapter " + currentLevel.ToString() + "           " +levels[currentLevel].name;
        txtStage.text = levels[currentLevel].name;
    }

    public void btnPlayClick(){
        switch(currentStage)
        {
            case StageEnum.History:
            {
                //Debug.Log("history.count " + levels[currentLevel].gamehistory.wordslist..Count);
                //Debug.Log("currentPlayerHistory " + currentPlayerHistory);
                if(txtChatPlayer.text.Length < levels[currentLevel].gamehistory.wordslist[currentPlayerHistory].Length)
                {
                    txtChatPlayer.text = levels[currentLevel].gamehistory.wordslist[currentPlayerHistory];
                }else{
                    if(levels[currentLevel].gamehistory.wordslist.Length > currentPlayerHistory + 1){
                        currentPlayerHistory++;
                        txtChatPlayer.text = "";
                    }else{
                        if(levels[currentLevel].gamemissions[0].hits > 0){
                            changeStage(StageEnum.Mission);
                        }else{
                            //Game Over
                                if(levels.Count > currentLevel + 1){
                                currentLevel++;
                                txtStage.text = levels[currentLevel].name;
                                txtChapter.text = "Chapter " + currentLevel.ToString() + " " + levels[currentLevel].name;
                                changeStage(StageEnum.History); 
                            }else{
                                changeStage(StageEnum.GameOver);
                            }
                        }
                    }
                }
                
                break;
            }
            case StageEnum.Mission:
            {
                if(levels[currentLevel].gamemissions.Length > currentPlayerMission +1){
                    currentPlayerMission++;
                    startMission();
                }
                else{
                    if(levels[currentLevel].gamechats.npcid >0){
                        changeStage(StageEnum.Chat); 
                    }else{
                        if(levels.Count > currentLevel + 1){
                            currentLevel++;
                            txtStage.text = levels[currentLevel].name;
                            txtChapter.text = "Chapter " + currentLevel.ToString() + " " + levels[currentLevel].name;
                            changeStage(StageEnum.History); 
                        }else{
                            changeStage(StageEnum.GameOver);
                        }
                    }
                }
                break;
            }
            case StageEnum.Chat:
            {
                bool nextBallon = false;
                if(levels[currentLevel].gamechats.isplayertalk[currentPlayerChat] == 1){
                    if(txtChatPlayer.text.Length < levels[currentLevel].gamechats.message[currentPlayerChat].Length)
                    {
                        txtChatPlayer.text = levels[currentLevel].gamechats.message[currentPlayerChat];
                        nextBallon = true;
                    } 
                }else{
                    if(txtChatNpc.text.Length < levels[currentLevel].gamechats.message[currentPlayerChat].Length)
                    {
                        txtChatNpc.text = levels[currentLevel].gamechats.message[currentPlayerChat];
                        nextBallon = true;
                    } 
                }
                if(!nextBallon){
                    if(levels[currentLevel].gamechats.isplayertalk.Length > currentPlayerChat +1 ){
                        currentPlayerChat ++;
                        if(levels[currentLevel].gamechats.isplayertalk[currentPlayerChat] == 1){
                            txtChatPlayer.text ="";
                        }else{
                            txtChatNpc.text ="";
                        }
                    }else{
                        if(levels.Count > currentLevel + 1){
                            currentLevel++;
                            txtStage.text = levels[currentLevel].name;
                            txtChapter.text = "Chapter " + currentLevel.ToString() + " "+ levels[currentLevel].name;
                            changeStage(StageEnum.History); 
                        }else{
                            changeStage(StageEnum.GameOver);
                        }
                    }
                }
                break;
            }
            default: break;
        }
    }

    #endregion

    #region Classes
    [Serializable]
    public class GameLevel
    {
        public int level;
        public string name;
        public GameHistory gamehistory;
        public GameMission[] gamemissions;
        public GameChat gamechats;
    }
    
    [Serializable]
    public class GameHistory
    {
        public string[] wordslist;
    }
    
    [Serializable]
    public class GameMission
    {
        public string objective;
        public string message;
        public int hits;
    }
    
    [Serializable]
    public class GameChat
    {
        public int npcid;
        //public GameMessage[] gamemessages;
        public int[] isplayertalk;
        public string[] message;
    }
    

    #endregion

    #region functions
    public void getLevelJson()
    {

        TextAsset textAssetLevels = Resources.Load<TextAsset>("level");
        List<GameLevel>  levelsResult = JsonConvert.DeserializeObject<List<GameLevel>>(textAssetLevels.text);
        levels.AddRange(levelsResult);
    }

    private void changeStage(StageEnum stage){
        currentStage = stage;
        pnStart.SetActive(false);
        pnButtons.SetActive(false);
        pnMission.SetActive(false);
        pnChat.SetActive(false);
        pnMain.SetActive(false);
        pnInfo.SetActive(false);
        pnMenu.SetActive(false);
        pnFlash.SetActive(false);
        ballonChatPlayer.SetActive(false);
        ballonChatNpc.SetActive(false);
        imgDone.SetActive(false);
        objDone.SetActive(false);
        AnimatedTapFinger.SetActive(false);
        foreach (GameObject item in characters)
        {
            item.SetActive(false);
        }
        switch(currentStage)
        {
            case StageEnum.Start:
            {
                pnStart.SetActive(true);
                break;
            }
            case StageEnum.History:
            {
                txtStage.text = levels[currentLevel].name;
                pnFlash.SetActive(true);
                pnMain.SetActive(true);
                pnChat.SetActive(true);
                pnButtons.SetActive(true);
                currentPlayerHistory = 0;
                txtChatPlayer.text = "";
                break;
            }
            case StageEnum.Mission:
            {
                pnMission.SetActive(true);
                pnButtons.SetActive(true);
                btnPlay.GetComponent<Button>().interactable = false;
                currentPlayerMission = 0;
                startMission();
                break;
            }
            case StageEnum.Chat:
            {txtStage.text = levels[currentLevel].name;
                pnMain.SetActive(true);
                pnChat.SetActive(true);
                pnButtons.SetActive(true);
                gameCharacterChat = (CharacterNameEnum)levels[currentLevel].gamechats.npcid;
                characters[(int)gameCharacterChat -1].SetActive(true);
                txtChatPlayer.text = "";
                txtChatNpc.text = "";
                currentPlayerChat = 0;
                break;
            }
            case StageEnum.GameOver:
            {
                pnStart.SetActive(true);
                pnGameOver.SetActive(true);
                break;
            }
            default: break;
        }
    }
    
    public void setBallonText(string textFrom, Text txtTo){
        if(timeType > deltaTimeType){
            deltaTimeType += 0.1f;
        }else{
            deltaTimeType = 0;
            txtTo.text = textFrom.Substring(0, txtTo.text.Length + 1);
        }
    }

    
    public void startMission(){
        //Debug.Log("Call start Mission");
        imgDone.SetActive(false);
        objDone.SetActive(false);
        txtStage.text = levels[currentLevel].gamemissions[currentPlayerMission].objective;
        objectiveTap = levels[currentLevel].gamemissions[currentPlayerMission].hits;
        sbTapMission.size = 0;
        countTap = 0;
        btnPlay.GetComponent<Button>().interactable = false;
        AnimatedTapFinger.SetActive(true);
    }

    #endregion

}
