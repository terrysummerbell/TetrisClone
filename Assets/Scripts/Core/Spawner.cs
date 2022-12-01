using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Shape[] m_allShapes;

    private Shape GetRandomShape()
    {
        int i = Random.Range(0, m_allShapes.Length);
        if (m_allShapes[i])
        {
            return m_allShapes[i];
        }
        else
        {
            Debug.Log("WARNING! Invalid shape in spawner!");
            return null;
        }
    }

    public Shape SpawnShape()
    {
        Shape shape = null;
        shape = Instantiate(GetRandomShape(), transform.position, Quaternion.identity) as Shape;
        if (shape)
        {
            return shape;
        }
        else
        {
            Debug.LogWarning("WARNING! Invalid shape in spawner!");
            return null;
        }
    }


    // Use this for initialization
    void Start()
    {
        Vector2 originalVector = new Vector2(4.3f, 1.3f);
        Vector2 newVector = Vectorf.Round(originalVector);

        Debug.Log(newVector.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
