using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateStars : MonoBehaviour
{
    
    public GameObject BaseStar;
    public GameObject BaseStarText;
    
    float areaRadius = 9.9f;
    Star[] loadedStars;

    [SerializeField]
    public GameObject StarParent;

    public GameObject Camera;
    public Material StarMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // TextAsset file = Resources.Load("PlanetData") as TextAsset;
        // string jsonString = System.IO.File.ReadAllText("./Assets/ARExoplanetLab/Scripts/Stars/PlanetData.json");

        // loadedStars = JsonHelper.FromJson<Star>(jsonString);

        TextAsset file = Resources.Load("StarData") as TextAsset;
        loadedStars = JsonHelper.FromJson<Star>(file.text);


        for (int x = 0; x < loadedStars.Length; x++) {
            double ra = SexigesimalToRadians(HourAngleToSexigesimal(loadedStars[x].rightAscenscion));
            double declination = SexigesimalToRadians(loadedStars[x].declination);
            Vector3 position = EquatorialToCartesian(ra, declination);
            //Debug.log("Star " + x + ": " + position.toString());
            createStarObject(loadedStars[x].name, position, loadedStars[x].radius, loadedStars[x].distance, loadedStars[x].luminosity, loadedStars[x].type);
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    void EquatorialToHorizontal()
    {

    }

    Vector3 HorizontalToCartesian(float altitudeDeg, float azimuthDeg)
    {
        // Inputs:
        // Alt and Az in degrees
        float altRad = altitudeDeg * Mathf.Deg2Rad;
        float azRad = azimuthDeg * Mathf.Deg2Rad;

        // Convert spherical to Cartesian (Unity's Y-up, Z-forward)
        float x = Mathf.Cos(altRad) * Mathf.Sin(azRad);
        float y = Mathf.Sin(altRad);
        float z = Mathf.Cos(altRad) * Mathf.Cos(azRad);

        // Create direction vector
        Vector3 direction = new Vector3(x, y, z).normalized;
        Vector3 position = direction * areaRadius;
        return position;
    }

    [Serializable]
    public class Star {
        public string name;
        public string rightAscenscion;

        public string declination;
        public float radius;
        public float distance;
        public float luminosity;
        public string type;
    }

    private void createStarObject(string starName, Vector3 position, float radius, float distance, float luminosity, string type)
    {
        // GameObject s = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject s = GameObject.Instantiate(BaseStar);
        s.transform.parent = StarParent.transform;

        // StarComp comp = s.AddComponent<StarComp>();

        // comp.intialize(starName, position, radius, distance, luminosity, type);

        // TODO to flip east and west ??
        position.x = position.x * -1;

        s.transform.localPosition = position * areaRadius;

        // TODO change later, forcing all stars above dome line
        if (s.transform.localPosition.y + 2.7432f > 3.5f)
        {
            s.transform.localPosition = new Vector3(s.transform.localPosition.x, s.transform.localPosition.y + 2.7432f, s.transform.localPosition.z);
        }
        else
        {
            s.transform.localPosition = new Vector3(s.transform.localPosition.x, UnityEngine.Random.Range(4f, 7f), s.transform.localPosition.z);
        }

        Debug.Log(s.transform.localPosition);

        // s.transform.localScale = new Vector3(comp.radius*0.5f, comp.radius*0.5f, comp.radius*0.5f);
        s.transform.localScale = new Vector3(radius * 0.5f, radius * 0.5f, radius * 0.5f);

        // s.transform.LookAt(Camera.transform.position);
        // s.transform.Rotate(0, 180, 0);
        // Material material = s.GetComponent<MeshRenderer>().material;
        // material.shader = Shader.Find("Unlit/StarShader");
        s.GetComponent<MeshRenderer>().material = StarMaterial;
        Material material = s.GetComponent<MeshRenderer>().material;

        material.color = Color.yellow;

        int starLayer = LayerMask.NameToLayer("Stars");
        s.layer = starLayer;
        
        GameObject label = Instantiate(BaseStarText, s.transform);
        label.transform.localPosition = new Vector3(0, radius * 0.6f, 0);
        label.transform.localRotation = Quaternion.identity;
        label.transform.localScale = Vector3.one * 0.4f;
        label.layer = starLayer;

        TMPro.TextMeshPro text = label.GetComponent<TMPro.TextMeshPro>();
        if (text != null)
        {
            text.text = starName;
        }
    }

    // https://www.jameswatkins.me/posts/converting-equatorial-to-cartesian.html
    public string HourAngleToSexigesimal(string hourAngle) {
        string[] parts = hourAngle.Split("|");
        float[] nums = new float[parts.Length];
        string sexigesimal = "+|";

        for (int x = 0; x < parts.Length; x++) {
            nums[x] = float.Parse(parts[x], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            nums[x] = nums[x]*15;
            if(x == parts.Length - 1) {
                sexigesimal += nums[x];
            } else {
                sexigesimal += nums[x]+"|";
            }
        }
        return sexigesimal;
    }

    public double SexigesimalToRadians(string sexigesiamal) {
        string[] parts = sexigesiamal.Split("|");
        double degree = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        double minute = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        double second = double.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);

        double rad = degree + (minute/60) + (second/3600);

        if(String.Equals(parts[0], "-")) {
            rad *= -1;
        }

        return rad;
    }

    // https://github.com/Firnox/StarrySky
    public Vector3 EquatorialToCartesian(double ra, double declination) {
        // Place stars on a cylinder using 2D trigonometry.
        double x = System.Math.Cos(ra);
        double y = System.Math.Sin(declination);
        double z = System.Math.Sin(ra);

        // Pull in ends to make the sphere
        // Work out y-adjacent and use this to scale (as on unit sphere)
        double y_cos = System.Math.Cos(declination);
        x *= y_cos;
        z *= y_cos;

        // y += 2.7432;

        // Return as float
        return new((float)x, (float)y, (float)z);
    }
}
