using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTrackLayers : MonoBehaviour
{
    public List<GameObject> Tracks;

    public LayerMask WallLayerMask;
    public LayerMask FloorLayerMask;

    private int _wallLayer = 0;
    private int _floorLayer = 0;

    // Start is called before the first frame update
    void Start()
    {
        int wallLayerMask = WallLayerMask.value;
        int floorLayerMask = FloorLayerMask.value;

        while (wallLayerMask != 0 && wallLayerMask != 1)
        {
            _wallLayer++;
            wallLayerMask >>= 1;
        }

        while (floorLayerMask != 0 && floorLayerMask != 1)
        {
            _floorLayer++;
            floorLayerMask >>= 1;
        }

        foreach (GameObject track in Tracks)
        {
            for (int i = 0; i < track.transform.childCount; i++)
            {
                Transform trackObject = track.transform.GetChild(i);

                if (trackObject.name == "Wall")
                {
                    trackObject.gameObject.layer = _wallLayer;
                }
                else if (trackObject.name == "Floor")
                {
                    trackObject.gameObject.layer = _floorLayer;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
