from time import sleep
import SenderLib
import random
from utils import GameControl

host, port = "127.0.0.1", 25002
# host, port = "192.168.1.33", 25003

sender = SenderLib.Sender(host, port)
turn = False

gameControl = GameControl.GameControl()
gameControl.applyMode = True
gameControl.mode = 5
gameControl.dataAcquisition = True

while True:
    print("Movement" if turn else "Rest")
    gameControl.left = turn
    gameControl.leftProbability = random.random()
    x = sender.send_data(gameControl)
    print(gameControl.leftProbability)
    turn = not turn
    sleep(1)
