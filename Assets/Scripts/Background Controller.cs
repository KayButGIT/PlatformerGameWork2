using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float startPosX, startPosY;
    private float lengthX, lengthY;

    public GameObject cam;

    [Range(0f, 1f)]
    public float parallaxEffectX = 0.5f;

    [Range(0f, 1f)]
    public float parallaxEffectY = 0.5f;

    void Start()
    {
        startPosX = transform.position.x;
        startPosY = transform.position.y;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        lengthX = sr.bounds.size.x;
        lengthY = sr.bounds.size.y;
    }

    void LateUpdate() // use LateUpdate for camera-based movement
    {
        float distanceX = cam.transform.position.x * parallaxEffectX;
        float distanceY = cam.transform.position.y * parallaxEffectY;

        float movementX = cam.transform.position.x * (1 - parallaxEffectX);
        float movementY = cam.transform.position.y * (1 - parallaxEffectY);

        transform.position = new Vector3(
            startPosX + distanceX,
            startPosY + distanceY,
            transform.position.z
        );

        // Infinite scroll X
        if (movementX > startPosX + lengthX)
            startPosX += lengthX;
        else if (movementX < startPosX - lengthX)
            startPosX -= lengthX;

        // Infinite scroll Y
        if (movementY > startPosY + lengthY)
            startPosY += lengthY;
        else if (movementY < startPosY - lengthY)
            startPosY -= lengthY;
    }
}
