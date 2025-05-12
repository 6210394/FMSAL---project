using UnityEngine;
using System.Collections;

public class EndLevelScript : MonoBehaviour
{
    public string nextSceneName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EndLevel()
    {
        StartCoroutine(IEndLevel());
    }

    IEnumerator IEndLevel()
    {
        StartCoroutine(FadeInOutScript.instance.IFadeOut(1f));
        yield return new WaitForSeconds(2f);
        StartCoroutine(GameManager.instance.ILoadMission(nextSceneName, 0f));
    }
}
