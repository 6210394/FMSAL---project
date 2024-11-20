using UnityEngine;
using UnityEngine.UI;

public class FadeInOutScript : MonoBehaviour
{
    public Image sprite;

    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<Image>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeIn()
    {
        sprite.color = new Color(0, 0, 0, 1);
        sprite.CrossFadeAlpha(0, 1, false);
    }

    public void FadeOut()
    {
        sprite.color = new Color(0, 0, 0, 1);
        sprite.CrossFadeAlpha(1, 1, false);
    }
}
