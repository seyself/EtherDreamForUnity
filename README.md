# EtherDream for Unity
EtherDream laser interface for Unity  
  
[robot-head/node-etherdream](https://github.com/robot-head/node-etherdream)  
NodeJS版のソースをUnity用に移植したものです。  
  
接続時のIPアドレスは予め固定されているので、USBで接続してIPの設定を適宜書き換えてください。  
[Ether Dream - Configuration](https://ether-dream.com/config.html)

## Search Devices

```
EtherDream.Find( args => {
    args.infoList.ForEach( device => {
        Debug.Log(device);
    });
});
```

## Connect EtherDream

```
void Start ()
{
    EtherDream _etherDream = new EtherDream();
    _etherDream.Connect("192.168.1.234", 7765, connection => {
        if (connection != null)
        {
            _etherDream.Start(RenderFrame, 1000);
        }
    });
}

void RenderFrame(int phase, List<DACPoint> framedata)
{
    ushort R = 0x6000;
    ushort G = 0x0000;
    ushort B = 0x0000;

    float v = 10000; // short range : -32768 to 32767
    float s1 = 0.97f;
    float c = 0;
    List<Vector3> points = new List<Vector3>();
    points.Add( new Vector3(c-v, c+v, 0) );

    points.Add( new Vector3(c-v*s1, c+v, 0) );
    points.Add( new Vector3(c+v*s1, c+v, 0) );

    points.Add( new Vector3(c+v, c+v, 0) );
    
    points.Add( new Vector3(c+v, c+v*s1, 0) );
    points.Add( new Vector3(c+v, c-v*s1, 0) );
    
    points.Add( new Vector3(c+v, c-v, 0) );
    
    points.Add( new Vector3(c+v*s1, c-v, 0) );
    points.Add( new Vector3(c-v*s1, c-v, 0) );
    
    points.Add( new Vector3(c-v, c-v, 0) );
    
    points.Add( new Vector3(c-v, c-v*s1, 0) );
    points.Add( new Vector3(c-v, c+v*s1, 0) );
    
    points.Add( new Vector3(c-v, c+v, 0) );
    
    _etherDream.DrawPath(framedata, points, R, G, B);
}
```