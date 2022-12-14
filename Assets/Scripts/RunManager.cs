using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RunManager : MonoBehaviour
{

    public Light primaryLight;
    public AudioSource musicSource;
    public AudioSource actionSource;
    public AudioSource combatSource;
    public AudioClip[] musicClips;
    public GameObject opponentPrefab;
    public GameObject playerPrefab;
    public GameObject monolithEye;
    public GameObject monolithEyeLight;
    public Vector3 playerPositon = new Vector3(-130, 60, 0);
    public Vector3 opponentPositon = new Vector3(130, 60, 0);

    public TMP_Text trackUI;
    public TMP_Text levelUI;
    public TMP_Text outcomeUI;
    public GameObject newRunPanel;

    private int level;
    private Dictionary<int, Dictionary<Slime,float>> levelDamage;
    private Slime player;
    private GameObject playerGO;
    private GameObject opponentGO;

    // Start is called before the first frame update
    void Start()
    {
      monolithEye = GameObject.Find("Monolith/Eye");
      monolithEyeLight = GameObject.Find("Monolith/Light");
      monolithEye.SetActive(false);
      monolithEyeLight.SetActive(false);
      musicSource.loop = true;
      musicClips = Resources.LoadAll<AudioClip>("Music");
      playerGO = Instantiate(playerPrefab, playerPositon, Quaternion.Euler(new Vector3(90,90,0)));
      playerGO.SetActive(true);
      // player = GameObject.Find("PlayerSlime").GetComponent<Slime>();
      player = playerGO.GetComponent<Slime>();

      outcomeUI.transform.parent.GetComponent<RectTransform>().localScale = new Vector3(0, 0);
      newRunPanel.SetActive(true);
      Button start = newRunPanel.GetComponentInChildren<Button>();
      GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject(start.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int Level(){
      return level;
    }

    public GameObject Player(){
      return playerGO;
    }

    public void ReportDamage(Slime dealer, float damage){
      if(levelDamage[level][dealer] == null){
        levelDamage[level][dealer] = 0f;
      }
      levelDamage[level][dealer] += damage;
    }

    IEnumerator Reset(){
      playerGO.transform.position = playerPositon;
      opponentGO.transform.position = opponentPositon;
      player.ResetHP();
      Slime opp = playerGO.GetComponent<Slime>();
      opp.ResetHP();
      yield return new WaitForSeconds(3f);
      outcomeUI.transform.parent.GetComponent<RectTransform>().localScale = new Vector3(0, 0);
    }

    public void NewRun(){
      level = 0;
      newRunPanel.SetActive(false);
      playerGO.SetActive(true);
      player.ResetLevel();
      if(opponentGO != null){
        opponentGO.SetActive(false);
      }
      StartCoroutine("SpawnLevel");
    }

    IEnumerator MonolithEye(){
      yield return new WaitForSeconds(3.25f);
      monolithEye.SetActive(true);
      monolithEyeLight.SetActive(true);
      yield return new WaitForSeconds(1.5f);
      monolithEye.transform.localScale = new Vector3(0.5f,0.1f,0.1f);
      yield return new WaitForSeconds(3f);
      monolithEye.transform.localScale = new Vector3(0.5f,0.2f,0.1f);
      yield return new WaitForSeconds(2f);
      monolithEye.SetActive(false);
      monolithEyeLight.SetActive(false);
    }

    IEnumerator SpawnLevel(){
      StartCoroutine("SwitchSongs");
      level++;

      if(level%3==0){
        StartCoroutine("MonolithEye");
      }

      primaryLight.transform.position = new Vector3(0f, 100f + ((float)level*20f), 0f);

      levelUI.text = level.ToString();
      player.LevelUp();
      yield return new WaitForSeconds(3f);
      outcomeUI.transform.parent.GetComponent<RectTransform>().localScale = new Vector3(0, 0);
      opponentGO = Instantiate(opponentPrefab, opponentPositon, Quaternion.Euler(new Vector3(90,90,0)));
      Reset();
    }

    IEnumerator SwitchSongs(){
      Vector3 panel = trackUI.transform.parent.transform.position;
      musicSource.clip = musicClips[level];
      musicSource.Play();
      trackUI.text = musicClips[level].ToString();
      trackUI.transform.parent.transform.position = new(panel.x,panel.y+100f,panel.z);
      yield return new WaitForSeconds(3f);
      trackUI.transform.parent.transform.position = new(panel.x,panel.y,panel.z);
    }

    public void Action(string action){
      AudioClip[] actions = Resources.LoadAll<AudioClip>("SFX/"+action);
      actionSource.clip = actions[0];
      actionSource.Play();
    }

    public void Hit(){
      AudioClip[] hits = Resources.LoadAll<AudioClip>("SFX/Hit");
      combatSource.clip = hits[0];
      combatSource.Play();
    }


    public void Resolve(Slime winner, Slime loser){
      if(winner.gameObject.layer == 10){
        outcomeUI.text = "YOU WIN";
      }else{
        outcomeUI.text = "YOU LOSE";
      }

      loser.NextLife();
      if(loser.LastLife()){
        loser.gameObject.SetActive(false);
        if(winner.gameObject.layer == 10){
          StartCoroutine("SpawnLevel");
        }else{
          StartCoroutine("NewRunOption");
        }
      }else{
        StartCoroutine("Reset");
      }

      outcomeUI.transform.parent.GetComponent<RectTransform>().localScale = new Vector3(0.2f, 0.1f);
      // Object.Destroy(loser.gameObject);
    }

    IEnumerator NewRunOption(){
      yield return new WaitForSeconds(3f);
      newRunPanel.SetActive(true);
    }
}
