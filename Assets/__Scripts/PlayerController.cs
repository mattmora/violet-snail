using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum ControlMode
    {
        Raft,
        Free
    }

    public ControlMode mode = ControlMode.Raft;

    public float raftLookSpeed = 10f;
    public float raftLookRange = 30f;

    public float freeLookSpeed = 30f;

    public GameObject verticalLookObject;
    private Vector3 verticalLookInitialPosition;
    private Quaternion verticalLookInitialRotation;
    private float freeBaseHeight;

    private LuxWater_SetToGerstnerHeight waterHeight;

    private float hInput, vInput;

    // Start is called before the first frame update
    void Start()
    {
        waterHeight = GetComponent<LuxWater_SetToGerstnerHeight>();
        verticalLookInitialPosition = verticalLookObject.transform.localPosition;
        verticalLookInitialRotation = verticalLookObject.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");

        if (mode == ControlMode.Raft) RaftUpdate();
        else FreeUpdate();
    }

    public void RaftMode()
    {
        transform.DORotate(Vector3.zero, 15f).SetEase(Ease.InOutElastic).OnComplete(() =>
        {
            mode = ControlMode.Raft;
        });
        verticalLookObject.transform.DOLocalRotateQuaternion(verticalLookInitialRotation, 12f).SetEase(Ease.InOutElastic);
    }

    public void FreeMode()
    {
        freeBaseHeight = transform.position.y;
        mode = ControlMode.Free;
        transform.rotation = Quaternion.Euler(0f, 0f, 180f);
        verticalLookObject.transform.localRotation = verticalLookInitialRotation;
    }

    public void Sink(float d)
    {
        transform.position += Vector3.down * d;
    }

    public void SetHeightAlpha(float a)
    {
        waterHeight.alpha = a;
    }

    void RaftUpdate()
    {
        float currentXRotation = verticalLookObject.transform.localEulerAngles.x;
        if (currentXRotation > 180f) currentXRotation -= 360f;
        float xLimit;
        if (vInput > 0) xLimit = raftLookRange;
        else xLimit = -raftLookRange;
        float xScalar = Mathf.Clamp01((xLimit - currentXRotation) / xLimit);

        float currentYRotation = transform.eulerAngles.y;
        if (currentYRotation > 180f) currentYRotation -= 360f;
        float yLimit;
        if (hInput > 0) yLimit = -raftLookRange;
        else yLimit = raftLookRange;
        float yScalar = Mathf.Clamp01((yLimit - currentYRotation) / yLimit);

        transform.Rotate(new Vector3(0f, -hInput * yScalar * raftLookSpeed * Time.deltaTime, 0f));
        verticalLookObject.transform.Rotate(new Vector3(-vInput * xScalar * raftLookSpeed * Time.deltaTime, 0f, 0f));

        float t = ((-currentXRotation / raftLookRange) + 1f) * 0.5f;
        verticalLookObject.transform.localPosition = verticalLookInitialPosition + new Vector3(0f, Mathf.Lerp(-0.1f, 0.1f, t), 0f);
    }

    void FreeUpdate()
    {
        transform.Rotate(new Vector3(0f, -hInput * freeLookSpeed * Time.deltaTime, 0f));
        verticalLookObject.transform.Rotate(new Vector3(-vInput * freeLookSpeed * Time.deltaTime, 0f, 0f));

        float move = Input.GetKey(KeyCode.Space) ? 4f : 0f;
        Vector3 newPosition = transform.position + (verticalLookObject.transform.forward * move * Time.deltaTime);

        //newPosition.x = Mathf.Clamp(newPosition.x, -100f, 100f);
        newPosition.y = Mathf.Clamp(newPosition.y, -80, -1.5f);
        //newPosition.z = Mathf.Clamp(newPosition.z, -100f, 100f);
        transform.position = newPosition;
    }
}
