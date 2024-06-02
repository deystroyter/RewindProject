using System.Collections.Generic;
using UnityEngine;
using RewindProject;

public class CubeBouncing : RewindNotifier_MonoBehaviour
{
    private Rigidbody _rigidbody;

    public Vector3 veloc = Vector3.zero;
    public Transform fr;

    [RewindableProperty] public string x
    {
        get => _x;
        set 
        {
            _x = value;
            NotifyRewindablePropertyChanged();
        }
    }
    public string _x = "zero";
    [RewindStateBased] public int testBounce = 0;
    [RewindableProperty]
    public float CBTestProp
    {
        get { return _testProp; }
        set
        {
            _testProp = value;
            NotifyRewindablePropertyChanged();
        }
    }
    private float _testProp;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.TryGetComponent<Rigidbody>(out _rigidbody);
        //Tester();
    }

    void Tester()
    {
        var comp = this.transform;
        var dict = new Dictionary<System.Reflection.PropertyInfo, RewindFrames<Vector3>>();
        var prop = typeof(Transform).GetProperty(nameof(this.transform.position));
        dict.Add(prop, new RewindFrames<Vector3>());
        var x = Vector3.zero;
        var s = System.Diagnostics.Stopwatch.StartNew();
        var arr = new Vector3[100000];

        for (var i = 0; i < 100000; i++) 
            //dict[prop].Add(i, prop.GetValue(comp));
            arr[0] = transform.position;
        Debug.Log("Write: " + s.ElapsedTicks);
        s = System.Diagnostics.Stopwatch.StartNew();
        for (var i = 0; i < 10000; i++)
            transform.position = arr[i];
            //prop.SetValue(comp, dict[prop].Get(i));
        Debug.Log("Write: " + s.ElapsedTicks);

    }
    private void FixedUpdate()
    {
        veloc = _rigidbody.velocity;
        if (Input.GetKey(KeyCode.W)) 
        {
            testBounce++;
            x = $"string{testBounce}";

        }
        if (!transform.hasChanged) 
        {
            //rigidbody.AddForce(Vector3.up * 100);
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        _rigidbody.AddForce(Vector3.up * 300);
        transform.hasChanged = false;
    }
}
