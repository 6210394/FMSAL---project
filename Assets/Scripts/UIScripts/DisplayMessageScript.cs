using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class DisplayMessageScript : MonoBehaviour
{
    public static DisplayMessageScript instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public TextMeshProUGUI displayMessageText;
    public Image displayBackground;


    public Color baseTextColor;
    public Color baseBackgroundColor;
    private List<Message> messageList = new List<Message>();
    public bool isDisplayingMessage = false;

    struct Message
    {
        public string message;
        public float speed;
        public float length;
    }

    void Start()
    {
        baseTextColor = displayMessageText.color;
        baseBackgroundColor = displayBackground.color;

        ImmidiatelyHideMessage();
        messageList.Clear();
    }


    public void ChangeDisplayMessage(string message, float speed, float length)
    {
        if(messageList.Count >= 1)
        {
            foreach (Message msg in messageList)
            {
                Debug.Log(msg.message);
            }
            return;
        }

        messageList.Add(new Message { message = message, speed = speed, length = length });

        if (!isDisplayingMessage)
        {
            StartCoroutine(DisplayNextMessage());
        }
    }

    public IEnumerator DisplayNextMessage()
    {
        if (messageList.Count > 0)
        {
            Message message = messageList[0];
            messageList.RemoveAt(0);
            displayMessageText.text = message.message;
            yield return StartCoroutine(FadeInText(message.speed, message.length));
        }
    }

    public void DisplayMessage(float speed, float length)
    {
        StartCoroutine(FadeInText(speed, length));
    }

    public void HideMessage(float speed)
    {
        StartCoroutine(FadeOutText(speed));
    }

    #region Immidiate Display and Hide
    public void ImmidiatelyDisplayMessage() //unfinised
    {
        StopAllCoroutines();
        messageList.Clear();
        displayMessageText.color = new Color(displayMessageText.color.r, displayMessageText.color.g, displayMessageText.color.b, 1);
        displayBackground.color = new Color(displayBackground.color.r, displayBackground.color.g, displayBackground.color.b, 1);
    }

    public void ImmidiatelyHideMessage()
    {
        messageList.Clear();
        StopAllCoroutines();
        isDisplayingMessage = false;

        displayMessageText.color = new Color(displayMessageText.color.r, displayMessageText.color.g, displayMessageText.color.b, 0);
        displayBackground.color = new Color(displayBackground.color.r, displayBackground.color.g, displayBackground.color.b, 0);
    }
    #endregion


    private IEnumerator FadeInText(float speed, float length)
    {
        if(!isDisplayingMessage)
        {
            isDisplayingMessage = true;

            Color textTransparency = displayMessageText.color;
            Color bgTransparency = displayBackground.color;

            textTransparency.a = 1;
            bgTransparency.a = 1;

            displayMessageText.color = textTransparency;
            displayBackground.color = bgTransparency;

            float elapsedTime = 0;
            while (elapsedTime < speed)
            {
                elapsedTime += Time.deltaTime;
                textTransparency.a = Mathf.Clamp01(elapsedTime / speed);
                bgTransparency.a = Mathf.Clamp01(elapsedTime / speed);
                displayMessageText.color = textTransparency;
                displayBackground.color = bgTransparency;
                yield return null;
            }

            yield return new WaitForSeconds(length);
            StartCoroutine(FadeOutText(speed));
        }
    }


    private IEnumerator FadeOutText(float speed)
    {
        Color textTransparency = displayMessageText.color;
        Color bgTransparency = displayBackground.color;

        textTransparency.a = 1;
        bgTransparency.a = 1;

        displayMessageText.color = textTransparency;
        displayBackground.color = bgTransparency;

        float elapsedTime = 0;
        while (elapsedTime < speed)
        {
            elapsedTime += Time.deltaTime;
            textTransparency.a = Mathf.Clamp01(1 - (elapsedTime / speed));
            bgTransparency.a = Mathf.Clamp01(1 - (elapsedTime / speed));
            displayMessageText.color = textTransparency;
            displayBackground.color = bgTransparency;
            yield return null;
        }
        
        isDisplayingMessage = false;
        StartCoroutine(DisplayNextMessage());
    }

}