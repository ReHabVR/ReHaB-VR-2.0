from utils import CameraConfig as CmrCfg
import json

class GameControl:
    movement = True
    left = True
    right = False
    applyMode = True
    leftProbability = 0
    rightProbability = 0
    camera_config = CmrCfg.CameraConfig()

    mode = 4

    dataAcquisition = True

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__,
                          sort_keys=True, indent=4)