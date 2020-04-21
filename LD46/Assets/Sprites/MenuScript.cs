using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR 
using UnityEditor;
#endif 
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{

    public Animator anim;

    public GameObject buttons;

    public GameObject pointer;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StaticNoise());
    }

    IEnumerator StaticNoise()
    {
        //Cursor.visible = false;
        yield return new WaitForSeconds(2f);
        anim.SetTrigger("Static");
        yield return new WaitForSeconds(1f);
        anim.SetTrigger("Idle");
        buttons.SetActive(true);
    }
    
    // Update is called once per frame
    void Update()
    {
        pointer.transform.position = Input.mousePosition;
    }

    public void StartLevel()
    {
        SceneManager.LoadScene("Level_1");
    }
    
    public void Exit()
    {
        Application.Quit();
    }
}
