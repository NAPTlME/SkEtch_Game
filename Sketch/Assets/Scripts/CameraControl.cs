using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    Vector3 startingPos;
    Quaternion startingRotation;

    Vector3 newPosition;
    Quaternion newRotation;

    [SerializeField]
    public bool allowMove;

    [SerializeField]
    Vector2 min, max;
    [SerializeField]
    Vector2 yRotationRange;
    [SerializeField]
    [Range(0.01f, 0.1f)]
    float lerpSpeed = 0.05f;
    float normSpeed = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        startingPos = transform.position;
        startingRotation = transform.rotation;
        newPosition = transform.position;
        newRotation = transform.rotation;

        GetNewTarget();

        allowMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        float currspeed;
        if (allowMove)
        {
            currspeed = lerpSpeed;
        }
        else
        {
            currspeed = normSpeed;
        }
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * currspeed);
        //transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * currspeed);
        if (Vector3.Distance(transform.position, newPosition) < 1f)
        {
            if (allowMove)
            {
                GetNewTarget();

            }
        } 
            
    }

    void GetNewTarget()
    {
        var xPos = Random.Range(min.x, max.x);
        var zPos = Random.Range(min.y, max.y);

        //newRotation = Quaternion.Euler(startingRotation.x, Random.Range(yRotationRange.x, yRotationRange.y),startingRotation.z);
        newPosition = new Vector3(xPos, startingPos.y, zPos);
    }

    public void ReturnToStart(float seconds)
    {
        newPosition = startingPos;
        newRotation = startingRotation;
    }

    public IEnumerator ReturnToStartPosition(float seconds)
    {
        newPosition = startingPos;
        newRotation = startingRotation;
        float speed = 1 / seconds;
        while (!transform.position.Equals(startingPos))
        {
            Vector3.Lerp(transform.position, startingPos, speed * Time.deltaTime);

            yield return new WaitForEndOfFrame();
        }
    }
}
