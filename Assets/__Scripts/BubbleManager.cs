using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BubbleManager : MonoBehaviour
{
    public TypeText bubbleCountType;
    public TMP_Text bubbleCountText;
    public TMP_Text bubbleCountBackText;

    public int bubbleCount;

    private bool preparingBubble;
    private bool bubbleReady;

    // Update is called once per frame
    void Update()
    {
        string bubble = (bubbleCount == 1) ? "bubble" : "bubbles";
        if (bubbleCountType.allDone)
        {
            bubbleCountText.text = $"You have {bubbleCount} {bubble}.";
            bubbleCountBackText.text = $"<mark=#000000>{bubbleCountText.text}</mark>";
        }
        else
        {
            bubbleCountType.typeString = $"You have {bubbleCount} {bubble}.";
        }

        if (!preparingBubble && Input.GetKeyDown(KeyCode.B))
        {
            preparingBubble = true;
            StartCoroutine(BubbleRoutine(Random.Range(3f, 4f)));
        }

        if (bubbleReady && Input.GetKeyUp(KeyCode.B))
        {
            bubbleCount++;
            bubbleReady = false;
        }
    }

    public void SetBubbleCount(int count)
    {
        bubbleCount = count;
    }

    private IEnumerator BubbleRoutine(float time)
    {
        float timer = 0f;
        while (Input.GetKey(KeyCode.B))
        {
            if (timer >= time)
            {
                bubbleReady = true;
                break;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        preparingBubble = false;
    }
}
