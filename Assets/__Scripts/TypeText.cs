using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class TypeText : MonoBehaviour
{
    public TMP_Text referenceText;

    [Tooltip("Type speed in characters per second.")]
    public float typeSpeed;
    [Tooltip("Additional time to wait after punctuation in seconds.")]
    public float punctuationWait;
    
    public string punctuation = "!,.?-";

    public bool playOnAwake;
    public float preWait;
    public float postWait;
    public bool clickToSkip;
    public bool waitForClick;

    public string clickPrompt;

    public bool hideWhenDone;

    public bool TMPTagSupport;

    public List<GameObject> hideWith;
    public List<GameObject> showWhenDone;

    [Tooltip("Called after prewait")]
    public UnityEvent startEvents;
    [Tooltip("Called after everything, including click if enabled")]
    public UnityEvent endEvents;

    // == PRIVATE == 
    private float typeWait;

    private TMP_Text[] textComponents;
    private string[] originalTexts;
    [HideInInspector]
    public string typeString;

    private int typeIndex;
    private int[] insertionIndexes;

    private bool skip;
    private bool typeDone;
    public bool allDone;

    private void Awake()
    {
        textComponents = GetComponentsInChildren<TMP_Text>(true);
        insertionIndexes = new int[textComponents.Length];
        originalTexts = new string[textComponents.Length];
        typeString = referenceText.text;
        for (int i = 0; i < textComponents.Length; i++)
        {
            originalTexts[i] = textComponents[i].text;
        }
     
        typeWait = 1f/typeSpeed;

        ResetType();

        if (playOnAwake) Play();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (clickToSkip) Skip();
            if (typeDone && waitForClick) End();
        }
    }

    private void ResetType()
    {
        skip = false;
        typeDone = false;
        allDone = false;

        typeIndex = 0;
        for (int i = 0; i < textComponents.Length; i++)
        {
            textComponents[i].text = originalTexts[i];

            int index = textComponents[i].text.IndexOf(typeString);
            insertionIndexes[i] = index;

            textComponents[i].text = textComponents[i].text.Replace(typeString, "");
        }
    }

    public void Play()
    {
        StartCoroutine(TypeRoutine());
    }

    public void Reverse()
    {
        StartCoroutine(UntypeRoutine());
    }

    public void Skip()
    {
        if (typeIndex == 0) return;
        skip = true; 
    }

    private void End()
    {
        allDone = true;
        endEvents.Invoke();
        foreach (var go in showWhenDone) go.SetActive(true);
        if (hideWhenDone)
        {
            foreach (var go in hideWith) go.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    public void Type(string s)
    {
        typeIndex += s.Length;

        for (int i = 0; i < textComponents.Length; i++)
        {
            if (insertionIndexes[i] < 0) continue;
            textComponents[i].text = textComponents[i].text.Insert(insertionIndexes[i], s);
            insertionIndexes[i] += s.Length;
        }
    }

    public void Untype(int n)
    {
        typeIndex -= n;

        for (int i = 0; i < textComponents.Length; i++)
        {
            if (insertionIndexes[i] < n) continue;
            textComponents[i].text = textComponents[i].text.Remove(insertionIndexes[i] - n, n);
            insertionIndexes[i] -= n;
        }
    }

    private IEnumerator TypeRoutine()
    {
        if (preWait > 0f) yield return new WaitForSeconds(preWait);

        startEvents.Invoke();

        while (typeIndex < typeString.Length)
        {
            string nextType = typeString.Substring(typeIndex, skip ? typeString.Length - typeIndex : 1);

            if (nextType == "<")
            {
                int tagEndIndex = typeString.IndexOf(">", typeIndex);
                // +2 because we want to skip the tag entirely and type the next character
                if (tagEndIndex > 0)
                {
                    int l = tagEndIndex - typeIndex + 2;
                    if (typeIndex + l >= typeString.Length) l--;
                    nextType = typeString.Substring(typeIndex, l);
                }
            }

            Type(nextType);

            float wait = typeWait;
            if (punctuation.Contains(nextType)) wait += punctuationWait;  

            yield return new WaitForSeconds(wait);
        }

        if (postWait > 0f) yield return new WaitForSeconds(postWait);

        typeDone = true;
        if (!waitForClick) End();
        else
        {
            while (!allDone)
            {
                Type(clickPrompt);
                yield return new WaitForSeconds(0.5f);
                Untype(clickPrompt.Length);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator UntypeRoutine()
    {
        typeIndex--;
        while (typeIndex >= 0)
        {
            string lastType = referenceText.text.Substring(typeIndex, 1);

            if (lastType == ">")
            {
                int tagStartIndex = referenceText.text.LastIndexOf("<");
                Debug.Log(typeIndex);
                Debug.Log(tagStartIndex);
                // +2 because we want to skip the tag entirely and type the next character
                if (tagStartIndex > 0)
                {
                    int l = typeIndex - tagStartIndex + 2;
                    if (typeIndex - l < 0) l--;
                    
                    lastType = referenceText.text.Substring(typeIndex - l, l);
                }
            }

            Untype(lastType.Length);

            float wait = typeWait;

            yield return new WaitForSeconds(wait);
        }
    }
}
