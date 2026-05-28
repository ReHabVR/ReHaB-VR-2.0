using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 class GameControl:
    movement = True
    left = False
    right = False
    leftProbability = 0
    rightProbability = 0
    applyMode = True
    mode = 4

    dataAcquisition = True

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__,
                          sort_keys=True, indent=4)
*/

public class PythonAdapter : AnimationCommandSource
{
    private volatile string _pendingJson;
    private GameControl _latest;

    public void Bind(PythonSocket socket)
    {
        socket.OnJsonReceived += (json) => { _pendingJson = json; };
    }

    void Update()
    {
        if (_pendingJson == null)
        {
            return;
        }

        try
        {
            _latest = JsonUtility.FromJson<GameControl>(_pendingJson);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse JSON: {e}");
        }

        _pendingJson = null;
    }

    public override bool TryGetCommand(out AnimationCommand command)
    {
        command = default;

        if (_latest == null)
        {
            return false;
        }

        if (!_latest.movement || !_latest.applyMode)
        {
            return false;
        }

        command = new AnimationCommand
        {
            ClipKey = ModeToClip(_latest.mode),
            Intensity = 1f
        };

        _latest = null;
        return true;
    }

    private string ModeToClip(int mode)
    {
        return mode switch
        {
            1 => "Movement",
            2 => "Rest", 
            _ => "Idle", 
        };
    }
}
