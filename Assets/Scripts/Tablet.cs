using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tablet : MonoBehaviour
{
    // Start is called before the first frame update
    public int animationState;
    public bool inView;
    public float minRot;
    public float maxRot;
    public float rate;
    public float progress = 0;

    Player player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimation();
        if(Input.GetKeyDown(KeyCode.I))
        {
            inView = !inView;
            if (inView)
            {
                transform.GetChild(0).gameObject.SetActive(true);
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            } else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            player.canMove = !inView;
            animationState = inView ? -1 : 1;
        }
    }

    void UpdateAnimation()
    {
        
        progress += Time.deltaTime * rate * animationState;
        transform.localEulerAngles = new Vector3(progress * (maxRot - minRot) + minRot, 0, 0);
        float actualAngle = transform.localEulerAngles.x;
        if(actualAngle > 180.0f)
        {
            actualAngle -= 360;
        }
        if(actualAngle > maxRot)
        {
            transform.GetChild(0).gameObject.SetActive(false);
            progress = 1;
            animationState = 0;
        } else if(actualAngle < minRot)
        {
            progress = 0;
            animationState = 0;
        }
    }
}
