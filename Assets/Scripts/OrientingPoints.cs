using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientingPoints : MonoBehaviour
{

    float areaRadius = 9.9f;
    public GameObject BaseStar;

    [SerializeField]
    public GameObject StarParent;

    public Material StarMaterial;

    public float radius;

    // Start is called before the first frame update
    void Start()
    {
        radius = 1f;
        for (int i = 0; i < 4; i++)
        {
            GameObject s = GameObject.Instantiate(BaseStar);
            s.transform.parent = StarParent.transform;
            
             s.GetComponent<MeshRenderer>().material = StarMaterial;
            Material material = s.GetComponent<MeshRenderer>().material;

            Vector3 direction;
            if (i == 0)
            {
                direction = new Vector3(1, 0, 0).normalized;
                material.color = Color.green;
            }
            else if (i == 1)
            {
                direction = new Vector3(0, 0, 1).normalized;
                material.color = Color.cyan;
            }
            else if (i == 2)
            {
                direction = new Vector3(-1, 0, 0).normalized;
                material.color = Color.blue;
            }
            else
            {
                direction = new Vector3(0, 0, -1).normalized;
                material.color = Color.red;
            }

            // Vector3 position = direction * areaRadius;
            
            direction.x = direction.x * -1;

            s.transform.localPosition = direction*areaRadius;
            
            s.transform.localPosition = new Vector3(s.transform.localPosition.x, s.transform.localPosition.y, s.transform.localPosition.z);

            // material.color = Color.red;


            s.transform.localScale = new Vector3(radius * 0.5f, radius * 0.5f, radius * 0.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
