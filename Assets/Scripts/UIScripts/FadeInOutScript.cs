using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOutScript : MonoBehaviour
{
    public static FadeInOutScript instance;
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

    public Image sprite;
    public Color initialColor;

    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<Image>();
        initialColor = sprite.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator IFadeOutInCycle(float speed, float length)
    {
        Debug.Log("Fading Out");
        sprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
        
        while (sprite.color.a < 1)
        {
            sprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, sprite.color.a + Time.deltaTime * speed);
            yield return null;
        }
        yield return new WaitForSeconds(length);
        StartCoroutine(IFadeIn(speed));
    }

    public IEnumerator IFadeOut(float speed)
    {
        Debug.Log("Fading Out");
        sprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0);
        
        while (sprite.color.a < 1)
        {
            sprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, sprite.color.a + Time.deltaTime * speed);
            yield return null;
        }
    }

    public IEnumerator IFadeIn(float speed)
    {
        Debug.Log("Fading In");
        sprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1);
        
        while (sprite.color.a > 0)
        {
            sprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, sprite.color.a - Time.deltaTime * speed);
            yield return null;
        }

        yield return null;
    }
}
