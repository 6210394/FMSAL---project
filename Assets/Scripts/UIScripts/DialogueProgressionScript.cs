using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class DialogueProgressionScript : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public string[] dialogueLines;
    int currentLineIndex = 0;
    public float typingSpeed = 0.05f; // Speed at which characters appear
    public float wobbleSpeed = 2f;
    public float wobbleAmount = 5f;

    private bool isTyping = false;
    public string sceneToLoad;

    public bool unique;
    public Animator animator;

    void Start()
    {
        if (dialogueLines.Length > 0)
        {
            StartCoroutine(TypeDialogue(dialogueLines[currentLineIndex]));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isTyping) // Move dialogue forward - Couldn't manage dialogue skipping yet
        {
            AdvanceDialogue();
        }
    }

    void AdvanceDialogue()
    {
        currentLineIndex++;

        if (currentLineIndex < dialogueLines.Length)
        {
            StartCoroutine(TypeDialogue(dialogueLines[currentLineIndex])); // Start typing the next line
        }
        else
        {
            if(unique)
            {
                animator.SetTrigger("FadeIn");
                return;
            }

            Debug.Log("Dialogue finished.");
            isTyping = true;
            StartCoroutine(FadeInOutScript.instance.IFadeOut(1f));
            StartCoroutine(GameManager.instance.ILoadMission(sceneToLoad, 3));
        }
    }

    IEnumerator TypeDialogue(string line)
    {
        isTyping = true;
        dialogueText.text += "\n"; //Skip line
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed); // Wait before adding the next character
        }
        isTyping = false;
    }

    void LateUpdate()
    {
        ApplyWobbleEffect();
    }

    #region Text Effects
    void ApplyWobbleEffect()
    {
        dialogueText.ForceMeshUpdate();
        TMP_TextInfo textInfo = dialogueText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[textInfo.characterInfo[i].materialReferenceIndex].vertices;

            Vector3 offset = Wobble(Time.time + i);
            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            dialogueText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }

    Vector3 Wobble(float time)
    {
        return new Vector3(Mathf.Sin(time * wobbleSpeed) * wobbleAmount, Mathf.Cos(time * wobbleSpeed) * wobbleAmount, 0);
    }
    #endregion
}