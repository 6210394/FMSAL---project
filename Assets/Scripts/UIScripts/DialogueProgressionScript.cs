using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class DialogueProgressionScript : MonoBehaviour
{
    public TextMeshProUGUI dialogueText; // Reference to the TextMeshProUGUI component
    public string[] dialogueLines; // Array of dialogue lines
    private int currentLineIndex = 0; // Tracks the current dialogue line
    public float typingSpeed = 0.05f; // Speed at which characters appear
    public float wobbleSpeed = 2f; // Speed of the wobble effect
    public float wobbleAmount = 5f; // Amount of wobble

    private bool isTyping = false; // Prevents skipping while typing
    public string sceneToLoad;

    public bool unique;
    public Animator animator;

    // Start is called before the first execution of Update
    void Start()
    {
        if (dialogueLines.Length > 0)
        {
            StartCoroutine(TypeDialogue(dialogueLines[currentLineIndex])); // Start typing the first line
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTyping) // Check if the spacebar is pressed and not typing
        {
            AdvanceDialogue();
        }
    }

    IEnumerator StartMission()
    {
        yield return new WaitForSeconds(3);
        GameManager.instance.LoadMission(sceneToLoad);
    }

    void AdvanceDialogue()
    {
        currentLineIndex++; // Move to the next line

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
            StartCoroutine(FadeInOutScript.instance.IFadeOut(1f));
            StartCoroutine(StartMission());
        }
    }

    IEnumerator TypeDialogue(string line)
    {
        isTyping = true; // Set typing flag
        dialogueText.text += "\n"; // Add a new line before typing the next line
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter; // Add one character at a time
            yield return new WaitForSeconds(typingSpeed); // Wait before adding the next character
        }
        isTyping = false; // Reset typing flag
    }

        void LateUpdate()
    {
        ApplyWobbleEffect();
    }

    void ApplyWobbleEffect()
    {
        dialogueText.ForceMeshUpdate(); // Update the text mesh
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
}