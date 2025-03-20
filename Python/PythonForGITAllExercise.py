from time import sleep
import SenderLib

#host, port = "127.0.0.1", 25002
# host, port = "192.168.132.6", 25002
host, port = "192.168.18.129", 25002
sender = SenderLib.Sender(host, port)
turn = False

gameControl = SenderLib.GameControl()
gameControl.applyMode = True

gameControl.dataAcquisition = True

for i in range(0,7):
    a = 5+i
    gameControl.mode = a
    for j in range(0,4):
        print("Movement" if turn else "Rest")
        gameControl.left = turn
        x = sender.send_data(gameControl)
        #print(str())
        if turn:
            sleep(6)  # Czas sleep dla turn = True
        else:
            sleep(2)  # Czas sleep dla turn = False
        turn = not turn
        
gameControl.mode = 0
gameControl.left = turn
sender.send_data(gameControl)
print("Movement" if turn else "Rest")
